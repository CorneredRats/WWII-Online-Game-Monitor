namespace BEGM.Widgets
{
  partial class BrigadeStatus
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
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager( typeof( BrigadeStatus ) );
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
      this.dgvHCUnits = new System.Windows.Forms.DataGridView();
      this.lblActiveTitle = new System.Windows.Forms.Label();
      this.picTreeCorps = new System.Windows.Forms.PictureBox();
      this.picTreeDivision = new System.Windows.Forms.PictureBox();
      this.picTreeBrigade = new System.Windows.Forms.PictureBox();
      this.tmrReveal = new System.Windows.Forms.Timer( this.components );
      this.lnkTreeBrigade = new System.Windows.Forms.LinkLabel();
      this.lnkTreeDivision = new System.Windows.Forms.LinkLabel();
      this.lnkTreeCorps = new System.Windows.Forms.LinkLabel();
      this.lnkTreeBranch = new System.Windows.Forms.LinkLabel();
      this.dgvMoves = new System.Windows.Forms.DataGridView();
      this.colTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.colPlayer = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.colFrom = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.colArrow = new System.Windows.Forms.DataGridViewImageColumn();
      this.colTo = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.colSpacer = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.colResult = new System.Windows.Forms.DataGridViewImageColumn();
      this.lblMoves = new System.Windows.Forms.Label();
      this.cbMovesHideFailed = new System.Windows.Forms.CheckBox();
      this.pnlMoves = new System.Windows.Forms.Panel();
      this.picHCUnitInfo = new ImageMap.ImageMap();
      ( (System.ComponentModel.ISupportInitialize)( this.dgvHCUnits ) ).BeginInit();
      ( (System.ComponentModel.ISupportInitialize)( this.picTreeCorps ) ).BeginInit();
      ( (System.ComponentModel.ISupportInitialize)( this.picTreeDivision ) ).BeginInit();
      ( (System.ComponentModel.ISupportInitialize)( this.picTreeBrigade ) ).BeginInit();
      ( (System.ComponentModel.ISupportInitialize)( this.dgvMoves ) ).BeginInit();
      this.pnlMoves.SuspendLayout();
      this.SuspendLayout();
      // 
      // dgvHCUnits
      // 
      this.dgvHCUnits.AllowUserToAddRows = false;
      this.dgvHCUnits.AllowUserToDeleteRows = false;
      this.dgvHCUnits.AllowUserToResizeColumns = false;
      this.dgvHCUnits.AllowUserToResizeRows = false;
      this.dgvHCUnits.BackgroundColor = System.Drawing.Color.Black;
      this.dgvHCUnits.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.dgvHCUnits.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleVertical;
      this.dgvHCUnits.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.dgvHCUnits.ColumnHeadersVisible = false;
      dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.BottomLeft;
      dataGridViewCellStyle1.BackColor = System.Drawing.Color.Black;
      dataGridViewCellStyle1.Font = new System.Drawing.Font( "Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ) );
      dataGridViewCellStyle1.ForeColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ) );
      dataGridViewCellStyle1.Padding = new System.Windows.Forms.Padding( 2, 0, 0, 0 );
      dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 56 ) ) ) ), ( (int)( ( (byte)( 56 ) ) ) ), ( (int)( ( (byte)( 56 ) ) ) ) );
      dataGridViewCellStyle1.SelectionForeColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ) );
      dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
      this.dgvHCUnits.DefaultCellStyle = dataGridViewCellStyle1;
      this.dgvHCUnits.GridColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 49 ) ) ) ), ( (int)( ( (byte)( 72 ) ) ) ), ( (int)( ( (byte)( 60 ) ) ) ) );
      resources.ApplyResources( this.dgvHCUnits, "dgvHCUnits" );
      this.dgvHCUnits.MultiSelect = false;
      this.dgvHCUnits.Name = "dgvHCUnits";
      this.dgvHCUnits.ReadOnly = true;
      this.dgvHCUnits.RowHeadersVisible = false;
      this.dgvHCUnits.RowTemplate.Height = 12;
      this.dgvHCUnits.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
      this.dgvHCUnits.ShowCellToolTips = false;
      this.dgvHCUnits.CellMouseEnter += new System.Windows.Forms.DataGridViewCellEventHandler( this.dgvHCUnits_CellMouseEnter );
      this.dgvHCUnits.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler( this.dgvHCUnits_CellPainting );
      this.dgvHCUnits.SelectionChanged += new System.EventHandler( this.dgvHCUnits_SelectionChanged );
      this.dgvHCUnits.KeyDown += new System.Windows.Forms.KeyEventHandler( this.dgvHCUnits_KeyDown );
      this.dgvHCUnits.MouseEnter += new System.EventHandler( this.dgvHCUnits_MouseEnter );
      this.dgvHCUnits.MouseLeave += new System.EventHandler( this.dgvHCUnits_MouseLeave );
      // 
      // lblActiveTitle
      // 
      this.lblActiveTitle.ForeColor = System.Drawing.Color.DarkGray;
      resources.ApplyResources( this.lblActiveTitle, "lblActiveTitle" );
      this.lblActiveTitle.Name = "lblActiveTitle";
      // 
      // picTreeCorps
      // 
      resources.ApplyResources( this.picTreeCorps, "picTreeCorps" );
      this.picTreeCorps.Name = "picTreeCorps";
      this.picTreeCorps.TabStop = false;
      // 
      // picTreeDivision
      // 
      resources.ApplyResources( this.picTreeDivision, "picTreeDivision" );
      this.picTreeDivision.Name = "picTreeDivision";
      this.picTreeDivision.TabStop = false;
      // 
      // picTreeBrigade
      // 
      resources.ApplyResources( this.picTreeBrigade, "picTreeBrigade" );
      this.picTreeBrigade.Name = "picTreeBrigade";
      this.picTreeBrigade.TabStop = false;
      // 
      // tmrReveal
      // 
      this.tmrReveal.Tick += new System.EventHandler( this.tmrReveal_Tick );
      // 
      // lnkTreeBrigade
      // 
      this.lnkTreeBrigade.ActiveLinkColor = System.Drawing.Color.White;
      resources.ApplyResources( this.lnkTreeBrigade, "lnkTreeBrigade" );
      this.lnkTreeBrigade.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
      this.lnkTreeBrigade.LinkColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ) );
      this.lnkTreeBrigade.Name = "lnkTreeBrigade";
      this.lnkTreeBrigade.TabStop = true;
      this.lnkTreeBrigade.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler( this.lnkTreeBrigade_LinkClicked );
      // 
      // lnkTreeDivision
      // 
      this.lnkTreeDivision.ActiveLinkColor = System.Drawing.Color.White;
      resources.ApplyResources( this.lnkTreeDivision, "lnkTreeDivision" );
      this.lnkTreeDivision.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
      this.lnkTreeDivision.LinkColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ) );
      this.lnkTreeDivision.Name = "lnkTreeDivision";
      this.lnkTreeDivision.TabStop = true;
      this.lnkTreeDivision.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler( this.lnkTreeDivision_LinkClicked );
      // 
      // lnkTreeCorps
      // 
      this.lnkTreeCorps.ActiveLinkColor = System.Drawing.Color.White;
      resources.ApplyResources( this.lnkTreeCorps, "lnkTreeCorps" );
      this.lnkTreeCorps.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
      this.lnkTreeCorps.LinkColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ) );
      this.lnkTreeCorps.Name = "lnkTreeCorps";
      this.lnkTreeCorps.TabStop = true;
      this.lnkTreeCorps.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler( this.lnkTreeCorps_LinkClicked );
      // 
      // lnkTreeBranch
      // 
      this.lnkTreeBranch.ActiveLinkColor = System.Drawing.Color.White;
      resources.ApplyResources( this.lnkTreeBranch, "lnkTreeBranch" );
      this.lnkTreeBranch.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
      this.lnkTreeBranch.LinkColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ) );
      this.lnkTreeBranch.Name = "lnkTreeBranch";
      this.lnkTreeBranch.TabStop = true;
      this.lnkTreeBranch.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler( this.lnkTreeBranch_LinkClicked );
      // 
      // dgvMoves
      // 
      this.dgvMoves.AllowUserToAddRows = false;
      this.dgvMoves.AllowUserToDeleteRows = false;
      this.dgvMoves.AllowUserToResizeColumns = false;
      this.dgvMoves.AllowUserToResizeRows = false;
      this.dgvMoves.BackgroundColor = System.Drawing.Color.Black;
      this.dgvMoves.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.dgvMoves.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
      this.dgvMoves.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.dgvMoves.ColumnHeadersVisible = false;
      this.dgvMoves.Columns.AddRange( new System.Windows.Forms.DataGridViewColumn[] {
            this.colTime,
            this.colPlayer,
            this.colFrom,
            this.colArrow,
            this.colTo,
            this.colSpacer,
            this.colResult} );
      dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
      dataGridViewCellStyle3.BackColor = System.Drawing.Color.Black;
      dataGridViewCellStyle3.Font = new System.Drawing.Font( "Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ) );
      dataGridViewCellStyle3.ForeColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ) );
      dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.Black;
      dataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.Silver;
      dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
      this.dgvMoves.DefaultCellStyle = dataGridViewCellStyle3;
      resources.ApplyResources( this.dgvMoves, "dgvMoves" );
      this.dgvMoves.Name = "dgvMoves";
      this.dgvMoves.ReadOnly = true;
      this.dgvMoves.RowHeadersVisible = false;
      this.dgvMoves.RowTemplate.Height = 16;
      this.dgvMoves.ShowCellToolTips = false;
      this.dgvMoves.CellMouseEnter += new System.Windows.Forms.DataGridViewCellEventHandler( this.dgvMoves_CellMouseEnter );
      this.dgvMoves.CellMouseLeave += new System.Windows.Forms.DataGridViewCellEventHandler( this.dgvMoves_CellMouseLeave );
      this.dgvMoves.MouseEnter += new System.EventHandler( this.dgvMoves_MouseEnter );
      this.dgvMoves.MouseLeave += new System.EventHandler( this.dgvMoves_MouseLeave );
      // 
      // colTime
      // 
      resources.ApplyResources( this.colTime, "colTime" );
      this.colTime.Name = "colTime";
      this.colTime.ReadOnly = true;
      // 
      // colPlayer
      // 
      resources.ApplyResources( this.colPlayer, "colPlayer" );
      this.colPlayer.Name = "colPlayer";
      this.colPlayer.ReadOnly = true;
      // 
      // colFrom
      // 
      resources.ApplyResources( this.colFrom, "colFrom" );
      this.colFrom.Name = "colFrom";
      this.colFrom.ReadOnly = true;
      // 
      // colArrow
      // 
      resources.ApplyResources( this.colArrow, "colArrow" );
      this.colArrow.Name = "colArrow";
      this.colArrow.ReadOnly = true;
      // 
      // colTo
      // 
      resources.ApplyResources( this.colTo, "colTo" );
      this.colTo.Name = "colTo";
      this.colTo.ReadOnly = true;
      // 
      // colSpacer
      // 
      this.colSpacer.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
      resources.ApplyResources( this.colSpacer, "colSpacer" );
      this.colSpacer.Name = "colSpacer";
      this.colSpacer.ReadOnly = true;
      // 
      // colResult
      // 
      dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
      dataGridViewCellStyle2.NullValue = null;
      dataGridViewCellStyle2.Padding = new System.Windows.Forms.Padding( 0, 0, 5, 0 );
      this.colResult.DefaultCellStyle = dataGridViewCellStyle2;
      resources.ApplyResources( this.colResult, "colResult" );
      this.colResult.Name = "colResult";
      this.colResult.ReadOnly = true;
      // 
      // lblMoves
      // 
      resources.ApplyResources( this.lblMoves, "lblMoves" );
      this.lblMoves.Name = "lblMoves";
      // 
      // cbMovesHideFailed
      // 
      resources.ApplyResources( this.cbMovesHideFailed, "cbMovesHideFailed" );
      this.cbMovesHideFailed.Name = "cbMovesHideFailed";
      this.cbMovesHideFailed.UseVisualStyleBackColor = true;
      this.cbMovesHideFailed.Click += new System.EventHandler( this.cbMovesHideFailed_Click );
      // 
      // pnlMoves
      // 
      this.pnlMoves.Controls.Add( this.lblMoves );
      this.pnlMoves.Controls.Add( this.dgvMoves );
      this.pnlMoves.Controls.Add( this.cbMovesHideFailed );
      resources.ApplyResources( this.pnlMoves, "pnlMoves" );
      this.pnlMoves.Name = "pnlMoves";
      // 
      // picHCUnitInfo
      // 
      this.picHCUnitInfo.Image = null;
      resources.ApplyResources( this.picHCUnitInfo, "picHCUnitInfo" );
      this.picHCUnitInfo.Name = "picHCUnitInfo";
      this.picHCUnitInfo.RegionClick += new ImageMap.ImageMap.RegionClickDelegate( this.picHCUnitInfo_RegionClick );
      // 
      // BrigadeStatus
      // 
      resources.ApplyResources( this, "$this" );
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.Black;
      this.Controls.Add( this.pnlMoves );
      this.Controls.Add( this.picHCUnitInfo );
      this.Controls.Add( this.lnkTreeBranch );
      this.Controls.Add( this.lnkTreeCorps );
      this.Controls.Add( this.lnkTreeDivision );
      this.Controls.Add( this.lnkTreeBrigade );
      this.Controls.Add( this.picTreeBrigade );
      this.Controls.Add( this.picTreeDivision );
      this.Controls.Add( this.picTreeCorps );
      this.Controls.Add( this.lblActiveTitle );
      this.Controls.Add( this.dgvHCUnits );
      this.ForeColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ) );
      this.Name = "BrigadeStatus";
      this.MouseEnter += new System.EventHandler( this.BrigadeStatus_MouseEnter );
      ( (System.ComponentModel.ISupportInitialize)( this.dgvHCUnits ) ).EndInit();
      ( (System.ComponentModel.ISupportInitialize)( this.picTreeCorps ) ).EndInit();
      ( (System.ComponentModel.ISupportInitialize)( this.picTreeDivision ) ).EndInit();
      ( (System.ComponentModel.ISupportInitialize)( this.picTreeBrigade ) ).EndInit();
      ( (System.ComponentModel.ISupportInitialize)( this.dgvMoves ) ).EndInit();
      this.pnlMoves.ResumeLayout( false );
      this.pnlMoves.PerformLayout();
      this.ResumeLayout( false );
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.DataGridView dgvHCUnits;
    private System.Windows.Forms.Label lblActiveTitle;
    private System.Windows.Forms.PictureBox picTreeCorps;
    private System.Windows.Forms.PictureBox picTreeDivision;
    private System.Windows.Forms.PictureBox picTreeBrigade;
    private System.Windows.Forms.Timer tmrReveal;
    private System.Windows.Forms.LinkLabel lnkTreeBrigade;
    private System.Windows.Forms.LinkLabel lnkTreeDivision;
    private System.Windows.Forms.LinkLabel lnkTreeCorps;
    private System.Windows.Forms.LinkLabel lnkTreeBranch;
    private ImageMap.ImageMap picHCUnitInfo;
    private System.Windows.Forms.DataGridView dgvMoves;
    private System.Windows.Forms.Label lblMoves;
    private System.Windows.Forms.CheckBox cbMovesHideFailed;
    private System.Windows.Forms.DataGridViewTextBoxColumn colTime;
    private System.Windows.Forms.DataGridViewTextBoxColumn colPlayer;
    private System.Windows.Forms.DataGridViewTextBoxColumn colFrom;
    private System.Windows.Forms.DataGridViewImageColumn colArrow;
    private System.Windows.Forms.DataGridViewTextBoxColumn colTo;
    private System.Windows.Forms.DataGridViewTextBoxColumn colSpacer;
    private System.Windows.Forms.DataGridViewImageColumn colResult;
    private System.Windows.Forms.Panel pnlMoves;
  }
}
