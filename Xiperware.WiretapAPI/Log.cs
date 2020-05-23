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
using System.IO;
using System.Windows.Forms;

namespace Xiperware.WiretapAPI
{
  public delegate void AddEntryDelegate( string entry, bool error );

  /// <summary>
  /// A static logger class, used for adding log entries throughout the program.
  /// </summary>
  public static class Log
  {
    #region Variables

    /// <summary>
    /// File handle to log file, if using WriteToFile.
    /// </summary>
    private static TextWriter logfile;

    #endregion

    #region Properties

    /// <summary>
    /// The name of the current log file.
    /// </summary>
    public static string LogFileName { get; private set; }

    /// <summary>
    /// A reference to the parent Control, used to detect which thread we are in
    /// (and invoke correct thread if necessary).
    /// </summary>
    public static Control MainThread { private get; set; }

    /// <summary>
    /// True if a logfile is currently open.
    /// </summary>
    public static bool FileOpen
    {
      get { return logfile != null; }
    }

    #endregion

    #region Events

    /// <summary>
    /// Called when a new log entry is made.
    /// </summary>
    public static event AddEntryDelegate OnNewLogEntry;

    #endregion

    #region Methods

    /// <summary>
    /// Add a normal entry to the log.
    /// </summary>
    /// <param name="entry">The text of the log entry.</param>
    public static void AddEntry( string entry )
    {
      ThreadSafeAddEntry( entry, false );
    }

    /// <summary>
    /// Add a normal entry to the log.
    /// </summary>
    /// <param name="format">A System.String containing zero or more format items.</param>
    /// <param name="args">An System.Object array containing zero or more objects to format.</param>
    public static void AddEntry( string format, params object[] args )
    {
      ThreadSafeAddEntry( String.Format( format, args ), false );
    }

    /// <summary>
    /// Add an error entry to the log.
    /// </summary>
    /// <param name="entry">The text of the log entry.</param>
    public static void AddError( string entry )
    {
      ThreadSafeAddEntry( entry, true );
    }

    /// <summary>
    /// Add an error entry to the log.
    /// </summary>
    /// <param name="format">A System.String containing zero or more format items.</param>
    /// <param name="args">An System.Object array containing zero or more objects to format.</param>
    public static void AddError( string format, params object[] args )
    {
      ThreadSafeAddEntry( String.Format( format, args ), true );
    }

    /// <summary>
    /// Add an exception error entry to the log.
    /// </summary>
    /// <param name="ex">The exception to log.</param>
    public static void AddException( Exception ex )
    {
      string indent = "";
      while( ex != null )
      {
        if( ex.InnerException == null )  // last stack frame
          ThreadSafeAddEntry( ex.ToString(), true );
        else
          ThreadSafeAddEntry( indent + ex.Message, true );

        ex = ex.InnerException;
        indent += "  ";
      }
    }

    /// <summary>
    /// Add the text "ERROR" to the log, without prepending a newline.
    /// </summary>
    public static void Error()
    {
      ThreadSafeAddEntry( "ERROR", true );
    }

    /// <summary>
    /// Add the text "OK" to the log, without prepending a newline.
    /// </summary>
    public static void Okay()
    {
      ThreadSafeAddEntry( "OK", false );
    }

    /// <summary>
    /// Add a log entry in a thread-safe manner by calling self from main thread if necessary.
    /// </summary>
    /// <param name="entry">The text of the log entry.</param>
    /// <param name="error">True if the text should appear highlighted red.</param>
    private static void ThreadSafeAddEntry( string entry, bool error )
    {
      if( MainThread != null && MainThread.InvokeRequired )  // we are in a different thread to the main window
        MainThread.Invoke( new AddEntryDelegate( ThreadSafeAddEntry ), new object[] { entry, error } );  // call self from main thread
      else if( OnNewLogEntry != null )
        OnNewLogEntry( entry, error );
    }

    /// <summary>
    /// Initialises a log file writer and returns the delegate to use.
    /// </summary>
    /// <param name="filename">The filename to write to (not including path).</param>
    /// <returns>An AddEntryDelegate that can be added to Log.OnNewLogEntry.</returns>
    public static AddEntryDelegate WriteToFile( string filename )
    {
      try
      {
        LogFileName = Path.Combine( Application.StartupPath, filename );
        bool append = false;

        if( File.Exists( LogFileName ) )
        {
          // get last modified time

          if( new FileInfo( LogFileName ).LastWriteTime > DateTime.Now.AddSeconds( -4 ) )
          {
            // modified in last 4 seconds: just transitioned sleep <=> running

            append = true;
          }
          else
          {
            // overwrite, but keep previous log

            string logfilename2 = LogFileName.Replace( ".log", ".prev.log" );
            if( File.Exists( logfilename2 ) ) File.Delete( logfilename2 );
            File.Move( LogFileName, logfilename2 );
          }
        }


        // open log file

        logfile = new StreamWriter( LogFileName, append );
        ( (StreamWriter)logfile ).AutoFlush = true;


        // write header

        if( !append )
        {
          logfile.WriteLine( "{0} v{1}", Application.ProductName, Application.ProductVersion );
          logfile.WriteLine( Application.ExecutablePath );
          logfile.WriteLine( "Log created at {0:ddd, dd MMM yyyy HH:mm:ss G\\MTzzz}", DateTime.Now );
        }


        // return AddEntryDelegate

        return AppendToLogFile;
      }
      catch
      {
        // if error, return null AddEntryDelegate

        logfile = null;
        return null;
      }
    }

    /// <summary>
    /// Append a log entry directly to the log file, without invoking the other AddEntryDelegate's.
    /// </summary>
    /// <param name="format">A System.String containing zero or more format items.</param>
    /// <param name="args">An System.Object array containing zero or more objects to format.</param>
    public static void AppendToLogFile( string format, params object[] args )
    {
      AppendToLogFile( String.Format( format, args ), false );
    }

    /// <summary>
    /// Append a log entry directly to the log file, without invoking the other AddEntryDelegate's.
    /// </summary>
    /// <param name="entry">The text of the log entry.</param>
    public static void AppendToLogFile( string entry )
    {
      AppendToLogFile( entry, false );
    }

    /// <summary>
    /// The internal AddEntryDelegate used to write to the logfile.
    /// </summary>
    /// <param name="entry">The text of the log entry.</param>
    /// <param name="error">True if the text should appear highlighted red (unused).</param>
    private static void AppendToLogFile( string entry, bool error )
    {
      if( logfile == null ) return;

      try
      {
        if( entry == "OK" || entry == "ERROR" )  // no timestamp or preceding newline
          logfile.Write( " " + entry );
        else                                     // normal log entry
          logfile.Write( "\r\n{0} {1}", Misc.Timestamp( DateTime.Now ), entry );
      }
      catch
      {
        logfile = null;  // if error, don't write anymore
      }
    }

    #endregion
  }
}
