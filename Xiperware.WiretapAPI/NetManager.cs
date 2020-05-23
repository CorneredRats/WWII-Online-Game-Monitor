/* =============================================================================
 * Xiperware Wiretap API                            Copyright (c) 2013 Xiperware
 * http://begm.sourceforge.net/                              xiperware@gmail.com
 * 
 * This file is part of the Xiperware Wiretap API library for WW2 Online.
 * 
 * This library is free software; you can redistribute it and/or modify it under
 * the terms of the GNU Lesser General Public License version 2.1 as published
 * by the Free Software Foundation.
 * 
 * This program is distributed in the hope that it will be useful, but WITHOUT
 * ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 * FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more
 * details.
 * 
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program. If not, see <http://www.gnu.org/licenses/>.
 * =============================================================================
 */

using System;
using System.Net;
using System.Net.Cache;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using SecureCredentialsLibrary;

namespace Xiperware.WiretapAPI
{
  /// <summary>
  /// The NetManager class handles all network operations, proxy authentication, timeouts,
  /// automatic retry on error, statistic tracking, etc
  /// </summary>
  public class NetManager
  {
    #region Variables
    
    private Uri wiretapServer;
    private readonly MyWebClient webClient;
    private readonly CredentialsDialog proxyCredDialog;
    private readonly IWebProxy IEProxy;

    private int bytesDownloaded = 0;  // this session (32bit int = max 2GB)
    public event EventHandler OnBytesDownloadedChanged;

    #endregion

    #region Constructors

    /// <summary>
    /// Create a new NetManager object.
    /// </summary>
    public NetManager()
    {
      Log.AddEntry( "Initialising network manager..." );

      wiretapServer = new Uri( "http://wiretap.wwiionline.com" );  // default server
      webClient = new MyWebClient();
      proxyCredDialog = new CredentialsDialog( "unused_target" );
      proxyCredDialog.Persist = true;
      proxyCredDialog.SaveChecked = true;
#if !SL
      IEProxy = WebRequest.DefaultWebProxy;  // store for later
#endif
      Log.Okay();
    }

    #endregion

    #region Properties

    /// <summary>
    /// Approximate number of bytes downloaded this session.
    /// </summary>
    public int BytesDownloaded
    {
      get { return this.bytesDownloaded; }
      set
      {
        this.bytesDownloaded = value;
        if( this.OnBytesDownloadedChanged != null )
          OnBytesDownloadedChanged( this, null );
      }
    }

    #endregion

    #region Delegates

    /// <summary>
    /// A generic delegate to encapsulate a web request.
    /// </summary>
    private delegate object WebRequestFunction( Object arg );

    #endregion

    #region Methods

    /// <summary>
    /// Apply the given network settings for use from now on.
    /// </summary>
    /// <remarks>Throws an exception if any settings are invalid.</remarks>
    /// <param name="settings">The NetworkSettings to use.</param>
    public void LoadSettings( NetworkSettings settings )
    {
      this.wiretapServer = new UriBuilder( "http", settings.wiretapHost, settings.WiretapPortInt ).Uri;

      if( settings.useCustomProxy )
      {
        if( settings.proxyHost == "" )
          WebRequest.DefaultWebProxy = null;  // force no proxy
        else
          WebRequest.DefaultWebProxy = new WebProxy( new UriBuilder( "http", settings.proxyHost, settings.ProxyPortInt ).Uri );
      }
      else
      {
        WebRequest.DefaultWebProxy = IEProxy;
      }
    }

    /// <summary>
    /// Creates a new NetworkSettings objects from the currently used settings.
    /// </summary>
    /// <returns></returns>
    private NetworkSettings CurrentSettings()
    {
      NetworkSettings netSettings = new NetworkSettings( this.wiretapServer.Host, this.wiretapServer.Port.ToString(),
                                                         WebRequest.DefaultWebProxy != this.IEProxy, "", "" );
      if( netSettings.useCustomProxy )
      {
        netSettings.proxyHost = ( (WebProxy)WebRequest.DefaultWebProxy ).Address.Host;
        netSettings.proxyPort = ( (WebProxy)WebRequest.DefaultWebProxy ).Address.Port.ToString();
      }

      return netSettings;
    }



    /// <summary>
    /// Download a meta xmlfile to a local cache.
    /// </summary>
    /// <param name="metafile">The MetaXmlFile to download.</param>
    public void DownloadMetaFile( MetaXmlFile metafile )
    {
      try
      {
        PerformWebRequest( WebRequest_DownloadMetaFile, metafile );
      }
      catch( Exception ex )
      {
        throw new NetworkException( String.Format( Language.Error_DownloadError, metafile.Name ), ex );
      }

      metafile.FlagAsChecked();
      this.BytesDownloaded += (int)metafile.LocalFileInfo.Length;
    }

    /// <summary>
    /// The WebRequestFunction used by DownloadMetaFile().
    /// </summary>
    /// <param name="arg">MetaXmlFile</param>
    /// <returns>null</returns>
    private object WebRequest_DownloadMetaFile( Object arg )
    {
      MetaXmlFile metafile = (MetaXmlFile)arg;
      Uri uri = new Uri( wiretapServer, metafile.Uri );
      webClient.DownloadFile( uri, metafile.LocalFile );
      return null;
    }



    /// <summary>
    /// Download and parse an xmlfile into memory and return it.
    /// </summary>
    /// <param name="xmlfile">The InitXmlFile or PollXmlFile to download.</param>
    /// <returns>The parsed XmlDocument structure.</returns>
    public XmlDocument GetXmlDocument( XmlFile xmlfile )
    {
      string xml;
      try
      {
        xml = (string)PerformWebRequest( WebRequest_DownloadString, new Uri( wiretapServer, xmlfile.Uri ) );
      }
      catch( Exception ex )
      {
        throw new NetworkException( String.Format( Language.Error_DownloadError, xmlfile.Name ), ex );
      }

      XmlDocument xmldoc = new XmlDocument();
      xmldoc.XmlResolver = null;  // disable automatic dtd validation
      xmldoc.LoadXml( xml );
      this.BytesDownloaded += xml.Length;
      return xmldoc;
    }

    /// <summary>
    /// Download a uri and return it as a string.
    /// </summary>
    /// <param name="uri">The Uri to download.</param>
    /// <returns>The response body.</returns>
    public string DownloadString( Uri uri )
    {
      string result;
      try
      {
        result = (string)PerformWebRequest( WebRequest_DownloadString, uri );
      }
      catch( Exception ex )
      {
        throw new NetworkException( String.Format( Language.Error_DownloadError, uri ), ex );
      }

      this.BytesDownloaded += result.Length;
      return result;
    }

    /// <summary>
    /// A WebRequestFunction used to download a Uri to a string.
    /// </summary>
    /// <param name="arg">Uri</param>
    /// <returns>string</returns>
    private object WebRequest_DownloadString( Object arg )
    {
      return webClient.DownloadString( ( (Uri)arg ).AbsoluteUri );
    }



    /// <summary>
    /// Gets the date the meta xmlfile was last modified on the server.
    /// </summary>
    /// <param name="xmlfile">The XmlFile to check.</param>
    /// <returns>A DateTime object.</returns>
    public DateTime GetLastModified( XmlFile xmlfile )
    {
      string lastmod = GetHeaders( xmlfile )[HttpResponseHeader.LastModified];

      DateTime remoteTime;
      if( DateTime.TryParse( lastmod, out remoteTime ) )
        return remoteTime;
      else
        throw new NetworkException( Language.Error_HeaderErrorLastMod,
                                    new Exception( String.Format( Language.Error_ParseDatetime, lastmod ) ) );
    }

    /// <summary>
    /// Performs a HEAD request to get the response headers for a meta xmlfile.
    /// </summary>
    /// <param name="xmlfile">The XmlFile to check.</param>
    /// <returns>A collection of web headers, can be indexed by the HttpResponseHeader enum.</returns>
    public WebHeaderCollection GetHeaders( XmlFile xmlfile )
    {
      try
      {
        return (WebHeaderCollection)PerformWebRequest( WebRequest_GetHeaders, xmlfile );
      }
      catch( Exception ex )
      {
        throw new NetworkException( String.Format( Language.Error_HeaderError, xmlfile.Name ), ex );
      }
    }

    /// <summary>
    /// The WebRequestFunction used by GetHeaders().
    /// </summary>
    /// <param name="arg">MetaXmlFile</param>
    /// <returns>WebHeaderCollection</returns>
    private object WebRequest_GetHeaders( Object arg )
    {
      XmlFile xmlfile = (XmlFile)arg;

      HttpWebRequest req = MyWebClient.CreateWebRequest( new Uri( wiretapServer, xmlfile.Uri ) );
      req.Method = "HEAD";

      WebResponse res = req.GetResponse();
      // this.KBytesDownloaded += (int)res.ContentLength;
      // NOTE: ContentLength is the value of the header, not the size of the response
      WebHeaderCollection headers = res.Headers;
      res.Close();

      return headers;
    }



    /// <summary>
    /// Uses the supplied network settings to perform a HEAD request on the wiretap server
    /// home page. Doesn't affect the currently applied network settings.
    /// </summary>
    /// <param name="settings">The NetworkSettings to test.</param>
    /// <returns>The error message if any setting or network error occurs, or String.Empty on success.</returns>
    public string TestConnection( NetworkSettings settings )
    {
      proxyCredDialog.AlwaysDisplay = true;
      if( !proxyCredDialog.SaveChecked )
        proxyCredDialog.Password = "";


      // store prev settings so we can revert after test

      NetworkSettings prevNetSettings = CurrentSettings();


      try
      {
        // apply new settings

        LoadSettings( settings );

        if( WebRequest.DefaultWebProxy != null )
          WebRequest.DefaultWebProxy.Credentials = null;  // force prompt


        // do test

        PerformWebRequest( WebRequest_TestConnection, null );
      }
      catch( Exception ex )
      {
        LoadSettings( prevNetSettings );  // revert

        if( ex is WebException && ((WebException)ex).Status == WebExceptionStatus.ProtocolError )
        {
          return String.Format( "HTTP {0}, {1}",
                                (int)( (HttpWebResponse)( (WebException)ex ).Response ).StatusCode,
                                ( (HttpWebResponse)( (WebException)ex ).Response ).StatusDescription );
        }
        else
          return ex.Message;
      }

      LoadSettings( prevNetSettings );  // revert

      proxyCredDialog.AlwaysDisplay = false;
      return String.Empty;  // no error message = success
    }

    /// <summary>
    /// The WebRequestFunction used by TestConnection().
    /// </summary>
    /// <param name="arg">null</param>
    /// <returns>null</returns>
    private object WebRequest_TestConnection( Object arg )
    {
      HttpWebRequest req = MyWebClient.CreateWebRequest( wiretapServer );
      req.Method = "HEAD";
      req.KeepAlive = false;

      WebResponse res = req.GetResponse();
      res.Close();

      if( req.RequestUri != res.ResponseUri )
        throw new Exception( "Redirected to " + res.ResponseUri );

      return null;
    }



    /// <summary>
    /// Performs the given WebRequestFunction, retrying if an error occurs, and handling
    /// proxy authentication.
    /// </summary>
    /// <param name="func">The WebRequestFunction to execute.</param>
    /// <param name="arg">The WebRequestFunction argument.</param>
    /// <returns>The WebRequestFunction return value.</returns>
    private object PerformWebRequest( WebRequestFunction func, Object arg )
    {
      int retries = 2;

      while( retries-- > 0 )
      {
        try
        {
          Object ret;
          WebException proxyAuthException;
          return ProxyAuthHandler( func, arg, out ret, out proxyAuthException );
        }
        catch
        {
          if( retries == 0 )
            throw;  // rethrow only if no more retries left
        }
      }

      return null;
    }

    /// <summary>
    /// If proxy auth is required then the proxy password is retrieved from saved credentials
    /// or requested from the user. This is repeated until the WebRequestFunction succeeds
    /// (upon which the credentials are saved), or the user cancels.
    /// </summary>
    private object ProxyAuthHandler( WebRequestFunction func, Object arg, out Object ret, out WebException proxyAuthException )
    {
      if( !IsProxyAuthRequired( func, arg, out ret, out proxyAuthException ) )
        return ret;


      // got HTTP 407

      // extract realm

      string realm = proxyAuthException.Response.Headers[HttpResponseHeader.ProxyAuthenticate];

      Match match = Regex.Match( realm, @"realm=\""(.+?)\""", RegexOptions.IgnoreCase );  // eg,  "Basic realm=\"The realm name\""  (inc quotes)
      if( match.Success )
        realm = match.Groups[1].Value;
      else if( WebRequest.DefaultWebProxy is WebProxy )  // custom
        realm = ( (WebProxy)WebRequest.DefaultWebProxy ).Address.Host;
      else
        realm = Language.NetManager_PleaseEnterCredentials;


      // ask for credentials until succeeds or cancelled

      while( true )
      {
        // remove save check if auth failed

        if( WebRequest.DefaultWebProxy.Credentials != null )
          proxyCredDialog.SaveChecked = false;


        // get proxy credentials (prompt if not stored)

        NetworkCredential cred = GetProxyCredentials( realm );
        if( cred == null ) throw proxyAuthException;  // user cancelled, return original 407 exception

        WebRequest.DefaultWebProxy.Credentials = cred;


        // try again

        if( !IsProxyAuthRequired( func, arg, out ret, out proxyAuthException ) )  // success
        {
          proxyCredDialog.AlwaysDisplay = false;

          try
          {
            if( proxyCredDialog.SaveChecked )
              proxyCredDialog.Confirm( true );   // store
            else
              proxyCredDialog.Confirm( false );  // unstore
          }
          catch { }

          return ret;
        }
        else  // failed again
        {
          proxyCredDialog.AlwaysDisplay = true;
          proxyCredDialog.Password = "";

          try  // throws an exception if didn't actually show dialog (ie, stored password was wrong)
          {
            proxyCredDialog.Confirm( false );
          }
          catch { }
        }
      }  // end while
    }

    /// <summary>
    /// Performs the actual WebRequestFunction and returns whether or not we require proxy auth.
    /// </summary>
    /// <returns>True if we received HTTP 407: Proxy Authentication Required.</returns>
    private bool IsProxyAuthRequired( WebRequestFunction func, Object arg, out Object ret, out WebException proxyAuthException )
    {
      try
      {
        ret = func( arg );
        proxyAuthException = null;
        return false;
      }
      catch( WebException ex )
      {
        if( ex.Status == WebExceptionStatus.ProtocolError && ( (HttpWebResponse)ex.Response ).StatusCode == HttpStatusCode.ProxyAuthenticationRequired )
        {
          // proxy auth required
          ret = null;
          proxyAuthException = ex;
          return true;
        }
        else
        {
          // other error
          throw;
        }
      }
    }

    /// <summary>
    /// Prompt the user for Proxy Credentials.
    /// </summary>
    /// <param name="message">The message to display to the user, usually containing the proxy realm.</param>
    /// <returns>A new NetworkCredential object with the values the user entered, or null if cancelled.</returns>
    private NetworkCredential GetProxyCredentials( string message )
    {
      if( WebRequest.DefaultWebProxy is WebProxy )  // custom
      {
        proxyCredDialog.Target = ( (WebProxy)WebRequest.DefaultWebProxy ).Address.AbsoluteUri;
        proxyCredDialog.Caption = String.Format( Language.NetManager_ConnectTo, ( (WebProxy)WebRequest.DefaultWebProxy ).Address.Host );
      }
      else
      {
        proxyCredDialog.Target = "ieproxy";
        proxyCredDialog.Caption = Language.NetManager_ConnectToProxy;
      }
      proxyCredDialog.Message = message;

      if( proxyCredDialog.Show() == DialogResult.OK )
        return proxyCredDialog.NetworkCredentials;
      else
        return null;
    }

    #endregion
  }

  /// <summary>
  /// A custom WebClient with shorter timeouts, no cache, and compression support.
  /// </summary>
  public class MyWebClient : WebClient
  {
    /// <summary>
    /// The current server time (as at the last web request).
    /// </summary>
    public static int ServerVersion { get; set; }

    /// <summary>
    /// Get the WebRequest object to use for this WebClient.
    /// </summary>
    /// <param name="address">A Uri for the address to request.</param>
    /// <returns>A new WebRequest object for the given address.</returns>
    protected override WebRequest GetWebRequest( Uri address )
    {
      return CreateWebRequest( address );
    }

    /// <summary>
    /// Create a new HttpWebRequest object.
    /// </summary>
    /// <remarks>Also used directly by GetHeaders() and TestConnection().</remarks>
    /// <param name="address">A Uri for the address to request.</param>
    /// <returns>A new HttpWebRequest object for the given address.</returns>
    public static HttpWebRequest CreateWebRequest( Uri address )
    {
      HttpWebRequest req = (HttpWebRequest)WebRequest.Create( address );

      req.KeepAlive = true;
      req.CachePolicy = new RequestCachePolicy( RequestCacheLevel.NoCacheNoStore );
      req.Timeout = 15000;           // default: 100000 = 100 secs
      req.ReadWriteTimeout = 20000;  // default: 300000 = 5 mins
      req.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
      
      // create a useragent w/ app name tag - just hardcode IE7 & XP/Vista to avoid any potential issues
      req.UserAgent = String.Format( "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1; {0} v{1})", Application.ProductName, Application.ProductVersion );

      // facilitylist.xml is 731kb, may need a little longer over a slow connection
      if( address.AbsolutePath.Contains( "facilitylist.xml" ) )
        req.Timeout *= 2;

      return req;
    }

    /// <summary>
    /// Returns the WebResponse for the specified WebRequest.
    /// </summary>
    /// <param name="request">A WebRequest that is used to obtain the response.</param>
    /// <returns>A WebResponse containing the response for the specified WebRequest.</returns>
    protected override WebResponse GetWebResponse( WebRequest request )
    {
      WebResponse response = base.GetWebResponse( request );
      try { ServerVersion = (int)DateTime.Parse( response.Headers["Date"] ).ToOADate(); } catch { }
      return response;
    }
  }

  /// <summary>
  /// A custom exception that indicates a network error has occurred during an NetManager 
  /// operation.
  /// </summary>
  public class NetworkException : ApplicationException
  {
    /// <summary>
    /// Create a new NetworkException.
    /// </summary>
    /// <param name="message">The general error message.</param>
    /// <param name="inner">The actual exception that caused this one to be thrown.</param>
    public NetworkException( string message, Exception inner )
      : base( message, inner )
    {

    }
  }

  /// <summary>
  /// Contains network-related settings.
  /// </summary>
  public class NetworkSettings
  {
    #region Variables

    public string wiretapHost = "wiretap.wwiionline.com";
    public string wiretapPort = "";
    public bool useCustomProxy = false;
    public string proxyHost = "";
    public string proxyPort = "";

    #endregion

    #region Constructors

    /// <summary>
    /// Create a new NetworkSettings object with default values.
    /// </summary>
    public NetworkSettings()
    {

    }

    /// <summary>
    /// Create a new NetworkSettings object with the given values.
    /// </summary>
    public NetworkSettings( string wiretapHost, string wiretapPort, bool useCustomProxy, string proxyHost, string proxyPort )
    {
      this.wiretapHost = wiretapHost;
      this.wiretapPort = wiretapPort;
      this.useCustomProxy = useCustomProxy;
      this.proxyHost = proxyHost;
      this.proxyPort = proxyPort;
    }

    #endregion

    #region Properties

    /// <summary>
    /// The Wiretap server port number, as an int (default 80).
    /// </summary>
    public int WiretapPortInt
    {
      get
      {
        if( this.wiretapPort == "" )
          return 80;
        else
          return int.Parse( this.wiretapPort );
      }
    }

    /// <summary>
    /// The custom proxy port number, as an int (default 8080).
    /// </summary>
    public int ProxyPortInt
    {
      get
      {
        if( this.proxyPort == "" )
          return 8080;
        else
          return int.Parse( this.proxyPort );
      }
    }

    #endregion
  }

}