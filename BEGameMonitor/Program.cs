/* =============================================================================
 * BEGameMonitor                                    Copyright (c) 2013 Xiperware
 * http://begm.sourceforge.net/                              xiperware@gmail.com
 * 
 * This file is part of Battleground Europe Game Monitor.
 * 
 * Battleground Europe Game Monitor is free software; you can redistribute it
 * and/or modify it under the terms of the GNU General Public License v2 as
 * published by the Free Software Foundation.
 * 
 * This program is distributed in the hope that it will be useful, but WITHOUT
 * ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 * FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License along with
 * this program. If not, see <http://www.gnu.org/licenses/>.
 * =============================================================================
 */

using System;
using System.Diagnostics;  // Process
using System.Globalization;
using System.Reflection;  // Assembly
using System.Threading;
using System.Windows.Forms;
using Xiperware.WiretapAPI;

namespace BEGM
{
  /// <summary>
  /// The entry point of the program.
  /// </summary>
  public static class Program
  {
    #region Variables

    /// <summary>
    /// A static reference to the GameStatus form.
    /// </summary>
    public static GameStatus gameStatus = null;

    /// <summary>
    /// The version of the current assembly.
    /// </summary>
    public static Version version = null;

    /// <summary>
    /// The formatted version string of the current assembly ("v1.2").
    /// </summary>
    public static string versionString = null;

    /// <summary>
    /// The default UI Culture to use for new threads.
    /// </summary>
    public static CultureInfo uiCulture = null;

    #endregion

    #region Methods

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    /// <param name="args">
    /// BEGameMonitor.exe                     - start normally (may be minimised depending on startup options)
    /// BEGameMonitor.exe minimised           - start minimised
    /// BEGameMonitor.exe sleep               - start in sleep mode (autowakeup off)
    /// BEGameMonitor.exe sleep autowakeup    - start in sleep mode (autowakeup on)
    /// </param>
    [STAThread]
    public static void Main( string[] args )
    {
      try  // last-resort try/catch block
      {

#if !DEBUG
        // unless debugging, handle all thread exceptions
        Application.ThreadException += Application_ThreadException;
#endif

        // get language

        LangSettings lang = new LangSettings();
        lang.LoadFromRegistry();

        string langLogError = null, langLogEntry = null;
        uiCulture = Thread.CurrentThread.CurrentUICulture;

        try
        {
          uiCulture = new CultureInfo( lang.langCode );
          Thread.CurrentThread.CurrentUICulture = uiCulture;
          langLogEntry = String.Format( "Setting language to {0}", uiCulture.EnglishName );
        }
        catch( Exception ex )
        {
          langLogError = String.Format( "Failed to set language: {0}", ex.Message );
        }


        // get app version info

        version = Assembly.GetExecutingAssembly().GetName().Version;
        versionString = Misc.VersionToString( version );


        // require win2k or greater, due to:
        // - GetLastInputInfo
        // - CredUIConfirmCredentials

        if( Environment.OSVersion.Version.Major < 5 )
        {
          MessageBox.Show( Language.Error_WindowsVersion,
                           Language.Error_Error, MessageBoxButtons.OK, MessageBoxIcon.Error );
          return;
        }


        // perform basic dupe process detection
#if !MAC
        if( Process.GetProcessesByName( Process.GetCurrentProcess().ProcessName ).Length > 1 )
        {
          // make sure we're not just waiting for ourself to exit after changing to/from sleep mode
          Thread.Sleep( 2000 );

          if( Process.GetProcessesByName( Process.GetCurrentProcess().ProcessName ).Length > 1 )
          {
            MessageBox.Show( Language.Error_AlreadyRunning,
                             Language.Error_Error, MessageBoxButtons.OK, MessageBoxIcon.Error );
            return;
          }
        }
#endif


        // process command-line arguments

        bool argSleep = false;
        if( args.Length >= 1 && args[0] == "sleep" )
          argSleep = true;

        bool argAutowakeup = false;
        if( argSleep && args.Length >= 2 && args[1] == "autowakeup" )
          argAutowakeup = true;

        bool argMinimised = false;
        if( args.Length >= 1 && args[0] == "minimised" )
          argMinimised = true;
        

        // init log to file (will append if changing sleep <=> running)

        Log.OnNewLogEntry += Log.WriteToFile( "BEGameMonitor.log" );

        if( langLogError != null )
          Log.AddError( langLogError );
        else if( langLogEntry != null )
          Log.AddEntry( langLogEntry );


        // make it pretty

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault( false );


        // if sleep arg, do sleep mode instead

        if( argSleep )
        {
          Log.AddEntry( "In sleep mode (autowakeup {0})", argAutowakeup ? "on" : "off" );
          new SleepTrayIcon( argAutowakeup );
          Application.Run();
          return;
        }


        // create game status window

        gameStatus = new GameStatus();


        // manually hide tary icon on app exit (tends to hang around until mouseover sometimes?!?)

        Application.ApplicationExit += Application_ApplicationExit;


        // make sure windows set to default dpi for now (otherwise will draw everything screwy)
        // TODO: make dpi independant?

        System.Drawing.Graphics g = gameStatus.CreateGraphics();
        int screenDpi = (int)g.DpiX;
        g.Dispose();

        if( screenDpi != 96 )
        {
          MessageBox.Show( String.Format( Language.Error_NonDefaultDPI, screenDpi ),
                           Language.Error_Error, MessageBoxButtons.OK, MessageBoxIcon.Error );
          gameStatus.trayIcon.Visible = false;
          return;
        }


        // trick to force handle creation before Show()ing the form (to allow Invoke() to be called)

        IntPtr handle = gameStatus.Handle;


        // start initialisation in background, show the gui if necessary, and start the message loop

        gameStatus.Init();

        if( gameStatus.options.Startup.startMinimised || argMinimised )
          {}  // start minimised
        else
          gameStatus.ShowWindow();

        Application.Run();
        
      }
#if DEBUG
      catch
      {
        throw;
      }
#else
      catch( Exception ex )  // handle any uncaught exceptions that occured in the main thread
      {
        UnhandledException( ex );
      }
#endif
    }  // end main

    /// <summary>
    /// Give the user some information on what happened and a stack trace to send back.
    /// </summary>
    /// <param name="ex">The exception object.</param>
    private static void UnhandledException( Exception ex )
    {
      if( Log.FileOpen )
      {
        // write exception details to log

        System.Text.StringBuilder logentry = new System.Text.StringBuilder( "Unhandled exception\r\n\r\n" );
        logentry.AppendFormat( "Message: {0}\r\nSource: {1}\r\nTarget: {2}\r\nStackTrace:\r\n{3}\r\n",
                               ex.Message, ex.Source, ex.TargetSite, ex.StackTrace );

        Exception inner = ex;
        while( inner.InnerException != null )
        {
          inner = inner.InnerException;
          logentry.AppendFormat( "\r\n\r\nInner Exception\r\nMessage: {0}\r\nSource: {1}\r\nTarget: {2}\r\nStackTrace:\r\n{3}\r\n",
                                 inner.Message, inner.Source, inner.TargetSite, inner.StackTrace );
        }

        Log.AppendToLogFile( logentry.ToString() );


        // display error dialog

        MessageBox.Show( String.Format( Language.Error_CrashLogfile, Log.LogFileName ) + "\n\n\n" + ex,
                         String.Format( Language.Error_CrashTitle, Program.versionString ),
                         MessageBoxButtons.OK, MessageBoxIcon.Error );

      }
      else  // no logfile
      {
        MessageBox.Show( Language.Error_CrashNoLogfile + "\n\n\n" + ex,
                         String.Format( Language.Error_CrashTitle, Program.versionString ),
                         MessageBoxButtons.OK, MessageBoxIcon.Error );
      }
    }

    #endregion

    #region Event Handlers

    /// <summary>
    /// When exiting, forcibly remove the tray icon.
    /// </summary>
    private static void Application_ApplicationExit( object sender, EventArgs e )
    {
      try
      {
        gameStatus.trayIcon.Visible = false;
      }
      catch { }  // if( trayIcon != null ) doesn't work?
    }

    /// <summary>
    /// Handle any uncaught exceptions that occurred in a thread.
    /// </summary>
    private static void Application_ThreadException( object sender, ThreadExceptionEventArgs e )
    {
      UnhandledException( e.Exception );
      Application.Exit();  // don't continue
    }

    #endregion
  }
}
