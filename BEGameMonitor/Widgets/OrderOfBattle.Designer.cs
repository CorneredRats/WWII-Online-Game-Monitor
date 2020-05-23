namespace BEGM.Widgets
{
  partial class OrderOfBattle
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager( typeof( OrderOfBattle ) );
      this.picReset = new System.Windows.Forms.PictureBox();
      this.picExpand = new System.Windows.Forms.PictureBox();
      this.picCollapse = new System.Windows.Forms.PictureBox();
      this.lblDelayNote = new System.Windows.Forms.Label();
      this.tmrReveal = new System.Windows.Forms.Timer( this.components );
      this.cmsOrbat = new System.Windows.Forms.ContextMenuStrip( this.components );
      this.miBrigadeStatus = new System.Windows.Forms.ToolStripMenuItem();
      this.miEquipment = new System.Windows.Forms.ToolStripMenuItem();
      this.tvwOrbat = new Aga.Controls.Tree.TreeViewAdv();
      this.tvwOrbatCol1 = new Aga.Controls.Tree.TreeColumn();
      this.tvwOrbatCol2 = new Aga.Controls.Tree.TreeColumn();
      this.tvwOrbatCol3 = new Aga.Controls.Tree.TreeColumn();
      this.tvwOrbatTxtTitle = new Aga.Controls.Tree.NodeControls.NodeTextBox();
      this.tvwOrbatTxtChokePoint = new Aga.Controls.Tree.NodeControls.NodeTextBox();
      this.tvwOrbatIconTimer = new Aga.Controls.Tree.NodeControls.NodeIcon();
      ( (System.ComponentModel.ISupportInitialize)( this.picReset ) ).BeginInit();
      ( (System.ComponentModel.ISupportInitialize)( this.picExpand ) ).BeginInit();
      ( (System.ComponentModel.ISupportInitialize)( this.picCollapse ) ).BeginInit();
      this.cmsOrbat.SuspendLayout();
      this.SuspendLayout();
      // 
      // picReset
      // 
      this.picReset.Cursor = System.Windows.Forms.Cursors.Hand;
      this.picReset.Image = global::BEGM.Properties.Resources.icon_reset;
      resources.ApplyResources( this.picReset, "picReset" );
      this.picReset.Name = "picReset";
      this.picReset.TabStop = false;
      this.picReset.MouseClick += new System.Windows.Forms.MouseEventHandler( this.picReset_MouseClick );
      // 
      // picExpand
      // 
      this.picExpand.Cursor = System.Windows.Forms.Cursors.Hand;
      this.picExpand.Image = global::BEGM.Properties.Resources.icon_plus;
      resources.ApplyResources( this.picExpand, "picExpand" );
      this.picExpand.Name = "picExpand";
      this.picExpand.TabStop = false;
      this.picExpand.MouseClick += new System.Windows.Forms.MouseEventHandler( this.picExpand_MouseClick );
      // 
      // picCollapse
      // 
      this.picCollapse.Cursor = System.Windows.Forms.Cursors.Hand;
      this.picCollapse.Image = global::BEGM.Properties.Resources.icon_minus;
      resources.ApplyResources( this.picCollapse, "picCollapse" );
      this.picCollapse.Name = "picCollapse";
      this.picCollapse.TabStop = false;
      this.picCollapse.MouseClick += new System.Windows.Forms.MouseEventHandler( this.picCollapse_MouseClick );
      // 
      // lblDelayNote
      // 
      resources.ApplyResources( this.lblDelayNote, "lblDelayNote" );
      this.lblDelayNote.ForeColor = System.Drawing.Color.Gray;
      this.lblDelayNote.Name = "lblDelayNote";
      // 
      // tmrReveal
      // 
      this.tmrReveal.Tick += new System.EventHandler( this.tmrReveal_Tick );
      // 
      // cmsOrbat
      // 
      this.cmsOrbat.BackColor = System.Drawing.Color.Black;
      this.cmsOrbat.Items.AddRange( new System.Windows.Forms.ToolStripItem[] {
            this.miBrigadeStatus,
            this.miEquipment} );
      this.cmsOrbat.Name = "cmsOrbat";
      this.cmsOrbat.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
      this.cmsOrbat.ShowImageMargin = false;
      resources.ApplyResources( this.cmsOrbat, "cmsOrbat" );
      this.cmsOrbat.Opening += new System.ComponentModel.CancelEventHandler( this.cmsOrbat_Opening );
      // 
      // miBrigadeStatus
      // 
      this.miBrigadeStatus.ForeColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ) );
      this.miBrigadeStatus.Name = "miBrigadeStatus";
      resources.ApplyResources( this.miBrigadeStatus, "miBrigadeStatus" );
      this.miBrigadeStatus.Click += new System.EventHandler( this.miBrigadeStatus_Click );
      // 
      // miEquipment
      // 
      this.miEquipment.ForeColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ) );
      this.miEquipment.Name = "miEquipment";
      resources.ApplyResources( this.miEquipment, "miEquipment" );
      this.miEquipment.Click += new System.EventHandler( this.miEquipment_Click );
      // 
      // tvwOrbat
      // 
      resources.ApplyResources( this.tvwOrbat, "tvwOrbat" );
      this.tvwOrbat.BackColor = System.Drawing.Color.Black;
      this.tvwOrbat.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.tvwOrbat.Columns.Add( this.tvwOrbatCol1 );
      this.tvwOrbat.Columns.Add( this.tvwOrbatCol2 );
      this.tvwOrbat.Columns.Add( this.tvwOrbatCol3 );
      this.tvwOrbat.ContextMenuStrip = this.cmsOrbat;
      this.tvwOrbat.Cursor = System.Windows.Forms.Cursors.Default;
      this.tvwOrbat.DefaultToolTipProvider = null;
      this.tvwOrbat.DragDropMarkColor = System.Drawing.Color.Black;
      this.tvwOrbat.FullRowSelect = true;
      this.tvwOrbat.Indent = 10;
      this.tvwOrbat.LineColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 49 ) ) ) ), ( (int)( ( (byte)( 72 ) ) ) ), ( (int)( ( (byte)( 60 ) ) ) ) );
      this.tvwOrbat.Model = null;
      this.tvwOrbat.Name = "tvwOrbat";
      this.tvwOrbat.NodeControls.Add( this.tvwOrbatTxtTitle );
      this.tvwOrbat.NodeControls.Add( this.tvwOrbatTxtChokePoint );
      this.tvwOrbat.NodeControls.Add( this.tvwOrbatIconTimer );
      this.tvwOrbat.Search.BackColor = System.Drawing.Color.Pink;
      this.tvwOrbat.Search.FontColor = System.Drawing.Color.Black;
      this.tvwOrbat.SelectedNode = null;
      this.tvwOrbat.ShowNodeToolTips = true;
      this.tvwOrbat.ShowPlusMinus = false;
      this.tvwOrbat.UseColumns = true;
      this.tvwOrbat.MouseEnter += new System.EventHandler( this.tvwOrbat_MouseEnter );
      // 
      // tvwOrbatCol1
      // 
      resources.ApplyResources( this.tvwOrbatCol1, "tvwOrbatCol1" );
      this.tvwOrbatCol1.SortOrder = System.Windows.Forms.SortOrder.None;
      // 
      // tvwOrbatCol2
      // 
      resources.ApplyResources( this.tvwOrbatCol2, "tvwOrbatCol2" );
      this.tvwOrbatCol2.SortOrder = System.Windows.Forms.SortOrder.None;
      // 
      // tvwOrbatCol3
      // 
      resources.ApplyResources( this.tvwOrbatCol3, "tvwOrbatCol3" );
      this.tvwOrbatCol3.SortOrder = System.Windows.Forms.SortOrder.None;
      // 
      // tvwOrbatTxtTitle
      // 
      this.tvwOrbatTxtTitle.DataPropertyName = "Text";
      this.tvwOrbatTxtTitle.ParentColumn = this.tvwOrbatCol1;
      this.tvwOrbatTxtTitle.Trimming = System.Drawing.StringTrimming.EllipsisCharacter;
      // 
      // tvwOrbatTxtChokePoint
      // 
      this.tvwOrbatTxtChokePoint.DataPropertyName = "ChokePoint";
      this.tvwOrbatTxtChokePoint.ParentColumn = this.tvwOrbatCol2;
      this.tvwOrbatTxtChokePoint.Trimming = System.Drawing.StringTrimming.EllipsisCharacter;
      // 
      // tvwOrbatIconTimer
      // 
      this.tvwOrbatIconTimer.DataPropertyName = "Timer";
      this.tvwOrbatIconTimer.IncrementalSearchEnabled = false;
      this.tvwOrbatIconTimer.ParentColumn = this.tvwOrbatCol3;
      this.tvwOrbatIconTimer.VerticalAlign = Aga.Controls.Tree.VerticalAlignment.Top;
      // 
      // OrderOfBattle
      // 
      resources.ApplyResources( this, "$this" );
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.Black;
      this.Controls.Add( this.lblDelayNote );
      this.Controls.Add( this.tvwOrbat );
      this.Controls.Add( this.picReset );
      this.Controls.Add( this.picExpand );
      this.Controls.Add( this.picCollapse );
      this.ForeColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ) );
      this.Name = "OrderOfBattle";
      ( (System.ComponentModel.ISupportInitialize)( this.picReset ) ).EndInit();
      ( (System.ComponentModel.ISupportInitialize)( this.picExpand ) ).EndInit();
      ( (System.ComponentModel.ISupportInitialize)( this.picCollapse ) ).EndInit();
      this.cmsOrbat.ResumeLayout( false );
      this.ResumeLayout( false );
      this.PerformLayout();

    }

    #endregion

    private Aga.Controls.Tree.TreeViewAdv tvwOrbat;
    private System.Windows.Forms.PictureBox picReset;
    private System.Windows.Forms.PictureBox picExpand;
    private System.Windows.Forms.PictureBox picCollapse;
    private Aga.Controls.Tree.TreeColumn tvwOrbatCol1;
    private Aga.Controls.Tree.TreeColumn tvwOrbatCol2;
    private Aga.Controls.Tree.NodeControls.NodeTextBox tvwOrbatTxtTitle;
    private Aga.Controls.Tree.NodeControls.NodeTextBox tvwOrbatTxtChokePoint;
    private System.Windows.Forms.Label lblDelayNote;
    private System.Windows.Forms.Timer tmrReveal;
    private Aga.Controls.Tree.TreeColumn tvwOrbatCol3;
    private Aga.Controls.Tree.NodeControls.NodeIcon tvwOrbatIconTimer;
    private System.Windows.Forms.ContextMenuStrip cmsOrbat;
    private System.Windows.Forms.ToolStripMenuItem miBrigadeStatus;
    private System.Windows.Forms.ToolStripMenuItem miEquipment;
  }
}
