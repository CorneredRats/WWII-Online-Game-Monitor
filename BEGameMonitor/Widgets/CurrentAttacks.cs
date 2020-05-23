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
using System.Linq;
using System.Windows.Forms;
using BEGM.Properties;
using XPExplorerBar;
using Xiperware.WiretapAPI;

namespace BEGM.Widgets
{
  /// <summary>
  /// The CurrentAttacks widget groups the game events by chokepoint, and provides the user
  /// an easy way to view the current &amp; recent battles, and the history of each battle.
  /// Provides a link to the TownStatus widget, and an interface to select which cps to
  /// receive capture alerts for.
  /// </summary>
  public partial class CurrentAttacks : UserControl, IWidget
  {
    #region Variables

    /// <summary>
    /// The current game state.
    /// </summary>
    private GameState game;

    /// <summary>
    /// The height of an non-collapsed, but not fully expanded, chokepoint expando.
    /// </summary>
    private const int EXPANDO_DEFAULT_HEIGHT = 165;

    /// <summary>
    /// A hash of currently displayed ChokePoints and their associated Expando controls.
    /// </summary>
    private Dictionary<ChokePoint, Expando> expandos;

    /// <summary>
    /// Order the events in the event list are displayed.
    /// </summary>
    private SortOrder eventSortOrder = SortOrder.Ascending;

    /// <summary>
    /// The current game options.
    /// </summary>
    /// <remarks>Needed to get/set alert filter settings.</remarks>
    public Options options;

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
    /// Create a new CurrentAttacks widget.
    /// </summary>
    public CurrentAttacks()
    {
      InitializeComponent();

      this.expandos = new Dictionary<ChokePoint, Expando>();


      // VS designer bug

      tskAttacks.CustomSettings.Padding = new XPExplorerBar.Padding( 0, 0, 0, 4 );
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
    /// Gets or sets the order the events in the event lists are displayed.
    /// </summary>
    internal SortOrder EventListSortOrder
    {
      get { return this.eventSortOrder; }
      set
      {
        this.eventSortOrder = value;

        // update sort order
        UpdateWidget( true );

        // reset scroll position
        foreach( Expando expando in expandos.Values )
        {
          CAControls controls = (CAControls)expando.Tag;
          if( controls.dgvEventList.Rows.Count > 0 )
            controls.dgvEventList.FirstDisplayedScrollingRowIndex = this.eventSortOrder == SortOrder.Ascending ? controls.dgvEventList.Rows.Count - 1 : 0;
        }
      }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Initialise the CurrentAttacks widget.
    /// </summary>
    /// <param name="game">The current game state.</param>
    public void InitWidget( GameState game )
    {
      Log.AddEntry( "  CurrentAttacks..." );

      this.game = game;


      // set tooltips

      GameStatus.ToolTip.SetToolTip( picExpand, Language.CurrentAttacks_Tooltip_ExpandAll );
      GameStatus.ToolTip.SetToolTip( picReset, Language.CurrentAttacks_Tooltip_ExpandContested );
      GameStatus.ToolTip.SetToolTip( picCollapse, Language.CurrentAttacks_Tooltip_CollapseAll );


      // populate widget

      UpdateWidget( true );


      // set state of "show all alerts" checkbox

      UpdateShowAllAlertsCheckbox();


      // expand (without animation) any contested cps

      foreach( KeyValuePair<ChokePoint, Expando> keyvalue in expandos )
        if( keyvalue.Key.IsContested )
          keyvalue.Value.Expand();


      // need to recalc after doing expand above

      tskAttacks.DoLayout();  // bug workaround: expCurrentAttacks.ExpandedHeight initially (2px * expandos) too big?!
      ( (Expando)this.Parent ).CalcAnimationHeights();


      Log.Okay();
    }

    /// <summary>
    /// Updates the CurrentAttacks widget when there is new capture data in GameEvents.List.
    /// Adds/removes expandos as needed, and updates each event list.
    /// </summary>
    public void UpdateWidget()
    {
      UpdateWidget( false );
    }

    /// <summary>
    /// Updates the CurrentAttacks widget when there is new capture data in GameEvents.List.
    /// Adds/removes expandos as needed, and updates each event list.
    /// </summary>
    /// <param name="init">If false, newly added ChokePoints will be expanded.</param>
    private void UpdateWidget( bool init )
    {
      if( this.game == null )
        return;  // not loaded


      // get sorted list of cp's to display

      List<ChokePoint> cplist = GetChokePointsToDisplay();
      cplist.Sort();


      // add any new expandos

      bool expandosAdded = false;
      if( init || game.Wiretap.NewCaptureData || game.Wiretap.NewHCUnitData )
      {
        foreach( ChokePoint cp in cplist )
        {
          if( expandos.ContainsKey( cp ) ) continue;

          Expando newExpando = CreateExpando( cp );
          tskAttacks.Expandos.Add( newExpando );
          expandos.Add( cp, newExpando );

          if( !init )
            Log.AddEntry( "Adding {0} to current attacks", cp );

          expandosAdded = true;
        }
      }


      // remove any old expandos

      ChokePoint[] keys = new ChokePoint[expandos.Keys.Count];
      expandos.Keys.CopyTo( keys, 0 );  // must copy as we can't modify while enumerating
      tmrRemoveChokePoint.Tag = new List<Expando>();
      foreach( ChokePoint cp in keys )
      {
        if( cplist.Contains( cp ) ) continue;

        Expando oldExpando = expandos[cp];
        expandos.Remove( cp );
        if( oldExpando.Collapsed && !oldExpando.Animating )  // already collapsed, remove
        {
          tskAttacks.Expandos.Remove( oldExpando );
        }
        else  // animate collapsed, start timer to remove when finished
        {
          oldExpando.Collapsed = true;
          ( (List<Expando>)tmrRemoveChokePoint.Tag ).Add( oldExpando );
          tmrRemoveChokePoint.Start();
        }

        if( !init )
          Log.AddEntry( "Removing {0} from current attacks", cp );
      }


      // sort expandos, if we've added any

      if( expandosAdded )
      {
        for( int i = 0; i < cplist.Count; i++ )
        {
          Expando expando = expandos[cplist[i]];
          tskAttacks.Expandos.Move( expando, i );
        }
      }


      // update each chokepoint

      if( init || game.Wiretap.NewCaptureData || game.Wiretap.NewHCUnitData )  // update everything, repopulate event list
      {
        foreach( ChokePoint cp in cplist )
          UpdateChokePoint( cp );
      }
      else  // just refresh "x mins ago" tooltips
      {
        foreach( ChokePoint cp in cplist )
          UpdateTooltip( cp );
      }


      // update parent expando's animation heights

      ( (Expando)this.Parent ).CalcAnimationHeights();
    }

    /// <summary>
    /// Remove references to local game objects.
    /// </summary>
    public void Cleanup()
    {
      this.game = null;
      this.expandos.Clear();
      this.tskAttacks.Expandos.Clear();
    }

    /// <summary>
    /// Makes a chokepoint within the Current Attacks widget visible by making sure both
    /// the widget and chokepoint expandos are expanded, scrolling it into view and flashing
    /// the chokepoint title.
    /// </summary>
    /// <seealso cref="tmrReveal_Tick"/>
    /// <param name="arg">The ChokePoint to make visible.</param>
    public void Reveal( object arg )
    {
      ChokePoint cp = arg as ChokePoint;
      if( cp == null ) return;

      if( !expandos.ContainsKey( cp ) )
        return;  // not shown in current attacks list
      if( tmrReveal.Enabled )
        return;  // another Reveal() in progress

      Expando expChokepoint = expandos[cp];
      Expando expCurrentAttacks = (Expando)this.Parent;

      if( !expChokepoint.CanCollapse )
        return;  // cp list disabled, no events

      if( expCurrentAttacks.Collapsed )
      {
        expChokepoint.Expand();  // no animation
        expCurrentAttacks.Collapsed = false;
      }
      if( expChokepoint.Collapsed )
        expChokepoint.Collapsed = false;  // animate

      tmrReveal.Tag = expChokepoint;
      tmrReveal.Start();
    }



    /// <summary>
    /// Returns the number of events currently shown in a specific ChokePoint's event list.
    /// </summary>
    /// <param name="cp">The ChokePoint to get the displayed event count for.</param>
    /// <returns>0 if none or not found, >0 otherwise.</returns>
    public int NumEvents( ChokePoint cp )
    {
      Expando expChokepoint;
      if( !expandos.TryGetValue( cp, out expChokepoint ) )
        return 0;  // not shown in current attacks list

      return ( (CAControls)expChokepoint.Tag ).dgvEventList.Rows.Count;
    }

    /// <summary>
    /// Create a new Expando control with the appropriate settings and sub-controls, based on the given ChokePoint.
    /// </summary>
    /// <param name="cp">The ChokePoint the Expando will represent.</param>
    /// <returns>The new Expando control</returns>
    private Expando CreateExpando( ChokePoint cp )
    {
      // control container

      CAControls controls = new CAControls();


      // expando

      Expando expando = new Expando();
      expando.Animate = true;
      expando.Collapse();
      expando.CustomHeaderSettings.BackImageHeight = 25;
      expando.CustomHeaderSettings.NormalGradientEndColor = Color.FromArgb( 51, 51, 51 );
      expando.CustomHeaderSettings.NormalGradientStartColor = Color.FromArgb( 51, 51, 51 );
      expando.CustomHeaderSettings.NormalTitleColor = Color.FromArgb( 224, 224, 224 );
      expando.CustomHeaderSettings.NormalTitleHotColor = Color.White;
      expando.CustomHeaderSettings.TitleFont = new Font( "Arial", 9F, FontStyle.Bold, GraphicsUnit.Point, 0 );
      expando.CustomHeaderSettings.TitleGradient = true;
      expando.CustomHeaderSettings.TitleRadius = 0;
      expando.CustomSettings.NormalBackColor = Color.Black;
      expando.CustomSettings.NormalBorderColor = Color.Black;
      expando.ExpandedHeight = EXPANDO_DEFAULT_HEIGHT;
      expando.Text = cp.Name;


      // event datagridview

      controls.dgvEventList = new DataGridView();
      controls.dgvEventList.AllowUserToAddRows = false;
      controls.dgvEventList.AllowUserToDeleteRows = false;
      controls.dgvEventList.AllowUserToResizeColumns = false;
      controls.dgvEventList.AllowUserToResizeRows = false;
      controls.dgvEventList.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
      controls.dgvEventList.BackgroundColor = Color.Black;
      controls.dgvEventList.BorderStyle = BorderStyle.None;
      controls.dgvEventList.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
      controls.dgvEventList.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
      controls.dgvEventList.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      controls.dgvEventList.ColumnHeadersVisible = false;
      DataGridViewCellStyle cellStyle1 = new DataGridViewCellStyle();
      cellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
      cellStyle1.BackColor = Color.Black;
      cellStyle1.Font = new Font( "Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0 );
      cellStyle1.ForeColor = Color.FromArgb( 224, 224, 224 );
      cellStyle1.SelectionBackColor = Color.Black;
      cellStyle1.SelectionForeColor = Color.FromArgb( 224, 224, 224 );
      cellStyle1.WrapMode = DataGridViewTriState.False;
      controls.dgvEventList.DefaultCellStyle = cellStyle1;
      controls.dgvEventList.GridColor = Color.FromArgb( 64, 64, 64 );
      controls.dgvEventList.Location = new Point( 3, 21 + 16 );  // 21 = header
      controls.dgvEventList.MultiSelect = false;
      controls.dgvEventList.RowHeadersVisible = false;
      controls.dgvEventList.ScrollBars = ScrollBars.Vertical;
      controls.dgvEventList.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
      controls.dgvEventList.Size = new Size( 330, 128 );
      controls.dgvEventList.StandardTab = true;
      controls.dgvEventList.DataError += ( sender, args ) => { };  //// avoid default DataError dialog

      DataGridViewImageColumn imagecol = new DataGridViewImageColumn();
      imagecol.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
      DataGridViewCellStyle cellStyle2 = new DataGridViewCellStyle();
      cellStyle2.Alignment = DataGridViewContentAlignment.TopLeft;
      cellStyle2.Padding = new System.Windows.Forms.Padding( 0, 2, 0, 0 );
      imagecol.DefaultCellStyle = cellStyle2;
      imagecol.ImageLayout = DataGridViewImageCellLayout.Zoom;
      imagecol.ReadOnly = true;
      imagecol.Resizable = DataGridViewTriState.False;
      imagecol.Width = 20;
      controls.dgvEventList.Columns.Add( imagecol );

      DataGridViewTextBoxColumn textcol = new DataGridViewTextBoxColumn();
      textcol.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
      DataGridViewCellStyle cellStyle3 = new DataGridViewCellStyle();
      cellStyle3.Padding = new System.Windows.Forms.Padding( 0, 0, 0, 2 );
      cellStyle3.WrapMode = DataGridViewTriState.True;
      textcol.DefaultCellStyle = cellStyle3;
      textcol.ReadOnly = true;
      textcol.Resizable = DataGridViewTriState.False;
      textcol.SortMode = DataGridViewColumnSortMode.NotSortable;
      controls.dgvEventList.Columns.Add( textcol );

      // focus on mouse enter
      expando.MouseEnter += delegate { BegmMisc.FocusWithoutScroll( controls.dgvEventList ); };
      controls.dgvEventList.MouseEnter += delegate { BegmMisc.FocusWithoutScroll( controls.dgvEventList ); };

      // don't draw focus rectangle
      controls.dgvEventList.RowPrePaint += delegate( object sender, DataGridViewRowPrePaintEventArgs e ) { e.PaintParts &= ~DataGridViewPaintParts.Focus; };

      // reveal other widgets on double click
      controls.dgvEventList.CellDoubleClick += dgvEventList_CellDoubleClick;

      expando.Items.Add( controls.dgvEventList );


      // event checkbox

      controls.cbShowAlerts = new CheckBox();
      controls.cbShowAlerts.AutoSize = true;
      controls.cbShowAlerts.Text = Language.CurrentAttacks_ShowAlerts;
      
      if( Application.RenderWithVisualStyles )
        controls.cbShowAlerts.Location = new Point( 2, 21 + 2 );
      else
        controls.cbShowAlerts.Location = new Point( 2, 21 + 4 );

      if( options.Alerts.filterChokePoint == false ||             // not filtering, or
          options.Alerts.filterChokePointIDs.Contains( cp.ID ) )  // filtering and includes this cp
        controls.cbShowAlerts.Checked = true;

      controls.cbShowAlerts.Click += delegate { cbShowAlerts_Click( controls.cbShowAlerts, cp ); };

      expando.Controls.Add( controls.cbShowAlerts );


      // capbar

      controls.picCapBar = new PictureBox();
      controls.picCapBar.Location = new Point( 104, 21 + 2 );
      controls.picCapBar.Size = new Size( 130, 12 );
      expando.Controls.Add( controls.picCapBar );


      // expand button

      controls.picExpandTown = new PictureBox();
      controls.picExpandTown.Cursor = Cursors.Hand;
      controls.picExpandTown.Image = Resources.icon_down;
      if( Application.RenderWithVisualStyles )
        controls.picExpandTown.Location = new Point( 324, 21 + 5 );
      else
        controls.picExpandTown.Location = new Point( 324, 21 + 7 );
      controls.picExpandTown.SizeMode = PictureBoxSizeMode.AutoSize;

      controls.picExpandTown.MouseClick += delegate ( object obj, MouseEventArgs e ) { if( e.Button == MouseButtons.Left ) picExpandTown_Click( expando ); };

      expando.Controls.Add( controls.picExpandTown );


      // town status link

      LinkLabel link = new LinkLabel();
      link.ActiveLinkColor = Color.White;
      link.LinkBehavior = LinkBehavior.HoverUnderline;
      link.LinkColor = Color.FromArgb( 224, 224, 224 );
      link.Text = Language.CurrentAttacks_TownStatus;
      link.TextAlign = ContentAlignment.TopRight;
      link.Size = new Size( 90, 13 );
      link.VisitedLinkColor = Color.FromArgb( 224, 224, 224 );

      if( Application.RenderWithVisualStyles )
        link.Location = new Point( 235, 21 + 1 );
      else
        link.Location = new Point( 235, 21 + 2 );

      link.LinkClicked += delegate { GameStatus_RevealWidget( WidgetType.TownStatus, cp ); };

      expando.Controls.Add( link );


      // store controls for future reference

      expando.Tag = controls;

      return expando;
    }

    /// <summary>
    /// Changes the height of the town's event list expando. Normally limited to 4-5 rows, when
    /// expanded will autosize to display all rows. (Unrelated to the Expando collapsed
    /// state.)
    /// </summary>
    /// <param name="expChokePoint">The Expando to modify.</param>
    /// <param name="expand">True to expand or update expanded height, false to collapse.</param>
    private void SetListExpanded( Expando expChokePoint, bool expand )
    {
      Expando expCurrentAttacks = (Expando)this.Parent;
      CAControls controls = (CAControls)expChokePoint.Tag;
      const int eventListDefaultHeight = 128;

      if( expand )
      {
        // remove scrollbar to get correct PreferredSize, and also to
        // prevent it flickering visible when adding a new events
        controls.dgvEventList.ScrollBars = ScrollBars.None;

        int offset = ( EXPANDO_DEFAULT_HEIGHT - eventListDefaultHeight );
        int newsize = controls.dgvEventList.PreferredSize.Height + offset;
        if( newsize <= EXPANDO_DEFAULT_HEIGHT )
        {
          controls.dgvEventList.ScrollBars = ScrollBars.Vertical;
          return;
        }

        expCurrentAttacks.SuspendLayout();
        expChokePoint.ExpandedHeight = newsize;
        expCurrentAttacks.Height += 1;  // workaround for bug in tskMain ScrollableControl:
        expCurrentAttacks.Height -= 1;  // force the correct scroll height
        controls.dgvEventList.Height = newsize - offset;
        controls.picExpandTown.Image = Resources.icon_up;
        expCurrentAttacks.ResumeLayout();
      }
      else
      {
        expCurrentAttacks.SuspendLayout();
        expChokePoint.ExpandedHeight = EXPANDO_DEFAULT_HEIGHT;
        expCurrentAttacks.Height += 1;  // workaround for bug in tskMain ScrollableControl:
        expCurrentAttacks.Height -= 1;  // force the correct scroll height
        controls.dgvEventList.Height = eventListDefaultHeight;
        controls.picExpandTown.Image = Resources.icon_down;
        controls.dgvEventList.ScrollBars = ScrollBars.Vertical;
        expCurrentAttacks.ResumeLayout();
      }

      expCurrentAttacks.CalcAnimationHeights();
    }

    /// <summary>
    /// Updates the given ChokePoint's Expando.
    /// </summary>
    /// <param name="cp">The ChokePoint to update.</param>
    private void UpdateChokePoint( ChokePoint cp )
    {
      Expando expando = expandos[cp];
      CAControls controls = (CAControls)expando.Tag;

      
      // update title image

      expando.TitleImage = cp.FlagImage;


      // update attack objective/activity level icon

      Image icon;

      if( cp.HasAO )
      {
        icon = new Bitmap( 45, 18 );  // 21x18 + 20x18
        Graphics g = Graphics.FromImage( icon );
        g.DrawImage( Resources.attack_objective, 0, 0, 21, 18 );
        g.DrawImage( cp.ActivityImage, icon.Width - 20, 0, 20, 18 );
        g.Dispose();
      }
      else
      {
        icon = cp.ActivityImage;
      }

      expando.CustomHeaderSettings.NormalArrowUp        =
        expando.CustomHeaderSettings.NormalArrowDown    =
        expando.CustomHeaderSettings.NormalArrowUpHot   =
        expando.CustomHeaderSettings.NormalArrowDownHot = icon;


      // capbar

      controls.picCapBar.Image = GenerateMiniCapBar( cp );


      // remember scroll position

      int prevFirstDisplayedIndex = controls.dgvEventList.FirstDisplayedScrollingRowIndex;
      if( controls.dgvEventList.Rows.Count > 0 && prevFirstDisplayedIndex >= controls.dgvEventList.Rows.Count - 5 )  // at bottom, preserve
        prevFirstDisplayedIndex = int.MaxValue;


      // clear list

      controls.dgvEventList.Rows.Clear();


      // loop over event list (forwards or backwards)

      for( int i = ( this.eventSortOrder == SortOrder.Ascending ? 0 : game.Events.List.Count - 1 );
           i >= 0 && i < game.Events.List.Count;
           i += ( this.eventSortOrder == SortOrder.Ascending ? 1 : -1 ) )
      {
        GameEvent gameEvent = game.Events.List[i];

        if( !gameEvent.ChokePoints.Contains( cp ) )
          continue;  // not associated with our chokepoint
        if( gameEvent.Type == GameEventType.Factory )
          continue;  // don't include factory events here


        // create & add entry w/ timestamp

        string entry = String.Format( "{0} {1}", Misc.Timestamp( gameEvent.EventTime ), gameEvent.Description );
        int idx = controls.dgvEventList.Rows.Add( gameEvent.Icon, entry );
        controls.dgvEventList.Rows[idx].Tag = gameEvent;


        // tooltip

        string tooltip = String.Format( Language.Time_MinsAgo, Misc.MinsAgoLong( gameEvent.EventTime ) );

        int minsDiff = (int)( gameEvent.EventReceived - gameEvent.EventTime ).TotalMinutes;
        if( minsDiff > 5 )  // received over 5 mins after the event occurred
          tooltip += " (" + String.Format( Language.Time_ReceivedMinsAgo, Misc.MinsAgoLong( gameEvent.EventReceived ) ) + ")";

        controls.dgvEventList[1, idx].ToolTipText = tooltip;


        // if special event, make bold

        if( gameEvent is ChokePointCapturedGameEvent
          || ( gameEvent is ChokePointUnderAttackGameEvent && ( (ChokePointUnderAttackGameEvent)gameEvent ).NewAttack ) )
          controls.dgvEventList[1, idx].Style.Font
            = new Font( controls.dgvEventList[1, idx].InheritedStyle.Font, FontStyle.Bold );
      }


      if( expando.ExpandedHeight > EXPANDO_DEFAULT_HEIGHT )  // expanded, update height
      {
        SetListExpanded( expando, true );
      }
      else  // restore scroll position
      {
        if( controls.dgvEventList.Rows.Count > 0 )
        {
          if( prevFirstDisplayedIndex >= 0 && prevFirstDisplayedIndex < controls.dgvEventList.Rows.Count )  // valid previous value
            controls.dgvEventList.FirstDisplayedScrollingRowIndex = prevFirstDisplayedIndex;
          else if( prevFirstDisplayedIndex == int.MaxValue )                                                // stay at end
            controls.dgvEventList.FirstDisplayedScrollingRowIndex = controls.dgvEventList.Rows.Count - 1;
          else                                                                                              // default: newest end
            controls.dgvEventList.FirstDisplayedScrollingRowIndex = this.eventSortOrder == SortOrder.Ascending ? controls.dgvEventList.Rows.Count - 1 : 0;
        }
      }


      // set enabled/disabled status if changed

      if( controls.dgvEventList.Rows.Count > 0 )  // has events
      {
        if( !expando.CanCollapse )  // currently disabled, enable
        {
          expando.CustomHeaderSettings.NormalTitleColor = Color.FromArgb( 224, 224, 224 );
          expando.CustomHeaderSettings.NormalGradientEndColor = Color.FromArgb( 51, 51, 51 );
          expando.CustomHeaderSettings.NormalGradientStartColor = Color.FromArgb( 51, 51, 51 );

          expando.CanCollapse = true;
          expando.Collapsed = false;  // animate

          GameStatus.ToolTip.SetToolTip( expando, null );
        }
      }
      else  // doesn't have events
      {
        if( expando.CanCollapse )  // currently enabled, disable
        {
          expando.CustomHeaderSettings.NormalTitleColor = Color.Gray;
          expando.CustomHeaderSettings.NormalGradientEndColor = Color.FromArgb( 30, 30, 30 );
          expando.CustomHeaderSettings.NormalGradientStartColor = Color.FromArgb( 30, 30, 30 );

          expando.Collapse();  // no animation
          expando.CanCollapse = false;

          GameStatus.ToolTip.SetToolTip( expando, String.Format( Language.CurrentAttacks_Tooltip_NoEvents, cp ) );
        }
      }

    }

    /// <summary>
    /// Updates the given ChokePoint's Expando's event list tooltips.
    /// </summary>
    /// <param name="cp">The ChokePoint to update.</param>
    private void UpdateTooltip( ChokePoint cp )
    {
      if( !expandos.ContainsKey( cp ) ) return;

      Expando expando = expandos[cp];
      CAControls controls = (CAControls)expando.Tag;

      foreach( DataGridViewRow row in controls.dgvEventList.Rows )
      {
        GameEvent gameEvent = (GameEvent)row.Tag;

        string tooltip = String.Format( Language.Time_MinsAgo, Misc.MinsAgoLong( gameEvent.EventTime ) );

        int minsDiff = (int)( gameEvent.EventReceived - gameEvent.EventTime ).TotalMinutes;
        if( minsDiff > 5 )  // received over 5 mins after the event occurred
          tooltip += " (" + String.Format( Language.Time_ReceivedMinsAgo, Misc.MinsAgoLong( gameEvent.EventReceived ) ) + ")";
        row.Cells[1].ToolTipText = tooltip;
      }
    }

    /// <summary>
    /// Generates a 50% scale capbar, showing the current state of the given ChokePoint.
    /// </summary>
    /// <param name="cp">The reference ChokePoint.</param>
    /// <returns>The mini capbar image.</returns>
    private Image GenerateMiniCapBar( ChokePoint cp )
    {
      Image bitmap = new Bitmap( 130, 12 );  // size of picCapBar
      Graphics g = Graphics.FromImage( bitmap );

      const int left = 5;
      const int right = 125;
      const int range = right - left;
      const float y = 3.5F;

      if( cp.IsContested )
      {
        int mid = (int)( ( 100 - cp.PercentOwnership ) * range / 100 ) + left;

        g.DrawImage( cp.Owner.Side == Side.Allied ? Resources.capbar_red_left : Resources.capbar_blue_left,
                     left - 2, y, 2, 5 );
        g.DrawImage( cp.Owner.Side == Side.Allied ? Resources.capbar_red : Resources.capbar_blue,
                     new RectangleF( left, y, mid - left, 5 ),
                     new Rectangle( 0, 0, 1, 10 ), GraphicsUnit.Pixel );
        g.DrawImage( cp.Owner.Side == Side.Allied ? Resources.capbar_blue : Resources.capbar_red,
                     new RectangleF( mid, y, right - mid, 5 ),
                     new Rectangle( 0, 0, 1, 10 ), GraphicsUnit.Pixel );
        g.DrawImage( cp.Owner.Side == Side.Allied ? Resources.capbar_blue_right : Resources.capbar_red_right,
                     right, y, 2, 5 );
        g.DrawImage( Resources.capbar_pointer, mid - 6.75F, y - 1.5F, 13.5F, 11.5F );
      }
      else  // not contested
      {
        g.DrawImage( cp.Owner.Side == Side.Allied ? Resources.capbar_blue_left : Resources.capbar_red_left,
             left - 2, y, 2, 5 );
        g.DrawImage( cp.Owner.Side == Side.Allied ? Resources.capbar_blue : Resources.capbar_red,
                     new RectangleF( left, y, range, 5 ),
                     new Rectangle( 0, 0, 1, 10 ), GraphicsUnit.Pixel );
        g.DrawImage( cp.Owner.Side == Side.Allied ? Resources.capbar_blue_right : Resources.capbar_red_right,
                     right, y, 2, 5 );
      }

      g.Dispose();

      return bitmap;
    }

    /// <summary>
    /// Generates a list of chokepoints that have capture events that occurred in the past hour.
    /// </summary>
    private List<ChokePoint> GetChokePointsToDisplay()
    {
      List<ChokePoint> displaycps = new List<ChokePoint>();


      // add any cp's with AO's

      foreach( ChokePoint cp in game.ValidChokePoints.Where( cp => cp.HasAO ) )
        displaycps.Add( cp );


      // add any other cp's with capture events in past hour

      DateTime oneHourAgo = DateTime.Now.AddHours( -1 );
      foreach( GameEvent gameEvent in game.Events )
      {
        if( gameEvent.EventTime < oneHourAgo ) continue;
        if( gameEvent.Type != GameEventType.Capture ) continue;

        foreach( ChokePoint cp in gameEvent.ChokePoints )
          if( !displaycps.Contains( cp ) )
            displaycps.Add( cp );
      }

      displaycps.Sort();

      return displaycps;
    }

    /// <summary>
    /// Update the state of all the "Show alerts" checkboxes to reflect the current settings.
    /// </summary>
    public void UpdateShowAlertsCheckboxes()
    {
      foreach( KeyValuePair<ChokePoint, Expando> keyvalue in this.expandos )
      {
        ChokePoint cp = keyvalue.Key;
        Expando expando = keyvalue.Value;
        CheckBox cbShowAlerts = ( (CAControls)expando.Tag ).cbShowAlerts;

        if( options.Alerts.filterChokePoint == false ||           // not filtering, or
          options.Alerts.filterChokePointIDs.Contains( cp.ID ) )  // filtering and includes this cp
          cbShowAlerts.Checked = true;
        else
          cbShowAlerts.Checked = false;
      }

      UpdateShowAllAlertsCheckbox();
    }

    /// <summary>
    /// Update the state of the "Show all alerts" checkbox.
    /// </summary>
    private void UpdateShowAllAlertsCheckbox()
    {
      if( !options.Alerts.filterChokePoint )
        cbShowAllAlerts.CheckState = CheckState.Checked;        // not filtering
      else if( options.Alerts.filterChokePointIDs.Count == 0 )
        cbShowAllAlerts.CheckState = CheckState.Unchecked;      // none selected
      else if( options.Alerts.filterChokePointIDs.Count == options.filterChokePointTotal )
        cbShowAllAlerts.CheckState = CheckState.Checked;        // all selected
      else
        cbShowAllAlerts.CheckState = CheckState.Indeterminate;  // some selected
    }

    #endregion

    #region Event Handlers

    // reveal other widgets on double click
    private void dgvEventList_CellDoubleClick( object sender, DataGridViewCellEventArgs e )
    {
      GameEvent gameEvent = ( (DataGridView)sender ).Rows[e.RowIndex].Tag as GameEvent;
      if( gameEvent == null ) return;

        switch( gameEvent.Type )
        {
          case GameEventType.Capture:
          case GameEventType.AttackObjective:
          case GameEventType.Firebase:
            if( e.ColumnIndex == 0 )  // clicked icon
              GameStatus_RevealWidget( WidgetType.GameMap, gameEvent.ChokePoints[0] );
            else                      // clicked description
              GameStatus_RevealWidget( WidgetType.TownStatus, gameEvent.ChokePoints[0] );
            break;

          case GameEventType.Factory:
            GameStatus_RevealWidget( WidgetType.FactoryStatus, ( (IFactoryGameEvent)gameEvent ).Factory );
            break;

          case GameEventType.HCUnit:
            if( e.ColumnIndex == 0 )  // clicked icon
              GameStatus_RevealWidget( WidgetType.OrderOfBattle, ( (IHCUnitGameEvent)gameEvent ).HCUnit );
            else                      // clicked description
              GameStatus_RevealWidget( WidgetType.BrigadeStatus, ( (IHCUnitGameEvent)gameEvent ).HCUnit );
            break;
        }
    }

    // expand/reset/collapse buttons
    private void picExpand_MouseClick( object sender, MouseEventArgs e )
    {
      if( e.Button != MouseButtons.Left ) return;

      foreach( Expando expando in expandos.Values )
        expando.Collapsed = false;
    }
    private void picReset_MouseClick( object sender, MouseEventArgs e )
    {
      if( e.Button != MouseButtons.Left ) return;

      foreach( KeyValuePair<ChokePoint, Expando> keyvalue in this.expandos )
      {
        SetListExpanded( keyvalue.Value, false );
        keyvalue.Value.Collapsed = !keyvalue.Key.IsContested;
      }
    }
    private void picCollapse_MouseClick( object sender, MouseEventArgs e )
    {
      if( e.Button != MouseButtons.Left ) return;

      foreach( Expando expando in expandos.Values )
        expando.Collapsed = true;
    }

    // expand town button
    private void picExpandTown_Click( Expando expando )
    {
      SetListExpanded( expando, expando.ExpandedHeight == EXPANDO_DEFAULT_HEIGHT );
    }

    // dynamic focus
    private void CurrentAttacks_MouseEnter( object sender, EventArgs e )
    {
      BegmMisc.FocusTaskPane( this );
    }

    // Reveal() timer
    private void tmrReveal_Tick( object sender, EventArgs e )
    {
      Expando expChokepoint = (Expando)tmrReveal.Tag;
      Expando expCurrentAttacks = (Expando)this.Parent;

      if( expCurrentAttacks.Animating )
        return;  // wait until finished animating

      int color = expChokepoint.CustomHeaderSettings.NormalGradientEndColor.R;

      if( color == 51 )  // first frame, highlight, scroll into view
      {
        expChokepoint.CustomHeaderSettings.NormalGradientEndColor =
          expChokepoint.CustomHeaderSettings.NormalGradientStartColor = Color.FromArgb( 101, 101, 101 );

        if( expCurrentAttacks.TaskPane != null )
          expCurrentAttacks.TaskPane.ScrollControlIntoView( expChokepoint );
      }
      else  // remainder, fade out, stop
      {
        color -= 10;

        expChokepoint.CustomHeaderSettings.NormalGradientEndColor =
          expChokepoint.CustomHeaderSettings.NormalGradientStartColor = Color.FromArgb( color, color, color );

        if( color == 51 )
          tmrReveal.Stop();  // finished
      }
    }

    // delete expandos when finished animating
    private void tmrRemoveChokePoint_Tick( object sender, EventArgs e )
    {
      List<Expando> expandosToRemove = (List<Expando>)tmrRemoveChokePoint.Tag;

      Expando[] oldExpandos = new Expando[expandosToRemove.Count];
      expandosToRemove.CopyTo( oldExpandos, 0 );  // make a copy to iterate over

      foreach( Expando oldExpando in oldExpandos )
      {
        if( !oldExpando.Animating )
        {
          tskAttacks.Expandos.Remove( oldExpando );
          expandosToRemove.Remove( oldExpando );
        }
      }

      if( expandosToRemove.Count == 0 )  // finished
        tmrRemoveChokePoint.Stop();
    }

    // "show all alerts" checkbox
    private void cbShowAllAlerts_Click( object sender, EventArgs e )
    {
      options.ShowAlerts( null, cbShowAllAlerts.Checked );


      // set all child "show alert" checkboxes to the same as parent

      foreach( Expando expando in this.expandos.Values )
        ( (CAControls)expando.Tag ).cbShowAlerts.Checked = cbShowAllAlerts.Checked;
    }

    // individual "show alerts" checkboxes
    private void cbShowAlerts_Click( CheckBox cbShowAlerts, ChokePoint cp )
    {
      if( cbShowAlerts.Checked )
      {
        options.ShowAlerts( cp, true );
      }
      else  // unchecked
      {
        if( !options.Alerts.filterChokePoint )  // filter disabled
        {
          // enable filter and add remaining selected items (so new cps will be unchecked)

          options.Alerts.filterChokePoint = true;
          
          foreach( KeyValuePair<ChokePoint, Expando> keyvalue in this.expandos )
          {
            ChokePoint cp2 = keyvalue.Key;
            Expando expando = keyvalue.Value;

            if( ( (CAControls)expando.Tag ).cbShowAlerts.Checked )
              options.Alerts.filterChokePointIDs.Add( cp2.ID );
          }
        }
        else  // already filtering
        {
          options.Alerts.filterChokePointIDs.Remove( cp.ID );
        }

        options.Alerts.RegenerateFlags();
        options.Alerts.SaveToRegistry( false );
        options.AlertsTab = options.Alerts;  // update form controls
      }


      // update state of "show all alerts" checkbox

      UpdateShowAllAlertsCheckbox();

    }

    #endregion
  }


  /// <summary>
  /// A structure for holding references to the controls within each expando.
  /// Stored in the expando's Tag property.
  /// </summary>
  public struct CAControls
  {
    public DataGridView dgvEventList;
    public CheckBox cbShowAlerts;
    public PictureBox picExpandTown;
    public PictureBox picCapBar;
  }
}
