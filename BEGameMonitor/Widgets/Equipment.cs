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
using System.Windows.Forms;
using Aga.Controls.Tree;
using Aga.Controls.Tree.NodeControls;
using BEGM.Properties;
using XPExplorerBar;
using Xiperware.WiretapAPI;

namespace BEGM.Widgets
{
  /// <summary>
  /// The Equipment widget displays a tree of all available TOE's and their contents.
  /// When a vehicle is selected it's details are shown along with supply details.
  /// </summary>
  public partial class Equipment : UserControl, IWidget
  {
    #region Variables

    public static int cycleOffset = 0;
    private int maxCycle = 0;  // highest seen cycle (current cycle + 1)

    private GameState game;
    private Vehicle selectedVehicle = null;
    private Toe selectedToe = null;

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
    /// Create a new Equipment widget.
    /// </summary>
    public Equipment()
    {
      InitializeComponent();


      // init treeviewadv

      tvwEquip.Model = new TreeModel();

      tvwEquipTxtName.DrawText += tvwEquip_DrawText;
      tvwEquipTxtNum1.DrawText += tvwEquip_DrawText;
      tvwEquipTxtNum2.DrawText += tvwEquip_DrawText;
      tvwEquipTxtNum3.DrawText += tvwEquip_DrawText;
      tvwEquipTxtNum4.DrawText += tvwEquip_DrawText;
      tvwEquipTxtNum5.DrawText += tvwEquip_DrawText;


      // dynamic positioning

      numSupplyCycle.Left = lblSupply.Right;
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
    /// Sets or gets the offset for scrolling the cycle columns left/right.
    /// </summary>
    private int CycleOffset
    {
      get { return cycleOffset; }
      set
      {
        int newCycleOffset = value;

        if( newCycleOffset > this.maxCycle - 4 )
          newCycleOffset = this.maxCycle - 4;
        if( newCycleOffset < 0 )
          newCycleOffset = 0;

        if( newCycleOffset == cycleOffset )
          return;

        cycleOffset = newCycleOffset;
        lblColHeader1.Text = ( cycleOffset + 0 ).ToString();
        lblColHeader2.Text = ( cycleOffset + 1 ).ToString();
        lblColHeader3.Text = ( cycleOffset + 2 ).ToString();
        lblColHeader4.Text = ( cycleOffset + 3 ).ToString();
        lblColHeader5.Text = ( cycleOffset + 4 ).ToString();
        this.Refresh();
      }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Initialise the Equipment widget.
    /// </summary>
    public void InitWidget( GameState game )
    {
      Log.AddEntry( "  Equipment..." );

      this.game = game;


      // set tooltips

      GameStatus.ToolTip.SetToolTip( picCyclePrev, Language.Equipment_Tooltip_PrevCycle );
      GameStatus.ToolTip.SetToolTip( picCycleNext, Language.Equipment_Tooltip_NextCycle );
      GameStatus.ToolTip.SetToolTip( picExpand, Language.Common_Tooltip_ExpandAll );
      GameStatus.ToolTip.SetToolTip( picReset, Language.Common_Tooltip_ExpandTop );
      GameStatus.ToolTip.SetToolTip( picCollapse, Language.Common_Tooltip_CollapseAll );


      // create tree nodes

      PopulateTree();


      // set cycle range

      this.CycleOffset = this.maxCycle - 4;  // start scrolled far right
      numSupplyCycle.Maximum = this.maxCycle;
      numSupplyCycle.Value = this.maxCycle > 0 ? this.maxCycle - 1 : 0;


      // set initial node expansion

      RestoreTree();


      Log.Okay();
    }

    /// <summary>
    /// Update the Equipment widget (unused).
    /// </summary>
    public void UpdateWidget()
    {
      
    }

    /// <summary>
    /// Remove references to local game objects.
    /// </summary>
    public void Cleanup()
    {
      ( (TreeModel)tvwEquip.Model ).Nodes.Clear();
      this.game = null;
      this.selectedVehicle = null;
      this.selectedToe = null;
    }

    /// <summary>
    /// Makes the Equipment widget visible by making sure it's expanded, scrolling it into
    /// view, and expanding the relevant tree nodes to select the given toe.
    /// </summary>
    /// <seealso cref="tmrReveal_Tick"/>
    /// <param name="arg">The Toe to select.</param>
    public void Reveal( object arg )
    {
      Toe toe = arg as Toe;
      if( toe == null ) return;

      if( tmrReveal.Enabled )
        return;  // another Reveal() in progress

      TreeNodeAdv node = FindEquipNode( toe );
      if( node == null ) return;  // not found;

      RestoreTree();
      node.IsExpanded = true;
      tvwEquip.ScrollToTop( node );  /// test
      tvwEquip.SelectedNode = node;

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
    }



    /// <summary>
    /// Populates the equipment tree by adding country, toe, vehicle class, and vehicle nodes.
    /// </summary>
    private void PopulateTree()
    {
      TreeModel model = (TreeModel)tvwEquip.Model;

      tvwEquip.BeginUpdate();


      // foreach country

      foreach( Country country in game.Countries )
      {
        if( !country.IsActive ) continue;

        Node countryNode = new EquipNode( country.Name );
        model.Nodes.Add( countryNode );


        // for each toe in country

        foreach( Toe toe in game.Toes.Values )
        {
          if( toe.Country != country ) continue;

          Node toeNode = new EquipNode( toe );
          countryNode.Nodes.Add( toeNode );


          // get all classes in toe

          List<VehicleClass> vehclasses = new List<VehicleClass>();
          foreach( Vehicle vehicle in toe.Vehicles )
            if( !vehclasses.Contains( vehicle.Class ) )
              vehclasses.Add( vehicle.Class );
          vehclasses.Sort();


          // for each vehicle class in toe

          foreach( VehicleClass vehclass in vehclasses )
          {
            Node classNode = new EquipNode( vehclass.Name );
            toeNode.Nodes.Add( classNode );


            // get all vehicles in class

            List<Vehicle> vehicles = new List<Vehicle>();
            foreach( Vehicle vehicle in toe.Vehicles )
              if( vehicle.Class == vehclass )
                vehicles.Add( vehicle );
            vehicles.Sort();


            // for each vehicle in class

            foreach( Vehicle vehicle in vehicles )
            {
              Node vehicleNode = new EquipNode( toe, vehicle );
              classNode.Nodes.Add( vehicleNode );


              // remember highest seen cycle number

              if( toe[vehicle].Cycle.Length - 1 > this.maxCycle )
                this.maxCycle = toe[vehicle].Cycle.Length - 1;

            }  // end foreach vehicle
          }  // end foreach vehicle class
        }  // end foreach toe
      }  // end foreach country


      if( this.maxCycle < 0 )
        this.maxCycle = 0;

      tvwEquip.EndUpdate();
    }

    /// <summary>
    /// Expand only the country and vehicle class levels of the tree.
    /// </summary>
    private void RestoreTree()
    {
      tvwEquip.BeginUpdate();

      foreach( TreeNodeAdv node1 in tvwEquip.Root.Nodes )
      {
        node1.IsExpanded = true;  // country
        foreach( TreeNodeAdv node2 in node1.Nodes )
        {
          node2.IsExpanded = false;  // toe
          foreach( TreeNodeAdv node3 in node2.Nodes )
          {
            node3.IsExpanded = true;  // vehicle class
          }
        }
      }

      tvwEquip.EndUpdate();
    }

    /// <summary>
    /// Gets the tree node that is associated with the given Toe.
    /// </summary>
    /// <param name="toe">The Toe to look for.</param>
    /// <returns>The node if found, otherwise null.</returns>
    private TreeNodeAdv FindEquipNode( Toe toe )
    {
      foreach( TreeNodeAdv countryNode in tvwEquip.Root.Nodes )
        foreach( TreeNodeAdv toeNode in countryNode.Nodes )
          if( ( (EquipNode)toeNode.Tag ).Toe == toe )
            return toeNode;

      return null;
    }

    /// <summary>
    /// Updates the controls with info on the currently selected vehicle.
    /// </summary>
    private void UpdateVehicleInfo()
    {
      // if null, clear controls

      if( this.selectedVehicle == null )
      {
        lblVehicleName.Text = lblVehicleClass.Text = null;
        picVehicleCountry.Image = null;
        picVehicle.Image = null;
        UpdateSupply();
        return;
      }


      // remove start tip

      if( lblStartTip.Visible )
      {
        lblStartTip.Visible = false;
        picVehicle.Visible = true;
      }


      // update vehicle controls

      lblVehicleName.Text = this.selectedVehicle.Name;
      lblVehicleClass.Text = this.selectedVehicle.Class.ToString();
      picVehicleCountry.Image = this.selectedVehicle.Country.CountryFlag;

      picVehicle.Image = GenerateVehicleImage();


      // update supply controls

      UpdateSupply();
    }

    /// <summary>
    /// Updates the supply controls with info on the currently selected vehicle.
    /// </summary>
    private void UpdateSupply()
    {
      // if no selected vehicle, hide controls

      if( this.selectedVehicle == null )
      {
        lblSupply.Visible = numSupplyCycle.Visible = false;
        lblSupplyTotalHead.Visible = lblSupplyTotal.Visible = false;
        picSupply.Image = null;
        picSupply.Height = 0;
        return;
      }


      // get hcunits that contain the selected vehicle

      List<HCUnit> hcunits = new List<HCUnit>();
      List<int> counts = new List<int>();
      if( game.TopHCUnit != null )
        GetSupply( game.TopHCUnit, ref hcunits, ref counts );


      // calc total

      int total = 0;
      foreach( int count in counts )
        total += count;


      // update controls

      lblSupply.Visible = numSupplyCycle.Visible = true;
      lblSupplyTotalHead.Visible = lblSupplyTotal.Visible = total > 0;
      lblSupplyTotal.Text = total.ToString();

      picSupply.Image = GenerateSupplyImage( hcunits, counts, picSupply );
      picSupply.Height = picSupply.Image.Height;
    }

    /// <summary>
    /// Recurses down the hcunit tree and generates a pair of parallel lists containing
    /// supply info for the currently selected vehicle.
    /// </summary>
    /// <param name="parent">The parent hcunit to process.</param>
    /// <param name="hcunits">A list containing the units that have the selected vehicle.</param>
    /// <param name="count">A list containing the number of vehicles the unit has.</param>
    private void GetSupply( HCUnit parent, ref List<HCUnit> hcunits, ref List<int> count )
    {
      int cycle = (int)numSupplyCycle.Value;


      // add unit if it's toe contains the vehicle and has supply

      if( parent.Toe != null && parent.Toe.Vehicles.Contains( this.selectedVehicle ) &&
        cycle < parent.Toe[this.selectedVehicle].Cycle.Length && parent.Toe[this.selectedVehicle].Cycle[cycle] > 0 )
      {
        hcunits.Add( parent );
        count.Add( parent.Toe[this.selectedVehicle].Cycle[cycle] );
      }


      // recurse child units

      foreach( HCUnit child in parent.ChildUnits )
        GetSupply( child, ref hcunits, ref count );
    }

    /// <summary>
    /// Generates an image of the currently selected vehicle, with background and rank information.
    /// </summary>
    /// <returns>The vehicle image, or null if no vehicle selected.</returns>
    private Image GenerateVehicleImage()
    {
      if( this.selectedVehicle == null )
        return null;  // no image


      // try to get vehicle image

      Image vechImage = (Image)Vehicles.ResourceManager.GetObject( String.Format( "vehicle{0:000}", this.selectedVehicle.ID ) );


      // create graphics resources

      Bitmap bitmap = new Bitmap( picVehicle.Width, picVehicle.Height );
      Graphics g = Graphics.FromImage( bitmap );

      Pen penRank = new Pen( Color.LightSlateGray, 2F );
      Brush brushRank = new SolidBrush( Color.LightSlateGray );

      Brush brushMain = new SolidBrush( Color.FromArgb( 192, 192, 192 ) );
      Brush brushRankHead = new SolidBrush( Color.Black );
      Font fontMain = new Font( "Tahoma", 8.25F );
      Font fontRank = new Font( "Microsoft Sans Serif", 6.75F );
      Font fontRankHead = new Font( fontRank, FontStyle.Bold );

      StringFormat alignCenter = new StringFormat { Alignment = StringAlignment.Center };


      // draw background

      g.DrawImage( Resources.vechbg, 0, 0, bitmap.Width, bitmap.Height );


      // draw rank boxes

      const int rankWidth = 104;
      const int rankHeight = 32;
      const int rankHeader = 12;

      g.DrawRectangle( penRank, bitmap.Width - rankWidth - 1, 1, rankWidth, rankHeight );
      g.FillRectangle( brushRank, bitmap.Width - rankWidth - 1, 1, rankWidth, rankHeader );
      g.DrawString( Language.Equipment_RankRequired, fontRankHead, brushRankHead, bitmap.Width - ( rankWidth / 2F ) - 1, 0, alignCenter );
      g.DrawString( String.Format( "{0} {1}", this.selectedVehicle.CurrentRank.Code, this.selectedVehicle.GetCurrentRankName( this.selectedToe.Branch ) ),
                    fontRank, brushMain, bitmap.Width - ( rankWidth / 2F ) - 1, rankHeader + 4, alignCenter );

      if( this.selectedVehicle.NextRank != this.selectedVehicle.CurrentRank )
      {
        g.DrawRectangle( penRank, bitmap.Width - rankWidth - 1, rankHeight + 1, rankWidth, rankHeight );
        g.FillRectangle( brushRank, bitmap.Width - rankWidth - 1, rankHeight + 1, rankWidth, rankHeader );
        g.DrawString( Language.Equipment_RankRequiredNextCycle, fontRankHead, brushRankHead, bitmap.Width - ( rankWidth / 2F ) - 1, rankHeight, alignCenter );
        g.DrawString( String.Format( "{0} {1}", this.selectedVehicle.NextRank.Code, this.selectedVehicle.GetNextRankName( this.selectedToe.Branch ) ),
                      fontRank, brushMain, bitmap.Width - ( rankWidth / 2F ) - 1, rankHeight + rankHeader + 4, alignCenter );
      }


      // draw vehicle (or message)

      if( vechImage != null )
        g.DrawImage( vechImage, 0, 0, bitmap.Width, bitmap.Height );
      else
        g.DrawString( Language.Equipment_NoImageAvailable, fontMain, brushMain, picVehicle.Width / 2, ( picVehicle.Height / 2 ) - 5, alignCenter );
        // note: int division intentional


      // all done

      g.Dispose();
      return bitmap;
    }

    /// <summary>
    /// Generates an image of all hcunits that have supply of the currently selected vehicle.
    /// </summary>
    /// <param name="hcunits">A list of hcunits.</param>
    /// <param name="count">A list of the hcunits' supply numbers.</param>
    /// <param name="imgmap">An ImageMap on which to add tooltips & clickable regions.</param>
    /// <returns>The supply image to display.</returns>
    private Image GenerateSupplyImage( IList<HCUnit> hcunits, IList<int> count, ImageMap.ImageMap imgmap )
    {
      // calculate required dimensions

      const int lineHeight = 14;
      int linesRequired = 0;

      HCUnit prevDivision = null;
      foreach( HCUnit hcunit in hcunits )
      {
        linesRequired++;
        HCUnit division = hcunit.Level == HCUnitLevel.Division ? hcunit : hcunit.ParentUnit;
        if( division != prevDivision ) { linesRequired++; prevDivision = division; }
      }

      if( linesRequired == 0 )
        linesRequired = 4;  // for "not available" message

      int height = ( linesRequired * lineHeight ) + 2;
      int width = imgmap.Width;


      // create bitmap, graphics objects

      Bitmap bitmap = new Bitmap( width, height );
      Graphics g = Graphics.FromImage( bitmap );
      g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
      g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;


      // remove previous tooltips

      imgmap.RemoveAll();


      // create graphics resources

      Brush brushMain = new SolidBrush( Color.FromArgb( 192, 192, 192 ) );
      Brush brushHCUnitAllied = new SolidBrush( Color.FromArgb( 128, 204, 255 ) );  // light blue
      Brush brushHCUnitAxis = new SolidBrush( Color.FromArgb( 255, 178, 102 ) );  // orange

      Font fontMain = new Font( "Tahoma", 8.25F );

      StringFormat alignRight = new StringFormat { Alignment = StringAlignment.Far };
      StringFormat alignCenter = new StringFormat { Alignment = StringAlignment.Center };


      // hcunit loop

      int y = 0;
      prevDivision = null;

      for( int i = 0; i < hcunits.Count; i++ )
      {
        HCUnit hcunit = hcunits[i];
        HCUnit division = hcunit.Level == HCUnitLevel.Division ? hcunit : hcunit.ParentUnit;
        HCUnit brigade  = hcunit.Level == HCUnitLevel.Brigade  ? hcunit : hcunit.GetDummyDivHQBrigade();


        // draw division heading

        if( division != prevDivision )
        {
          g.DrawImage( division.Country.BrigadeFlag, 0, y + 2.5F, 17.57F, 9 );  // 41x21
          g.DrawString( division.Title, fontMain, brushMain, 17, y );

          y += lineHeight;
          prevDivision = division;
        }


        // draw unit title

        g.FillEllipse( hcunit.Country.Side == Side.Allied ? brushHCUnitAllied : brushHCUnitAxis,
                       21, y + 4, 5, 5 );
        g.DrawString( brigade.Title, fontMain, brushMain, 29, y );

        int stringWidth = (int)g.MeasureString( brigade.Title, fontMain, width - 29 ).Width - 4;
        imgmap.AddRectangle( Language.Common_Tooltip_GoToBrigade, new Rectangle( 30, y + 2, stringWidth, 10 ), true, hcunit );


        // draw count

        g.DrawString( count[i].ToString(), fontMain, brushMain, 250, y, alignRight );


        y += lineHeight;

      }  // end hcunit loop


      // if none available, show message

      if( hcunits.Count == 0 )
        g.DrawString( String.Format( Language.Equipment_NotAvailable, numSupplyCycle.Value ),
                      fontMain, brushMain, pnlSupply.Width / 2, lineHeight * 2.5F, alignCenter );
                      // note: int division intentional

      // all done

      g.Dispose();
      return bitmap;
    }

    #endregion

    #region Event Handlers
    
    // prev/next cycle buttons
    private void picCyclePrev_MouseClick( object sender, MouseEventArgs e )
    {
      if( e.Button != MouseButtons.Left ) return;
      this.CycleOffset--;
    }
    private void picCyclePrev_MouseDoubleClick( object sender, MouseEventArgs e )
    {
      if( e.Button != MouseButtons.Left ) return;
      this.CycleOffset--;
    }
    private void picCycleNext_MouseClick( object sender, MouseEventArgs e )
    {
      if( e.Button != MouseButtons.Left ) return;
      this.CycleOffset++;
    }
    private void picCycleNext_MouseDoubleClick( object sender, MouseEventArgs e )
    {
      if( e.Button != MouseButtons.Left ) return;
      this.CycleOffset++;
    }

    // expand/restore/collapse buttons
    private void picExpand_MouseClick( object sender, MouseEventArgs e )
    {
      if( e.Button != MouseButtons.Left ) return;
      tvwEquip.ExpandAll();
    }
    private void picReset_MouseClick( object sender, MouseEventArgs e )
    {
      if( e.Button != MouseButtons.Left ) return;

      tvwEquip.SelectedNode = null;
      RestoreTree();
    }
    private void picCollapse_MouseClick( object sender, MouseEventArgs e )
    {
      if( e.Button != MouseButtons.Left ) return;

      tvwEquip.SelectedNode = null;
      tvwEquip.CollapseAll();
    }

    // update new vehicle info
    private void tvwEquip_SelectionChanged( object sender, EventArgs e )
    {
      // get selected node

      if( tvwEquip.SelectedNode != null )
      {
        EquipNode node = (EquipNode)tvwEquip.SelectedNode.Tag;
        this.selectedVehicle = node.Vehicle;
        this.selectedToe = this.selectedVehicle == null ? null : ( (EquipNode)node.Parent.Parent ).Toe;
      }
      else
      {
        this.selectedVehicle = null;
        this.selectedToe = null;
      }


      // update controls

      UpdateVehicleInfo();
    }

    // text colour
    private void tvwEquip_DrawText( object sender, DrawEventArgs e )
    {
      e.TextBrush = new SolidBrush( Color.FromArgb( 224, 224, 224 ) ); // default forecolor

      if( sender == tvwEquipTxtName ) return;

      EquipNode node = (EquipNode)e.Node.Tag;

      int col = -1;
      if     ( sender == tvwEquipTxtNum1 ) col = 1;
      else if( sender == tvwEquipTxtNum2 ) col = 2;
      else if( sender == tvwEquipTxtNum3 ) col = 3;
      else if( sender == tvwEquipTxtNum4 ) col = 4;
      else if( sender == tvwEquipTxtNum5 ) col = 5;


      if( node.GetColValue( col ) == "-" )
        e.TextBrush = new SolidBrush( Color.FromArgb( 128, 128, 128 ) );
      else
        e.TextBrush = new SolidBrush( Color.FromArgb( 224, 224, 224 ) );

      int cycle = node.GetColCycle( col );
      if( cycle >= 0 && cycle == node.CurrentCycle )
        e.BackgroundBrush = new SolidBrush( Color.FromArgb( 80, 80, 80 ) );
    }

    // update supply details
    private void numSupplyCycle_ValueChanged( object sender, EventArgs e )
    {
      UpdateSupply();
    }

    // supply links
    private void picSupply_RegionClick( int index, object tag, MouseButtons button )
    {
      if( button != MouseButtons.Left )
        return;

      GameStatus_RevealWidget( WidgetType.BrigadeStatus, tag );  // tag is a hcunit
    }

    // prevent mousewheel scrolling taskpane while in tree or supply windows
    private void tvwEquip_MouseEnter( object sender, EventArgs e )
    {
      BegmMisc.FocusWithoutScroll( tvwEquip );

      Expando expEquipment = (Expando)this.Parent;
      if( expEquipment.TaskPane != null )
        expEquipment.TaskPane.PreventAutoScroll = true;
    }
    private void tvwEquip_MouseLeave( object sender, EventArgs e )
    {
      Expando expEquipment = (Expando)this.Parent;
      if( expEquipment.TaskPane != null )
        expEquipment.TaskPane.PreventAutoScroll = false;

      BegmMisc.FocusTaskPane( this );
    }
    private void pnlSupply_MouseEnter( object sender, EventArgs e )
    {
      BegmMisc.FocusWithoutScroll( pnlSupply );

      Expando expEquipment = (Expando)this.Parent;
      if( expEquipment.TaskPane != null )
        expEquipment.TaskPane.PreventAutoScroll = true;
    }
    private void picSupply_MyMouseEnter( object sender, EventArgs e )
    {
      BegmMisc.FocusWithoutScroll( pnlSupply );

      Expando expEquipment = (Expando)this.Parent;
      if( expEquipment.TaskPane != null )
        expEquipment.TaskPane.PreventAutoScroll = true;
    }
    private void pnlSupply_MouseLeave( object sender, EventArgs e )
    {
      Expando expEquipment = (Expando)this.Parent;
      if( expEquipment.TaskPane != null )
        expEquipment.TaskPane.PreventAutoScroll = false;

      BegmMisc.FocusTaskPane( this );
    }
    private void picSupply_MyMouseLeave( object sender, EventArgs e )
    {
      Expando expEquipment = (Expando)this.Parent;
      if( expEquipment.TaskPane != null )
        expEquipment.TaskPane.PreventAutoScroll = false;

      BegmMisc.FocusTaskPane( this );
    }

    // avoid focus
    private void numSupplyCycle_Enter( object sender, EventArgs e )
    {
      BegmMisc.FocusWithoutScroll( pnlSupply );
    }

    // Reveal() timer
    private void tmrReveal_Tick( object sender, EventArgs e )
    {
      Expando expEquipment = (Expando)this.Parent;

      if( expEquipment.Animating )
        return;  // wait until finished animating

      if( expEquipment.TaskPane != null )
        expEquipment.TaskPane.ScrollControlIntoView( expEquipment );
      tmrReveal.Stop();
    }

    // dynamic focus
    private void Equipment_MouseEnter( object sender, EventArgs e )
    {
      BegmMisc.FocusTaskPane( this );
    }

    #endregion

    #region Classes

    /// <summary>
    /// A specialised Node that stores data for each of the cycle columns, as well as a reference to
    /// the actual toe or vehicle.
    /// </summary>
    public class EquipNode : Node
    {
      #region Variables

      private Toe toe = null;
      private Vehicle vehicle = null;
      private int[] cycles = new int[0];

      #endregion

      #region Constructors

      /// <summary>
      /// Create a new Toe node.
      /// </summary>
      public EquipNode( Toe toe )
        : base( toe.Name )
      {
        this.toe = toe;
      }

      /// <summary>
      /// Create a new Vehicle node.
      /// </summary>
      public EquipNode( Toe toe, Vehicle vehicle )
        : base( vehicle.Name )
      {
        this.vehicle = vehicle;
        this.cycles = toe[vehicle].Cycle;
      }

      /// <summary>
      /// Create a new Misc node (eg: country, equip category).
      /// </summary>
      /// <param name="text"></param>
      public EquipNode( string text )
        : base( text )
      {

      }

      #endregion

      #region Properties

      /// <summary>
      /// Gets the value to be displayed in cycle column 1.
      /// </summary>
      public string Col1
      {
        get { return GetColValue( 1 ); }
      }

      /// <summary>
      /// Gets the value to be displayed in cycle column 2.
      /// </summary>
      public string Col2
      {
        get { return GetColValue( 2 ); }
      }

      /// <summary>
      /// Gets the value to be displayed in cycle column 3.
      /// </summary>
      public string Col3
      {
        get { return GetColValue( 3 ); }
      }

      /// <summary>
      /// Gets the value to be displayed in cycle column 4.
      /// </summary>
      public string Col4
      {
        get { return GetColValue( 4 ); }
      }

      /// <summary>
      /// Gets the value to be displayed in cycle column 5.
      /// </summary>
      public string Col5
      {
        get { return GetColValue( 5 ); }
      }

      /// <summary>
      /// Gets the active RDP cycle, or -1 if not a vehicle node.
      /// </summary>
      public int CurrentCycle
      {
        get { return this.vehicle == null ? -1 : this.vehicle.Country.CurrentRDPCycle; }
      }

      /// <summary>
      /// Gets the vehicle associated with this node, or null if not a vehicle node.
      /// </summary>
      public Vehicle Vehicle
      {
        get { return this.vehicle; }
      }

      /// <summary>
      /// Gets the toe associated with this node, or null if not a toe node.
      /// </summary>
      public Toe Toe
      {
        get { return toe; }
      }

      #endregion

      #region Methods

      /// <summary>
      /// Gets the rdp cycle assigned to the given column.
      /// </summary>
      /// <param name="col">The column number (first column = 1).</param>
      /// <returns>The cycle number, or -1 if no cycle assigned.</returns>
      public int GetColCycle( int col )
      {
        int index = Equipment.cycleOffset + col - 1;

        if( this.cycles.Length > index )
          return index;
        else
          return -1;
      }

      /// <summary>
      /// Gets the value to display in a given cycle column.
      /// </summary>
      /// <param name="col">The column number (first column = 1).</param>
      /// <returns>The number of vehicles in the cycle assigned to the column, "-" if none, or null if not a vehicle node.</returns>
      public string GetColValue( int col )
      {
        int index = GetColCycle( col );

        if( index < 0 )
          return null;
        if( this.cycles[index] == 0 )
          return "-";

        return this.cycles[index].ToString();
      }

      #endregion
    }

    #endregion
  }
}
