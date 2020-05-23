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
using Aga.Controls.Tree;  // TreeViewAdv
using Aga.Controls.Tree.NodeControls;
using XPExplorerBar;
using BEGM.Properties;
using Xiperware.WiretapAPI;

namespace BEGM.Widgets
{
  /// <summary>
  /// The OrderOfBattle widget displays a tree of all the HCUnits in game in the appropriate structure.
  /// The user can navigate through the tree to view the current deployment location of divisions &amp;
  /// brigades, the player in charge, and the time/player that last moved the unit.
  /// </summary>
  public partial class OrderOfBattle : UserControl, IWidget
  {
    #region Variables

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
    /// Create a new OrderOfBattle widget.
    /// </summary>
    public OrderOfBattle()
    {
      InitializeComponent();


      // win classic theme

      if( !Application.RenderWithVisualStyles )
        picExpand.Top = picReset.Top = picCollapse.Top += 2;  // down 2px


      // init treeviewadv

      tvwOrbat.Model = new TreeModel();

      tvwOrbatTxtTitle.DrawText += tvwOrbat_DrawText;
      tvwOrbatTxtChokePoint.DrawText += tvwOrbat_DrawText;

      tvwOrbat.Expanded += tvwOrbat_Expanded;
      tvwOrbat.Collapsed += tvwOrbat_Collapsed;

      tvwOrbatTxtTitle.ToolTipProvider = new OrbatToolTipProvider();
      tvwOrbatTxtChokePoint.ToolTipProvider = new OrbatToolTipProvider();
      tvwOrbatIconTimer.ToolTipProvider = new OrbatToolTipProvider();

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
    /// Gets the selected brigade/division, or null if one isn't is selected.
    /// </summary>
    private HCUnit SelectedBrigade
    {
      get
      {
        if( tvwOrbat.SelectedNode == null )
          return null;

        HCUnit hcunit = ( (OrbatNode)tvwOrbat.SelectedNode.Tag ).hcunit;
        if( hcunit == null || hcunit.Level < HCUnitLevel.Division )
          return null;

        return hcunit;
      }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Initialise the OrderOfBattle widget.
    /// </summary>
    /// <param name="game">The current game state.</param>
    public void InitWidget( GameState game )
    {
      Log.AddEntry( "  OrderOfBattle..." );


      // set tooltips

      GameStatus.ToolTip.SetToolTip( picExpand, Language.Common_Tooltip_ExpandAll );
      GameStatus.ToolTip.SetToolTip( picReset, Language.Common_Tooltip_ExpandTop );
      GameStatus.ToolTip.SetToolTip( picCollapse, Language.Common_Tooltip_CollapseAll );


      // create tree nodes

      PopulateTree( game.Countries, game.TopHCUnit );


      Log.Okay();
    }

    /// <summary>
    /// Refreshes the tree with the current owner and deployed cp info.
    /// </summary>
    public void UpdateWidget()
    {
      foreach( Node node in ( (TreeModel)tvwOrbat.Model ).Nodes )
        UpdateNode( node );

      this.Refresh();
    }

    /// <summary>
    /// Remove references to local game objects.
    /// </summary>
    public void Cleanup()
    {
      ( (TreeModel)tvwOrbat.Model ).Nodes.Clear();
    }

    /// <summary>
    /// Makes the Order of Battle widget visible by making sure it's expanded, scrolling it into
    /// view, and expanding the relevant tree nodes to select the given hcunit.
    /// </summary>
    /// <seealso cref="tmrReveal_Tick"/>
    /// <param name="arg">The HCUnit to select.</param>
    public void Reveal( object arg )
    {
      HCUnit hcunit = arg as HCUnit;
      if( hcunit == null ) return;

      if( tmrReveal.Enabled )
        return;  // another Reveal() in progress

      TreeNodeAdv node = FindOrbatNode( tvwOrbat.Root, hcunit );
      if( node == null ) return;  // not found;

      tvwOrbat.EnsureVisible( node );
      node.IsExpanded = true;
      tvwOrbat.SelectedNode = node;

      Expando expOrderOfBattle = (Expando)this.Parent;
      if( expOrderOfBattle.Collapsed )
      {
        expOrderOfBattle.Collapsed = false;  // animate
        tmrReveal.Start();                   // scroll into view after animation
      }
      else
      {
        if( expOrderOfBattle.TaskPane != null )
          expOrderOfBattle.TaskPane.ScrollControlIntoView( expOrderOfBattle );
      }
    }



    /// <summary>
    /// Populates the tree with the given countries and hcunit hierarchy.
    /// </summary>
    /// <param name="countrys">An array of Country's.</param>
    /// <param name="topHCUnit">The top-level HCUnit that contains all other hcunits.</param>
    private void PopulateTree( IEnumerable<Country> countrys, HCUnit topHCUnit )
    {
      tvwOrbat.BeginUpdate();

      ( (TreeModel)tvwOrbat.Model ).Nodes.Clear();

      foreach( Country country in countrys )
      {
        if( !country.IsActive ) continue;

        Node countryNode = new OrbatNode( country.Name );
        ( (TreeModel)tvwOrbat.Model ).Nodes.Add( countryNode );

        foreach( int branch in Enum.GetValues( typeof( Branch ) ) )
        {
          if( branch == (int)Branch.None ) continue;

          Node branchNode = new OrbatNode( Misc.EnumString( (Branch)branch ) );
          countryNode.Nodes.Add( branchNode );

          if( topHCUnit != null )
          {
            foreach( HCUnit hcunit in topHCUnit.ChildUnits ) // top levels
            {
              if( hcunit.Country != country || hcunit.Branch != (Branch)branch ) continue;

              Node hcunitNode = new OrbatNode( hcunit );
              branchNode.Nodes.Add( hcunitNode );
              PopulateNode( hcunitNode, hcunit );
            }
          }
        }
      }

      RestoreTree();
      tvwOrbat.EndUpdate();
      ResizeWidget();
    }

    /// <summary>
    /// Adds any child HCUnits to the given node.
    /// </summary>
    /// <param name="parentNode">The node under which to add items.</param>
    /// <param name="parentUnit">The HCUnit containing child units.</param>
    private void PopulateNode( Node parentNode, HCUnit parentUnit )
    {
      foreach( HCUnit childUnit in parentUnit.ChildUnits )
      {
        Node childNode = new OrbatNode( childUnit );
        parentNode.Nodes.Add( childNode );
        PopulateNode( childNode, childUnit );
      }
    }

    /// <summary>
    /// A recursive method to update a specific tree node and all its children.
    /// </summary>
    /// <param name="parent">The parent node to update.</param>
    private void UpdateNode( Node parent )
    {
      ( (OrbatNode)parent ).Update();

      foreach( Node child in parent.Nodes )
        UpdateNode( child );
    }

    /// <summary>
    /// A recursive method to find the tree node that is associated with the given HCUnit.
    /// </summary>
    /// <param name="root">The root node to start searching.</param>
    /// <param name="hcunit">The HCUnit to look for.</param>
    /// <returns>The node if found, otherwise null.</returns>
    private TreeNodeAdv FindOrbatNode( TreeNodeAdv root, HCUnit hcunit )
    {
      foreach( TreeNodeAdv node in root.Nodes )
      {
        if( ( (OrbatNode)node.Tag ).hcunit == hcunit )
          return node;
        TreeNodeAdv res = FindOrbatNode( node, hcunit );
        if( res != null )
          return res;
      }
      return null;
    }

    /// <summary>
    /// Expand only the first three levels of the tree.
    /// </summary>
    private void RestoreTree()
    {
      tvwOrbat.CollapseAll();
      tvwOrbat.BeginUpdate();

      foreach( TreeNodeAdv node1 in tvwOrbat.Root.Nodes )
      {
        node1.IsExpanded = true;
        foreach( TreeNodeAdv node2 in node1.Nodes )
        {
          node2.IsExpanded = true;
          foreach( TreeNodeAdv node3 in node2.Nodes )
          {
            node3.IsExpanded = true;
          }
        }
      }

      tvwOrbat.EndUpdate();
    }

    /// <summary>
    /// Adjust the height of the parent expando to fit the full tree in.
    /// </summary>
    private void ResizeWidget()
    {
      if( !tvwOrbat._suspendUpdate )
      {
        int tvwHeight = tvwOrbat.RowCount * tvwOrbat.RowHeight;
        this.Height = tvwHeight + 20;

        Expando expOrderOfBattle = (Expando)this.Parent;
        expOrderOfBattle.SuspendLayout();
        expOrderOfBattle.ExpandedHeight = tvwHeight + 46;
        expOrderOfBattle.Height += 1;  // workaround for bug in tskMain ScrollableControl:
        expOrderOfBattle.Height -= 1;  // force the correct scroll height
        expOrderOfBattle.ResumeLayout();
      }
    }

    #endregion

    #region Event Handlers

    // text colour
    private void tvwOrbat_DrawText( object sender, DrawEventArgs e )
    {
      e.TextBrush = new SolidBrush( Color.FromArgb( 224, 224, 224 ) );  // default forecolor

      HCUnit hcunit = ( (OrbatNode)e.Node.Tag ).hcunit;
      if( hcunit == null ) return;

      if( hcunit.IsDeployable && !hcunit.IsDeployed )
      {
        e.TextBrush = Brushes.Gray;  // is an undeployed division or brigade
      }
      else if( sender == tvwOrbatTxtChokePoint )
      {
        // color of chokepoint column depends on how recently the unit has
        // been moved, within the past 24 hours

        if( hcunit.LastMovedPlayer == null ) return;  // not moved recently
        e.TextBrush = new SolidBrush( Misc.GetFadedYellow( hcunit.LastMovedTime, 12 * 60, 224 ) );
      }
    }

    // prevent scroll on focus
    private void tvwOrbat_MouseEnter( object sender, EventArgs e )
    {
      BegmMisc.FocusWithoutScroll( tvwOrbat );
    }

    // expand/restore/collapse buttons
    private void picExpand_MouseClick( object sender, MouseEventArgs e )
    {
      if( e.Button != MouseButtons.Left ) return;

      tvwOrbat.ExpandAll();
      ResizeWidget();
    }
    private void picReset_MouseClick( object sender, MouseEventArgs e )
    {
      if( e.Button != MouseButtons.Left ) return;

      RestoreTree();
      ResizeWidget();
    }
    private void picCollapse_MouseClick( object sender, MouseEventArgs e )
    {
      if( e.Button != MouseButtons.Left ) return;

      tvwOrbat.CollapseAll();
      ResizeWidget();
    }

    // resize expando on expand/collapse
    private void tvwOrbat_Expanded( object sender, TreeViewAdvEventArgs e )
    {
      Expando expOrderOfBattle = (Expando)this.Parent;

      if( expOrderOfBattle.TaskPane != null )
        expOrderOfBattle.TaskPane.PreventAutoScroll = true;

      ResizeWidget();

      if( expOrderOfBattle.TaskPane != null )
        expOrderOfBattle.TaskPane.PreventAutoScroll = false;
    }
    private void tvwOrbat_Collapsed( object sender, TreeViewAdvEventArgs e )
    {
      Expando expOrderOfBattle = (Expando)this.Parent;

      if( expOrderOfBattle.TaskPane != null )
        expOrderOfBattle.TaskPane.PreventAutoScroll = true;

      ResizeWidget();

      if( expOrderOfBattle.TaskPane != null )
        expOrderOfBattle.TaskPane.PreventAutoScroll = false;
    }

    // Reveal() timer
    private void tmrReveal_Tick( object sender, EventArgs e )
    {
      Expando expOrderOfBattle = (Expando)this.Parent;

      if( expOrderOfBattle.Animating )
        return;  // wait until finished animating

      if( expOrderOfBattle.TaskPane != null )
        expOrderOfBattle.TaskPane.ScrollControlIntoView( expOrderOfBattle );

      tmrReveal.Stop();
    }

    // don't show right-click menu on non-brigade/divisions
    private void cmsOrbat_Opening( object sender, System.ComponentModel.CancelEventArgs e )
    {
      HCUnit hcunit = this.SelectedBrigade;
      if( hcunit == null )
        e.Cancel = true;
    }

    // right-click menu items
    private void miBrigadeStatus_Click( object sender, EventArgs e )
    {
      HCUnit hcunit = this.SelectedBrigade;
      if( hcunit == null )
        return;

      GameStatus_RevealWidget( WidgetType.BrigadeStatus, hcunit );
    }
    private void miEquipment_Click( object sender, EventArgs e )
    {
      HCUnit hcunit = this.SelectedBrigade;
      if( hcunit == null )
        return;

      GameStatus_RevealWidget( WidgetType.Equipment, hcunit.Toe );
    }

    #endregion

    #region Classes

    /// <summary>
    /// A specialised Node that stores data for each of the three columns, as well as a reference to
    /// the actual HCUnit.
    /// </summary>
    public class OrbatNode : Node
    {
      #region Variables

      public string ChokePoint;
      public Bitmap Timer;
      public HCUnit hcunit;

      #endregion

      #region Constructors

      /// <summary>
      /// Create a new node for a HCUnit.
      /// </summary>
      /// <param name="hcunit"></param>
      public OrbatNode( HCUnit hcunit )
        : base( hcunit.Title )
      {
        this.hcunit = hcunit;

        this.Update();
      }

      /// <summary>
      /// Create a new node for a Country or Branch (text only).
      /// </summary>
      /// <param name="text"></param>
      public OrbatNode( string text )
        : base( text )
      {

      }

      #endregion

      #region Methods

      /// <summary>
      /// Updates the node with the current state of it's HCUnit.
      /// No-op for text-nodes or hcunits above division.
      /// </summary>
      public void Update()
      {
        if( this.hcunit == null ) return;  // don't do eg, country/branch nodes
        if( this.hcunit.Level < HCUnitLevel.Division ) return;  // only do division/brigade

        if( hcunit.IsDeployed )
          this.ChokePoint = hcunit.DeployedChokePoint.Name;
        else if( hcunit.RoutedFromChokePoint != null )
          this.ChokePoint = hcunit.RoutedFromChokePoint.Name;
        else
          this.ChokePoint = null;

        this.Timer = this.hcunit.MovePending ? Resources.movepending : DrawNewTimer();
      }

      /// <summary>
      /// Draws a new pie graph timer image for the current division/brigade.
      /// </summary>
      /// <returns>The resulting image.</returns>
      private Bitmap DrawNewTimer()
      {
        // create new bitmap

        Bitmap bitmap = new Bitmap( 15, 15 );
        Graphics g = Graphics.FromImage( bitmap );
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;


        // get timer percent

        float percent = this.hcunit.NextMovePercent;


        // get colours to display

        Brush pieForeground = this.hcunit.IsDeployed ? Brushes.Green : Brushes.Red;

        Brush pieBackground;
        if( percent == 0 )
          pieBackground = this.hcunit.IsDeployed ? new SolidBrush( Color.FromArgb( 0, 50, 0 ) ) : new SolidBrush( Color.FromArgb( 100, 0, 0 ) );
        else
          pieBackground = this.hcunit.IsDeployed ? new SolidBrush( Color.FromArgb( 0, 80, 0 ) ) : new SolidBrush( Color.FromArgb( 150, 0, 0 ) );


        // draw pie

        g.FillEllipse( pieBackground, 0.5F, 0.5F, 14, 14 );
        if( percent > 0 )
        {
          float angle = ( percent * 360F ) / 100F;
          g.FillPie( pieForeground, 0.5F, 0.5F, 14, 14, -90, angle );
        }


        // all done

        g.Dispose();
        return bitmap;
      }

      #endregion
    }

    /// <summary>
    /// A IToolTipProvider implementation that uses HCUnit.Tooltip/TooltipRedeploy.
    /// </summary>
    /// <see cref="HCUnit.Tooltip"/>
    /// <see cref="HCUnit.TooltipRedeploy"/>
    public class OrbatToolTipProvider : IToolTipProvider
    {
      /// <summary>
      /// Gets a tooltip for the orbat entries.
      /// </summary>
      /// <returns>The tooltip string, or null if no tooltip.</returns>
      public string GetToolTip( TreeNodeAdv node, NodeControl nodeControl )
      {
        HCUnit hcunit = ( (OrbatNode)node.Tag ).hcunit;
        if( hcunit == null )
          return null;

        if( nodeControl.ParentColumn.Index == 2 )
          return hcunit.TooltipRedeploy;
        else
          return hcunit.Tooltip;
      }
    }

    #endregion
  }
}
