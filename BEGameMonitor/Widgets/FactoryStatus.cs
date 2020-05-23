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
using System.Text.RegularExpressions;
using System.Windows.Forms;
using XLib.Extensions;
using XPExplorerBar;
using Xiperware.WiretapAPI;

namespace BEGM.Widgets
{
  /// <summary>
  /// The FactoryStatus widget displays summary and individual information about all the factories
  /// in the game. This includes daily &amp; campaign production output, damage &amp; rdp graphs for the
  /// past 24 hours, current health &amp; rdp state, and an allied/axis summary of these.
  /// </summary>
  public partial class FactoryStatus : UserControl, IWidget
  {
    #region Variables

    /// <summary>
    /// The current game state.
    /// </summary>
    private GameState game;

    /// <summary>
    /// References to each country to display.
    /// </summary>
    Country british, french, german;

    /// <summary>
    /// GameStatus's LoadFactoryData().
    /// </summary>
    /// <remarks>Used when the "Get Factory Data" button is pressed.</remarks>
    public Action GameStatus_LoadFactoryData;

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
    /// Create a new FactoryStatus widget.
    /// </summary>
    public FactoryStatus()
    {
      InitializeComponent();


      // VS designer bug

      tskFactory.CustomSettings.Padding = new XPExplorerBar.Padding( 0, 0, 0, 4 );


      cbAlwaysLoad.Left = ( ( cbAlwaysLoad.Parent.Width - cbAlwaysLoad.Width ) / 2 );
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
    internal FactoryStatusState State
    {
      get
      {
        FactoryStatusState state = new FactoryStatusState {
          britishExpanded = ( !expBritish.Collapsed ),
          frenchExpanded  = ( !expFrench.Collapsed  ),
          germanExpanded  = ( !expGerman.Collapsed  )
        };

        return state;
      }
      set
      {
        if( value.britishExpanded ) expBritish.Expand(); else expBritish.Collapse();
        if( value.frenchExpanded  ) expFrench.Expand();  else expFrench.Collapse();
        if( value.germanExpanded  ) expGerman.Expand();  else expGerman.Collapse();
      }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Initialise the FactoryStatus widget.
    /// </summary>
    /// <param name="game">The current game state.</param>
    public void InitWidget( GameState game )
    {
      if( !game.Wiretap.IsSubscribed( Livedata.Factory ) )
        return;  // skip, factory data not loaded

      Log.AddEntry( "  FactoryStatus..." );

      this.game = game;

      this.british = game.Countries[1];
      this.french = game.Countries[3];
      this.german = game.Countries[4];


      // create initial stat pages

      GenerateAll();


      Log.Okay();
    }

    /// <summary>
    /// Update the factory stats images with the latest data.
    /// </summary>
    public void UpdateWidget()
    {
      if( this.game == null || !game.Wiretap.NewFactoryData )
        return;  // skip, no new data (or not loaded)

      GenerateAll();
    }

    /// <summary>
    /// Remove references to local game objects.
    /// </summary>
    public void Cleanup()
    {
      this.game = null;
    }

    /// <summary>
    /// Makes a country within the Factory Status widget visible by making sure both
    /// the widget and country expandos are expanded, scrolling it into view and flashing
    /// the country title.
    /// </summary>
    /// <seealso cref="tmrReveal_Tick"/>
    /// <param name="arg">The Factory to make visible.</param>
    public void Reveal( object arg )
    {
      Factory factory = arg as Factory;
      if( factory == null ) return;

      if( tmrReveal.Enabled )
        return;  // another Reveal() in progress

      Expando expFactoryStatus = (Expando)this.Parent;
      Expando expCountry;

      switch( factory.Country.Abbr )
      {
        case "UK": expCountry = expBritish; break;
        case "FR": expCountry = expFrench; break;
        case "DE": expCountry = expGerman; break;
        default: return;
      }

      if( expFactoryStatus.Collapsed )
      {
        expCountry.Expand();  // no animation
        expFactoryStatus.Collapsed = false;
      }
      if( expCountry.Collapsed )
        expCountry.Collapsed = false;  // animate

      tmrReveal.Tag = expCountry;
      tmrReveal.Start();
    }



    /// <summary>
    /// Specify whether the "Load Factory Data" message should be shown.
    /// </summary>
    /// <param name="show">True/False</param>
    public void ShowLoadMessage( bool show )
    {
      if( show )
      {
        this.Controls.Remove( tskFactory );  // must be removed in order to set ExpandedHeight
        this.Controls.Remove( picSummary );
        this.Height = 100;
        ( (Expando)this.Parent ).ExpandedHeight = this.Height + 25;
      }
      else
      {
        this.Controls.Remove( pnlLoadFactoryData );
        if( !this.Controls.Contains( tskFactory ) )
          this.Controls.Add( tskFactory );
        if( !this.Controls.Contains( picSummary ) )
          this.Controls.Add( picSummary );
        if( expBritish.Collapsed && expFrench.Collapsed && expGerman.Collapsed )
        {
          this.Height = 184;
          ( (Expando)this.Parent ).ExpandedHeight = this.Height + 25;
        }
      }
    }

    /// <summary>
    /// Generate all summary and country images.
    /// </summary>
    private void GenerateAll()
    {
      picSummary.Image = GenerateSummary();

      picBritish.Image = GenerateImage( picBritish, british );
      picFrench.Image = GenerateImage( picFrench, french );
      picGerman.Image = GenerateImage( picGerman, german );

      expBritish.ExpandedHeight = picBritish.Image.Height + expBritish.HeaderHeight + 1;
      expFrench.ExpandedHeight = picFrench.Image.Height + expFrench.HeaderHeight + 1;
      expGerman.ExpandedHeight = picGerman.Image.Height + expGerman.HeaderHeight + 1;

      ( (Expando)this.Parent ).CalcAnimationHeights();
    }

    /// <summary>
    /// Generate a summary image showing Allied/Axis factory output/damage, and
    /// total/campaign performance.
    /// </summary>
    /// <returns>The image to display.</returns>
    private Image GenerateSummary()
    {
      // calc stats

      int numFactoriesAlliedAll = 0, numFactoriesAxisAll = 0;  // factories originally owned by side
      int numFactoriesAlliedOwn = 0, numFactoriesAxisOwn = 0;  // factories currently owned by side (ie, not captured)
      int currentOutputAllied   = 0, currentOutputAxis   = 0;  // percent of all
      int currentHealthAllied   = 0, currentHealthAxis   = 0;  // percent of own
      int dailyOutputAllied     = 0, dailyOutputAxis     = 0;  // percent of all
      int campaignOutputAllied  = 0, campaignOutputAxis  = 0;  // percent of all

      foreach( Factory factory in game.Factories.Values )
      {
        if( factory.Country.Side == Side.Allied )
        {
          numFactoriesAlliedAll++;
          dailyOutputAllied    += factory.OutputPercentPastDay;
          campaignOutputAllied += factory.OutputPercentCampaign;
          if( factory.CurrentRDP )
            currentOutputAllied++;

          if( factory.Owner.Side == Side.Allied )  // owned by orig side
          {
            numFactoriesAlliedOwn++;
            currentHealthAllied += factory.CurrentHealth;
          }
        }
        else if( factory.Country.Side == Side.Axis )
        {
          numFactoriesAxisAll++;
          dailyOutputAxis    += factory.OutputPercentPastDay;
          campaignOutputAxis += factory.OutputPercentCampaign;
          if( factory.CurrentRDP )
            currentOutputAxis++;

          if( factory.Owner.Side == Side.Axis )  // owned by orig side
          {
            numFactoriesAxisOwn++;
            currentHealthAxis += factory.CurrentHealth;
          }
        }
      }

      if( numFactoriesAlliedAll != 0 )
      {
        currentOutputAllied   = (int)( ( (float)currentOutputAllied  / (float)numFactoriesAlliedAll ) * 100 );
        dailyOutputAllied     = (int)(   (float)dailyOutputAllied    / (float)numFactoriesAlliedAll );
        campaignOutputAllied  = (int)(   (float)campaignOutputAllied / (float)numFactoriesAlliedAll );

        if( numFactoriesAlliedOwn != 0 )
          currentHealthAllied = (int)( (float)currentHealthAllied / (float)numFactoriesAlliedOwn );
      }

      if( numFactoriesAxisAll != 0 )
      {
        currentOutputAxis   = (int)( ( (float)currentOutputAxis  / (float)numFactoriesAxisAll ) * 100 );
        dailyOutputAxis     = (int)(   (float)dailyOutputAxis    / (float)numFactoriesAxisAll );
        campaignOutputAxis  = (int)(   (float)campaignOutputAxis / (float)numFactoriesAxisAll );

        if( numFactoriesAxisOwn != 0 )
          currentHealthAxis = (int)( (float)currentHealthAxis / (float)numFactoriesAxisOwn );
      }

      float[] avgRdpAllied    = new float[96];
      float[] avgRdpAxis      = new float[96];
      float[] avgDamageAllied = new float[96];
      float[] avgDamageAxis   = new float[96];

      for( int i = 0; i < 96; i++ )
      {
        float rdpAllied = 0, rdpAxis = 0;
        float damageAllied = 0, damageAxis = 0;

        foreach( Factory factory in game.Factories.Values )
        {
          if( i >= factory.Ticks.Count ) break;

          if( factory.Country.Side == Side.Allied )
          {
            rdpAllied += factory.Ticks[i].RDP;
            if( factory.Owner.Side == Side.Allied )
              damageAllied += factory.Ticks[i].Damage;
          }
          else if( factory.Country.Side == Side.Axis )
          {
            rdpAxis += factory.Ticks[i].RDP;
            if( factory.Owner.Side == Side.Axis )
              damageAxis += factory.Ticks[i].Damage;
          }
        }

        if( numFactoriesAlliedAll != 0 ) avgRdpAllied[i]    = rdpAllied    / numFactoriesAlliedAll;
        if( numFactoriesAxisAll   != 0 ) avgRdpAxis[i]      = rdpAxis      / numFactoriesAxisAll;
        if( numFactoriesAlliedOwn != 0 ) avgDamageAllied[i] = damageAllied / numFactoriesAlliedOwn;
        if( numFactoriesAxisOwn   != 0 ) avgDamageAxis[i]   = damageAxis   / numFactoriesAxisOwn;
      }


      // create bitmap, graphics objects

      Bitmap bitmap = new Bitmap( 336, 115 );  // picSummary dimensions

      Graphics g = Graphics.FromImage( bitmap );
      g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
      g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;


      // create graphics resources

      Brush brushHead = new SolidBrush( Color.FromArgb( 224, 224, 224 ) );
      Brush brushDark = new SolidBrush( Color.FromArgb( 140, 140, 140 ) );

      Font fontTitle = new Font( "Tahoma", 10, FontStyle.Bold );
      Font fontOutput = new Font( "Tahoma", 25, FontStyle.Bold );
      Font fontPercent = new Font( "Tahoma", 13 );
      Font fontMain = new Font( "Tahoma", 8.25F );

      StringFormat alignCenter = new StringFormat { Alignment = StringAlignment.Center };
      StringFormat alignRight = new StringFormat { Alignment = StringAlignment.Far };

      Pen penHealth = new Pen( Color.FromArgb( 224, 224, 224 ) );
      Pen penDark = new Pen( Color.FromArgb( 128, 128, 128 ) );
      Pen penGrid = new Pen( Color.FromArgb( 64, 64, 64 ) );
      Brush brushRDP = new SolidBrush( Color.FromArgb( 48, 48, 48 ) );


      // headings

      string allied = Misc.EnumString( Side.Allied );
      if( Program.uiCulture.TwoLetterISOLanguageName == "de" )  // use "Allied" version #2 for de
        allied = Misc.EnumString( Side.Allied )[2].ToTitleCase();
      g.DrawString( allied, fontTitle, brushHead, 85, 3, alignCenter );
      g.DrawString( Misc.EnumString( Side.Axis ), fontTitle, brushHead, 250, 3, alignCenter );
      

      // stats

      g.DrawString( currentOutputAllied.ToString(), fontOutput, brushHead, 85, 15, alignRight );
      g.DrawString( currentOutputAxis.ToString(), fontOutput, brushHead, 250, 15, alignRight );

      g.DrawString( "%", fontPercent, brushHead, 85, 31 );
      g.DrawString( "%", fontPercent, brushHead, 250, 31 );

      g.DrawString( Language.FactoryStatus_Output, fontMain, brushHead, 158, 38, alignRight );
      g.DrawString( Language.FactoryStatus_Output, fontMain, brushHead, 325, 38, alignRight );

      g.DrawString( String.Format( Language.FactoryStatus_Health, currentHealthAllied ), fontMain, brushDark, 158, 20, alignRight );
      g.DrawString( String.Format( Language.FactoryStatus_Health, currentHealthAxis ), fontMain, brushDark, 325, 20, alignRight );

      g.DrawString( String.Format( Language.FactoryStatus_OutputPercent, dailyOutputAllied, campaignOutputAllied ), fontMain, brushDark, 85, 55, alignCenter );
      g.DrawString( String.Format( Language.FactoryStatus_OutputPercent, dailyOutputAxis, campaignOutputAxis ), fontMain, brushDark, 250, 55, alignCenter );


      // allied/axis summary graphs

      const float pixlesPerTick = 154F / 96F;


      // rdp bar graph in background

      for( int i = 0; i < 96; i++ )
      {
        g.FillRectangle( brushRDP, 8F + ( i * pixlesPerTick ), 105 - ( avgRdpAllied[i] * 30F ), 2, avgRdpAllied[i] * 30F );
        g.FillRectangle( brushRDP, 172F + ( i * pixlesPerTick ), 105 - ( avgRdpAxis[i] * 30F ), 2, avgRdpAxis[i] * 30F );
      }


      // frame and gridlines

      g.DrawLine( penGrid, 8, 90, 162, 90 );
      g.DrawLine( penGrid, 172, 90, 326, 90 );

      const float xInterval = 154F / 12F;

      for( float x1 = 8 + xInterval - 1, x2 = 172 + xInterval - 1; x1 < 8 + 154; x1 += xInterval, x2 += xInterval )
      {
        g.DrawLine( penGrid, x1, 75, x1, 105 );
        g.DrawLine( penGrid, x2, 75, x2, 105 );
      }

      g.DrawRectangle( penDark, 8, 75, 154, 30 );
      g.DrawRectangle( penDark, 172, 75, 154, 30 );


      // health line graph in foreground

      for( int i = 1; i < 96; i++ )
      {
        PointF pt1Allied = new PointF( 9F + ( (i-1) * pixlesPerTick ), 76 + ( avgDamageAllied[i-1] * 0.29F ) );
        PointF pt1Axis = new PointF( 173F + ( (i-1) * pixlesPerTick ), 76 + ( avgDamageAxis[i-1] * 0.29F ) );

        PointF pt2Allied = new PointF( 9F + ( i * pixlesPerTick ), 76 + ( avgDamageAllied[i] * 0.29F ) );
        PointF pt2Axis = new PointF( 173F + ( i * pixlesPerTick ), 76 + ( avgDamageAxis[i] * 0.29F ) );

        g.DrawLine( penHealth, pt1Allied, pt2Allied );
        g.DrawLine( penHealth, pt1Axis, pt2Axis );
      }


      g.Dispose();

      return bitmap;
    }

    /// <summary>
    /// Generate a stats image for the given Country, showing production
    /// output percentages and damage/output graphs.
    /// </summary>
    /// <param name="imgmap">An ImageMap on which to add tooltips.</param>
    /// <param name="country">The reference Country.</param>
    /// <returns>The image to display.</returns>
    private Image GenerateImage( ImageMap.ImageMap imgmap, Country country )
    {
      // get sorted list of factories to display

      List<Factory> countryFactorys = new List<Factory>();

      foreach( Factory factory in game.Factories.Values )
        if( factory.Country == country )
          countryFactorys.Add( factory );

      countryFactorys.Sort();


      // determine wether to show RDP info

      bool showRDP = true;
      if( country.RDPGoal <= 0 )
      {
        showRDP = false;
      }
      else
      {
        foreach( Factory factory in countryFactorys )
          if( factory.TotalProduced == 0 || factory.Ticks.Count == 0 )
            showRDP = false;
      }


      // create bitmap, graphics objects

      const int width = 334;
      int height = 25 * countryFactorys.Count;
      if( showRDP ) height += 20;

      Bitmap bitmap = new Bitmap( width, height );
      Graphics g = Graphics.FromImage( bitmap );
      g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

      int y = -22;

      imgmap.RemoveAll();  // remove previous tooltips


      // create graphics resources

      Brush brushHead = new SolidBrush( Color.FromArgb( 224, 224, 224 ) );
      Brush brushMain = new SolidBrush( Color.FromArgb( 192, 192, 192 ) );

      Brush brushPieLight = Brushes.CornflowerBlue;
      Brush brushPieDark = Brushes.MediumBlue;

      Font fontMain = new Font( "Tahoma", 8.25F );
      Font fontHead = new Font( "Tahoma", 8.25F, FontStyle.Bold );
      Font fontRDP = new Font( "Tahoma", 7.5F );

      Pen penLine = new Pen( Color.FromArgb( 75, 75, 75 ) );

      StringFormat alignCenter = new StringFormat { Alignment = StringAlignment.Center };


      // rdp summary

      if( showRDP )
      {
        y += 20;


        // calc percent complete

        int totalPointsCampaign = 0;
        foreach( Factory factory in countryFactorys )
          totalPointsCampaign += factory.TotalProduced;

        int totalPointsThisCycle = totalPointsCampaign - country.RDPPrevCyclePoints;
        if( totalPointsThisCycle > country.RDPGoal )
          totalPointsThisCycle = country.RDPGoal;  // cycle finished

        float percentComplete = ( (float)totalPointsThisCycle / (float)country.RDPGoal ) * 100;


        // calc eta

        int totalPointsRemaining = country.RDPGoal - totalPointsThisCycle;
        DateTime cycleCompleted = CalcRdpEstimatedCompletion( countryFactorys, totalPointsRemaining );


        // draw stuff, add tooltips

        g.DrawLine( penLine, 0, 20, 334, 20 );

        if( cycleCompleted == DateTime.MaxValue )  // never
        {
          g.DrawString( String.Format( Language.FactoryStatus_RDPCycleNoEta, country.NextRDPCycle, percentComplete.ToString( "0.00" ) ),
                      fontHead, brushMain, 167, 3, alignCenter );
          imgmap.AddRectangle( Language.FactoryStatus_Tooltip_AllFactoriesCaptured, new Rectangle( 230, 5, 90, 10 ) );
        }
        else
        {
          g.DrawString( String.Format( Language.FactoryStatus_RDPCycle, country.NextRDPCycle, percentComplete.ToString( "0.00" ), Misc.MinsUntilShort( cycleCompleted ) ),
                        fontHead, brushMain, 167, 3, alignCenter );
          imgmap.AddRectangle( String.Format( Language.FactoryStatus_Tooltip_EstimatedCompletion, cycleCompleted.ToString( "ddd, d MMM, h:mm tt" ) ),
                     new Rectangle( 230, 5, 90, 10 ) );
        }

        imgmap.AddRectangle( String.Format( Language.FactoryStatus_Tooltip_RdpPoints, totalPointsThisCycle, country.RDPGoal ),
                             new Rectangle( 110, 5, 100, 10 ) );
      }


      // factory loop

      foreach( Factory factory in countryFactorys )
      {
        y += 25;


        // draw heading/parent chokepoint

        int x_indent = 0;
        if( factory.Owner.Side != country.Side )  // not original side, indent and draw flag
        {
          x_indent = 15;
          g.DrawImage( factory.Owner.CountryFlag, 2F, y + 3.5F, 12, 7.125F );
        }

        string factoryName = Regex.Replace( factory.Name, @"^\S+\s", "" );  // remove eg, "British "
        g.DrawString( factoryName, fontHead, brushHead, x_indent, y );
        g.DrawString( factory.ChokePoint.Name, fontMain, brushMain, x_indent + 10, y + 11 );


        if( factory.Ticks.Count == 0 ) continue;  // skip if no data


        // draw rdp output percent pie & bar graph

        g.FillRectangle( brushPieDark, 137, y + 1, 6, 20 );
        float rdpBarHeight = ( factory.OutputPercentPastDay * 20F ) / 100F;
        g.FillRectangle( brushPieLight, 137, y + 1 + 20 - rdpBarHeight, 6, rdpBarHeight );
        imgmap.AddRectangle( String.Format( Language.FactoryStatus_Tooltip_OutputDay, factory.OutputPercentPastDay ), new Rectangle( 137, y + 1, 6, 20 ) );

        g.FillEllipse( brushPieDark, 147, y + 1, 20, 20 );
        int angle = (int)( ( factory.OutputPercentCampaign * 360F ) / 100F );
        g.FillPie( brushPieLight, 147, y + 1, 20, 20, -90, angle );
        imgmap.AddElipse( String.Format( Language.FactoryStatus_Tooltip_OutputCampaign, factory.OutputPercentCampaign ), 157, y + 11, 10 );


        // draw rdp output percentages

        g.DrawString( factory.OutputPercentPastDay + "%", fontMain, brushMain, 185, y, alignCenter );
        g.DrawString( factory.OutputPercentCampaign + "%", fontMain, brushPieLight, 185, y + 11, alignCenter );

        imgmap.AddRectangle( String.Format( Language.FactoryStatus_Tooltip_OutputDay, factory.OutputPercentPastDay ), new Rectangle( 171, y + 2, 29, 10 ) );
        imgmap.AddRectangle( String.Format( Language.FactoryStatus_Tooltip_OutputCampaign, factory.OutputPercentCampaign ), new Rectangle( 171, y + 13, 29, 10 ) );


        // draw damage & rdp 24-hour graph

        int rdpHeight = -1;  // 0-5, rdp=1 => ++, rdp=0 => --

        for( int i = 0; i < factory.Ticks.Count; i++ )  // should be <= 96
        {
          // damage graph

          float damagePercent = factory.Ticks[i].Damage / 100F;
          float barHeight = 10F - ( 10F * damagePercent );

          Pen penDamage = new Pen( Misc.BlendColour( Color.FromArgb( 0, 150, 0 ), Color.Red, damagePercent ) );
          g.DrawLine( penDamage, 204 + i, y + 12, 204 + i, y + 12 - barHeight );


          // rdp graph

          if( rdpHeight == -1 )  // first tick
            rdpHeight = factory.Ticks[i].RDP == 0 ? 0 : 5;
          else if( factory.Ticks[i].RDP == 0 && rdpHeight > 0 )
            rdpHeight--;
          else if( factory.Ticks[i].RDP == 1 && rdpHeight < 5 )
            rdpHeight++;

          g.DrawLine( Pens.Gray, 204 + i, y + 21, 204 + i, y + 21 - rdpHeight );
          g.DrawLine( Pens.White, 204 + i, y + 21 - rdpHeight, 204 + i, y + 21 - rdpHeight - 1 );
        }

        imgmap.AddRectangle( Language.FactoryStatus_Tooltip_GraphDamage, new Rectangle( 204, y + 2, 96, 11 ) );
        imgmap.AddRectangle( Language.FactoryStatus_Tooltip_GraphOutput, new Rectangle( 204, y + 15, 96, 7 ) );


        // draw damage percent, rdp flag

        g.DrawString( factory.CurrentHealth + "%", fontMain, brushMain, 317, y, alignCenter );
        imgmap.AddRectangle( String.Format( Language.FactoryStatus_Tooltip_CurrentHealth, factory.CurrentHealth ), new Rectangle( 303, y + 2, 29, 10 ) );

        Brush brushRDP = new SolidBrush( factory.CurrentRDP ? Color.FromArgb( 0, 150, 0 ) : Color.Red );
        g.DrawString( "RDP", fontRDP, brushRDP, 317, y + 12, alignCenter );

        imgmap.AddRectangle( ( factory.CurrentRDP ? Language.FactoryStatus_Tooltip_RdpOn : Language.FactoryStatus_Tooltip_RdpOff ), new Rectangle( 307, y + 14, 20, 10 ) );

      }  // end factory loop


      g.Dispose();

      return bitmap;
    }

    /// <summary>
    /// Calculates the completion date of the current rdp cycle given the current state of the factories.
    /// </summary>
    /// <param name="countryFactorys">A list of the selected countries factories.</param>
    /// <param name="totalPointsRemaining">Number of rdp points remaining in the current cycle.</param>
    /// <returns>The estimated date the cycle will complete or DateTime.MaxValue if unknown.</returns>
    private DateTime CalcRdpEstimatedCompletion( IList<Factory> countryFactorys, int totalPointsRemaining )
    {
      /* Assumptions:
       * - A healthy factory produces 1 rdp point every tick.
       * - RDP output stops when damage is > x%
       * - RDP output resumes somwhere between 5% => 4% damage (ie, 95.5% health)
       * - Factories rebuild at an uneven rate, slightly faster as health increases,
       *   on average: 1.717833694 percent health per tick
       * - For reference, a 10000 point cycle takes 9 factories about 11.5 days @ 100% production
       */


      // get list of only friendly owned factories

      List<Factory> friendlyFactorys = new List<Factory>();
      foreach( Factory factory in countryFactorys )
        if( factory.Owner.Side == factory.Country.Side )
          friendlyFactorys.Add( factory );


      // if all factories have been captured, return maxvalue (eta: unknown)

      if( friendlyFactorys.Count == 0 )
        return DateTime.MaxValue;


      // first simulate any non-producing factories rebuilding to full health

      int rebuildMins = 0;    // minutes spent rebuilding
      int rebuildPoints = 0;  // rdp points produced during rebuildMins

      Dictionary<Factory, float> health = new Dictionary<Factory, float>();
      foreach( Factory factory in friendlyFactorys )
        health.Add( factory, factory.CurrentHealth );

      while( true )
      {
        // exit loop if all factories are contributing to RDP (health > 95.5)

        bool allProducing = true;
        foreach( Factory factory in friendlyFactorys )
        {
          if( !factory.CurrentRDP && health[factory] <= 95.5F )  // not contributing to RDP
          {
            allProducing = false;
            break;
          }
        }
        if( allProducing )
          break;


        // exit loop if rdp cycle completed during rebuilding

        if( rebuildPoints >= totalPointsRemaining )
          break;


        // simulate tick

        rebuildMins += 15;


        // increment health

        foreach( Factory factory in friendlyFactorys )
        {
          if( health[factory] <= 95.5F )
            health[factory] += 1.717833694F;
        }


        // increment rdp points

        foreach( Factory factory in friendlyFactorys )
        {
          if( factory.CurrentRDP || health[factory] > 95.5F )
            rebuildPoints++;
        }

      }


      // now get final estimate

      DateTime estimate;

      if( rebuildPoints >= totalPointsRemaining )  // completed cycle during rebuild
      {
        estimate = DateTime.Now.AddMinutes( rebuildMins );
      }
      else  // calculate remaining time assuming no further damage
      {
        double daysRemaining = (double)( totalPointsRemaining - rebuildPoints ) / (double)( friendlyFactorys.Count * 96 );
        estimate = DateTime.Now.AddDays( daysRemaining ).AddMinutes( rebuildMins );
      }

      return estimate;
    }

    #endregion

    #region Event Handlers

    // dynamic focus
    private void tskFactory_MouseEnter( object sender, EventArgs e )
    {
      BegmMisc.FocusTaskPane( this );
    }
    private void expBritish_MouseEnter( object sender, EventArgs e )
    {
      BegmMisc.FocusTaskPane( this );
    }
    private void expFrench_MouseEnter( object sender, EventArgs e )
    {
      BegmMisc.FocusTaskPane( this );
    }
    private void expGerman_MouseEnter( object sender, EventArgs e )
    {
      BegmMisc.FocusTaskPane( this );
    }
    private void picBritish_MyMouseEnter( object sender, EventArgs e )
    {
      BegmMisc.FocusTaskPane( this );
    }
    private void picFrench_MyMouseEnter( object sender, EventArgs e )
    {
      BegmMisc.FocusTaskPane( this );
    }
    private void picGerman_MyMouseEnter( object sender, EventArgs e )
    {
      BegmMisc.FocusTaskPane( this );
    }

    // load factory data button
    private void btnLoadFactoryData_Click( object sender, EventArgs e )
    {
      GameStatus_LoadFactoryData();
    }

    // Reveal() timer
    private void tmrReveal_Tick( object sender, EventArgs e )
    {
      Expando expCountry = (Expando)tmrReveal.Tag;
      Expando expFactoryStatus = (Expando)this.Parent;

      if( expFactoryStatus.Animating )
        return;  // wait until finished animating

      int color = expCountry.CustomHeaderSettings.NormalGradientEndColor.R;

      if( color == 51 )  // first frame, highlight, scroll into view
      {
        expCountry.CustomHeaderSettings.NormalGradientEndColor =
          expCountry.CustomHeaderSettings.NormalGradientStartColor = Color.FromArgb( 101, 101, 101 );

        if( expFactoryStatus.TaskPane != null )
          expFactoryStatus.TaskPane.ScrollControlIntoView( expCountry );
      }
      else  // remainder, fade out, stop
      {
        color -= 10;

        expCountry.CustomHeaderSettings.NormalGradientEndColor =
          expCountry.CustomHeaderSettings.NormalGradientStartColor = Color.FromArgb( color, color, color );

        if( color == 51 )
          tmrReveal.Stop();  // finished
      }
    }

    #endregion
  }
}
