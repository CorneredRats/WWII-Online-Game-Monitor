namespace BEGM.Widgets
{
  partial class GameMap
  {
    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary> 
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose( bool disposing )
    {
      if( disposing && ( components != null ) )
      {
        components.Dispose();
      }
      base.Dispose( disposing );
    }

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager( typeof( GameMap ) );
      this.tmrUnloadMap = new System.Windows.Forms.Timer( this.components );
      this.lblLatLong = new System.Windows.Forms.Label();
      this.picCPFlag = new System.Windows.Forms.PictureBox();
      this.txtActiveCP = new System.Windows.Forms.TextBox();
      this.tmrReveal = new System.Windows.Forms.Timer( this.components );
      this.lnkLocation = new System.Windows.Forms.LinkLabel();
      this.lblLocationHead = new System.Windows.Forms.Label();
      this.lnkCPName = new System.Windows.Forms.LinkLabel();
      this.lblOrigOwnerHead = new System.Windows.Forms.Label();
      this.lblSupplyLinksHead = new System.Windows.Forms.Label();
      this.lblDeploymentsHead = new System.Windows.Forms.Label();
      this.lblFacilitiesHead = new System.Windows.Forms.Label();
      this.lblDeployments = new System.Windows.Forms.Label();
      this.lblFacilities = new System.Windows.Forms.Label();
      this.picOrigOwner = new System.Windows.Forms.PictureBox();
      this.lblOrigOwner = new System.Windows.Forms.Label();
      this.lblSupplyLinks = new System.Windows.Forms.Label();
      this.lblCell = new System.Windows.Forms.Label();
      this.bgwDrawOverlay = new System.ComponentModel.BackgroundWorker();
      this.cbMapOptions = new System.Windows.Forms.CheckBox();
      this.tvwMapOptions = new System.Windows.Forms.TreeView();
      this.mapViewer = new BEGM.MapViewer();
      this.picResizeCorner = new System.Windows.Forms.PictureBox();
      ( (System.ComponentModel.ISupportInitialize)( this.picCPFlag ) ).BeginInit();
      ( (System.ComponentModel.ISupportInitialize)( this.picOrigOwner ) ).BeginInit();
      ( (System.ComponentModel.ISupportInitialize)( this.picResizeCorner ) ).BeginInit();
      this.SuspendLayout();
      // 
      // tmrUnloadMap
      // 
      this.tmrUnloadMap.Tick += new System.EventHandler( this.tmrUnloadMap_Tick );
      // 
      // lblLatLong
      // 
      resources.ApplyResources( this.lblLatLong, "lblLatLong" );
      this.lblLatLong.Name = "lblLatLong";
      // 
      // picCPFlag
      // 
      resources.ApplyResources( this.picCPFlag, "picCPFlag" );
      this.picCPFlag.Name = "picCPFlag";
      this.picCPFlag.TabStop = false;
      // 
      // txtActiveCP
      // 
      this.txtActiveCP.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
      this.txtActiveCP.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
      this.txtActiveCP.BackColor = System.Drawing.Color.Black;
      this.txtActiveCP.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.txtActiveCP.ForeColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ) );
      resources.ApplyResources( this.txtActiveCP, "txtActiveCP" );
      this.txtActiveCP.Name = "txtActiveCP";
      this.txtActiveCP.KeyUp += new System.Windows.Forms.KeyEventHandler( this.txtActiveCP_KeyUp );
      this.txtActiveCP.Enter += new System.EventHandler( this.txtActiveCP_Enter );
      // 
      // tmrReveal
      // 
      this.tmrReveal.Tick += new System.EventHandler( this.tmrReveal_Tick );
      // 
      // lnkLocation
      // 
      this.lnkLocation.ActiveLinkColor = System.Drawing.Color.White;
      resources.ApplyResources( this.lnkLocation, "lnkLocation" );
      this.lnkLocation.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
      this.lnkLocation.LinkColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ) );
      this.lnkLocation.Name = "lnkLocation";
      this.lnkLocation.VisitedLinkColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ) );
      this.lnkLocation.MouseLeave += new System.EventHandler( this.lnkLocation_MouseLeave );
      this.lnkLocation.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler( this.lnkLocation_LinkClicked );
      this.lnkLocation.MouseHover += new System.EventHandler( this.lnkLocation_MouseHover );
      // 
      // lblLocationHead
      // 
      resources.ApplyResources( this.lblLocationHead, "lblLocationHead" );
      this.lblLocationHead.Name = "lblLocationHead";
      // 
      // lnkCPName
      // 
      this.lnkCPName.ActiveLinkColor = System.Drawing.Color.White;
      resources.ApplyResources( this.lnkCPName, "lnkCPName" );
      this.lnkCPName.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
      this.lnkCPName.LinkColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ) );
      this.lnkCPName.Name = "lnkCPName";
      this.lnkCPName.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler( this.lnkCPName_LinkClicked );
      // 
      // lblOrigOwnerHead
      // 
      resources.ApplyResources( this.lblOrigOwnerHead, "lblOrigOwnerHead" );
      this.lblOrigOwnerHead.Name = "lblOrigOwnerHead";
      // 
      // lblSupplyLinksHead
      // 
      resources.ApplyResources( this.lblSupplyLinksHead, "lblSupplyLinksHead" );
      this.lblSupplyLinksHead.Name = "lblSupplyLinksHead";
      // 
      // lblDeploymentsHead
      // 
      resources.ApplyResources( this.lblDeploymentsHead, "lblDeploymentsHead" );
      this.lblDeploymentsHead.Name = "lblDeploymentsHead";
      // 
      // lblFacilitiesHead
      // 
      resources.ApplyResources( this.lblFacilitiesHead, "lblFacilitiesHead" );
      this.lblFacilitiesHead.Name = "lblFacilitiesHead";
      // 
      // lblDeployments
      // 
      resources.ApplyResources( this.lblDeployments, "lblDeployments" );
      this.lblDeployments.Name = "lblDeployments";
      // 
      // lblFacilities
      // 
      resources.ApplyResources( this.lblFacilities, "lblFacilities" );
      this.lblFacilities.Name = "lblFacilities";
      // 
      // picOrigOwner
      // 
      resources.ApplyResources( this.picOrigOwner, "picOrigOwner" );
      this.picOrigOwner.Name = "picOrigOwner";
      this.picOrigOwner.TabStop = false;
      // 
      // lblOrigOwner
      // 
      resources.ApplyResources( this.lblOrigOwner, "lblOrigOwner" );
      this.lblOrigOwner.Name = "lblOrigOwner";
      // 
      // lblSupplyLinks
      // 
      resources.ApplyResources( this.lblSupplyLinks, "lblSupplyLinks" );
      this.lblSupplyLinks.Name = "lblSupplyLinks";
      // 
      // lblCell
      // 
      resources.ApplyResources( this.lblCell, "lblCell" );
      this.lblCell.Name = "lblCell";
      // 
      // bgwDrawOverlay
      // 
      this.bgwDrawOverlay.WorkerSupportsCancellation = true;
      this.bgwDrawOverlay.DoWork += new System.ComponentModel.DoWorkEventHandler( this.bgwDrawOverlay_DoWork );
      this.bgwDrawOverlay.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler( this.bgwDrawOverlay_RunWorkerCompleted );
      // 
      // cbMapOptions
      // 
      resources.ApplyResources( this.cbMapOptions, "cbMapOptions" );
      this.cbMapOptions.BackColor = System.Drawing.Color.Black;
      this.cbMapOptions.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 64 ) ) ) ), ( (int)( ( (byte)( 64 ) ) ) ), ( (int)( ( (byte)( 64 ) ) ) ) );
      this.cbMapOptions.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 96 ) ) ) ), ( (int)( ( (byte)( 96 ) ) ) ), ( (int)( ( (byte)( 96 ) ) ) ) );
      this.cbMapOptions.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Black;
      this.cbMapOptions.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 32 ) ) ) ), ( (int)( ( (byte)( 32 ) ) ) ), ( (int)( ( (byte)( 32 ) ) ) ) );
      this.cbMapOptions.ForeColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ) );
      this.cbMapOptions.Name = "cbMapOptions";
      this.cbMapOptions.UseVisualStyleBackColor = false;
      this.cbMapOptions.CheckedChanged += new System.EventHandler( this.cbMapOptions_CheckedChanged );
      // 
      // tvwMapOptions
      // 
      resources.ApplyResources( this.tvwMapOptions, "tvwMapOptions" );
      this.tvwMapOptions.BackColor = System.Drawing.Color.Black;
      this.tvwMapOptions.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.tvwMapOptions.CheckBoxes = true;
      this.tvwMapOptions.ForeColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ) );
      this.tvwMapOptions.Name = "tvwMapOptions";
      this.tvwMapOptions.Nodes.AddRange( new System.Windows.Forms.TreeNode[] {
            ((System.Windows.Forms.TreeNode)(resources.GetObject("tvwMapOptions.Nodes"))),
            ((System.Windows.Forms.TreeNode)(resources.GetObject("tvwMapOptions.Nodes1"))),
            ((System.Windows.Forms.TreeNode)(resources.GetObject("tvwMapOptions.Nodes2"))),
            ((System.Windows.Forms.TreeNode)(resources.GetObject("tvwMapOptions.Nodes3"))),
            ((System.Windows.Forms.TreeNode)(resources.GetObject("tvwMapOptions.Nodes4"))),
            ((System.Windows.Forms.TreeNode)(resources.GetObject("tvwMapOptions.Nodes5"))),
            ((System.Windows.Forms.TreeNode)(resources.GetObject("tvwMapOptions.Nodes6"))),
            ((System.Windows.Forms.TreeNode)(resources.GetObject("tvwMapOptions.Nodes7"))),
            ((System.Windows.Forms.TreeNode)(resources.GetObject("tvwMapOptions.Nodes8"))),
            ((System.Windows.Forms.TreeNode)(resources.GetObject("tvwMapOptions.Nodes9")))} );
      this.tvwMapOptions.Scrollable = false;
      this.tvwMapOptions.ShowLines = false;
      this.tvwMapOptions.ShowPlusMinus = false;
      this.tvwMapOptions.ShowRootLines = false;
      this.tvwMapOptions.AfterCheck += new System.Windows.Forms.TreeViewEventHandler( this.tvwMapOptions_AfterCheck );
      this.tvwMapOptions.BeforeCollapse += new System.Windows.Forms.TreeViewCancelEventHandler( this.tvwMapOptions_BeforeCollapse );
      this.tvwMapOptions.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler( this.tvwMapOptions_BeforeSelect );
      // 
      // mapViewer
      // 
      resources.ApplyResources( this.mapViewer, "mapViewer" );
      this.mapViewer.Center = ( (System.Drawing.PointF)( resources.GetObject( "mapViewer.Center" ) ) );
      this.mapViewer.MaxZoom = 20F;
      this.mapViewer.MinZoom = 0.05F;
      this.mapViewer.Name = "mapViewer";
      this.mapViewer.ZoomFactor = 1F;
      this.mapViewer.KeyPress += new System.Windows.Forms.KeyPressEventHandler( this.mapViewer_KeyPress );
      // 
      // picResizeCorner
      // 
      resources.ApplyResources( this.picResizeCorner, "picResizeCorner" );
      this.picResizeCorner.Cursor = System.Windows.Forms.Cursors.SizeNWSE;
      this.picResizeCorner.Image = global::BEGM.Properties.Resources.resize_corner;
      this.picResizeCorner.Name = "picResizeCorner";
      this.picResizeCorner.TabStop = false;
      this.picResizeCorner.MouseDown += new System.Windows.Forms.MouseEventHandler( this.picResizeCorner_MouseDown );
      this.picResizeCorner.MouseUp += new System.Windows.Forms.MouseEventHandler( this.picResizeCorner_MouseUp );
      // 
      // GameMap
      // 
      resources.ApplyResources( this, "$this" );
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.Black;
      this.Controls.Add( this.picResizeCorner );
      this.Controls.Add( this.tvwMapOptions );
      this.Controls.Add( this.cbMapOptions );
      this.Controls.Add( this.lblCell );
      this.Controls.Add( this.lblSupplyLinks );
      this.Controls.Add( this.lblOrigOwner );
      this.Controls.Add( this.picOrigOwner );
      this.Controls.Add( this.lblFacilities );
      this.Controls.Add( this.lblDeployments );
      this.Controls.Add( this.lblFacilitiesHead );
      this.Controls.Add( this.lblDeploymentsHead );
      this.Controls.Add( this.lblSupplyLinksHead );
      this.Controls.Add( this.lblOrigOwnerHead );
      this.Controls.Add( this.lnkCPName );
      this.Controls.Add( this.lblLocationHead );
      this.Controls.Add( this.lnkLocation );
      this.Controls.Add( this.txtActiveCP );
      this.Controls.Add( this.picCPFlag );
      this.Controls.Add( this.lblLatLong );
      this.Controls.Add( this.mapViewer );
      this.ForeColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ) );
      this.Name = "GameMap";
      ( (System.ComponentModel.ISupportInitialize)( this.picCPFlag ) ).EndInit();
      ( (System.ComponentModel.ISupportInitialize)( this.picOrigOwner ) ).EndInit();
      ( (System.ComponentModel.ISupportInitialize)( this.picResizeCorner ) ).EndInit();
      this.ResumeLayout( false );
      this.PerformLayout();

    }

    #endregion

    private MapViewer mapViewer;
    private System.Windows.Forms.Timer tmrUnloadMap;
    private System.Windows.Forms.Label lblLatLong;
    private System.Windows.Forms.PictureBox picCPFlag;
    private System.Windows.Forms.TextBox txtActiveCP;
    private System.Windows.Forms.Timer tmrReveal;
    private System.Windows.Forms.LinkLabel lnkLocation;
    private System.Windows.Forms.Label lblLocationHead;
    private System.Windows.Forms.LinkLabel lnkCPName;
    private System.Windows.Forms.Label lblOrigOwnerHead;
    private System.Windows.Forms.Label lblSupplyLinksHead;
    private System.Windows.Forms.Label lblDeploymentsHead;
    private System.Windows.Forms.Label lblFacilitiesHead;
    private System.Windows.Forms.Label lblDeployments;
    private System.Windows.Forms.Label lblFacilities;
    private System.Windows.Forms.PictureBox picOrigOwner;
    private System.Windows.Forms.Label lblOrigOwner;
    private System.Windows.Forms.Label lblSupplyLinks;
    private System.Windows.Forms.Label lblCell;
    private System.ComponentModel.BackgroundWorker bgwDrawOverlay;
    private System.Windows.Forms.CheckBox cbMapOptions;
    private System.Windows.Forms.TreeView tvwMapOptions;
    private System.Windows.Forms.PictureBox picResizeCorner;
  }
}
