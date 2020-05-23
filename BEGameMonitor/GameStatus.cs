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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.Reflection;  // BindingFlags
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.IO;
using Microsoft.Win32;  // Registry
using BEGM.Properties;
using XLib.Extensions;
using XPExplorerBar;
using Xiperware.WiretapAPI;

namespace BEGM
{
  /// <summary>
  /// The main game window.
  /// </summary>
  public partial class GameStatus : Form
  {
    #region Variables

    private const int COLUMN_WIDTH = 350;  // includes right gutter
    private const int COLUMN_GUTTER = 12;
    private const int SNAP_COLUMNS = 40;  // distance to snap to when resizing columns

    private readonly Expando[] allExpandos;
    private readonly IWidget[] allWidgets;
    private readonly GameState game;
    private readonly Wiretap wiretap;
    public readonly Options options;
    private readonly About aboutDialog;
    private readonly AlertWindow alertWindow;

    private bool factoryDataLoaded = false;
    private bool ww2Running = false;
    private bool delaySleepWhenPlay = false;
    private DateTime lastPollTime;

    private ProcessStartInfo procPlaygate, procWW2Live;

    private Size resizeWindowsStartSize;
    private Point resizeWindowStartMouse;
    private FormWindowState prevWindowState = FormWindowState.Normal;
    private List<GameStatusColumn> columns = new List<GameStatusColumn>();

    private string helpFile;

    #endregion

    #region Constructors

    /// <summary>
    /// Create a new GameStatus window.
    /// </summary>
    public GameStatus()
    {
      // create form, controls

      InitializeComponent();

      GameStatus.ToolTip = this.toolTip;

#if MAC
      // set expandos to non-animated

      foreach( Expando expando in tskMain.Expandos )
      {
        expando.Animate = false;
      }
#endif

      // create arrays of expandos & widgets
      // (note: must be in same order as WidgetType enum)

      allExpandos = new[] { expServerStatus, expRecentEvents, expCurrentAttacks, expTownStatus, expGameMap,
                            expFactoryStatus, expOrderOfBattle, expBrigadeStatus, expEquipment };

      allWidgets = new IWidget[] { wgtServerStatus, wgtRecentEvents, wgtCurrentAttacks, wgtTownStatus, wgtGameMap,
                                   wgtFactoryStatus, wgtOrderOfBattle, wgtBrigadeStatus, wgtEquipment };

      foreach( Expando expando in allExpandos )
      {
        expando.CustomHeaderSettings.NormalPadding = new XPExplorerBar.Padding( 2, 0, 0, 0 );  // vs designer bug
        expando.DefaultTaskPane = tskMain;
      }


      // dirty hack to get rid of vertical autoscroll bar:
      // increase width to push scrollbar off the side of the form, and compensate with padding

      tskMain.Width += SystemInformation.VerticalScrollBarWidth;
      tskMain.CustomSettings.Padding = new XPExplorerBar.Padding( 12, 12, 12 + SystemInformation.VerticalScrollBarWidth, 12 );
      
      columns.Add( new GameStatusColumn( pnlMainColumn ) );


      // set all widgets to non-expandable during startup

      foreach( Expando expando in allExpandos )
        expando.CanCollapse = false;


      // update version number

      lnkVersion.Text = Program.versionString;
      lnkVersion.Links[0].Enabled = false;


      // dynamic positioning

      lnkAbout.Left = lnkOptions.Right + 6;
      lnkHelp.Left = lnkAbout.Right + 6;


      // init about dialog, alert window

      aboutDialog = new About();
      alertWindow = new AlertWindow();
      alertWindow.GameStatus_RevealWidget = RevealWidget;


      // init logger

      Log.MainThread = this;
      Log.OnNewLogEntry += aboutDialog.AddLogEntry;  // add entries to Log tab in about dialog
      Log.OnNewLogEntry += Log_OnNewLogEntry;        // update status bar


      // try load hires map plugin

      wgtGameMap.LoadHiresMapPlugin();


      // init game state and wiretap object, subscribe to data

      game = new GameState();
      game.Wiretap = this.wiretap = new Wiretap();
      game.Events = new GameEventCollection();

      wiretap.Subscribe( Metadata.Map | Metadata.Vehicle | Metadata.Toes | Metadata.HCUnit | Metadata.Config );
      wiretap.Subscribe( Livedata.ChokePoint | Livedata.Capture | Livedata.HCUnit | Livedata.Movement
                         | Livedata.Firebase | Livedata.Deathmap | Livedata.Servers );
      
      wiretap.Net.OnBytesDownloadedChanged += NetManager_OnBytesDownloadedChanged;  // update about dialog


      // init options

      options = new Options( wiretap.Net );

      options.CheckEventsClicked += options_CheckEventsClicked;
      options.ResetAllDataClicked += options_ResetAllDataClicked;
      options.DockWindowChanged += options_DockWindowChanged;
      options.EventSortOrderChanged += options_EventSortOrderChanged;
      options.GameStatusDisplayChanged += options_GameStatusDisplayChanged;
      options.MapOptionsChanged += options_MapOptionsChanged;
      options.MapWallpaperChanged += options_MapWallpaperChanged;
      options.LanguageChanged += options_LanguageChanged;

      options.GameStatus_CheckIfBusy = CheckIfBusy;            // make sure no background tasks in progress
      options.GameStatus_UpdateContextMenu = UpdateContextMenu;      // update "disable all alerts" in context menu
      options.AlertWindow_TestAlert = alertWindow.TestAlert;  // "test alert" button
      options.CurrentAttacks_UpdateShowAlertsCheckboxes = wgtCurrentAttacks.UpdateShowAlertsCheckboxes;  // update "show events" checkboxs

      if( wgtGameMap.HiresMapPluginLoaded )
        options.EnableHiresMapOptions();

      options.LoadSettings();
      Log.AddEntry( "Using server {0}", options.Network );
      options.LoadWidgetPosition( this.columns, this.allExpandos );

      miTrayDisableAllAlerts.Checked = !options.Alerts.showAlerts;  // set init checked state
      aboutDialog.PrevTotalDownload = options.GameStatusState.totalKBytesDownloaded;  // tell the about dialog what our previous total was
      alertWindow.options = wgtCurrentAttacks.options = options;    // needs to modify alert settings
      wgtGameMap.options = options;
      UpdateEventLists();  // event list sort order

      wgtServerStatus.State = options.ServerStatusState;
      wgtRecentEvents.State = options.RecentEventsState;
      //wgtTownStatus.State done after init
      wgtGameMap.State = options.GameMapState;
      wgtFactoryStatus.State = options.FactoryStatusState;
      wgtBrigadeStatus.State = options.BrigadeStatusState;


      // only load factory data if specified

      if( options.Startup.loadFactoryData )
      {
        wiretap.Subscribe( Livedata.Factory );
        factoryDataLoaded = true;
      }

      wgtFactoryStatus.ShowLoadMessage( !factoryDataLoaded );
      wgtRecentEvents.FactoryCheckboxEnabled = factoryDataLoaded;


      // make note of whether ww2 was running when we were started

      this.ww2Running = BegmMisc.WW2Running();
      if( options.Startup.sleepWhenPlay )
        this.delaySleepWhenPlay = this.ww2Running;


      // widget delegates

      foreach( IWidget widget in this.allWidgets )
        widget.GameStatus_RevealWidget = RevealWidget;  // jump to another widget

      wgtFactoryStatus.GameStatus_LoadFactoryData = LoadFactoryData;              // "load factory data" button
      wgtTownStatus.CurrentAttacks_NumEvents      = wgtCurrentAttacks.NumEvents;  // get event count for town


      // show launch menu if game installed

      SetupLaunchMenu();


      // watermark internal build

#if INTERNAL_BUILD
      AddLogoWatermark();
#endif


      // locate help file

      string helpDefault = Path.Combine( Application.StartupPath, "BEGameMonitor.chm" );
      string helpLocal = Path.Combine( Application.StartupPath, String.Format( "{0}\\BEGameMonitor.{0}.chm", options.Lang.langCode ) );

      if( !options.Lang.langCode.StartsWith( "en" ) && File.Exists( helpLocal ) )
        this.helpFile = helpLocal;
      else if( File.Exists( helpDefault ) )
        this.helpFile = helpDefault;
      else
        lnkHelp.Enabled = false;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets the internal widget state.
    /// </summary>
    internal GameStatusState State
    {
      get
      {
        Rectangle bounds = this.WindowState == FormWindowState.Normal
                         ? this.Bounds
                         : this.RestoreBounds;

        GameStatusState state = new GameStatusState
        {
          windowSize = bounds.Size,
          windowLocation = bounds.Location,
          totalKBytesDownloaded = options.GameStatusState.totalKBytesDownloaded  // previous total
                                  + ( wiretap.Net.BytesDownloaded / 1024 )       // plus new usage
        };

        return state;
      }
    }

    /// <summary>
    /// Allow other widgets to access the GameStatus tooltip object.
    /// </summary>
    public static ToolTip ToolTip
    {
      get; set;
    }

    /// <summary>
    /// Allow minimise by clicking on taskbar icon when docked.
    /// </summary>
    protected override CreateParams CreateParams
    {
      get
      {
        CreateParams cp = base.CreateParams;

        if( this.FormBorderStyle == FormBorderStyle.None )  // docked
          cp.Style |= 0x20000;  // WS_MINIMIZEBOX

        return cp;
      }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Perform program initialisation in the background, making sure metadata is current
    /// and establishing initial game state. If any network errors occur, user is prompted
    /// to check their settings, otherwise each widget is initialised and the polling event
    /// loop started.
    /// </summary>
    public void Init()
    {
      GameEvent.syncEventReceivedTime = true;  // during init
      picInit.Visible = lblInit.Visible = true;

      bgwInit.RunWorkerAsync();
    }

    /// <summary>
    /// Init() background thread.
    /// </summary>
    private void bgwInit_DoWork( object sender, DoWorkEventArgs e )
    {
      // set thread ui culture

      Thread.CurrentThread.CurrentUICulture = Program.uiCulture;


      // update/parse metadata, download/parse init files

      wiretap.GetMetaData( BegmMisc.ShowError );
      wiretap.GetInitState( BegmMisc.ShowError );


      // do version check

      if( options.Startup.checkVersion && VersionCheckNeeded() )
        e.Result = IsNewerVersion();
    }

    /// <summary>
    /// Init() completed handler.
    /// </summary>
    private void bgwInit_RunWorkerCompleted( object sender, RunWorkerCompletedEventArgs e )
    {
      GameEvent.syncEventReceivedTime = false;  // reset
      picInit.Visible = lblInit.Visible = false;
      

      if( e.Error != null )  // exception occured in thread
      {
        if( !( e.Error is NetworkException ) )
          throw e.Error;

        Log.AddException( e.Error );

        DialogResult result = MessageBox.Show( string.Format( "{0}\n{1}\n\n" + Language.Error_CheckNetworkSettings, e.Error.Message, e.Error.InnerException.Message ),
                                               "Battleground Europe Game Monitor", MessageBoxButtons.OKCancel, MessageBoxIcon.Error );
        if( result == DialogResult.Cancel )
        {
          ExitBEGM();
          return;
        }
        // else OK, show options dialog


        Log.AddEntry( "User checking network settings" );

        options.tabControl.SelectedIndex = 0;  // select network tab

        options.UpdateTabEnabled = false;  // prevent user from doing check new events/reset all
        options.ShowDialog();
        options.UpdateTabEnabled = true;

      //MessageBox.Show( string.Format( Language.Error_TryAgain, "Battleground Europe Game Monitor" ), "Battleground Europe Game Monitor",
      //                 MessageBoxButtons.OK, MessageBoxIcon.Information );
        Reload();
        return;
      }


      // populate chokepoint alert filter

      options.PopulateChokePointFilter( game.ChokePoints );


      // init widgets

      Log.AddEntry( "Initialising widgets:" );

      foreach( IWidget widget in this.allWidgets )
        widget.InitWidget( this.game );

      wgtTownStatus.State = options.TownStatusState;  // maintain selected cp
      UpdateLaunchMenu();


      // add campaign info to logo

#if !INTERNAL_BUILD
      AddLogoCampaign();
#endif


      // enable version link to homepage if newer version was detected

      if( e.Result is int )  // version was checked
      {
        switch( (int)e.Result )
        {
          case 0:
            toolTip.SetToolTip( lnkVersion, Language.GameStatus_NoUpdateAvailable );
            break;
          case 1:
            lnkVersion.LinkBehavior = LinkBehavior.HoverUnderline;
            lnkVersion.Links[0].Enabled = true;
            toolTip.SetToolTip( lnkVersion, Language.GameStatus_UpdateAvailable );
            break;
          case 2:
            lnkVersion.LinkBehavior = LinkBehavior.HoverUnderline;
            lnkVersion.Links[0].Enabled = true;
            toolTip.SetToolTip( lnkVersion, Language.GameStatus_UpdateAvailablePlugin );
            break;
        }
      }


      // animate widgets to prev state

      foreach( Expando expando in this.allExpandos )
        expando.CanCollapse = true;
      options.LoadWidgetState( this.allExpandos );


      // start polling

#if !DEBUG_FRONTLINE
      tmrEventLoop.Start();
      lastPollTime = DateTime.Now;
#endif

      Log.AddEntry( "BEGM initialisation successful" );
    }



    /// <summary>
    /// Starts the background process of loading and parsing the initial factory state.
    /// </summary>
    private void LoadFactoryData()
    {
      // make sure not polling

      if( CheckIfBusy() )
      {
        MessageBox.Show( Language.Error_EventLoopBusy,
                         "Battleground Europe Game Monitor", MessageBoxButtons.OK, MessageBoxIcon.Information );
        return;
      }


      // collapse & disable widget

      expFactoryStatus.Collapsed = true;
      expFactoryStatus.CanCollapse = false;
      expFactoryStatus.Cursor = Cursors.WaitCursor;


      // subscribe to factory data

      wiretap.Subscribe( Livedata.Factory );


      // start background thread

      bgwInitFactory.RunWorkerAsync();
    }

    /// <summary>
    /// LoadFactoryData() background thread.
    /// </summary>
    private void bgwInitFactory_DoWork( object sender, DoWorkEventArgs e )
    {
      // set thread ui culture

      Thread.CurrentThread.CurrentUICulture = Program.uiCulture;


      // parse factory init files

      wiretap.GetInitState( Livedata.Factory, BegmMisc.ShowError );
    }

    /// <summary>
    /// LoadFactoryData() completed handler.
    /// </summary>
    private void bgwInitFactory_RunWorkerCompleted( object sender, RunWorkerCompletedEventArgs e )
    {
      if( e.Error != null )  // exception occured in thread
      {
        if( !( e.Error is NetworkException ) )
          throw e.Error;

        Log.AddException( e.Error );

        MessageBox.Show( String.Format( "{0}\n{1}", e.Error.Message, e.Error.InnerException.Message ),
                         "Battleground Europe Game Monitor", MessageBoxButtons.OK, MessageBoxIcon.Error );

        expFactoryStatus.CanCollapse = true;
        expFactoryStatus.Collapsed = false;
        expFactoryStatus.Cursor = Cursors.Arrow;

        return;
      }


      // swap out load message

      wgtFactoryStatus.ShowLoadMessage( false );


      // popluate widget

      Log.AddEntry( "Initialising factory widget" );
      wgtFactoryStatus.InitWidget( this.game );


      // if always load checkbox is checked, set option & save

      if( wgtFactoryStatus.cbAlwaysLoad.Checked )
      {
        options.Startup.loadFactoryData = true;
        options.StartupTab = options.Startup;  // update form controls
        options.Startup.SaveToRegistry( false );
      }


      // animate back to expanded

      expFactoryStatus.CanCollapse = true;
      expFactoryStatus.Collapsed = false;
      expFactoryStatus.Cursor = Cursors.Arrow;


      // finished

      wgtRecentEvents.FactoryCheckboxEnabled = true;
      wgtRecentEvents.cbFactory.Checked = true;
      factoryDataLoaded = true;
      Log.AddEntry( "Factory data loaded successfully" );
    }



    /// <summary>
    /// The poll event loop is called every minute to perform regular tasks. If the user
    /// is idle or playing ww2online, the program is put into a sleep state. Otherwise
    /// metadata is checked and the appropriate xml files are polled for new events.
    /// </summary>
    private void EventLoop()
    {
      // call NotifyIcon's private UpdateIcon( bool showIconInTray ) method via reflection
      // workaround in case the user alt-f4's the tray context menu and removes the icon
#if !MAC
      trayIcon.GetType().InvokeMember( "UpdateIcon",
                                       BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.NonPublic,
                                       null, trayIcon, new object[] { true } );
#endif


      // if the pc has been suspended, wait for network to come back

      if( lastPollTime < DateTime.Now.AddMinutes( -5 ) )
      {
        Log.AddEntry( "Resume from standby, waiting for network" );
        WaitForNetwork();
      }


      // if the pc has been suspended > 2 hours, reload

      if( lastPollTime < DateTime.Now.AddHours( -2 ) )
      {
        Log.AddEntry( "Suspended for over 2 hours, reinitialising" );
        Reload();
        return;
      }


      // check if user is idle

      if( options.Startup.sleepWhenIdle )
      {
        if( BegmMisc.GetIdleTime().Hours >= options.Startup.idleTimeout )
        {
          Log.AddEntry( "User has been idle for {0} hours, going to sleep", options.Startup.idleTimeout );
          SleepBEGM( false );
          return;
        }
      }


      // check wether ww2 is running

      bool prevWw2Running = this.ww2Running;
      this.ww2Running = BegmMisc.WW2Running();


      // sleep if ww2 is running

      if( options.Startup.sleepWhenPlay )
      {
        if( this.delaySleepWhenPlay )
        {
          // if the game was running when we were launched, we don't want to go to
          // sleep straight away - wait until it has been closed

          if( !this.ww2Running )
            this.delaySleepWhenPlay = false;
        }
        else if( this.ww2Running )
        {
          Log.AddEntry( "User playing Battleground Europe, going to sleep" );
          SleepBEGM( true );
          return;
        }
      }


      // if returning from idle/fullscreen, show postponed alerts

      if( alertWindow.PostponedAlerts && !alertWindow.Postpone )
      {
        Log.AddEntry( "User returned, showing alerts" );
        alertWindow.ShowCurrentAlerts();
      }


      // reload map size if ww2 running state changed

      if( wgtGameMap.MapLoaded && options.Map.useDefaultWhenPlaying && options.Map.mapSize != 40 && this.ww2Running != prevWw2Running )
      {
        wgtGameMap.ReloadMapSize();
      }


      // update campaign info in header

#if !INTERNAL_BUILD
      if( DateTime.Now.Minute % 10 == 0 )  // only every 10 mins
        AddLogoCampaign();
#endif


      // check in background for any updated metadata or new events

      Poll( false );
    }

    /// <summary>
    /// Start the background process of checking for updated metadata, then downloading
    /// and parsing any poll xml files that are due to be checked. If new data is found,
    /// new game events are generated and the appropriate widgets updated.
    /// </summary>
    /// <param name="logIfNoEvents">True if we should log even if there are no new events.</param>
    private void Poll( bool logIfNoEvents )
    {
      if( CheckIfBusy() ) return;  // another operation is taking place (eg, loading factory data)

      game.Events.ResetNewEvents();
      bgwPoll.RunWorkerAsync( logIfNoEvents );
    }

    /// <summary>
    /// Poll() background thread.
    /// </summary>
    private void bgwPoll_DoWork( object sender, DoWorkEventArgs e )
    {
      // set thread ui culture

      Thread.CurrentThread.CurrentUICulture = Program.uiCulture;


      // check for new metadata

      if( wiretap.CheckUpdatedMetadata() )
      {
        Log.AddEntry( "Updated metadata found" );
        Invoke( new MethodInvoker( Reload ) );
        e.Cancel = true;
        return;
      }


      // check for new game events

      wiretap.GetPollData();
      lastPollTime = DateTime.Now;


      // update wallpaper

      if( options.Map.showWallpaper && wgtGameMap.WallpaperNeedsUpdate )
        wgtGameMap.UpdateWallpaper();


      // update frontlines

      game.UpdateFrontlines();


      // pass logIfNoEvents arg to completed handler

      e.Result = e.Argument;
    }

    /// <summary>
    /// Poll() completed handler.
    /// </summary>
    private void bgwPoll_RunWorkerCompleted( object sender, RunWorkerCompletedEventArgs e )
    {
      if( e.Cancelled ) return;  // new metadata found
      bool logIfNoEvents = (bool)e.Result;

      game.Events.Prune();


      // write result of poll to log

      StringBuilder logEntry = new StringBuilder();

      if( wiretap.NewCapturesCount > 0 )
        logEntry.Append( String.Format( "{0} capture(s), "        , wiretap.NewCapturesCount    ).Pluralise() );
      if( wiretap.NewAOsCount > 0 )
        logEntry.Append( String.Format( "{0} ao change(s), "      , wiretap.NewAOsCount         ).Pluralise() );
      if( wiretap.NewFirebaseCount > 0 )
        logEntry.Append( String.Format( "{0} firebase change(s), ", wiretap.NewFirebaseCount    ).Pluralise() );
      if( wiretap.NewHCMovesCount > 0 )
        logEntry.Append( String.Format( "{0} hcunit move(s), "    , wiretap.NewHCMovesCount     ).Pluralise() );
      if( wiretap.NewFactoryTickCount > 0 )
        logEntry.Append( String.Format( "{0} factory tick(s), "   , wiretap.NewFactoryTickCount ).Pluralise() );

      if( logEntry.Length > 0 )  // got data
      {
        logEntry.Length = logEntry.Length - 2;  // remove trailing ", "
        logEntry.Insert( 0, "Received " );

        Log.AddEntry( logEntry.ToString() );
      }
      else if( logIfNoEvents )
      {
        Log.AddEntry( "No new events found" );
      }


      // update widgets

      UpdateLaunchMenu();

      foreach( IWidget widget in this.allWidgets )
        widget.UpdateWidget();


      // show alerts for any new events

      if( options.Alerts.showAlerts )
        ShowAlerts();
    }



    /// <summary>
    /// Update the checked state of the "Disable all alerts" item in the tray context menu.
    /// </summary>
    private void UpdateContextMenu()
    {
      miTrayDisableAllAlerts.Checked = !options.Alerts.showAlerts;
    }

    /// <summary>
    /// Updates the game event lists in the RecentEvents and CurrentAttacks widgets with the
    /// current sort order and resets their scroll position.
    /// </summary>
    private void UpdateEventLists()
    {
      wgtRecentEvents.EventListSortOrder     =
        wgtCurrentAttacks.EventListSortOrder =
        wgtBrigadeStatus.EventListSortOrder  = options.Misc.eventSortOrder;
    }



    /// <summary>
    /// Checks to see if there is currently a background task running.
    /// </summary>
    /// <returns>True if a task is busy.</returns>
    private bool CheckIfBusy()
    {
      return bgwInit.IsBusy || bgwPoll.IsBusy || bgwInitFactory.IsBusy;
    }

    /// <summary>
    /// Checks whether a version check has been performed recently.
    /// </summary>
    /// <returns>True if another check is needed.</returns>
    private bool VersionCheckNeeded()
    {
      RegistryKey key = Registry.CurrentUser.OpenSubKey( "Software\\BEGameMonitor" );
      if( key == null ) return true;

      DateTime lastCheck;
      try
      {
        lastCheck = DateTime.Parse( (string)key.GetValue( "LastVersionCheck" ) );
      }
      catch  // key not found, not valid format, etc
      {
        return true;
      }
      finally
      {
        key.Close();
      }

      return lastCheck < DateTime.Now.AddDays( -7 );  // last checked over a week ago
    }

    /// <summary>
    /// Checks whether there is a newer version of BEGM available.
    /// </summary>
    /// <returns></returns>
    private int IsNewerVersion()
    {
      Log.AddEntry( "Checking for latest version..." );

      try
      {
        // get latest version from begm website

        string versionString = wiretap.Net.DownloadString( new Uri( "http://begm.sourceforge.net/versioncheck" ) );

        string[] lines = Regex.Split( versionString, @"[\r\n]+" );
        Version[] versions = new Version[lines.Length];

        for( int i = 0; i < lines.Length; i++ )
          if( !String.IsNullOrEmpty( lines[i] ) )
            versions[i] = new Version( lines[i] );

        Log.Okay();


        // remember last checked time

        RegistryKey key = Registry.CurrentUser.CreateSubKey( "Software\\BEGameMonitor" );
        if( key != null )
        {
          key.SetValue( "LastVersionCheck", DateTime.Now );
          key.Close();
        }


        // compare with current versions

        Version pluginVersion = wgtGameMap.HiresMapPluginVersion;

        if( versions.Length > 0 && versions[0] != null && versions[0] > Program.version )
        {
          Log.AddEntry( "Newer version available: v{0} => v{1}", Program.version, versions[0] );
          return 1;
        }
        if( versions.Length > 1 && versions[1] != null && pluginVersion != null && versions[1] > pluginVersion )
        {
          Log.AddEntry( "Newer hires map plugin available: v{0} => v{1}", pluginVersion, versions[1] );
          return 2;
        }

        return 0;
      }
      catch
      {
        Log.Error();
        return 0;
      }
    }



    /// <summary>
    /// Clears the in-memory data, and re-initialises the program.
    /// </summary>
    /// <remarks>Will not download metadata again unless wiretap.FlushCache() is called
    /// beforehand. Occurs when there is an error during init and the user updates their
    /// network settings, the user clicks "reset all game data" under options, or if new
    /// metadata is found during polling.</remarks>
    private void Reload()
    {
      tmrEventLoop.Stop();

      options.SaveWidgetPosition( this.columns, this.allExpandos );

      foreach( Expando expando in this.allExpandos )
      {
        expando.Collapsed = true;
        expando.CanCollapse = false;
      }

      Log.AddEntry( "Reloading game data" );
      lblInit.Text = Language.GameStatus_Reinitialising;

      ClearMemory();
      Init();
    }

    /// <summary>
    /// Delete all GameEvents, remove references to everything, and do a manual garbage collection.
    /// </summary>
    private void ClearMemory()
    {
      Log.AddEntry( "Clearing in-memory data structures" );

      alertWindow.Cleanup();

      foreach( IWidget widget in this.allWidgets )
        widget.Cleanup();

      game.Cleanup();

      GC.Collect();
    }

    /// <summary>
    /// Waits for up to 20 secs for the network to come back when the pc
    /// resumes from standby.
    /// </summary>
    private void WaitForNetwork()
    {
      for( int i = 0; i < 10; i++ )
      {
        if( this.wiretap.Net.TestConnection( this.options.Network ) == String.Empty )
          return;  // success
        else
          Thread.Sleep( 2000 );
      }
    }



    /// <summary>
    /// Transfers the program to it's low-resource "sleep" state (relaunches self with "sleep" argument).
    /// </summary>
    /// <param name="autowakeup">True if the program should relaunch after ww2online has exited.</param>
    private void SleepBEGM( bool autowakeup )
    {
      try
      {
        Process.Start( Application.ExecutablePath, autowakeup ? "sleep autowakeup" : "sleep" );
      }
      catch( Exception ex )
      {
        if( autowakeup )  // if doing autowakeup (eg, user is playing ww2), just exit silently on error
        {
          Log.AppendToLogFile( "Failed to sleep: {0}", ex.Message );
        }
        else
        {
          MessageBox.Show( Language.Error_Sleep + ":\n\n" + ex.Message, Language.Error_Error, MessageBoxButtons.OK, MessageBoxIcon.Error );
          return;
        }
      }

      ExitBEGM();
    }

    /// <summary>
    /// Exits and relaunches the program (eg, when language is changed).
    /// </summary>
    private void RestartBEGM()
    {
      try
      {
        Process.Start( Application.ExecutablePath );
      }
      catch( Exception ex )
      {
        MessageBox.Show( Language.Error_Restart + ":\n\n" + ex.Message, Language.Error_Error, MessageBoxButtons.OK, MessageBoxIcon.Error );
        return;
      }

      ExitBEGM();
    }

    /// <summary>
    /// Exits the program after saving settings and tidying up.
    /// </summary>
    private void ExitBEGM()
    {
      options.SaveWidgetPosition( this.columns, this.allExpandos );

      this.State.SaveToRegistry();
      wgtServerStatus.State.SaveToRegistry();  //// make state interface
      wgtRecentEvents.State.SaveToRegistry();
      wgtTownStatus.State.SaveToRegistry();
      wgtGameMap.State.SaveToRegistry();
      wgtFactoryStatus.State.SaveToRegistry();
      wgtBrigadeStatus.State.SaveToRegistry();

      wgtGameMap.ExitWallpaper();

      Application.Exit();
    }



    private bool firstShowWindow = true;

    /// <summary>
    /// Shows the Game Status window, resizing according to the current dock style and screen size.
    /// </summary>
    public void ShowWindow()
    {
      // make sure not minimised

      if( this.WindowState == FormWindowState.Minimized )
      {
        this.Visible = true;  // must be visible to update bounds when restoring
        this.WindowState = this.prevWindowState;
      }


      // update window docked style and size/position

      UpdateWindow();


      // if map is expanded, make sure it's loaded

      if( !expGameMap.Collapsed )
      {
        if( !wgtGameMap.LoadMap() )
          System.Media.SystemSounds.Hand.Play();
      }


      // make sure detatched widgets visible

      foreach( Expando expando in this.allExpandos )
        if( expando.Detatched )
          expando.FindForm().Visible = true;


      // display window

      this.Show();
      this.Activate();
      miTrayShowHide.Text = Language.GameStatus_Hide;
      firstShowWindow = false;
    }

    /// <summary>
    /// Update the window dock style, size and position.
    /// </summary>
    private void UpdateWindow()
    {
      if( options.Misc.dockWindow == DockStyle.None )
      {
        // convert to undocked window style and update size/position

        if( this.FormBorderStyle != FormBorderStyle.FixedToolWindow )
        {
          Rectangle desktop = Screen.PrimaryScreen.WorkingArea;


          // convert to undocked

          this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
          pnlResizeLeft.Visible = pnlResizeRight.Visible = picResizeCorner.Visible = true;
          this.Text = this.Text.Insert( 0, " " );


          // set window size

          if( firstShowWindow && !options.GameStatusState.windowSize.IsEmpty )
          {
            this.Size = options.GameStatusState.windowSize;
          }
          else  // default: 50% desktop height
          {
            this.Height = desktop.Height / 2;
          }


          // set window location

          if( firstShowWindow && !options.GameStatusState.windowLocation.IsEmpty )
          {
            this.Location = options.GameStatusState.windowLocation;
            desktop = Screen.GetWorkingArea( this.Bounds );
          }
          else  // default: centered on screen
          {
            int x = desktop.Left + ( desktop.Width / 2 ) - ( this.Width / 2 );
            int y = desktop.Top + ( desktop.Height / 2 ) - ( this.Height / 2 );
            this.Location = new Point( x, y );
          }


          // make sure window fits inside current desktop

          if( firstShowWindow )
          {
            Rectangle window = this.Bounds;

            if( window.Width > desktop.Width )
              window.Width = desktop.Width;
            if( window.Height > desktop.Height )
              window.Height = desktop.Height;

            if( window.X < desktop.X )
              window.X = desktop.X;
            if( window.X > desktop.Right - window.Width )
              window.X = desktop.Right - window.Width;
            if( window.Y < desktop.Y )
              window.Y = desktop.Y;
            if( window.Y > desktop.Bottom - window.Height )
              window.Y = desktop.Bottom - window.Height;

            this.Bounds = window;
          }


          // set minimum size

          Size decoration = this.Size - this.ClientSize;  // setting height above forces correct decoration size here
          this.MinimumSize = new Size( COLUMN_WIDTH + COLUMN_GUTTER + decoration.Width, 129 + decoration.Height );
        }
      }
      else  // dockWindow Left or Right
      {
        // get chosen desktop bounds

        Rectangle desktop = Screen.AllScreens[options.Misc.GameStatusDisplayIndex].WorkingArea;


        // convert to docked window style

        if( this.FormBorderStyle != FormBorderStyle.None )
        {
          this.MinimumSize = new Size( COLUMN_WIDTH + COLUMN_GUTTER, 0 );
          this.FormBorderStyle = FormBorderStyle.None;
          picResizeCorner.Visible = false;
          this.Text = this.Text.TrimStart( ' ' );
        }


        // set window size

        int width = this.Width;
        if( firstShowWindow )  // use saved width
        {
          if( options.GameStatusState.windowSize.Width != 0 )
            width = options.GameStatusState.windowSize.Width;
          else // default: minimum
            width = this.MinimumSize.Width;
        }
        if( width > desktop.Width )
          width = desktop.Width;
        this.Size = new Size( width, desktop.Height );


        // set window location

        if( options.Misc.dockWindow == DockStyle.Left )
        {
          this.Location = new Point( desktop.Left, desktop.Y );
          pnlResizeLeft.Visible = false;
          pnlResizeRight.Visible = true;
        }
        else
        {
          this.Location = new Point( desktop.Right - this.Width, desktop.Y );
          pnlResizeRight.Visible = false;
          pnlResizeLeft.Visible = true;
        }
      }
    }

    /// <summary>
    /// Hides the Game Status window.
    /// </summary>
    /// <param name="incDetatched">If true will also hide any detatched widgets.</param>
    private void HideWindow( bool incDetatched )
    {
      miTrayShowHide.Text = Language.GameStatus_Show;
      this.Visible = false;

      if( incDetatched )
        foreach( Expando expando in this.allExpandos )
          if( expando.Detatched )
            expando.FindForm().Visible = false;

      if( !wgtGameMap.Visible )
        wgtGameMap.QueueUnloadMap();
    }

    /// <summary>
    /// Closes the Game Status window (exits BEGM completely if holding shift).
    /// </summary>
    private void CloseWindow()
    {
#if DEBUG
      ExitBEGM();
#else
      if( Control.ModifierKeys == Keys.Shift )
        ExitBEGM();
      else
        HideWindow( false );
#endif
    }



    /// <summary>
    /// Wrapper for the various widgets Reveal() methods. Also makes sure the widget is visible.
    /// </summary>
    /// <param name="widgetType">The widget type to reveal.</param>
    /// <param name="arg">The argument to the widgets Reveal() method (must be correct type).</param>
    private void RevealWidget( WidgetType widgetType, object arg )
    {
      // if arg is an int, convert cpid => cp

      if( arg is int )
        arg = game.ChokePoints[(int)arg];


      // get appropriate widget & expando

      IWidget widget = this.allWidgets[(int)widgetType];
      Expando expando = this.allExpandos[(int)widgetType];


      // make sure visible and at front

      if( expando.Detatched )
      {
        expando.FindForm().Visible = true;
        expando.FindForm().Activate();
      }
      else
      {
        ShowWindow();
      }


      // call appropriate Reveal() method

      widget.Reveal( arg );
    }

    /// <summary>
    /// Shows or activates the about dialog, making sure its contents are up-to-date.
    /// </summary>
    private void ShowAboutWindow()
    {
      if( aboutDialog.Visible )
      {
        aboutDialog.Activate();
      }
      else
      {
        aboutDialog.UpdateKBytes( wiretap.Net.BytesDownloaded / 1024 );
        aboutDialog.Show();
      }
    }

    /// <summary>
    /// Shows or activates the options dialog.
    /// </summary>
    private void ShowOptionsWindow()
    {
      if( options.Visible )
        options.Activate();
      else
        options.ShowDialog();
    }

    /// <summary>
    /// Display the latest round of alerts based on the users current alert filters.
    /// </summary>
    private void ShowAlerts()
    {
      // get filtered list

      List<GameEvent> alertEvents = new List<GameEvent>();

      foreach( GameEvent gameEvent in game.Events.New )
      {
        // always show alerts

        if( options.Alerts.alwaysAlertUnderAttack )
        {
          if( gameEvent is ChokePointUnderAttackGameEvent && ( (ChokePointUnderAttackGameEvent)gameEvent ).NewAttack )
          {
            alertEvents.Add( gameEvent );
            continue;
          }
        }

        if( options.Alerts.alwaysAlertChokePointCaptured )
        {
          if( gameEvent is ChokePointCapturedGameEvent )
          {
            alertEvents.Add( gameEvent );
            continue;
          }
        }


        // filter by event type

        if( options.Alerts.filterEventType )
        {
          if( gameEvent is FacilityCapturedGameEvent )
          {
            if( ( (FacilityCapturedGameEvent)gameEvent ).Recaptured )
            {
              if( !options.Alerts.filterCapturesFacilityRecaptured ) continue;
            }
            else
            {
              if( !options.Alerts.filterCapturesFacilityCaptured ) continue;
            }
          }
          else if( gameEvent is SpawnableDepotCapturedGameEvent )
          {
            if( ( (SpawnableDepotCapturedGameEvent)gameEvent ).Recaptured )
            {
              if( !options.Alerts.filterCapturesFacilitySpawnableRecaptured ) continue;
            }
            else
            {
              if( !options.Alerts.filterCapturesFacilitySpawnableCaptured ) continue;
            }
          }
          else if( gameEvent is ChokePointCapturedGameEvent && !options.Alerts.filterCapturesChokePointCaptured ) continue;
          else if( gameEvent is ChokePointUnderAttackGameEvent && !options.Alerts.filterCapturesChokePointContestedUnderAttack ) continue;
          else if( gameEvent is ChokePointRegainedGameEvent && !options.Alerts.filterCapturesChokePointContestedRegained ) continue;
          else if( gameEvent is ChokePointControllerChangedGameEvent && !options.Alerts.filterCapturesChokePointControlChanged ) continue;
          else if( gameEvent is AttackObjectiveChangedGameEvent )
          {
            if( ( (AttackObjectiveChangedGameEvent)gameEvent ).AOPlaced )
            {
              if( !options.Alerts.filterAttackObjectivePlaced ) continue;
            }
            else
            {
              if( !options.Alerts.filterAttackObjectiveWithdrawn ) continue;
            }
          }
          else if( gameEvent is FirebaseBlownGameEvent )
          {
            if( gameEvent.Country.Side == Side.Allied )
            {
              if( !options.Alerts.filterFirebaseAlliedBlown ) continue;
            }
            else if( gameEvent.Country.Side == Side.Axis )
            {
              if( !options.Alerts.filterFirebaseAxisBlown ) continue;
            }
            else
            {
              continue;
            }
          }
          else if( gameEvent is NewBrigadeFirebaseGameEvent )
          {
            if( !options.Alerts.filterFirebaseNewBrigade ) continue;
          }
          else if( gameEvent is HCUnitMovedGameEvent && !options.Alerts.filterHCUnitMoved ) continue;
          else if( gameEvent is HCUnitDeployedGameEvent && !options.Alerts.filterHCUnitDeployed ) continue;
          else if( gameEvent is HCUnitRetreatGameEvent && !options.Alerts.filterHCUnitRetreated ) continue;
          else if( gameEvent is HCUnitRoutedGameEvent && !options.Alerts.filterHCUnitRouted ) continue;
          else if( gameEvent is FactoryDamagedGameEvent && !options.Alerts.filterFactoryHealthDamaged ) continue;
          else if( gameEvent is FactoryHealthyGameEvent && !options.Alerts.filterFactoryHealthRepaired ) continue;
          else if( gameEvent is FactoryDestroyedGameEvent && !options.Alerts.filterFactoryHealthDestroyed ) continue;
          else if( gameEvent is FactoryRDPChangedGameEvent )
          {
            if( ( (FactoryRDPChangedGameEvent)gameEvent ).Resumed )
            {
              if( !options.Alerts.filterFactoryRdpResumed ) continue;
            }
            else
            {
              if( !options.Alerts.filterFactoryRdpHalted ) continue;
            }
          }
        }


        // filter by chokepoint

        if( options.Alerts.filterChokePoint )
        {
          bool found = false;

          foreach( ChokePoint cp in gameEvent.ChokePoints )
          {
            if( options.Alerts.filterChokePointIDLookup.ContainsKey( cp.ID ) )
              found = true;
          }

          if( !found )
            continue;
        }


        // filter by country

        if( options.Alerts.filterCountry )
        {
          if( !options.Alerts.filterCountryIDs.Contains( gameEvent.Country.ID ) )
            continue;
        }


        // event passes all filters

        alertEvents.Add( gameEvent );


      }  // end newevent loop


      // show alerts

      if( alertEvents.Count > 0 )
        alertWindow.ShowAlerts( alertEvents );
    }



    /// <summary>
    /// Display the Launch Game link and menu with the appropriate options depending
    /// on what's installed.
    /// </summary>
    private void SetupLaunchMenu()
    {
      string path = "", exec = "";
      RegistryKey key;


      // look for playgate path

      key = Registry.LocalMachine.OpenSubKey( "Software\\Playnet\\apps\\appid1\\instance1" );

      if( key == null )  // if running on 64-bit windows (playgate is 32-bit)
        key = Registry.LocalMachine.OpenSubKey( "Software\\Wow6432Node\\Playnet\\apps\\appid1\\instance1" );

      if( key != null )
      {
        path = (string)key.GetValue( "path", "" );
        exec = (string)key.GetValue( "exec", "" );
        key.Close();
      }
#if !DEBUG
      else
        return;
#endif


      // create PSI objects

      this.procPlaygate = new ProcessStartInfo( Path.Combine( path, "Playgate.exe" ) );
      this.procPlaygate.WorkingDirectory = path;

      this.procWW2Live = new ProcessStartInfo( Path.Combine( path, exec ) );
      this.procWW2Live.WorkingDirectory = path;


      // check files exist

#if !DEBUG
      if( !File.Exists( this.procPlaygate.FileName ) )
        return;
      if( !File.Exists( this.procWW2Live.FileName ) )
        return;
#endif


      // configure menu options

      lnkLaunchGame.Visible = btnLaunchGame.Visible = true;

      if( true )  //// disable beta links for now
      {
        cmsLaunchGame.Items.Remove( miLaunchGameOnlineBeta );
        cmsLaunchGame.Items.Remove( miLaunchGameOfflineBeta );

        cmsLaunchGame.Height -= cmsLaunchGame.Items[0].Height * 2;
      }
    }

    /// <summary>
    /// Update the Launch Game menu status images and population tooltips.
    /// </summary>
    private void UpdateLaunchMenu()
    {
      foreach( Server server in game.Servers.Values )
      {
        // get menu item

        ToolStripMenuItem menuItem;

        switch( server.ID )
        {
          case 1:   menuItem = miLaunchGameOnlineLive;     break;
          case 22:  menuItem = miLaunchGameOnlineTraining; break;
          case 222: menuItem = miLaunchGameOnlineBeta;     break;
          default:  continue;
        }


        // update image

        if( server.Online )
          menuItem.Image = Resources.server_online;
        else if( server.Locked )
          menuItem.Image = Resources.server_locked;
        else if( server.Offline )
          menuItem.Image = Resources.server_offline;
        else
          menuItem.Image = Resources.server_other;


        // update tooltip

        string serverState = server.State;
        if( server.StateInfo != null )
          serverState += String.Format( " ({0})", server.StateInfo );

        menuItem.ToolTipText = String.Format( Language.GameStatus_Tooltip_LaunchGame,
                                              server.Name,
                                              serverState,
                                              Misc.EnumString( server.Population ).ToLower() );
      }

    }

    /// <summary>
    /// Add the campaign/intermission subheading to the BEGM logo.
    /// </summary>
    private void AddLogoCampaign()
    {
      // get text to display

      string campaignText;

      if( !game.ServerParams.ContainsKey( "arena.intermission" ) || !game.ServerParams.ContainsKey( "arena.campaign_id" ) ) return;
      if( game.ServerParams["arena.intermission"].Value == "0" )  // campaign
      {
        if( !game.ServerParams.ContainsKey( "arena.campaign_start" ) ) return;

        DateTime campaignStart = Misc.ParseTimestamp( game.ServerParams["arena.campaign_start"].Value );
        int dayNumber = (int)( DateTime.Now - campaignStart ).TotalDays;  // crs count days from 0?!
        //int dayNumber = (int)Math.Ceiling( ( DateTime.Now - campaignStart ).TotalDays );

        campaignText = String.Format( Language.GameStatus_HeaderCampaign, game.ServerParams["arena.campaign_id"].Value, dayNumber );
      }
      else  // intermission
      {
        campaignText = String.Format( Language.GameStatus_HeaderIntermission, game.ServerParams["arena.campaign_id"].Value );
      }


      // double space, uppercase

      StringBuilder sb = new StringBuilder();
      foreach( char c in campaignText.ToUpper() )
      {
        if( sb.Length > 0 )
          sb.Append( ' ' );
        sb.Append( c );
      }
      campaignText = sb.ToString();


      // draw text on logo

      Bitmap image = Resources.begm_logo;
      Graphics g = Graphics.FromImage( image );
      g.TextRenderingHint = TextRenderingHint.AntiAlias;
      Brush brushBack = new SolidBrush( Color.FromArgb( 20, 20, 20 ) );
      Font fontMain = new Font( "Arial Black", 6F, FontStyle.Bold );

      int textWidth = (int)g.MeasureString( campaignText, fontMain ).Width;
      int x = ( image.Width - textWidth ) / 2;
      int y = 77;

      g.FillRectangle( brushBack, x - 1, y + 1, textWidth + 1, 10 );
      g.DrawString( campaignText, fontMain, Brushes.White, x, y );

      g.Dispose();
      picBELogo.Image = image;
    }

#if INTERNAL_BUILD
    /// <summary>
    /// Add a watermark to the BEGM logo for internal test builds.
    /// </summary>
    private void AddLogoWatermark()
    {
      Bitmap image = Resources.begm_logo;
      Graphics g = Graphics.FromImage( image );

      g.DrawString( "INTERNAL TEST BUILD ONLY",
                    new Font( SystemFonts.DefaultFont.FontFamily, 14.5F, FontStyle.Bold ),
                    Brushes.Red, 0, 68 );

      g.Dispose();
      picBELogo.Image = image;
    }
#endif

    #endregion

    #region Event Handlers

    #region General

    // minimise to tray instead of close
    private void GameStatus_FormClosing( object sender, FormClosingEventArgs e )
    {
      // note: this will not be called on exit if the GameStatus window has never been shown
      //       (ie, started minimised, closed via tray icon)

      if( e.CloseReason == CloseReason.UserClosing )  // user clicked close button or alt+f4'd
      {
        CloseWindow();
        e.Cancel = true;
      }
    }

    // update about dialog if visible
    private void NetManager_OnBytesDownloadedChanged( object sender, EventArgs e )
    {
      if( aboutDialog.Visible )
        aboutDialog.UpdateKBytes( wiretap.Net.BytesDownloaded / 1024 );
    }

    // update status bar
    private void Log_OnNewLogEntry( string entry, bool error )
    {
      if( entry == "ERROR" || entry == "OK" )
      {
        lblStatus.Text = null;
        tmrStatusBar.Stop();
        return;
      }

      lblStatus.Text = entry;
      lblStatus.ForeColor = error ? Color.FromArgb( 255, 50, 50 ) : Color.FromArgb( 224, 224, 224 );

      tmrStatusBar.Start();  // fade out
    }

    // event loop timer
    private void tmrEventLoop_Tick( object sender, EventArgs e )
    {
      EventLoop();
    }

    #endregion

    #region Tray Icon

    // show/hide main window on left click
    private void trayIcon_MouseClick( object sender, MouseEventArgs e )
    {
      if( e.Button != MouseButtons.Left ) return;

      if( Control.ModifierKeys == Keys.Shift )
        HideWindow( true );
      else if( this.Visible && this.WindowState != FormWindowState.Minimized )
        HideWindow( false );
      else
        ShowWindow();
    }

    // context menu items
    private void miTrayShowHide_Click( object sender, EventArgs e )
    {
      if( this.Visible )
        HideWindow( Control.ModifierKeys == Keys.Shift );
      else
        ShowWindow();
    }
    private void miTrayShowLastAlerts_Click( object sender, EventArgs e )
    {
      alertWindow.ShowCurrentAlerts();
    }
    private void miTrayAbout_Click( object sender, EventArgs e )
    {
      ShowAboutWindow();
    }
    private void miTrayOptions_Click( object sender, EventArgs e )
    {
      if( bgwInit.IsBusy ) return;

      ShowOptionsWindow();
    }
    private void miTrayDisableAllAlerts_Click( object sender, EventArgs e )
    {
      options.Alerts.showAlerts = !miTrayDisableAllAlerts.Checked;
      options.AlertsTab = options.Alerts;  // update form controls
      options.Alerts.SaveToRegistry( false );
    }
    private void miTraySleep_Click( object sender, EventArgs e )
    {
      Log.AppendToLogFile( "User requested sleep" );
      SleepBEGM( false );
    }
    private void miTrayExit_Click( object sender, EventArgs e )
    {
      ExitBEGM();
    }

    #endregion

    #region Game Window

    // resize window
    private void pnlResize_MouseDown( object sender, MouseEventArgs e )
    {
      // start resizing

      this.resizeWindowsStartSize = this.Size;
      this.resizeWindowStartMouse = Control.MousePosition;
      ( (Control)sender ).MouseMove += pnlResize_MouseMove;
    }
    private void pnlResize_MouseMove( object sender, MouseEventArgs e )
    {
      bool anchorLeft     = ( ( (Control)sender ).Anchor & AnchorStyles.Left ) == AnchorStyles.Left;  // dragging a control anchored to left edge (ie, pnlResizeLeft)
      bool resizeVertical = ( ( (Control)sender ).Anchor & AnchorStyles.Top  ) != AnchorStyles.Top;   // dragging a control NOT anchored to top (ie, picResize)

      Size decoration = this.Size - this.ClientSize;
      Point newMouse = Control.MousePosition;
      Size newSize = this.Size;


      // calculate new window width

      if( anchorLeft )
        newSize.Width = resizeWindowsStartSize.Width - ( newMouse.X - resizeWindowStartMouse.X );
      else
        newSize.Width = resizeWindowsStartSize.Width + ( newMouse.X - resizeWindowStartMouse.X );


      // calculate new window height

      if( resizeVertical )
      {
        newSize.Height = resizeWindowsStartSize.Height + ( newMouse.Y - resizeWindowStartMouse.Y );
      }


      // snap width to column boundary

      if( Control.ModifierKeys != Keys.Shift )
      {
        // calc column boundaries each side of current position

        int newColumns = ( newSize.Width - COLUMN_GUTTER ) / COLUMN_WIDTH;
        int colWidth1 = ( newColumns * COLUMN_WIDTH ) + COLUMN_GUTTER + decoration.Width;
        int colWidth2 = colWidth1 + COLUMN_WIDTH;


        // get distance from current width

        int offset1 = Math.Abs( newSize.Width - colWidth1 );
        int offset2 = Math.Abs( newSize.Width - colWidth2 );


        // snap to if nearby

        if( offset1 <= SNAP_COLUMNS )
          newSize.Width = colWidth1;
        else if( offset2 <= SNAP_COLUMNS )
          newSize.Width = colWidth2;
      }


      // maximum size

      if( options.Misc.dockWindow != DockStyle.None )
      {
        Rectangle desktop = Screen.AllScreens[options.Misc.GameStatusDisplayIndex].WorkingArea;

        if( newSize.Width > desktop.Width )
          newSize.Width = desktop.Width;
      }


      // minimum size

      if( newSize.Width < this.MinimumSize.Width )
        newSize.Width = this.MinimumSize.Width;

      if( newSize.Height < this.MinimumSize.Height )
        newSize.Height = this.MinimumSize.Height;

      if( newSize == this.Size ) return;


      // set new size

      int x = this.Left;
      if( anchorLeft )
        x += this.Width - newSize.Width;  // maintain left edge position

      // use Bounds to update both location and size together
      this.Bounds = new Rectangle( x, this.Bounds.Top, newSize.Width, newSize.Height );
      this.Update();
    }
    private void pnlResize_MouseUp( object sender, MouseEventArgs e )
    {
      // stop resizing

      ( (Control)sender ).MouseMove -= pnlResize_MouseMove;
    }

    // adjust columns based on new window size
    private void GameStatus_Resize( object sender, EventArgs e )
    {
      if( this.WindowState == FormWindowState.Minimized )
        return;
      else
        this.prevWindowState = this.WindowState;


      // calc new number of columns

      int newColumns = ( this.Width - COLUMN_GUTTER ) / COLUMN_WIDTH;


      // add columns as necessary

      while( newColumns > this.tblLayout.ColumnCount )
      {
        // add column to table

        tblLayout.ColumnCount++;


        // add columnstyle with new width

        float width = 100F / ( newColumns - 1 );
        tblLayout.ColumnStyles.Add( new ColumnStyle( SizeType.Percent, width ) );


        // update other columns with new width

        for( int i = 1; i < tblLayout.ColumnCount - 1; i++ )
          tblLayout.ColumnStyles[i].Width = width;


        // create new Panel control, add to table cell

        GameStatusColumn col = new GameStatusColumn( tblLayout.ColumnCount,
                                                     new Size( COLUMN_WIDTH, tblLayout.Height ),
                                                     tskMain );
        col.AddToTable( this.tblLayout );
        this.columns.Add( col );
      }


      // remove columns as necessary

      while( newColumns < this.columns.Count )
      {
        // get last column controls

        int lastIndex = this.columns.Count - 1;
        GameStatusColumn lastColumn = this.columns[lastIndex];
        GameStatusColumn secondLastColumn = this.columns[lastIndex - 1];


        // move any expandos to second-last-column

        lastColumn.SuspendLayout();
        secondLastColumn.SuspendLayout();
        while( lastColumn.Expandos.Count > 0 )
        {
          Expando temp = lastColumn.Expandos[0];
          lastColumn.Expandos.Remove( temp );
          secondLastColumn.Expandos.Add( temp );
        }
        secondLastColumn.ResumeLayout();
        lastColumn.ResumeLayout();


        // remove and dispose controls

        lastColumn.RemoveFromTable( tblLayout );
        lastColumn.Dispose();
        this.columns.Remove( lastColumn );

        secondLastColumn.Refresh();

        tblLayout.ColumnCount--;
        tblLayout.ColumnStyles.RemoveAt( lastIndex );
      }
    }

    // menu links
    private void lnkOptions_LinkClicked( object sender, LinkLabelLinkClickedEventArgs e )
    {
      if( bgwInit.IsBusy ) return;

      ShowOptionsWindow();
    }
    private void lnkAbout_LinkClicked( object sender, LinkLabelLinkClickedEventArgs e )
    {
      ShowAboutWindow();
    }
    private void lnkHelp_LinkClicked( object sender, LinkLabelLinkClickedEventArgs e )
    {
      Help.ShowHelp( this, this.helpFile, "help/index.html" );
    }
    private void lnkVersion_LinkClicked( object sender, LinkLabelLinkClickedEventArgs e )
    {
      try
      {
        Process.Start( options.Lang.Homepage + "#Download" );
      }
      catch( Exception ex )
      {
        MessageBox.Show( ex.Message, Language.Error_Error, MessageBoxButtons.OK, MessageBoxIcon.Error );
      }
    }

    // launch game link/menu
    private void btnLaunchGame_Click( object sender, EventArgs e )
    {
      picBELogo.Focus();  // unfocus button
      cmsLaunchGame.Show( btnLaunchGame, new Point( btnLaunchGame.Width - cmsLaunchGame.Width - 1, btnLaunchGame.Height ) );
    }
    private void lnkLaunchGame_LinkClicked( object sender, LinkLabelLinkClickedEventArgs e )
    {
      try
      {
        this.procPlaygate.Arguments = "1";
        Process.Start( this.procPlaygate );
      }
      catch( Exception ex )
      {
        MessageBox.Show( ex.Message, Language.Error_Error, MessageBoxButtons.OK, MessageBoxIcon.Error );
      }
    }
    private void miLaunchGameOnlineLive_Click( object sender, EventArgs e )
    {
      try
      {
        this.procPlaygate.Arguments = "1";
        Process.Start( this.procPlaygate );
      }
      catch( Exception ex )
      {
        MessageBox.Show( ex.Message, Language.Error_Error, MessageBoxButtons.OK, MessageBoxIcon.Error );
      }
    }
    private void miLaunchGameOnlineTraining_Click( object sender, EventArgs e )
    {
      try
      {
        this.procPlaygate.Arguments = "22";
        Process.Start( this.procPlaygate );
      }
      catch( Exception ex )
      {
        MessageBox.Show( ex.Message, Language.Error_Error, MessageBoxButtons.OK, MessageBoxIcon.Error );
      }
    }
    private void miLaunchGameOnlineBeta_Click( object sender, EventArgs e )
    {
      try
      {
        this.procPlaygate.Arguments = "222";
        Process.Start( this.procPlaygate );
      }
      catch( Exception ex )
      {
        MessageBox.Show( ex.Message, Language.Error_Error, MessageBoxButtons.OK, MessageBoxIcon.Error );
      }
    }
    private void miLaunchGameOfflineLive_Click( object sender, EventArgs e )
    {
      try
      {
        Process.Start( this.procWW2Live );
      }
      catch( Exception ex )
      {
        MessageBox.Show( ex.Message, Language.Error_Error, MessageBoxButtons.OK, MessageBoxIcon.Error );
      }
    }
    private void miLaunchGameOfflineBeta_Click( object sender, EventArgs e )
    {
      try
      {
        Process.Start( this.procWW2Live );  // same exe as live now
      }
      catch( Exception ex )
      {
        MessageBox.Show( ex.Message, Language.Error_Error, MessageBoxButtons.OK, MessageBoxIcon.Error );
      }
    }

    // close button
    private void picClose_MouseEnter( object sender, EventArgs e )
    {
      picClose.Image = Resources.close_hover;
    }
    private void picClose_MouseLeave( object sender, EventArgs e )
    {
      picClose.Image = Resources.close;
    }
    private void picClose_MouseDown( object sender, MouseEventArgs e )
    {
      if( e.Button == MouseButtons.Left )
        picClose.Image = Resources.close_active;
    }
    private void picClose_MouseUp( object sender, MouseEventArgs e )
    {
      if( e.Button == MouseButtons.Left )
        picClose.Image = Resources.close;
    }
    private void picClose_MouseClick( object sender, MouseEventArgs e )
    {
      if( e.Button == MouseButtons.Left )
        CloseWindow();
    }

    // status bar
    private void tmrStatusBar_Tick( object sender, EventArgs e )
    {
      // fade out status bar messages

      int r = lblStatus.ForeColor.R;
      int g = lblStatus.ForeColor.G;
      int b = lblStatus.ForeColor.B;

      if( r + g + b == 0 )
      {
        tmrStatusBar.Stop();
        lblStatus.Text = null;
      }
      else
      {
        if( r > 0 ) r--;
        if( g > 0 ) g--;
        if( b > 0 ) b--;
        lblStatus.ForeColor = Color.FromArgb( r, g, b );
      }
    }
    private void lblStatus_MouseHover( object sender, EventArgs e )
    {
      lblStatusMsg.Visible = true;
    }
    private void lblStatusMsg_MouseLeave( object sender, EventArgs e )
    {
      lblStatusMsg.Visible = false;
    }
    private void lblStatusMsg_MouseDoubleClick( object sender, MouseEventArgs e )
    {
      aboutDialog.tabControl.SelectedIndex = 1;  // select log tab
      // doesn't raise the SelectedIndexChanged event?
      aboutDialog.Text = Language.About_LogTabTitle;  // workaround

      ShowAboutWindow();
    }

    // dynamic focus
    private void GameStatus_MouseEnter( object sender, EventArgs e )
    {
      BegmMisc.FocusTaskPane( tskMain );
    }
    private void picBELogo_MouseEnter( object sender, EventArgs e )
    {
      BegmMisc.FocusTaskPane( tskMain );
    }

    #endregion

    #region Widgets

    // update town status on expand
    private void expTownStatus_StateChanging( object sender, XPExplorerBar.ExpandoEventArgs e )
    {
      if( !e.Collapsed )
        wgtTownStatus.UpdateWidget();
    }

    // load/unload map when game map widget expanded/collapsed
    private void expGameMap_StateChanging( object sender, XPExplorerBar.ExpandoEventArgs e )
    {
      if( !expGameMap.Visible ) return;  // don't load on restore widget state while minimised

      if( e.Collapsed )  // about to expand
      {
        // make sure map is loaded
        if( !wgtGameMap.LoadMap() )
        {
          e.Cancel = true;
          System.Media.SystemSounds.Hand.Play();
        }
      }
      else  // about to collapse
      {
        wgtGameMap.CloseMapOptions();
      }
    }
    private void expGameMap_StateChanged( object sender, XPExplorerBar.ExpandoEventArgs e )
    {
      if( e.Collapsed )  // finished collapsing
        wgtGameMap.QueueUnloadMap();
    }

    #endregion

    #region Options

    // updates tab buttons
    private void options_CheckEventsClicked( object sender, EventArgs e )
    {
      Log.AddEntry( "Manually checking for new events" );
      wiretap.ResetPollTimes();
      Poll( true );
    }
    private void options_ResetAllDataClicked( object sender, EventArgs e )
    {
      wiretap.FlushCache();
      Reload();
    }

    // misc options changed
    private void options_DockWindowChanged( object sender, EventArgs e )
    {
      UpdateWindow();
    }
    private void options_EventSortOrderChanged( object sender, EventArgs e )
    {
      UpdateEventLists();
    }
    private void options_GameStatusDisplayChanged( object sender, EventArgs e )
    {
      UpdateWindow();
    }
    private void options_MapOptionsChanged( object sender, EventArgs e )
    {
      wgtGameMap.ReloadMapSize();
    }
    private void options_MapWallpaperChanged( object sender, EventArgs e )
    {
      wgtGameMap.UpdateWallpaper( options.Map.OverlayParams );
    }
    private void options_LanguageChanged( object sender, EventArgs e )
    {
      if( MessageBox.Show( Language.Options_NewLanguage, Language.Error_Question, MessageBoxButtons.YesNo, MessageBoxIcon.Question ) == DialogResult.Yes )
        RestartBEGM();
    }

    #endregion

    #endregion
  }


  #region Interfaces

  /// <summary>
  /// An interface for Game Status widgets.
  /// </summary>
  public interface IWidget
  {
    /// <summary>
    /// Initialise the widget.
    /// </summary>
    /// <param name="game">The current game state.</param>
    void InitWidget( GameState game );

    /// <summary>
    /// Update the widget with the current game state.
    /// </summary>
    void UpdateWidget();

    /// <summary>
    /// Remove references to local game objects.
    /// </summary>
    void Cleanup();

    /// <summary>
    /// Display the widget with the given piece of information.
    /// </summary>
    /// <param name="arg">The argument to display (eg, a ChokePoint, HCUnit, etc, depending on widget).</param>
    void Reveal( object arg );

    /// <summary>
    /// The GameStatus's RevealWidget() method.
    /// </summary>
    RevealDelegate GameStatus_RevealWidget
    {
      get;
      set;
    }

    /// <summary>
    /// Called when the Widget's parent Expando has been detached or re-attached.
    /// </summary>
    event EventHandler DetatchedChanged;

    void OnDetatchedChanged();

  }

  #endregion

  #region Classes

  /// <summary>
  /// A column in the GameStats window, made up of a TaskPane inside a Panel.
  /// </summary>
  public class GameStatusColumn
  {
    #region Variables

    private Panel panel;
    private TaskPane taskpane;
    
    #endregion

    #region Constructors

    /// <summary>
    /// Create a new GameStatusColumn with an existing column (ie, the first column).
    /// </summary>
    /// <param name="existing">The first Panel that contains tskMain.</param>
    public GameStatusColumn( Panel existing )
    {
      this.panel = existing;
      this.taskpane = (TaskPane)this.panel.Controls[0];
      this.taskpane.MouseEnter += GameStatusColumn_MouseEnter;
    }

    /// <summary>
    /// Create a new GameStatusColumn from scratch (ie, when adding a column).
    /// </summary>
    /// <param name="num">The new column index.</param>
    /// <param name="size">The size of the new column.</param>
    /// <param name="refTaskPane">A reference TaskPane to copy settings from.</param>
    public GameStatusColumn( int num, Size size, TaskPane refTaskPane )
    {
      this.panel = new Panel
                     {
                       Name = "pnlColumn" + num,
                       Anchor = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom,
                       Size = size,
                       Margin = new System.Windows.Forms.Padding( 0 ),
                     };

      this.taskpane = new TaskPane
                        {
                          Name = "tskColumn" + num,
                          Location = new Point( -12, 0 ),
                          Size = new Size( refTaskPane.Width, this.panel.Height ),
                          Anchor = refTaskPane.Anchor,
                          AllowExpandoDragging = refTaskPane.AllowExpandoDragging,
                          AutoScroll = refTaskPane.AutoScroll,
                          PreventAutoScroll = refTaskPane.PreventAutoScroll,
                          CustomSettings = refTaskPane.CustomSettings,
                        };

      this.panel.Controls.Add( this.taskpane );
      this.taskpane.MouseEnter += GameStatusColumn_MouseEnter;
    }

    #endregion

    #region Properties

    /// <summary>
    /// The Expandos contained in this column.
    /// </summary>
    public TaskPane.ExpandoCollection Expandos
    {
      get { return this.taskpane.Expandos; }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Add this column to a table.
    /// </summary>
    /// <param name="table">The TableLayoutPanel to use.</param>
    public void AddToTable( TableLayoutPanel table )
    {
      table.SetRowSpan( this.panel, 3 );
      table.Controls.Add( this.panel );
    }

    /// <summary>
    /// Remove this column from a table.
    /// </summary>
    /// <param name="table">The TableLayoutPanel to use.</param>
    public void RemoveFromTable( TableLayoutPanel table )
    {
      table.Controls.Remove( this.panel );
    }

    /// <summary>
    /// Forces the control to invalidate its client area and immediately redraw itself and any child controls.
    /// </summary>
    public void Refresh()
    {
      this.panel.Refresh();
    }

    /// <summary>
    /// Temporarily suspends the layout logic for the control.
    /// </summary>
    public void SuspendLayout()
    {
      this.taskpane.SuspendLayout();
    }

    /// <summary>
    /// Resumes usual layout logic.
    /// </summary>
    public void ResumeLayout()
    {
      this.taskpane.ResumeLayout();
    }

    /// <summary>
    /// Releases all resources used by the System.ComponentModel.Component.
    /// </summary>
    public void Dispose()
    {
      this.panel.Controls.RemoveAt( 0 );
      this.taskpane.Dispose();
      this.panel.Dispose();
    }

    #endregion

    #region Event Handlers

    // focus the TaskPane when mouse is over
    private void GameStatusColumn_MouseEnter( object sender, EventArgs e )
    {
      BegmMisc.FocusTaskPane( this.taskpane );
    }

    #endregion
  }

  #endregion
}
