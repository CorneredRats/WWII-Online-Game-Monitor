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
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using BEGM.Properties;
using XPExplorerBar;
using Xiperware.WiretapAPI;

namespace BEGM.Widgets
{
  /// <summary>
  /// The TownStatus widget displays detailed information about the current state of the
  /// selected chokepoint. This includes the general chokepoint information, contained
  /// facilities, deployed hcunits, linked towns &amp; firebases, and any nearby bridges.
  /// </summary>
  public partial class TownStatus : UserControl, IWidget
  {
    #region Variables

    /// <summary>
    /// The current game state.
    /// </summary>
    private GameState game;

    /// <summary>
    /// The currently displayed ChokePoint (may be null).
    /// </summary>
    private ChokePoint selectedChokePoint = null;

    /// <summary>
    /// The CurrentAttacks widget's NumEvents() method.
    /// </summary>
    public Func<ChokePoint,int> CurrentAttacks_NumEvents;

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
    /// Create a new TownStatus widget.
    /// </summary>
    public TownStatus()
    {
      InitializeComponent();


      // dynamic positioning

      lnkMap.Left = 331 - lnkMap.Width;
      lnkEvents.Left = lnkMap.Left - 6 - lnkEvents.Width;
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
    /// Gets the currently displayed ChokePoint (or null if none is selected).
    /// </summary>
    public ChokePoint SelectedChokePoint
    {
      get { return this.selectedChokePoint; }
    }

    /// <summary>
    /// Gets or sets the internal widget state.
    /// </summary>
    internal TownStatusState State
    {
      get
      {
        TownStatusState state = new TownStatusState
        {
          selectedChokePointId = this.selectedChokePoint == null ? 0 : this.selectedChokePoint.ID,
        };

        return state;
      }
      set
      {
        foreach( var item in lbCPList.Items )
        {
          ChokePoint cp = (ChokePoint)item;
          if( cp.ID == value.selectedChokePointId )
          {
            CenterSelection( cp );
            DisplayChokePointInfo( cp );
            break;
          }
        }
      }
    }

    /// <summary>
    /// Update the LinkBehavior when setting lnkEvents.Enabled.
    /// </summary>
    private bool LnkEventsEnabled
    {
      set
      {
        lnkEvents.Links[0].Enabled = value;
        lnkEvents.LinkBehavior = value ? LinkBehavior.HoverUnderline : LinkBehavior.NeverUnderline;
      }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Initialise the TownStatus widget.
    /// </summary>
    /// <param name="game">The current game state.</param>
    public void InitWidget( GameState game )
    {
      Log.AddEntry( "  TownStatus..." );


      // keep ref to bridge list to calculate nearby bridges

      this.game = game;


      // populate and sort the chokepoint list

      lbCPList.Items.Clear();
      lbCPList.Sorted = false;
      foreach( ChokePoint cp in game.ValidChokePoints.Where( cp => !cp.IsTraining ) )
        lbCPList.Items.Add( cp );
      lbCPList.Sorted = true;  // only sort once at end


      Log.Okay();
    }

    /// <summary>
    /// Refreshes the currently displayed ChokePoint.
    /// </summary>
    public void UpdateWidget()
    {
      if( this.game == null ) return;

      Expando expTownStatus = (Expando)this.Parent;
      if( expTownStatus.Collapsed ) return;

      if( this.selectedChokePoint != null )
        DisplayChokePointInfo( this.selectedChokePoint );
    }

    /// <summary>
    /// Remove references to local game objects.
    /// </summary>
    public void Cleanup()
    {
      DisplayChokePointInfo( null );  //this.selectedChokePoint = null;
      this.game = null;
      lbCPList.Items.Clear();
      picCPInfo.RemoveAll();
    }

    /// <summary>
    /// Makes the Town Status widget visible by making sure it's expanded, scrolling it into
    /// view, and populating it with the given ChokePoint.
    /// </summary>
    /// <seealso cref="tmrReveal_Tick"/>
    /// <param name="arg">The ChokePoint to display.</param>
    public void Reveal( object arg )
    {
      ChokePoint cp = arg as ChokePoint;
      if( cp == null ) return;

      if( tmrReveal.Enabled )
        return;  // another Reveal() in progress

      CenterSelection( cp );
      DisplayChokePointInfo( cp );

      if( this.selectedChokePoint == null )  // not found
        return;

      Expando expTownStatus = (Expando)this.Parent;
      if( expTownStatus.Collapsed )
      {
        expTownStatus.Collapsed = false;  // animate
        tmrReveal.Start();                // scroll into view after animation
      }
      else
      {
        if( expTownStatus.TaskPane != null )
          expTownStatus.TaskPane.ScrollControlIntoView( expTownStatus );
      }
    }



    /// <summary>
    /// Updates the various controls to display information about the given ChokePoint.
    /// </summary>
    /// <param name="cp">The ChokePoint to display, or null to clear.</param>
    private void DisplayChokePointInfo( ChokePoint cp )
    {
      this.selectedChokePoint = cp;


      // if null, clear everything

      if( cp == null )
      {
        picCPFlag.Image = null;

        GameStatus.ToolTip.SetToolTip( picCPFlag, null );
        lblCPName.Text = null;
        lnkEvents.Visible = lnkMap.Visible = false;

        picCPInfo.Image = null;
        picCPInfo.RemoveAll();  // remove previous tooltips

        return;
      }


      // hide start tip

      if( lblStartTip.Visible )
        lblStartTip.Visible = false;


      // flag

      picCPFlag.Image = cp.FlagImage;
      GameStatus.ToolTip.SetToolTip( picCPFlag, cp.FlagTooltip );


      // title

      lblCPName.Text = cp.Name;


      // info

      picCPInfo.Image = GenerateInfoImage( cp, picCPInfo );
      picCPInfo.Height = picCPInfo.Image.Height;


      // event link

      int displayedEventCount = CurrentAttacks_NumEvents( cp );

      int actualEventCount = 0;
      foreach( GameEvent gameEvent in cp.Events )
        actualEventCount++;

      if( displayedEventCount > 0 )
      {
        lnkEvents.Text = String.Format( displayedEventCount == 1 ? Language.TownStatus_Event : Language.TownStatus_Events, displayedEventCount );
        lnkEvents.Visible = true;
        LnkEventsEnabled = true;
      }
      else if( actualEventCount > 0 )  // events exist but not present in CurrentAttacks
      {
        lnkEvents.Text = String.Format( actualEventCount == 1 ? Language.TownStatus_Event : Language.TownStatus_Events, actualEventCount );
        lnkEvents.Visible = true;
        LnkEventsEnabled = false;
      }
      else
      {
        lnkEvents.Visible = false;
      }


      // map link

      lnkMap.Visible = true;


      // resize parent expando

      Expando expTownStatus = (Expando)this.Parent;
      if( expTownStatus.ExpandedHeight != 151 + picCPInfo.Height )
      {
        expTownStatus.SuspendLayout();
        expTownStatus.ExpandedHeight = 151 + picCPInfo.Height;
        expTownStatus.Height += 1;  // workaround for bug in tskMain ScrollableControl:
        expTownStatus.Height -= 1;  // force the correct scroll height
        expTownStatus.ResumeLayout();
      }
    }

    /// <summary>
    /// Generate an info image for the given ChokePoint, showing facility status,
    /// deployed hcunits, and linked cps.
    /// </summary>
    /// <param name="cp">The reference ChokePoint.</param>
    /// <param name="imgmap">An ImageMap on which to add tooltips & clickable regions.</param>
    /// <returns>The info image.</returns>
    private Image GenerateInfoImage( ChokePoint cp, ImageMap.ImageMap imgmap )
    {
      #region Init

      // get data

      ReadOnlyCollection<Bridge> nearbyBridges = cp.GetNearbyBridges( game.Bridges );
      SortedList<float, ChokePoint> attackFrom = cp.AttackFrom;


      // calculate required dimensions

      const int lineHeight = 14;
      int linesRequired = 5;

      if( attackFrom.Count == 1 )
        linesRequired += 1;
      else if( attackFrom.Count > 1 )
        linesRequired += 1 + attackFrom.Count;

      if( cp.IsContested )
        linesRequired += 2;  // capbar

      linesRequired += 1 + cp.Facilities.Count;

      if( cp.LinkedChokePoints.Count > 0 )
        linesRequired += 1 + cp.LinkedChokePoints.Count;

      if( cp.DeployedHCUnits.Count > 0 )
      {
        linesRequired++;
        HCUnit prevDivision = null;
        foreach( HCUnit hcunit in cp.DeployedHCUnits )
        {
          linesRequired++;
          HCUnit division = hcunit.Level == HCUnitLevel.Division ? hcunit : hcunit.ParentUnit;
          if( division != prevDivision ) { linesRequired++; prevDivision = division; }
        }
      }

      if( nearbyBridges.Count > 0 )
        linesRequired += 1 + nearbyBridges.Count;

      int height = ( linesRequired * lineHeight ) + 2;
      const int width = 334;


      // create bitmap, graphics resources

      Bitmap bitmap = new Bitmap( width, height );
      Graphics g = Graphics.FromImage( bitmap );
      g.SmoothingMode = SmoothingMode.HighQuality;
      g.InterpolationMode = InterpolationMode.High;

      Brush brushMain         = new SolidBrush( Color.FromArgb( 192, 192, 192 ) );
      Brush brushHead         = new SolidBrush( Color.FromArgb( 224, 224, 224 ) );
      Brush brushHCUnitAllied = new SolidBrush( Color.FromArgb( 128, 204, 255 ) );  // light blue
      Brush brushHCUnitAxis   = new SolidBrush( Color.FromArgb( 255, 178, 102 ) );  // orange

      Pen penDark = new Pen( Color.FromArgb( 80, 80, 80 ) );

      Font fontMain = new Font( "Tahoma", 8.25F );
      Font fontHead = new Font( "Tahoma", 8.25F, FontStyle.Bold );

      StringFormat alignRight = new StringFormat { Alignment = StringAlignment.Far };
      StringFormat formatNoWrap = new StringFormat { FormatFlags = StringFormatFlags.NoWrap };  // prevents odd spacing when line is clipped

      DateTime oneHourAgo = DateTime.Now.AddHours( -1 );
      DateTime twelveHoursAgo = DateTime.Now.AddHours( -12 );

      imgmap.RemoveAll();  // remove previous tooltips

      int y = 0;
      int[] xTab = new[] { 2, 15, 32, 44 };
      int stringWidth;

      #endregion

      #region General info

      // get x position for owner/controller values

      float xFlagPos = 80;  // min
      stringWidth = (int)g.MeasureString( Language.Misc_OwnedBy, fontHead ).Width;
      if( xTab[0] + stringWidth + 2 > xFlagPos )
        xFlagPos = xTab[0] + stringWidth + 2;
      stringWidth = (int)g.MeasureString( Language.Misc_ControlledBy, fontHead ).Width;
      if( xTab[0] + stringWidth + 2 > xFlagPos )
        xFlagPos = xTab[0] + stringWidth + 2;
      xFlagPos = (float)Math.Round( xFlagPos );


      // owner

      g.DrawString( Language.Misc_OwnedBy, fontHead, brushHead, xTab[0], y );
      g.DrawImage( cp.Owner.CountryFlag, xFlagPos, y + 3.5F, 12, 7.125F );

      string captured = null;
      if( cp.LastOwnerChanged > twelveHoursAgo )  // within past 12 hours
        captured = " (" + String.Format( Language.Time_CapturedMinsAgo, Misc.MinsAgoLong( cp.LastOwnerChanged ) ) + ")";

      g.DrawString( String.Format( "{0}{1}", cp.Owner, captured ), fontMain, brushMain, xFlagPos + 15, y );
      y += lineHeight;


      // controller

      g.DrawString( Language.Misc_ControlledBy, fontHead, brushHead, xTab[0], y );
      g.DrawImage( cp.Controller.CountryFlag, xFlagPos, y + 3.5F, 12, 7.125F );

      string gained = null;
      if( cp.LastControllerChanged > twelveHoursAgo )  // within past 12 hours
        gained = " (" + String.Format( cp.Controller.Side == cp.Owner.Side ? Language.Time_RegainedMinsAgo : Language.Time_GainedMinsAgo,
                                       Misc.MinsAgoLong( cp.LastControllerChanged ) ) + ")";

      g.DrawString( String.Format( "{0}{1}", cp.Controller, gained ), fontMain, brushMain, xFlagPos + 15, y );
      y += lineHeight;


      // contested

      if( cp.IsContested )
      {
        g.DrawImage( Resources.contested, xTab[1], y + 2, 13, 10.52F );  // 21x17

        string since = null;
        if( cp.LastContestedChanged > twelveHoursAgo )  // within past 12 hours
          since = " (" + String.Format( Language.Time_ForMins, Misc.MinsAgoLong( cp.LastContestedChanged ) ) + ")";

        g.DrawString( Language.Misc_Contested + since, fontMain, brushMain, xTab[2], y );
      }
      else
      {
        g.DrawImage( Resources.uncontested, xTab[1], y + 2, 13, 10.52F );  // 21x17

        string since = null;
        if( cp.LastContestedChanged > twelveHoursAgo )  // within past 12 hours
          since = " (" + String.Format( Language.Time_ForMins, Misc.MinsAgoLong( cp.LastContestedChanged ) ) + ")";

        g.DrawString( Language.Misc_Uncontested + since, fontMain, brushMain, xTab[2], y );
      }
      y += lineHeight;


      // attack objective

      if( cp.HasAO )
      {
        g.DrawImage( Resources.attack_objective, xTab[1], y + 2, 12, 10.29F );  // 21x18

        string placed = null;
        if( cp.LastAOChanged > twelveHoursAgo )  // within past 12 hours
          placed = " (" + String.Format( Language.Time_PlacedMinsAgo, Misc.MinsAgoLong( cp.LastAOChanged ) ) + ")";
        g.DrawString( Language.TownStatus_AttackObjective + placed, fontMain, brushMain, xTab[2], y );

        string attackUnit = cp.AttackingHCUnit.Title;
        if( cp.AttackingHCUnit.DeployedChokePoint != null )
          attackUnit += " " + String.Format( Language.Misc_AtChokePoint, cp.AttackingHCUnit.DeployedChokePoint );

        string aoTooltip = String.Format( Language.TownStatus_Tooltip_AOPlacedBy, attackUnit );

        imgmap.AddRectangle( aoTooltip, new Rectangle( xTab[1], y + 2, 100, 10 ) );

        y += lineHeight;
      }
      else
      {
        g.DrawImage( Resources.no_attack_objective, xTab[1], y + 2, 12, 10.29F );  // 21x18

        string removed = null;
        if( cp.LastAOChanged > twelveHoursAgo )  // within past 12 hours
          removed = " (" + String.Format( Language.Time_RemovedMinsAgo, Misc.MinsAgoLong( cp.LastAOChanged ) ) + ")";

        g.DrawString( Language.TownStatus_NoAttackObjective + removed, fontMain, brushMain, xTab[2], y );
        y += lineHeight;
      }


      // activity level

      g.DrawImage( cp.ActivityImage, xTab[1], y + 2, 12, 10.8F );  // 20x18
      g.DrawString( Language.TownStatus_Activity + " " + Misc.EnumString( cp.Activity ), fontMain, brushMain, xTab[2], y );
      y += lineHeight;


      // attack from

      if( attackFrom.Count > 0 )
      {
        if( attackFrom.Count == 1 )
        {
          g.DrawString( Language.TownStatus_UnderAttackFrom + " " + attackFrom.Values[0].Name, fontMain, brushMain, xTab[2], y );
          y += lineHeight;
        }
        else
        {
          g.DrawString( Language.TownStatus_UnderAttackFrom, fontMain, brushMain, xTab[2], y );
          y += lineHeight;

          for( int i = attackFrom.Count - 1; i >= 0; i-- )
          {
            g.DrawString( String.Format( "{0} ({1:0}%)", attackFrom.Values[i], attackFrom.Keys[i] ), fontMain, brushMain, xTab[3], y );
            y += lineHeight;
          }
        }
      }

      #endregion

      #region Base counts

      int countArmybase = 0, countAirbase = 0, countNavalbase = 0;

      foreach( Facility facility in cp.Facilities )
      {
        if( facility.Type == FacilityType.Armybase )
          countArmybase++;
        else if( facility.Type == FacilityType.Airbase )
          countAirbase++;
        else if( facility.Type == FacilityType.Navalbase )
          countNavalbase++;
      }

      int yOffset = 0;
      if( countArmybase > 0 )
      {
        g.DrawImage( Resources.armybase, 300, yOffset, 18, 16 );
        g.DrawString( "x" + countArmybase, fontMain, brushMain, 334, yOffset + 1, alignRight );
        yOffset += 18;
      }
      if( countAirbase > 0 )
      {
        g.DrawImage( Resources.airbase, 300, yOffset, 18, 16 );
        g.DrawString( "x" + countAirbase, fontMain, brushMain, 334, yOffset + 1, alignRight );
        yOffset += 20;
      }
      if( countNavalbase > 0 )
      {
        g.DrawImage( Resources.navalbase, 300, yOffset, 18, 16 );
        g.DrawString( "x" + countNavalbase, fontMain, brushMain, 334, yOffset + 1, alignRight );
      }

      #endregion

      #region Capbar

      if( cp.IsContested )
      {
        const int left = 19;
        const int right = 319;
        int range = right - left;
        int mid = (int)( ( 100 - cp.PercentOwnership ) * range / 100 ) + left;

        g.DrawImage( cp.Owner.Side == Side.Allied ? Resources.capbar_red_left : Resources.capbar_blue_left,
                     left - 4, y + 9, 4, 10 );
        g.DrawImage( cp.Owner.Side == Side.Allied ? Resources.capbar_red : Resources.capbar_blue,
                     new Rectangle( left, y + 9, mid - left, 10 ),
                     new Rectangle( 0, 0, 1, 10 ), GraphicsUnit.Pixel );
        g.DrawImage( cp.Owner.Side == Side.Allied ? Resources.capbar_blue : Resources.capbar_red,
                     new Rectangle( mid, y + 9, right - mid, 10 ),
                     new Rectangle( 0, 0, 1, 10 ), GraphicsUnit.Pixel );
        g.DrawImage( cp.Owner.Side == Side.Allied ? Resources.capbar_blue_right : Resources.capbar_red_right,
                     right, y + 9, 4, 10 );
        g.DrawImage( Resources.capbar_pointer, mid - 13, y + 6, 27, 23 );

        y += lineHeight + lineHeight;
      }

      #endregion

      #region Facility list

      int maxNameWidth = 255;

      // heading(s)

      g.DrawString( Language.TownStatus_Section_Facilities, fontHead, brushHead, xTab[0], y );
      if( cp.HasAO || cp.IsContested )
      {
        g.DrawString( Language.TownStatus_Tables, fontMain, brushHead, 252, y );
        maxNameWidth = 215;
      }
      if( cp.IsContested )
      {
        g.DrawString( Language.TownStatus_HeldFor, fontMain, brushHead, 250, y, alignRight );
        maxNameWidth = 160;
      }
      g.DrawString( Language.TownStatus_Spawn, fontMain, brushHead, 297, y );


      // facility loop

      foreach( Facility facility in cp.Facilities )
      {
        y += lineHeight;


        // owner flag & facility icon

        g.DrawImage( facility.Owner.CountryFlag, xTab[1], y + 3.5F, 12, 7.125F );
        g.DrawImage( facility.Icon, xTab[2], y + 2.5F, 11, 8.94F );


        // facility name

        g.DrawString( facility.Name, fontMain, brushMain, new RectangleF( xTab[3], y, maxNameWidth, lineHeight ), formatNoWrap );


        // held for column

        if( cp.IsContested )
        {
          if( facility.LastOwnerChanged > twelveHoursAgo )  // within past 12 hours
          {
            Brush brush;
            if( facility.LastOwnerChanged < oneHourAgo )
              brush = new SolidBrush( Color.FromArgb( 128, 128, 128 ) );
            else
              brush = new SolidBrush( Misc.GetFadedYellow( facility.LastOwnerChanged, 15, 192 ) );

            g.DrawString( Misc.MinsAgoShort( facility.LastOwnerChanged ), fontMain, brush, 250, y, alignRight );
            imgmap.AddRectangle( Misc.Timestamp( facility.LastOwnerChanged ) + " " + String.Format( Language.TownStatus_Tooltip_CapturedBy, facility.LastCapper ),
                                 new Rectangle( 210, y + 2, 38, 10 ) );
          }
          else
            g.DrawLine( penDark, 209, y + 6, 247.5F, y + 6 );
        }


        // table status column

        if( cp.HasAO || cp.IsContested )
        {
          if( facility.RadioTableUp )
            g.DrawImage( Resources.table_up, 269, y + 2, 12, 10 );
          else
            g.DrawImage( Resources.table_down, 269, y + 2, 12, 10 );
        }


        // spawnable column

        Image spawnImage;
        string spawnTooltip;

        if( !facility.SpawnableFacility )
        {
          spawnImage = Resources.spawn_nonspawnable;
          spawnTooltip = null;
        }
        else if( facility.Owner.Side == cp.Owner.Side )  // friendly
        {
          if( facility.EnemySpawnable )
          {
            spawnImage = Resources.spawn_enemyspawnable;
            spawnTooltip = Language.TownStatus_Tooltip_SpawnSpawnable;
          }
          else if( facility.OwnerCanSpawn )
          {
            spawnImage = Resources.spawn_friendly;
            spawnTooltip = Language.TownStatus_Tooltip_SpawnFriendly;
          }
          else
          {
            spawnImage = Resources.spawn_closed;
            spawnTooltip = Language.TownStatus_Tooltip_SpawnClosed;
          }
        }
        else  // enemy
        {
          if( facility.OwnerCanSpawn )
          {
            spawnImage = Resources.spawn_enemy;
            spawnTooltip = Language.TownStatus_Tooltip_SpawnEnemy;
          }
          else
          {
            spawnImage = Resources.spawn_closed;
            spawnTooltip = Language.TownStatus_Tooltip_SpawnClosed;
          }
        }

        g.DrawImage( spawnImage, 311, y + 2, 9, 9 );
        if( spawnTooltip != null )
          imgmap.AddRectangle( spawnTooltip, new Rectangle( 311, y + 2, 9, 9 ) );
      
      }  // end facility loop

      #endregion

      #region Deployed hcunits

      if( cp.DeployedHCUnits.Count > 0 )
      {
        int xTimeColumn = new BegmMisc.TextWidth( fontMain, 70, 230 ).MeasureAll( cp.DeployedHCUnits ).MaxWidth + 100;

        y += lineHeight;
        g.DrawString( Language.TownStatus_Section_HCUnits, fontHead, brushHead, xTab[0], y );

        HCUnit prevDivision = null;

        foreach( HCUnit hcunit in cp.DeployedHCUnits )
        {
          y += lineHeight;

          HCUnit division = hcunit.Level == HCUnitLevel.Division ? hcunit : hcunit.ParentUnit;
          HCUnit brigade  = hcunit.Level == HCUnitLevel.Brigade  ? hcunit : hcunit.GetDummyDivHQBrigade();

          if( division != prevDivision )  // draw division heading w/ tooltip
          {
            prevDivision = division;

            g.DrawImage( division.Country.BrigadeFlag, xTab[1], y + 2.5F, 17.57F, 9 );  // 41x21
            g.DrawString( division.Title, fontMain, brushMain, xTab[2], y );

            string divTooltip = null;
            if( division.IsDeployed )
            {
              if( division.DeployedChokePoint != cp )
                divTooltip = String.Format( Language.Common_Tooltip_DivisionDeployed, division.DeployedChokePoint );
            }
            else
              divTooltip = Language.Common_Tooltip_DivisionNotDeployed;

            if( divTooltip != null )
            {
              stringWidth = (int)g.MeasureString( division.Title, fontMain, width - 32 ).Width - 2;
              imgmap.AddRectangle( divTooltip, new Rectangle( xTab[2] + 1, y + 2, stringWidth, 10 ), true, division );
            }

            y += lineHeight;
          }


          // draw unit title, time w/ last moved tooltip

          g.FillEllipse( hcunit.Country.Side == Side.Allied ? brushHCUnitAllied : brushHCUnitAxis,
                         xTab[2] + 4, y + 4, 5, 5 );
          g.DrawString( brigade.Title, fontMain, brushMain, xTab[3], y );
          if( hcunit.HasLastMoved )
            g.DrawString( Misc.MinsAgoShort( hcunit.LastMovedTime ), fontMain, brushMain, xTimeColumn, y, alignRight );

          string brigTooltip = hcunit.Tooltip;
          if( brigTooltip != null )
          {
            stringWidth = (int)g.MeasureString( brigade.Title, fontMain, width - xTab[3] ).Width - 2;
            imgmap.AddRectangle( brigTooltip, new Rectangle( xTab[3] + 1, y + 2, stringWidth, 10 ), true, hcunit );
          }
        }
      }

      #endregion

      #region Linked cps/firebases

      if( cp.LinkedChokePoints.Count > 0 )
      {
        // calc x position for kms column based on max town name length

        float xKmsColumn = 170;  // min
        foreach( ChokePoint linkedChokePoint in cp.LinkedChokePoints )
        {
          stringWidth = (int)g.MeasureString( linkedChokePoint.Name, fontMain ).Width;
          if( stringWidth + 100 > xKmsColumn )
            xKmsColumn = stringWidth + 100;
        }


        y += lineHeight;
        g.DrawString( Language.TownStatus_Section_LinkedTowns, fontHead, brushHead, xTab[0], y );

        foreach( ChokePoint linkedChokePoint in cp.LinkedChokePoints )
        {
          y += lineHeight;


          // owner flag

          g.DrawImage( linkedChokePoint.Owner.CountryFlag, xTab[1], y + 3.5F, 12, 7.125F );
          imgmap.AddRectangle( String.Format( Language.Common_Tooltip_GoToMap, linkedChokePoint.Name ), new Rectangle( xTab[1], y + 3, 12, 8 ), true, linkedChokePoint.ID );


          // fb icon

          Firebase linkedFirebase = cp.GetFirebaseFrom( linkedChokePoint );
          string fbTooltip;

          if( linkedFirebase == null )
            fbTooltip = String.Format( Language.TownStatus_Tooltip_FirebaseNone, cp, linkedChokePoint );
          else  // draw fb icon
          {
            Image fbImage;

            if( linkedFirebase.Link.State == FirebaseState.Inactive )
            {
              fbImage = Resources.facility_firebase;
              fbTooltip = String.Format( Language.TownStatus_Tooltip_FirebaseInactive, cp, linkedChokePoint );
            }
            else if( linkedFirebase.IsOpen )
            {
              fbImage = Resources.facility_firebase_open;
              fbTooltip = String.Format( Language.TownStatus_Tooltip_FirebaseUp,
                                         linkedChokePoint, cp, Misc.EnumString( linkedFirebase.Link.State ).ToLower(), Misc.EnumString( linkedFirebase.Link.Side ) );
            }
            else
            {
              fbImage = Resources.facility_firebase_closed;
              fbTooltip = String.Format( Language.TownStatus_Tooltip_FirebaseDown,
                                         linkedChokePoint, cp, Misc.EnumString( linkedFirebase.Link.State ).ToLower(), Misc.EnumString( linkedFirebase.Link.Side ) );
            }

            g.DrawImage( fbImage, xTab[2], y + 2.5F, 11, 8.94F );
          }
          imgmap.AddRectangle( fbTooltip, new Rectangle( xTab[2], y + 2, 11, 9 ) );


          // linked cp name

          g.DrawString( linkedChokePoint.Name, fontMain, brushMain, xTab[3], y );
          stringWidth = (int)g.MeasureString( linkedChokePoint.Name, fontMain, width - xTab[3] ).Width - 2;
          imgmap.AddRectangle( String.Format( Language.Common_Tooltip_GoToStatus, linkedChokePoint.Name ), new Rectangle( xTab[3] + 1, y + 2, stringWidth, 10 ), true, linkedChokePoint );


          // distance

          double distance = Misc.DistanceBetween( cp.LocationOctets, linkedChokePoint.LocationOctets ) * 0.8;  // x 800m / 1000m
          g.DrawString( distance.ToString( "0.0 km" ), fontMain, brushMain, (int)xKmsColumn, y, alignRight );

        }

      }

      #endregion

      #region Nearby bridges

      if( nearbyBridges.Count > 0 )
      {
        y += lineHeight;
        g.DrawString( Language.TownStatus_Section_Bridges, fontHead, brushHead, xTab[0], y );

        foreach( Bridge bridge in nearbyBridges )
        {
          y += lineHeight;
          g.DrawImage( Resources.bridge, xTab[2], y + 3, 11, 9 );
          g.DrawString( bridge.Name, fontMain, brushMain, xTab[3], y );
        }
      }

      #endregion

      g.Dispose();
      return bitmap;
    }

    /// <summary>
    /// Select the given ChokePoint in lbCPList and center it.
    /// </summary>
    /// <param name="cp">The ChokePoint to find in the list.</param>
    private void CenterSelection( ChokePoint cp )
    {
      lbCPList.SelectedItem = cp;
      lbCPList.TopIndex = lbCPList.SelectedIndex < 3 ? 0 : lbCPList.SelectedIndex - 3;
    }

    #endregion

    #region Event Handlers

    // CPLIST

    // ownerdraw displaycps to draw custom background colour
    private void lbCPList_DrawItem( object sender, DrawItemEventArgs e )
    {
      if( e.Index == -1 ) return;

      if( ( e.State & DrawItemState.Selected ) == DrawItemState.Selected )
        e = new DrawItemEventArgs( e.Graphics, e.Font, e.Bounds, e.Index, e.State ^ DrawItemState.Selected, e.ForeColor, Color.FromArgb( 51, 51, 51 ) );

      e.DrawBackground();
      e.Graphics.DrawString( lbCPList.Items[e.Index].ToString(), e.Font, new SolidBrush( Color.FromArgb( 224, 224, 224 ) ), e.Bounds );
    }

    // mouse click
    private void lbCPList_Click( object sender, EventArgs e )
    {
      DisplayChokePointInfo( (ChokePoint)lbCPList.SelectedItem );
    }

    // image map link click
    private void picCPInfo_RegionClick( int index, object tag, MouseButtons button )
    {
      if( button != MouseButtons.Left )
        return;

      if( tag is ChokePoint )
      {
        // show town status
        CenterSelection( (ChokePoint)tag );
        DisplayChokePointInfo( (ChokePoint)tag );
      }
      else if( tag is HCUnit )
      {
        // reveal orbat

        GameStatus_RevealWidget( WidgetType.BrigadeStatus, tag );
      }
      else if( tag is int )
      {
        // reveal map

        GameStatus_RevealWidget( WidgetType.GameMap, tag );
      }
    }

    // when scrolling via up/down keys, update after 100ms (not for each item passed over)
    private void lbCPList_KeyDown( object sender, KeyEventArgs e )
    {
      if( e.KeyCode == Keys.Up || e.KeyCode == Keys.Down )
      {
        tmrCPListSelect.Stop();
        tmrCPListSelect.Start();
      }
    }
    private void tmrCPListSelect_Tick( object sender, EventArgs e )
    {
      tmrCPListSelect.Stop();
      DisplayChokePointInfo( (ChokePoint)lbCPList.SelectedItem );
    }

    // allow incremental searching via keypresses within 750ms
    private string lbCPListSearchText = "";
    private void lbCPList_KeyPress( object sender, KeyPressEventArgs e )
    {
      lbCPListSearchText += e.KeyChar;

      tmrCPListSearch.Stop();
      tmrCPListSearch.Start();

      int index = lbCPList.FindString( lbCPListSearchText );
      lbCPList.SelectedIndex = index;

      CenterSelection( (ChokePoint)lbCPList.SelectedItem );
      DisplayChokePointInfo( (ChokePoint)lbCPList.SelectedItem );

      e.Handled = true;
    }
    private void tmrCPListSearch_Tick( object sender, EventArgs e )
    {
      // time expired, reset search text

      tmrCPListSearch.Stop();
      lbCPListSearchText = "";
    }


    // OTHER

    // event link
    private void lnkEvents_LinkClicked( object sender, LinkLabelLinkClickedEventArgs e )
    {
      ChokePoint cp = lbCPList.SelectedItem as ChokePoint;

      if( cp != null )
        GameStatus_RevealWidget( WidgetType.CurrentAttacks, cp );
    }

    // map link
    private void lnkMap_LinkClicked( object sender, LinkLabelLinkClickedEventArgs e )
    {
      GameStatus_RevealWidget( WidgetType.GameMap, lbCPList.SelectedItem );
    }

    // dynamic focus
    private void TownStatus_MouseEnter( object sender, EventArgs e )
    {
      BegmMisc.FocusTaskPane( this );
    }
    private void lbCPList_MouseEnter( object sender, EventArgs e )
    {
      BegmMisc.FocusWithoutScroll( lbCPList );
    }
    private void picCPInfo_MyMouseEnter( object sender, EventArgs e )
    {
      BegmMisc.FocusTaskPane( this );
    }

    // Reveal() timer
    private void tmrReveal_Tick( object sender, EventArgs e )
    {
      Expando expTownStatus = (Expando)this.Parent;

      if( expTownStatus.Animating )
        return;  // wait until finished animating

      if( expTownStatus.TaskPane != null )
        expTownStatus.TaskPane.ScrollControlIntoView( expTownStatus );

      tmrReveal.Stop();
    }

    #endregion
  }
}
