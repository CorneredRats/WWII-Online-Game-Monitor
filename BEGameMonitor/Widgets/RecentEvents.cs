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
using System.Drawing;
using System.Windows.Forms;
using Xiperware.WiretapAPI;

namespace BEGM.Widgets
{
  /// <summary>
  /// The RecentEvents widget lists all game events that have occurred in the past 2 hours (including
  /// when the program in first started, for most events). These can be filtered by type: Capture, AO,
  /// HC Unit &amp; Factory, and double-clicking on an item actives the appropriate widget to display
  /// more information.
  /// </summary>
  public partial class RecentEvents : UserControl, IWidget
  {
    #region Variables

    /// <summary>
    /// The current game state.
    /// </summary>
    private GameState game;

    /// <summary>
    /// Order the events in the event list are displayed.
    /// </summary>
    private SortOrder eventSortOrder = SortOrder.Ascending;

    /// <summary>
    /// The enabled state of the factory filter checkbox.
    /// </summary>
    private bool factoryCheckboxEnabled = false;

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
    /// Create a new RecentEvents widget.
    /// </summary>
    public RecentEvents()
    {
      InitializeComponent();


      // dynamic positioning

      Control[] controls = new Control[] { lblViewEvents, cbCapture, cbAttackObjective, cbFirebase, cbHCUnit, cbFactory };

      int left = controls[0].Left;
      int right = controls[controls.Length - 1].Right;
      int width = 0;
      foreach( Control control in controls )
        width += control.Width;

      float pad = ( right - left - width ) / (float)controls.Length;

      for( int i = 0; i < controls.Length; i++ )
      {
        int widthBefore = 0;
        for( int j = 0; j < i; j++ )
          widthBefore += controls[j].Width;

        controls[i].Left = left + widthBefore + (int)Math.Round( pad * i );
      }

      //// avoid default DataError dialog
      dgvEventList.DataError += ( sender, args ) => { };
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
    internal RecentEventsState State
    {
      get
      {
        RecentEventsState state = new RecentEventsState {
          showCaptureEvents         = cbCapture.Checked,
          showAttackObjectiveEvents = cbAttackObjective.Checked,
          showFirebaseEvents        = cbFirebase.Checked,
          showHCUnitEvents          = cbHCUnit.Checked,
          showFactoryEvents         = cbFactory.Checked
        };

        return state;
      }
      set
      {
        cbCapture.Checked         = value.showCaptureEvents;
        cbAttackObjective.Checked = value.showAttackObjectiveEvents;
        cbFirebase.Checked        = value.showFirebaseEvents;
        cbHCUnit.Checked          = value.showHCUnitEvents;
        cbFactory.Checked         = value.showFactoryEvents;
      }
    }

    /// <summary>
    /// Gets or sets the order the events in the event list are displayed.
    /// </summary>
    internal SortOrder EventListSortOrder
    {
      get { return this.eventSortOrder; }
      set
      {
        this.eventSortOrder = value;

        if( dgvEventList.Rows.Count > 0 )
        {
          // update sort order
          UpdateWidget();

          // reset scroll position
          dgvEventList.FirstDisplayedScrollingRowIndex = this.eventSortOrder == SortOrder.Ascending ? dgvEventList.Rows.Count - 1 : 0;
        }
      }
    }

    /// <summary>
    /// Custom implementation of the cbFactory.Enabled property, as the default one always
    /// draws the text dark grey (on black) when disabled. Also sets/removes a tooltip.
    /// </summary>
    internal bool FactoryCheckboxEnabled
    {
      get { return this.factoryCheckboxEnabled; }
      set
      {
        this.factoryCheckboxEnabled = value;

        if( value )
        {
          cbFactory.ForeColor = Color.FromArgb( 224, 224, 224 );
          GameStatus.ToolTip.SetToolTip( cbFactory, null );
        }
        else
        {
          cbFactory.Checked = false;
          cbFactory.ForeColor = Color.Gray;
          GameStatus.ToolTip.SetToolTip( cbFactory, Language.RecentEvents_Tooltip_FactoryDataNotLoaded );
        }
      }
    }

    // NOTE: internal is used rather than public to prevent VS Designer using the properties

    #endregion

    #region Methods

    /// <summary>
    /// Initialise the RecentEvents widget.
    /// </summary>
    /// <param name="game">The current game state.</param>
    public void InitWidget( GameState game )
    {
      Log.AddEntry( "  RecentEvents..." );

      this.game = game;


      // populate event list

      UpdateWidget();


      Log.Okay();
    }

    /// <summary>
    /// Updates the event list with the latest GameEvents.List data.
    /// </summary>
    public void UpdateWidget()
    {
      if( this.game == null ) return;


      // remember scroll position

      int prevFirstDisplayedIndex = dgvEventList.FirstDisplayedScrollingRowIndex;
      if( dgvEventList.Rows.Count > 0 && prevFirstDisplayedIndex >= dgvEventList.Rows.Count - 9 )  // at bottom, preserve
        prevFirstDisplayedIndex = int.MaxValue;


      // clear list

      dgvEventList.Rows.Clear();


      // skip remainder if no event types selected

      if( !cbCapture.Checked && !cbAttackObjective.Checked && !cbFirebase.Checked && !cbHCUnit.Checked && !cbFactory.Checked )
        return;


      // loop over event list (forwards or backwards)

      for( int i = ( this.eventSortOrder == SortOrder.Ascending ? 0 : game.Events.List.Count - 1 );
           i >= 0 && i < game.Events.List.Count;
           i += ( this.eventSortOrder == SortOrder.Ascending ? 1 : -1 ) )
      {
        GameEvent gameEvent = game.Events.List[i];


        // filter events

        switch( gameEvent.Type )
        {
          case GameEventType.Capture: if( !cbCapture.Checked ) continue; break;
          case GameEventType.AttackObjective: if( !cbAttackObjective.Checked ) continue; break;
          case GameEventType.Firebase: if( !cbFirebase.Checked ) continue; break;
          case GameEventType.HCUnit: if( !cbHCUnit.Checked ) continue; break;
          case GameEventType.Factory: if( !cbFactory.Checked ) continue; break;
        }


        // create & add entry w/ timestamp

        string entry = String.Format( "{0} {1}", Misc.Timestamp( gameEvent.EventTime ), gameEvent.Description );
        int idx = dgvEventList.Rows.Add( gameEvent.Icon, entry );
        dgvEventList.Rows[idx].Tag = gameEvent;


        // tooltip

        string tooltip = String.Format( Language.Time_MinsAgo, Misc.MinsAgoLong( gameEvent.EventTime ) );

        int minsDiff = (int)( gameEvent.EventReceived - gameEvent.EventTime ).TotalMinutes;
        if( minsDiff > 5 )  // received over 5 mins after the event occurred
          tooltip += " (" + String.Format( Language.Time_ReceivedMinsAgo, Misc.MinsAgoLong( gameEvent.EventReceived ) ) + ")";

        dgvEventList[1, idx].ToolTipText = tooltip;


        // if special event, make bold

        if( gameEvent is ChokePointCapturedGameEvent || gameEvent is HCUnitRoutedGameEvent
          || ( gameEvent is ChokePointUnderAttackGameEvent && ( (ChokePointUnderAttackGameEvent)gameEvent ).NewAttack ) )
          dgvEventList[1, idx].Style.Font = new Font( dgvEventList[1, idx].InheritedStyle.Font, FontStyle.Bold );


        // if received within last 15 mins, highlight yellow

        dgvEventList[1, idx].Style.ForeColor = dgvEventList[1, idx].Style.SelectionForeColor = Misc.GetFadedYellow( gameEvent.EventReceived, 15, 224 );
      }


      // restore scroll position

      if( dgvEventList.Rows.Count > 0 )
      {
        if( prevFirstDisplayedIndex >= 0 && prevFirstDisplayedIndex < dgvEventList.Rows.Count )  // valid previous value
          dgvEventList.FirstDisplayedScrollingRowIndex = prevFirstDisplayedIndex;
        else if( prevFirstDisplayedIndex == int.MaxValue )                                       // stay at end
          dgvEventList.FirstDisplayedScrollingRowIndex = dgvEventList.Rows.Count - 1;
        else                                                                                     // default: newest end
          dgvEventList.FirstDisplayedScrollingRowIndex = this.eventSortOrder == SortOrder.Ascending ? dgvEventList.Rows.Count - 1 : 0;
      }
    }

    /// <summary>
    /// Remove references to local game objects.
    /// </summary>
    public void Cleanup()
    {
      this.game = null;
      dgvEventList.Rows.Clear();
    }

    /// <summary>
    /// Not Implemented.
    /// </summary>
    /// <param name="arg"></param>
    public void Reveal( object arg )
    {
      throw new NotImplementedException();
    }

    #endregion

    #region Event Handlers

    // don't draw focus rectangle
    private void dgvEventList_RowPrePaint( object sender, DataGridViewRowPrePaintEventArgs e )
    {
      e.PaintParts &= ~DataGridViewPaintParts.Focus;
    }

    // reveal other widgets on double click
    private void dgvEventList_CellDoubleClick( object sender, DataGridViewCellEventArgs e )
    {
      GameEvent gameEvent = (GameEvent)dgvEventList.Rows[e.RowIndex].Tag;

      switch( gameEvent.Type )
      {
        case GameEventType.Capture:
        case GameEventType.AttackObjective:
        case GameEventType.Firebase:
          if( e.ColumnIndex == 0 )  // clicked icon
            GameStatus_RevealWidget( WidgetType.CurrentAttacks, gameEvent.ChokePoints[0] );
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

    // event type checkboxes
    private void cbCapture_CheckedChanged( object sender, EventArgs e )
    {
      UpdateWidget();
    }
    private void cbAttackObjective_CheckedChanged( object sender, EventArgs e )
    {
      UpdateWidget();
    }
    private void cbFirebase_CheckedChanged( object sender, EventArgs e )
    {
      UpdateWidget();
    }
    private void cbHCUnit_CheckedChanged( object sender, EventArgs e )
    {
      UpdateWidget();
    }
    private void cbFactory_CheckedChanged( object sender, EventArgs e )
    {
      if( factoryCheckboxEnabled )
        UpdateWidget();
      else if( cbFactory.Checked )
        cbFactory.Checked = false;  // will recurse once
    }

    // dynamic focus
    private void RecentEvents_MouseEnter( object sender, EventArgs e )
    {
      BegmMisc.FocusTaskPane( this );
    }
    private void dgvEventList_MouseEnter( object sender, EventArgs e )
    {
      BegmMisc.FocusWithoutScroll( dgvEventList );
    }

    #endregion
  }
}
