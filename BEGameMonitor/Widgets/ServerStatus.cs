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
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using BEGM.Properties;
using XPExplorerBar;
using Xiperware.WiretapAPI;

namespace BEGM.Widgets
{
  /// <summary>
  /// The ServerStatus widget displays info about the live server, such as current
  /// and historical population, captures and kills, as well as server state.
  /// </summary>
  public partial class ServerStatus : UserControl, IWidget
  {
    #region Variables

    private GameState game;
    private bool configExpanded = false;
    private Dictionary<string, ServerParam> prevServerParams;
    private MetaXmlFile prevServerConfigXmlFile;

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
    /// Create a new ServerStatus widget.
    /// </summary>
    public ServerStatus()
    {
      InitializeComponent();

      //// avoid default DataError dialog
      dgvConfig.DataError += ( sender, args ) => { };
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
    internal ServerStatusState State
    {
      get
      {
        ServerStatusState state = new ServerStatusState {
          showConfig = this.configExpanded
        };

        return state;
      }
      set
      {
        this.ConfigExpanded = value.showConfig;
      }
    }

    /// <summary>
    /// Sets whether the config section is visible or not, and updates the widget
    /// height to match.
    /// </summary>
    private bool ConfigExpanded
    {
      get { return this.configExpanded; }
      set
      {
        this.configExpanded = value;
        dgvConfig.Visible = lnkResetConfig.Visible = value;
        picConfig.Image = value ? Resources.icon_minus : Resources.icon_plus;

        UpdateWidgetHeight();
      }
    }

    /// <summary>
    /// Update the LinkBehavior when setting lnkEvents.Enabled.
    /// </summary>
    private bool LnkResetConfigEnabled
    {
      set
      {
        lnkResetConfig.Links[0].Enabled = value;
        lnkResetConfig.LinkBehavior = value ? LinkBehavior.HoverUnderline : LinkBehavior.NeverUnderline;
      }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Initialise the ServerStatus widget.
    /// </summary>
    /// <param name="game">The current game state.</param>
    public void InitWidget( GameState game )
    {
      Log.AddEntry( "  ServerStatus..." );

      this.game = game;


      // init prev server config

      /* <config>
       *   <r id="arena.campaign_id" val="45" desc="Current Campaign Number" />
       *   ...
       * </config>
       */
      this.prevServerConfigXmlFile = new MetaXmlFile( "config.prev.xml", "/config/r" );
      this.prevServerConfigXmlFile.PostParse += MetaXmlFile_PrevConfig_PostParse;
      this.prevServerConfigXmlFile.AttrNames = new string[] { "id", "val", "desc" };
      this.prevServerConfigXmlFile.AttrDefaults = new object[] { null, "(unknown)", "(unknown)" };


      // populate widget

      UpdateWidget();
      UpdateConfig();


      // set tooltips
      GameStatus.ToolTip.SetToolTip( picGaugePopulation, Language.ServerStatus_Tooltip_Population );
      GameStatus.ToolTip.SetToolTip( picGaugeKills, Language.ServerStatus_Tooltip_Kills );
      GameStatus.ToolTip.SetToolTip( picGraphKills, Language.ServerStatus_Tooltip_Kills );
      GameStatus.ToolTip.SetToolTip( picGaugeCaptures, Language.ServerStatus_Tooltip_Captures );
      GameStatus.ToolTip.SetToolTip( picGraphCaptures, Language.ServerStatus_Tooltip_Captures );


      Log.Okay();
    }

    /// <summary>
    /// Updates the various server status controls with the given data.
    /// </summary>
    public void UpdateWidget()
    {
      if( this.game == null ) return;


      // calc time ranges

      DateTime now = DateTime.Now;
      DateTime dateGraphEnd = new DateTime( now.Year, now.Month, now.Day, now.Hour, ( now.Minute / 5 ) * 5, 0 );  // interger division = round to last 5 min interval
      DateTime dateGraphStart = dateGraphEnd.AddHours( -1 );
      DateTime dateGaugeStart = now.AddHours( -1 );

      /* For both capture and death counts below, there are two subtly different 
       * time ranges used between the gauges and the graphs. The gauges are updated
       * every minute and show counts for the last hour, as normal. The graphs however
       * are only updated every 5 minutes and contain 12 datapoints each representing
       * a 5-minute window. To avoid the window changing and jumping around, they are
       * all aligned to 5 min intervals (0, 5, 10, 15, etc).
       */


      // count captures

      SideCount countCaptures = new SideCount();  // past 60 mins
      SideCount[] countCapturesGrouped = new SideCount[12];  // past 5-65 mins

      foreach( GameEvent gameEvent in game.Events )
      {
        ICaptureFacilityGameEvent capGameEvent = gameEvent as ICaptureFacilityGameEvent;

        if( capGameEvent == null ) continue;
        if( gameEvent.EventTime < dateGaugeStart ) continue;

        if( capGameEvent.NewOwner.Side == Side.Allied )
          countCaptures.Add( 1, 0 );
        else
          countCaptures.Add( 0, 1 );

        if( gameEvent.EventTime < dateGraphStart || gameEvent.EventTime > dateGraphEnd ) continue;

        int age = (int)Math.Round( ( dateGraphEnd - gameEvent.EventTime ).TotalMinutes / 5 ) - 1;  // 0 - 11
        if( age < 0 || age > 11 ) continue;  // shouldn't happen

        if( capGameEvent.NewOwner.Side == Side.Allied )
          countCapturesGrouped[age].Add( 1, 0 );
        else
          countCapturesGrouped[age].Add( 0, 1 );
      }


      // count deaths

      SideCount countDeaths = new SideCount();  // past 60 mins
      SideCount[] countDeathsGrouped = new SideCount[12];  // past 5-65 mins

      foreach( MapCell cell in game.MapCells.Values )
      {
        countDeaths.Add( cell.Deaths );

        SideCount[] cellDeaths = cell.DeathsGrouped;
        for( int i = 0; i < cellDeaths.Length; i++ )
          countDeathsGrouped[i].Add( cellDeaths[i] );
      }


      // swap deaths for kills (not 100% accurate, but near enough)

      SideCount countKills = new SideCount( countDeaths.Axis, countDeaths.Allied );
      SideCount[] countKillsGrouped = new SideCount[countDeathsGrouped.Length];

      for( int i = 0; i < countKillsGrouped.Length; i++ )
        countKillsGrouped[i] = new SideCount( countDeathsGrouped[i].Axis, countDeathsGrouped[i].Allied );


      // count cps, fbs, AOs

      SideCount countChokePoints = new SideCount();
      SideCount countFirebases = new SideCount();
      SideCount countAOs = new SideCount();

      foreach( ChokePoint cp in game.ValidChokePoints )
      {
        if( cp.Owner.Side == Side.Allied )
        {
          countChokePoints.Allied++;
          if( cp.HasAO )
            countAOs.Axis++;
        }
        else if( cp.Owner.Side == Side.Axis )
        {
          countChokePoints.Axis++;
          if( cp.HasAO )
            countAOs.Allied++;
        }
      }

      foreach( Firebase fb in game.Firebases.Values )
      {
        if( !fb.IsOpen ) continue;

        if( fb.Link.Side == Side.Allied )
          countFirebases.Allied++;
        else if( fb.Link.Side == Side.Axis )
          countFirebases.Axis++;
      }


      // update controls

      if( game.Servers.ContainsKey( 1 ) )
      {
        lblPopulationValue.Text = Misc.EnumString( game.Servers[1].Population );
        int pop = (int)game.Servers[1].Population;  // 0 - 6
        picGaugePopulation.Image = DrawGauge( new SideCount( 6 - pop, pop ), false );

        if( game.Servers[1].Online )
          picServerState.Image = Resources.server_online;
        else if( game.Servers[1].Locked )
          picServerState.Image = Resources.server_locked;
        else if( game.Servers[1].Offline )
          picServerState.Image = Resources.server_offline;
        else
          picServerState.Image = Resources.server_other;

        lblServerState.Text = game.Servers[1].State;
        GameStatus.ToolTip.SetToolTip( lblServerState, game.Servers[1].StateInfo );
        GameStatus.ToolTip.SetToolTip( picServerState, game.Servers[1].StateInfo );
      }

      lblKillsAllied.Text = countKills.Allied.ToString();
      lblKillsAxis.Text = countKills.Axis.ToString();
      picGaugeKills.Image = DrawGauge( countKills, true );

      lblCapturesAllied.Text = countCaptures.Allied.ToString();
      lblCapturesAxis.Text = countCaptures.Axis.ToString();
      picGaugeCaptures.Image = DrawGauge( countCaptures, true );

      picGraphKills.Image = DrawGraph( countKillsGrouped, true );
      picGraphCaptures.Image = DrawGraph( countCapturesGrouped, false );

      lblChokePointsAllied.Text = countChokePoints.Allied.ToString();
      lblChokePointsAxis.Text = countChokePoints.Axis.ToString();
      picGaugeChokePoints.Image = DrawGauge( countChokePoints, false );

      lblFirebasesAllied.Text = countFirebases.Allied.ToString();
      lblFirebasesAxis.Text = countFirebases.Axis.ToString();
      picGaugeFirebases.Image = DrawGauge( countFirebases, false );

      lblAOsAllied.Text = countAOs.Allied.ToString();
      lblAOsAxis.Text = countAOs.Axis.ToString();
      picGaugeAOs.Image = DrawGauge( countAOs, false );
    }

    /// <summary>
    /// Remove references to local game objects.
    /// </summary>
    public void Cleanup()
    {
      this.game = null;
      if( this.prevServerParams != null )
      {
        this.prevServerParams.Clear();
        this.prevServerParams = null;
      }
    }

    /// <summary>
    /// Not Implemented.
    /// </summary>
    /// <param name="arg"></param>
    public void Reveal( object arg )
    {
      throw new NotImplementedException();
    }



    /// <summary>
    /// Updates the configuration section.
    /// </summary>
    private void UpdateConfig()
    {
      // get stored prev server config

      string currConfigFile = Path.Combine( Wiretap.CachePath, "config.xml" );
      string prevConfigFile = Path.Combine( Wiretap.CachePath, "config.prev.xml" );
      DateTime trackChangeTime;

      try
      {
        if( File.Exists( prevConfigFile ) )  // exists, re-parse
        {
          this.prevServerConfigXmlFile.Parse();  // repopulates this.prevServerParams
          trackChangeTime = new FileInfo( prevConfigFile ).LastWriteTime;
        }
        else  // doesn't exist, make initial copy
        {
          if( File.Exists( currConfigFile ) )
            File.Copy( currConfigFile, prevConfigFile );

          this.prevServerParams = null;  // null = no changes
          trackChangeTime = DateTime.Now;
        }
      }
      catch( Exception ex )
      {
        Log.AddException( ex );
        MessageBox.Show( "Error updating server config: " + ex.Message, Language.Error_Error, MessageBoxButtons.OK, MessageBoxIcon.Error );
        return;
      }

      GameStatus.ToolTip.SetToolTip( lnkResetConfig, String.Format( Language.ServerStatus_Tooltip_TrackingChangesSince, Misc.FormatDateLong( trackChangeTime ) ) );


      // populate list with items

      List<ServerParam> items = new List<ServerParam>();
      if( this.game != null )
        items.AddRange( this.game.ServerParams.Values );
      items.Sort();

      dgvConfig.Rows.Clear();

      DataGridViewCellStyle sectionStyle = dgvConfig.DefaultCellStyle.Clone();
      sectionStyle.Font = new Font( sectionStyle.Font, FontStyle.Bold );
      sectionStyle.BackColor = sectionStyle.SelectionBackColor = Color.FromArgb( 32, 32, 32 );

      string prevSection = null;
      bool anyDifferent = false;
      foreach( ServerParam item in items )
      {
        if( item.Key == "arena.campaign_start" || item.Key == "arena.campaign_id" || item.Key == "arena.intermission" ) continue;  // skip these


        // add section row (if new section), item row

        int idx;
        if( item.Section != prevSection )  // section heading
        {
          idx = dgvConfig.Rows.Add( item.Section );
          dgvConfig.Rows[idx].DefaultCellStyle = sectionStyle;
          dgvConfig.Rows[idx].Tag = false;
          prevSection = item.Section;
        }

        idx = dgvConfig.Rows.Add( item.Description, item.Value );


        // check if different to stored value, store in tag, set tooltip

        bool different = false;
        if( this.prevServerParams != null && this.prevServerParams.ContainsKey( item.Key ) && item != this.prevServerParams[item.Key] )
          different = true;

        dgvConfig.Rows[idx].Tag = different;

        if( different )
        {
          anyDifferent = true;
          dgvConfig[1, idx].ToolTipText = String.Format( Language.ServerStatus_Tooltip_UsedToBe, this.prevServerParams[item.Key].Value );
        }
        else
          dgvConfig[1, idx].ToolTipText = null;
      }


      // link is only enabled when there are changed items

      this.LnkResetConfigEnabled = anyDifferent;
    }

    /// <summary>
    /// Create a mini gauge image with the given data.
    /// </summary>
    /// <param name="count">The side counts that determine the needle angle.</param>
    /// <param name="doubleScale">If true the needle angle will be exaggerated x 2.</param>
    /// <returns>An image containing the gauge needle (doesn't include the gauge background).</returns>
    private Bitmap DrawGauge( SideCount count, bool doubleScale )
    {
      const int radius = 25;
      const int left = 5;
      const int top = 5;


      // calculate needle angle

      float angle;
      if( count.Allied != 0 || count.Axis != 0 )
        angle = ( (float)count.Axis / ( count.Allied + count.Axis ) ) * 180;
      else
        angle = 90;

      if( doubleScale )
        angle += ( angle - 90 );  // exaggerate angle x2

      if( angle < 10 ) angle = 10;
      if( angle > 170 ) angle = 170;


      // get points to draw

      PointF needleCenter = new PointF( left + radius - 0.5F, top + radius - 1 );
      PointF needleStart = Misc.AngleOffset( needleCenter, angle - 90, radius * 0.4 );
      PointF needleEnd = Misc.AngleOffset( needleCenter, angle - 270, radius * 1.1 );


      // init graphics resources

      Pen penNeedle = new Pen( Color.FromArgb( 192, 192, 192 ), 2F );

      Bitmap img = new Bitmap( 60, 40 );
      Graphics g = Graphics.FromImage( img );
      g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;


      // draw gauge

      g.DrawLine( penNeedle, needleStart, needleEnd );

      g.Dispose();
      return img;
    }

    /// <summary>
    /// Create a mini line graph image with the given data.
    /// </summary>
    /// <param name="data">An array of SideCount data to plot on the graph.</param>
    /// <param name="removeGaps">If true will smooth any 0 values (see comment).</param>
    /// <returns>An image containing the graph.</returns>
    private Bitmap DrawGraph( SideCount[] data, bool removeGaps )
    {
      const int width = 72;
      const int height = 25;


      // remove gaps

      /* Because the init deaths xml data retrieved on startup is grouped by 10 mins,
       * every second 5 min interval here is 0 when the program starts. To avoid
       * this we simply replace any 0's with the average of their neighbours.
       */

      if( removeGaps )
      {
        for( int i = 0; i < data.Length; i++ )
        {
          if( i == 0 )  // first
          {
            if( data[i].Allied == 0 ) data[i].Allied = data[i + 1].Allied;
            if( data[i].Axis   == 0 ) data[i].Axis   = data[i + 1].Axis;
          }
          else if( i == data.Length - 1 )  // last
          {
            if( data[i].Allied == 0 ) data[i].Allied = data[i - 1].Allied;
            if( data[i].Axis   == 0 ) data[i].Axis   = data[i - 1].Axis;
          }
          else  // middle
          {
            if( data[i].Allied == 0 ) data[i].Allied = (int)Math.Round( ( data[i - 1].Allied + data[i + 1].Allied ) / 2F );
            if( data[i].Axis   == 0 ) data[i].Axis   = (int)Math.Round( ( data[i - 1].Axis   + data[i + 1].Axis   ) / 2F );
          }
        }
      }


      // smooth line (3-point moving average)

      SideCountF[] dataSmoothed = new SideCountF[data.Length];
      dataSmoothed[0] = data[0];  // first
      for( int i = 1; i < data.Length - 1; i++ )  // middle
      {
        dataSmoothed[i].Allied = (float)Math.Round( ( data[i - 1].Allied + data[i].Allied + data[i + 1].Allied ) / 3F );
        dataSmoothed[i].Axis = (float)Math.Round( ( data[i - 1].Axis + data[i].Axis + data[i + 1].Axis ) / 3F );
      }
      dataSmoothed[data.Length - 1] = data[data.Length - 1];  // last


      // get max y value

      float maxValue = 0;
      foreach( SideCountF count in dataSmoothed )
      {
        if( count.Allied > maxValue ) maxValue = count.Allied;
        if( count.Axis   > maxValue ) maxValue = count.Axis;
      }
      if( maxValue < 3 )
        maxValue = 3;

      float pixelsPerUnit = (float)width / (float)data.Length;


      // create points to plot

      PointF[] pointsAllied = new PointF[data.Length + 2];
      PointF[] pointsAxis = new PointF[data.Length + 2];

      for( int i = 0; i < dataSmoothed.Length; i++ )
      {
        float x = ( pixelsPerUnit / 2 ) + ( pixelsPerUnit * ( dataSmoothed.Length - 1 - i ) );  // backwards

        pointsAllied[i + 1] = new PointF( x, height - ( ( dataSmoothed[i].Allied / maxValue ) * ( height - 5 ) ) - 3 );
        pointsAxis[i + 1] = new PointF( x, height - ( ( dataSmoothed[i].Axis / maxValue ) * ( height - 5 ) ) - 3 );
      }


      // add first/last points

      pointsAllied[0] = new PointF( width - 2, pointsAllied[1].Y );
      pointsAllied[pointsAllied.Length - 1] = new PointF( 1, pointsAllied[pointsAllied.Length - 2].Y );
      pointsAxis[0] = new PointF( width - 2, pointsAxis[1].Y );
      pointsAxis[pointsAxis.Length - 1] = new PointF( 1, pointsAxis[pointsAxis.Length - 2].Y );


      // init graphics resources

      Pen penAllied = new Pen( Color.FromArgb( 255, 80, 80, 255 ) );
      Pen penAxis = new Pen( Color.FromArgb( 200, 255, 50, 50 ) );
      Pen penFrame = new Pen( Color.FromArgb( 64, 64, 64 ) );
      Brush brushBack = new SolidBrush( Color.FromArgb( 32, 32, 32 ) );

      Bitmap img = new Bitmap( width, height );
      Graphics g = Graphics.FromImage( img );
      g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;


      // draw graph

      g.FillRectangle( brushBack, 0, 0, width - 1, height - 1 );
      g.DrawRectangle( penFrame, 0, 0, width - 1, height - 1 );

      g.DrawCurve( penAllied, pointsAllied );
      g.DrawCurve( penAxis, pointsAxis );

      g.Dispose();
      return img;
    }

    /// <summary>
    /// Resizes the widget and various controls to fit the current contents.
    /// </summary>
    private void UpdateWidgetHeight()
    {
      Expando expServerStatus = (Expando)this.Parent;

      int newHeight = 23 + 4;

      if( this.configExpanded )
        newHeight += dgvConfig.Bottom;
      else
        newHeight += lnkConfig.Bottom;

      if( expServerStatus.ExpandedHeight != newHeight )
      {
        expServerStatus.SuspendLayout();
        expServerStatus.ExpandedHeight = newHeight;
        expServerStatus.Height += 1;  // workaround for bug in tskMain ScrollableControl:
        expServerStatus.Height -= 1;  // force the correct scroll height
        expServerStatus.ResumeLayout();
      }
    }

    /// <summary>
    /// Parse the local prev config metadata and create a list of ServerParam objects.
    /// </summary>
    private void MetaXmlFile_PrevConfig_PostParse( object sender, PostParseEventArgs e )
    {
      /* <config>
       *   <r id="arena.campaign_id" val="45" desc="Current Campaign Number" />
       *   ...
       * </config>
       */


      // initialise game object collections

      this.prevServerParams = new Dictionary<string, ServerParam>();


      // loop over xml data

      Exception firstException = null;
      for( int i = 0; i < e.Data.Count; i++ )
      {
        e.Data.Offset = i;

        try
        {
          // parse and validate attribute values

          string key = e.Data.GetValue<string>( "id" );
          string value = e.Data.GetValue<string>( "val" );
          string desc = e.Data.GetValue<string>( "desc", key );


          // create game objects

          this.prevServerParams.Add( key, new ServerParam( key, value, desc ) );
        }
        catch( Exception ex )
        {
          if( firstException == null )
            firstException = ex;
        }

      }  // for e.Data


      // if any errors, throw first

      if( firstException != null )
        throw firstException;
    }

    #endregion

    #region Event Handlers

    // toggle config section
    private void lnkConfig_LinkClicked( object sender, LinkLabelLinkClickedEventArgs e )
    {
      this.ConfigExpanded = !this.ConfigExpanded;
    }
    private void picConfig_Click( object sender, EventArgs e )
    {
      this.ConfigExpanded = !this.ConfigExpanded;
    }

    // reset config change markers
    private void lnkResetConfig_LinkClicked( object sender, LinkLabelLinkClickedEventArgs e )
    {
      string prevConfigFile = Path.Combine( Wiretap.CachePath, "config.prev.xml" );

      File.Delete( prevConfigFile );
      UpdateConfig();
    }

    // draw tick/cross images, and changed markers
    private void dgvConfig_CellPainting( object sender, DataGridViewCellPaintingEventArgs e )
    {
      if( e.ColumnIndex != 1 ) return;

      string value = (string)dgvConfig[e.ColumnIndex, e.RowIndex].Value;
      bool drawImage = value == "y" || value == "n";
      bool drawBorder = (bool)dgvConfig.Rows[e.RowIndex].Tag;  // row is different

      if( !drawImage && !drawBorder ) return;

      e.PaintBackground( e.ClipBounds, false );
      int y = ( e.CellBounds.Y == 0 ? 1 : e.CellBounds.Y ) + 2;

      if( drawImage )
      {
        Image image = value == "y" ? Resources.tick : Resources.cross;
        int x = e.CellBounds.X + ( e.CellBounds.Width - image.Width ) / 2;
        e.Graphics.DrawImage( image, x, y + 1.5F, image.Width, image.Height );
      }
      if( drawBorder )
        e.Graphics.DrawRectangle( Pens.DarkRed, e.CellBounds.X + 2, y, e.CellBounds.Width - 6, 16 );
      if( !drawImage )
        e.PaintContent( e.ClipBounds );

      e.Handled = true;
    }

    // dynamic focus
    private void ServerStatus_MouseEnter( object sender, EventArgs e )
    {
      BegmMisc.FocusTaskPane( this );
    }
    private void dgvConfig_MouseEnter( object sender, EventArgs e )
    {
      BegmMisc.FocusWithoutScroll( dgvConfig );
    }

    #endregion
  }
}
