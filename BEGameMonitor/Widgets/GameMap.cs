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
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using BEGM.Properties;
using XPExplorerBar;
using System.Threading;
using Xiperware.WiretapAPI;

namespace BEGM.Widgets
{
  /// <summary>
  /// The GameMap widget provides a mini scrollable/zoomable map of the game world, and
  /// has the option to contain various levels of detail including AOs, FBs, Airfields,
  /// Links, Grid, Frontlines, Arrows, and Activity.
  /// </summary>
  public partial class GameMap : UserControl, IWidget
  {
    #region Variables

    /* Map constants
     * These values are specific to the region covered by the given map, but are
     * unaffected by it's scale. This means the map can be resized and towns will
     * still be plotted correctly.
     */

    private readonly Size MAP40_SIZE_PIXELS     = new Size( 3275, 1580 );  // pixel size of the default map
    private readonly Size MAP40_SIZE_WALLPAPER  = new Size( 2946, 1580 );  // excluding frankfurt (old map size)
    private readonly RectangleF MAP_SIZE_METERS = new RectangleF( 65375, 3171970, 494110, 238500 );
    private const int UNLOADTIME = 30;  // unload map from memory after collapsed for 30 secs

    private GameState game;
    public Options options;
    private Type hiresMapPlugin = null;
    private Version hiresMapPluginVersion = null;
    private readonly Version hiresMapPluginVersionRequired = new Version( 1, 2 );

    private List<CountryBorder> borders;

    private ChokePoint activeCP   = null;  // mouseover
    private ChokePoint selectedCP = null;  // mouseclick

    private Dictionary<string, ChokePoint> cpLookup;  // for txtActiveCP autocomplete
    private bool altitudeInMeters = true;  // false = display in feet
    private bool commitOverlayAfterScroll = false;
    private Image[] countryFlags;

    private MapMetrics mm = null;  // metrics for the currently loaded game map
    private MapMetrics mmWallpaper = null;  // metrices for the wallpaper map (always map40)

    private DrawOverlayParams overlayParamsGameMap = null;
    private DrawOverlayParams overlayParamsWallpaper = null;

    private bool wallpaperShown = false;
    private Rectangle wallSrcRect;
    private Wallpaper origWallpaper;
    private readonly Wallpaper begmWallpaper = Wallpaper.BEGM;
    private DateTime lastWallpaperUpdate;

#if DEBUG_FRONTLINE
    private NumericUpDown numDebugFrontlineStep;
#endif

    Bitmap imgContested, imgFirebaseAllied, imgFirebaseAxis;

    private Size resizeWindowsStartSize;
    private Point resizeWindowStartMouse;
    
    #endregion

    #region Events

    public event EventHandler DetatchedChanged;

    public void OnDetatchedChanged()
    {
      EventHandler handler = this.DetatchedChanged;
      if( handler != null )
        handler( this, new EventArgs() );
    }

    #endregion

    #region Constructors

    /// <summary>
    /// Create a new GameMap widget.
    /// </summary>
    public GameMap()
    {
#if DEBUG_FRONTLINE
      // add step-count debug control

      this.numDebugFrontlineStep = new NumericUpDown();
      this.numDebugFrontlineStep.Maximum = decimal.MaxValue;
      this.numDebugFrontlineStep.Location = new Point( 3, 17 );
      this.numDebugFrontlineStep.Size = new Size( 60, 20 );
      this.numDebugFrontlineStep.ValueChanged += delegate { UpdateMapOverlay(); };
      this.Controls.Add( this.numDebugFrontlineStep );
#endif

      InitializeComponent();


      // set localised language text

      BegmMisc.LocaliseTreeView( this.tvwMapOptions );


      // init MapViewer control

      mapViewer.MinZoom = 0.3F;
      mapViewer.MaxZoom = 1;  // 1:1

      mapViewer.MouseEnter += mapViewer_MouseEnter;
      mapViewer.MouseLeave += mapViewer_MouseLeave;
      mapViewer.MapMouseClick += mapViewer_MapMouseClick;
      mapViewer.MapMouseDoubleClick += mapViewer_MapMouseDoubleClick;
      mapViewer.MapMouseMove += mapViewer_MapMouseMove;
      mapViewer.MapScrollToPointCompleted += mapViewer_MapScrollToPointCompleted;

      tmrUnloadMap.Interval = UNLOADTIME * 1000;

      this.cpLookup = new Dictionary<string, ChokePoint>();

      this.overlayParamsGameMap = new DrawOverlayParams();
      this.overlayParamsWallpaper = new DrawOverlayParams();


      // permanently expand all map options nodes and set height

      int nodeCount = 0;
      foreach( TreeNode node in tvwMapOptions.Nodes )
      {
        node.ExpandAll();
        nodeCount += 1 + node.Nodes.Count;
      }

      int bottom = tvwMapOptions.Bottom;
      tvwMapOptions.Height = ( nodeCount * 14 ) + 1;
      tvwMapOptions.Top = bottom - tvwMapOptions.Height;  // keep same bottom position


      // dynamic positioning

      Control[] controls = new Control[] { lblFacilitiesHead, lblDeploymentsHead, lblSupplyLinksHead, lblOrigOwnerHead, lblLocationHead };
      int maxWidth = 0;

      foreach( Control control in controls )
        if( control.Width > maxWidth )
          maxWidth = control.Width;

      controls = new Control[] { lblFacilities, lblDeployments, lblSupplyLinks, picOrigOwner, lblOrigOwner, lnkLocation };

      foreach( Control control in controls )
        control.Left = maxWidth + 5;
      picOrigOwner.Left += 3;
      lblOrigOwner.Left += 17;


      // get border data

      this.borders = Data.GetCountryBorders();


      // make resizable when detatched

      this.DetatchedChanged += GameMap_DetatchedChanged;
    }

    #endregion

    #region Properties

    /// <summary>
    /// The GameStatus's RevealWidget() method.
    /// </summary>
    public RevealDelegate GameStatus_RevealWidget
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the internal widget state.
    /// </summary>
    /// <remarks>Access level is set to internal rather than public to prevent VS Designer
    /// from using it.</remarks>
    internal GameMapState State
    {
      get
      {
        GameMapState state = new GameMapState {
          mapParams = this.overlayParamsGameMap,
          altitudeMeters = this.altitudeInMeters
        };

        return state;
      }
      set
      {
        this.overlayParamsGameMap = value.mapParams;
        this.overlayParamsGameMap.mm = this.mm;
        this.overlayParamsGameMap.UpdateControl( tvwMapOptions );

        this.altitudeInMeters = value.altitudeMeters;
      }
    }

    /// <summary>
    /// True if the game map has been loaded into memory.
    /// </summary>
    public bool MapLoaded
    {
      get { return this.mapViewer.MapLoaded; }
    }

    /// <summary>
    /// Gets the currently loaded map size.
    /// </summary>
    public int LoadedMapSizeCode
    {
      get { return this.mm.ScaleCode; }
    }

    /// <summary>
    /// True if the plugin was found and loaded successfully.
    /// </summary>
    public bool HiresMapPluginLoaded
    {
      get { return this.hiresMapPlugin != null; }
    }

    /// <summary>
    /// Returns the version of the plugin (wether or not loaded) or null if not present.
    /// </summary>
    public Version HiresMapPluginVersion
    {
      get { return this.hiresMapPluginVersion; }
    }

    /// <summary>
    /// True if the wallpaper needs updating (specified update time has elapsed and
    /// user is not fullscreen).
    /// </summary>
    public bool WallpaperNeedsUpdate
    {
      get
      {
        if( !this.wallpaperShown )
          return false;

        if( BegmMisc.IsFullScreen( 0 ) )
          return false;  // don't update while fullscreen (causes flicker)

        if( this.lastWallpaperUpdate < DateTime.Now.AddSeconds( ( options.Map.wallUpdate * -60 ) + 10 ) )  // allow some leeway
          return true;

        return false;
      }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Initialise the GameMap widget.
    /// </summary>
    /// <param name="game">The current game state.</param>
    public void InitWidget( GameState game )
    {
      Log.AddEntry( "  GameMap..." );

      this.game = game;




      // create a local copy of country flag images for access in worker thread
      // (otherwise may encounter "Object is currently in use elsewhere.")

      this.countryFlags = new Image[this.game.Countries.Length];
      for( int i = 0; i < this.game.Countries.Length; i++ )
      {
        this.countryFlags[i] = (Image)this.game.Countries[i].CountryFlag.Clone();
      }

      imgContested      = Resources.contested;
      imgFirebaseAllied = Resources.mapicon_firebase_allied;
      imgFirebaseAxis   = Resources.mapicon_firebase_axis;


      // set tooltip

      GameStatus.ToolTip.SetToolTip( lnkCPName, Language.GameMap_Tooltip_ViewTownStatus );


      // populate chokepoint combobox

      ReloadCPList();


      // set initial zoom & center position

      mapViewer.ZoomFactor = mapViewer.MinZoom;  // start zoomed out

      // init to map40 size (if larger map loaded, set Center will convert from map40 metrics
      mapViewer.InitSize( MAP40_SIZE_PIXELS );
      this.mm = this.mmWallpaper = new MapMetrics( MAP40_SIZE_PIXELS, MAP_SIZE_METERS, 40 );
      this.overlayParamsWallpaper.mm = this.mmWallpaper;

      Point newCenter = GetMapCenterAverage();
      mapViewer.Center = this.mm.OctetToMap( newCenter );


      // create initial frontlines

      game.UpdateFrontlines();


      // set initial wallpaper

      if( options.Map.showWallpaper )
        UpdateWallpaper( options.Map.OverlayParams );


      Log.Okay();
    }

    /// <summary>
    /// Updates the map overlay with the current game state, selected cp and map settings,
    /// and populates or refreshes the selected cp data.
    /// </summary>
    public void UpdateWidget()
    {
      if( this.game == null ) return;

      if( !game.Wiretap.NewMapData ) return;

      UpdateMapOverlay();
      UpdateSelectedCPInfo();
    }

    /// <summary>
    /// Remove references to local game objects.
    /// </summary>
    public void Cleanup()
    {
      mapViewer.UnloadMap();

      this.game = null;
      this.activeCP = null;
      this.selectedCP = null;
      this.overlayParamsGameMap.selectedCP = null;
      cpLookup.Clear();
    }

    /// <summary>
    /// Makes the Game Map widget visible by making sure it's expanded, scrolling it into
    /// view, and selecting the given ChokePoint.
    /// </summary>
    /// <seealso cref="tmrReveal_Tick"/>
    /// <param name="arg">The ChokePoint to display.</param>
    public void Reveal( object arg )
    {
      ChokePoint cp = arg as ChokePoint;
      if( cp == null ) return;

      if( tmrReveal.Enabled )
        return;  // another Reveal() in progress

      Expando expGameMap = (Expando)this.Parent;

      if( !LoadMap() )
      {
        System.Media.SystemSounds.Hand.Play();
        return;
      }

      Point ptCP = this.mm.OctetToMap( cp.LocationOctets );

      if( expGameMap.Collapsed )
      {
        SelectChokePoint( cp );
        mapViewer.ZoomFactor = mapViewer.MaxZoom;
        mapViewer.Center = ptCP;
        expGameMap.Collapsed = false;  // animate
        tmrReveal.Start();             // scroll into view after animation
      }
      else
      {
        if( expGameMap.TaskPane != null )
          expGameMap.TaskPane.ScrollControlIntoView( expGameMap );

        this.commitOverlayAfterScroll = true;  // avoid stutter during ScrollToPoint()
        SelectChokePoint( cp );
        mapViewer.ScrollToPoint( ptCP, true );
      }
    }

    /// <summary>
    /// If present, attempts to load the HiresMapPlugin to enable larger maps.
    /// </summary>
    public void LoadHiresMapPlugin()
    {
      if( !File.Exists( Path.Combine( Application.StartupPath, "BEGM_HiresMapPlugin.dll" ) ) )
        return;

      Log.AddEntry( "Loading hires map plugin..." );

      try
      {
        Assembly assembly = Assembly.LoadFile( Path.Combine( Application.StartupPath, "BEGM_HiresMapPlugin.dll" ) );


        // check version >= required

        this.hiresMapPluginVersion = assembly.GetName().Version;
        if( this.hiresMapPluginVersion < this.hiresMapPluginVersionRequired )
          throw new Exception( String.Format( "BEGM {0} requires plugin version {1} or greater (you have {2})",
                                              Program.versionString,
                                              Misc.VersionToString( this.hiresMapPluginVersionRequired ),
                                              Misc.VersionToString( this.hiresMapPluginVersion ) ) );


        this.hiresMapPlugin = assembly.GetType( "HiresMapPlugin.Resources" );
        Log.Okay();
      }
      catch( Exception ex )
      {
        Log.Error();
        Log.AddException( ex );
        this.hiresMapPlugin = null;
      }
    }

    /// <summary>
    /// Load the map image data into memory, available for display.
    /// </summary>
    /// <returns>True if successful.</returns>
    public bool LoadMap()
    {
#if MAC
      return true;
#endif
      tmrUnloadMap.Stop();

      if( mapViewer.MapLoaded )
        return true;

      this.Parent.Cursor = Cursors.WaitCursor;


      // load background image

      int newMapSizeCode = GetRequiredMapSizeCode();
      Bitmap image = null;

      try
      {
        image = GetMapBackground( newMapSizeCode );

        // accessing .Height or .Width will throw "ArgumentException: Parameter is not valid."
        // if ran out of memory while trying to load from resource file
        if( image.Height <= 0 || image.Width <= 0 )  
        {
          Log.AddError( "ERROR: Failed to load game map: HxW={0}x{1} ?", image.Height, image.Width );
          return false;
        }

        mapViewer.LoadMap( image );
      }
      catch( Exception ex )
      {
        MapLoadFailed( "Failed to load game map", ex );

        try
        {
          if( image != null )
            image.Dispose();
        }
        catch { }

        return false;
      }


      // calc map scale/offset/etc

      this.mm = new MapMetrics( mapViewer.MapSize, MAP_SIZE_METERS, newMapSizeCode );
      this.overlayParamsGameMap.mm = this.mm;


      // update details

      UpdateMapOverlay();
      UpdateSelectedCPInfo();


      this.Parent.Cursor = Cursors.Arrow;

      return true;
    }

    /// <summary>
    /// Unload the map data from memory, after a short delay.
    /// </summary>
    public void QueueUnloadMap()
    {
      tmrUnloadMap.Start();
    }

    /// <summary>
    /// If the required map size has changed, reloads the map.
    /// </summary>
    public void ReloadMapSize()
    {
      if( !mapViewer.MapLoaded ) return;

      int newSizeCode = GetRequiredMapSizeCode();
      if( this.LoadedMapSizeCode == newSizeCode )
        return;

      Log.AddEntry( "Changing to {0}% scale map", options.Map.mapSize );

      mapViewer.UnloadMap();

      if( !LoadMap() )
        System.Media.SystemSounds.Hand.Play();
    }

    /// <summary>
    /// Update the map overlay asynchronously with current game status.
    /// </summary>
    private void UpdateMapOverlay()
    {
      if( !mapViewer.MapLoaded )
        return;

      if( bgwDrawOverlay.IsBusy )
        bgwDrawOverlay.CancelAsync();  // will restart bgw when finished cancelling
      else
        bgwDrawOverlay.RunWorkerAsync();
    }

    /// <summary>
    /// Updates the selected chokepoint controls.
    /// </summary>
    private void UpdateSelectedCPInfo()
    {
      // if selection empty, hide values

      if( this.selectedCP == null )
      {
        lnkCPName.Visible = lnkLocation.Visible = false;
        picCPFlag.Image = picOrigOwner.Image = null;
        lblFacilities.Text = lblDeployments.Text = lblOrigOwner.Text = lblSupplyLinks.Text = null;
        return;
      }
      else
      {
        lnkCPName.Visible = lnkLocation.Visible = true;
      }


      // flag

      picCPFlag.Image = selectedCP.FlagImage;
      GameStatus.ToolTip.SetToolTip( picCPFlag, selectedCP.FlagTooltip );


      // title

      lnkCPName.Text = selectedCP.Name;


      // facilities

      int ab = 0, af = 0, dep = 0, oth = 0;

      foreach( Facility facility in selectedCP.Facilities )
      {
        switch( facility.Type )
        {
          case FacilityType.Armybase:
            ab++;
            break;
          case FacilityType.Airbase:
            af++;
            break;
          case FacilityType.Depot:
            dep++;
            break;
          default:
            oth++;
            break;
        }
      }

      StringBuilder sbFac = new StringBuilder();
      if( ab > 0 )
        sbFac.AppendFormat( "{0} {1}, ", ab, ab == 1 ? Language.GameMap_Facility_AB : Language.GameMap_Facility_ABs );
      if( af > 0 )
        sbFac.AppendFormat( "{0} {1}, ", af, af == 1 ? Language.GameMap_Facility_AF : Language.GameMap_Facility_AFs );
      if( dep > 0 )
        sbFac.AppendFormat( "{0} {1}, ", dep, dep == 1 ? Language.GameMap_Facility_Depot : Language.GameMap_Facility_Depots );
      if( oth > 0 )
        sbFac.AppendFormat( "{0} {1}, ", oth, oth == 1 ? Language.GameMap_Facility_Other : Language.GameMap_Facility_Others );

      if( sbFac.Length > 0 )
        sbFac.Length -= 2;
      else
        sbFac.Append( Language.GameMap_None );

      lblFacilities.Text = sbFac.ToString();


      // deployments

      int army = 0, air = 0, navy = 0;

      foreach( HCUnit hcunit in selectedCP.DeployedHCUnits )
      {
        switch( hcunit.Branch )
        {
          case Branch.Army:
            army++;
            break;
          case Branch.Airforce:
            air++;
            break;
          case Branch.Navy:
            navy++;
            break;
        }
      }

      StringBuilder sbDeploy = new StringBuilder();
      int lastnum = 0;
      if( army > 0 )
      {
        sbDeploy.AppendFormat( "{0} {1}, ", army, Language.Enum_Branch_Army.ToLower() );
        lastnum = army;
      }
      if( air > 0 )
      {
        sbDeploy.AppendFormat( "{0} {1}, ", air, Language.Enum_Branch_Airforce.ToLower() );
        lastnum = air;
      }
      if( navy > 0 )
      {
        sbDeploy.AppendFormat( "{0} {1}, ", navy, Language.Enum_Branch_Navy.ToLower() );
        lastnum = navy;
      }

      if( sbDeploy.Length > 0 )
      {
        sbDeploy.Length -= 2;
        sbDeploy.Append( " " );
        sbDeploy.Append( lastnum == 1 ? Language.GameMap_Unit : Language.GameMap_Units );
      }
      else
        sbDeploy.Append( Language.GameMap_None );

      lblDeployments.Text = sbDeploy.ToString();


      // supply links

      int available = 0;

      foreach( ChokePoint cp in selectedCP.LinkedChokePoints )
        if( cp.Owner.Side == selectedCP.Owner.Side )
          available++;

      if( selectedCP.LinkedChokePoints.Count == 0 )
        lblSupplyLinks.Text = Language.GameMap_None;
      else if( available == selectedCP.LinkedChokePoints.Count )
        lblSupplyLinks.Text = selectedCP.LinkedChokePoints.Count + " (" + Language.GameMap_Links_AllAvailable + ")";
      else
        lblSupplyLinks.Text = selectedCP.LinkedChokePoints.Count + " (" + String.Format( Language.GameMap_Links_SomeAvailable, available ) + ")";


      // original owner

      picOrigOwner.Image = selectedCP.OriginalCountry.CountryFlag;
      lblOrigOwner.Text = selectedCP.OriginalCountry.Name;


      // location

      int zoom = 13 - ab;
      double x, y;
      this.mm.OctetToDecimalLatLong( selectedCP.LocationOctets, out x, out y );

      string latlong = String.Format( new Misc.LatLongFormatter(), "{0:Y}, {1:X}", y, x );
      string url = String.Format( "http://maps.google.com/maps?ll={0},{1}&z={2}&t=h", y, x, zoom );

      lnkLocation.Text = latlong;
      lnkLocation.Links.Clear();
      lnkLocation.Links.Add( 0, latlong.Length, url );
      lnkLocation.Tag = latlong;  // for UpdateSelectedCPAltitude()

      UpdateSelectedCPAltitude();
    }

    /// <summary>
    /// Appends or updates the "@ Xm" or "@ Xft" link at the end of the location text.
    /// </summary>
    private void UpdateSelectedCPAltitude()
    {
      if( selectedCP.AltitudeMeters == 0 )
        return;  // no alt data for this cp

      string latlong = (string)lnkLocation.Tag;

      string alt;
      if( this.altitudeInMeters )
        alt = String.Format( Language.GameMap_AltitudeMeters, selectedCP.AltitudeMeters );
      else
        alt = String.Format( Language.GameMap_AltitudeFeet, selectedCP.AltitudeFeet );

      lnkLocation.Text = String.Format( "{0} @ {1}", latlong, alt );

      if( lnkLocation.Links.Count == 1 )
        lnkLocation.Links.Add( latlong.Length + 3, alt.Length );
      else
        lnkLocation.Links[1].Length = alt.Length;
    }

    /// <summary>
    /// Performs the drawing of the map overlay.
    /// </summary>
    /// <param name="g">The graphics object to draw with.</param>
    /// <param name="param">The DrawOverlayParams object contains information about what to draw.</param>
    /// <param name="bgw">If running in a thread, the parent BackgroundWorker to check for pending cancellation (may be null).</param>
    /// <returns>True if ran to completion without being cancelled.</returns>
    private bool DrawOverlay( Graphics g, DrawOverlayParams param, BackgroundWorker bgw )
    {
      #region Init

      g.InterpolationMode = InterpolationMode.High;
      g.SmoothingMode = SmoothingMode.HighQuality;
      g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

      Color colorAllied = Color.Blue;
      Color colorAxis = Color.Red;
      Color colorAlliedAlpha = Color.FromArgb( 75, colorAllied );
      Color colorAxisAlpha = Color.FromArgb( 75, colorAxis );
      Color colorHCUnitAllied = Color.FromArgb( 128, 204, 255 );  // light blue
      Color colorHCUnitAxis = Color.FromArgb( 255, 178, 102 );    // orange
      Color colorAirfield = Color.FromArgb( 80, 0, 0, 0 );

      Pen penGrid = new Pen( Color.FromArgb( 80, 0, 0, 0 ), param.mm.MapScaleUp( 1, 2 ) );
      Pen penLink = new Pen( Color.FromArgb( 125, 0, 0, 0 ), param.mm.MapScaleUp( 1, 2 ) );
      Pen penLinkDisabled = new Pen( Color.FromArgb( 180, Color.White ), param.mm.MapScaleUp( 1, 2 ) );
      Pen penAlliedThick = new Pen( colorAlliedAlpha, param.mm.MapScaleUp( 5, 8 ) );
      Pen penAxisThick = new Pen( colorAxisAlpha, param.mm.MapScaleUp( 5, 8 ) );
      Pen penAttackObjective = new Pen( Color.FromArgb( 150, Color.Yellow ), param.mm.MapScaleUp( 2, 3 ) );
      Pen penHCUnitAllied = new Pen( colorHCUnitAllied, param.mm.MapScaleUp( 2F, 3 ) );
      Pen penHCUnitAxis = new Pen( colorHCUnitAxis, param.mm.MapScaleUp( 2F, 3 ) );
      Pen penHCUnitAlliedDash = new Pen( colorHCUnitAllied, param.mm.MapScaleUp( 1, 2 ) );
      Pen penHCUnitAxisDash = new Pen( colorHCUnitAxis, param.mm.MapScaleUp( 1, 2 ) );
      penHCUnitAlliedDash.DashPattern = penHCUnitAxisDash.DashPattern = new float[] { 20, 10 };
      Pen penAirfield = new Pen( colorAirfield, param.mm.MapScaleUp( 1 ) );
      Pen penFrontlineAllied = new Pen( Color.FromArgb( 100, 0, 0, 100 ), param.mm.MapScaleUp( 10F, 20F ) );
      Pen penFrontlineAxis = new Pen( Color.FromArgb( 100, 100, 0, 0 ), param.mm.MapScaleUp( 10F, 20F ) );
      Pen penActivityGridline = new Pen( Color.FromArgb( 60, Color.Black ) );
      Pen penAttackArrowAllied = new Pen( Color.FromArgb( 200, colorAllied ), param.mm.MapScaleUp( 5, 7 ) );
      Pen penAttackArrowAxis = new Pen( Color.FromArgb( 200, colorAxis ), param.mm.MapScaleUp( 5, 7 ) );
      penAttackArrowAllied.CustomEndCap =
        penAttackArrowAxis.CustomEndCap = new AdjustableArrowCap( param.mm.MapScaleUp( 3, 3.5F ), param.mm.MapScaleUp( 3.5F, 4 ) );
      Pen penAttackArrowAlliedDash = (Pen)penAttackArrowAllied.Clone();
      Pen penAttackArrowAxisDash = (Pen)penAttackArrowAxis.Clone();
      penAttackArrowAlliedDash.DashStyle =
        penAttackArrowAxisDash.DashStyle = DashStyle.Dot;
      Pen penCountryBorder = new Pen( Color.FromArgb( 96, 0, 0, 0 ), param.mm.MapScaleUp( 2F ) );
      penCountryBorder.LineJoin = LineJoin.Round;

      Brush brushAllied = new SolidBrush( colorAlliedAlpha );
      Brush brushAxis = new SolidBrush( colorAxisAlpha );
      Brush brushHCUnitAllied = new SolidBrush( colorHCUnitAllied );
      Brush brushHCUnitAxis = new SolidBrush( colorHCUnitAxis );
      Brush brushAirfield = new SolidBrush( colorAirfield );
      Brush brushAttackObjective = new SolidBrush( Color.FromArgb( 50, Color.Yellow ) );
      Brush brushAOLabel1 = new SolidBrush( Color.FromArgb( 150, Color.Black ) );
      Brush brushAOLabel2 = new SolidBrush( Color.FromArgb( 190, Color.Yellow ) );
      Brush brushActivityBackground = new SolidBrush( Color.FromArgb( 60, Color.Black ) );
      Brush brushCountryName = new SolidBrush( Color.FromArgb( 112, 0, 0, 0 ) );

      Font fontAOLabel = new Font( "Arial Black", param.mm.MapScaleUp( 8.25F, 11.25F ), FontStyle.Bold );
      Font fontCountryName = new Font( "Arial Black", param.mm.MapScaleUp( 17F ), FontStyle.Bold );

      StringFormat alignCenter = new StringFormat { Alignment = StringAlignment.Center };

      int regionTagRadiusCP = (int)param.mm.MapScaleUp( 10, 20 );
      int regionTagRadiusFB = (int)param.mm.MapScaleUp( 6, 12 );

      float distTail = param.mm.MapScaleUp( 50 );
      LinearGradientBrush brushTail;
      Blend blendTail = new Blend();
      blendTail.Positions = new[] { 0.0F, 0.7F, 1.0F };
      blendTail.Factors   = new[] { 0.0F, 0.0F, 1.0F };

      if( bgw != null && bgw.CancellationPending ) { return false; }

      #endregion

      #region Country Borders

      if( param.showCountryBorders )
      {
        foreach( CountryBorder border in this.borders )
        {
          foreach( Point[] poly in border.GetBorders( param.mm ) )
            g.DrawLines( penCountryBorder, poly );

          if( param.showCountryBordersNames )
            g.DrawString( border.CountryName, fontCountryName, brushCountryName, border.GetCenter( param.mm ), alignCenter );
        }
      }

      #endregion

      #region Air Grid

      if( param.showAirGrid )
      {
        // horizontal segments (AA-DW, aka 1-101)

        for( float x = param.mm.GridPixels.Bounds.X, i = 0; i <= param.mm.GridPixels.CellCount.X; x += param.mm.GridPixels.Cell.Width, i++ )
        {
          if( x < 0 || x > param.mm.Pixels.Width ) continue;
          g.DrawLine( penGrid, x, param.mm.GridPixels.Bounds.Top, x, param.mm.GridPixels.Bounds.Bottom );  // vert lines
        }

        // vertical segments (0 - 60)

        for( float y = param.mm.GridPixels.Bounds.Y, i = 0; i <= param.mm.GridPixels.CellCount.Y; y += param.mm.GridPixels.Cell.Height, i++ )
        {
          if( y < 0 || y > param.mm.Pixels.Height ) continue;
          g.DrawLine( penGrid, param.mm.GridPixels.Bounds.Left, y, param.mm.GridPixels.Bounds.Right, y );  // horiz lines
        }
      }

      if( bgw != null && bgw.CancellationPending ) { return false; }

      #endregion

      #region Supply Links

      if( param.showSupplyLinks )
      {
#if !DEBUG_FRONTLINE
        double linkWidth = param.mm.MapScaleUp( 2, 4 );

        foreach( ChokePoint cp in this.game.ValidChokePoints )
        {
          PointF pt1 = param.mm.MetersToMapF( cp.Location );
          foreach( ChokePoint linkCP in cp.LinkedChokePoints )
          {
            PointF pt2 = param.mm.MetersToMapF( linkCP.Location );


            // get points offset 2px 90deg

            double angle = Misc.AngleBetween( pt1, pt2 ) + 90;
            PointF pt1offset = Misc.AngleOffset( pt2, angle, linkWidth );
            PointF pt2offset = Misc.AngleOffset( pt1, angle, linkWidth );


            // check if link disabled (enemy depot at either end)

            bool linkDisabled = false;
            if( cp.Owner.Side == linkCP.Owner.Side )  // don't draw white lines to enemy towns
              linkDisabled = !cp.CanMoveUnitTo( linkCP );


            // draw line (link other direction will draw other line)

            if( linkDisabled )
              g.DrawLine( penLinkDisabled, pt1offset, pt2offset );
            else
              g.DrawLine( penLink, pt1offset, pt2offset );
          }

          if( bgw != null && bgw.CancellationPending ) { return false; }
        }
#else
        // debugging, draw near paths instead

        foreach( ChokePoint cp in game.AllCPs )
        {
          if( cp == null ) continue;

          Point ptCP = param.mm.OctetToMap( cp.Location );
          foreach( ChokePoint cpNear in cp.NearbyChokePoints )
          {
            Point ptCP2 = param.mm.OctetToMap( cpNear.Location );
            g.DrawLine( Pens.LightGray, ptCP, ptCP2 );
          }
        }
#endif
      }

      #endregion

      #region Airfields

      if( param.showAirfields )
      {
        // create compass ring region template

        float diamInner = param.mm.MapScaleUp( 82 );
        float diamOuter = param.mm.MapScaleUp( 90 );
        float diamPie   = param.mm.MapScaleUp( 95 );

        Region afRingTemplate = new Region();
        afRingTemplate.MakeEmpty();

        GraphicsPath circle = new GraphicsPath();
        circle.AddEllipse( -( diamOuter / 2 ), -( diamOuter / 2 ), diamOuter, diamOuter );
        circle.AddEllipse( -( diamInner / 2 ), -( diamInner / 2 ), diamInner, diamInner );
        afRingTemplate.Union( circle );

        GraphicsPath arcs = new GraphicsPath();
        for( float angle = 22.5F; angle < 360; angle += 45F )
          arcs.AddPie( -( diamPie / 2 ), -( diamPie / 2 ), diamPie, diamPie, angle, 22.5F );
        afRingTemplate.Exclude( arcs );


        // draw for each cp w/ an airfield

        foreach( ChokePoint cp in this.game.ValidChokePoints )
        {
          Point ptAirfield = new Point();
          foreach( Facility facility in cp.Facilities )
          {
            if( facility.Type == FacilityType.Airbase )
            {
              ptAirfield = param.mm.MetersToMap( facility.Location );
              break;
            }
          }
          if( ptAirfield.IsEmpty ) continue;

          Region afRing = afRingTemplate.Clone();
          afRing.Translate( ptAirfield.X, ptAirfield.Y );

          g.FillRegion( brushAirfield, afRing );
          g.DrawEllipse( penAirfield, ptAirfield.X - ( diamOuter / 2 ), ptAirfield.Y - ( diamOuter / 2 ), diamOuter, diamOuter );
          g.DrawEllipse( penAirfield, ptAirfield.X - ( diamInner / 2 ), ptAirfield.Y - ( diamInner / 2 ), diamInner, diamInner );

          if( bgw != null && bgw.CancellationPending ) { return false; }
        }
      }

      #endregion

      #region Frontlines

      if( param.showFrontlines && game.AlliedFrontline != null && game.AxisFrontline != null )
      {
#if DEBUG_FRONTLINE
        bool debugFrontlines = false;

        if( this.numDebugFrontlineStep.Value == 0 )
          game.AlliedFrontline.debugMaxStep = game.AxisFrontline.debugMaxStep = int.MaxValue;
        else
        {
          game.AlliedFrontline.debugMaxStep = game.AxisFrontline.debugMaxStep = (int)this.numDebugFrontlineStep.Value;
          debugFrontlines = true;
        }


        // update every redraw when debugging

        Aga.Controls.TimeCounter.Start();
        game.UpdateFrontlines();
        float time = Aga.Controls.TimeCounter.Finish();
        Log.AddError( "Frontline draw: {0} seconds", time );
        

        if( debugFrontlines )
        {  
          // draw raw hulls (black)

          foreach( List<ChokePoint> cplist in game.AlliedFrontline.Hulls )
          {
            for( int i = 0; i < cplist.Count; i++ )
            {
              int i2 = i + 1; if( i2 > cplist.Count - 1 ) i2 -= cplist.Count;
              Point pt1 = param.mm.OctetToMap( cplist[i].Location );
              Point pt2 = param.mm.OctetToMap( cplist[i2].Location );
              g.DrawLine( Pens.Black, pt1, pt2 );
            }
          }
          foreach( List<ChokePoint> cplist in game.AxisFrontline.Hulls )
          {
            for( int i = 0; i < cplist.Count; i++ )
            {
              int i2 = i + 1; if( i2 > cplist.Count - 1 ) i2 -= cplist.Count;
              Point pt1 = param.mm.OctetToMap( cplist[i].Location );
              Point pt2 = param.mm.OctetToMap( cplist[i2].Location );
              g.DrawLine( Pens.Black, pt1, pt2 );
            }
          }


          // draw raw lines (red/blue)

          foreach( List<Point> frontline in game.AlliedFrontline.Lines )
          {
            for( int i = 0; i < frontline.Count - 1; i++ )
            {
              Point pt1 = param.mm.OctetToMap( frontline[i] );
              Point pt2 = param.mm.OctetToMap( frontline[i + 1] );
              g.DrawLine( Pens.Blue, pt1, pt2 );
            }
          }
          foreach( List<Point> frontline in game.AxisFrontline.Lines )
          {
            for( int i = 0; i < frontline.Count - 1; i++ )
            {
              Point pt1 = param.mm.OctetToMap( frontline[i] );
              Point pt2 = param.mm.OctetToMap( frontline[i + 1] );
              g.DrawLine( Pens.Red, pt1, pt2 );
            }
          }


          // draw enemy links

          for( int i = 0; i < game.AlliedFrontline.EnemyLinks.Count; i += 2 )
          {
            Point pt1 = param.mm.OctetToMap( game.AlliedFrontline.EnemyLinks[i] );
            Point pt2 = param.mm.OctetToMap( game.AlliedFrontline.EnemyLinks[i + 1] );
            g.DrawLine( Pens.Orange, pt1, pt2 );
          }
          for( int i = 0; i < game.AxisFrontline.EnemyLinks.Count; i += 2 )
          {
            Point pt1 = param.mm.OctetToMap( game.AxisFrontline.EnemyLinks[i] );
            Point pt2 = param.mm.OctetToMap( game.AxisFrontline.EnemyLinks[i + 1] );
            g.DrawLine( Pens.Orange, pt1, pt2 );
          }


          // bounding box

          if( param.selectedCP != null )
          {
            int i1 = -1;
            List<ChokePoint> hull = null;

            foreach( List<ChokePoint> cplist in game.AlliedFrontline.Hulls )
            {
              if( i1 < 0 )
              {
                i1 = cplist.IndexOf( param.selectedCP );
                hull = cplist;
              }
            }
            foreach( List<ChokePoint> cplist in game.AxisFrontline.Hulls )
            {
              if( i1 < 0 )
              {
                i1 = cplist.IndexOf( param.selectedCP );
                hull = cplist;
              }
            }

            if( i1 >= 0 )
            {
              int i0 = i1 - 1; if( i0 < 0 ) i0 += hull.Count;
              int i2 = i1 + 1; if( i2 > hull.Count - 1 ) i2 -= hull.Count;
              int i3 = i1 + 2; if( i3 > hull.Count - 1 ) i3 -= hull.Count;

              Point[] polyFriendly = Frontline.GetCollisionPoly( hull[i0], hull[i1], hull[i2], hull[i3], true );
              Point[] polyEnemy = Frontline.GetCollisionPoly( hull[i0], hull[i1], hull[i2], hull[i3], false );

              Point[] lines = new Point[polyFriendly.Length + polyEnemy.Length];
              for( int i = 0; i < polyFriendly.Length; i++ )
                lines[i] = param.mm.MetersToMap( polyFriendly[i] );
              for( int i = 0; i < polyEnemy.Length; i++ )
                lines[i + polyFriendly.Length] = param.mm.MetersToMap( polyEnemy[i] );

              g.DrawLines( Pens.LightGreen, lines );
            }
          }
        }
#endif

        // draw frontline curves

        foreach( List<Point> frontline in game.AlliedFrontline.Lines )
        {
          Point[] points = new Point[frontline.Count];
          for( int i = 0; i < frontline.Count; i++ )
            points[i] = param.mm.OctetToMap( frontline[i] );
          g.DrawCurve( penFrontlineAllied, points, 1, points.Length - 3, 0.5F );

          int iEnd = frontline.Count - 1;
          if( frontline[1] != frontline[iEnd - 1] && frontline[0] != frontline[iEnd] )  // draw tail
          {
            Point ptStart = Misc.AngleOffset( points[1], Misc.AngleBetween( points[0], points[1] ), distTail );
            brushTail = new LinearGradientBrush( points[1], ptStart, penFrontlineAllied.Color, Color.Transparent ) { Blend = blendTail };
            g.DrawCurve( new Pen( brushTail, penFrontlineAllied.Width ), new[] { points[2], points[1], ptStart }, 1, 1, 0.5F );

            Point ptEnd = Misc.AngleOffset( points[iEnd - 1], Misc.AngleBetween( points[iEnd], points[iEnd - 1] ), distTail );
            brushTail = new LinearGradientBrush( points[iEnd - 1], ptEnd, penFrontlineAllied.Color, Color.Transparent ) { Blend = blendTail };
            g.DrawCurve( new Pen( brushTail, penFrontlineAllied.Width ), new[] { points[iEnd - 2], points[iEnd - 1], ptEnd }, 1, 1, 0.5F );
          }
        }
        foreach( List<Point> frontline in game.AxisFrontline.Lines )
        {
          Point[] points = new Point[frontline.Count];
          for( int i = 0; i < frontline.Count; i++ )
            points[i] = param.mm.OctetToMap( frontline[i] );
          g.DrawCurve( penFrontlineAxis, points, 1, points.Length - 3, 0.5F );

          int iEnd = frontline.Count - 1;
          if( frontline[1] != frontline[iEnd - 1] && frontline[0] != frontline[iEnd] )  // draw tail
          {
            Point ptStart = Misc.AngleOffset( points[1], Misc.AngleBetween( points[0], points[1] ), distTail );
            brushTail = new LinearGradientBrush( points[1], ptStart, penFrontlineAxis.Color, Color.Transparent ) { Blend = blendTail };
            g.DrawCurve( new Pen( brushTail, penFrontlineAxis.Width ), new[] { points[2], points[1], ptStart }, 1, 1, 0.5F );

            Point ptEnd = Misc.AngleOffset( points[iEnd - 1], Misc.AngleBetween( points[iEnd], points[iEnd - 1] ), distTail );
            brushTail = new LinearGradientBrush( points[iEnd - 1], ptEnd, penFrontlineAxis.Color, Color.Transparent ) { Blend = blendTail };
            g.DrawCurve( new Pen( brushTail, penFrontlineAxis.Width ), new[] { points[iEnd - 2], points[iEnd - 1], ptEnd }, 1, 1, 0.5F );
          }
        }

        if( bgw != null && bgw.CancellationPending ) { return false; }
      }

      #endregion

      #region Attack Arrows

      if( param.showAttackArrows )
      {
        double offsetDist = param.mm.MapScaleUp( 7 );
        double offsetDistAO = param.mm.MapScaleUp( 15 );  // diamAO = 28

        foreach( ChokePoint cp in this.game.ValidChokePoints.Where( cp => cp.HasAO ) )
        {
          // draw curved arrows: linked cps with open fb

          foreach( ChokePoint cpLinked in cp.LinkedChokePoints )
          {
            if( cpLinked.Owner.Side == cp.Owner.Side ) continue;
            Firebase fb = cp.GetFirebaseFrom( cpLinked );
            if( fb == null || !fb.IsOpen ) continue;

            Point ptStart = param.mm.OctetToMap( cpLinked.LocationOctets );
            Point ptMiddle = param.mm.MetersToMap( fb.Location );
            Point ptEnd = param.mm.OctetToMap( cp.LocationOctets );

            double angleStart = Misc.AngleBetween( ptMiddle, ptStart );
            double angleEnd = Misc.AngleBetween( ptMiddle, ptEnd );

            double distStart = param.showAttackObjectives && cpLinked.HasAO ? offsetDistAO : offsetDist;
            double distEnd = param.showAttackObjectives ? offsetDistAO : offsetDist;

            Point ptStartOffset = Misc.AngleOffset( ptStart, angleStart, distStart );
            Point ptEndOffset = Misc.AngleOffset( ptEnd, angleEnd, distEnd );

            double distMid = param.mm.MapScaleUp( 26 );
            double distMidMax = Misc.DistanceBetween( ptStartOffset, ptEndOffset ) * 0.6;
            if( distMid > distMidMax )
              distMid = distMidMax;

            Point ptMiddleOffset = Misc.AngleOffset( ptEndOffset, angleEnd, distMid );

            g.DrawBezier( cpLinked.Owner.Side == Side.Allied ? penAttackArrowAllied : penAttackArrowAxis,
                          ptStartOffset, ptMiddleOffset, ptMiddleOffset, ptEndOffset );
          }


          // draw straight arrows: any other cps in cp.AttackFrom (including linked cps with no fb)

          foreach( ChokePoint cpAttacking in cp.AttackFrom.Values )
          {
            bool linked = false;
            if( cp.LinkedChokePoints.Contains( cpAttacking ) )
            {
              linked = true;
              if( cp.GetFirebaseFrom( cpAttacking ) != null ) // fb exists, already drawn above
                continue;
            }

            if( param.showAttackArrowsLinked && !linked )
              continue; // only show linked

            Point ptStart = param.mm.OctetToMap( cpAttacking.LocationOctets );
            Point ptEnd = param.mm.OctetToMap( cp.LocationOctets );

            double angleStart = Misc.AngleBetween( ptEnd, ptStart );
            double angleEnd = angleStart + 180; // opposite

            double distStart = param.showAttackObjectives && cpAttacking.HasAO ? offsetDistAO : offsetDist;
            double distEnd = param.showAttackObjectives ? offsetDistAO : offsetDist;

            Point ptStartOffset = Misc.AngleOffset( ptStart, angleStart, distStart );
            Point ptEndOffset = Misc.AngleOffset( ptEnd, angleEnd, distEnd );

            Pen pen;
            if( linked ) pen = cpAttacking.Owner.Side == Side.Allied ? penAttackArrowAllied : penAttackArrowAxis;
            else pen = cpAttacking.Owner.Side == Side.Allied ? penAttackArrowAlliedDash : penAttackArrowAxisDash;

            g.DrawLine( pen, ptStartOffset, ptEndOffset );
          }
        }
      }

      #endregion

      #region Selected ChokePoint

      if( param.selectedCP != null )
      {
        Point loc = param.mm.OctetToMap( param.selectedCP.LocationOctets );

        float diamInner  = param.mm.MapScaleUp( 34 );
        float diamOuter  = param.mm.MapScaleUp( 40 );
        float diamLinked = param.mm.MapScaleUp( 20, 40 );


        // draw main selected circle

        Color color = param.selectedCP.Owner.Side == Side.Allied ? Color.Blue : Color.Red;
        Pen penOuter = new Pen( color, param.mm.MapScaleUp( 1.5F, 2 ) );
        Pen penInner = new Pen( Color.FromArgb( 50, color ), param.mm.MapScaleUp( 5 ) );

        g.DrawEllipse( penOuter, loc.X - ( diamOuter / 2 ), loc.Y - ( diamOuter / 2 ), diamOuter, diamOuter );
        g.DrawEllipse( penInner, loc.X - ( diamInner / 2 ), loc.Y - ( diamInner / 2 ), diamInner, diamInner );


        // draw linked cps

        foreach( ChokePoint linkedCP in param.selectedCP.LinkedChokePoints )
        {
          Point loc2 = param.mm.OctetToMap( linkedCP.LocationOctets );
          double angle = Misc.AngleBetween( loc, loc2 );
          PointF linestart = Misc.AngleOffset( loc, angle + 180, diamOuter / 2 );
          PointF lineend = Misc.AngleOffset( loc2, angle, diamLinked / 2 );

          g.FillEllipse( linkedCP.Owner.Side == Side.Allied ? brushAllied : brushAxis, loc2.X - ( diamLinked / 2 ), loc2.Y - ( diamLinked / 2 ), diamLinked, diamLinked );
          g.DrawLine( linkedCP.Owner.Side == Side.Allied ? penAlliedThick : penAxisThick, linestart, lineend );
        }

        if( bgw != null && bgw.CancellationPending ) { return false; }


        // draw selected cp hcunit links

        if( param.showBrigadeLinks && param.showBrigadeLinksSelected && param.selectedCP.DeployedHCUnits.Count > 0 )
        {
          // get list of divisions

          List<HCUnit> divisions = new List<HCUnit>();
          foreach( HCUnit hcunit in param.selectedCP.DeployedHCUnits )
          {
            HCUnit division;
            if( hcunit.Level == HCUnitLevel.Division )
              division = hcunit;
            else if( hcunit.Level == HCUnitLevel.Brigade )
              division = hcunit.ParentUnit;
            else
              continue;

            if( !divisions.Contains( division ) )
              divisions.Add( division );
          }


          // draw line from division cp to each brigade cp

          foreach( HCUnit division in divisions )
          {
            if( !division.IsDeployed || division.Country.Side != param.selectedCP.Owner.Side ) continue;
            if( param.showBrigadeLinksArmy && division.Branch != Branch.Army ) continue;

            Point ptDivision = param.mm.OctetToMap( division.DeployedChokePoint.LocationOctets );

            foreach( HCUnit brigade in division.ChildUnits )
            {
              if( !brigade.IsDeployed || brigade.DeployedChokePoint == division.DeployedChokePoint ) continue;
              Point ptBrigade = param.mm.OctetToMap( brigade.DeployedChokePoint.LocationOctets );

              Pen penLine;

              if( division.MoveType == MoveType.Land )
                penLine = division.Country.Side == Side.Allied ? penHCUnitAllied : penHCUnitAxis;
              else
                penLine = division.Country.Side == Side.Allied ? penHCUnitAlliedDash : penHCUnitAxisDash;

              g.DrawLine( penLine, ptDivision, ptBrigade );
            }
          }
        }

        if( bgw != null && bgw.CancellationPending ) { return false; }

      }

      #endregion

      #region Brigade Links

      if( param.showBrigadeLinks && !param.showBrigadeLinksSelected )
      {
        foreach( HCUnit division in this.game.HCUnits.Values )
        {
          if( division.Level != HCUnitLevel.Division ) continue;
          if( !division.IsDeployed ) continue;
          if( param.showBrigadeLinksArmy && division.Branch != Branch.Army ) continue;

          Point ptDivision = param.mm.OctetToMap( division.DeployedChokePoint.LocationOctets );

          foreach( HCUnit brigade in division.ChildUnits )
          {
            if( !brigade.IsDeployed || brigade.DeployedChokePoint == division.DeployedChokePoint ) continue;
            Point ptBrigade = param.mm.OctetToMap( brigade.DeployedChokePoint.LocationOctets );

            Pen penLine;
            if( division.MoveType == MoveType.Land )
              penLine = division.Country.Side == Side.Allied ? penHCUnitAllied : penHCUnitAxis;
            else
              penLine = division.Country.Side == Side.Allied ? penHCUnitAlliedDash : penHCUnitAxisDash;

            g.DrawLine( penLine, ptDivision, ptBrigade );
          }

          if( bgw != null && bgw.CancellationPending ) { return false; }
        }
      }

      #endregion

      #region Chokepoints

      foreach( ChokePoint cp in this.game.ValidChokePoints )
      {
        Point ptCP = param.mm.OctetToMap( cp.LocationOctets );

        if( param.showAttackObjectives && cp.HasAO )
        {
          float diamAO = param.mm.MapScaleUp( 28 );
          g.DrawEllipse( penAttackObjective, ptCP.X - ( diamAO / 2 ), ptCP.Y - ( diamAO / 2 ), diamAO, diamAO );
          g.FillEllipse( brushAttackObjective, ptCP.X - ( diamAO / 2 ), ptCP.Y - ( diamAO / 2 ), diamAO, diamAO );
        }

        if( cp.IsContested )
        {
          SizeF szContest = param.mm.MapScaleUp( imgContested.Size, 21, 42 );
          float yoffset = param.mm.MapScaleUp( 7.75F, 15.5F );
          g.DrawImage( imgContested, ptCP.X - ( szContest.Width / 2 ), ptCP.Y - yoffset, szContest.Width, szContest.Height );  // 21x17
        }

        SizeF szFlag = param.mm.MapScaleUp( countryFlags[cp.Owner.ID].Size, 12, 24 );
        g.DrawImage( countryFlags[cp.Owner.ID], ptCP.X - ( szFlag.Width / 2 ), ptCP.Y - ( szFlag.Height / 2 ), szFlag.Width, szFlag.Height );

        if( cp.DeployedHCUnits.Count > 0 )
        {
          float diamDot = param.mm.MapScaleUp( 3.5F, 6 );
          float xoffset = param.mm.MapScaleUp( 5, 8 );
          PointF ptDot = new PointF( ptCP.X - ( szFlag.Width / 2 ) - xoffset, ptCP.Y - ( szFlag.Height / 2 ) - 0.5F );

          // one dot = one unit
          g.FillEllipse( cp.Owner.Side == Side.Allied ? brushHCUnitAllied : brushHCUnitAxis,
               ptDot.X, ptDot.Y, diamDot, diamDot );

          // two dots = multiple units
          if( cp.DeployedHCUnits.Count > 1 )
            g.FillEllipse( cp.Owner.Side == Side.Allied ? brushHCUnitAllied : brushHCUnitAxis,
                           ptDot.X, ptDot.Y + diamDot + 1, diamDot, diamDot );
        }


        // add region for mouseover

        if( bgw != null )  // doing game map (not wallpaper)
          mapViewer.AddRegionTag( ptCP, regionTagRadiusCP, cp.ID );

        if( bgw != null && bgw.CancellationPending ) { return false; }
      }

#if DEBUG_FRONTLINE
      if( this.numDebugFrontlineStep.Value != 0 )
      {
        foreach( ChokePoint cp in game.AllCPs )
        {
          if( cp == null || cp.Type != ChokePointType.Dummy ) continue;
          Point ptCP = param.mm.OctetToMap( cp.Location );
          g.FillEllipse( Brushes.White, ptCP.X - 5, ptCP.Y - 5, 10, 10 );
          g.DrawString( cp.Name, SystemFonts.DefaultFont, Brushes.White, ptCP.X + 6, ptCP.Y - 5.5F );
        }
      }
#endif

      #endregion

      #region Firebases

      if( param.showFirebases )
      {
        foreach( ChokePoint cp in this.game.ValidChokePoints )
        {
          foreach( Firebase fb in cp.Firebases.Where( fb => fb.IsOpen ) )
          {
            Bitmap fbicon = fb.Link.Side == Side.Allied ? imgFirebaseAllied : imgFirebaseAxis;
            SizeF szFB = param.mm.MapScaleUp( fbicon.Size, 12, 21 );
            Point ptFB = param.mm.MetersToMap( fb.Location );
            g.DrawImage( fbicon, ptFB.X - ( szFB.Width / 2 ), ptFB.Y - ( szFB.Height / 2 ), szFB.Width, szFB.Height );

            if( bgw != null )  // doing game map (not wallpaper)
              mapViewer.AddRegionTag( ptFB, regionTagRadiusFB, fb.Name );
          }

          if( bgw != null && bgw.CancellationPending ) { return false; }
        }
      }

      #endregion

      #region AO Town Names

      if( param.showAttackObjectiveNames )
      {
        foreach( ChokePoint cp in this.game.ValidChokePoints.Where( cp => cp.HasAO ) )
        {
          Point ptCP = param.mm.OctetToMap( cp.LocationOctets );
          int x = (int)param.mm.MapScaleUp( -16F );
          int y = (int)param.mm.MapScaleUp( 15F );

          // black text outline, +/- 1.5px
          g.DrawString( cp.Name.ToUpper(), fontAOLabel, brushAOLabel1, ptCP.X + x + 1.5F, ptCP.Y + y );
          g.DrawString( cp.Name.ToUpper(), fontAOLabel, brushAOLabel1, ptCP.X + x - 1.5F, ptCP.Y + y );
          g.DrawString( cp.Name.ToUpper(), fontAOLabel, brushAOLabel1, ptCP.X + x, ptCP.Y + y + 1.5F );
          g.DrawString( cp.Name.ToUpper(), fontAOLabel, brushAOLabel1, ptCP.X + x, ptCP.Y + y - 1.5F );

          g.DrawString( cp.Name.ToUpper(), fontAOLabel, brushAOLabel2, ptCP.X + x, ptCP.Y + y );
        }
      }

      #endregion

      #region Player Activity

      if( param.showPlayerActivity && this.game.MapCells.Count > 0 )
      {
        // get cell size in pixels

        SizeF cellSize = param.mm.MetersToMapF( new SizeF( this.game.MapCellSize, this.game.MapCellSize ) );


        // align grid to first entry in map cells

        float yTop = 0;
        foreach( Point ptCellMeters in this.game.MapCells.Keys )
        {
          PointF ptCellMap = param.mm.MetersToMapF( ptCellMeters );
          yTop = ptCellMap.Y % cellSize.Height;
          break;
        }


        // dim map

        g.FillRectangle( brushActivityBackground, 0, 0, param.mm.Pixels.Width, param.mm.Pixels.Height );


        // draw activity cells

        float margin = param.mm.MapScaleUp( 0.4F, 0.6F );
        foreach( Point ptCellMeters in this.game.MapCells.Keys )
        {
          PointF ptCellMap = param.mm.MetersToMapF( ptCellMeters );
          Brush brushDeaths = new SolidBrush( this.game.MapCells[ptCellMeters].GetCellColour() );
          g.FillRectangle( brushDeaths, ptCellMap.X - cellSize.Width, ptCellMap.Y, cellSize.Width - margin, cellSize.Height - margin );

          if( bgw != null && bgw.CancellationPending ) { return false; }
        }


        // draw horizontal gridlines

        for( float y = yTop; y < param.mm.Pixels.Height; y += cellSize.Height )
          g.DrawLine( penActivityGridline, 0, y, param.mm.Pixels.Width, y );
      }

      #endregion

      return true;
    }

    /// <summary>
    /// Populate the ChokePoint combobox with the towns in displaycps.
    /// </summary>
    private void ReloadCPList()
    {
      txtActiveCP.AutoCompleteCustomSource.Clear();
      cpLookup.Clear();

      AutoCompleteStringCollection acsc = new AutoCompleteStringCollection();

      foreach( ChokePoint cp in this.game.ValidChokePoints.Where( cp => !cp.IsTraining ) )
      {
        // really slow adding to assigned instance!?
        // txtActiveCP.AutoCompleteCustomSource.Add( cp.Name );
        acsc.Add( cp.Name );
        cpLookup.Add( cp.Name.ToLower(), cp );
      }

      txtActiveCP.AutoCompleteCustomSource = acsc;
    }

    /// <summary>
    /// Selects the given chokepoint and updates the widget.
    /// </summary>
    /// <param name="cp">The ChokePoint to select.</param>
    private void SelectChokePoint( ChokePoint cp )
    {
      this.selectedCP = cp;
      UpdateMapOverlay();
      UpdateSelectedCPInfo();
    }

    /// <summary>
    /// Gets the map size that should be used according to whether the plugin is loaded,
    /// game running, user preference, etc.
    /// </summary>
    /// <returns>Map size code (40-100).</returns>
    private int GetRequiredMapSizeCode()
    {
      if( this.hiresMapPlugin == null )  // large map plugin not loaded, always use default
        return 40;

      if( options.Map.useDefaultWhenPlaying && BegmMisc.WW2Running() )
        return 40;

      return options.Map.mapSize;
    }

    /// <summary>
    /// Gets a new map background of the requested size.
    /// </summary>
    /// <param name="mapSizeCode">The size to create (>40 required the plugin to be loaded).</param>
    /// <returns>A new Bitmap to use.</returns>
    private Bitmap GetMapBackground( int mapSizeCode )
    {
      switch( mapSizeCode )
      {
        case 40:
          return Resources.map;
        case 60:
          return (Bitmap)this.hiresMapPlugin.InvokeMember( "map60", BindingFlags.GetProperty | BindingFlags.NonPublic | BindingFlags.Static, null, null, null );
        case 80:
          return (Bitmap)this.hiresMapPlugin.InvokeMember( "map80", BindingFlags.GetProperty | BindingFlags.NonPublic | BindingFlags.Static, null, null, null );
        case 100:
          return (Bitmap)this.hiresMapPlugin.InvokeMember( "map100", BindingFlags.GetProperty | BindingFlags.NonPublic | BindingFlags.Static, null, null, null );
        default:
          throw new ApplicationException( "unknown map size: " + mapSizeCode );
      }
    }

    /// <summary>
    /// Calculates a point that is approx in the middle of the action
    /// (the average location of all towns w/ an AO).
    /// </summary>
    /// <returns>The center point in game world octets.</returns>
    private Point GetMapCenterAverage()
    {
      float avgAOx = 0, avgAOy = 0, count = 0;

      foreach( ChokePoint cp in this.game.ValidChokePoints.Where( cp => cp.HasAO ) )
      {
        avgAOx += cp.LocationOctets.X;
        avgAOy += cp.LocationOctets.Y;
        count++;
      }

      if( count > 0 )
        return new Point( (int)( avgAOx / count ), (int)( avgAOy / count ) );
      else
        return new Point( 178, 3808 );  // default = 1380:830px
    }

    /// <summary>
    /// Calculates a point that is approx in the middle of the action
    /// (the center of the rectangle containing all towns w/ an AO).
    /// </summary>
    /// <returns>The center point in game world octets.</returns>
    private Point GetMapCenterBounded()
    {
      int minX = 0, maxX = 0, minY = 0, maxY = 0, count = 0;

      foreach( ChokePoint cp in this.game.ValidChokePoints.Where( cp => cp.HasAO ) )
      {
        if( count == 0 )  // first
        {
          minX = maxX = cp.LocationOctets.X;
          minY = maxY = cp.LocationOctets.Y;
        }
        else
        {
          if( cp.LocationOctets.X < minX ) minX = cp.LocationOctets.X;
          if( cp.LocationOctets.X > maxX ) maxX = cp.LocationOctets.X;
          if( cp.LocationOctets.Y < minY ) minY = cp.LocationOctets.Y;
          if( cp.LocationOctets.Y > maxY ) maxY = cp.LocationOctets.Y;
        }

        count++;
      }

      if( count > 0 )
        return new Point( (int)( ( minX + maxX ) / 2F ), (int)( ( minY + maxY ) / 2F ) );
      else
        return new Point( 178, 3808 );  // default = 1380:830px
    }

    /// <summary>
    /// Log error and collapse widget when the map failed to load.
    /// </summary>
    /// <param name="message">A message about which area failed.</param>
    /// <param name="ex">The exception that occurred.</param>
    private void MapLoadFailed( string message, Exception ex )
    {
      Kernel32.MEMORYSTATUSEX msex = new Kernel32.MEMORYSTATUSEX();
      if( Kernel32.GlobalMemoryStatusEx( msex ) )
        Log.AddEntry( "DEBUG: {0}", msex );

      Log.AddError( "ERROR: {0}: {1}", message, ex.Message );

      ( (Expando)this.Parent ).Collapse();

      try
      {
        mapViewer.UnloadMap();
      }
      catch { }
    }

    /// <summary>
    /// Gets the region of the original map to display as wallpaper, taking into
    /// account screen size and wallpaper zoom settings.
    /// </summary>
    /// <returns>The region of the map to scale down to fit the display, or Empty if
    /// the screen is larger than the map.</returns>
    private Rectangle GetWallpaperRegion()
    {
      Point ptCenter = GetMapCenterBounded();
      Point ptMapCenter = this.mmWallpaper.OctetToMap( ptCenter );

      Rectangle rectScreen = Screen.PrimaryScreen.Bounds;
      Rectangle rectMap = new Rectangle( new Point(), MAP40_SIZE_WALLPAPER );  // don't show mostly empty frankfurt edge

      float xRatio = (float)rectScreen.Width  / (float)rectMap.Width;
      float yRatio = (float)rectScreen.Height / (float)rectMap.Height;

      if( xRatio >= 1 || yRatio >= 1 )  // screen is larger than map
        return Rectangle.Empty;


      // get ratio to use

      float minRatio = xRatio > yRatio ? xRatio : yRatio;  // shrink to fit height/width
      float maxRatio = 1;                                  // 100% scale
      float ratio = minRatio + ( ( maxRatio - minRatio ) * ( options.Map.wallZoom / 6F ) );

      int width = (int)Math.Round( rectScreen.Width / ratio );
      int height = (int)Math.Round( rectScreen.Height / ratio );

      int x = ptMapCenter.X - ( width / 2 );
      if( x > rectMap.Width - width )
        x = rectMap.Width - width;
      if( x < 0 )
        x = 0;

      int y = ptMapCenter.Y - ( height / 2 );
      if( y > rectMap.Height - height )
        y = rectMap.Height - height;
      if( y < 0 )
        y = 0;

      return new Rectangle( x, y, width, height );
    }

    /// <summary>
    /// Updates the wallpaper with the current settings and data.
    /// </summary>
    /// <param name="param">New overlay params to use.</param>
    public void UpdateWallpaper( DrawOverlayParams param )
    {
      this.overlayParamsWallpaper = param;
      this.overlayParamsWallpaper.mm = this.mmWallpaper;


      // get updated region to display (in case zoom has changed)

      this.wallSrcRect = GetWallpaperRegion();


      UpdateWallpaper();
    }

    /// <summary>
    /// Updates the wallpaper with the current settings and data.
    /// </summary>
    public void UpdateWallpaper()
    {
      if( !options.Map.showWallpaper )
      {
        if( this.wallpaperShown )  // remove
          RemoveWallpaper();

        return;
      }

      if( !this.wallpaperShown )  // first time
      {
        // get wallpaper to revert to

        this.origWallpaper = Wallpaper.Current;
        if( this.origWallpaper.FilePath == this.begmWallpaper.FilePath )  // exited begm with "remove wallpaper on exit" unchecked
          this.origWallpaper.LoadFromRegistry();  // get stored values from registry


        // set new wallpaper properties

        this.begmWallpaper.Apply();


        // get region to display (persistant until begm restart or wallpaper removed)

        this.wallSrcRect = GetWallpaperRegion();
      }


      // get map background, draw overlay

      Bitmap map = Resources.map;
      Graphics g = Graphics.FromImage( map );
      DrawOverlay( g, this.overlayParamsWallpaper, null );
      g.Dispose();


      // draw wallpaper

      Brush brushWhite = new SolidBrush( Color.FromArgb( this.overlayParamsWallpaper.showPlayerActivity ? 120 : 140, Color.White ) );
      Brush brushBlack = new SolidBrush( Color.FromArgb( 40, Color.Black ) );
      Font fontMain = new Font( "Arial Black", 9F );
      StringFormat alignRight = new StringFormat { Alignment = StringAlignment.Far };

      Bitmap wallpaper;

      if( this.wallSrcRect.IsEmpty )  // screen is larger than map
      {
        // wallpaper image is same size as map

        wallpaper = map;
        g = Graphics.FromImage( wallpaper );
      }
      else
      {
        // wallpaper image is screen-size, with wallSrcRect scaled into it

        Rectangle wallpaperDestRect = Screen.PrimaryScreen.Bounds;
        wallpaper = new Bitmap( wallpaperDestRect.Width, wallpaperDestRect.Height );

        g = Graphics.FromImage( wallpaper );
        g.DrawImage( map, wallpaperDestRect, this.wallSrcRect, GraphicsUnit.Pixel );
        map.Dispose();
      }


      // draw logo

      ImageAttributes ia = new ImageAttributes();
      ColorMatrix cm = new ColorMatrix();
      cm.Matrix00 = cm.Matrix11 = cm.Matrix22 = cm.Matrix44 = 1;
      cm.Matrix33 = 40F / 100F;  // 40% transparency
      ia.SetColorMatrix( cm );
      g.DrawImage( Resources.begm_watermark, new Rectangle( wallpaper.Width - 379, 23, 339, 89 ), 0, 0, 339, 89, GraphicsUnit.Pixel, ia );


      // draw map update time

      string time;
      if( DateTimeFormatInfo.CurrentInfo.LongTimePattern.Contains( "H" ) )
        time = String.Format( "{0:d MMM, HH:mm}", DateTime.Now );  // 24-hour
      else
        time = String.Format( "{0:d MMM, h:mm tt}", DateTime.Now );  // 12-hour

      int x = wallpaper.Width - 40;
      const int y = 95;

      g.DrawString( time, fontMain, brushBlack, x + 1.5F, y, alignRight );
      g.DrawString( time, fontMain, brushBlack, x - 1.5F, y, alignRight );
      g.DrawString( time, fontMain, brushBlack, x, y + 1.5F, alignRight );
      g.DrawString( time, fontMain, brushBlack, x, y - 1.5F, alignRight );
      g.DrawString( time, fontMain, brushWhite, x, y, alignRight );

      g.Dispose();


      wallpaper.Save( this.begmWallpaper.FilePath, ImageFormat.Bmp );
      wallpaper.Dispose();

      this.begmWallpaper.Update();
      this.wallpaperShown = true;
      this.lastWallpaperUpdate = DateTime.Now;
    }

    /// <summary>
    /// Remove the wallpaper and revert to the previous version.
    /// </summary>
    private void RemoveWallpaper()
    {
      this.wallpaperShown = false;


      // revert to original wallpaper

      this.origWallpaper.Apply();
      this.origWallpaper.Update();
      this.origWallpaper = null;


      // remove temp image file

      try
      {
        if( File.Exists( this.begmWallpaper.FilePath ) )
          File.Delete( this.begmWallpaper.FilePath );
      }
      catch { }

    }

    /// <summary>
    /// Removes the wallpaper on exit, if required (otherwise stores original in
    /// registry to be restored next time).
    /// </summary>
    public void ExitWallpaper()
    {
      if( !this.wallpaperShown ) return;

      if( options.Map.wallRemove )
        RemoveWallpaper();
      else
        this.origWallpaper.SaveToRegistry();
    }

    /// <summary>
    /// Closes the map options menu.
    /// </summary>
    public void CloseMapOptions()
    {
      if( cbMapOptions.Checked )
        cbMapOptions.Checked = false;
    }

    /// <summary>
    /// Update the size of the GameMap widget when detatched.
    /// </summary>
    /// <param name="newSize">The new size (of the widget, not the expando).</param>
    public void SetDetatchedSize( Size newSize )
    {
      // maximum size

      int screenIndex = 0;
      if( options != null )
        screenIndex = options.Misc.GameStatusDisplayIndex;
      Rectangle desktop = Screen.AllScreens[screenIndex].WorkingArea;


      // maximum size

      if( newSize.Width > desktop.Width )
        newSize.Width = desktop.Width;

      if( newSize.Height > desktop.Height )
        newSize.Height = desktop.Height;


      // minimum size

      if( newSize.Width < this.MinimumSize.Width )
        newSize.Width = this.MinimumSize.Width;

      if( newSize.Height < this.MinimumSize.Height )
        newSize.Height = this.MinimumSize.Height;


      // set new size

      Expando expGameMap = (Expando)this.Parent;
      Form form = expGameMap.Parent as Form;

      if( form != null )
        form.SuspendLayout();

      this.Size = newSize;

      newSize.Width += 2;
      newSize.Height += expGameMap.HeaderHeight + 1;
      expGameMap.Size = newSize;

      this.mapViewer.Zoom();  // update map zoom

      if( form != null )
        form.ResumeLayout( true );


      // update performance mode

      this.mapViewer.MapHighPerformanceMode = ( this.Width > this.MinimumSize.Width || this.Height > this.MinimumSize.Height );
    }

    #endregion

    #region Event Handlers

    // focus on enter to allow mousewheel zooming (prevent taskpane scrolling)
    private void mapViewer_MouseEnter( object sender, EventArgs e )
    {
      CloseMapOptions();

      if( txtActiveCP.Focused ) return;
      
      BegmMisc.FocusWithoutScroll( mapViewer );

      Expando expGameMap = (Expando)this.Parent;
      if( expGameMap.TaskPane != null )
        expGameMap.TaskPane.PreventAutoScroll = true;  // prevent mousewheel scrolling
    }
    private void mapViewer_MouseLeave( object sender, EventArgs e )
    {
      Expando expGameMap = (Expando)this.Parent;
      if( expGameMap.TaskPane != null )
        expGameMap.TaskPane.PreventAutoScroll = false;

      lblCell.Text = lblLatLong.Text = null;

      if( !txtActiveCP.Focused )
        txtActiveCP.Text = null;
    }

    // on click, select active cp
    private void mapViewer_MapMouseClick( object sender, MouseEventArgs e )
    {
      if( Control.ModifierKeys == (Keys.Control | Keys.Shift | Keys.Alt) )
      {
        if( this.activeCP == null )
          return;

        // eegg: click to toggle cp

        Country otherSide = this.activeCP.Owner.Abbr == "DE"
                          ? new Country( 1, "UK", Language.Country_Name_England, Language.Country_Demonym_England, Side.Allied )
                          : new Country( 4, "DE", Language.Country_Name_Germany, Language.Country_Demonym_Germany, Side.Axis );

        Log.AddError( "DEBUG: pwning {0} {1}", this.activeCP, otherSide.Demonym );
        this.activeCP.Uncontest( otherSide );

#if !DEBUG_FRONTLINE
        game.UpdateFrontlines();
#endif

        UpdateMapOverlay();
      }
      else
      {
        // select cp under mouse (active cp)

        if( this.activeCP != this.selectedCP )
          SelectChokePoint( this.activeCP );
      }
    }

    // on doubleclick, go to town status
    private void mapViewer_MapMouseDoubleClick( object sender, MouseEventArgs e )
    {
      if( this.activeCP != null )
      {
        Expando expGameMap = (Expando)this.Parent;

        if( expGameMap.TaskPane != null )
          expGameMap.TaskPane.PreventAutoScroll = false;

        GameStatus_RevealWidget( WidgetType.TownStatus, activeCP );

        if( expGameMap.TaskPane != null )
          expGameMap.TaskPane.PreventAutoScroll = true;
      }
      else
      {
        mapViewer.ScrollToPoint( e.Location, false );
      }
    }

    // on move, get the chokepoint under the cursor, update latlong
    private void mapViewer_MapMouseMove( object sender, MouseEventArgs e )
    {
      if( mapViewer.IsScrolling || picResizeCorner.Capture )
        return;  // too cpu intensive to do this while momentum scrolling


      // update location fields

      if( this.overlayParamsGameMap.showAirGrid )
        lblCell.Text = this.mm.MapToCell( e.Location );
      lblLatLong.Text = this.mm.MapToLatLong( e.Location );


      if( txtActiveCP.Focused )
      {
        mapViewer.Cursor = Cursors.Arrow;
        return;  // typing
      }


      // update active cp, cursor

      object tag = mapViewer.GetRegionTag( e.Location );

      if( tag == null )
      {
        this.activeCP = null;
        mapViewer.Cursor = Cursors.Arrow;
        txtActiveCP.Text = null;
      }
      else if( tag is int )  // tag = chokepoint id
      {
        this.activeCP = this.game.ChokePoints[(int)tag];
        mapViewer.Cursor = Cursors.Hand;
        txtActiveCP.Text = activeCP.Name;
      }
      else if( tag is string )  // tag = firebase name
      {
        this.activeCP = null;
        mapViewer.Cursor = Cursors.Arrow;
        txtActiveCP.Text = (string)tag;
      }
    }

    // type on mapViewer or txtActiveCP to search for cp
    private void mapViewer_KeyPress( object sender, KeyPressEventArgs e )
    {
      if( e.KeyChar == 8 || e.KeyChar == 43 || e.KeyChar == 45 || e.KeyChar == 61 || e.KeyChar == 95 )
        return;  // backspace, +/- used for zooming

      txtActiveCP.Focus();
      txtActiveCP.Text = e.KeyChar.ToString();
      txtActiveCP.SelectionStart = 1;
      e.Handled = true;
    }
    private void txtActiveCP_Enter( object sender, EventArgs e )
    {
      txtActiveCP.Text = null;
    }
    private void txtActiveCP_KeyUp( object sender, KeyEventArgs e )
    {
      if( e.KeyCode == Keys.Enter )
      {
        ChokePoint cp;
        if( !cpLookup.TryGetValue( txtActiveCP.Text.ToLower(), out cp ) )
          return;  // not found

        txtActiveCP.Text = cp.Name;
        this.commitOverlayAfterScroll = true;  // avoid stutter during ScrollToPoint()
        SelectChokePoint( cp );
        mapViewer.ScrollToPoint( this.mm.OctetToMap( cp.LocationOctets ), true );
        mapViewer.Focus();
      }
    }

    // unload map on collapse after delay
    private void tmrUnloadMap_Tick( object sender, EventArgs e )
    {
      tmrUnloadMap.Stop();
      mapViewer.UnloadMap();
    }

    // Reveal() timer
    private void tmrReveal_Tick( object sender, EventArgs e )
    {
      Expando expGameMap = (Expando)this.Parent;

      if( expGameMap.Animating )
        return;  // wait until finished animating

      if( expGameMap.TaskPane != null )
        expGameMap.TaskPane.ScrollControlIntoView( expGameMap );

      tmrReveal.Stop();
    }

    // links
    private void lnkCPName_LinkClicked( object sender, LinkLabelLinkClickedEventArgs e )
    {
      GameStatus_RevealWidget( WidgetType.TownStatus, this.selectedCP );
    }
    private void lnkLocation_LinkClicked( object sender, LinkLabelLinkClickedEventArgs e )
    {
      if( e.Link.LinkData is string )  // google maps link
      {
        try
        {
          Process.Start( (string)e.Link.LinkData );  // contains the "http://..." url
        }
        catch( Exception ex )
        {
          MessageBox.Show( ex.Message, Language.Error_Error, MessageBoxButtons.OK, MessageBoxIcon.Error );
        }
      }
      else  // swap altitude meters/feet
      {
        this.altitudeInMeters = !altitudeInMeters;
        UpdateSelectedCPAltitude();
      }
    }

    // manually show tooltip when over google maps link
    private void lnkLocation_MouseHover( object sender, EventArgs e )
    {
      Point pt = lnkLocation.PointToClient( Cursor.Position );
      if( pt.X < 108 )
        GameStatus.ToolTip.Show( Language.GameMap_Tooltip_GoogleMaps, lnkLocation, pt.X, pt.Y + 21, 5000 );
    }
    private void lnkLocation_MouseLeave( object sender, EventArgs e )
    {
      GameStatus.ToolTip.Hide( lnkLocation );
    }

    // draw overlay worker thread
    private void bgwDrawOverlay_DoWork( object sender, DoWorkEventArgs e )
    {
      // set thread ui culture

      Thread.CurrentThread.CurrentUICulture = Program.uiCulture;


      // get current bgw object

      BackgroundWorker bgw = sender as BackgroundWorker;


      // get overlay handle, create graphics resources

      Graphics g = mapViewer.BeginDrawNewOverlay();

      this.overlayParamsGameMap.selectedCP = this.selectedCP;

      bool completed = DrawOverlay( g, this.overlayParamsGameMap, bgw );
      if( !completed ) e.Cancel = true;

      g.Dispose();
    }
    private void bgwDrawOverlay_RunWorkerCompleted( object sender, RunWorkerCompletedEventArgs e )
    {
      if( e.Error != null )
      {
        MapLoadFailed( "Failed to draw map overlay", e.Error );
      }
      else if( e.Cancelled )
      {
        bgwDrawOverlay.RunWorkerAsync();  // restart
      }
      else
      {
        if( this.commitOverlayAfterScroll )
        {
          // EndDrawNewOverlay() is called when mapViewer.ScrollToPoint has completed
        }
        else
        {
          try
          {
            mapViewer.EndDrawNewOverlay();  // commit
            this.Refresh();
          }
          catch( Exception ex )
          {
            MapLoadFailed( "Failed to commit new map overlay", ex );
          }
        }
      }
    }
    private void mapViewer_MapScrollToPointCompleted( object sender, EventArgs e )
    {
      if( !this.commitOverlayAfterScroll )
        return;

      if( bgwDrawOverlay.IsBusy )  // still not finished
      {
        this.commitOverlayAfterScroll = false;  // commit when done
      }
      else  // finished
      {
        try
        {
          mapViewer.EndDrawNewOverlay();  // commit
          this.Refresh();
        }
        catch( Exception ex )
        {
          MapLoadFailed( "Failed to commit new map overlay", ex );
        }
        this.commitOverlayAfterScroll = false;
      }
    }

    // toggle map options
    private void cbMapOptions_CheckedChanged( object sender, EventArgs e )
    {
      tvwMapOptions.Visible = cbMapOptions.Checked;
      lblFacilities.Focus();  // remove button focus
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
    private bool recurseLock = false;
    private void tvwMapOptions_AfterCheck( object sender, TreeViewEventArgs e )
    {
      // update overlay params

      switch( e.Node.Name )
      {
        case "nodeAttackObjectives":     this.overlayParamsGameMap.showAttackObjectives     = e.Node.Checked; break;
        case "nodeAttackObjectiveNames": this.overlayParamsGameMap.showAttackObjectiveNames = e.Node.Checked; break;
        case "nodeFirebases":            this.overlayParamsGameMap.showFirebases            = e.Node.Checked; break;
        case "nodeAirfields":            this.overlayParamsGameMap.showAirfields            = e.Node.Checked; break;
        case "nodeSupplyLinks":          this.overlayParamsGameMap.showSupplyLinks          = e.Node.Checked; break;
        case "nodeBrigadeLinks":         this.overlayParamsGameMap.showBrigadeLinks         = e.Node.Checked; break;
        case "nodeBrigadeLinksSelected": this.overlayParamsGameMap.showBrigadeLinksSelected = e.Node.Checked; break;
        case "nodeBrigadeLinksArmy":     this.overlayParamsGameMap.showBrigadeLinksArmy     = e.Node.Checked; break;
        case "nodeAirGrid":
          this.overlayParamsGameMap.showAirGrid = e.Node.Checked;
          if( !e.Node.Checked )
            lblCell.Text = null;
          break;
        case "nodeFrontlines":           this.overlayParamsGameMap.showFrontlines           = e.Node.Checked; break;
        case "nodeAttackArrows":         this.overlayParamsGameMap.showAttackArrows         = e.Node.Checked; break;
        case "nodeAttackArrowsLinked":   this.overlayParamsGameMap.showAttackArrowsLinked   = e.Node.Checked; break;
        case "nodePlayerActivity":       this.overlayParamsGameMap.showPlayerActivity       = e.Node.Checked; break;
        case "nodeCountryBorders":       this.overlayParamsGameMap.showCountryBorders       = e.Node.Checked; break;
        case "nodeCountryBordersNames":  this.overlayParamsGameMap.showCountryBordersNames  = e.Node.Checked; break;
      }


      // sync parent/child check marks

      if( recurseLock ) return;
      recurseLock = true;

      // if selecting child, make sure parent is checked
      if( e.Node.Checked && e.Node.Parent != null && !e.Node.Parent.Checked )
        e.Node.Parent.Checked = true;

      // if unselecting parent, clear all children
      if( !e.Node.Checked )
        foreach( TreeNode node in e.Node.Nodes )
          node.Checked = false;

      recurseLock = false;


      // redraw

      UpdateMapOverlay();
    }

    // resize window
    private void GameMap_DetatchedChanged( object sender, EventArgs e )
    {
      Expando expGameMap = (Expando)this.Parent;
      bool nowDetatched = expGameMap.Detatched;

      picResizeCorner.Visible = nowDetatched;

      if( !nowDetatched )
      {
        SetDetatchedSize( this.MinimumSize );  // restore original size
        this.mapViewer.MapHighPerformanceMode = false;
      }
    }
    private void picResizeCorner_MouseDown( object sender, MouseEventArgs e )
    {
      // start resizing
      this.resizeWindowsStartSize = this.Size;
      this.resizeWindowStartMouse = Control.MousePosition;
      picResizeCorner.MouseMove += picResizeCorner_MouseMove;
      picResizeCorner.Capture = true;
      this.mapViewer.MapFastScaling = true;
    }
    private void picResizeCorner_MouseMove( object sender, MouseEventArgs e )
    {
      // calculate new window size

      Point newMouse = Control.MousePosition;

      Size newSize = new Size {
        Width = this.resizeWindowsStartSize.Width + (newMouse.X - this.resizeWindowStartMouse.X),
        Height = this.resizeWindowsStartSize.Height + (newMouse.Y - this.resizeWindowStartMouse.Y)
      };

      SetDetatchedSize( newSize );
    }
    private void picResizeCorner_MouseUp( object sender, MouseEventArgs e )
    {
      // stop resizing

      picResizeCorner.MouseMove -= picResizeCorner_MouseMove;
      picResizeCorner.Capture = false;
      this.mapViewer.MapFastScaling = false;
      this.mapViewer.Refresh();
    }

    #endregion
  }
}
