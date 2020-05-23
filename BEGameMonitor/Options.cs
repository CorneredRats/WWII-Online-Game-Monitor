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
using System.Diagnostics;  // Process
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Microsoft.Win32;  // Registry
using XPExplorerBar;
using Xiperware.WiretapAPI;
using XLib.Extensions;

namespace BEGM
{
  /// <summary>
  /// The Options class encapsulates user settings and preferences, and provides an interface
  /// to test network connectivity, manually check for game events, and reset all game data.
  /// </summary>
  public partial class Options : Form
  {
    #region Variables

    private readonly NetManager net;
    private NetworkSettings testedNetSettings;

    private NetworkSettings network;
    private StartupSettings startup;
    private AlertSettings alerts;
    private MiscSettings misc;
    private MapSettings map;
    private LangSettings lang;

    private GameStatusState gameStatusState;
    private ServerStatusState serverStatusState;
    private RecentEventsState recentEventsState;
    private TownStatusState townStatusState;
    private GameMapState gameMapState;
    private FactoryStatusState factoryStatusState;
    private BrigadeStatusState brigadeStatusState;

    public Func<bool> GameStatus_CheckIfBusy;
    public Action GameStatus_UpdateContextMenu;
    public Action CurrentAttacks_UpdateShowAlertsCheckboxes;
    public Action AlertWindow_TestAlert;

    private List<TreeNode> tvwFilterEventTypeNodes;
    public int filterChokePointTotal;

    private readonly Color treeviewDisabled = Color.FromArgb( 238, 239, 247 );

    #endregion

    #region Events

    // misc
    public event EventHandler DockWindowChanged;
    public event EventHandler EventSortOrderChanged;
    public event EventHandler GameStatusDisplayChanged;

    // updates
    public event EventHandler CheckEventsClicked;
    public event EventHandler ResetAllDataClicked;

    // map
    public event EventHandler MapOptionsChanged;
    public event EventHandler MapWallpaperChanged;

    // language
    public event EventHandler LanguageChanged;

    #endregion

    #region Constructors

    /// <summary>
    /// Create a new Options dialog.
    /// </summary>
    /// <param name="net">The NetManager to use for network testing and to apply network settings.</param>
    public Options( NetManager net )
    {
      InitializeComponent();

      this.net = net;


      // get flattened list of all ndoes in tvwFilterEventType.Nodes to iterate over

      tvwFilterEventTypeNodes = new List<TreeNode>();

      foreach( TreeNode rootNode in tvwFilterEventType.Nodes )
        AddChildNodes( rootNode, tvwFilterEventTypeNodes );


      // populate display comboboxs

      for( int i = 0; i < Screen.AllScreens.Length; i++ )
      {
        int idx = cmbGameStatusDisplay.Items.Add( String.Format( Language.Options_Display, i + 1 ) );
        cmbAlertDisplay.Items.Add( String.Format( Language.Options_Display, i + 1 ) );

        if( Screen.AllScreens[i].Primary )  // default: primary screen
          cmbGameStatusDisplay.SelectedIndex = cmbAlertDisplay.SelectedIndex = idx;
      }


      // set localised language text

      BegmMisc.LocaliseTreeView( this.tvwMapOptions, this.tvwFilterEventType, this.tvwFilterCountry );

      foreach( object control in tabLanguage.Controls )
      {
        RadioButton radio = control as RadioButton;
        if( radio == null ) continue;

        string langCode = (string)radio.Tag;
        if( langCode == "en" ) langCode = "en-GB";
        CultureInfo ci = new CultureInfo( langCode );
        radio.Text = String.Format( "           {0}", ci.TextInfo.ToTitleCase( ci.NativeName ) );
      }

#if DEBUG  // normally button is disabled for 30 sec
      this.tmrEnableNewEvents.Interval = 1000;  // 1 sec
#endif


      // dynamic positioning

      cbAlwaysAlertUnderAttack.Left = cbAlwaysAlertChokePointCaptured.Left = lblAlwaysAlert.Right + 6;
      rbDockWindowRight.Left = rbDockWindowLeft.Right + 6;


      // set initial dynamic text

      cbLoadFactoryData_CheckedChanged( null, null );
      trkSleepWhenIdle_ValueChanged( null, null );
      trkAutoNextTime_ValueChanged( null, null );
      trkPostponeIdle_ValueChanged( null, null );
      trkWallUpdateTime_ValueChanged( null, null );
    }

    #endregion

    #region Properties

    /// <summary>
    /// The current network settings.
    /// </summary>
    public NetworkSettings Network
    {
      get { return this.network; }
    }

    /// <summary>
    /// The current startup settings.
    /// </summary>
    public StartupSettings Startup
    {
      get { return this.startup; }
    }

    /// <summary>
    /// The current alert settings.
    /// </summary>
    public AlertSettings Alerts
    {
      get { return this.alerts; }
    }

    /// <summary>
    /// The current misc settings.
    /// </summary>
    public MiscSettings Misc
    {
      get { return this.misc; }
    }

    /// <summary>
    /// The current map settings.
    /// </summary>
    public MapSettings Map
    {
      get { return this.map; }
    }

    /// <summary>
    /// The current language settings.
    /// </summary>
    public LangSettings Lang
    {
      get { return this.lang; }
    }

    /// <summary>
    /// The initial state of the Game Status window.
    /// </summary>
    public GameStatusState GameStatusState
    {
      get { return this.gameStatusState; }
    }

    /// <summary>
    /// The initial state of the ServerStatus widget.
    /// </summary>
    public ServerStatusState ServerStatusState
    {
      get { return this.serverStatusState; }
    }

    /// <summary>
    /// The initial state of the RecentEvents widget.
    /// </summary>
    public RecentEventsState RecentEventsState
    {
      get { return this.recentEventsState; }
    }

    /// <summary>
    /// The initial state of the TownStatus widget.
    /// </summary>
    public TownStatusState TownStatusState
    {
      get { return this.townStatusState; }
    }

    /// <summary>
    /// The initial state of the GameMap widget.
    /// </summary>
    public GameMapState GameMapState
    {
      get { return this.gameMapState; }
    }

    /// <summary>
    /// The initial state of the FactoryStatus widget.
    /// </summary>
    public FactoryStatusState FactoryStatusState
    {
      get { return this.factoryStatusState; }
    }

    /// <summary>
    /// The initial state of the BrigadeStatus widget.
    /// </summary>
    public BrigadeStatusState BrigadeStatusState
    {
      get { return this.brigadeStatusState; }
    }

    /// <summary>
    /// Enables or disables the Updates tab.
    /// </summary>
    public bool UpdateTabEnabled
    {
      set { gbCheckEvents.Enabled = gbResetAllData.Enabled = value; }
    }

    /// <summary>
    /// Gets or sets the values of the controls on the Network tab.
    /// </summary>
    private NetworkSettings NetworkTab
    {
      get
      {
        return new NetworkSettings( txtWiretapHost.Text, txtWiretapPort.Text, rbUseCustomProxy.Checked,
                                    txtProxyHost.Text, txtProxyPort.Text );
      }
      set
      {
        txtWiretapHost.Text = value.wiretapHost;
        txtWiretapPort.Text = value.wiretapPort;
        rbUseCustomProxy.Checked = value.useCustomProxy;
        rbUseIEProxy.Checked = !rbUseCustomProxy.Checked;
        txtProxyHost.Text = value.proxyHost;
        txtProxyPort.Text = value.proxyPort;
      }
    }

    /// <summary>
    /// Gets or sets the values of the controls on the Startup tab.
    /// </summary>
    public StartupSettings StartupTab
    {
      get
      {
        return new StartupSettings( cbRunOnStartup.Checked, cbStartMinimised.Checked, cbSleepWhenIdle.Checked,
                                    trkSleepWhenIdle.Value, cbSleepWhenPlay.Checked, cbWakeAfterPlay.Checked,
                                    cbLoadFactoryData.Checked, cbCheckVersion.Checked );
      }
      set
      {
        cbRunOnStartup.Checked    = value.runOnStartup;
        cbStartMinimised.Checked  = value.startMinimised;
        cbSleepWhenIdle.Checked   = value.sleepWhenIdle;
        trkSleepWhenIdle.Value = value.idleTimeout;
        cbSleepWhenPlay.Checked   = value.sleepWhenPlay;
        cbWakeAfterPlay.Checked   = value.wakeAfterPlay;
        cbLoadFactoryData.Checked = value.loadFactoryData;
        cbCheckVersion.Checked    = value.checkVersion;
      }
    }

    /// <summary>
    /// Gets or sets the values of the controls on the Alerts tab.
    /// </summary>
    public AlertSettings AlertsTab
    {
      get
      {
        DockStyle alertPosition = rbPositionTop.Checked ? DockStyle.Top : DockStyle.Bottom;

        List<int> filterEventTypeIDs = new List<int>();
        List<int> filterChokePointIDs = new List<int>();
        List<int> filterCountryIDs = new List<int>();

        foreach( TreeNode node in tvwFilterEventTypeNodes )  // all nodes
          if( node.Checked && node.Tag != null )
            filterEventTypeIDs.Add( (int)node.Tag );

        foreach( TreeNode node in tvwFilterChokePoint.Nodes )
          if( node.Checked && node.Tag != null )
            filterChokePointIDs.Add( (int)node.Tag );

        foreach( TreeNode node in tvwFilterCountry.Nodes )
          if( node.Checked && node.Tag != null )
            filterCountryIDs.Add( (int)node.Tag );

        return new AlertSettings( cbShowAlerts.Checked, alertPosition, trkAutoNextTime.Value, cbPlayAlertSound.Checked,
          cbPostponeFullscreen.Checked, cbPostponeIdle.Checked, trkPostponeIdle.Value * 5, cbAlwaysAlertUnderAttack.Checked,
          cbAlwaysAlertChokePointCaptured.Checked, cbFilterEventType.Checked, cbFilterChokePoint.Checked,
          cbFilterCountry.Checked, filterEventTypeIDs, filterChokePointIDs, filterCountryIDs );
      }
      set
      {
        // options

        if( value.alertPosition == DockStyle.Top )
          rbPositionTop.Checked = true;
        else
          rbPositionBottom.Checked = true;

        trkAutoNextTime.Value        = value.autoNextTime;
        cbPlayAlertSound.Checked     = value.playSound;
        cbPostponeFullscreen.Checked = value.postponeFullscreen;
        cbPostponeIdle.Checked       = value.postponeIdle;
        trkPostponeIdle.Value        = value.postponeIdleTime / 5;  // eg, 20 mins = 4 ticks across;


        // filters

        cbAlwaysAlertUnderAttack.Checked        = value.alwaysAlertUnderAttack;
        cbAlwaysAlertChokePointCaptured.Checked = value.alwaysAlertChokePointCaptured;

        cbFilterEventType.Checked  = value.filterEventType;
        cbFilterChokePoint.Checked = value.filterChokePoint;
        cbFilterCountry.Checked    = value.filterCountry;


        // event types
          
        foreach( TreeNode node in tvwFilterEventTypeNodes )  // all nodes
        {
          if( node.Tag == null ) continue;

          switch( (int)node.Tag )
          {
            case 1:   node.Checked = value.filterCapturesChokePointCaptured;              break;
            case 2:   node.Checked = value.filterCapturesChokePointControlChanged;        break;
            case 3:   node.Checked = value.filterCapturesChokePointContestedUnderAttack;  break;
            case 4:   node.Checked = value.filterCapturesChokePointContestedRegained;     break;
            case 5:   node.Checked = value.filterCapturesFacilityCaptured;                break;
            case 6:   node.Checked = value.filterCapturesFacilityRecaptured;              break;
            case 7:   node.Checked = value.filterCapturesFacilitySpawnableCaptured;       break;
            case 8:   node.Checked = value.filterCapturesFacilitySpawnableRecaptured;     break;
            case 9:   node.Checked = value.filterAttackObjectivePlaced;                   break;
            case 10:  node.Checked = value.filterAttackObjectiveWithdrawn;                break;
            case 11:  node.Checked = value.filterFirebaseAlliedBlown;                     break;
            case 12:  node.Checked = value.filterFirebaseAxisBlown;                       break;
            case 13:  node.Checked = value.filterHCUnitDeployed;                          break;
            case 14:  node.Checked = value.filterHCUnitMoved;                             break;
            case 15:  node.Checked = value.filterHCUnitRetreated;                         break;
            case 16:  node.Checked = value.filterHCUnitRouted;                            break;
            case 17:  node.Checked = value.filterFactoryHealthDamaged;                    break;
            case 18:  node.Checked = value.filterFactoryHealthDestroyed;                  break;
            case 19:  node.Checked = value.filterFactoryHealthRepaired;                   break;
            case 20:  node.Checked = value.filterFactoryRdpHalted;                        break;
            case 21:  node.Checked = value.filterFactoryRdpResumed;                       break;
            case 22:  node.Checked = value.filterFirebaseNewBrigade;                      break;
          }
        }
          
          
        // chokepoints

        foreach( TreeNode node in tvwFilterChokePoint.Nodes )
          if( node.Tag != null )  // MAC
            node.Checked = value.filterChokePointIDLookup.ContainsKey( (int)node.Tag );

          
        // countries

        foreach( TreeNode node in tvwFilterCountry.Nodes )
        {
          if( node.Tag == null ) continue;  // MAC
          switch( (int)node.Tag )
          {
            case 1:  node.Checked = value.filterCountryEngland;  break;
            case 3:  node.Checked = value.filterCountryFrance;   break;
            case 4:  node.Checked = value.filterCountryGerman;   break;
          }
        }
          

        cbShowAlerts.Checked = value.showAlerts;  // do last as it disables the entire page
      }
    }

    /// <summary>
    /// Gets or sets the values of the controls on the Misc tab.
    /// </summary>
    private MiscSettings MiscTab
    {
      get
      {
        DockStyle dockWindow = DockStyle.None;
        if( cbDockWindow.Checked )
          dockWindow = rbDockWindowLeft.Checked ? DockStyle.Left : DockStyle.Right;

        return new MiscSettings( dockWindow, cmbGameStatusDisplay.SelectedIndex + 1,
                                 rbEventSortBottom.Checked ? SortOrder.Ascending : SortOrder.Descending,
                                 cmbAlertDisplay.SelectedIndex + 1 );
      }
      set
      {
        cbDockWindow.Checked = value.dockWindow != DockStyle.None;
        rbDockWindowLeft.Checked = value.dockWindow == DockStyle.Left;
        rbDockWindowRight.Checked = value.dockWindow != DockStyle.Left;

        rbEventSortTop.Checked = value.eventSortOrder == SortOrder.Descending;
        rbEventSortBottom.Checked = value.eventSortOrder != SortOrder.Descending;

        cmbGameStatusDisplay.SelectedIndex = value.GameStatusDisplayIndex;
        cmbAlertDisplay.SelectedIndex = value.AlertDisplayIndex;
      }
    }

    /// <summary>
    /// Gets or sets the values of the controls on the Map tab.
    /// </summary>
    private MapSettings MapTab
    {
      get
      {
        int mapSize = 40;

        if     ( rbMapSize60.Checked  ) mapSize = 60;
        else if( rbMapSize80.Checked  ) mapSize = 80;
        else if( rbMapSize100.Checked ) mapSize = 100;

        DrawOverlayParams wallOverlayParams = new DrawOverlayParams() {
          showAttackObjectives     = tvwMapOptions.Nodes["nodeAttackObjectives"].Checked,
          showAttackObjectiveNames = tvwMapOptions.Nodes["nodeAttackObjectives"].Nodes["nodeAttackObjectiveNames"].Checked,
          showFirebases            = tvwMapOptions.Nodes["nodeFirebases"].Checked,
          showAirfields            = tvwMapOptions.Nodes["nodeAirfields"].Checked,
          showSupplyLinks          = tvwMapOptions.Nodes["nodeSupplyLinks"].Checked,
          showBrigadeLinks         = tvwMapOptions.Nodes["nodeBrigadeLinks"].Checked,
          showBrigadeLinksSelected = false,
          showBrigadeLinksArmy     = tvwMapOptions.Nodes["nodeBrigadeLinks"].Nodes["nodeBrigadeLinksArmy"].Checked,
          showAirGrid              = tvwMapOptions.Nodes["nodeAirGrid"].Checked,
          showFrontlines           = tvwMapOptions.Nodes["nodeFrontlines"].Checked,
          showAttackArrows         = tvwMapOptions.Nodes["nodeAttackArrows"].Checked,
          showAttackArrowsLinked   = tvwMapOptions.Nodes["nodeAttackArrows"].Nodes["nodeAttackArrowsLinked"].Checked,
          showPlayerActivity       = tvwMapOptions.Nodes["nodePlayerActivity"].Checked,
          showCountryBorders       = tvwMapOptions.Nodes["nodeCountryBorders"].Checked,
          showCountryBordersNames  = tvwMapOptions.Nodes["nodeCountryBorders"].Nodes["nodeCountryBordersNames"].Checked,
        };

        return new MapSettings( cbAlwaysUseDefaultMapSize.Checked, mapSize, cbShowWallpaper.Checked,
          wallOverlayParams, trkWallZoom.Value, trkWallUpdateTime.Value, cbWallRemove.Checked );
      }
      set
      {
        if( cbAlwaysUseDefaultMapSize.Enabled )  // hires plugin loaded
        {
          cbAlwaysUseDefaultMapSize.Checked = value.useDefaultWhenPlaying;

          switch( value.mapSize )
          {
            case 60: rbMapSize60.Checked = true; break;
            case 80: rbMapSize80.Checked = true; break;
            case 100: rbMapSize100.Checked = true; break;
            default: rbMapSize40.Checked = true; break;
          }
        }

        cbShowWallpaper.Checked = value.showWallpaper;

        value.wallOverlayParams.UpdateControl( tvwMapOptions );

        trkWallZoom.Value = value.wallZoom;
        trkWallUpdateTime.Value = value.wallUpdate;
        cbWallRemove.Checked = value.wallRemove;
      }
    }

    /// <summary>
    /// Gets or sets the values of the controls on the Language tab.
    /// </summary>
    private LangSettings LangTab
    {
      get
      {
        foreach( object control in tabLanguage.Controls )
        {
          RadioButton radio = control as RadioButton;
          if( radio == null ) continue;

          if( radio.Checked )
            return new LangSettings( (string)radio.Tag );
        }

        return new LangSettings( "en" );  // default
      }
      set
      {
        foreach( object control in tabLanguage.Controls )
        {
          RadioButton radio = control as RadioButton;
          if( radio == null ) continue;

          radio.Checked = (string)radio.Tag == value.langCode;
        }


        // also update map plugin info link position

        string linkText = "Hires Map Plugin";
        if( value.langCode == "es" )
          linkText = "Plugin del Mapa";

        lnkMapPluginInfo.Links.Clear();
        int iStart = lnkMapPluginInfo.Text.IndexOf(linkText);
        if( iStart >= 0 )
          lnkMapPluginInfo.Links.Add( iStart, linkText.Length );
      }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Load settings from the registry.
    /// </summary>
    public void LoadSettings()
    {
      Log.AddEntry( "Loading settings..." );

#if !DEBUG
      try
      {
#endif
        // network

        NetworkSettings networkSettings = new NetworkSettings();
        networkSettings.LoadFromRegistry();

        try
        {
          net.LoadSettings( networkSettings );
        }
        catch( Exception ex )
        {
          MessageBox.Show( Language.Error_LoadNetworkSettings + "\n\n" + ex.Message,
                           Language.Error_Error, MessageBoxButtons.OK, MessageBoxIcon.Error );

          networkSettings = new NetworkSettings();
          net.LoadSettings( networkSettings );
        }

        this.NetworkTab = this.network = this.testedNetSettings = networkSettings;


        // startup

        this.startup = new StartupSettings();
        this.startup.LoadFromRegistry();
        this.StartupTab = this.startup;


        // alert

        this.alerts = new AlertSettings();
        this.alerts.LoadFromRegistry();
        this.AlertsTab = this.alerts;


        // misc

        this.misc = new MiscSettings();
        this.misc.LoadFromRegistry();
        this.MiscTab = this.misc;


        // map

        this.map = new MapSettings();
        this.map.LoadFromRegistry();
        this.MapTab = this.map;


        // language

        this.lang = new LangSettings();
        this.lang.LoadFromRegistry();
        this.LangTab = this.lang;


        // game status state

        this.gameStatusState = new GameStatusState();
        this.gameStatusState.LoadFromRegistry();


        // server status state

        this.serverStatusState = new ServerStatusState();
        this.serverStatusState.LoadFromRegistry();


        // recent events state

        this.recentEventsState = new RecentEventsState();
        this.recentEventsState.LoadFromRegistry();


        // town status state

        this.townStatusState = new TownStatusState();
        this.townStatusState.LoadFromRegistry();


        // game map state

        this.gameMapState = new GameMapState();
        this.gameMapState.LoadFromRegistry();


        // factory status state

        this.factoryStatusState = new FactoryStatusState();
        this.factoryStatusState.LoadFromRegistry();


        // brigade status state

        this.brigadeStatusState = new BrigadeStatusState();
        this.brigadeStatusState.LoadFromRegistry();


        Log.Okay();

#if !DEBUG
      }
      catch( Exception ex )
      {
        Log.Error();
        Log.AddError( ex.Message );
      }
#endif
    }

    /// <summary>
    /// Restore the position of the widgets from the values stored in the registry.
    /// </summary>
    /// <remarks>Assumes all expandos start in first column.</remarks>
    /// <param name="columns">The collection of GameStatusColumns to restore.</param>
    /// <param name="allExpandos">An array of all expandos.</param>
    public void LoadWidgetPosition( List<GameStatusColumn> columns, Expando[] allExpandos )
    {
      Log.AddEntry( "Restoring widget order..." );

#if !DEBUG
      try
      {
#endif
        // create array of sorted lists: index[column][position][expando], sorted by position within each column

        SortedList<float, Expando>[] exp = new SortedList<float, Expando>[columns.Count];
        for( int i = 0; i < columns.Count; i++ )
          exp[i] = new SortedList<float, Expando>();


        // get saved index for each expando

        RegistryKey key = Registry.CurrentUser.OpenSubKey( "Software\\BEGameMonitor\\Widgets" );
        if( key == null ) return;

        foreach( Expando expando in allExpandos )
        {
          string name = expando.Name.Replace( "exp", "" );
          int i = (int)key.GetValue( name + "Index", -1 );  // value: colx1000 + row, or 999 if detatched

          if( i == 999 )  // detatched
          {
            try
            {
              string pos = (string)key.GetValue( name + "Position", "" );  // value: "-123x456" x/y coords
              Point ptPosition = Xiperware.WiretapAPI.Misc.ParsePoint( pos );
              expando.Detatch( ptPosition.X, ptPosition.Y, true );

              if( name == "GameMap" )  // also load size
              {
                Widgets.GameMap wgtGameMap = (Widgets.GameMap)expando.Controls[0];
                string strSize = (string)key.GetValue( name + "Size", "" );  // value: "123x456" width/height
                if( !String.IsNullOrEmpty( strSize ) )
                {
                  Point ptSize = Xiperware.WiretapAPI.Misc.ParsePoint( strSize );
                  wgtGameMap.SetDetatchedSize( new Size( ptSize.X, ptSize.Y ) );
                }
              }

              continue;
            }
            catch {}  // fall through, use default position
          }

          int iColumn = 0;
          float iPosition = 999;  // default: end

          if( i >= 0 )
          {
            iColumn = i / 1000;
            iPosition = i % 1000;
          }

          if( iColumn > columns.Count - 1 )
            iColumn = 0;

          while( exp[iColumn].ContainsKey( iPosition ) )
            iPosition += 0.01F; // make unique

          exp[iColumn].Add( iPosition, expando );
        }

        key.Close();


        // remove all expandos from first column

        while( columns[0].Expandos.Count > 0 )
          columns[0].Expandos.RemoveAt( 0 );


        // add back in new position

        for( int iColumn = 0; iColumn < columns.Count; iColumn++ )
          foreach( Expando expando in exp[iColumn].Values )
            columns[iColumn].Expandos.Add( expando );


        Log.Okay();

#if !DEBUG
      }
      catch( Exception ex )
      {
        Log.Error();
        Log.AddError( ex.Message );
      }
#endif
    }

    /// <summary>
    /// Restore the expanded/collapsed widget state from the values stored in the registry.
    /// </summary>
    /// <param name="allExpandos">An array of all expandos.</param>
    public void LoadWidgetState( Expando[] allExpandos )
    {
      Log.AddEntry( "Restoring widget state..." );

#if !DEBUG
      try
      {
#endif
        // read saved setting and apply to each expando

        RegistryKey key = Registry.CurrentUser.CreateSubKey( "Software\\BEGameMonitor\\Widgets" );
        if( key == null ) return;

        foreach( Expando expando in allExpandos )
        {
          string name = expando.Name.Replace( "exp", "" );

          // first load: current attacks expanded, others collapsed
          string expanded = (string)key.GetValue( name + "Expanded", null );

          if( expanded == "True" )
            expando.Collapsed = false;
          else if( expanded == "False" )
            expando.Collapsed = true;
          // no saved setting found, defaults:
          else if( name == "CurrentAttacks" )
            expando.Collapsed = false;
          else
            expando.Collapsed = true;
        }

        key.Close();

        Log.Okay();

#if !DEBUG
      }
      catch( Exception ex )
      {
        Log.Error();
        Log.AddError( ex.Message );
      }
#endif
    }

    /// <summary>
    /// Save the position and expanded state for each widget to the registry.
    /// </summary>
    /// <param name="columns">The collection of GameStatusColumns to save.</param>
    /// <param name="allExpandos">An array of all expandos.</param>
    public void SaveWidgetPosition( List<GameStatusColumn> columns, Expando[] allExpandos )
    {
#if !DEBUG
      try
      {
#endif
        Dictionary<string,bool> seen = new Dictionary<string, bool>();


        // attached expandos (belong to a GameStatusColumn)

        RegistryKey key = Registry.CurrentUser.CreateSubKey( "Software\\BEGameMonitor\\Widgets" );
        if( key == null ) return;

        for( int i = 0; i < columns.Count; i++ )
        {
          foreach( Expando expando in columns[i].Expandos )
          {
            string name = expando.Name.Replace( "exp", "" );
            key.SetValue( name + "Index", ( i * 1000 ) + columns[i].Expandos.IndexOf( expando ) );
            key.SetValue( name + "Expanded", !expando.Collapsed );
            seen.Add( expando.Name, true );
          }
        }


        // detatched expandos

        foreach( Expando expando in allExpandos )
        {
          if( seen.ContainsKey( expando.Name ) ) continue;
          if( !expando.Detatched )
            continue;  // shouldn't happen

          string name = expando.Name.Replace( "exp", "" );
          key.SetValue( name + "Index", 999 );
          key.SetValue( name + "Expanded", !expando.Collapsed );
          key.SetValue( name + "Position", String.Format( "{0}x{1}", expando.Parent.Location.X, expando.Parent.Location.Y ) );
          if( name == "GameMap" )  // also save size
          {
            Widgets.GameMap wgtGameMap = (Widgets.GameMap)expando.Controls[0];
            key.SetValue( name + "Size", String.Format( "{0}x{1}", wgtGameMap.Width, wgtGameMap.Height ) );
          }
        }


        key.Close();

#if !DEBUG
      }
      catch { }
#endif
    }

    /// <summary>
    /// Populate the chokepoint filter on the alerts tab with the given list of cps.
    /// </summary>
    /// <remarks>Called after program init completes, when we have a list of cps.</remarks>
    /// <param name="cps">An array of all ChokePoints.</param>
    public void PopulateChokePointFilter( ChokePoint[] cps )
    {
      // copy and sort chokepoint array

      ChokePoint[] sortedCps = (ChokePoint[])cps.Clone();
      Array.Sort( sortedCps );


      // clear list

      tvwFilterChokePoint.Nodes.Clear();


      // populate

      foreach( ChokePoint cp in sortedCps )
      {
        if( cp == null || cp.IsTraining ) continue;

        int idx = tvwFilterChokePoint.Nodes.Add( new TreeNode( cp.Name ) );
        tvwFilterChokePoint.Nodes[idx].Tag = cp.ID;
      }


      // store total

      filterChokePointTotal = tvwFilterChokePoint.Nodes.Count;


      // reapply checkmarks

      this.AlertsTab = this.alerts;
    }

    /// <summary>
    /// Recursive method to add all child nodes to the given list.
    /// </summary>
    /// <remarks>Used by the Options constructor.</remarks>
    /// <param name="parentNode">The node to add and recurse.</param>
    /// <param name="list">The TreeNode list to append to.</param>
    private void AddChildNodes( TreeNode parentNode, List<TreeNode> list )
    {
      list.Add( parentNode );

      foreach( TreeNode childNode in parentNode.Nodes )
        AddChildNodes( childNode, list );
    }

    /// <summary>
    /// Adjust alert settings to show/not show alerts for the given ChokePoint.
    /// </summary>
    /// <remarks>Used by the alert window ("Show future alerts for this attack")
    /// and the Current Attacks widget.</remarks>
    /// <param name="cp">The ChokePoint to show/unshow alerts (or all cps if null).</param>
    /// <param name="show">True to enable alerts, false to disable.</param>
    public void ShowAlerts( ChokePoint cp, bool show )
    {
      if( show )
      {
        // make the necessary changes to alert options to make sure future capture alerts are displayed

        if( !this.alerts.showAlerts )
        {
          this.alerts.showAlerts = true;
          GameStatus_UpdateContextMenu();
        }


        // update event type filters to include capture events

        if( this.alerts.filterEventType )
        {
          for( int i = 1; i <= 8; i++ )  // 1 - 8 = capture filters
            if( !this.alerts.filterEventTypeIDs.Contains( i ) )
              this.alerts.filterEventTypeIDs.Add( i );
        }


        // disable chokepoint filter, or include given chokepoint

        if( cp == null )  // all cps
        {
          this.alerts.filterChokePoint = false;
          this.alerts.filterChokePointIDs.Clear();
        }
        else              // one cp
        {
          if( this.alerts.filterChokePoint )
          {
            if( !this.alerts.filterChokePointIDLookup.ContainsKey( cp.ID ) )
              this.alerts.filterChokePointIDs.Add( cp.ID );
          }
        }

        // disable country filter

        if( this.alerts.filterCountry )
        {
          this.alerts.filterCountry = false;
        }
      }
      else  // do not show alerts
      {
        if( cp == null )  // all cps
        {
          // enable empty filter

          this.alerts.filterChokePoint = true;
          this.alerts.filterChokePointIDs.Clear();
        }
        else              // one cp
        {
          // if filtering, remove chokepoint

          if( this.alerts.filterChokePoint && this.alerts.filterChokePointIDLookup.ContainsKey( cp.ID ) )
          {
            this.alerts.filterChokePointIDs.Remove( cp.ID );
          }
        }
      }


      // save and update

      this.alerts.RegenerateFlags();
      this.alerts.SaveToRegistry( false );
      this.AlertsTab = this.alerts;
    }

    /// <summary>
    /// Enables the controls on the Map options tab when the plugin is available.
    /// </summary>
    public void EnableHiresMapOptions()
    {
      lnkMapPluginInfo.Visible = false;
      lblMapInfo.Visible = true;
      rbMapSize60.Enabled =
        rbMapSize80.Enabled =
        rbMapSize100.Enabled =
        lblMemUsage60.Enabled =
        lblMemUsage80.Enabled =
        lblMemUsage100.Enabled =
        cbAlwaysUseDefaultMapSize.Enabled = true;
    }

    #endregion

    #region Event Handlers

    #region General

    // form activated/tab changed
    private void Options_Activated( object sender, EventArgs e )
    {
      // workaround for itemheight bug

      tabControl_SelectedIndexChanged( sender, e );
    }
    private void tabControl_SelectedIndexChanged( object sender, EventArgs e )
    {
      // workaround for itemheight bug

      if( tabControl.SelectedTab == tabAlerts )
      {
        if( cbAlertFilters.Checked && tvwFilterEventType.ItemHeight == 14 )
          tvwFilterEventType.ItemHeight = tvwFilterChokePoint.ItemHeight = tvwFilterCountry.ItemHeight = 15;
      }
      else if( tabControl.SelectedTab == tabMap )
      {
        if( cbWallOptions.Checked )
        {
          if( tvwMapOptions.ItemHeight == 14 )
            tvwMapOptions.ItemHeight = 15;
          foreach( TreeNode node in tvwMapOptions.Nodes )
            node.ExpandAll();
        }


        // update map tab freemem

        Kernel32.MEMORYSTATUSEX msex = new Kernel32.MEMORYSTATUSEX();
        if( Kernel32.GlobalMemoryStatusEx( msex ) )
          lblFreeMem.Text = String.Format( "{0}mb/{1}mb", msex.ullAvailPhys / 1048576, msex.ullTotalPhys / 1048576 );
        else
          lblFreeMem.Text = null;
      }
    }

    // cancel when user clicks [X] or alt-f4's dialog
    private void Options_FormClosing( object sender, FormClosingEventArgs e )
    {
      if( e.CloseReason == CloseReason.UserClosing )
      {
        btnCancel.PerformClick();
      }

      lblTestResult.Text = "";

      // workaround for itemheight bug
      tvwFilterEventType.ItemHeight = tvwFilterChokePoint.ItemHeight = tvwFilterCountry.ItemHeight = tvwMapOptions.ItemHeight = 14;
    }

    // action buttons
    private void btnOK_Click( object sender, EventArgs e )
    {
      // check to see if we need to test network settings

      NetworkSettings newNetworkSettings = this.NetworkTab;

      if( !this.testedNetSettings.Equivalent( newNetworkSettings ) )
      {
        tabControl.SelectTab( tabNetwork );
        btnTestConn.Focus();
        DialogResult result = MessageBox.Show( Language.Error_NetworkSettingsUntested,
                                               Language.Error_Question, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question );
        if( result == DialogResult.Yes )
        {
          btnTestConn.PerformClick();
          return;
        }
        else if( result == DialogResult.Cancel )
        {
          return;
        }
        // else DialogResult.No, continue
      }


      // apply settings


      // network

      if( this.network != newNetworkSettings )
      {
        try
        {
          net.LoadSettings( newNetworkSettings );
        }
        catch( Exception ex )
        {
          MessageBox.Show( Language.Error_ApplyNetworkSettings + ":\n\n" + ex.Message,
                           Language.Error_Error, MessageBoxButtons.OK, MessageBoxIcon.Error );
          return;
        }

        if( !this.network.Equivalent( newNetworkSettings ) )
          Log.AddEntry( "Using server {0}", newNetworkSettings );

        this.network = this.testedNetSettings = newNetworkSettings;
        this.network.SaveToRegistry( true );
      }


      // startup

      StartupSettings newStartupSettings = this.StartupTab;
      if( this.startup != newStartupSettings )
      {
        this.startup = newStartupSettings;
        this.startup.SaveToRegistry( true );
      }


      // alerts

      AlertSettings newAlertSettings = this.AlertsTab;
      if( this.alerts != newAlertSettings )
      {
        this.alerts = newAlertSettings;
        this.alerts.SaveToRegistry( true );

        GameStatus_UpdateContextMenu();
        CurrentAttacks_UpdateShowAlertsCheckboxes();
      }


      // misc

      MiscSettings newMiscSettings = this.MiscTab;
      if( this.misc != newMiscSettings )
      {
        MiscSettings prevMiscSettings = this.misc;
        this.misc = newMiscSettings;
        this.misc.SaveToRegistry( true );

        if( this.misc.dockWindow != prevMiscSettings.dockWindow && this.DockWindowChanged != null )
          this.DockWindowChanged( null, null );
        if( this.misc.eventSortOrder != prevMiscSettings.eventSortOrder && this.EventSortOrderChanged != null )
          this.EventSortOrderChanged( null, null );
        if( this.misc.GameStatusDisplayIndex != prevMiscSettings.gameStatusDisplay - 1 && this.GameStatusDisplayChanged != null )
          this.GameStatusDisplayChanged( null, null );
      }


      // map

      MapSettings newMapSettings = this.MapTab;
      if( this.map != newMapSettings )
      {
        MapSettings prevMapSettings = this.map;
        this.map = newMapSettings;
        this.map.SaveToRegistry( true );

        if( ( this.map.mapSize != prevMapSettings.mapSize || this.map.useDefaultWhenPlaying != prevMapSettings.useDefaultWhenPlaying ) && this.MapOptionsChanged != null )
          this.MapOptionsChanged( null, null );
        if( !this.map.WallpaperEquivalent( prevMapSettings ) && this.MapWallpaperChanged != null )
          this.MapWallpaperChanged( null, null );
      }


      // language

      LangSettings newLangSettings = this.LangTab;
      if( this.lang != newLangSettings )
      {
        LangSettings prevLangSettings = this.lang;
        this.lang = newLangSettings;
        this.lang.SaveToRegistry( true );

        if( this.lang != prevLangSettings && this.LanguageChanged != null )
          this.LanguageChanged( null, null );
      }


      // close window

      this.Close();
    }
    private void btnCancel_Click( object sender, EventArgs e )
    {
      // revert any unsaved changes

      this.NetworkTab = this.network;
      this.StartupTab = this.startup;
      this.AlertsTab = this.alerts;
      this.MapTab = this.map;
      this.MiscTab = this.misc;
      this.LangTab = this.lang;
    }
    private void btnDefaults_Click( object sender, EventArgs e )
    {
      // set controls to default (don't apply yet)

      if( tabControl.SelectedTab == tabNetwork )
        this.NetworkTab = new NetworkSettings();
      else if( tabControl.SelectedTab == tabStartup )
        this.StartupTab = new StartupSettings();
      else if( tabControl.SelectedTab == tabAlerts )
        this.AlertsTab = new AlertSettings();
      else if( tabControl.SelectedTab == tabMap )
        this.MapTab = new MapSettings();
      else if( tabControl.SelectedTab == tabMisc )
        this.MiscTab = new MiscSettings();
    }

    #endregion

    #region Network

    // proxy host/port only enabled when custom proxy checked
    private void rbProxyServer_CheckedChanged( object sender, EventArgs e )
    {
      txtProxyHost.Enabled = txtProxyPort.Enabled = rbUseCustomProxy.Checked;
    }

    // test connection button
    private void btnTestConn_Click( object sender, EventArgs e )
    {
      lblTestResult.Focus();  // unfocus
      btnTestConn.Enabled = false;
      lblTestResult.Text = "";
      this.Update();

      NetworkSettings testNetSettings = this.NetworkTab;
      Log.AddEntry( "Testing network settings: {0}", testNetSettings );

      string errorMessage = net.TestConnection( testNetSettings );
      if( errorMessage == "" )
      {
        lblTestResult.ForeColor = Color.DarkGreen;
        lblTestResult.Text = Language.Options_NetworkTestSuccess;
        Log.AddEntry( "Network test successful" );
      }
      else
      {
        lblTestResult.ForeColor = Color.Red;
        lblTestResult.Text = errorMessage;
        Log.AddError( errorMessage );
      }

      this.testedNetSettings = testNetSettings;
      btnTestConn.Enabled = true;
    }

    // port number validation
    private void ValidatePort( TextBox textbox, CancelEventArgs e )
    {
      try
      {
        if( textbox.Text != "" )
        {
          ushort port = ushort.Parse( textbox.Text );  // 0-65535
          if( port == 0 )
            e.Cancel = true;
        }
      }
      catch
      {
        e.Cancel = true;
      }

      if( e.Cancel )
        errorProvider.SetError( textbox, Language.Error_InvalidPort );
      else
        errorProvider.SetError( textbox, null );
    }
    private void txtWiretapPort_Validating( object sender, CancelEventArgs e )
    {
      ValidatePort( txtWiretapPort, e );
    }
    private void txtProxyPort_Validating( object sender, CancelEventArgs e )
    {
      ValidatePort( txtProxyPort, e );
    }

    #endregion

    #region Startup

    // update "x hours" text when trackbar moved
    private void trkSleepWhenIdle_ValueChanged( object sender, EventArgs e )
    {
      lblSleepWhenIdle.Text = trkSleepWhenIdle.Value + " " + ( trkSleepWhenIdle.Value == 1 ? Language.Time_Hour : Language.Time_Hours );
    }

    // sleep when idle trackbar only enabled when option checked
    private void cbSleepWhenIdle_CheckedChanged( object sender, EventArgs e )
    {
      trkSleepWhenIdle.Enabled = lblSleepWhenIdle.Enabled = cbSleepWhenIdle.Checked;
    }

    // wake after play checkbox only enabled if sleep when play checked
    private void cbSleepWhenPlay_CheckedChanged( object sender, EventArgs e )
    {
      cbWakeAfterPlay.Enabled = cbSleepWhenPlay.Checked;
    }

    // update help text data amount when load factory data checked
    private void cbLoadFactoryData_CheckedChanged( object sender, EventArgs e )
    {
      int data = 97;  // kb
      if( cbLoadFactoryData.Checked )
        data += 138;

      lblSleepMode.Text = String.Format( Language.Options_SleepMode, data );
    }
    
    #endregion

    #region Alerts

    // test alert button
    private void btnTestAlert_Click( object sender, EventArgs e )
    {
      AlertSettings prevAlertSettings = this.alerts;
      this.alerts = this.AlertsTab;

      Log.AddEntry( "Displaying test alert ({0})", this.alerts );
      AlertWindow_TestAlert();

      this.alerts = prevAlertSettings;
    }

    // disable tab page when "Show alerts" unchecked
    private void cbShowAlerts_CheckedChanged( object sender, EventArgs e )
    {
      bool enabled = cbShowAlerts.Checked;

      // options

      lblAlertPosition.Enabled = rbPositionTop.Enabled = rbPositionBottom.Enabled = enabled;
      lblAutoNext.Enabled = trkAutoNextTime.Enabled = lblAutoNextTime.Enabled = enabled;
      cbPlayAlertSound.Enabled = btnTestAlert.Enabled = enabled;
      lblPostponeAlerts.Enabled = cbPostponeFullscreen.Enabled = cbPostponeIdle.Enabled = enabled;
      trkPostponeIdle.Enabled = lblPostponeIdle.Enabled = enabled && cbPostponeIdle.Checked;

      // filters

      gbFilterBy.Enabled = lblAlwaysAlert.Enabled = enabled;
      cbAlwaysAlertUnderAttack.Enabled = cbAlwaysAlertChokePointCaptured.Enabled = enabled;

      if( !enabled )
      {
        cbFilterEventType.Enabled = cbFilterChokePoint.Enabled = cbFilterCountry.Enabled = false;
        tvwFilterEventType.Enabled = tvwFilterChokePoint.Enabled = tvwFilterCountry.Enabled = false;

        tvwFilterEventType.BackColor =
          tvwFilterChokePoint.BackColor =
          tvwFilterCountry.BackColor = treeviewDisabled;
      }
      else
      {
        cbFilterEventType.Enabled = cbFilterChokePoint.Enabled = cbFilterCountry.Enabled = true;

        if( cbFilterEventType.Checked )
        {
          tvwFilterEventType.Enabled = true;
          tvwFilterEventType.BackColor = SystemColors.Window;
        }
        if( cbFilterChokePoint.Checked )
        {
          tvwFilterChokePoint.Enabled = true;
          tvwFilterChokePoint.BackColor = SystemColors.Window;
        }
        if( cbFilterCountry.Checked )
        {
          tvwFilterCountry.Enabled = true;
          tvwFilterCountry.BackColor = SystemColors.Window;
        }
      }
    }

    // edit filters toggle button
    private void cbAlertFilters_CheckedChanged( object sender, EventArgs e )
    {
      if( cbAlertFilters.Checked )
      {
        pnlAlertsFilters.Visible = true;
        pnlAlertsOptions.Visible = false;

        // workaround for itemheight bug
        if( tvwFilterEventType.ItemHeight == 14 )
          tvwFilterEventType.ItemHeight = tvwFilterChokePoint.ItemHeight = tvwFilterCountry.ItemHeight = 15;
      }
      else
      {
        pnlAlertsOptions.Visible = true;
        pnlAlertsFilters.Visible = false;
      }
    }

    // disable treeviews when respective filter checkbox unchecked
    private void cbFilterEventType_CheckedChanged( object sender, EventArgs e )
    {
      tvwFilterEventType.Enabled = cbFilterEventType.Checked;
      tvwFilterEventType.BackColor = cbFilterEventType.Checked ? SystemColors.Window : treeviewDisabled;
    }
    private void cbFilterChokePoint_CheckedChanged( object sender, EventArgs e )
    {
      tvwFilterChokePoint.Enabled = cbFilterChokePoint.Checked;
      tvwFilterChokePoint.BackColor = cbFilterChokePoint.Checked ? SystemColors.Window : treeviewDisabled;
    }
    private void cbFilterCountry_CheckedChanged( object sender, EventArgs e )
    {
      tvwFilterCountry.Enabled = cbFilterCountry.Checked;
      tvwFilterCountry.BackColor = cbFilterCountry.Checked ? SystemColors.Window : treeviewDisabled;
    }

    // keep tvwFilterEventType checkmarks in sync
    bool recurseLock = false;
    ArrowDirection recurseDirection = ArrowDirection.Down;
    private void tvwFilterEventType_AfterCheck( object sender, TreeViewEventArgs e )
    {
      bool recurseTop = false;  // top of recursion stack
      if( !recurseLock )
        recurseTop = recurseLock = true;


      // set child nodes to same state as current

      if( recurseTop ) recurseDirection = ArrowDirection.Down;
      if( recurseDirection == ArrowDirection.Down )
      {
        foreach( TreeNode node in e.Node.Nodes )
          node.Checked = e.Node.Checked;
      }


      // set parent node checked if all siblings checked

      if( recurseTop ) recurseDirection = ArrowDirection.Up;
      if( recurseDirection == ArrowDirection.Up && e.Node.Parent != null )  // not root node
      {
        bool allSiblingsChecked = true;
        foreach( TreeNode node in e.Node.Parent.Nodes )
          if( !node.Checked )
            allSiblingsChecked = false;

        if( e.Node.Parent.Checked != allSiblingsChecked )
          e.Node.Parent.Checked = allSiblingsChecked;
      }


      if( recurseTop )
        recurseLock = false;  // release lock
    }

    // chokepoint filter context menu
    private void itmChokePointSelectAll_Click( object sender, EventArgs e )
    {
      foreach( TreeNode node in tvwFilterChokePoint.Nodes )
        node.Checked = true;
    }
    private void itmChokePointClearAll_Click( object sender, EventArgs e )
    {
      foreach( TreeNode node in tvwFilterChokePoint.Nodes )
        node.Checked = false;
    }

    // update trackbar labels
    private void trkAutoNextTime_ValueChanged( object sender, EventArgs e )
    {
      lblAutoNextTime.Text = trkAutoNextTime.Value + " " + ( trkAutoNextTime.Value == 1 ? Language.Time_Sec : Language.Time_Secs );
    }
    private void trkPostponeIdle_ValueChanged( object sender, EventArgs e )
    {
      lblPostponeIdle.Text = ( trkPostponeIdle.Value * 5 ) + " " + Language.Time_Mins;
    }

    // disable postpone idle trackbar if option unchecked
    private void cbPostponeIdle_CheckedChanged( object sender, EventArgs e )
    {
      trkPostponeIdle.Enabled = lblPostponeIdle.Enabled = cbPostponeIdle.Checked;
    }

    #endregion

    #region Map

    // link to plugin page on website
    private void lnkMapPluginInfo_LinkClicked( object sender, LinkLabelLinkClickedEventArgs e )
    {
      try
      {
        Process.Start( this.Lang.Homepage + "hires-map-plugin.html" );
      }
      catch( Exception ex )
      {
        MessageBox.Show( ex.Message, Language.Error_Error, MessageBoxButtons.OK, MessageBoxIcon.Error );
      }
    }

    // wallpaper options toggle button
    private void cbWallOptions_CheckedChanged( object sender, EventArgs e )
    {
      if( cbWallOptions.Checked )
      {
        pnlMapWallOptions.Visible = true;
        pnlMapOptions.Visible = false;
        if( tvwMapOptions.ItemHeight == 14 )
          tvwMapOptions.ItemHeight = 15;
        foreach( TreeNode node in tvwMapOptions.Nodes )
          node.ExpandAll();
      }
      else
      {
        pnlMapOptions.Visible = true;
        pnlMapWallOptions.Visible = false;
      }
    }

    // wallpaper controls
    private void cbShowWallpaper_CheckedChanged( object sender, EventArgs e )
    {
      bool enabled = cbShowWallpaper.Checked;

      lblWallOptions.Enabled = tvwMapOptions.Enabled = enabled;
      lblWallZoom.Enabled = trkWallZoom.Enabled = lblWallZoomMin.Enabled = lblWallZoomMax.Enabled = enabled;
      lblWallUpdate.Enabled = trkWallUpdateTime.Enabled = lblWallUpdateTime.Enabled = enabled;
      cbWallRemove.Enabled = enabled;
    }
    private void trkWallUpdateTime_ValueChanged( object sender, EventArgs e )
    {
      lblWallUpdateTime.Text = trkWallUpdateTime.Value + " " + ( trkWallUpdateTime.Value == 1 ? Language.Time_Min : Language.Time_Mins );
    }

    // prevent map options selection, collapsing
    private void tvwMapOptions_BeforeSelect( object sender, TreeViewCancelEventArgs e )
    {
      e.Node.Checked = !e.Node.Checked;
      e.Cancel = true;
    }
    private void tvwMapOptions_BeforeCollapse( object sender, TreeViewCancelEventArgs e )
    {
      e.Cancel = true;
    }

    // handle map options changing
    private void tvwMapOptions_AfterCheck( object sender, TreeViewEventArgs e )
    {
      if( recurseLock ) return;
      recurseLock = true;

      // sync parent/child check marks

      // if selecting child, make sure parent is checked
      if( e.Node.Checked && e.Node.Parent != null && !e.Node.Parent.Checked )
        e.Node.Parent.Checked = true;

      // if unselecting parent, clear all children
      if( !e.Node.Checked )
        foreach( TreeNode node in e.Node.Nodes )
          node.Checked = false;

      recurseLock = false;
    }

    // apply button
    private void btnApplyWallpaper_Click( object sender, EventArgs e )
    {
      // revert other map options (to avoid changing)

      cbAlwaysUseDefaultMapSize.Checked = this.map.useDefaultWhenPlaying;

      switch( this.map.mapSize )
      {
        case 60: rbMapSize60.Checked = true; break;
        case 80: rbMapSize80.Checked = true; break;
        case 100: rbMapSize100.Checked = true; break;
        default: rbMapSize40.Checked = true; break;
      }


      // get and save new wallpaper map settings

      this.map = this.MapTab;
      this.map.SaveToRegistry( true );


      // redraw with new settings

      if( this.MapWallpaperChanged != null )
        this.MapWallpaperChanged( null, null );
    }

    #endregion

    #region Misc

    private void cbDockWindow_CheckedChanged( object sender, EventArgs e )
    {
      pnlDockWindow.Enabled = cbDockWindow.Checked;
    }
    #endregion

    #region Updates

    // buttons
    private void btnCheckEvents_Click( object sender, EventArgs e )
    {
      if( this.CheckEventsClicked == null ) return;

      if( this.GameStatus_CheckIfBusy() )
      {
        MessageBox.Show( Language.Error_EventLoopBusy,
                         "Battleground Europe Game Monitor", MessageBoxButtons.OK, MessageBoxIcon.Information );
        return;
      }

      btnCheckEvents.Enabled = false;
      tmrEnableNewEvents.Start();

      this.CheckEventsClicked( null, null );
    }
    private void btnResetAllData_Click( object sender, EventArgs e )
    {
      if( this.ResetAllDataClicked == null ) return;

      if( this.GameStatus_CheckIfBusy() )
      {
        MessageBox.Show( Language.Error_EventLoopBusy,
                         "Battleground Europe Game Monitor", MessageBoxButtons.OK, MessageBoxIcon.Information );
        return;
      }

      this.Close();
      this.ResetAllDataClicked( null, null );
    }

    // re-enable timers
    private void tmrEnableNewEvents_Tick( object sender, EventArgs e )
    {
      btnCheckEvents.Enabled = true;
      tmrEnableNewEvents.Stop();
    }

    #endregion

    #endregion
  }

  #region Settings Classes

  /// <summary>
  /// Contains network-related settings (more specific version of Xiperware.WW2Online.NetManager's NetworkSettings).
  /// </summary>
  public class NetworkSettings : Xiperware.WiretapAPI.NetworkSettings
  {
    #region Variables

    #endregion

    #region Constructors
    
    /// <summary>
    /// Create a new NetworkSettings object with default values.
    /// </summary>
    public NetworkSettings()
      : base()
    {

    }

    /// <summary>
    /// Create a new NetworkSettings object with the given values.
    /// </summary>
    public NetworkSettings( string wiretapHost, string wiretapPort, bool useCustomProxy, string proxyHost, string proxyPort )
      : base( wiretapHost, wiretapPort, useCustomProxy, proxyHost, proxyPort )
    {

    }
    
    #endregion

    #region Properties

    #endregion

    #region Methods

    /// <summary>
    /// Populate this object with values stored in the registry.
    /// </summary>
    public void LoadFromRegistry()
    {
      RegistryKey key = null;

      try
      {
        key = Registry.CurrentUser.OpenSubKey( "Software\\BEGameMonitor\\Options\\Network" );
        if( key == null ) return;  // doesn't exist, not an error

        string wiretapHost = (string)key.GetValue( "WiretapHost", "" );
        string wiretapPort = (string)key.GetValue( "WiretapPort", "" );

        if( wiretapHost != "" )
        {
          this.wiretapHost = wiretapHost;
          this.wiretapPort = wiretapPort;
        }

        string useCustomProxy = (string)key.GetValue( "UseCustomProxy", "" );
        string proxyHost = (string)key.GetValue( "ProxyHost", "" );
        string proxyPort = (string)key.GetValue( "ProxyPort", "" );

        if( useCustomProxy != "" && proxyHost != "" )
        {
          this.useCustomProxy = useCustomProxy == "True";
          this.proxyHost = proxyHost;
          this.proxyPort = proxyPort;
        }
      }
      catch( Exception ex )
      {
        Log.AddError( "WARNING: Failed to load Network settings: {0}", ex.Message );
      }
      finally
      {
        if( key != null ) key.Close();
      }

      if( this.wiretapHost == "web3.wwiionline.com" )  // convert old default url to new default
      {
        this.wiretapHost = "wiretap.wwiionline.com";
        this.SaveToRegistry( false );
      }
    }

    /// <summary>
    /// Save the values in this object to the registry.
    /// </summary>
    /// <param name="showError">Display a message box if an error occurs.</param>
    public void SaveToRegistry( bool showError )
    {
      RegistryKey key = null;

      try
      {
        key = Registry.CurrentUser.CreateSubKey( "Software\\BEGameMonitor\\Options\\Network" );
        if( key == null )
          throw new ApplicationException( "Failed to create registry key." );

        key.SetValue( "WiretapHost", this.wiretapHost );
        key.SetValue( "WiretapPort", this.wiretapPort );
        key.SetValue( "UseCustomProxy", this.useCustomProxy );
        key.SetValue( "ProxyHost", this.proxyHost );
        key.SetValue( "ProxyPort", this.proxyPort );
      }
      catch( Exception ex )
      {
        Log.AddError( "ERROR: Failed to save Network settings: {0}", ex.Message );
        if( showError )
          MessageBox.Show( Language.Error_SaveNetwork + ":\n\n" + ex.Message, Language.Error_Error, MessageBoxButtons.OK, MessageBoxIcon.Error );
      }
      finally
      {
        if( key != null ) key.Close();
      }
    }

    /// <summary>
    /// Tests if the given NetworkSettings are functionally the same (ie, ignoring unused values).
    /// </summary>
    /// <param name="other">The NetworkSettings to test.</param>
    /// <returns>True if the relevant parts are identical.</returns>
    public bool Equivalent( NetworkSettings other )
    {
      if( this.wiretapHost != other.wiretapHost
        || this.wiretapPort != other.wiretapPort
        || this.useCustomProxy != other.useCustomProxy )
        return false;

      if( this.useCustomProxy )  // only check custom proxy equality if we're using them
        if( this.proxyHost != other.proxyHost || this.proxyPort != other.proxyPort )
          return false;

      return true;
    }

    /// <summary>
    /// Provides a string representation of this object.
    /// </summary>
    /// <returns>"serverhost[:port] (no proxy|via IE proxy|via proxyhost[:port])"</returns>
    public override string ToString()
    {
      string server = this.wiretapHost;
      if( this.wiretapPort != "" )
        server += ":" + this.wiretapPort;

      string proxy = "IE proxy";
      if( this.useCustomProxy )
      {
        if( this.proxyHost == "" )
          proxy = "no proxy";
        else
        {
          proxy = "via " + this.proxyHost;
          if( this.proxyPort != "" )
            proxy += ":" + this.proxyPort;
        }
      }

      return String.Format( "{0} ({1})", server, proxy );
    }

    /// <summary>
    /// Returns a value indicating whether this instance is equal to a specified object value.
    /// </summary>
    /// <param name="obj">An object to compare with this instance.</param>
    /// <returns>True if obj is an equivalent NetworkSettings.</returns>
    public override bool Equals( object obj )
    {
      if( !( obj is NetworkSettings ) ) return false;
      NetworkSettings other = (NetworkSettings)obj;

      if( this.wiretapHost    != other.wiretapHost    ) return false;
      if( this.wiretapPort    != other.wiretapPort    ) return false;
      if( this.useCustomProxy != other.useCustomProxy ) return false;
      if( this.proxyHost      != other.proxyHost      ) return false;
      if( this.proxyPort      != other.proxyPort      ) return false;

      return true;
    }

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    /// <returns>Object.GetHashCode().</returns>
    public override int GetHashCode()
    {
      return base.GetHashCode();
    }

    /// <summary>
    /// Support for the == operator.
    /// </summary>
    public static bool operator ==( NetworkSettings a, NetworkSettings b )
    {
      if( (object)a == null ) return (object)b == null;
      return a.Equals( b );
    }

    /// <summary>
    /// Support for the != operator.
    /// </summary>
    public static bool operator !=( NetworkSettings a, NetworkSettings b )
    {
      if( (object)a == null ) return (object)b != null;
      return !a.Equals( b );
    }

    #endregion
  }


  /// <summary>
  /// Contains settings related to program startup.
  /// </summary>
  public class StartupSettings
  {
    #region Variables

    public bool runOnStartup = false;
    public bool startMinimised = false;
    public bool sleepWhenIdle = true;
    public int idleTimeout = 3;
    public bool sleepWhenPlay = false;
    public bool wakeAfterPlay = true;
    public bool loadFactoryData = false;
    public bool checkVersion = true;

    #endregion

    #region Constructors

    /// <summary>
    /// Create a new StartupSettings object with default values.
    /// </summary>
    public StartupSettings()
    {

    }

    /// <summary>
    /// Create a new StartupSettings object with the given values.
    /// </summary>
    public StartupSettings( bool runOnStartup, bool startMinimised, bool sleepWhenIdle, int sleepTimeout,
      bool sleepWhilePlaying, bool wakeAfterPlaying, bool loadFactoryData, bool checkVersion )
    {
      this.runOnStartup = runOnStartup;
      this.startMinimised = startMinimised;
      this.sleepWhenIdle = sleepWhenIdle;
      this.idleTimeout = sleepTimeout;
      this.sleepWhenPlay = sleepWhilePlaying;
      this.wakeAfterPlay = wakeAfterPlaying;
      this.loadFactoryData = loadFactoryData;
      this.checkVersion = checkVersion;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Populate this object with values stored in the registry.
    /// </summary>
    public void LoadFromRegistry()
    {
      RegistryKey key = null;

      try
      {
        key = Registry.CurrentUser.OpenSubKey( "Software\\BEGameMonitor\\Options\\Startup" );
        if( key != null )
        {
          string startMinimised = (string)key.GetValue( "StartMinimised", "" );
          if( startMinimised != "" ) this.startMinimised = startMinimised == "True";

          string sleepWhenIdle = (string)key.GetValue( "SleepWhenIdle", "" );
          if( sleepWhenIdle != "" ) this.sleepWhenIdle = sleepWhenIdle == "True";

          int idleTimeout = (int)key.GetValue( "IdleTimeout", 0 );
          if( idleTimeout != 0 ) this.idleTimeout = idleTimeout;

          string sleepWhenPlay = (string)key.GetValue( "SleepWhenPlay", "" );
          if( sleepWhenPlay != "" ) this.sleepWhenPlay = sleepWhenPlay == "True";

          string wakeAfterPlay = (string)key.GetValue( "WakeAfterPlay", "" );
          if( wakeAfterPlay != "" ) this.wakeAfterPlay = wakeAfterPlay == "True";

          string loadFactoryData = (string)key.GetValue( "LoadFactoryData", "" );
          if( loadFactoryData != "" ) this.loadFactoryData = loadFactoryData == "True";

          string checkVersion = (string)key.GetValue( "CheckVersion", "" );
          if( checkVersion != "" ) this.checkVersion = checkVersion == "True";

          key.Close();
        }


        key = Registry.CurrentUser.OpenSubKey( "Software\\Microsoft\\Windows\\CurrentVersion\\Run" );
        if( key != null )
        {
          string startupPath = (string)key.GetValue( "BEGameMonitor", "" );
          this.runOnStartup = startupPath.Contains( Application.ExecutablePath );

          key.Close();
        }
      }
      catch( Exception ex )
      {
        Log.AddError( "WARNING: Failed to load Startup settings: {0}", ex.Message );
      }
      finally
      {
        if( key != null ) key.Close();
      }
    }

    /// <summary>
    /// Save the values in this object to the registry.
    /// </summary>
    /// <param name="showError">Display a message box if an error occurs.</param>
    public void SaveToRegistry( bool showError )
    {
      RegistryKey key = null;

      try
      {
        key = Registry.CurrentUser.CreateSubKey( "Software\\BEGameMonitor\\Options\\Startup" );
        if( key == null )
          throw new ApplicationException( "Failed to create registry key." );

        key.SetValue( "StartMinimised", this.startMinimised );
        key.SetValue( "SleepWhenIdle", this.sleepWhenIdle );
        key.SetValue( "IdleTimeout", this.idleTimeout );
        key.SetValue( "SleepWhenPlay", this.sleepWhenPlay );
        key.SetValue( "WakeAfterPlay", this.wakeAfterPlay );
        key.SetValue( "LoadFactoryData", this.loadFactoryData );
        key.SetValue( "CheckVersion", this.checkVersion );

        key.Close();


        key = Registry.CurrentUser.OpenSubKey( "Software\\Microsoft\\Windows\\CurrentVersion\\Run", true );
        if( key == null )
          throw new ApplicationException( "Failed to open system run registry key." );

        if( this.runOnStartup )
          key.SetValue( "BEGameMonitor", String.Format( "\"{0}\" sleep autowakeup", Application.ExecutablePath ) );
        else
          key.DeleteValue( "BEGameMonitor", false );

        key.Close();
      }
      catch( Exception ex )
      {
        Log.AddError( "ERROR: Failed to save Startup settings: {0}", ex.Message );
        if( showError )
          MessageBox.Show( Language.Error_SaveStartup + ":\n\n" + ex.Message, Language.Error_Error, MessageBoxButtons.OK, MessageBoxIcon.Error );
      }
      finally
      {
        if( key != null ) key.Close();
      }
    }

    /// <summary>
    /// Returns a value indicating whether this instance is equal to a specified object value.
    /// </summary>
    /// <param name="obj">An object to compare with this instance.</param>
    /// <returns>True if obj is an equivalent StartupSettings.</returns>
    public override bool Equals( object obj )
    {
      if( !( obj is StartupSettings ) ) return false;
      StartupSettings other = (StartupSettings)obj;

      if( this.runOnStartup    != other.runOnStartup    ) return false;
      if( this.startMinimised  != other.startMinimised  ) return false;
      if( this.sleepWhenIdle   != other.sleepWhenIdle   ) return false;
      if( this.idleTimeout     != other.idleTimeout     ) return false;
      if( this.sleepWhenPlay   != other.sleepWhenPlay   ) return false;
      if( this.wakeAfterPlay   != other.wakeAfterPlay   ) return false;
      if( this.loadFactoryData != other.loadFactoryData ) return false;
      if( this.checkVersion    != other.checkVersion    ) return false;

      return true;
    }

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    /// <returns>Object.GetHashCode().</returns>
    public override int GetHashCode()
    {
      return base.GetHashCode();
    }

    /// <summary>
    /// Support for the == operator.
    /// </summary>
    public static bool operator ==( StartupSettings a, StartupSettings b )
    {
      if( (object)a == null ) return (object)b == null;
      return a.Equals( b );
    }

    /// <summary>
    /// Support for the != operator.
    /// </summary>
    public static bool operator !=( StartupSettings a, StartupSettings b )
    {
      if( (object)a == null ) return (object)b != null;
      return !a.Equals( b );
    }

    #endregion
  }


  /// <summary>
  /// Contains settings related to the display of game event alerts.
  /// </summary>
  public class AlertSettings
  {
    #region Variables

    public bool showAlerts = true;

    public DockStyle alertPosition = DockStyle.Bottom;
    public int autoNextTime = 5;
    public bool playSound = false;
    public bool postponeFullscreen = true;
    public bool postponeIdle = true;
    public int postponeIdleTime = 20;

    public bool alwaysAlertUnderAttack = true;
    public bool alwaysAlertChokePointCaptured = true;

    public bool filterEventType = false;
    public bool filterChokePoint = true;
    public bool filterCountry = false;

    public List<int> filterEventTypeIDs = new List<int>();
    public List<int> filterChokePointIDs = new List<int>();
    public List<int> filterCountryIDs = new List<int>();

    public bool filterCapturesChokePointCaptured = false;
    public bool filterCapturesChokePointControlChanged = false;
    public bool filterCapturesChokePointContestedUnderAttack = false;
    public bool filterCapturesChokePointContestedRegained = false;
    public bool filterCapturesFacilityCaptured = false;
    public bool filterCapturesFacilityRecaptured = false;
    public bool filterCapturesFacilitySpawnableCaptured = false;
    public bool filterCapturesFacilitySpawnableRecaptured = false;
    public bool filterAttackObjectivePlaced = false;
    public bool filterAttackObjectiveWithdrawn = false;
    public bool filterFirebaseAlliedBlown = false;
    public bool filterFirebaseAxisBlown = false;
    public bool filterFirebaseNewBrigade = false;
    public bool filterHCUnitDeployed = false;
    public bool filterHCUnitMoved = false;
    public bool filterHCUnitRetreated = false;
    public bool filterHCUnitRouted = false;
    public bool filterFactoryHealthDamaged = false;
    public bool filterFactoryHealthDestroyed = false;
    public bool filterFactoryHealthRepaired = false;
    public bool filterFactoryRdpHalted = false;
    public bool filterFactoryRdpResumed = false;

    public Dictionary<int, bool> filterChokePointIDLookup = new Dictionary<int,bool>();

    public bool filterCountryEngland = false;
    public bool filterCountryFrance = false;
    public bool filterCountryGerman = false;

    #endregion

    #region Constructors

    /// <summary>
    /// Create a new AlertSettings object with default values.
    /// </summary>
    public AlertSettings()
    {

    }

    /// <summary>
    /// Create a new AlertSettings object with the given values.
    /// </summary>
    public AlertSettings( bool showAlerts, DockStyle alertPosition, int autoNextTime, bool playSound, bool postponeFullscreen, bool postponeIdle, int postponeIdleTime,
      bool alwaysAlertUnderAttack, bool alwaysAlertChokePointCaptured, bool filterEventType, bool filterChokePoint, bool filterCountry,
      List<int> filterEventTypeIDs, List<int> filterChokePointIDs, List<int> filterCountryIDs )
    {
      this.showAlerts = showAlerts;

      this.alertPosition = alertPosition;
      this.autoNextTime = autoNextTime;
      this.playSound = playSound;
      this.postponeFullscreen = postponeFullscreen;
      this.postponeIdle = postponeIdle;
      this.postponeIdleTime = postponeIdleTime;

      this.alwaysAlertUnderAttack = alwaysAlertUnderAttack;
      this.alwaysAlertChokePointCaptured = alwaysAlertChokePointCaptured;

      this.filterEventType = filterEventType;
      this.filterChokePoint = filterChokePoint;
      this.filterCountry = filterCountry;

      this.filterEventTypeIDs = filterEventTypeIDs;
      this.filterChokePointIDs = filterChokePointIDs;
      this.filterCountryIDs = filterCountryIDs;

      RegenerateFlags();
    }

    #endregion

    #region Methods

    /// <summary>
    /// Populate this object with values stored in the registry.
    /// </summary>
    public void LoadFromRegistry()
    {
      RegistryKey key = null;

      try
      {
        key = Registry.CurrentUser.OpenSubKey( "Software\\BEGameMonitor\\Options\\Alerts" );
        if( key == null ) return;  // doesn't exist, not an error

        string showAlerts = (string)key.GetValue( "ShowAlerts", "" );
        if( showAlerts != "" ) this.showAlerts = showAlerts == "True";

        string alertPosition = (string)key.GetValue( "AlertPosition", "" );
        if( alertPosition != "" ) this.alertPosition = alertPosition == "Top" ? DockStyle.Top : DockStyle.Bottom;

        int autoNextTime = (int)key.GetValue( "AutoNextTime", -1 );
        if( autoNextTime != -1 ) this.autoNextTime = autoNextTime;

        string playSound = (string)key.GetValue( "PlayAlertSound", "" );
        if( playSound != "" ) this.playSound = playSound == "True";

        string postponeFullscreen = (string)key.GetValue( "PostponeFullscreen", "" );
        if( postponeFullscreen != "" ) this.postponeFullscreen = postponeFullscreen == "True";

        string postponeIdle = (string)key.GetValue( "PostponeIdle", "" );
        if( postponeIdle != "" ) this.postponeIdle = postponeIdle == "True";

        int postponeIdleTime = (int)key.GetValue( "PostponeIdleTime", -1 );
        if( postponeIdleTime != -1 ) this.postponeIdleTime = postponeIdleTime;

        string alwaysAlertUnderAttack = (string)key.GetValue( "AlwaysAlertUnderAttack", "" );
        if( alwaysAlertUnderAttack != "" ) this.alwaysAlertUnderAttack = alwaysAlertUnderAttack == "True";

        string alwaysAlertChokePointCaptured = (string)key.GetValue( "AlwaysAlertChokePointCaptured", "" );
        if( alwaysAlertChokePointCaptured != "" ) this.alwaysAlertChokePointCaptured = alwaysAlertChokePointCaptured == "True";

        string filterEventType = (string)key.GetValue( "FilterEventType", "" );
        if( filterEventType != "" ) this.filterEventType = filterEventType == "True";

        string filterChokePoint = (string)key.GetValue( "FilterChokePoint", "" );
        if( filterChokePoint != "" ) this.filterChokePoint = filterChokePoint == "True";

        string filterCountry = (string)key.GetValue( "FilterCountry", "" );
        if( filterCountry != "" ) this.filterCountry = filterCountry == "True";

        string filterEventTypeIDs = (string)key.GetValue( "FilterEventTypeIDs", "" );
        if( filterEventTypeIDs != "" ) this.filterEventTypeIDs = filterEventTypeIDs.SplitRegex<int>();

        string filterChokePointIDs = (string)key.GetValue( "FilterChokePointIDs", "" );
        if( filterChokePointIDs != "" ) this.filterChokePointIDs = filterChokePointIDs.SplitRegex<int>();

        string filterCountryIDs = (string)key.GetValue( "FilterCountryIDs", "" );
        if( filterCountryIDs != "" ) this.filterCountryIDs = filterCountryIDs.SplitRegex<int>();
      }
      catch( Exception ex )
      {
        Log.AddError( "WARNING: Failed to load Alert settings: {0}", ex.Message );
      }
      finally
      {
        if( key != null ) key.Close();
      }

      RegenerateFlags();
    }

    /// <summary>
    /// Save the values in this object to the registry.
    /// </summary>
    /// <param name="showError">Display a message box if an error occurs.</param>
    public void SaveToRegistry( bool showError )
    {
      RegistryKey key = null;

      try
      {
        key = Registry.CurrentUser.CreateSubKey( "Software\\BEGameMonitor\\Options\\Alerts" );
        if( key == null )
          throw new ApplicationException( "Failed to create registry key." );

        key.SetValue( "ShowAlerts", this.showAlerts );

        key.SetValue( "AlertPosition", this.alertPosition );
        key.SetValue( "AutoNextTime", this.autoNextTime );
        key.SetValue( "PlayAlertSound", this.playSound );
        key.SetValue( "PostponeFullscreen", this.postponeFullscreen );
        key.SetValue( "PostponeIdle", this.postponeIdle );
        key.SetValue( "PostponeIdleTime", this.postponeIdleTime );

        key.SetValue( "AlwaysAlertUnderAttack", this.alwaysAlertUnderAttack );
        key.SetValue( "AlwaysAlertChokePointCaptured", this.alwaysAlertChokePointCaptured );

        key.SetValue( "FilterEventType", this.filterEventType );
        key.SetValue( "FilterChokePoint", this.filterChokePoint );
        key.SetValue( "FilterCountry", this.filterCountry );

        key.SetValue( "FilterEventTypeIDs", this.filterEventTypeIDs.Join() );
        key.SetValue( "FilterChokePointIDs", this.filterChokePointIDs.Join() );
        key.SetValue( "FilterCountryIDs", this.filterCountryIDs.Join() );
      }
      catch( Exception ex )
      {
        Log.AddError( "ERROR: Failed to save Alert settings: {0}", ex.Message );
        if( showError )
          MessageBox.Show( Language.Error_SaveAlert + ":\n\n" + ex.Message, Language.Error_Error, MessageBoxButtons.OK, MessageBoxIcon.Error );
      }
      finally
      {
        if( key != null ) key.Close();
      }
    }

    /// <summary>
    /// Updates the set of boolean flags for the event-type &amp; country filters
    /// with the values in the id lists, and generates the cp id lookup hash.
    /// </summary>
    /// <remarks>Assumes all the flags are set to false.</remarks>
    public void RegenerateFlags()
    {
      // event types

      foreach( int id in this.filterEventTypeIDs )
      {
        switch( id )
        {
          case 1:  this.filterCapturesChokePointCaptured             = true;  break;
          case 2:  this.filterCapturesChokePointControlChanged       = true;  break;
          case 3:  this.filterCapturesChokePointContestedUnderAttack = true;  break;
          case 4:  this.filterCapturesChokePointContestedRegained    = true;  break;
          case 5:  this.filterCapturesFacilityCaptured               = true;  break;
          case 6:  this.filterCapturesFacilityRecaptured             = true;  break;
          case 7:  this.filterCapturesFacilitySpawnableCaptured      = true;  break;
          case 8:  this.filterCapturesFacilitySpawnableRecaptured    = true;  break;
          case 9:  this.filterAttackObjectivePlaced                  = true;  break;
          case 10: this.filterAttackObjectiveWithdrawn               = true;  break;
          case 11: this.filterFirebaseAlliedBlown                    = true;  break;
          case 12: this.filterFirebaseAxisBlown                      = true;  break;
          case 13: this.filterHCUnitDeployed                         = true;  break;
          case 14: this.filterHCUnitMoved                            = true;  break;
          case 15: this.filterHCUnitRetreated                        = true;  break;
          case 16: this.filterHCUnitRouted                           = true;  break;
          case 17: this.filterFactoryHealthDamaged                   = true;  break;
          case 18: this.filterFactoryHealthDestroyed                 = true;  break;
          case 19: this.filterFactoryHealthRepaired                  = true;  break;
          case 20: this.filterFactoryRdpHalted                       = true;  break;
          case 21: this.filterFactoryRdpResumed                      = true;  break;
          case 22: this.filterFirebaseNewBrigade                     = true;  break;
        }
      }


      // chokepoint

      // convert filterChokePointIDs to a lookup hash
      // (retrieval close to O(1), rather than doing a linear search of a list in O(n))

      filterChokePointIDLookup = new Dictionary<int, bool>();
      foreach( int i in this.filterChokePointIDs )
        filterChokePointIDLookup.Add( i, true );


      // countrys

      foreach( int id in this.filterCountryIDs )
      {
        switch( id )
        {
          case 1: this.filterCountryEngland = true;  break;
          case 3: this.filterCountryFrance  = true;  break;
          case 4: this.filterCountryGerman  = true;  break;
        }
      }
    }

    /// <summary>
    /// Provides a string representation of this object.
    /// </summary>
    /// <returns>eg, "bottom, 5 secs, sound"</returns>
    public override string ToString()
    {
      return String.Format( "{0}, {1} {2}{3}",
                            this.alertPosition.ToString().ToLower(),
                            this.autoNextTime,
                            this.autoNextTime == 1 ? Language.Time_Sec : Language.Time_Secs,
                            this.playSound ? ", sound" : null );
    }

    /// <summary>
    /// Returns a value indicating whether this instance is equal to a specified object value.
    /// </summary>
    /// <param name="obj">An object to compare with this instance.</param>
    /// <returns>True if obj is an equivalent StartupSettings.</returns>
    public override bool Equals( object obj )
    {
      if( !( obj is AlertSettings ) ) return false;
      AlertSettings other = (AlertSettings)obj;

      if( this.showAlerts                    != other.showAlerts                    ) return false;
      if( this.alertPosition                 != other.alertPosition                 ) return false;
      if( this.playSound                     != other.playSound                     ) return false;
      if( this.postponeFullscreen            != other.postponeFullscreen            ) return false;
      if( this.postponeIdle                  != other.postponeIdle                  ) return false;
      if( this.postponeIdleTime              != other.postponeIdleTime              ) return false;
      if( this.alwaysAlertUnderAttack        != other.alwaysAlertUnderAttack        ) return false;
      if( this.alwaysAlertChokePointCaptured != other.alwaysAlertChokePointCaptured ) return false;
      if( this.filterEventType               != other.filterEventType               ) return false;
      if( this.filterChokePoint              != other.filterChokePoint              ) return false;
      if( this.filterCountry                 != other.filterCountry                 ) return false;
      if( this.filterEventTypeIDs.Join()     != other.filterEventTypeIDs.Join()     ) return false;
      if( this.filterChokePointIDs.Join()    != other.filterChokePointIDs.Join()    ) return false;
      if( this.filterCountryIDs.Join()       != other.filterCountryIDs.Join()       ) return false;

      return true;
    }

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    /// <returns>Object.GetHashCode().</returns>
    public override int GetHashCode()
    {
      return base.GetHashCode();
    }

    /// <summary>
    /// Support for the == operator.
    /// </summary>
    public static bool operator ==( AlertSettings a, AlertSettings b )
    {
      if( (object)a == null ) return (object)b == null;
      return a.Equals( b );
    }

    /// <summary>
    /// Support for the != operator.
    /// </summary>
    public static bool operator !=( AlertSettings a, AlertSettings b )
    {
      if( (object)a == null ) return (object)b != null;
      return !a.Equals( b );
    }

    #endregion
  }


  /// <summary>
  /// Contains settings related to game events.
  /// </summary>
  public class MiscSettings
  {
    #region Variables

    public DockStyle dockWindow = DockStyle.Right;
    public SortOrder eventSortOrder = SortOrder.Ascending;
    public int gameStatusDisplay = 0;  // 0 = detect primary, 1 = use first (Screen.AllScreens[0]), etc.
    public int alertDisplay = 0;

    #endregion

    #region Constructors

    /// <summary>
    /// Create a new MiscSettings object with default values.
    /// </summary>
    public MiscSettings()
    {

    }

    /// <summary>
    /// Create a new MiscSettings object with the given values.
    /// </summary>
    public MiscSettings( DockStyle dockWindow, int gameStatusDisplay, SortOrder eventSortOrder, int alertDisplay )
    {
      this.dockWindow = dockWindow;
      this.gameStatusDisplay = gameStatusDisplay;
      this.eventSortOrder = eventSortOrder;
      this.alertDisplay = alertDisplay;
    }

    #endregion

    #region Properties

    /// <summary>
    /// The display index of Screen.AllScreens on which to show the game status window.
    /// Is bounds-checked on access in case monitor configuration has changed.
    /// </summary>
    public int GameStatusDisplayIndex
    {
      get
      {
        if( this.gameStatusDisplay > 0 && this.gameStatusDisplay <= Screen.AllScreens.Length )
          return this.gameStatusDisplay - 1;

        // out of range, reset to primary display

        int primary = 1;
        for( int i = 0; i < Screen.AllScreens.Length; i++ )
          if( Screen.AllScreens[i].Primary )
            primary = i + 1;
        this.gameStatusDisplay = primary;

        return this.gameStatusDisplay - 1;
      }
    }

    /// <summary>
    /// The display index of Screen.AllScreens on which to show the alert window.
    /// Is bounds-checked on access in case monitor configuration has changed.
    /// </summary>
    public int AlertDisplayIndex
    {
      get
      {
        if( this.alertDisplay > 0 && this.alertDisplay <= Screen.AllScreens.Length )
          return this.alertDisplay - 1;

        // out of range, reset to primary display

        int primary = 1;
        for( int i = 0; i < Screen.AllScreens.Length; i++ )
          if( Screen.AllScreens[i].Primary )
            primary = i + 1;
        this.alertDisplay = primary;

        return this.alertDisplay - 1;
      }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Populate this object with values stored in the registry.
    /// </summary>
    public void LoadFromRegistry()
    {
      try
      {
        Registry.CurrentUser.DeleteSubKeyTree( "Software\\BEGameMonitor\\Options\\Events" );  // old tree from < v0.9.1
      }
      catch { }  // ignore if not found


      RegistryKey key = null;

      try
      {
        key = Registry.CurrentUser.OpenSubKey( "Software\\BEGameMonitor\\Options\\Misc" );
        if( key == null ) return;  // doesn't exist, not an error

        string dockWindow = (string)key.GetValue( "DockWindow", "" );
        if( dockWindow != "" )
        {
          if     ( dockWindow == "None" ) this.dockWindow = DockStyle.None;
          else if( dockWindow == "Left" ) this.dockWindow = DockStyle.Left;
          else                            this.dockWindow = DockStyle.Right;
        }

        string eventSortOrder = (string)key.GetValue( "SortOrder", "" );
        if( eventSortOrder != "" ) this.eventSortOrder = eventSortOrder == "Descending" ? SortOrder.Descending : SortOrder.Ascending;

        int gameStatusDisplay = (int)key.GetValue( "GameStatusDisplay", -1 );
        if( gameStatusDisplay != -1 ) this.gameStatusDisplay = gameStatusDisplay;

        int alertDisplay = (int)key.GetValue( "AlertDisplay", -1 );
        if( alertDisplay != -1 ) this.alertDisplay = alertDisplay;
      }
      catch( Exception ex )
      {
        Log.AddError( "WARNING: Failed to load Misc settings: {0}", ex.Message );
      }
      finally
      {
        if( key != null ) key.Close();
      }
    }

    /// <summary>
    /// Save the values in this object to the registry.
    /// </summary>
    /// <param name="showError">Display a message box if an error occurs.</param>
    public void SaveToRegistry( bool showError )
    {
      RegistryKey key = null;

      try
      {
        key = Registry.CurrentUser.CreateSubKey( "Software\\BEGameMonitor\\Options\\Misc" );
        if( key == null )
          throw new ApplicationException( "Failed to create registry key." );

        key.SetValue( "DockWindow", this.dockWindow );
        key.SetValue( "SortOrder", this.eventSortOrder );
        key.SetValue( "GameStatusDisplay", this.gameStatusDisplay );
        key.SetValue( "AlertDisplay", this.alertDisplay );
      }
      catch( Exception ex )
      {
        Log.AddError( "ERROR: Failed to save Misc settings: {0}", ex.Message );
        if( showError )
          MessageBox.Show( Language.Error_SaveMisc + ":\n\n" + ex.Message, Language.Error_Error, MessageBoxButtons.OK, MessageBoxIcon.Error );
      }
      finally
      {
        if( key != null ) key.Close();
      }
    }

    /// <summary>
    /// Returns a value indicating whether this instance is equal to a specified object value.
    /// </summary>
    /// <param name="obj">An object to compare with this instance.</param>
    /// <returns>True if obj is an equivalent MiscSettings.</returns>
    public override bool Equals( object obj )
    {
      if( !( obj is MiscSettings ) ) return false;
      MiscSettings other = (MiscSettings)obj;

      if( this.dockWindow        != other.dockWindow        ) return false;
      if( this.eventSortOrder    != other.eventSortOrder    ) return false;
      if( this.gameStatusDisplay != other.gameStatusDisplay ) return false;
      if( this.alertDisplay      != other.alertDisplay      ) return false;

      return true;
    }

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    /// <returns>Object.GetHashCode().</returns>
    public override int GetHashCode()
    {
      return base.GetHashCode();
    }

    /// <summary>
    /// Support for the == operator.
    /// </summary>
    public static bool operator ==( MiscSettings a, MiscSettings b )
    {
      if( (object)a == null ) return (object)b == null;
      return a.Equals( b );
    }

    /// <summary>
    /// Support for the != operator.
    /// </summary>
    public static bool operator !=( MiscSettings a, MiscSettings b )
    {
      if( (object)a == null ) return (object)b != null;
      return !a.Equals( b );
    }

    #endregion
  }


  /// <summary>
  /// Contains settings related to the game map widget.
  /// </summary>
  public class MapSettings
  {
    #region Variables

    public int mapSize = 40;
    public bool useDefaultWhenPlaying = true;

    public bool showWallpaper = false;
    public DrawOverlayParams wallOverlayParams = new DrawOverlayParams();
    public int wallZoom = 0;  // 0 = min (scale to fit), 6 = max (map40 @ 100%)
    public int wallUpdate = 5;  // minutes
    public bool wallRemove = true;

    #endregion

    #region Constructors

    /// <summary>
    /// Create a new MapSettings object with default values.
    /// </summary>
    public MapSettings()
    {
      // update defaults
      this.wallOverlayParams.showAttackObjectiveNames = true;
      this.wallOverlayParams.showBrigadeLinks = false;
      this.wallOverlayParams.showBrigadeLinksSelected = false;
    }

    /// <summary>
    /// Create a new MiscSettings object with the given values.
    /// </summary>
    public MapSettings( bool useDefaultWhenPlaying, int mapSize, bool showWallpaper, DrawOverlayParams wallOverlayParams,
      int wallZoom, int wallUpdate, bool wallRemove )
    {
      this.useDefaultWhenPlaying = useDefaultWhenPlaying;
      this.mapSize = mapSize;

      this.showWallpaper = showWallpaper;
      this.wallOverlayParams = wallOverlayParams;
      this.wallZoom = wallZoom;
      this.wallUpdate = wallUpdate;
      this.wallRemove = wallRemove;
    }

    #endregion

    #region Properties

    /// <summary>
    /// The current params based on the map settings.
    /// </summary>
    public DrawOverlayParams OverlayParams
    {
      get { return this.wallOverlayParams; }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Populate this object with values stored in the registry.
    /// </summary>
    public void LoadFromRegistry()
    {
      RegistryKey key = null;

      try
      {
        key = Registry.CurrentUser.OpenSubKey( "Software\\BEGameMonitor\\Options\\Map" );
        if( key == null ) return;  // doesn't exist, not an error

        string useDefaultWhenPlaying = (string)key.GetValue( "UseDefaultWhenPlaying", "" );
        if( useDefaultWhenPlaying != "" ) this.useDefaultWhenPlaying = useDefaultWhenPlaying != "False";

        int mapSize = (int)key.GetValue( "MapSize", -1 );
        if( mapSize != -1 ) this.mapSize = mapSize;

        string showWallpaper = (string)key.GetValue( "ShowWallpaper", "" );
        if( showWallpaper != "" ) this.showWallpaper = showWallpaper != "False";

        int wallZoom = (int)key.GetValue( "WallZoom", -1 );
        if( wallZoom != -1 ) this.wallZoom = wallZoom;

        int wallUpdate = (int)key.GetValue( "WallUpdate", -1 );
        if( wallUpdate != -1 ) this.wallUpdate = wallUpdate;

        string wallRemove = (string)key.GetValue( "WallRemove", "" );
        if( wallRemove != "" ) this.wallRemove = wallRemove == "True";

        key.Close();


        key = Registry.CurrentUser.OpenSubKey( "Software\\BEGameMonitor\\Options\\Map\\Wallpaper" );
        if( key == null ) return;  // doesn't exist, not an error

        this.wallOverlayParams.LoadFromRegistry( key );

        key.Close();
      }
      catch( Exception ex )
      {
        Log.AddError( "WARNING: Failed to load Map settings: {0}", ex.Message );
      }
      finally
      {
        if( key != null ) key.Close();
      }
    }

    /// <summary>
    /// Save the values in this object to the registry.
    /// </summary>
    /// <param name="showError">Display a message box if an error occurs.</param>
    public void SaveToRegistry( bool showError )
    {
      RegistryKey key = null;

      try
      {
        key = Registry.CurrentUser.CreateSubKey( "Software\\BEGameMonitor\\Options\\Map" );
        if( key == null )
          throw new ApplicationException( "Failed to create registry key." );

        key.SetValue( "UseDefaultWhenPlaying", this.useDefaultWhenPlaying );
        key.SetValue( "MapSize", this.mapSize );
        key.SetValue( "ShowWallpaper", this.showWallpaper );
        key.SetValue( "WallZoom", this.wallZoom );
        key.SetValue( "WallUpdate", this.wallUpdate );
        key.SetValue( "WallRemove", this.wallRemove );
        key.Close();


        key = Registry.CurrentUser.CreateSubKey( "Software\\BEGameMonitor\\Options\\Map\\Wallpaper" );
        if( key == null )
          throw new ApplicationException( "Failed to create wallpaper registry sub-key." );

        this.wallOverlayParams.SaveToRegistry( key );

        key.Close();
      }
      catch( Exception ex )
      {
        Log.AddError( "ERROR: Failed to save Map settings: {0}", ex.Message );
        if( showError )
          MessageBox.Show( Language.Error_SaveMap + ":\n\n" + ex.Message, Language.Error_Error, MessageBoxButtons.OK, MessageBoxIcon.Error );
      }
      finally
      {
        if( key != null ) key.Close();
      }
    }

    /// <summary>
    /// Tests whether the given wallpaper settings are equivalent.
    /// </summary>
    /// <param name="other">The object to compare.</param>
    /// <returns>False if changes exist that require an update.</returns>
    public bool WallpaperEquivalent( MapSettings other )
    {
      if( this.showWallpaper != other.showWallpaper ) return false;
      if( !this.wallOverlayParams.IsEquivalent( other.wallOverlayParams ) ) return false;
      if( this.wallZoom != other.wallZoom ) return false;

      return true;
    }

    /// <summary>
    /// Returns a value indicating whether this instance is equal to a specified object value.
    /// </summary>
    /// <param name="obj">An object to compare with this instance.</param>
    /// <returns>True if obj is an equivalent MiscSettings.</returns>
    public override bool Equals( object obj )
    {
      if( !( obj is MapSettings ) ) return false;
      MapSettings other = (MapSettings)obj;

      if( this.useDefaultWhenPlaying    != other.useDefaultWhenPlaying    ) return false;
      if( this.mapSize                  != other.mapSize                  ) return false;
      if( this.showWallpaper            != other.showWallpaper            ) return false;
      if( this.wallOverlayParams        != other.wallOverlayParams        ) return false;
      if( this.wallZoom                 != other.wallZoom                 ) return false;
      if( this.wallUpdate               != other.wallUpdate               ) return false;
      if( this.wallRemove               != other.wallRemove               ) return false;

      return true;
    }

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    /// <returns>Object.GetHashCode().</returns>
    public override int GetHashCode()
    {
      return base.GetHashCode();
    }

    /// <summary>
    /// Support for the == operator.
    /// </summary>
    public static bool operator ==( MapSettings a, MapSettings b )
    {
      if( (object)a == null ) return (object)b == null;
      return a.Equals( b );
    }

    /// <summary>
    /// Support for the != operator.
    /// </summary>
    public static bool operator !=( MapSettings a, MapSettings b )
    {
      if( (object)a == null ) return (object)b != null;
      return !a.Equals( b );
    }

    #endregion
  }


  /// <summary>
  /// Contains settings related to game events.
  /// </summary>
  public class LangSettings
  {
    #region Variables

    /// <summary>
    /// A list of valid language codes.
    /// </summary>
    private readonly List<string> supportedLanguages = new List<string>( new[] { "en", "en-US", "es", "de", "fr" } );

    public string langCode = "en";

    #endregion

    #region Properties

    /// <summary>
    /// Gets the localised homepage url.
    /// </summary>
    public string Homepage
    {
      get
      {
        if( this.langCode.StartsWith( "en" ) )
          return "http://begm.sourceforge.net/";
        else
          return String.Format( "http://begm.sourceforge.net/{0}/", langCode );
      }
    }

    #endregion

    #region Constructors

    /// <summary>
    /// Create a new LangSettings object with default values.
    /// </summary>
    public LangSettings()
    {

    }

    /// <summary>
    /// Create a new LangSettings object with the given values.
    /// </summary>
    public LangSettings( string langCode )
    {
      this.langCode = langCode;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Populate this object with values stored in the registry.
    /// </summary>
    public void LoadFromRegistry()
    {
      RegistryKey key = null;

      try
      {
        key = Registry.CurrentUser.OpenSubKey( "Software\\BEGameMonitor" );
        if( key == null ) return;  // doesn't exist, not an error

        string newLangCode = (string)key.GetValue( "Language", "" );
        if( newLangCode != "" && supportedLanguages.Contains( newLangCode ) ) this.langCode = newLangCode;
      }
      catch( Exception ex )
      {
        Log.AddError( "WARNING: Failed to load Language settings: {0}", ex.Message );
      }
      finally
      {
        if( key != null ) key.Close();
      }
    }

    /// <summary>
    /// Save the values in this object to the registry.
    /// </summary>
    /// <param name="showError">Display a message box if an error occurs.</param>
    public void SaveToRegistry( bool showError )
    {
      RegistryKey key = null;

      try
      {
        key = Registry.CurrentUser.CreateSubKey( "Software\\BEGameMonitor" );
        if( key == null )
          throw new ApplicationException( "Failed to create registry key." );

        key.SetValue( "Language", this.langCode );
      }
      catch( Exception ex )
      {
        Log.AddError( "ERROR: Failed to save Language settings: {0}", ex.Message );
        if( showError )
          MessageBox.Show( Language.Error_SaveLanguage + ":\n\n" + ex.Message, Language.Error_Error, MessageBoxButtons.OK, MessageBoxIcon.Error );
      }
      finally
      {
        if( key != null ) key.Close();
      }
    }

    /// <summary>
    /// Returns a value indicating whether this instance is equal to a specified object value.
    /// </summary>
    /// <param name="obj">An object to compare with this instance.</param>
    /// <returns>True if obj is an equivalent LangSettings.</returns>
    public override bool Equals( object obj )
    {
      if( !( obj is LangSettings ) ) return false;
      LangSettings other = (LangSettings)obj;

      if( this.langCode != other.langCode ) return false;

      return true;
    }

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    /// <returns>Object.GetHashCode().</returns>
    public override int GetHashCode()
    {
      return base.GetHashCode();
    }

    /// <summary>
    /// Support for the == operator.
    /// </summary>
    public static bool operator ==( LangSettings a, LangSettings b )
    {
      if( (object)a == null ) return (object)b == null;
      return a.Equals( b );
    }

    /// <summary>
    /// Support for the != operator.
    /// </summary>
    public static bool operator !=( LangSettings a, LangSettings b )
    {
      if( (object)a == null ) return (object)b != null;
      return !a.Equals( b );
    }

    #endregion
  }

  #endregion

  #region State Classes

  /// <summary>
  /// Contains state related to the Game Status window.
  /// </summary>
  public class GameStatusState
  {
    #region Variables

    public Size windowSize = Size.Empty;
    public Point windowLocation = Point.Empty;
    public int totalKBytesDownloaded = 0;

    #endregion

    #region Methods

    /// <summary>
    /// Populate this object with values stored in the registry.
    /// </summary>
    public void LoadFromRegistry()
    {
      RegistryKey key = null;

      try
      {
        key = Registry.CurrentUser.OpenSubKey( "Software\\BEGameMonitor" );
        if( key != null )  // doesn't exist, not an error
        {
          string gameStatusSize = (string)key.GetValue( "GameStatusSize", "" );
          string gameStatusLocation = (string)key.GetValue( "GameStatusLocation", "" );

          try
          {
            this.windowSize = (Size)Misc.ParsePoint( gameStatusSize );
            this.windowLocation = Misc.ParsePoint( gameStatusLocation );
          }
          catch { }
        }

        key = Registry.CurrentUser.OpenSubKey( "Software\\BEGameMonitor\\Stats" );
        if( key != null )  // doesn't exist, not an error
        {
          int totalKBytesDownloaded = (int)key.GetValue( "TotalKBytesDownloaded", -1 );
          if( totalKBytesDownloaded != -1 ) this.totalKBytesDownloaded = totalKBytesDownloaded;
        }
      }
      catch( Exception ex )
      {
        Log.AddError( "WARNING: Failed to load Game Status state: {0}", ex.Message );
      }
      finally
      {
        if( key != null ) key.Close();
      }
    }

    /// <summary>
    /// Save the values in this object to the registry.
    /// </summary>
    public void SaveToRegistry()
    {
      RegistryKey key = null;
      try
      {
        // window size/location

        key = Registry.CurrentUser.CreateSubKey( "Software\\BEGameMonitor" );
        if( key == null )
          throw new ApplicationException( "Failed to create registry key." );

        key.SetValue( "GameStatusSize", String.Format( "{0}x{1}", this.windowSize.Width, this.windowSize.Height ) );
        key.SetValue( "GameStatusLocation", String.Format( "{0}x{1}", this.windowLocation.X, this.windowLocation.Y ) );
        key.Close();


        // stats

        key = Registry.CurrentUser.CreateSubKey( "Software\\BEGameMonitor\\Stats" );
        if( key == null )
          throw new ApplicationException( "Failed to create registry key." );

        key.SetValue( "TotalKBytesDownloaded", this.totalKBytesDownloaded );
      }
      catch( Exception ex )
      {
        Log.AddError( "ERROR: Failed to save Game Status state: {0}", ex.Message );
      }
      finally
      {
        if( key != null ) key.Close();
      }
    }

    #endregion
  }

  /// <summary>
  /// Contains state related to the RecentEvents widget.
  /// </summary>
  public class RecentEventsState
  {
    #region Variables

    public bool showCaptureEvents = true;
    public bool showAttackObjectiveEvents = true;
    public bool showFirebaseEvents = true;
    public bool showHCUnitEvents = true;
    public bool showFactoryEvents = true;
    
    #endregion

    #region Methods

    /// <summary>
    /// Populate this object with values stored in the registry.
    /// </summary>
    public void LoadFromRegistry()
    {
      RegistryKey key = null;

      try
      {
        key = Registry.CurrentUser.OpenSubKey( "Software\\BEGameMonitor\\Widgets\\RecentEvents" );
        if( key == null ) return;  // doesn't exist, not an error

        string showCaptureEvents = (string)key.GetValue( "ShowCaptureEvents", "" );
        if( showCaptureEvents != "" ) this.showCaptureEvents = showCaptureEvents == "True";

        string showAttackObjectiveEvents = (string)key.GetValue( "ShowAttackObjectiveEvents", "" );
        if( showAttackObjectiveEvents != "" ) this.showAttackObjectiveEvents = showAttackObjectiveEvents == "True";

        string showFirebaseEvents = (string)key.GetValue( "ShowFirebaseEvents", "" );
        if( showFirebaseEvents != "" ) this.showFirebaseEvents = showFirebaseEvents == "True";

        string showHCUnitEvents = (string)key.GetValue( "ShowHCUnitEvents", "" );
        if( showHCUnitEvents != "" ) this.showHCUnitEvents = showHCUnitEvents == "True";

        string showFactoryEvents = (string)key.GetValue( "ShowFactoryEvents", "" );
        if( showFactoryEvents != "" ) this.showFactoryEvents = showFactoryEvents == "True";
      }
      catch( Exception ex )
      {
        Log.AddError( "WARNING: Failed to load Recent Events state: {0}", ex.Message );
      }
      finally
      {
        if( key != null ) key.Close();
      }
    }

    /// <summary>
    /// Save the values in this object to the registry.
    /// </summary>
    public void SaveToRegistry()
    {
      RegistryKey key = null;
      try
      {
        key = Registry.CurrentUser.CreateSubKey( "Software\\BEGameMonitor\\Widgets\\RecentEvents" );
        if( key == null )
          throw new ApplicationException( "Failed to create registry key." );

        key.SetValue( "ShowCaptureEvents", this.showCaptureEvents );
        key.SetValue( "ShowAttackObjectiveEvents", this.showAttackObjectiveEvents );
        key.SetValue( "ShowFirebaseEvents", this.showFirebaseEvents );
        key.SetValue( "ShowHCUnitEvents", this.showHCUnitEvents );
        key.SetValue( "ShowFactoryEvents", this.showFactoryEvents );
      }
      catch( Exception ex )
      {
        Log.AddError( "ERROR: Failed to save Recent Events state: {0}", ex.Message );
      }
      finally
      {
        if( key != null ) key.Close();
      }
    }

    #endregion
  }

  /// <summary>
  /// Contains state related to the TownStatus widget.
  /// </summary>
  public class TownStatusState
  {
    #region Variables

    public int selectedChokePointId = -1;

    #endregion

    #region Methods

    /// <summary>
    /// Populate this object with values stored in the registry.
    /// </summary>
    public void LoadFromRegistry()
    {
      RegistryKey key = null;

      try
      {
        key = Registry.CurrentUser.OpenSubKey( "Software\\BEGameMonitor\\Widgets\\TownStatus" );
        if( key == null ) return;  // doesn't exist, not an error

        int selectedChokePointId = (int)key.GetValue( "SelectedChokePointId", -1 );
        if( selectedChokePointId != -1 ) this.selectedChokePointId = selectedChokePointId;
      }
      catch( Exception ex )
      {
        Log.AddError( "WARNING: Failed to load Town Status state: {0}", ex.Message );
      }
      finally
      {
        if( key != null ) key.Close();
      }
    }

    /// <summary>
    /// Save the values in this object to the registry.
    /// </summary>
    public void SaveToRegistry()
    {
      RegistryKey key = null;
      try
      {
        key = Registry.CurrentUser.CreateSubKey( "Software\\BEGameMonitor\\Widgets\\TownStatus" );
        if( key == null )
          throw new ApplicationException( "Failed to create registry key." );

        key.SetValue( "SelectedChokePointId", this.selectedChokePointId );
      }
      catch( Exception ex )
      {
        Log.AddError( "ERROR: Failed to save Town Status state: {0}", ex.Message );
      }
      finally
      {
        if( key != null ) key.Close();
      }
    }

    #endregion
  }

  /// <summary>
  /// Contains state related to the GameMap widget.
  /// </summary>
  public class GameMapState
  {
    #region Variables

    public DrawOverlayParams mapParams = new DrawOverlayParams();
    public bool altitudeMeters = true;

    #endregion

    #region Methods

    /// <summary>
    /// Populate this object with values stored in the registry.
    /// </summary>
    public void LoadFromRegistry()
    {
      RegistryKey key = null;
      
      try
      {
        key = Registry.CurrentUser.OpenSubKey( "Software\\BEGameMonitor\\Widgets\\GameMap" );
        if( key == null ) return;  // doesn't exist, not an error

        this.mapParams.LoadFromRegistry( key );

        string altitudeMeters = (string)key.GetValue( "AltitudeMeters", "" );
        if( altitudeMeters != "" ) this.altitudeMeters = altitudeMeters == "True";
      }
      catch( Exception ex )
      {
        Log.AddError( "WARNING: Failed to load Game Map state: {0}", ex.Message );
      }
      finally
      {
        if( key != null ) key.Close();
      }
    }

    /// <summary>
    /// Save the values in this object to the registry.
    /// </summary>
    public void SaveToRegistry()
    {
      RegistryKey key = null;

      try
      {
        key = Registry.CurrentUser.CreateSubKey( "Software\\BEGameMonitor\\Widgets\\GameMap" );
        if( key == null )
          throw new ApplicationException( "Failed to create registry key." );

        this.mapParams.SaveToRegistry( key );

        key.SetValue( "AltitudeMeters", this.altitudeMeters );
      }
      catch( Exception ex )
      {
        Log.AddError( "ERROR: Failed to save Game Map state: {0}", ex.Message );
      }
      finally
      {
        if( key != null ) key.Close();
      }
    }

    #endregion
  }

  /// <summary>
  /// Contains state related to the FactoryStatus widget.
  /// </summary>
  public class FactoryStatusState
  {
    #region Variables

    public bool britishExpanded = true;
    public bool frenchExpanded = true;
    public bool germanExpanded = true;

    #endregion

    #region Methods

    /// <summary>
    /// Populate this object with values stored in the registry.
    /// </summary>
    public void LoadFromRegistry()
    {
      RegistryKey key = null;
      
      try
      {
        key = Registry.CurrentUser.OpenSubKey( "Software\\BEGameMonitor\\Widgets\\FactoryStatus" );
        if( key == null ) return;  // doesn't exist, not an error

        string britishExpanded = (string)key.GetValue( "BritishExpanded", "" );
        if( britishExpanded != "" ) this.britishExpanded = britishExpanded == "True";

        string frenchExpanded = (string)key.GetValue( "FrenchExpanded", "" );
        if( frenchExpanded != "" ) this.frenchExpanded = frenchExpanded == "True";

        string germanExpanded = (string)key.GetValue( "GermanExpanded", "" );
        if( germanExpanded != "" ) this.germanExpanded = germanExpanded == "True";
      }
      catch( Exception ex )
      {
        Log.AddError( "WARNING: Failed to load Factory Status state: {0}", ex.Message );
      }
      finally
      {
        if( key != null ) key.Close();
      }
    }

    /// <summary>
    /// Save the values in this object to the registry.
    /// </summary>
    public void SaveToRegistry()
    {
      RegistryKey key = null;
      
      try
      {
        key = Registry.CurrentUser.CreateSubKey( "Software\\BEGameMonitor\\Widgets\\FactoryStatus" );
        if( key == null )
          throw new ApplicationException( "Failed to create registry key." );

        key.SetValue( "BritishExpanded", this.britishExpanded );
        key.SetValue( "FrenchExpanded", this.frenchExpanded );
        key.SetValue( "GermanExpanded", this.germanExpanded );
      }
      catch( Exception ex )
      {
        Log.AddError( "ERROR: Failed to save Factory Status state: {0}", ex.Message );
      }
      finally
      {
        if( key != null ) key.Close();
      }
    }

    #endregion
  }

  /// <summary>
  /// Contains state related to the BrigadeStatus widget.
  /// </summary>
  public class BrigadeStatusState
  {
    #region Variables

    public bool hideFailedAttempts = false;

    #endregion

    #region Methods

    /// <summary>
    /// Populate this object with values stored in the registry.
    /// </summary>
    public void LoadFromRegistry()
    {
      RegistryKey key = null;

      try
      {
        key = Registry.CurrentUser.OpenSubKey( "Software\\BEGameMonitor\\Widgets\\BrigadeStatus" );
        if( key == null ) return;  // doesn't exist, not an error

        string hideFailedAttempts = (string)key.GetValue( "HideFailedAttempts", "" );
        if( hideFailedAttempts != "" ) this.hideFailedAttempts = hideFailedAttempts == "True";
      }
      catch( Exception ex )
      {
        Log.AddError( "WARNING: Failed to load Brigade Status state: {0}", ex.Message );
      }
      finally
      {
        if( key != null ) key.Close();
      }
    }

    /// <summary>
    /// Save the values in this object to the registry.
    /// </summary>
    public void SaveToRegistry()
    {
      RegistryKey key = null;

      try
      {
        key = Registry.CurrentUser.CreateSubKey( "Software\\BEGameMonitor\\Widgets\\BrigadeStatus" );
        if( key == null )
          throw new ApplicationException( "Failed to create registry key." );

        key.SetValue( "HideFailedAttempts", this.hideFailedAttempts );
      }
      catch( Exception ex )
      {
        Log.AddError( "ERROR: Failed to save Brigade Status state: {0}", ex.Message );
      }
      finally
      {
        if( key != null ) key.Close();
      }
    }

    #endregion
  }

  /// <summary>
  /// Contains state related to the ServerStatusState widget.
  /// </summary>
  public class ServerStatusState
  {
    #region Variables

    public bool showConfig = false;

    #endregion

    #region Methods

    /// <summary>
    /// Populate this object with values stored in the registry.
    /// </summary>
    public void LoadFromRegistry()
    {
      RegistryKey key = null;

      try
      {
        key = Registry.CurrentUser.OpenSubKey( "Software\\BEGameMonitor\\Widgets\\ServerStatus" );
        if( key == null ) return;  // doesn't exist, not an error

        string showConfig = (string)key.GetValue( "ShowConfig", "" );
        if( showConfig != "" ) this.showConfig = showConfig == "True";
      }
      catch( Exception ex )
      {
        Log.AddError( "WARNING: Failed to load Server Status state: {0}", ex.Message );
      }
      finally
      {
        if( key != null ) key.Close();
      }
    }

    /// <summary>
    /// Save the values in this object to the registry.
    /// </summary>
    public void SaveToRegistry()
    {
      RegistryKey key = null;

      try
      {
        key = Registry.CurrentUser.CreateSubKey( "Software\\BEGameMonitor\\Widgets\\ServerStatus" );
        if( key == null )
          throw new ApplicationException( "Failed to create registry key." );

        key.SetValue( "ShowConfig", this.showConfig );
      }
      catch( Exception ex )
      {
        Log.AddError( "ERROR: Failed to save Server Status state: {0}", ex.Message );
      }
      finally
      {
        if( key != null ) key.Close();
      }
    }

    #endregion
  }

  #endregion
}
