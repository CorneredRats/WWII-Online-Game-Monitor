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
using System.Media;
using System.Windows.Forms;
using BEGM.Properties;
using Xiperware.WiretapAPI;

namespace BEGM
{
  /// <summary>
  /// The Alert Window is a small notification pop-up that is shown always-on-top
  /// and without taking focus. It contains details about each event the user has
  /// opted to be notified for.
  /// </summary>
  public partial class AlertWindow : Form
  {
    #region Variables

    /// <summary>
    /// The current game options.
    /// </summary>
    /// <remarks>Needed to get/set alert settings.</remarks>
    public Options options;

    /// <summary>
    /// The GameStatus's RevealWidget() method.
    /// </summary>
    public RevealDelegate GameStatus_RevealWidget;

    private List<GameEvent> currentEvents = new List<GameEvent>();
    private int currentIndex;
    private Rectangle desktop;
    private int yPosition;

    private int fadeIndex;
    private readonly double[] fadeOpacity;
    private readonly List<int> fadeHeights;

    private bool postponedAlerts = false;
    private readonly SoundPlayer alertNew, alertMore;

    private Bitmap iconNormal;
    private Bitmap iconActive;

    private bool autoNextCancelled = false;  // true if user clicks alert background

    #endregion

    #region Constructors

    /// <summary>
    /// Create a new AlertWindow.
    /// </summary>
    public AlertWindow()
    {
      InitializeComponent();


      // load sounds

      this.alertNew = new SoundPlayer( Resources.alertsound_new );
      this.alertMore = new SoundPlayer( Resources.alertsound_more );

      this.alertNew.Load();
      this.alertMore.Load();


      // calculate fade in values

      this.fadeOpacity = new double[23];

      this.fadeOpacity[0]  = 0.0079;
      this.fadeOpacity[1]  = 0.0241;
      this.fadeOpacity[2]  = 0.0426;
      this.fadeOpacity[3]  = 0.0629;
      this.fadeOpacity[4]  = 0.0857;
      this.fadeOpacity[5]  = 0.1135;
      this.fadeOpacity[6]  = 0.1442;
      this.fadeOpacity[7]  = 0.1817;
      this.fadeOpacity[8]  = 0.2265;
      this.fadeOpacity[9]  = 0.2840;
      this.fadeOpacity[10] = 0.3603;
      this.fadeOpacity[11] = 0.4718;
      this.fadeOpacity[12] = 0.5910;
      this.fadeOpacity[13] = 0.6907;
      this.fadeOpacity[14] = 0.7652;
      this.fadeOpacity[15] = 0.8208;
      this.fadeOpacity[16] = 0.8620;
      this.fadeOpacity[17] = 0.8972;
      this.fadeOpacity[18] = 0.9264;
      this.fadeOpacity[19] = 0.9515;
      this.fadeOpacity[20] = 0.9719;
      this.fadeOpacity[21] = 0.9912;
      this.fadeOpacity[22] = 1.0000;

      double height = this.Height;

      this.fadeHeights = new List<int>();
      for( int i = 0; i < this.fadeOpacity.Length; i++ )
        this.fadeHeights.Add( (int)( height * this.fadeOpacity[i] ) );

    }

    #endregion

    #region Properties

    /// <summary>
    /// True if the Prev event button should be enabled.
    /// </summary>
    private bool PrevEnabled
    {
      get
      {
        if( this.currentEvents.Count <= 1 )
          return false;  // zero or one event

        return this.currentIndex > 0;
      }
    }

    /// <summary>
    /// True if the Next event button should be enabled.
    /// </summary>
    private bool NextEnabled
    {
      get
      {
        if( this.currentEvents.Count <= 1 )
          return false;  // zero or one event

        if( this.currentIndex < this.currentEvents.Count - 1 )
          return true;
        else
          return false;  // at end
      }
    }

    /// <summary>
    /// True if alerts should be postponed until the user is available.
    /// </summary>
    public bool Postpone
    {
      get
      {
        if( BegmMisc.WW2Running() )  // always postpone if ww2 is running (doesn't draw properly)
          return true;

        if( options.Alerts.postponeFullscreen && BegmMisc.IsFullScreen( options.Misc.AlertDisplayIndex ) )
          return true;

        if( options.Alerts.postponeIdle && BegmMisc.GetIdleTime().TotalMinutes > options.Alerts.postponeIdleTime )
          return true;

        return false;
      }
    }

    /// <summary>
    /// True if we currently have alerts waiting for the user.
    /// </summary>
    public bool PostponedAlerts
    {
      get { return this.postponedAlerts; }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Displays the current list of alerts, from the last ShowAlerts() call.
    /// </summary>
    public void ShowCurrentAlerts()
    {
      this.currentIndex = 0;
      this.postponedAlerts = false;
      DisplayAlerts();
    }

    /// <summary>
    /// Displays the given list of game events as pop-up alerts. If the user is away,
    /// they will be appended to the queue to be shown later.
    /// </summary>
    /// <param name="gameEvents">A list of events to display.</param>
    public void ShowAlerts( List<GameEvent> gameEvents )
    {
      if( gameEvents.Count == 0 )
        return;

      bool postpone = this.Postpone;

      if( this.Visible )
      {
        // append, preserve index

        currentEvents.AddRange( gameEvents );


        // trim to 10 mins

        DateTime tenMinsAgo = DateTime.Now.AddMinutes( -10 );
        while( this.currentEvents.Count > 1 && this.currentEvents[0].EventTime < tenMinsAgo )
        {
          this.currentEvents.RemoveAt( 0 );
          if( this.currentIndex > 0 )
            this.currentIndex--;
        }
      }
      else if( postpone )
      {
        // if first postpone, clear prev events

        if( !this.postponedAlerts )
          this.currentEvents.Clear();


        // append, reset to start

        this.currentEvents.AddRange( gameEvents );
        this.currentIndex = 0;


        // trim to 10 mins

        DateTime tenMinsAgo = DateTime.Now.AddMinutes( -10 );
        while( this.currentEvents.Count > 1 && this.currentEvents[0].EventTime < tenMinsAgo )
          this.currentEvents.RemoveAt( 0 );


        // log entry

        if( !this.postponedAlerts )
          Log.AddEntry( "User away, postponing alerts" );


        // set flag

        this.postponedAlerts = true;
      }
      else
      {
        // replace, reset to start

        this.currentEvents.Clear();
        this.currentEvents.AddRange( gameEvents );
        this.currentIndex = 0;
      }

      if( !postpone )
        DisplayAlerts();
    }

    /// <summary>
    /// Remove references to local game objects.
    /// </summary>
    public void Cleanup()
    {
      if( this.Visible )  // hide current alert
      {
        tmrFadein.Enabled = tmrFadeout.Enabled = tmrAutoNext.Enabled = false;
        this.Visible = false;
      }

      this.currentEvents.Clear();
    }

    /// <summary>
    /// Sets the initial alert position, draws the first event, and starts the fade
    /// in process according to current alert settings.
    /// </summary>
    private void DisplayAlerts()
    {
      if( this.currentEvents.Count == 0 )
        return;


      // update corners

      this.picCornerTop.Visible = options.Alerts.alertPosition != DockStyle.Top;
      this.picCornerBottom.Visible = options.Alerts.alertPosition == DockStyle.Top;


      // set initial location

      Rectangle newdesktop = Screen.AllScreens[options.Misc.AlertDisplayIndex].WorkingArea;
      if( !this.Visible || newdesktop != this.desktop )  // changed
      {
        this.desktop = newdesktop;

        if( options.Alerts.alertPosition == DockStyle.Top )
        {
          if( this.Visible )
            this.yPosition = this.desktop.Top;
          else
            this.yPosition = this.desktop.Top - this.Height;
        }
        else  // alert position = bottom
        {
          if( this.Visible )
            this.yPosition = this.desktop.Bottom - this.Height;
          else
            this.yPosition = this.desktop.Bottom;
        }
      }


      // play alert sound

      if( options.Alerts.playSound )
      {
        if( this.Visible )
          this.alertMore.Play();
        else
          this.alertNew.Play();
      }


      // display alert

      if( !this.Visible )
      {
        this.autoNextCancelled = false;
        FadeIn();
      }


      // draw first (or redraw current) event, and update location
      // (must be done after visible)

      UpdateDialog();
      this.Location = new Point( desktop.Right - this.Width, yPosition );

      tmrAutoNext.Interval = options.Alerts.autoNextTime * 1000;
    }

    /// <summary>
    /// Updates the various controls on the alert dialog for the current GameEvent.
    /// </summary>
    private void UpdateDialog()
    {
      // get the game event to display

      GameEvent gameEvent = this.currentEvents[currentIndex];


      // update title

      lblTimestamp.Text = Misc.Timestamp( gameEvent.EventTime );
      lblTitle.Text = gameEvent.Title;

      int minsDiff = (int)( gameEvent.EventReceived - gameEvent.EventTime ).TotalMinutes;
      if( minsDiff > 5 )  // received over 5 mins after the event occurred
      {
        lblMinsAgo.Visible = true;
        lblMinsAgo.Text = "(" + String.Format( Language.Time_MinsAgo, Misc.MinsAgoShort( gameEvent.EventTime ) ) + ")";
      }
      else
      {
        lblMinsAgo.Visible = false;
      }


      // update icons

      this.iconNormal = (Bitmap)gameEvent.AlertIcon;
      this.iconActive = Misc.AdjustBrightness( iconNormal, 40 );  // brighter version for mouseover

      picIcon.Image = iconNormal;


      // update description
      
      lblDescription.Text = gameEvent.Description;


      // if capture event, show flags (move icon up a bit)

      if( gameEvent is ICountryChangeGameEvent )
      {
        picIcon.Location = new Point( picIcon.Location.X, 35 );
        picFlags.Image = GenerateFlagImage( (ICountryChangeGameEvent)gameEvent );
        picFlags.Visible = true;
      }
      else
      {
        picIcon.Location = new Point( picIcon.Location.X, 38 );
        picFlags.Visible = false;
      }


      // show "show future alerts" checkbox if new under attack event, and filtering by cp

      if( gameEvent is ChokePointUnderAttackGameEvent && ( (ChokePointUnderAttackGameEvent)gameEvent ).NewAttack
       && options.Alerts.filterChokePoint )
      {
        lblDescription.Padding = new Padding( 0, 0, 0, 18 );
        cbShowAlerts.Visible = true;
        cbShowAlerts.Checked = options.Alerts.filterChokePointIDLookup.ContainsKey( gameEvent.ChokePoints[0].ID );
      }
      else
      {
        lblDescription.Padding = new Padding( 0 );
        cbShowAlerts.Visible = false;
      }


      // update prev/next buttons

      lblCountCurrent.Text = ( this.currentIndex + 1 ).ToString();
      lblCountTotal.Text = this.currentEvents.Count.ToString();

      picPrev.Image = this.PrevEnabled ? Resources.alertarrow_left_normal : Resources.alertarrow_left_disabled;
      picNext.Image = this.NextEnabled ? Resources.alertarrow_right_normal : Resources.alertarrow_right_disabled;
    }

    /// <summary>
    /// For certain events, generates the [From] => [ To ] flag image.
    /// </summary>
    /// <param name="gameEvent">The reference GameEvent that implements ICountryChangeGameEvent.</param>
    /// <returns>The flag image.</returns>
    private Image GenerateFlagImage( ICountryChangeGameEvent gameEvent )
    {
      // get flags

      Country countryFrom = gameEvent.PrevCountry;
      Country countryTo = gameEvent.NewCountry;

      if( countryFrom == null || countryTo == null )
        return null;

      Image flagFrom = countryFrom.CountryFlag;
      Image flagTo = countryTo.CountryFlag;


      // create image

      Bitmap bitmap = new Bitmap( 32, 8 );
      Graphics g = Graphics.FromImage( bitmap );
      g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;

      g.DrawImage( flagFrom, 0, 0.5F, 12, 7.125F );
      g.DrawImage( Resources.arrow_right, 13, 1, 7, 7 );
      g.DrawImage( flagTo, 20, 0.5F, 12, 7.125F );


      g.Dispose();

      return bitmap;
    }

    /// <summary>
    /// Navigates to the previous alert in the current list.
    /// </summary>
    private void GoPrev()
    {
      if( this.currentIndex < 1 ) return;

      this.currentIndex--;
      UpdateDialog();
    }

    /// <summary>
    /// Navigates to the next alert in the current list.
    /// </summary>
    private void GoNext()
    {
      if( this.currentIndex > this.currentEvents.Count - 2 ) return;

      this.currentIndex++;
      UpdateDialog();
    }

    /// <summary>
    /// Initialises the alert window fade-in process.
    /// </summary>
    private void FadeIn()
    {
      this.Opacity = 0.0;
      this.fadeIndex = -1;
      User32.ShowWindow( this.Handle, User32.WindowShowStyle.ShowNoActivate );

      tmrFadein.Tag = options.Alerts.alertPosition == DockStyle.Top;  // as will be reverted during test
      tmrFadein.Start();
      picClose.Image = Resources.alertclose_disabled;
    }

    /// <summary>
    /// Initialises the alert window fade-out process.
    /// </summary>
    private void FadeOut()
    {
      tmrFadein.Enabled = tmrAutoNext.Enabled = tmrDetectLeave.Enabled = false;

      fadeIndex = 23;
      tmrFadeout.Start();
    }

    /// <summary>
    /// Called when the mouse enters the form (or a control on the form).
    /// Starts checking for mouseleave, initiates fade-back-in, and/or cancels auto-next.
    /// </summary>
    private void OnMouseEnter()
    {
      if( tmrDetectLeave.Enabled )
        return;  // already inside alert window


      // ugly mouseleave detection (this.MouseLeave, this.Capture, and User32.TrackMouseEvent() are unsuitable)

      tmrDetectLeave.Start();


      // abort fade out and fade back in

      if( tmrFadeout.Enabled && tmrFadeout.Tag == null )  // null = didn't press close button
      {
        tmrFadeout.Stop();
        tmrFadein.Start();
        picClose.Image = Resources.alertclose_normal;
      }


      // cancel auto next and enable close button

      if( tmrAutoNext.Enabled )
      {
        tmrAutoNext.Stop();
        picClose.Image = Resources.alertclose_normal;
      }
    }

    /// <summary>
    /// When the form (or a control on the form) is clicked, sets a flag to not resume
    /// auto-next on mouseleave.
    /// </summary>
    private void OnMouseClick()
    {
      this.autoNextCancelled = true;
    }

    /// <summary>
    /// Resumes auto-next (unless cancelled).
    /// </summary>
    private void OnMouseLeave()
    {
      if( this.autoNextCancelled )
        return;  // leave without enabling tmrAutoNext

      if( !tmrAutoNext.Enabled )
      {
        tmrAutoNext.Start();
        picClose.Image = Resources.alertclose_disabled;
      }
    }

    /// <summary>
    /// Displays a series of dummy events for alert testing.
    /// </summary>
    public void TestAlert()
    {
      this.currentEvents.Clear();
      this.currentIndex = 0;

      DateTime time = DateTime.Now.AddMinutes( -10 );
      Country newOwner = new Country( 1, "UK", Language.Country_Name_England, Language.Country_Demonym_England, Side.Allied );
      Country prevOwner = new Country( 4, "DE", Language.Country_Name_Germany, Language.Country_Demonym_Germany, Side.Axis );
      ChokePoint testCP = new ChokePoint( 0, "Berlin", ChokePointType.Generic, prevOwner, new Point(), null );
      MilitaryFacility testFacility = new MilitaryFacility( 0, "Berlin Central Armybase", testCP, FacilityType.Armybase, new Point() );
      Facility testFacility2 = new Facility( 0, "Bilton's House", testCP, FacilityType.City, new Point() );
      HCUnit testHCUnit = new HCUnit( 0, HCUnitLevel.Brigade, "27idkg1", "1.Kampfgruppe", "27.Infanterie 1.Kampfgruppe", null, prevOwner, Branch.Army, MoveType.Land, null, null );

      this.currentEvents.Add( new FacilityCapturedGameEvent( time, testFacility, newOwner, prevOwner, "xiper" ) );
      this.currentEvents.Add( new HCUnitRoutedGameEvent( time, testHCUnit, testCP ) );
      this.currentEvents.Add( new ChokePointCapturedGameEvent( time, testCP, newOwner, prevOwner, DateTime.Now.AddMinutes( -171 ) ) );

      testCP.SetInitState( prevOwner, newOwner, testHCUnit );
      testCP.Activity = ActivityLevel.Heavy;
      testFacility.Owner = newOwner;

      if( tmrAutoNext.Enabled )
        tmrAutoNext.Stop();

      if( this.Visible )
        this.Visible = false;

      DisplayAlerts();
    }

    #endregion

    #region Event Handlers

    // timers
    private void tmrFadein_Tick( object sender, EventArgs e )
    {
      bool positionTop = (bool)tmrFadein.Tag;
      this.fadeIndex++;


      // last frame

      if( this.fadeIndex > 22 )
      {
        tmrFadein.Stop();

        if( !tmrDetectLeave.Enabled )  // don't start if cursor inside alert window after fade-back-in
          tmrAutoNext.Start();

        return;
      }


      // if not in final position (ie, not doing fade-back-in after mouseover)

      if( this.yPosition != ( positionTop ? this.desktop.Top : this.desktop.Bottom - this.Height ) )
      {
        // update position

        if( positionTop )
          this.yPosition = this.desktop.Top - this.Height + this.fadeHeights[this.fadeIndex];
        else
          this.yPosition = this.desktop.Bottom - this.fadeHeights[this.fadeIndex];

        this.Location = new Point( this.desktop.Right - this.Width, this.yPosition );
      }


      // update opacity

      this.Opacity = this.fadeOpacity[this.fadeIndex];
    }
    private void tmrFadeout_Tick( object sender, EventArgs e )
    {
      this.fadeIndex--;


      // last frame

      if( this.fadeIndex < 0 )
      {
        this.Visible = false;
        tmrFadeout.Stop();
        tmrFadeout.Tag = null;
        return;
      }


      // update opacity

      this.Opacity = this.fadeOpacity[this.fadeIndex];
    }
    private void tmrAutoNext_Tick( object sender, EventArgs e )
    {
      if( this.NextEnabled )
        GoNext();
      else
        FadeOut();  // will stop this timer
    }
    private void tmrDetectLeave_Tick( object sender, EventArgs e )
    {
      if( this.ClientRectangle.Contains( this.PointToClient( Cursor.Position ) ) )
        return;  // still inside alert window

      tmrDetectLeave.Stop();
      OnMouseLeave();
    }

    // close button
    private void picClose_MouseClick( object sender, MouseEventArgs e )
    {
      if( e.Button != MouseButtons.Left )
        return;

      tmrFadeout.Tag = false;  // don't allow cancel on mouseover
      FadeOut();

      if( Control.ModifierKeys == Keys.Shift )
      {
        options.Alerts.showAlerts = false;
        options.AlertsTab = options.Alerts;  // update form controls
        options.Alerts.SaveToRegistry( false );
        options.GameStatus_UpdateContextMenu();
      }
    }

    // prev/next buttons
    private void picPrev_MouseClick( object sender, MouseEventArgs e )
    {
      if( e.Button == MouseButtons.Left && this.PrevEnabled )
        GoPrev();
    }
    private void picPrev_MouseDoubleClick( object sender, MouseEventArgs e )
    {
      if( e.Button == MouseButtons.Left && this.PrevEnabled )
        GoPrev();
    }
    private void picNext_MouseClick( object sender, MouseEventArgs e )
    {
      if( e.Button == MouseButtons.Left && this.NextEnabled )
        GoNext();
    }
    private void picNext_MouseDoubleClick( object sender, MouseEventArgs e )
    {
      if( e.Button == MouseButtons.Left && this.NextEnabled )
        GoNext();
    }

    // "show future alerts for this attack" checkbox
    private void cbShowAlerts_CheckedChanged( object sender, EventArgs e )
    {
      ChokePointUnderAttackGameEvent gameEvent = currentEvents[currentIndex] as ChokePointUnderAttackGameEvent;
      if( gameEvent == null ) return;

      options.ShowAlerts( gameEvent.ChokePoint, cbShowAlerts.Checked );
      options.CurrentAttacks_UpdateShowAlertsCheckboxes();
    }

    // prev/next image state
    private void picPrev_MouseEnter( object sender, EventArgs e )
    {
      OnMouseEnter();

      if( this.PrevEnabled )
        picPrev.Image = Resources.alertarrow_left_hover;
    }
    private void picPrev_MouseLeave( object sender, EventArgs e )
    {
      if( this.PrevEnabled )
        picPrev.Image = Resources.alertarrow_left_normal;
    }
    private void picPrev_MouseDown( object sender, MouseEventArgs e )
    {
      if( e.Button == MouseButtons.Left && this.PrevEnabled )
        picPrev.Image = Resources.alertarrow_left_active;
    }
    private void picPrev_MouseUp( object sender, MouseEventArgs e )
    {
      if( e.Button == MouseButtons.Left && this.PrevEnabled )
        picPrev.Image = Resources.alertarrow_left_hover;
    }
    private void picNext_MouseEnter( object sender, EventArgs e )
    {
      OnMouseEnter();

      if( this.NextEnabled )
        picNext.Image = Resources.alertarrow_right_hover;
    }
    private void picNext_MouseLeave( object sender, EventArgs e )
    {
      if( this.NextEnabled )
        picNext.Image = Resources.alertarrow_right_normal;
    }
    private void picNext_MouseDown( object sender, MouseEventArgs e )
    {
      if( e.Button == MouseButtons.Left && this.NextEnabled )
        picNext.Image = Resources.alertarrow_right_active;
    }
    private void picNext_MouseUp( object sender, MouseEventArgs e )
    {
      if( e.Button == MouseButtons.Left && this.NextEnabled )
        picNext.Image = Resources.alertarrow_right_hover;
    }

    // close image state
    private void picClose_MouseEnter( object sender, EventArgs e )
    {
      OnMouseEnter();
      picClose.Image = Resources.alertclose_hover;
    }
    private void picClose_MouseLeave( object sender, EventArgs e )
    {
      picClose.Image = Resources.alertclose_normal;
    }
    private void picClose_MouseDown( object sender, MouseEventArgs e )
    {
      if( e.Button == MouseButtons.Left )
        picClose.Image = Resources.alertclose_active;
    }
    private void picClose_MouseUp( object sender, MouseEventArgs e )
    {
      if( e.Button == MouseButtons.Left )
        picClose.Image = Resources.alertclose_hover;
    }

    // highlight icon on mouseover
    private void picIcon_MouseEnter( object sender, EventArgs e )
    {
      OnMouseEnter();
      picIcon.Image = iconActive;
    }
    private void picIcon_MouseLeave( object sender, EventArgs e )
    {
      picIcon.Image = iconNormal;
    }

    // click icon to reveal widget for current game event
    private void picIcon_MouseClick( object sender, MouseEventArgs e )
    {
      if( e.Button != MouseButtons.Left ) return;

      GameEvent gameEvent = this.currentEvents[this.currentIndex];

      switch( gameEvent.Type )
      {
        case GameEventType.Capture:
        case GameEventType.AttackObjective:
        case GameEventType.Firebase:
          GameStatus_RevealWidget( WidgetType.TownStatus, gameEvent.ChokePoints[0] );
          break;

        case GameEventType.Factory:
          GameStatus_RevealWidget( WidgetType.FactoryStatus, ( (IFactoryGameEvent)gameEvent ).Factory );
          break;

        case GameEventType.HCUnit:
          GameStatus_RevealWidget( WidgetType.OrderOfBattle, ( (IHCUnitGameEvent)gameEvent ).HCUnit );
          break;
      }

      if( !this.NextEnabled )  // at last event in queue
      {
        tmrFadeout.Tag = false;  // don't allow cancel on mouseover
        FadeOut();
      }
    }

    // OnMouseEnter for each control...
    private void AlertWindow_MouseEnter( object sender, EventArgs e )
    {
      OnMouseEnter();
    }
    private void lblTimestamp_MouseEnter( object sender, EventArgs e )
    {
      OnMouseEnter();
    }
    private void lblTitle_MouseEnter( object sender, EventArgs e )
    {
      OnMouseEnter();
    }
    private void lblMinsAgo_MouseEnter( object sender, EventArgs e )
    {
      OnMouseEnter();
    }
    private void picLine_MouseEnter( object sender, EventArgs e )
    {
      OnMouseEnter();
    }
    private void picFlags_MouseEnter( object sender, EventArgs e )
    {
      OnMouseEnter();
    }
    private void lblDescription_MouseEnter( object sender, EventArgs e )
    {
      OnMouseEnter();
    }
    private void cbShowAlerts_MouseEnter( object sender, EventArgs e )
    {
      OnMouseEnter();
    }
    private void lblCountCurrent_MouseEnter( object sender, EventArgs e )
    {
      OnMouseEnter();
    }
    private void lblCountDivider_MouseEnter( object sender, EventArgs e )
    {
      OnMouseEnter();
    }
    private void lblCountTotal_MouseEnter( object sender, EventArgs e )
    {
      OnMouseEnter();
    }
    private void picCornerTop_MouseEnter( object sender, EventArgs e )
    {
      OnMouseEnter();
    }
    private void picCornerBottom_MouseEnter( object sender, EventArgs e )
    {
      OnMouseEnter();
    }

    // OnMouseClick for each control...
    private void AlertWindow_MouseClick( object sender, MouseEventArgs e )
    {
      OnMouseClick();
    }
    private void lblTimestamp_MouseClick( object sender, MouseEventArgs e )
    {
      OnMouseClick();
    }
    private void lblTitle_MouseClick( object sender, MouseEventArgs e )
    {
      OnMouseClick();
    }
    private void lblMinsAgo_MouseClick( object sender, MouseEventArgs e )
    {
      OnMouseClick();
    }
    private void picLine_MouseClick( object sender, MouseEventArgs e )
    {
      OnMouseClick();
    }
    private void picFlags_MouseClick( object sender, MouseEventArgs e )
    {
      OnMouseClick();
    }
    private void lblDescription_MouseClick( object sender, MouseEventArgs e )
    {
      OnMouseClick();
    }
    private void lblCountCurrent_MouseClick( object sender, MouseEventArgs e )
    {
      OnMouseClick();
    }
    private void lblCountDivider_MouseClick( object sender, MouseEventArgs e )
    {
      OnMouseClick();
    }
    private void lblCountTotal_MouseClick( object sender, MouseEventArgs e )
    {
      OnMouseClick();
    }
    private void picCornerTop_MouseClick( object sender, MouseEventArgs e )
    {
      OnMouseClick();
    }
    private void picCornerBottom_MouseClick( object sender, MouseEventArgs e )
    {
      OnMouseClick();
    }

    #endregion
  }
}
