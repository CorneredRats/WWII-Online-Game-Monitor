namespace BEGM.Widgets
{
  partial class FactoryStatus
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager( typeof( FactoryStatus ) );
      this.pnlLoadFactoryData = new System.Windows.Forms.Panel();
      this.cbAlwaysLoad = new System.Windows.Forms.CheckBox();
      this.lblAlwaysLoad = new System.Windows.Forms.Label();
      this.btnLoadFactoryData = new System.Windows.Forms.Button();
      this.picSummary = new System.Windows.Forms.PictureBox();
      this.tmrReveal = new System.Windows.Forms.Timer( this.components );
      this.tskFactory = new XPExplorerBar.TaskPane();
      this.expBritish = new XPExplorerBar.Expando();
      this.picBritish = new ImageMap.ImageMap();
      this.expFrench = new XPExplorerBar.Expando();
      this.picFrench = new ImageMap.ImageMap();
      this.expGerman = new XPExplorerBar.Expando();
      this.picGerman = new ImageMap.ImageMap();
      this.pnlLoadFactoryData.SuspendLayout();
      ( (System.ComponentModel.ISupportInitialize)( this.picSummary ) ).BeginInit();
      ( (System.ComponentModel.ISupportInitialize)( this.tskFactory ) ).BeginInit();
      this.tskFactory.SuspendLayout();
      ( (System.ComponentModel.ISupportInitialize)( this.expBritish ) ).BeginInit();
      this.expBritish.SuspendLayout();
      ( (System.ComponentModel.ISupportInitialize)( this.expFrench ) ).BeginInit();
      this.expFrench.SuspendLayout();
      ( (System.ComponentModel.ISupportInitialize)( this.expGerman ) ).BeginInit();
      this.expGerman.SuspendLayout();
      this.SuspendLayout();
      // 
      // pnlLoadFactoryData
      // 
      this.pnlLoadFactoryData.Controls.Add( this.cbAlwaysLoad );
      this.pnlLoadFactoryData.Controls.Add( this.lblAlwaysLoad );
      this.pnlLoadFactoryData.Controls.Add( this.btnLoadFactoryData );
      resources.ApplyResources( this.pnlLoadFactoryData, "pnlLoadFactoryData" );
      this.pnlLoadFactoryData.Name = "pnlLoadFactoryData";
      // 
      // cbAlwaysLoad
      // 
      resources.ApplyResources( this.cbAlwaysLoad, "cbAlwaysLoad" );
      this.cbAlwaysLoad.Name = "cbAlwaysLoad";
      this.cbAlwaysLoad.UseVisualStyleBackColor = true;
      // 
      // lblAlwaysLoad
      // 
      resources.ApplyResources( this.lblAlwaysLoad, "lblAlwaysLoad" );
      this.lblAlwaysLoad.Name = "lblAlwaysLoad";
      // 
      // btnLoadFactoryData
      // 
      this.btnLoadFactoryData.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 64 ) ) ) ), ( (int)( ( (byte)( 64 ) ) ) ), ( (int)( ( (byte)( 64 ) ) ) ) );
      this.btnLoadFactoryData.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Black;
      this.btnLoadFactoryData.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 32 ) ) ) ), ( (int)( ( (byte)( 32 ) ) ) ), ( (int)( ( (byte)( 32 ) ) ) ) );
      resources.ApplyResources( this.btnLoadFactoryData, "btnLoadFactoryData" );
      this.btnLoadFactoryData.Name = "btnLoadFactoryData";
      this.btnLoadFactoryData.TabStop = false;
      this.btnLoadFactoryData.UseVisualStyleBackColor = true;
      this.btnLoadFactoryData.Click += new System.EventHandler( this.btnLoadFactoryData_Click );
      // 
      // picSummary
      // 
      resources.ApplyResources( this.picSummary, "picSummary" );
      this.picSummary.Name = "picSummary";
      this.picSummary.TabStop = false;
      // 
      // tmrReveal
      // 
      this.tmrReveal.Tick += new System.EventHandler( this.tmrReveal_Tick );
      // 
      // tskFactory
      // 
      resources.ApplyResources( this.tskFactory, "tskFactory" );
      this.tskFactory.CustomSettings.GradientEndColor = System.Drawing.Color.Black;
      this.tskFactory.CustomSettings.GradientStartColor = System.Drawing.Color.Black;
      this.tskFactory.Expandos.AddRange( new XPExplorerBar.Expando[] {
            this.expBritish,
            this.expFrench,
            this.expGerman} );
      this.tskFactory.Name = "tskFactory";
      this.tskFactory.PreventAutoScroll = false;
      this.tskFactory.MouseEnter += new System.EventHandler( this.tskFactory_MouseEnter );
      // 
      // expBritish
      // 
      resources.ApplyResources( this.expBritish, "expBritish" );
      this.expBritish.Animate = true;
      this.expBritish.CustomHeaderSettings.BackImageHeight = 25;
      this.expBritish.CustomHeaderSettings.NormalGradientEndColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 51 ) ) ) ), ( (int)( ( (byte)( 51 ) ) ) ), ( (int)( ( (byte)( 51 ) ) ) ) );
      this.expBritish.CustomHeaderSettings.NormalGradientStartColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 51 ) ) ) ), ( (int)( ( (byte)( 51 ) ) ) ), ( (int)( ( (byte)( 51 ) ) ) ) );
      this.expBritish.CustomHeaderSettings.NormalTitleColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ) );
      this.expBritish.CustomHeaderSettings.NormalTitleHotColor = System.Drawing.Color.White;
      this.expBritish.CustomHeaderSettings.TitleFont = new System.Drawing.Font( "Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ) );
      this.expBritish.CustomHeaderSettings.TitleGradient = true;
      this.expBritish.CustomHeaderSettings.TitleRadius = 0;
      this.expBritish.CustomSettings.NormalBackColor = System.Drawing.Color.Black;
      this.expBritish.CustomSettings.NormalBorderColor = System.Drawing.Color.Black;
      this.expBritish.ExpandedHeight = 50;
      this.expBritish.Items.AddRange( new System.Windows.Forms.Control[] {
            this.picBritish} );
      this.expBritish.Name = "expBritish";
      this.expBritish.TitleImage = global::BEGM.Properties.Resources.flag_country_british;
      this.expBritish.MouseEnter += new System.EventHandler( this.expBritish_MouseEnter );
      // 
      // picBritish
      // 
      resources.ApplyResources( this.picBritish, "picBritish" );
      this.picBritish.Image = null;
      this.picBritish.Name = "picBritish";
      this.picBritish.MyMouseEnter += new System.EventHandler( this.picBritish_MyMouseEnter );
      // 
      // expFrench
      // 
      resources.ApplyResources( this.expFrench, "expFrench" );
      this.expFrench.Animate = true;
      this.expFrench.CustomHeaderSettings.BackImageHeight = 25;
      this.expFrench.CustomHeaderSettings.NormalGradientEndColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 51 ) ) ) ), ( (int)( ( (byte)( 51 ) ) ) ), ( (int)( ( (byte)( 51 ) ) ) ) );
      this.expFrench.CustomHeaderSettings.NormalGradientStartColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 51 ) ) ) ), ( (int)( ( (byte)( 51 ) ) ) ), ( (int)( ( (byte)( 51 ) ) ) ) );
      this.expFrench.CustomHeaderSettings.NormalTitleColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ) );
      this.expFrench.CustomHeaderSettings.NormalTitleHotColor = System.Drawing.Color.White;
      this.expFrench.CustomHeaderSettings.TitleFont = new System.Drawing.Font( "Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ) );
      this.expFrench.CustomHeaderSettings.TitleGradient = true;
      this.expFrench.CustomHeaderSettings.TitleRadius = 0;
      this.expFrench.CustomSettings.NormalBackColor = System.Drawing.Color.Black;
      this.expFrench.CustomSettings.NormalBorderColor = System.Drawing.Color.Black;
      this.expFrench.ExpandedHeight = 50;
      this.expFrench.Items.AddRange( new System.Windows.Forms.Control[] {
            this.picFrench} );
      this.expFrench.Name = "expFrench";
      this.expFrench.TitleImage = global::BEGM.Properties.Resources.flag_country_french;
      this.expFrench.MouseEnter += new System.EventHandler( this.expFrench_MouseEnter );
      // 
      // picFrench
      // 
      resources.ApplyResources( this.picFrench, "picFrench" );
      this.picFrench.Image = null;
      this.picFrench.Name = "picFrench";
      this.picFrench.MyMouseEnter += new System.EventHandler( this.picFrench_MyMouseEnter );
      // 
      // expGerman
      // 
      resources.ApplyResources( this.expGerman, "expGerman" );
      this.expGerman.Animate = true;
      this.expGerman.CustomHeaderSettings.BackImageHeight = 25;
      this.expGerman.CustomHeaderSettings.NormalGradientEndColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 51 ) ) ) ), ( (int)( ( (byte)( 51 ) ) ) ), ( (int)( ( (byte)( 51 ) ) ) ) );
      this.expGerman.CustomHeaderSettings.NormalGradientStartColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 51 ) ) ) ), ( (int)( ( (byte)( 51 ) ) ) ), ( (int)( ( (byte)( 51 ) ) ) ) );
      this.expGerman.CustomHeaderSettings.NormalTitleColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ) );
      this.expGerman.CustomHeaderSettings.NormalTitleHotColor = System.Drawing.Color.White;
      this.expGerman.CustomHeaderSettings.TitleFont = new System.Drawing.Font( "Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ) );
      this.expGerman.CustomHeaderSettings.TitleGradient = true;
      this.expGerman.CustomHeaderSettings.TitleRadius = 0;
      this.expGerman.CustomSettings.NormalBackColor = System.Drawing.Color.Black;
      this.expGerman.CustomSettings.NormalBorderColor = System.Drawing.Color.Black;
      this.expGerman.ExpandedHeight = 50;
      this.expGerman.Items.AddRange( new System.Windows.Forms.Control[] {
            this.picGerman} );
      this.expGerman.Name = "expGerman";
      this.expGerman.TitleImage = global::BEGM.Properties.Resources.flag_country_german;
      this.expGerman.MouseEnter += new System.EventHandler( this.expGerman_MouseEnter );
      // 
      // picGerman
      // 
      resources.ApplyResources( this.picGerman, "picGerman" );
      this.picGerman.Image = null;
      this.picGerman.Name = "picGerman";
      this.picGerman.MyMouseEnter += new System.EventHandler( this.picGerman_MyMouseEnter );
      // 
      // FactoryStatus
      // 
      resources.ApplyResources( this, "$this" );
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.Black;
      this.Controls.Add( this.pnlLoadFactoryData );
      this.Controls.Add( this.picSummary );
      this.Controls.Add( this.tskFactory );
      this.ForeColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ) );
      this.Name = "FactoryStatus";
      this.pnlLoadFactoryData.ResumeLayout( false );
      this.pnlLoadFactoryData.PerformLayout();
      ( (System.ComponentModel.ISupportInitialize)( this.picSummary ) ).EndInit();
      ( (System.ComponentModel.ISupportInitialize)( this.tskFactory ) ).EndInit();
      this.tskFactory.ResumeLayout( false );
      ( (System.ComponentModel.ISupportInitialize)( this.expBritish ) ).EndInit();
      this.expBritish.ResumeLayout( false );
      ( (System.ComponentModel.ISupportInitialize)( this.expFrench ) ).EndInit();
      this.expFrench.ResumeLayout( false );
      ( (System.ComponentModel.ISupportInitialize)( this.expGerman ) ).EndInit();
      this.expGerman.ResumeLayout( false );
      this.ResumeLayout( false );

    }

    #endregion

    private XPExplorerBar.TaskPane tskFactory;
    private XPExplorerBar.Expando expBritish;
    private ImageMap.ImageMap picBritish;
    private XPExplorerBar.Expando expFrench;
    private ImageMap.ImageMap picFrench;
    private XPExplorerBar.Expando expGerman;
    private ImageMap.ImageMap picGerman;
    private System.Windows.Forms.Panel pnlLoadFactoryData;
    private System.Windows.Forms.Button btnLoadFactoryData;
    private System.Windows.Forms.Label lblAlwaysLoad;
    public System.Windows.Forms.CheckBox cbAlwaysLoad;
    private System.Windows.Forms.PictureBox picSummary;
    private System.Windows.Forms.Timer tmrReveal;
  }
}
