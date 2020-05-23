namespace BEGM.Widgets
{
  partial class TownStatus
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager( typeof( TownStatus ) );
      this.picCPFlag = new System.Windows.Forms.PictureBox();
      this.lblCPName = new System.Windows.Forms.Label();
      this.tmrCPListSelect = new System.Windows.Forms.Timer( this.components );
      this.tmrCPListSearch = new System.Windows.Forms.Timer( this.components );
      this.lnkEvents = new System.Windows.Forms.LinkLabel();
      this.tmrReveal = new System.Windows.Forms.Timer( this.components );
      this.lnkMap = new System.Windows.Forms.LinkLabel();
      this.lblStartTip = new System.Windows.Forms.Label();
      this.lbCPList = new XPExplorerBar.XPListBox();
      this.picCPInfo = new ImageMap.ImageMap();
      ( (System.ComponentModel.ISupportInitialize)( this.picCPFlag ) ).BeginInit();
      this.SuspendLayout();
      // 
      // picCPFlag
      // 
      resources.ApplyResources( this.picCPFlag, "picCPFlag" );
      this.picCPFlag.Name = "picCPFlag";
      this.picCPFlag.TabStop = false;
      // 
      // lblCPName
      // 
      resources.ApplyResources( this.lblCPName, "lblCPName" );
      this.lblCPName.Name = "lblCPName";
      // 
      // tmrCPListSelect
      // 
      this.tmrCPListSelect.Tick += new System.EventHandler( this.tmrCPListSelect_Tick );
      // 
      // tmrCPListSearch
      // 
      this.tmrCPListSearch.Interval = 750;
      this.tmrCPListSearch.Tick += new System.EventHandler( this.tmrCPListSearch_Tick );
      // 
      // lnkEvents
      // 
      this.lnkEvents.ActiveLinkColor = System.Drawing.Color.White;
      this.lnkEvents.DisabledLinkColor = System.Drawing.Color.Gray;
      resources.ApplyResources( this.lnkEvents, "lnkEvents" );
      this.lnkEvents.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
      this.lnkEvents.LinkColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ) );
      this.lnkEvents.Name = "lnkEvents";
      this.lnkEvents.TabStop = true;
      this.lnkEvents.VisitedLinkColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ) );
      this.lnkEvents.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler( this.lnkEvents_LinkClicked );
      // 
      // tmrReveal
      // 
      this.tmrReveal.Tick += new System.EventHandler( this.tmrReveal_Tick );
      // 
      // lnkMap
      // 
      this.lnkMap.ActiveLinkColor = System.Drawing.Color.White;
      resources.ApplyResources( this.lnkMap, "lnkMap" );
      this.lnkMap.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
      this.lnkMap.LinkColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ) );
      this.lnkMap.Name = "lnkMap";
      this.lnkMap.TabStop = true;
      this.lnkMap.VisitedLinkColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ) );
      this.lnkMap.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler( this.lnkMap_LinkClicked );
      // 
      // lblStartTip
      // 
      resources.ApplyResources( this.lblStartTip, "lblStartTip" );
      this.lblStartTip.ForeColor = System.Drawing.Color.Gray;
      this.lblStartTip.Name = "lblStartTip";
      // 
      // lbCPList
      // 
      this.lbCPList.BackColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 32 ) ) ) ), ( (int)( ( (byte)( 32 ) ) ) ), ( (int)( ( (byte)( 32 ) ) ) ) );
      this.lbCPList.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.lbCPList.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
      this.lbCPList.ForeColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ) );
      this.lbCPList.FormattingEnabled = true;
      resources.ApplyResources( this.lbCPList, "lbCPList" );
      this.lbCPList.Name = "lbCPList";
      this.lbCPList.Sorted = true;
      this.lbCPList.DrawItem += new System.Windows.Forms.DrawItemEventHandler( this.lbCPList_DrawItem );
      this.lbCPList.MouseEnter += new System.EventHandler( this.lbCPList_MouseEnter );
      this.lbCPList.KeyPress += new System.Windows.Forms.KeyPressEventHandler( this.lbCPList_KeyPress );
      this.lbCPList.KeyDown += new System.Windows.Forms.KeyEventHandler( this.lbCPList_KeyDown );
      this.lbCPList.Click += new System.EventHandler( this.lbCPList_Click );
      // 
      // picCPInfo
      // 
      this.picCPInfo.Image = null;
      resources.ApplyResources( this.picCPInfo, "picCPInfo" );
      this.picCPInfo.Name = "picCPInfo";
      this.picCPInfo.MyMouseEnter += new System.EventHandler( this.picCPInfo_MyMouseEnter );
      this.picCPInfo.RegionClick += new ImageMap.ImageMap.RegionClickDelegate( this.picCPInfo_RegionClick );
      // 
      // TownStatus
      // 
      resources.ApplyResources( this, "$this" );
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.Black;
      this.Controls.Add( this.lblStartTip );
      this.Controls.Add( this.lnkMap );
      this.Controls.Add( this.lnkEvents );
      this.Controls.Add( this.lbCPList );
      this.Controls.Add( this.picCPFlag );
      this.Controls.Add( this.lblCPName );
      this.Controls.Add( this.picCPInfo );
      this.ForeColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ) );
      this.Name = "TownStatus";
      this.MouseEnter += new System.EventHandler( this.TownStatus_MouseEnter );
      ( (System.ComponentModel.ISupportInitialize)( this.picCPFlag ) ).EndInit();
      this.ResumeLayout( false );
      this.PerformLayout();

    }

    #endregion

    private XPExplorerBar.XPListBox lbCPList;
    private System.Windows.Forms.PictureBox picCPFlag;
    private System.Windows.Forms.Label lblCPName;
    private ImageMap.ImageMap picCPInfo;
    private System.Windows.Forms.Timer tmrCPListSelect;
    private System.Windows.Forms.Timer tmrCPListSearch;
    private System.Windows.Forms.LinkLabel lnkEvents;
    private System.Windows.Forms.Timer tmrReveal;
    private System.Windows.Forms.LinkLabel lnkMap;
    private System.Windows.Forms.Label lblStartTip;
  }
}
