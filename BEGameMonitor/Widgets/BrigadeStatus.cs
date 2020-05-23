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
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using BEGM.Properties;
using XPExplorerBar;
using Xiperware.WiretapAPI;

namespace BEGM.Widgets
{
  /// <summary>
  /// The Brigade Status widget lists information about a specific brigade, such as
  /// division members, deployment and command details, and movement history.
  /// </summary>
  public partial class BrigadeStatus : UserControl, IWidget
  {
    #region Variables

    private List<HCUnit> hcunits;

    private int iActive = -1;    // mouseover
    private HCUnit selectedHCUnit = null;  // mouseclick

    /// <summary>
    /// Order events in the movement history list are displayed.
    /// </summary>
    private SortOrder eventSortOrder = SortOrder.Ascending;

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
    /// Create a new BrigadeStatus widget.
    /// </summary>
    public BrigadeStatus()
    {
      InitializeComponent();

      this.hcunits = new List<HCUnit>();

      dgvMoves.ForeColor = Color.Silver;  // workaround: dgvMoves.DefaultCellStyle.ForeColor keeps reverting to dgvMoves.ForeColor

      //// avoid default DataError dialog
      dgvHCUnits.DataError += ( sender, args ) => { };
      dgvMoves.DataError += ( sender, args ) => { };


      // dynamic positioning

      cbMovesHideFailed.Left = pnlMoves.Width - cbMovesHideFailed.Width + 2;
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
    internal BrigadeStatusState State
    {
      get
      {
        BrigadeStatusState state = new BrigadeStatusState();
        state.hideFailedAttempts = cbMovesHideFailed.Checked;

        return state;
      }
      set
      {
        cbMovesHideFailed.Checked = value.hideFailedAttempts;
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


        // update sort order

        UpdateMoveHistory( false );
      }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Initialise the Brigade Status widget.
    /// </summary>
    /// <param name="game">The current game state.</param>
    public void InitWidget( GameState game )
    {
      Log.AddEntry( "  BrigadeStatus..." );


      // init hcunit grid

      dgvHCUnits.DefaultCellStyle.Font = new Font( "Microsoft Sans Serif", 6.75F );
      dgvHCUnits.DataSource = GetHCUnitDataSource( game.TopHCUnit );  // note: generates columns

      for( int i = 0; i < dgvHCUnits.Columns.Count; i++ )
      {
        dgvHCUnits.Columns[i].DataPropertyName = "Column" + i;
        dgvHCUnits.Columns[i].Width = i == dgvHCUnits.Columns.Count - 1 ? 49 : 48;
        dgvHCUnits.Columns[i].DefaultCellStyle = dgvHCUnits.DefaultCellStyle;
      }


      // set tree images

      picTreeBrigade.Image = picTreeDivision.Image = picTreeCorps.Image = GenerateTreeImage();


      Log.Okay();
    }

    /// <summary>
    /// Refreshes the currently displayed HCUnit.
    /// </summary>
    public void UpdateWidget()
    {
      if( this.selectedHCUnit != null )
        DisplayHCUnitInfo( this.selectedHCUnit );
    }

    /// <summary>
    /// Remove references to local game objects.
    /// </summary>
    public void Cleanup()
    {
      if( this.hcunits != null )
      {
        this.hcunits.Clear();
        this.hcunits = null;
      }
      this.selectedHCUnit = null;
    }

    /// <summary>
    /// Makes the BrigadeStatus widget visible by making sure it's expanded, scrolling it into
    /// view, and selecting the given hcunit.
    /// </summary>
    /// <seealso cref="tmrReveal_Tick"/>
    /// <param name="arg">The HCUnit to display (should be a division/brigade).</param>
    public void Reveal( object arg )
    {
      HCUnit hcunit = arg as HCUnit;
      if( hcunit == null ) return;

      if( tmrReveal.Enabled )
        return;  // another Reveal() in progress

      int i = this.hcunits.IndexOf( hcunit );
      if( i < 0 ) return;  // not a brigade/division

      Expando expEquipment = (Expando)this.Parent;
      if( expEquipment.Collapsed )
      {
        expEquipment.Collapsed = false;  // animate
        tmrReveal.Start();               // scroll into view after animation
      }
      else
      {
        if( expEquipment.TaskPane != null )
          expEquipment.TaskPane.ScrollControlIntoView( expEquipment );
      }

      // must be done after animating open otherwise it reverts?
      dgvHCUnits[i / dgvHCUnits.RowCount, i % dgvHCUnits.RowCount].Selected = true;
    }



    /// <summary>
    /// Generates a list of HCUnitRow's to use as a virtual datasource for the hcunits
    /// Data Grid View.
    /// </summary>
    /// <param name="topHCUnit">The top-level hcunit, used to recurse down the hcunit tree.</param>
    /// <returns>A List of HCUnitRow's.</returns>
    private List<HCUnitRow> GetHCUnitDataSource( HCUnit topHCUnit )
    {
      // generate a flat sorted list of hcunits

      this.hcunits = new List<HCUnit>();
      AddToList( topHCUnit );
      this.hcunits.Sort();


      // calc cols/rows

      const int numCols = 7;
      int numRows = (int)Math.Ceiling( (double)this.hcunits.Count / numCols );  // interger division, rounded up


      // create rows of hcunits

      List<HCUnitRow> data = new List<HCUnitRow>();
      for( int iRow = 0; iRow < numRows; iRow++ )
      {
        HCUnit[] cols = new HCUnit[numCols];
        for( int iCol = 0; iCol < numCols; iCol++ )
        {
          int i = ( iCol * numRows ) + ( iRow % numRows );
          cols[iCol] = i < this.hcunits.Count ? this.hcunits[i] : null;
        }

        data.Add( new HCUnitRow( cols ) );
      }

      return data;
    }

    /// <summary>
    /// Recursively adds all divisions/brigades to this.hcunits.
    /// </summary>
    /// <param name="parent">The top level unit to process.</param>
    private void AddToList( HCUnit parent )
    {
      if( parent == null ) return;

      if( parent.Level < HCUnitLevel.Division )  // above division (eg, corps)
      {
        foreach( HCUnit child in parent.ChildUnits )
          AddToList( child );
      }
      else if( parent.Level == HCUnitLevel.Division )
      {
        this.hcunits.Add( parent );
        this.hcunits.AddRange( parent.ChildUnits );
      }
    }

    /// <summary>
    /// Updates the various controls to display information about the given hcunit.
    /// </summary>
    /// <param name="hcunit">The HCUnit to display, or null to clear.</param>
    private void DisplayHCUnitInfo( HCUnit hcunit )
    {
      bool preserveMovesScrollPosition = this.selectedHCUnit == hcunit;  // preserve if updating same unit
      this.selectedHCUnit = hcunit;


      // if null, clear everything

      if( hcunit == null )
      {
        lnkTreeBrigade.Text = lnkTreeDivision.Text = lnkTreeCorps.Text = lnkTreeBranch.Text = null;
        picTreeBrigade.Visible = picTreeDivision.Visible = picTreeCorps.Visible = false;
        picHCUnitInfo.Image = null;
        pnlMoves.Visible = false;
        return;
      }


      // update tree/title

      lblActiveTitle.Text = null;
      picTreeBrigade.Visible = picTreeDivision.Visible = picTreeCorps.Visible = true;
      Font fontTreeLarge = new Font( "Arial", 11.25F, FontStyle.Bold );
      Font fontTreeSmall = new Font( "Tahoma", 8.25F );

      lnkTreeBrigade.Text = lnkTreeDivision.Text = lnkTreeCorps.Text = lnkTreeBranch.Text = null;

      if( hcunit.Level == HCUnitLevel.Division )
      {
        lnkTreeBrigade.Text = Language.Common_DivisionHQ;
        lnkTreeDivision.Text = hcunit.Title;
        if( hcunit.ParentUnit != null )
        {
          lnkTreeCorps.Text = hcunit.ParentUnit.Title;
          if( hcunit.ParentUnit.ParentUnit != null )
          {
            lnkTreeBranch.Text = hcunit.ParentUnit.ParentUnit.Title;
          }
        }

        lnkTreeDivision.Font = fontTreeLarge;
        lnkTreeBrigade.Font = fontTreeSmall;

        lnkTreeBrigade.Top = 272;
        picTreeBrigade.Top = 275;
      }
      else
      {
        lnkTreeBrigade.Text = hcunit.Title;
        if( hcunit.ParentUnit != null )
        {
          lnkTreeDivision.Text = hcunit.ParentUnit.Title;
          if( hcunit.ParentUnit.ParentUnit != null )
          {
            lnkTreeCorps.Text = hcunit.ParentUnit.ParentUnit.Title;
            if( hcunit.ParentUnit.ParentUnit.ParentUnit != null )
            {
              lnkTreeBranch.Text = hcunit.ParentUnit.ParentUnit.ParentUnit.Title;
            }
          }
        }

        lnkTreeDivision.Font = fontTreeSmall;
        lnkTreeBrigade.Font = fontTreeLarge;

        lnkTreeBrigade.Top = 268;
        picTreeBrigade.Top = 271;
      }


      // update main info image

      try
      {
        picHCUnitInfo.Image = GenerateInfoImage( hcunit, picHCUnitInfo );
      }
      catch { }  //// unknown error here
      picHCUnitInfo.Height = picHCUnitInfo.Image.Height;


      // update movement history

      UpdateMoveHistory( preserveMovesScrollPosition );


      // resize parent expando

      UpdateWidgetHeight();

    }

    /// <summary>
    /// Generate an info image for the given HCUnit showing division, deployment
    /// and command details.
    /// </summary>
    /// <param name="hcunit">The reference HCUnit.</param>
    /// <param name="imgmap">An ImageMap on which to add tooltips & clickable regions.</param>
    /// <returns>The info image.</returns>
    private Image GenerateInfoImage( HCUnit hcunit, ImageMap.ImageMap imgmap )
    {
      HCUnit division = hcunit.Level == HCUnitLevel.Division ? hcunit : hcunit.ParentUnit;


      #region Init

      // calculate required dimensions

      const int lineHeight = 14;
      int linesRequired = 0;
      linesRequired += 2 + division.ChildUnits.Count;  // brigade members
      linesRequired += 3;  // deployment
      linesRequired += 2 + (hcunit.Owner == "" ? 0 : 1);  // command

      if( hcunit.IsDeployed )
      {
        int linesRequiredStacked = 0;
        HCUnit prevDivision = null;
        foreach( HCUnit stackHCUnit in hcunit.DeployedChokePoint.DeployedHCUnits )
        {
          if( stackHCUnit == hcunit ) continue;
          linesRequiredStacked++;
          HCUnit stackDivision = stackHCUnit.Level == HCUnitLevel.Division ? stackHCUnit : stackHCUnit.ParentUnit;
          if( stackDivision != prevDivision ) { linesRequiredStacked++; prevDivision = stackDivision; }
        }
        if( linesRequiredStacked > 0 )
          linesRequired += 1 + linesRequiredStacked;
      }

      int height = ( linesRequired * lineHeight ) + 2;
      const int width = 334;


      // create bitmap, graphics resources

      Bitmap bitmap = new Bitmap( width, height );
      Graphics g = Graphics.FromImage( bitmap );
      g.SmoothingMode = SmoothingMode.HighQuality;
      g.InterpolationMode = InterpolationMode.High;

      Brush brushHead = new SolidBrush( Color.FromArgb( 224, 224, 224 ) );
      Brush brushMain = new SolidBrush( Color.FromArgb( 192, 192, 192 ) );
      Brush brushDark = new SolidBrush( Color.FromArgb( 140, 140, 140 ) );
      Brush brushHCUnitAllied = new SolidBrush( Color.FromArgb( 128, 204, 255 ) );  // light blue
      Brush brushHCUnitAxis = new SolidBrush( Color.FromArgb( 255, 178, 102 ) );  // orange

      Font fontMain = new Font( "Tahoma", 8.25F );
      Font fontHead = new Font( "Tahoma", 8.25F, FontStyle.Bold );

      StringFormat alignRight = new StringFormat { Alignment = StringAlignment.Far };
      StringFormat formatNoWrap = new StringFormat { FormatFlags = StringFormatFlags.NoWrap };

      imgmap.RemoveAll();  // remove previous tooltips

      int x = 0, y = 0;
      int[] xTab = new[] { 2, 16, 32, 44 };
      int stringWidth;

      #endregion

      #region Division Members

      List<HCUnit> members = new List<HCUnit>();
      members.Add( division );
      members.AddRange( division.ChildUnits );


      // calc chokepoint column position

      BegmMisc.TextWidth twUnitTitle  = new BegmMisc.TextWidth( fontMain );
      BegmMisc.TextWidth twChokePoint = new BegmMisc.TextWidth( fontMain );

      foreach( HCUnit member in members )
      {
        twUnitTitle.Measure( member == division ? Language.Common_DivisionHQ : member.Title );
        twChokePoint.Measure( member.IsDeployed ? member.DeployedChokePoint.Name : Language.BrigadeStatus_NotDeployed );
      }

      twUnitTitle.MaxWidth += 7;
      twChokePoint.MaxWidth += 5;

      int xColUnitName = xTab[1] + 8;
      int xColChokePoint = xColUnitName + twUnitTitle.MaxWidth + twChokePoint.MaxWidth;
      if( xColChokePoint > this.Width - 2 )
        xColChokePoint = this.Width - 2;


      // draw heading

      g.DrawString( Language.BrigadeStatus_Section_DivisionMembers, fontHead, brushHead, xTab[0], y );
      y += lineHeight;


      // draw members

      foreach( HCUnit member in members )
      {
        Brush brushTitle = hcunit == member ? brushHead : brushMain;
        Brush brushChokePoint = member.IsDeployed ? brushTitle : brushDark;

        g.FillEllipse( member.Country.Side == Side.Allied ? brushHCUnitAllied : brushHCUnitAxis, xTab[1], y + 4, 5, 5 );

        g.DrawString( member == division ? Language.Common_DivisionHQ : member.Title, fontMain, brushTitle, new Rectangle( xColUnitName, y, xColChokePoint - twChokePoint.MaxWidth - xColUnitName, lineHeight ), formatNoWrap );
        stringWidth = (int)g.MeasureString( member == division ? Language.Common_DivisionHQ : member.Title, fontMain ).Width;
        imgmap.AddRectangle( "", new Rectangle( xColUnitName + 1, y + 2, stringWidth, 10 ), true, member );

        g.DrawString( member.IsDeployed ? member.DeployedChokePoint.Name : Language.BrigadeStatus_NotDeployed, fontMain, brushChokePoint, new Rectangle( xColChokePoint - twChokePoint.MaxWidth, y, twChokePoint.MaxWidth, lineHeight ), alignRight );
        if( member.IsDeployed )
        {
          stringWidth = (int)g.MeasureString( member.DeployedChokePoint.Name, fontMain ).Width;
          imgmap.AddRectangle( "", new Rectangle( xColChokePoint - stringWidth + 1, y + 2, stringWidth, 10 ), true, member.DeployedChokePoint );
        }
        y += lineHeight;
      }

      #endregion

      #region Deployment

      // draw timer

      float percent = hcunit.NextMovePercent;


      // get colours to display

      Brush pieForeground = hcunit.IsDeployed ? Brushes.Green : Brushes.Red;

      Brush pieBackground;
      if( percent == 0 )  // can move, darker
        pieBackground = hcunit.IsDeployed ? new SolidBrush( Color.FromArgb( 0, 50, 0 ) ) : new SolidBrush( Color.FromArgb( 100, 0, 0 ) );
      else
        pieBackground = hcunit.IsDeployed ? new SolidBrush( Color.FromArgb( 0, 80, 0 ) ) : new SolidBrush( Color.FromArgb( 150, 0, 0 ) );

      Brush pieShadowForeground = hcunit.IsDeployed ? new SolidBrush( Color.FromArgb( 0, 80, 0 ) ) : new SolidBrush( Color.FromArgb( 200, 0, 0 ) );

      Brush pieShadowBackground;
      if( percent == 0 )
        pieShadowBackground = hcunit.IsDeployed ? new SolidBrush( Color.FromArgb( 0, 30, 0 ) ) : new SolidBrush( Color.FromArgb( 70, 0, 0 ) );
      else
        pieShadowBackground = hcunit.IsDeployed ? new SolidBrush( Color.FromArgb( 0, 50, 0 ) ) : new SolidBrush( Color.FromArgb( 100, 0, 0 ) );


      // draw pie

      const int pieWidth = 40;
      const int pieHeight = 30;
      x = width - pieWidth - 5;
      float angle = ( percent * 360F ) / 100F;

      y += 5;
      for( int offset = 6; offset >= 0; offset-- )
      {
        if( offset == 0 )  // top layer
        {
          g.FillEllipse( pieBackground, x, y, pieWidth, pieHeight );
          if( percent > 0 )
            g.FillPie( pieForeground, x, y, pieWidth, pieHeight, -90, angle );
        }
        else  // 3d layer
        {
          g.FillEllipse( pieShadowBackground, x, y + offset, pieWidth, pieHeight );
          if( percent > 0 )
            g.FillPie( pieShadowForeground, x, y + offset, pieWidth, pieHeight, -90, angle );
        }
      }
      imgmap.AddElipse( hcunit.TooltipRedeploy, x + ( pieWidth / 2 ), y + ( ( pieHeight + 5 ) / 2 ), pieWidth / 2, false, null );
      y -= 5;


      // deployment info

      g.DrawString( Language.BrigadeStatus_Section_Deployment, fontHead, brushHead, xTab[0], y );
      y += lineHeight;

      x = xTab[1];
      if( hcunit.IsDeployed )
      {
        g.DrawString( Language.BrigadeStatus_DeployedAt1, fontMain, brushMain, xTab[1], y );
        x += (int)g.MeasureString( Language.BrigadeStatus_DeployedAt1, fontMain ).Width + 4;

        Country country = hcunit.DeployedChokePoint.Owner;
        if( country.Side != hcunit.Country.Side )  // can occur after town cap, before hcunit move arrives
          country = hcunit.Country;

        g.DrawImage( country.CountryFlag, x, y + 3.5F, 12, 7.125F );
        imgmap.AddRectangle( String.Format( Language.Common_Tooltip_GoToMap, hcunit.DeployedChokePoint ), new Rectangle( x, y + 3, 12, 9 ), true, hcunit.DeployedChokePoint.ID );
        x += 14;

        if( hcunit.HasLastMoved )
          g.DrawString( String.Format( Language.BrigadeStatus_DeployedAt2, hcunit.DeployedChokePoint, Misc.MinsAgoLong( hcunit.LastMovedTime ) ), fontMain, brushMain, x, y );
        else
          g.DrawString( hcunit.DeployedChokePoint.Name, fontMain, brushMain, x, y );

        stringWidth = (int)g.MeasureString( hcunit.DeployedChokePoint.Name, fontMain ).Width;
        imgmap.AddRectangle( String.Format( Language.Common_Tooltip_GoToStatus, hcunit.DeployedChokePoint ), new Rectangle( x + 1, y + 2, stringWidth, 10 ), true, hcunit.DeployedChokePoint );
      }
      else
      {
        if( hcunit.HasLastMoved )
          g.DrawString( String.Format( Language.BrigadeStatus_UndeployedFor, Misc.MinsAgoLong( hcunit.LastMovedTime ) ), fontMain, brushMain, xTab[1], y );
        else
          g.DrawString( Language.BrigadeStatus_Undeployed, fontMain, brushMain, xTab[1], y );
      }
      y += lineHeight;


      // last moved info

      if( hcunit.HasLastMoved )
        g.DrawString( String.Format( Language.BrigadeStatus_LastMoved, Misc.FormatDateLong( hcunit.LastMovedTime ), hcunit.LastMovedPlayer ), fontMain, brushMain, xTab[1], y );
      y += lineHeight;

      #endregion

      #region Command

      // draw command details

      g.DrawString( Language.BrigadeStatus_Section_Command, fontHead, brushHead, xTab[0], y );
      y += lineHeight;

      string toeName = hcunit.Toe != null ? hcunit.Toe.Name : "Unknown TOE";
      g.DrawString( toeName + ", " + hcunit.Nick, fontMain, brushMain, xTab[1], y );
      stringWidth = (int)g.MeasureString( toeName, fontMain ).Width;
      imgmap.AddRectangle( Language.Common_Tooltip_GoToEquipment, new Rectangle( xTab[1] + 1, y + 2, stringWidth, 10 ), true, hcunit.Toe );
      y += lineHeight;

      if( hcunit.Owner != "" )
      {
        g.DrawString( Language.BrigadeStatus_CommandingOfficer + " " + hcunit.Owner, fontMain, brushMain, xTab[1], y );
        y += lineHeight;
      }

      /* TODO
      g.DrawString( Language.BrigadeStatus_OfficerInCharge + " " + "(todo)", fontMain, brushMain, xTab[1], y );
      y += lineHeight;
      */

      #endregion

      #region Stacked with

      if( hcunit.IsDeployed && hcunit.DeployedChokePoint.DeployedHCUnits.Count > 1 )
      {
        // calc chokepoint column position

        BegmMisc.TextWidth twColTime = new BegmMisc.TextWidth( fontMain, 70, 230 );
        foreach( HCUnit stackHCUnit in hcunit.DeployedChokePoint.DeployedHCUnits )
        {
          if( stackHCUnit == hcunit ) continue;  // skip self
          twColTime.Measure( stackHCUnit.Level == HCUnitLevel.Division ? Language.Common_DivisionHQ : stackHCUnit.Title );
        }
        int xColTime = twColTime.MaxWidth + 100;


        // draw stacked with info

        g.DrawString( Language.BrigadeStatus_Section_StackedWith, fontHead, brushHead, xTab[0], y );

        HCUnit prevDivision = null;

        foreach( HCUnit stackHCUnit in hcunit.DeployedChokePoint.DeployedHCUnits )
        {
          if( stackHCUnit == hcunit ) continue;  // skip self

          y += lineHeight;

          HCUnit stackDivision = stackHCUnit.Level == HCUnitLevel.Division ? stackHCUnit : stackHCUnit.ParentUnit;
          HCUnit stackBrigade  = stackHCUnit.Level == HCUnitLevel.Brigade  ? stackHCUnit : stackHCUnit.GetDummyDivHQBrigade();

          if( stackDivision != prevDivision ) // draw division heading w/ tooltip
          {
            prevDivision = stackDivision;

            g.DrawImage( stackDivision.Country.BrigadeFlag, xTab[1] - 1, y + 2.5F, 17.57F, 9 ); // 41x21
            g.DrawString( stackDivision.Title, fontMain, brushMain, xTab[2], y );

            string divTooltip;
            if( stackDivision.IsDeployed )
              divTooltip = String.Format( Language.Common_Tooltip_DivisionDeployed, stackDivision.DeployedChokePoint );
            else
              divTooltip = Language.Common_Tooltip_DivisionNotDeployed;

            if( divTooltip != null )
            {
              stringWidth = (int)g.MeasureString( stackDivision.Title, fontMain, width - xTab[2] ).Width - 2;
              imgmap.AddRectangle( divTooltip, new Rectangle( xTab[2] + 1, y + 2, stringWidth, 10 ), true, stackDivision );
            }

            y += lineHeight;
          }


          // draw unit title, time w/ last moved tooltip

          g.FillEllipse( stackHCUnit.Country.Side == Side.Allied ? brushHCUnitAllied : brushHCUnitAxis, xTab[2] + 4, y + 4, 5, 5 );
          g.DrawString( stackBrigade.Title, fontMain, brushMain, xTab[3], y );
          if( stackHCUnit.HasLastMoved )
            g.DrawString( Misc.MinsAgoShort( stackHCUnit.LastMovedTime ), fontMain, brushMain, xColTime, y, alignRight );

          string brigTooltip = stackHCUnit.Tooltip;
          if( brigTooltip != null )
          {
            stringWidth = (int)g.MeasureString( stackBrigade.Title, fontMain, width - xTab[3] ).Width - 2;
            imgmap.AddRectangle( brigTooltip, new Rectangle( xTab[3] + 1, y + 2, stringWidth, 10 ), true, stackHCUnit );
          }

        }  // end foreach stackHCUnit

        y += lineHeight;
      }

      #endregion

      g.Dispose();
      return bitmap;
    }

    /// <summary>
    /// Generates the simple tree branch image used by picTree*.
    /// </summary>
    /// <returns>The image.</returns>
    private Image GenerateTreeImage()
    {
      Bitmap bitmap = new Bitmap( 10, 10 );
      Graphics g = Graphics.FromImage( bitmap );
      Pen penLine = new Pen( Color.FromArgb( 49, 72, 60 ) );
      penLine.DashStyle = DashStyle.Dot;

      const int x = 4;
      const int y = 5;

      g.DrawLine( penLine, x, 0, x, y );
      g.DrawLine( penLine, x, y, 10, y );

      g.Dispose();
      return bitmap;
    }

    /// <summary>
    /// Updates the contents of the move history data grid view for the currently selected hcunit.
    /// </summary>
    /// <param name="preserveScroll">Whether the current scroll position should be preserved.</param>
    private void UpdateMoveHistory( bool preserveScroll )
    {
      if( this.selectedHCUnit == null ) return;


      // get original scroll position

      int scrollPosition = 0;
      if( preserveScroll )
        scrollPosition = dgvMoves.FirstDisplayedScrollingRowIndex;


      // reset datagridview

      pnlMoves.Visible = true;
      dgvMoves.Rows.Clear();


      // loop over move event list (forwards or backwards)

      for( int i = ( this.eventSortOrder == SortOrder.Ascending ? 0 : this.selectedHCUnit.Moves.Count - 1 );
           i >= 0 && i < this.selectedHCUnit.Moves.Count;
           i += ( this.eventSortOrder == SortOrder.Ascending ? 1 : -1 ) )
      {
        HCUnitMove move = this.selectedHCUnit.Moves[i];


        // only show success & pending if checkbox checked

        if( this.cbMovesHideFailed.Checked && ( move.State != HCUnitMoveState.Success && move.State != HCUnitMoveState.Pending ) ) continue;


        // get image to display

        Image result;
        if( move.Player == "SYSTEM" )
          result = new Bitmap( 1, 1 );  // blank image
        else if( move.State == HCUnitMoveState.Success )
          result = Resources.tick;
        else if( move.State == HCUnitMoveState.Pending )
          result = Resources.movepending;
        else
          result = Resources.cross;


        // add row

        object to = move.To ?? (object)"(unknown)";
        string player = move.Player;
        if( player == "SYSTEM" ) player = null;

        int rowid = dgvMoves.Rows.Add( Misc.Timestamp( move.Time ), player, move.From, Resources.arrow_right, to, null, result );


        // set row color

        if( move.Player == "SYSTEM" )
          dgvMoves.Rows[rowid].DefaultCellStyle.ForeColor = dgvMoves.Rows[rowid].DefaultCellStyle.SelectionForeColor = Color.FromArgb( 128, 128, 128 );
        else
          dgvMoves.Rows[rowid].DefaultCellStyle.ForeColor = dgvMoves.Rows[rowid].DefaultCellStyle.SelectionForeColor = dgvMoves.DefaultCellStyle.ForeColor;


        // set image tooltip

        string resultTooltip;
        if( move.Player == "SYSTEM" )
          resultTooltip = null;
        else if( move.State == HCUnitMoveState.Success || move.State == HCUnitMoveState.Pending || move.State == HCUnitMoveState.Cancelled )
          resultTooltip = Misc.EnumString( move.State );
        else if( move.State == HCUnitMoveState.RequestFailed )
          resultTooltip = move.RequestResult.Description;
        else if( move.State == HCUnitMoveState.CompletionFailed )
          resultTooltip = move.CompletedResult.Description;
        else
          resultTooltip = null;

        dgvMoves[6, rowid].ToolTipText = resultTooltip;

      }  // foreach move


      // set visiblity

      pnlMoves.Visible = dgvMoves.Rows.Count > 0;

      if( pnlMoves.Visible )
      {
        // set height

        int rows = dgvMoves.Rows.Count;
        if( rows > 5 )
          rows = 5;
        dgvMoves.Height = rows * dgvMoves.RowTemplate.Height;
        pnlMoves.Height = dgvMoves.Bottom;


        // autosize and reduce column width if necessary

        dgvMoves.AutoResizeColumns( DataGridViewAutoSizeColumnsMode.AllCells );

        int totalColumnWidth = 0;
        foreach( DataGridViewColumn column in dgvMoves.Columns )
          totalColumnWidth += column.Width;

        if( totalColumnWidth > dgvMoves.DisplayRectangle.Width )
        {
          int diff = totalColumnWidth - dgvMoves.DisplayRectangle.Width;
          dgvMoves.Columns[2].Width -= diff / 2;
          dgvMoves.Columns[4].Width -= diff / 2;
        }


        // restore scroll position

        if( preserveScroll )
        {
          if( scrollPosition > dgvMoves.RowCount - 1 )
            scrollPosition = dgvMoves.RowCount - 1;
        }
        else
        {
          scrollPosition = this.eventSortOrder == SortOrder.Ascending ? dgvMoves.RowCount - 1 : 0;  // bottom/top
        }

        if( scrollPosition >= 0 && scrollPosition < dgvMoves.RowCount )
          dgvMoves.FirstDisplayedScrollingRowIndex = scrollPosition;
      }
    }

    /// <summary>
    /// Resizes the widget and various controls to fit the current contents.
    /// </summary>
    private void UpdateWidgetHeight()
    {
      Expando expBrigadeStatus = (Expando)this.Parent;

      int newHeight = 23 + picHCUnitInfo.Top + picHCUnitInfo.Height;
      if( pnlMoves.Visible )
        newHeight += pnlMoves.Height + 2;

      if( expBrigadeStatus.ExpandedHeight != newHeight )
      {
        expBrigadeStatus.SuspendLayout();
        expBrigadeStatus.ExpandedHeight = newHeight;
        expBrigadeStatus.Height += 1;  // workaround for bug in tskMain ScrollableControl:
        expBrigadeStatus.Height -= 1;  // force the correct scroll height
        pnlMoves.Top = picHCUnitInfo.Bottom - 2;
        expBrigadeStatus.ResumeLayout();
      }
    }

    #endregion

    #region Event Handlers

    // mouseover effects
    private void dgvHCUnits_CellMouseEnter( object sender, DataGridViewCellEventArgs e )
    {
      if( dgvHCUnits.RowCount == 0 ) return;

      int iCurrent = ( e.ColumnIndex * dgvHCUnits.RowCount ) + ( e.RowIndex % dgvHCUnits.RowCount );


      // if holding mouse button down, select cell

      if( Control.MouseButtons == MouseButtons.Left )
      {
        dgvHCUnits[e.ColumnIndex, e.RowIndex].Selected = true;
        lblActiveTitle.Text = null;
      }


      // store new active unit, remember prev unit

      int iPrev = this.iActive;
      if( iCurrent >= this.hcunits.Count )
      {
        this.iActive = -1;
        lblActiveTitle.Text = null;
      }
      else
      {
        this.iActive = iCurrent;
        if( Control.MouseButtons != MouseButtons.Left )
          lblActiveTitle.Text = this.hcunits[iCurrent].Title;
      }


      // invalidate cells

      dgvHCUnits.InvalidateCell( e.ColumnIndex, e.RowIndex );
      dgvHCUnits.InvalidateCell( iPrev / dgvHCUnits.RowCount,
                                 iPrev % dgvHCUnits.RowCount );

      if( iPrev < 0 && iActive < 0 )
        return;  // still no selection
      if( iPrev >= 0 && iActive >= 0 && this.hcunits[iPrev].Country == this.hcunits[iActive].Country && this.hcunits[iPrev].Branch == this.hcunits[iActive].Branch )
        return;  // same country/branch

      if( iPrev >= 0 )  // old cells
      {
        int iStart = iPrev;
        while( iStart > 0 && this.hcunits[iStart - 1].Country == this.hcunits[iPrev].Country && this.hcunits[iStart - 1].Branch == this.hcunits[iPrev].Branch )
          iStart--;

        while( iStart < this.hcunits.Count && this.hcunits[iStart].Country == this.hcunits[iPrev].Country && this.hcunits[iStart].Branch == this.hcunits[iPrev].Branch )
        {
          dgvHCUnits.InvalidateCell( iStart / dgvHCUnits.RowCount,
                                     iStart % dgvHCUnits.RowCount );
          iStart++;
        }
      }

      if( this.iActive >= 0 )  // new cells
      {
        int iStart = iCurrent;
        while( iStart > 0 && this.hcunits[iStart - 1].Country == this.hcunits[iActive].Country && this.hcunits[iStart - 1].Branch == this.hcunits[iActive].Branch )
          iStart--;

        while( iStart < this.hcunits.Count && this.hcunits[iStart].Country == this.hcunits[iActive].Country && this.hcunits[iStart].Branch == this.hcunits[iActive].Branch )
        {
          dgvHCUnits.InvalidateCell( iStart / dgvHCUnits.RowCount,
                                     iStart % dgvHCUnits.RowCount );
          iStart++;
        }
      }
    }
    private void dgvHCUnits_CellPainting( object sender, DataGridViewCellPaintingEventArgs e )
    {
      e.Graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

      int iCurrent = ( e.ColumnIndex * dgvHCUnits.RowCount ) + ( e.RowIndex % dgvHCUnits.RowCount );

      if( iCurrent < this.hcunits.Count )
      {
        // set forecolor based on country

        if( iCurrent == iActive )  // mouseover
        {
          if( this.hcunits[iCurrent].Country.Abbr == "UK" )
            e.CellStyle.ForeColor = e.CellStyle.SelectionForeColor = Color.FromArgb( 109, 152, 230 );  // 173, 216, 230
          else if( this.hcunits[iCurrent].Country.Abbr == "US" )
            e.CellStyle.ForeColor = e.CellStyle.SelectionForeColor = Color.FromArgb( 180, 180, 180 );
          else if( this.hcunits[iCurrent].Country.Abbr == "FR" )
            e.CellStyle.ForeColor = e.CellStyle.SelectionForeColor = Color.White;
          else if( this.hcunits[iCurrent].Country.Abbr == "DE" )
            e.CellStyle.ForeColor = e.CellStyle.SelectionForeColor = Color.FromArgb( 255, 118, 129 );  // 255, 182, 193
        }
        else
        {
          if( this.hcunits[iCurrent].Country.Abbr == "UK" )
            e.CellStyle.ForeColor = e.CellStyle.SelectionForeColor = Color.LightBlue;
          else if( this.hcunits[iCurrent].Country.Abbr == "US" )
            e.CellStyle.ForeColor = e.CellStyle.SelectionForeColor = Color.White;
          else if( this.hcunits[iCurrent].Country.Abbr == "FR" )
            e.CellStyle.ForeColor = e.CellStyle.SelectionForeColor = Color.FromArgb( 180, 180, 180 );
          else if( this.hcunits[iCurrent].Country.Abbr == "DE" )
            e.CellStyle.ForeColor = e.CellStyle.SelectionForeColor = Color.LightPink;
        }


        // set backcolor based on iActive (mouseover section)

        if( this.iActive >= 0 && this.hcunits[iCurrent].Country == this.hcunits[iActive].Country
            && this.hcunits[iCurrent].Branch == this.hcunits[iActive].Branch )
          e.CellStyle.BackColor = Color.FromArgb( 32, 32, 32 );
      }


      // draw cell

      e.Paint( e.CellBounds, e.PaintParts );


      // draw bottom line, seperators between countries / branches

      Pen penLine = new Pen( Color.FromArgb( 49, 72, 60 ) );

      if( e.RowIndex == dgvHCUnits.RowCount - 1 )  // last row
      {
        e.Graphics.DrawLine( penLine, e.CellBounds.Left, e.CellBounds.Bottom, e.CellBounds.Right, e.CellBounds.Bottom );
      }
      else if( iCurrent > 0 && iCurrent <= this.hcunits.Count )  // line under section
      {
        if( iCurrent == this.hcunits.Count
            || this.hcunits[iCurrent].Country != this.hcunits[iCurrent - 1].Country
            || this.hcunits[iCurrent].Branch != this.hcunits[iCurrent - 1].Branch )
          e.Graphics.DrawLine( penLine, e.CellBounds.Left, e.CellBounds.Top, e.CellBounds.Right, e.CellBounds.Top );
      }


      e.Handled = true;
    }

    // display info on selection
    private void dgvHCUnits_SelectionChanged( object sender, EventArgs e )
    {
      if( this.Parent == null ) return;  // expando animating

      int iSelected = -1;

      if( dgvHCUnits.SelectedCells.Count != 0 )
      {
        iSelected = ( dgvHCUnits.SelectedCells[0].ColumnIndex * dgvHCUnits.RowCount )
                  + ( dgvHCUnits.SelectedCells[0].RowIndex % dgvHCUnits.RowCount );
      }

      if( iSelected >= 0 && iSelected < this.hcunits.Count )
        DisplayHCUnitInfo( this.hcunits[iSelected] );
      else
        DisplayHCUnitInfo( null );
    }

    // changing selection via up/down keys wrap columns
    private void dgvHCUnits_KeyDown( object sender, KeyEventArgs e )
    {
      if( e.KeyCode == Keys.Down
          && dgvHCUnits.SelectedCells[0].RowIndex == dgvHCUnits.RowCount - 1  // bottom row
          && dgvHCUnits.SelectedCells[0].ColumnIndex < dgvHCUnits.ColumnCount - 1 )  // not last column
      {
        dgvHCUnits[dgvHCUnits.SelectedCells[0].ColumnIndex + 1, 0].Selected = true;
        e.Handled = true;
      }
      else if( e.KeyCode == Keys.Up
               && dgvHCUnits.SelectedCells[0].RowIndex == 0  // top row
               && dgvHCUnits.SelectedCells[0].ColumnIndex > 0 )  // not first column
      {
        dgvHCUnits[dgvHCUnits.SelectedCells[0].ColumnIndex - 1, dgvHCUnits.RowCount - 1].Selected = true;
        e.Handled = true;
      }
    }

    // tree links to orbat
    private void lnkTreeBranch_LinkClicked( object sender, LinkLabelLinkClickedEventArgs e )
    {
      if( this.selectedHCUnit == null ) return;

      if( this.selectedHCUnit.Level == HCUnitLevel.Division )
        GameStatus_RevealWidget( WidgetType.OrderOfBattle, this.selectedHCUnit.ParentUnit.ParentUnit );
      else
        GameStatus_RevealWidget( WidgetType.OrderOfBattle, this.selectedHCUnit.ParentUnit.ParentUnit.ParentUnit );
    }
    private void lnkTreeCorps_LinkClicked( object sender, LinkLabelLinkClickedEventArgs e )
    {
      if( this.selectedHCUnit == null ) return;

      if( this.selectedHCUnit.Level == HCUnitLevel.Division )
        GameStatus_RevealWidget( WidgetType.OrderOfBattle, this.selectedHCUnit.ParentUnit );
      else
        GameStatus_RevealWidget( WidgetType.OrderOfBattle, this.selectedHCUnit.ParentUnit.ParentUnit );
    }
    private void lnkTreeDivision_LinkClicked( object sender, LinkLabelLinkClickedEventArgs e )
    {
      if( this.selectedHCUnit == null ) return;

      if( this.selectedHCUnit.Level == HCUnitLevel.Division )
        GameStatus_RevealWidget( WidgetType.OrderOfBattle, this.selectedHCUnit );
      else
        GameStatus_RevealWidget( WidgetType.OrderOfBattle, this.selectedHCUnit.ParentUnit );
    }
    private void lnkTreeBrigade_LinkClicked( object sender, LinkLabelLinkClickedEventArgs e )
    {
      if( this.selectedHCUnit == null ) return;

      GameStatus_RevealWidget( WidgetType.OrderOfBattle, this.selectedHCUnit );
    }

    // Reveal() timer
    private void tmrReveal_Tick( object sender, EventArgs e )
    {
      Expando expBrigadeStatus = (Expando)this.Parent;

      if( expBrigadeStatus.Animating )
        return;  // wait until finished animating

      if( expBrigadeStatus.TaskPane != null )
        expBrigadeStatus.TaskPane.ScrollControlIntoView( expBrigadeStatus );
      tmrReveal.Stop();
    }

    // dynamic focus
    private void BrigadeStatus_MouseEnter( object sender, EventArgs e )
    {
      BegmMisc.FocusTaskPane( this );
    }
    private void dgvHCUnits_MouseEnter( object sender, EventArgs e )
    {
      BegmMisc.FocusWithoutScroll( dgvHCUnits );
    }
    private void dgvHCUnits_MouseLeave( object sender, EventArgs e )
    {
      // remove mouseover effect
      dgvHCUnits_CellMouseEnter( null, new DataGridViewCellEventArgs( dgvHCUnits.ColumnCount - 1, dgvHCUnits.RowCount - 1 ) );

      BegmMisc.FocusTaskPane( this );
    }
    private void dgvMoves_MouseEnter( object sender, EventArgs e )
    {
      BegmMisc.FocusWithoutScroll( dgvMoves );
    }
    private void dgvMoves_MouseLeave( object sender, EventArgs e )
    {
      BegmMisc.FocusTaskPane( this );
    }

    // moves result image tooltip
    private void dgvMoves_CellMouseEnter( object sender, DataGridViewCellEventArgs e )
    {
      if( e.ColumnIndex != 6 )
      {
        GameStatus.ToolTip.Hide( dgvMoves );
        return;
      }

      string text = dgvMoves[6, e.RowIndex].ToolTipText;
      Size size = TextRenderer.MeasureText( text, SystemFonts.StatusFont );

      int x = dgvMoves.DisplayRectangle.Width - size.Width - 20;
      int y = ( e.RowIndex - dgvMoves.FirstDisplayedScrollingRowIndex ) * dgvMoves.RowTemplate.Height;

      GameStatus.ToolTip.Show( text, dgvMoves, x, y );  // align right
    }
    private void dgvMoves_CellMouseLeave( object sender, DataGridViewCellEventArgs e )
    {
      GameStatus.ToolTip.Hide( dgvMoves );
    }

    // handle imgmap links
    private void picHCUnitInfo_RegionClick( int index, object tag, MouseButtons button )
    {
      if( button != MouseButtons.Left )
        return;

      if( tag is HCUnit )
        Reveal( (HCUnit)tag );
      else if( tag is ChokePoint )
        GameStatus_RevealWidget( WidgetType.TownStatus, tag );
      else if( tag is int )
        GameStatus_RevealWidget( WidgetType.GameMap, tag );
      else if( tag is Toe )
        GameStatus_RevealWidget( WidgetType.Equipment, tag );
    }

    // update move history
    private void cbMovesHideFailed_Click( object sender, EventArgs e )
    {
      UpdateMoveHistory( false );
      UpdateWidgetHeight();
    }

    #endregion

    #region Classes

    /// <summary>
    /// An object that represents a row in HCUnit selection grid data source.
    /// </summary>
    public class HCUnitRow
    {
      #region Variables

      private List<HCUnit> hcunits;

      #endregion

      #region Constructors

      /// <summary>
      /// Create a new HCUnitRow with the given HCUnits.
      /// </summary>
      /// <param name="hcunits">The HCUnits in the row.</param>
      public HCUnitRow( params HCUnit[] hcunits )
      {
        this.hcunits = new List<HCUnit>( hcunits );
      }

      #endregion

      #region Properties

      /// <summary>
      /// Get the value for column 0.
      /// </summary>
      public string Column0
      {
        get { return GetColValue( 0 ); }
      }

      /// <summary>
      /// Get the value for column 1.
      /// </summary>
      public string Column1
      {
        get { return GetColValue( 1 ); }
      }

      /// <summary>
      /// Get the value for column 2.
      /// </summary>
      public string Column2
      {
        get { return GetColValue( 2 ); }
      }

      /// <summary>
      /// Get the value for column 3.
      /// </summary>
      public string Column3
      {
        get { return GetColValue( 3 ); }
      }

      /// <summary>
      /// Get the value for column 4.
      /// </summary>
      public string Column4
      {
        get { return GetColValue( 4 ); }
      }

      /// <summary>
      /// Get the value for column 5.
      /// </summary>
      public string Column5
      {
        get { return GetColValue( 5 ); }
      }

      /// <summary>
      /// Get the value for column 6.
      /// </summary>
      public string Column6
      {
        get { return GetColValue( 6 ); }
      }

      #endregion

      #region Methods

      /// <summary>
      /// Gets the string value to display in a specific column of this row.
      /// </summary>
      /// <param name="i">The column index.</param>
      /// <returns>The HCUnits shortname, indented if a brigade.</returns>
      private string GetColValue( int i )
      {
        if( this.hcunits[i] == null ) return null;

        if( this.hcunits[i].Level == HCUnitLevel.Division )
          return this.hcunits[i].ShortName;
        else
          return "   " + this.hcunits[i].ShortName;
      }

      #endregion
    }

    #endregion
  }
}
