namespace BEGM.Widgets
{
  partial class CurrentAttacks
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager( typeof( CurrentAttacks ) );
      this.picExpand = new System.Windows.Forms.PictureBox();
      this.picCollapse = new System.Windows.Forms.PictureBox();
      this.cbShowAllAlerts = new System.Windows.Forms.CheckBox();
      this.picReset = new System.Windows.Forms.PictureBox();
      this.tmrReveal = new System.Windows.Forms.Timer( this.components );
      this.tmrRemoveChokePoint = new System.Windows.Forms.Timer( this.components );
      this.tskAttacks = new XPExplorerBar.TaskPane();
      ( (System.ComponentModel.ISupportInitialize)( this.picExpand ) ).BeginInit();
      ( (System.ComponentModel.ISupportInitialize)( this.picCollapse ) ).BeginInit();
      ( (System.ComponentModel.ISupportInitialize)( this.picReset ) ).BeginInit();
      ( (System.ComponentModel.ISupportInitialize)( this.tskAttacks ) ).BeginInit();
      this.SuspendLayout();
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
      // cbShowAllAlerts
      // 
      resources.ApplyResources( this.cbShowAllAlerts, "cbShowAllAlerts" );
      this.cbShowAllAlerts.Name = "cbShowAllAlerts";
      this.cbShowAllAlerts.UseVisualStyleBackColor = true;
      this.cbShowAllAlerts.Click += new System.EventHandler( this.cbShowAllAlerts_Click );
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
      // tmrReveal
      // 
      this.tmrReveal.Tick += new System.EventHandler( this.tmrReveal_Tick );
      // 
      // tmrRemoveChokePoint
      // 
      this.tmrRemoveChokePoint.Tick += new System.EventHandler( this.tmrRemoveChokePoint_Tick );
      // 
      // tskAttacks
      // 
      resources.ApplyResources( this.tskAttacks, "tskAttacks" );
      this.tskAttacks.CustomSettings.GradientEndColor = System.Drawing.Color.Black;
      this.tskAttacks.CustomSettings.GradientStartColor = System.Drawing.Color.Black;
      this.tskAttacks.Name = "tskAttacks";
      this.tskAttacks.PreventAutoScroll = false;
      // 
      // CurrentAttacks
      // 
      resources.ApplyResources( this, "$this" );
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.Black;
      this.Controls.Add( this.picReset );
      this.Controls.Add( this.picExpand );
      this.Controls.Add( this.picCollapse );
      this.Controls.Add( this.tskAttacks );
      this.Controls.Add( this.cbShowAllAlerts );
      this.ForeColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ) );
      this.Name = "CurrentAttacks";
      this.MouseEnter += new System.EventHandler( this.CurrentAttacks_MouseEnter );
      ( (System.ComponentModel.ISupportInitialize)( this.picExpand ) ).EndInit();
      ( (System.ComponentModel.ISupportInitialize)( this.picCollapse ) ).EndInit();
      ( (System.ComponentModel.ISupportInitialize)( this.picReset ) ).EndInit();
      ( (System.ComponentModel.ISupportInitialize)( this.tskAttacks ) ).EndInit();
      this.ResumeLayout( false );
      this.PerformLayout();

    }

    #endregion

    private XPExplorerBar.TaskPane tskAttacks;
    private System.Windows.Forms.PictureBox picExpand;
    private System.Windows.Forms.PictureBox picCollapse;
    private System.Windows.Forms.CheckBox cbShowAllAlerts;
    private System.Windows.Forms.PictureBox picReset;
    private System.Windows.Forms.Timer tmrReveal;
    private System.Windows.Forms.Timer tmrRemoveChokePoint;
  }
}
