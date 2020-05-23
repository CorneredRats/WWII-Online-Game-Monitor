namespace BEGM.Widgets
{
  partial class Equipment
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager( typeof( Equipment ) );
      this.lblColHeader5 = new System.Windows.Forms.Label();
      this.lblColHeader4 = new System.Windows.Forms.Label();
      this.lblColHeader3 = new System.Windows.Forms.Label();
      this.lblColHeader2 = new System.Windows.Forms.Label();
      this.lblColHeader1 = new System.Windows.Forms.Label();
      this.lblColHeader0 = new System.Windows.Forms.Label();
      this.picReset = new System.Windows.Forms.PictureBox();
      this.picExpand = new System.Windows.Forms.PictureBox();
      this.picCollapse = new System.Windows.Forms.PictureBox();
      this.picCycleNext = new System.Windows.Forms.PictureBox();
      this.picCyclePrev = new System.Windows.Forms.PictureBox();
      this.lblVehicleName = new System.Windows.Forms.Label();
      this.lblVehicleClass = new System.Windows.Forms.Label();
      this.picVehicle = new System.Windows.Forms.PictureBox();
      this.lblSupply = new System.Windows.Forms.Label();
      this.picVehicleCountry = new System.Windows.Forms.PictureBox();
      this.pnlSupply = new System.Windows.Forms.Panel();
      this.picSupply = new ImageMap.ImageMap();
      this.lblSupplyTotalHead = new System.Windows.Forms.Label();
      this.lblSupplyTotal = new System.Windows.Forms.Label();
      this.numSupplyCycle = new System.Windows.Forms.NumericUpDown();
      this.tmrReveal = new System.Windows.Forms.Timer( this.components );
      this.tvwEquip = new Aga.Controls.Tree.TreeViewAdv();
      this.tvwEquipCol0 = new Aga.Controls.Tree.TreeColumn();
      this.tvwEquipCol1 = new Aga.Controls.Tree.TreeColumn();
      this.tvwEquipCol2 = new Aga.Controls.Tree.TreeColumn();
      this.tvwEquipCol3 = new Aga.Controls.Tree.TreeColumn();
      this.tvwEquipCol4 = new Aga.Controls.Tree.TreeColumn();
      this.tvwEquipCol5 = new Aga.Controls.Tree.TreeColumn();
      this.tvwEquipTxtName = new Aga.Controls.Tree.NodeControls.NodeTextBox();
      this.tvwEquipTxtNum1 = new Aga.Controls.Tree.NodeControls.NodeTextBox();
      this.tvwEquipTxtNum2 = new Aga.Controls.Tree.NodeControls.NodeTextBox();
      this.tvwEquipTxtNum3 = new Aga.Controls.Tree.NodeControls.NodeTextBox();
      this.tvwEquipTxtNum4 = new Aga.Controls.Tree.NodeControls.NodeTextBox();
      this.tvwEquipTxtNum5 = new Aga.Controls.Tree.NodeControls.NodeTextBox();
      this.lblStartTip = new System.Windows.Forms.Label();
      ( (System.ComponentModel.ISupportInitialize)( this.picReset ) ).BeginInit();
      ( (System.ComponentModel.ISupportInitialize)( this.picExpand ) ).BeginInit();
      ( (System.ComponentModel.ISupportInitialize)( this.picCollapse ) ).BeginInit();
      ( (System.ComponentModel.ISupportInitialize)( this.picCycleNext ) ).BeginInit();
      ( (System.ComponentModel.ISupportInitialize)( this.picCyclePrev ) ).BeginInit();
      ( (System.ComponentModel.ISupportInitialize)( this.picVehicle ) ).BeginInit();
      ( (System.ComponentModel.ISupportInitialize)( this.picVehicleCountry ) ).BeginInit();
      this.pnlSupply.SuspendLayout();
      ( (System.ComponentModel.ISupportInitialize)( this.numSupplyCycle ) ).BeginInit();
      this.SuspendLayout();
      // 
      // lblColHeader5
      // 
      resources.ApplyResources( this.lblColHeader5, "lblColHeader5" );
      this.lblColHeader5.Name = "lblColHeader5";
      // 
      // lblColHeader4
      // 
      resources.ApplyResources( this.lblColHeader4, "lblColHeader4" );
      this.lblColHeader4.Name = "lblColHeader4";
      // 
      // lblColHeader3
      // 
      resources.ApplyResources( this.lblColHeader3, "lblColHeader3" );
      this.lblColHeader3.Name = "lblColHeader3";
      // 
      // lblColHeader2
      // 
      resources.ApplyResources( this.lblColHeader2, "lblColHeader2" );
      this.lblColHeader2.Name = "lblColHeader2";
      // 
      // lblColHeader1
      // 
      resources.ApplyResources( this.lblColHeader1, "lblColHeader1" );
      this.lblColHeader1.Name = "lblColHeader1";
      // 
      // lblColHeader0
      // 
      resources.ApplyResources( this.lblColHeader0, "lblColHeader0" );
      this.lblColHeader0.Name = "lblColHeader0";
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
      // picCycleNext
      // 
      this.picCycleNext.Cursor = System.Windows.Forms.Cursors.Hand;
      this.picCycleNext.Image = global::BEGM.Properties.Resources.icon_right;
      resources.ApplyResources( this.picCycleNext, "picCycleNext" );
      this.picCycleNext.Name = "picCycleNext";
      this.picCycleNext.TabStop = false;
      this.picCycleNext.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler( this.picCycleNext_MouseDoubleClick );
      this.picCycleNext.MouseClick += new System.Windows.Forms.MouseEventHandler( this.picCycleNext_MouseClick );
      // 
      // picCyclePrev
      // 
      this.picCyclePrev.Cursor = System.Windows.Forms.Cursors.Hand;
      this.picCyclePrev.Image = global::BEGM.Properties.Resources.icon_left;
      resources.ApplyResources( this.picCyclePrev, "picCyclePrev" );
      this.picCyclePrev.Name = "picCyclePrev";
      this.picCyclePrev.TabStop = false;
      this.picCyclePrev.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler( this.picCyclePrev_MouseDoubleClick );
      this.picCyclePrev.MouseClick += new System.Windows.Forms.MouseEventHandler( this.picCyclePrev_MouseClick );
      // 
      // lblVehicleName
      // 
      resources.ApplyResources( this.lblVehicleName, "lblVehicleName" );
      this.lblVehicleName.Name = "lblVehicleName";
      // 
      // lblVehicleClass
      // 
      resources.ApplyResources( this.lblVehicleClass, "lblVehicleClass" );
      this.lblVehicleClass.Name = "lblVehicleClass";
      // 
      // picVehicle
      // 
      resources.ApplyResources( this.picVehicle, "picVehicle" );
      this.picVehicle.Name = "picVehicle";
      this.picVehicle.TabStop = false;
      // 
      // lblSupply
      // 
      resources.ApplyResources( this.lblSupply, "lblSupply" );
      this.lblSupply.Name = "lblSupply";
      // 
      // picVehicleCountry
      // 
      resources.ApplyResources( this.picVehicleCountry, "picVehicleCountry" );
      this.picVehicleCountry.Name = "picVehicleCountry";
      this.picVehicleCountry.TabStop = false;
      // 
      // pnlSupply
      // 
      resources.ApplyResources( this.pnlSupply, "pnlSupply" );
      this.pnlSupply.Controls.Add( this.picSupply );
      this.pnlSupply.Name = "pnlSupply";
      this.pnlSupply.MouseLeave += new System.EventHandler( this.pnlSupply_MouseLeave );
      this.pnlSupply.MouseEnter += new System.EventHandler( this.pnlSupply_MouseEnter );
      // 
      // picSupply
      // 
      resources.ApplyResources( this.picSupply, "picSupply" );
      this.picSupply.Image = null;
      this.picSupply.Name = "picSupply";
      this.picSupply.MyMouseLeave += new System.EventHandler( this.picSupply_MyMouseLeave );
      this.picSupply.MyMouseEnter += new System.EventHandler( this.picSupply_MyMouseEnter );
      this.picSupply.RegionClick += new ImageMap.ImageMap.RegionClickDelegate( this.picSupply_RegionClick );
      // 
      // lblSupplyTotalHead
      // 
      resources.ApplyResources( this.lblSupplyTotalHead, "lblSupplyTotalHead" );
      this.lblSupplyTotalHead.Name = "lblSupplyTotalHead";
      // 
      // lblSupplyTotal
      // 
      resources.ApplyResources( this.lblSupplyTotal, "lblSupplyTotal" );
      this.lblSupplyTotal.Name = "lblSupplyTotal";
      // 
      // numSupplyCycle
      // 
      this.numSupplyCycle.BackColor = System.Drawing.Color.Black;
      this.numSupplyCycle.BorderStyle = System.Windows.Forms.BorderStyle.None;
      resources.ApplyResources( this.numSupplyCycle, "numSupplyCycle" );
      this.numSupplyCycle.ForeColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ) );
      this.numSupplyCycle.Name = "numSupplyCycle";
      this.numSupplyCycle.ReadOnly = true;
      this.numSupplyCycle.ValueChanged += new System.EventHandler( this.numSupplyCycle_ValueChanged );
      this.numSupplyCycle.Enter += new System.EventHandler( this.numSupplyCycle_Enter );
      // 
      // tmrReveal
      // 
      this.tmrReveal.Tick += new System.EventHandler( this.tmrReveal_Tick );
      // 
      // tvwEquip
      // 
      resources.ApplyResources( this.tvwEquip, "tvwEquip" );
      this.tvwEquip.BackColor = System.Drawing.Color.Black;
      this.tvwEquip.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.tvwEquip.Columns.Add( this.tvwEquipCol0 );
      this.tvwEquip.Columns.Add( this.tvwEquipCol1 );
      this.tvwEquip.Columns.Add( this.tvwEquipCol2 );
      this.tvwEquip.Columns.Add( this.tvwEquipCol3 );
      this.tvwEquip.Columns.Add( this.tvwEquipCol4 );
      this.tvwEquip.Columns.Add( this.tvwEquipCol5 );
      this.tvwEquip.Cursor = System.Windows.Forms.Cursors.Default;
      this.tvwEquip.DefaultToolTipProvider = null;
      this.tvwEquip.DragDropMarkColor = System.Drawing.Color.Black;
      this.tvwEquip.FullRowSelect = true;
      this.tvwEquip.Indent = 10;
      this.tvwEquip.LineColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 49 ) ) ) ), ( (int)( ( (byte)( 72 ) ) ) ), ( (int)( ( (byte)( 60 ) ) ) ) );
      this.tvwEquip.Model = null;
      this.tvwEquip.Name = "tvwEquip";
      this.tvwEquip.NodeControls.Add( this.tvwEquipTxtName );
      this.tvwEquip.NodeControls.Add( this.tvwEquipTxtNum1 );
      this.tvwEquip.NodeControls.Add( this.tvwEquipTxtNum2 );
      this.tvwEquip.NodeControls.Add( this.tvwEquipTxtNum3 );
      this.tvwEquip.NodeControls.Add( this.tvwEquipTxtNum4 );
      this.tvwEquip.NodeControls.Add( this.tvwEquipTxtNum5 );
      this.tvwEquip.Search.BackColor = System.Drawing.Color.Pink;
      this.tvwEquip.Search.FontColor = System.Drawing.Color.Black;
      this.tvwEquip.SelectedNode = null;
      this.tvwEquip.ShowNodeToolTips = true;
      this.tvwEquip.ShowPlusMinus = false;
      this.tvwEquip.UseColumns = true;
      this.tvwEquip.SelectionChanged += new System.EventHandler( this.tvwEquip_SelectionChanged );
      this.tvwEquip.MouseEnter += new System.EventHandler( this.tvwEquip_MouseEnter );
      this.tvwEquip.MouseLeave += new System.EventHandler( this.tvwEquip_MouseLeave );
      // 
      // tvwEquipCol0
      // 
      resources.ApplyResources( this.tvwEquipCol0, "tvwEquipCol0" );
      this.tvwEquipCol0.SortOrder = System.Windows.Forms.SortOrder.None;
      this.tvwEquipCol0.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      // 
      // tvwEquipCol1
      // 
      resources.ApplyResources( this.tvwEquipCol1, "tvwEquipCol1" );
      this.tvwEquipCol1.SortOrder = System.Windows.Forms.SortOrder.None;
      this.tvwEquipCol1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      // 
      // tvwEquipCol2
      // 
      resources.ApplyResources( this.tvwEquipCol2, "tvwEquipCol2" );
      this.tvwEquipCol2.SortOrder = System.Windows.Forms.SortOrder.None;
      this.tvwEquipCol2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      // 
      // tvwEquipCol3
      // 
      resources.ApplyResources( this.tvwEquipCol3, "tvwEquipCol3" );
      this.tvwEquipCol3.SortOrder = System.Windows.Forms.SortOrder.None;
      this.tvwEquipCol3.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      // 
      // tvwEquipCol4
      // 
      resources.ApplyResources( this.tvwEquipCol4, "tvwEquipCol4" );
      this.tvwEquipCol4.SortOrder = System.Windows.Forms.SortOrder.None;
      this.tvwEquipCol4.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      // 
      // tvwEquipCol5
      // 
      resources.ApplyResources( this.tvwEquipCol5, "tvwEquipCol5" );
      this.tvwEquipCol5.SortOrder = System.Windows.Forms.SortOrder.None;
      this.tvwEquipCol5.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      // 
      // tvwEquipTxtName
      // 
      this.tvwEquipTxtName.DataPropertyName = "Text";
      this.tvwEquipTxtName.ParentColumn = this.tvwEquipCol0;
      this.tvwEquipTxtName.Trimming = System.Drawing.StringTrimming.EllipsisCharacter;
      // 
      // tvwEquipTxtNum1
      // 
      this.tvwEquipTxtNum1.DataPropertyName = "Col1";
      this.tvwEquipTxtNum1.ParentColumn = this.tvwEquipCol1;
      this.tvwEquipTxtNum1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      // 
      // tvwEquipTxtNum2
      // 
      this.tvwEquipTxtNum2.DataPropertyName = "Col2";
      this.tvwEquipTxtNum2.ParentColumn = this.tvwEquipCol2;
      this.tvwEquipTxtNum2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      // 
      // tvwEquipTxtNum3
      // 
      this.tvwEquipTxtNum3.DataPropertyName = "Col3";
      this.tvwEquipTxtNum3.ParentColumn = this.tvwEquipCol3;
      this.tvwEquipTxtNum3.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      // 
      // tvwEquipTxtNum4
      // 
      this.tvwEquipTxtNum4.DataPropertyName = "Col4";
      this.tvwEquipTxtNum4.ParentColumn = this.tvwEquipCol4;
      this.tvwEquipTxtNum4.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      // 
      // tvwEquipTxtNum5
      // 
      this.tvwEquipTxtNum5.DataPropertyName = "Col5";
      this.tvwEquipTxtNum5.ParentColumn = this.tvwEquipCol5;
      this.tvwEquipTxtNum5.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      // 
      // lblStartTip
      // 
      this.lblStartTip.ForeColor = System.Drawing.Color.Gray;
      resources.ApplyResources( this.lblStartTip, "lblStartTip" );
      this.lblStartTip.Name = "lblStartTip";
      // 
      // Equipment
      // 
      resources.ApplyResources( this, "$this" );
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.Black;
      this.Controls.Add( this.lblStartTip );
      this.Controls.Add( this.lblSupplyTotal );
      this.Controls.Add( this.lblSupplyTotalHead );
      this.Controls.Add( this.pnlSupply );
      this.Controls.Add( this.picVehicleCountry );
      this.Controls.Add( this.lblVehicleName );
      this.Controls.Add( this.lblVehicleClass );
      this.Controls.Add( this.picCycleNext );
      this.Controls.Add( this.picCyclePrev );
      this.Controls.Add( this.picReset );
      this.Controls.Add( this.picExpand );
      this.Controls.Add( this.picCollapse );
      this.Controls.Add( this.lblColHeader0 );
      this.Controls.Add( this.lblColHeader1 );
      this.Controls.Add( this.lblColHeader2 );
      this.Controls.Add( this.lblColHeader3 );
      this.Controls.Add( this.lblColHeader4 );
      this.Controls.Add( this.lblColHeader5 );
      this.Controls.Add( this.tvwEquip );
      this.Controls.Add( this.picVehicle );
      this.Controls.Add( this.numSupplyCycle );
      this.Controls.Add( this.lblSupply );
      this.ForeColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ) );
      this.Name = "Equipment";
      this.MouseEnter += new System.EventHandler( this.Equipment_MouseEnter );
      ( (System.ComponentModel.ISupportInitialize)( this.picReset ) ).EndInit();
      ( (System.ComponentModel.ISupportInitialize)( this.picExpand ) ).EndInit();
      ( (System.ComponentModel.ISupportInitialize)( this.picCollapse ) ).EndInit();
      ( (System.ComponentModel.ISupportInitialize)( this.picCycleNext ) ).EndInit();
      ( (System.ComponentModel.ISupportInitialize)( this.picCyclePrev ) ).EndInit();
      ( (System.ComponentModel.ISupportInitialize)( this.picVehicle ) ).EndInit();
      ( (System.ComponentModel.ISupportInitialize)( this.picVehicleCountry ) ).EndInit();
      this.pnlSupply.ResumeLayout( false );
      ( (System.ComponentModel.ISupportInitialize)( this.numSupplyCycle ) ).EndInit();
      this.ResumeLayout( false );
      this.PerformLayout();

    }

    #endregion

    private Aga.Controls.Tree.TreeViewAdv tvwEquip;
    private Aga.Controls.Tree.TreeColumn tvwEquipCol0;
    private Aga.Controls.Tree.TreeColumn tvwEquipCol1;
    private Aga.Controls.Tree.TreeColumn tvwEquipCol2;
    private Aga.Controls.Tree.TreeColumn tvwEquipCol3;
    private Aga.Controls.Tree.TreeColumn tvwEquipCol4;
    private Aga.Controls.Tree.NodeControls.NodeTextBox tvwEquipTxtName;
    private Aga.Controls.Tree.NodeControls.NodeTextBox tvwEquipTxtNum1;
    private Aga.Controls.Tree.NodeControls.NodeTextBox tvwEquipTxtNum2;
    private Aga.Controls.Tree.NodeControls.NodeTextBox tvwEquipTxtNum3;
    private Aga.Controls.Tree.NodeControls.NodeTextBox tvwEquipTxtNum4;
    private Aga.Controls.Tree.TreeColumn tvwEquipCol5;
    private Aga.Controls.Tree.NodeControls.NodeTextBox tvwEquipTxtNum5;
    private System.Windows.Forms.Label lblColHeader5;
    private System.Windows.Forms.Label lblColHeader4;
    private System.Windows.Forms.Label lblColHeader3;
    private System.Windows.Forms.Label lblColHeader2;
    private System.Windows.Forms.Label lblColHeader1;
    private System.Windows.Forms.Label lblColHeader0;
    private System.Windows.Forms.PictureBox picReset;
    private System.Windows.Forms.PictureBox picExpand;
    private System.Windows.Forms.PictureBox picCollapse;
    private System.Windows.Forms.PictureBox picCycleNext;
    private System.Windows.Forms.PictureBox picCyclePrev;
    private System.Windows.Forms.Label lblVehicleName;
    private System.Windows.Forms.Label lblVehicleClass;
    private System.Windows.Forms.PictureBox picVehicle;
    private System.Windows.Forms.Label lblSupply;
    private System.Windows.Forms.PictureBox picVehicleCountry;
    private System.Windows.Forms.Panel pnlSupply;
    private System.Windows.Forms.Label lblSupplyTotalHead;
    private System.Windows.Forms.Label lblSupplyTotal;
    private ImageMap.ImageMap picSupply;
    private System.Windows.Forms.NumericUpDown numSupplyCycle;
    private System.Windows.Forms.Timer tmrReveal;
    private System.Windows.Forms.Label lblStartTip;

  }
}
