namespace BEGM.Widgets
{
  partial class RecentEvents
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager( typeof( RecentEvents ) );
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
      this.dgvEventList = new System.Windows.Forms.DataGridView();
      this.ImageCol = new System.Windows.Forms.DataGridViewImageColumn();
      this.TextCol = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.cbCapture = new System.Windows.Forms.CheckBox();
      this.cbFactory = new System.Windows.Forms.CheckBox();
      this.cbHCUnit = new System.Windows.Forms.CheckBox();
      this.lblViewEvents = new System.Windows.Forms.Label();
      this.cbAttackObjective = new System.Windows.Forms.CheckBox();
      this.cbFirebase = new System.Windows.Forms.CheckBox();
      ( (System.ComponentModel.ISupportInitialize)( this.dgvEventList ) ).BeginInit();
      this.SuspendLayout();
      // 
      // dgvEventList
      // 
      this.dgvEventList.AllowUserToAddRows = false;
      this.dgvEventList.AllowUserToDeleteRows = false;
      this.dgvEventList.AllowUserToResizeColumns = false;
      this.dgvEventList.AllowUserToResizeRows = false;
      resources.ApplyResources( this.dgvEventList, "dgvEventList" );
      this.dgvEventList.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
      this.dgvEventList.BackgroundColor = System.Drawing.Color.Black;
      this.dgvEventList.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.dgvEventList.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
      this.dgvEventList.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
      this.dgvEventList.ColumnHeadersVisible = false;
      this.dgvEventList.Columns.AddRange( new System.Windows.Forms.DataGridViewColumn[] {
            this.ImageCol,
            this.TextCol} );
      dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
      dataGridViewCellStyle3.BackColor = System.Drawing.Color.Black;
      dataGridViewCellStyle3.Font = new System.Drawing.Font( "Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ) );
      dataGridViewCellStyle3.ForeColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ) );
      dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.Black;
      dataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ) );
      dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
      this.dgvEventList.DefaultCellStyle = dataGridViewCellStyle3;
      this.dgvEventList.GridColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 64 ) ) ) ), ( (int)( ( (byte)( 64 ) ) ) ), ( (int)( ( (byte)( 64 ) ) ) ) );
      this.dgvEventList.MultiSelect = false;
      this.dgvEventList.Name = "dgvEventList";
      this.dgvEventList.RowHeadersVisible = false;
      this.dgvEventList.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
      this.dgvEventList.StandardTab = true;
      this.dgvEventList.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler( this.dgvEventList_CellDoubleClick );
      this.dgvEventList.RowPrePaint += new System.Windows.Forms.DataGridViewRowPrePaintEventHandler( this.dgvEventList_RowPrePaint );
      this.dgvEventList.MouseEnter += new System.EventHandler( this.dgvEventList_MouseEnter );
      // 
      // ImageCol
      // 
      this.ImageCol.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
      dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopLeft;
      dataGridViewCellStyle1.NullValue = ( (object)( resources.GetObject( "dataGridViewCellStyle1.NullValue" ) ) );
      dataGridViewCellStyle1.Padding = new System.Windows.Forms.Padding( 0, 2, 0, 0 );
      this.ImageCol.DefaultCellStyle = dataGridViewCellStyle1;
      resources.ApplyResources( this.ImageCol, "ImageCol" );
      this.ImageCol.ImageLayout = System.Windows.Forms.DataGridViewImageCellLayout.Zoom;
      this.ImageCol.Name = "ImageCol";
      this.ImageCol.ReadOnly = true;
      this.ImageCol.Resizable = System.Windows.Forms.DataGridViewTriState.False;
      // 
      // TextCol
      // 
      this.TextCol.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
      dataGridViewCellStyle2.Padding = new System.Windows.Forms.Padding( 0, 0, 0, 2 );
      dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
      this.TextCol.DefaultCellStyle = dataGridViewCellStyle2;
      resources.ApplyResources( this.TextCol, "TextCol" );
      this.TextCol.Name = "TextCol";
      this.TextCol.ReadOnly = true;
      this.TextCol.Resizable = System.Windows.Forms.DataGridViewTriState.False;
      this.TextCol.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
      // 
      // cbCapture
      // 
      resources.ApplyResources( this.cbCapture, "cbCapture" );
      this.cbCapture.Checked = true;
      this.cbCapture.CheckState = System.Windows.Forms.CheckState.Checked;
      this.cbCapture.Name = "cbCapture";
      this.cbCapture.UseVisualStyleBackColor = true;
      this.cbCapture.CheckedChanged += new System.EventHandler( this.cbCapture_CheckedChanged );
      // 
      // cbFactory
      // 
      resources.ApplyResources( this.cbFactory, "cbFactory" );
      this.cbFactory.Checked = true;
      this.cbFactory.CheckState = System.Windows.Forms.CheckState.Checked;
      this.cbFactory.Name = "cbFactory";
      this.cbFactory.CheckedChanged += new System.EventHandler( this.cbFactory_CheckedChanged );
      // 
      // cbHCUnit
      // 
      resources.ApplyResources( this.cbHCUnit, "cbHCUnit" );
      this.cbHCUnit.Checked = true;
      this.cbHCUnit.CheckState = System.Windows.Forms.CheckState.Checked;
      this.cbHCUnit.Name = "cbHCUnit";
      this.cbHCUnit.UseVisualStyleBackColor = true;
      this.cbHCUnit.CheckedChanged += new System.EventHandler( this.cbHCUnit_CheckedChanged );
      // 
      // lblViewEvents
      // 
      resources.ApplyResources( this.lblViewEvents, "lblViewEvents" );
      this.lblViewEvents.Name = "lblViewEvents";
      // 
      // cbAttackObjective
      // 
      resources.ApplyResources( this.cbAttackObjective, "cbAttackObjective" );
      this.cbAttackObjective.Checked = true;
      this.cbAttackObjective.CheckState = System.Windows.Forms.CheckState.Checked;
      this.cbAttackObjective.Name = "cbAttackObjective";
      this.cbAttackObjective.UseVisualStyleBackColor = true;
      this.cbAttackObjective.CheckedChanged += new System.EventHandler( this.cbAttackObjective_CheckedChanged );
      // 
      // cbFirebase
      // 
      resources.ApplyResources( this.cbFirebase, "cbFirebase" );
      this.cbFirebase.Checked = true;
      this.cbFirebase.CheckState = System.Windows.Forms.CheckState.Checked;
      this.cbFirebase.Name = "cbFirebase";
      this.cbFirebase.UseVisualStyleBackColor = true;
      this.cbFirebase.CheckedChanged += new System.EventHandler( this.cbFirebase_CheckedChanged );
      // 
      // RecentEvents
      // 
      resources.ApplyResources( this, "$this" );
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.Black;
      this.Controls.Add( this.dgvEventList );
      this.Controls.Add( this.cbFactory );
      this.Controls.Add( this.cbHCUnit );
      this.Controls.Add( this.cbFirebase );
      this.Controls.Add( this.cbAttackObjective );
      this.Controls.Add( this.cbCapture );
      this.Controls.Add( this.lblViewEvents );
      this.ForeColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ) );
      this.Name = "RecentEvents";
      this.MouseEnter += new System.EventHandler( this.RecentEvents_MouseEnter );
      ( (System.ComponentModel.ISupportInitialize)( this.dgvEventList ) ).EndInit();
      this.ResumeLayout( false );
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.DataGridView dgvEventList;
    private System.Windows.Forms.CheckBox cbCapture;
    private System.Windows.Forms.CheckBox cbHCUnit;
    private System.Windows.Forms.Label lblViewEvents;
    private System.Windows.Forms.CheckBox cbAttackObjective;
    private System.Windows.Forms.DataGridViewImageColumn ImageCol;
    private System.Windows.Forms.DataGridViewTextBoxColumn TextCol;
    public System.Windows.Forms.CheckBox cbFactory;
    private System.Windows.Forms.CheckBox cbFirebase;

  }
}
