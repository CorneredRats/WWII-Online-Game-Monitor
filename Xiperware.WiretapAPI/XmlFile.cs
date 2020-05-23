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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Xiperware.WiretapAPI
{
  /// <summary>
  /// The base class that represents an xml data file on the wiretap server. This class isn't
  /// used directly; all xmlfiles fall into one of the below three types.
  /// </summary>
  public class XmlFile
  {
    #region Variables

    private readonly string name;
    private readonly string uri;  // relative path, eg "/xml/displaycps.xml"
    private readonly string xpath;
    private int queryParam = 0;  // 0 = unused
    private string[] attributes;
    private object[] defaults;

    #endregion

    #region Events

    public event EventHandler PreParse;
    public event EventHandler<PostParseEventArgs> PostParse;

    #endregion

    #region Constructors

    /// <summary>
    /// Create a new XmlFile (used by child classes).
    /// </summary>
    /// <param name="uri">The xmlfile's relative url.</param>
    /// <param name="xpath">The xpath referencing the xml data.</param>
    public XmlFile( string uri, string xpath )
    {
      this.name = Path.GetFileNameWithoutExtension( uri ) + ".xml";
      this.uri = uri;
      this.xpath = xpath;
    }

    #endregion

    #region Properties

    /// <summary>
    /// The relative url that refers to the xml file, including the current query param (if any).
    /// </summary>
    public string Uri
    {
      get
      {
        if( this.queryParam > 0 )
          return this.uri + this.queryParam;
        else
          return this.uri;
      }
    }

    /// <summary>
    /// Sets or gets the query parameter to append to the url. Will be used next request.
    /// </summary>
    public int QueryParam
    {
      get { return this.queryParam; }
      set { this.queryParam = value; }
    }

    /// <summary>
    /// Gets the filename of the xmlfile without any path components, eg, "displaycps.xml".
    /// </summary>
    public string Name
    {
      get { return this.name; }
    }

    /// <summary>
    /// A list of all attribute names to process.
    /// </summary>
    public string[] AttrNames
    {
      get { return this.attributes; }
      set { this.attributes = value; }
    }

    /// <summary>
    /// A list of default values for attributes if an error occurs (null if attribute is required).
    /// </summary>
    public object[] AttrDefaults
    {
      get { return this.defaults; }
      set { this.defaults = value; }
    }

    /// <summary>
    /// The Livedata flag value this file belongs to.
    /// </summary>
    public Livedata DataFlag
    {
      get; set;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Parse the xmlfile and process the data it contains.
    /// </summary>
    /// <param name="net">NetManager object to use when downloading xml files
    /// (null if parsing a MetaXmlFile).</param>
    public void Parse( NetManager net )
    {
      MultiExceptionHandler mex = new MultiExceptionHandler();
      string xpathDefaults = "/" + this.xpath.Split( '/' )[1] + "/defaults/def";


      // call preparse event handler

      if( this.PreParse != null )
        this.PreParse( this, null );


      // get xml document

      XmlDocument xml;
      if( this is MetaXmlFile )
      {
        xml = new XmlDocument();
        xml.XmlResolver = null;  // disable automatic dtd validation
        xml.Load( ( (MetaXmlFile)this ).LocalFile );
      }
      else
      {
        xml = net.GetXmlDocument( this );
      }


      // parse defaults (if present)

      Dictionary<string, string> xmlDefaults = new Dictionary<string, string>();
      foreach( XmlElement node in xml.SelectNodes( xpathDefaults ) )
        if( node.HasAttribute( "att" ) && node.HasAttribute( "value" ) )
          xmlDefaults.Add( node.Attributes["att"].Value, node.Attributes["value"].Value );


      // parse data rows

      XmlDataRows rows = new XmlDataRows( this );

      foreach( XmlElement node in xml.SelectNodes( this.xpath ) )
      {
        try
        {
          string[] row = new string[this.attributes.Length];

          for( int i = 0; i < this.attributes.Length; i++ )
          {
            string attr = this.attributes[i];

            if( node.HasAttribute( attr ) )
              row[i] = node.Attributes[attr].Value;
            else if( xmlDefaults.ContainsKey( attr ) )
              row[i] = xmlDefaults[attr];
#if DEBUG
            else
              mex.Add( new ApplicationException( String.Format( "Parse {0}: missing attr '{1}'", this.name, attr ) ) );
#endif
          }

          rows.Add( row );
        }
        catch( Exception ex ) { mex.Add( ex ); }
      }


      // call the postparse event handler

      try
      {
        if( this.PostParse != null )
          this.PostParse( this, new PostParseEventArgs( rows, xml ) );
      }
      catch( Exception ex ) { mex.Add( ex ); }


      // if any errors, throw first

      mex.Throw();
    }

    /// <summary>
    /// Provides a string representation of this object.
    /// </summary>
    /// <returns>The filename.</returns>
    public override string ToString()
    {
      return this.Name;
    }

    #endregion
  }

  /// <summary>
  /// Init xmlfiles are downloaded and parsed into memory on startup (and wakeup) to establish
  /// the base game state. These can't be cached because they contain dynamic game information.
  /// </summary>
  public class InitXmlFile : XmlFile
  {
    #region Constructors

    public InitXmlFile( string uri, string xpath )
      : base( uri, xpath )
    {

    }

    #endregion
  }

  /// <summary>
  /// Poll xmlfiles are downloaded are parsed into memory every so often, depending on how
  /// frequently the data changes, to maintain the current game state. Where possible,
  /// query parameters are used to specify we only want any new data since last poll.
  /// </summary>
  public class PollXmlFile : XmlFile
  {
    #region Variables

    private TimeSpan updateFreq;
    private DateTime nextUpdate;
    private int newDataCount;

    #endregion

    #region Constructors

    /// <summary>
    /// Create a new empty PollXmlFile.
    /// </summary>
    public PollXmlFile()
      : base( null, null )
    {
      this.updateFreq = TimeSpan.MaxValue;
      this.nextUpdate = DateTime.MaxValue;
      this.newDataCount = 0;
    }

    /// <summary>
    /// Create a new PollXmlFile with a custom update frequency.
    /// </summary>
    /// <param name="uri">The xmlfile's relative url.</param>
    /// <param name="xpath">The xpath referencing the xml data.</param>
    /// <param name="updateFreq">How often to check for new data in minutes.</param>
    public PollXmlFile( string uri, string xpath, int updateFreq )
      : base( uri, xpath )
    {
      this.updateFreq = new TimeSpan( 0, updateFreq, 0 );
      this.nextUpdate = DateTime.Now;
      this.newDataCount = 0;
    }

    #endregion

    #region Properties

    /// <summary>
    /// How often to poll for new data.
    /// </summary>
    public TimeSpan UpdateFreq
    {
      get { return this.updateFreq; }
      set
      {
        this.nextUpdate = this.nextUpdate - this.updateFreq + value;  // convert to new freq
        this.updateFreq = value;
      }
    }

    /// <summary>
    /// The DateTime the next poll is due.
    /// </summary>
    public DateTime NextUpdate
    {
      get { return this.nextUpdate; }
      set { this.nextUpdate = value; }
    }

    /// <summary>
    /// True if this xmlfile is due to be checked for updates.
    /// </summary>
    public bool NeedsChecking
    {
      get { return this.nextUpdate <= DateTime.Now.AddSeconds( 10 ); }  // allow some leeway
    }

    /// <summary>
    /// The number of new records found last poll.
    /// </summary>
    public int NewDataCount
    {
      get { return this.newDataCount; }
      set { this.newDataCount = value; }
    }

    /// <summary>
    /// True if new data was found last poll.
    /// </summary>
    public bool NewData
    {
      get { return this.newDataCount > 0; }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Flag this PollXmlFile as having been checked.
    /// </summary>
    /// <param name="newDataCount">The number of new records found.</param>
    public void FlagAsChecked( int newDataCount )
    {
      this.newDataCount = newDataCount;
      this.nextUpdate = DateTime.Now + this.updateFreq;
    }

    #endregion 
  }

  /// <summary>
  /// Meta xmlfiles contain relatively static game information that can be cached locally.
  /// On load, and every hour, a HEAD request is made to check the Last-Modified header.
  /// If any have been updated, they are re-downloaded and the entire set re-parsed.
  /// </summary>
  public class MetaXmlFile : PollXmlFile
  {
    #region Variables

    private readonly string localFile;
    private readonly bool reloadOnChange;

    #endregion

    #region Constructors

    /// <summary>
    /// Create a new server-side MetaXmlFile.
    /// </summary>
    /// <param name="uri">The xmlfile's relative url.</param>
    /// <param name="xpath">The xpath referencing the xml data.</param>
    /// <param name="reloadOnChange">True if all data should be reloaded when this file changes.</param>
    public MetaXmlFile( string uri, string xpath, bool reloadOnChange )
      : base( uri, xpath, 30 )  // check every 30 mins
    {
      this.localFile = Path.Combine( Wiretap.CachePath, Path.GetFileName( uri ) );
      this.reloadOnChange = reloadOnChange;

      if( File.Exists( this.localFile ) )
        this.NextUpdate = this.LocalFileInfo.LastWriteTime + this.UpdateFreq;
    }

    /// <summary>
    /// Create a new client-side MetaXmlFile.
    /// </summary>
    public MetaXmlFile( string filename, string xpath )
      : base( null, xpath, 0 )
    {
      this.localFile = Path.Combine( Wiretap.CachePath, filename );
    }
    
    #endregion

    #region Properties

    /// <summary>
    /// The full path to the local cache file.
    /// </summary>
    public string LocalFile
    {
      get { return this.localFile; }
    }

    /// <summary>
    /// A FileInfo object for the local cache file.
    /// </summary>
    public FileInfo LocalFileInfo
    {
      get { return new FileInfo( this.localFile ); }
    }

    /// <summary>
    /// True if all data should be reloaded when this file changes.
    /// </summary>
    public bool ReloadOnChange
    {
      get { return this.reloadOnChange; }
    }

    /// <summary>
    /// The Metadata flag value this file belongs to.
    /// </summary>
    public new Metadata DataFlag
    {
      get;
      set;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Gets whether the cached xml file is valid (ie, not empty or truncated).
    /// </summary>
    public bool IsValid
    {
      get
      {
        // check size

        if( this.LocalFileInfo.Length == 0 )
          return false;


        // test parse

        XmlDocument xmldoc = new XmlDocument();
        xmldoc.XmlResolver = null;  // disable automatic dtd validation

        try
        {
          xmldoc.Load( this.localFile );
        }
        catch( Exception )
        {
          return false;
        }

        return true;
      }
    }

    /// <summary>
    /// Parse the xmlfile and process the data it contains.
    /// </summary>
    public void Parse()
    {
      base.Parse( null );
    }

    /// <summary>
    /// Flag this MetaXmlFile as having been checked.
    /// </summary>
    public void FlagAsChecked()
    {
      this.NextUpdate = DateTime.Now + this.UpdateFreq;
    }

    /// <summary>
    /// Update the local cache file's mtime.
    /// </summary>
    /// <remarks>Used to avoid checking for updates more often than once an hour,
    /// including spanning program launches.</remarks>
    public void Touch()
    {
      File.SetLastWriteTime( this.LocalFileInfo.FullName, DateTime.Now );
    }

    #endregion
  }

  /// <summary>
  /// A collection of the data parsed from an xml file. Handles parsing values into the
  /// required type, and provides fail-safe default values if an attribute is missing.
  /// </summary>
  public class XmlDataRows
  {
    #region Variables

    private readonly string xmlname;
    private readonly List<string[]> data;
    private readonly Dictionary<string, int> attr;  // map name -> index
    private readonly object[] defaults;
    private int currentOffset;

    #endregion

    #region Constructors

    /// <summary>
    /// Create a new xml data collection.
    /// </summary>
    /// <param name="xmlfile">The xmlfile this collection belongs to.</param>
    public XmlDataRows( XmlFile xmlfile )
    {
      this.xmlname = xmlfile.Name;
      this.data = new List<string[]>();

      this.attr = new Dictionary<string, int>();
      for( int i = 0; i < xmlfile.AttrNames.Length; i++ )
        this.attr.Add( xmlfile.AttrNames[i], i );

      this.defaults = xmlfile.AttrDefaults;
      this.currentOffset = 0;
    }

    #endregion

    #region Properties

    /// <summary>
    /// The name of the parent xmlfile.
    /// </summary>
    public string XmlName
    {
      get { return this.xmlname; }
    }

    /// <summary>
    /// The number of rows in the collection.
    /// </summary>
    public int Count
    {
      get { return this.data.Count; }
    }

    /// <summary>
    /// The current row offset used by GetValue().
    /// </summary>
    public int Offset
    {
      get { return this.currentOffset; }
      set { this.currentOffset = value; }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Add a data row to the collection.
    /// </summary>
    /// <param name="row">The array of values to add.</param>
    public void Add( string[] row )
    {
      this.data.Add( row );
    }

    /// <summary>
    /// Gets the value of an attribute at the current row offset.
    /// </summary>
    /// <typeparam name="T">The type to parse the value to.</typeparam>
    /// <param name="attr">The name of the attribute to retrieve.</param>
    /// <returns>The attribute value as the given type.</returns>
    public T GetValue<T>( string attr )
    {
      return GetValue( attr, false, default( T ) );
    }

    /// <summary>
    /// Gets the value of an attribute at the current row offset.
    /// </summary>
    /// <typeparam name="T">The type to parse the value to.</typeparam>
    /// <param name="attr">The name of the attribute to retrieve.</param>
    /// <param name="defaultValue">The default value to use if the attribute is missing.</param>
    /// <returns>The attribute value as the given type, or defaultValue if missing.</returns>
    public T GetValue<T>( string attr, T defaultValue )
    {
      return GetValue( attr, true, defaultValue );
    }

    /// <summary>
    /// Gets the value of an attribute at the current row offset.
    /// </summary>
    /// <typeparam name="T">The type to parse the value to.</typeparam>
    /// <param name="attr">The name of the attribute to retrieve.</param>
    /// <param name="useDefaultValue">True if defaultValue should be used.</param>
    /// <param name="defaultValue">The default value to use if the attribute is missing.</param>
    /// <returns>The attribute value as the given type.</returns>
    private T GetValue<T>( string attr, bool useDefaultValue, T defaultValue )
    {
      // get value to parse

      if( !this.attr.ContainsKey( attr ) )
        throw new ApplicationException( String.Format( "unknown attribute '{0}'", attr ) );
      int i = this.attr[attr];
      string value = this.data[this.currentOffset][i];


      // try return default if missing from xml

      if( String.IsNullOrEmpty( value ) )
      {
        if( useDefaultValue )                // use supplied default
          return defaultValue;
        else if( this.defaults[i] != null )  // use internal default
          return (T)this.defaults[i];
        else                                 // internal default == null, attribute required
          throw new ApplicationException( String.Format( "No default value for missing attribute '{0}' ({1}:{2})", attr, this.xmlname, this.currentOffset ) );
      }


      // parse into dest type

      Type type = typeof( T );
      try
      {
        return Misc.ParseToType<T>( value );
      }
      catch( Exception ex )
      {
#if DEBUG
        Log.AddError( "GetValue {0}: failed to parse attr '{1}' value '{2}' to type {3}", this.xmlname, attr, value, type );
#endif
        if( this.defaults[i] != null )  // use internal default
          return (T)this.defaults[i];
        else
          throw new ApplicationException( String.Format( "Failed to parse attr '{0}' value '{1}' to type {2}: {3}", attr, value, type, ex.Message ), ex );
      }
    }

    /// <summary>
    /// Gets the maximum value of an int attribute.
    /// </summary>
    /// <param name="attr">The name of the attribute (must parse to int).</param>
    /// <returns>The largest number found.</returns>
    public int GetMaxValue( string attr )
    {
      int max = 0;
      int i = this.attr[attr];

      foreach( string[] row in data )
      {
        try
        {
          int value = int.Parse( row[i] );
          if( value > max ) max = value;
        }
        catch { }
      }

      return max;
    }

    #endregion
  }

  /// <summary>
  /// EventArgs for the PostParse event handler.
  /// </summary>
  public class PostParseEventArgs : EventArgs
  {
    #region AutoProperties

    /// <summary>
    /// The data parsed from the xml file.
    /// </summary>
    public XmlDataRows Data { get; private set; }

    /// <summary>
    /// The xml document object.
    /// </summary>
    public XmlDocument Xml { get; private set; }

    #endregion

    #region Constructors

    /// <summary>
    /// Create a new PostParseEventArgs object.
    /// </summary>
    public PostParseEventArgs( XmlDataRows data, XmlDocument xml )
    {
      this.Data = data;
      this.Xml = xml;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Checks whether the given index exists in the list.
    /// </summary>
    /// <param name="attr">The attribute name.</param>
    /// <param name="index">The attribute value.</param>
    /// <param name="list">The list to test against.</param>
    public void CheckAttribute( string attr, int index, IList list )
    {
      if( index < 0 || index >= list.Count || list[index] == null )
        throw new InvalidAttributeException( this.Data.XmlName, attr, index );
    }

    /// <summary>
    /// Checks whether the given key exists in the Dictionary.
    /// </summary>
    /// <typeparam name="TKey">The type of the dictionary key.</typeparam>
    /// <typeparam name="TValue">The type of the dictionary value.</typeparam>
    /// <param name="attr">The attribute name.</param>
    /// <param name="key">The attribute value.</param>
    /// <param name="dictionary">The dictionary to test against.</param>
    public void CheckAttribute<TKey, TValue>( string attr, TKey key, IDictionary<TKey, TValue> dictionary )
      where TValue : class  // must be reference type because we check if null
    {
      if( !dictionary.ContainsKey( key ) || dictionary[key] == null )
        throw new InvalidAttributeException( this.Data.XmlName, attr, key );
    }

    /// <summary>
    /// Checks whether the given value is defined in the Enum.
    /// </summary>
    /// <param name="attr">The attribute name.</param>
    /// <param name="value">The attribute value.</param>
    /// <param name="enumtype">The enum type to test against.</param>
    public void CheckAttribute( string attr, int value, Type enumtype )
    {
      if( !Enum.IsDefined( enumtype, value ) )
        throw new InvalidAttributeException( this.Data.XmlName, attr, value );
    }

    /// <summary>
    /// Checks whether the given index already exists in the list.
    /// </summary>
    /// <param name="attr">The attribute name.</param>
    /// <param name="index">The attribute value.</param>
    /// <param name="list">The list to test against.</param>
    public void CheckAttributeDupe( string attr, int index, IList list )
    {
      if( index < 0 || index > list.Count || list[index] != null )
        throw new DuplicateAttributeException( this.Data.XmlName, attr, index );
    }

    /// <summary>
    /// Checks whether the given key already exists in the Dictionary.
    /// </summary>
    /// <typeparam name="TKey">The type of the dictionary key.</typeparam>
    /// <typeparam name="TValue">The type of the dictionary value.</typeparam>
    /// <param name="attr">The attribute name.</param>
    /// <param name="key">The attribute value.</param>
    /// <param name="dictionary">The dictionary to test against.</param>
    public void CheckAttributeDupe<TKey, TValue>( string attr, TKey key, IDictionary<TKey, TValue> dictionary )
    {
      if( dictionary.ContainsKey( key ) )
        throw new DuplicateAttributeException( this.Data.XmlName, attr, key );
    }

    #endregion
  }
}
