namespace BEGM
{
  partial class GameStatus
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

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager( typeof( GameStatus ) );
      this.trayIcon = new System.Windows.Forms.NotifyIcon( this.components );
      this.cmsTrayIcon = new System.Windows.Forms.ContextMenuStrip( this.components );
      this.miTrayShowHide = new System.Windows.Forms.ToolStripMenuItem();
      this.miTrayShowLastAlerts = new System.Windows.Forms.ToolStripMenuItem();
      this.miTrayOptions = new System.Windows.Forms.ToolStripMenuItem();
      this.miTrayAbout = new System.Windows.Forms.ToolStripMenuItem();
      this.miTraySep1 = new System.Windows.Forms.ToolStripSeparator();
      this.miTrayDisableAllAlerts = new System.Windows.Forms.ToolStripMenuItem();
      this.miTraySep2 = new System.Windows.Forms.ToolStripSeparator();
      this.miTraySleep = new System.Windows.Forms.ToolStripMenuItem();
      this.miTrayExit = new System.Windows.Forms.ToolStripMenuItem();
      this.lnkOptions = new System.Windows.Forms.LinkLabel();
      this.lblStatus = new System.Windows.Forms.Label();
      this.picClose = new System.Windows.Forms.PictureBox();
      this.picBELogo = new System.Windows.Forms.PictureBox();
      this.lnkAbout = new System.Windows.Forms.LinkLabel();
      this.lnkHelp = new System.Windows.Forms.LinkLabel();
      this.toolTip = new System.Windows.Forms.ToolTip( this.components );
      this.picInit = new System.Windows.Forms.PictureBox();
      this.lblInit = new System.Windows.Forms.Label();
      this.lblStatusMsg = new System.Windows.Forms.Label();
      this.lnkVersion = new System.Windows.Forms.LinkLabel();
      this.btnLaunchGame = new System.Windows.Forms.Button();
      this.lnkLaunchGame = new System.Windows.Forms.LinkLabel();
      this.cmsLaunchGame = new System.Windows.Forms.ContextMenuStrip( this.components );
      this.miLaunchGameOnline = new System.Windows.Forms.ToolStripMenuItem();
      this.miLaunchGameOnlineLive = new System.Windows.Forms.ToolStripMenuItem();
      this.miLaunchGameOnlineTraining = new System.Windows.Forms.ToolStripMenuItem();
      this.miLaunchGameOnlineBeta = new System.Windows.Forms.ToolStripMenuItem();
      this.miLaunchGameOffline = new System.Windows.Forms.ToolStripMenuItem();
      this.miLaunchGameOfflineLive = new System.Windows.Forms.ToolStripMenuItem();
      this.miLaunchGameOfflineBeta = new System.Windows.Forms.ToolStripMenuItem();
      this.tmrStatusBar = new System.Windows.Forms.Timer( this.components );
      this.bgwInit = new System.ComponentModel.BackgroundWorker();
      this.tmrEventLoop = new System.Windows.Forms.Timer( this.components );
      this.bgwPoll = new System.ComponentModel.BackgroundWorker();
      this.bgwInitFactory = new System.ComponentModel.BackgroundWorker();
      this.pnlResizeLeft = new System.Windows.Forms.Panel();
      this.tblLayout = new System.Windows.Forms.TableLayoutPanel();
      this.pnlHeader = new System.Windows.Forms.Panel();
      this.pnlFooter = new System.Windows.Forms.Panel();
      this.pnlMainColumn = new System.Windows.Forms.Panel();
      this.tskMain = new XPExplorerBar.TaskPane();
      this.expServerStatus = new XPExplorerBar.Expando();
      this.wgtServerStatus = new BEGM.Widgets.ServerStatus();
      this.expRecentEvents = new XPExplorerBar.Expando();
      this.wgtRecentEvents = new BEGM.Widgets.RecentEvents();
      this.expCurrentAttacks = new XPExplorerBar.Expando();
      this.wgtCurrentAttacks = new BEGM.Widgets.CurrentAttacks();
      this.expTownStatus = new XPExplorerBar.Expando();
      this.wgtTownStatus = new BEGM.Widgets.TownStatus();
      this.expGameMap = new XPExplorerBar.Expando();
      this.wgtGameMap = new BEGM.Widgets.GameMap();
      this.expFactoryStatus = new XPExplorerBar.Expando();
      this.wgtFactoryStatus = new BEGM.Widgets.FactoryStatus();
      this.expOrderOfBattle = new XPExplorerBar.Expando();
      this.wgtOrderOfBattle = new BEGM.Widgets.OrderOfBattle();
      this.expBrigadeStatus = new XPExplorerBar.Expando();
      this.wgtBrigadeStatus = new BEGM.Widgets.BrigadeStatus();
      this.expEquipment = new XPExplorerBar.Expando();
      this.wgtEquipment = new BEGM.Widgets.Equipment();
      this.pnlResizeRight = new System.Windows.Forms.Panel();
      this.picResizeCorner = new System.Windows.Forms.PictureBox();
      this.cmsTrayIcon.SuspendLayout();
      ( (System.ComponentModel.ISupportInitialize)( this.picClose ) ).BeginInit();
      ( (System.ComponentModel.ISupportInitialize)( this.picBELogo ) ).BeginInit();
      ( (System.ComponentModel.ISupportInitialize)( this.picInit ) ).BeginInit();
      this.cmsLaunchGame.SuspendLayout();
      this.tblLayout.SuspendLayout();
      this.pnlHeader.SuspendLayout();
      this.pnlFooter.SuspendLayout();
      this.pnlMainColumn.SuspendLayout();
      ( (System.ComponentModel.ISupportInitialize)( this.tskMain ) ).BeginInit();
      this.tskMain.SuspendLayout();
      ( (System.ComponentModel.ISupportInitialize)( this.expServerStatus ) ).BeginInit();
      this.expServerStatus.SuspendLayout();
      ( (System.ComponentModel.ISupportInitialize)( this.expRecentEvents ) ).BeginInit();
      this.expRecentEvents.SuspendLayout();
      ( (System.ComponentModel.ISupportInitialize)( this.expCurrentAttacks ) ).BeginInit();
      this.expCurrentAttacks.SuspendLayout();
      ( (System.ComponentModel.ISupportInitialize)( this.expTownStatus ) ).BeginInit();
      this.expTownStatus.SuspendLayout();
      ( (System.ComponentModel.ISupportInitialize)( this.expGameMap ) ).BeginInit();
      this.expGameMap.SuspendLayout();
      ( (System.ComponentModel.ISupportInitialize)( this.expFactoryStatus ) ).BeginInit();
      this.expFactoryStatus.SuspendLayout();
      ( (System.ComponentModel.ISupportInitialize)( this.expOrderOfBattle ) ).BeginInit();
      this.expOrderOfBattle.SuspendLayout();
      ( (System.ComponentModel.ISupportInitialize)( this.expBrigadeStatus ) ).BeginInit();
      this.expBrigadeStatus.SuspendLayout();
      ( (System.ComponentModel.ISupportInitialize)( this.expEquipment ) ).BeginInit();
      this.expEquipment.SuspendLayout();
      ( (System.ComponentModel.ISupportInitialize)( this.picResizeCorner ) ).BeginInit();
      this.SuspendLayout();
      // 
      // trayIcon
      // 
      this.trayIcon.ContextMenuStrip = this.cmsTrayIcon;
      resources.ApplyResources( this.trayIcon, "trayIcon" );
      this.trayIcon.MouseClick += new System.Windows.Forms.MouseEventHandler( this.trayIcon_MouseClick );
      // 
      // cmsTrayIcon
      // 
      this.cmsTrayIcon.Items.AddRange( new System.Windows.Forms.ToolStripItem[] {
            this.miTrayShowHide,
            this.miTrayShowLastAlerts,
            this.miTrayOptions,
            this.miTrayAbout,
            this.miTraySep1,
            this.miTrayDisableAllAlerts,
            this.miTraySep2,
            this.miTraySleep,
            this.miTrayExit} );
      this.cmsTrayIcon.Name = "cmsTrayIcon";
      resources.ApplyResources( this.cmsTrayIcon, "cmsTrayIcon" );
      // 
      // miTrayShowHide
      // 
      resources.ApplyResources( this.miTrayShowHide, "miTrayShowHide" );
      this.miTrayShowHide.Name = "miTrayShowHide";
      this.miTrayShowHide.Click += new System.EventHandler( this.miTrayShowHide_Click );
      // 
      // miTrayShowLastAlerts
      // 
      this.miTrayShowLastAlerts.Name = "miTrayShowLastAlerts";
      resources.ApplyResources( this.miTrayShowLastAlerts, "miTrayShowLastAlerts" );
      this.miTrayShowLastAlerts.Click += new System.EventHandler( this.miTrayShowLastAlerts_Click );
      // 
      // miTrayOptions
      // 
      this.miTrayOptions.Name = "miTrayOptions";
      resources.ApplyResources( this.miTrayOptions, "miTrayOptions" );
      this.miTrayOptions.Click += new System.EventHandler( this.miTrayOptions_Click );
      // 
      // miTrayAbout
      // 
      this.miTrayAbout.Name = "miTrayAbout";
      resources.ApplyResources( this.miTrayAbout, "miTrayAbout" );
      this.miTrayAbout.Click += new System.EventHandler( this.miTrayAbout_Click );
      // 
      // miTraySep1
      // 
      this.miTraySep1.Name = "miTraySep1";
      resources.ApplyResources( this.miTraySep1, "miTraySep1" );
      // 
      // miTrayDisableAllAlerts
      // 
      this.miTrayDisableAllAlerts.CheckOnClick = true;
      this.miTrayDisableAllAlerts.Name = "miTrayDisableAllAlerts";
      resources.ApplyResources( this.miTrayDisableAllAlerts, "miTrayDisableAllAlerts" );
      this.miTrayDisableAllAlerts.Click += new System.EventHandler( this.miTrayDisableAllAlerts_Click );
      // 
      // miTraySep2
      // 
      this.miTraySep2.Name = "miTraySep2";
      resources.ApplyResources( this.miTraySep2, "miTraySep2" );
      // 
      // miTraySleep
      // 
      this.miTraySleep.Name = "miTraySleep";
      resources.ApplyResources( this.miTraySleep, "miTraySleep" );
      this.miTraySleep.Click += new System.EventHandler( this.miTraySleep_Click );
      // 
      // miTrayExit
      // 
      this.miTrayExit.Name = "miTrayExit";
      resources.ApplyResources( this.miTrayExit, "miTrayExit" );
      this.miTrayExit.Click += new System.EventHandler( this.miTrayExit_Click );
      // 
      // lnkOptions
      // 
      this.lnkOptions.ActiveLinkColor = System.Drawing.Color.White;
      resources.ApplyResources( this.lnkOptions, "lnkOptions" );
      this.lnkOptions.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
      this.lnkOptions.LinkColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ) );
      this.lnkOptions.Name = "lnkOptions";
      this.lnkOptions.TabStop = true;
      this.lnkOptions.VisitedLinkColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ) );
      this.lnkOptions.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler( this.lnkOptions_LinkClicked );
      // 
      // lblStatus
      // 
      this.lblStatus.ForeColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ) );
      resources.ApplyResources( this.lblStatus, "lblStatus" );
      this.lblStatus.Name = "lblStatus";
      this.lblStatus.MouseHover += new System.EventHandler( this.lblStatus_MouseHover );
      // 
      // picClose
      // 
      this.picClose.Cursor = System.Windows.Forms.Cursors.Hand;
      this.picClose.Image = global::BEGM.Properties.Resources.close;
      resources.ApplyResources( this.picClose, "picClose" );
      this.picClose.Name = "picClose";
      this.picClose.TabStop = false;
      this.picClose.Tag = false;
      this.toolTip.SetToolTip( this.picClose, resources.GetString( "picClose.ToolTip" ) );
      this.picClose.MouseClick += new System.Windows.Forms.MouseEventHandler( this.picClose_MouseClick );
      this.picClose.MouseDown += new System.Windows.Forms.MouseEventHandler( this.picClose_MouseDown );
      this.picClose.MouseEnter += new System.EventHandler( this.picClose_MouseEnter );
      this.picClose.MouseLeave += new System.EventHandler( this.picClose_MouseLeave );
      this.picClose.MouseUp += new System.Windows.Forms.MouseEventHandler( this.picClose_MouseUp );
      // 
      // picBELogo
      // 
      this.picBELogo.Image = global::BEGM.Properties.Resources.begm_logo;
      resources.ApplyResources( this.picBELogo, "picBELogo" );
      this.picBELogo.Name = "picBELogo";
      this.picBELogo.TabStop = false;
      this.picBELogo.MouseEnter += new System.EventHandler( this.picBELogo_MouseEnter );
      // 
      // lnkAbout
      // 
      this.lnkAbout.ActiveLinkColor = System.Drawing.Color.White;
      resources.ApplyResources( this.lnkAbout, "lnkAbout" );
      this.lnkAbout.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
      this.lnkAbout.LinkColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ) );
      this.lnkAbout.Name = "lnkAbout";
      this.lnkAbout.TabStop = true;
      this.lnkAbout.VisitedLinkColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ) );
      this.lnkAbout.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler( this.lnkAbout_LinkClicked );
      // 
      // lnkHelp
      // 
      this.lnkHelp.ActiveLinkColor = System.Drawing.Color.White;
      resources.ApplyResources( this.lnkHelp, "lnkHelp" );
      this.lnkHelp.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
      this.lnkHelp.LinkColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ) );
      this.lnkHelp.Name = "lnkHelp";
      this.lnkHelp.TabStop = true;
      this.lnkHelp.VisitedLinkColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ) );
      this.lnkHelp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler( this.lnkHelp_LinkClicked );
      // 
      // picInit
      // 
      resources.ApplyResources( this.picInit, "picInit" );
      this.picInit.Image = global::BEGM.Properties.Resources.gears;
      this.picInit.Name = "picInit";
      this.picInit.TabStop = false;
      // 
      // lblInit
      // 
      resources.ApplyResources( this.lblInit, "lblInit" );
      this.lblInit.ForeColor = System.Drawing.Color.DimGray;
      this.lblInit.Name = "lblInit";
      // 
      // lblStatusMsg
      // 
      this.lblStatusMsg.ForeColor = System.Drawing.Color.Gray;
      resources.ApplyResources( this.lblStatusMsg, "lblStatusMsg" );
      this.lblStatusMsg.Name = "lblStatusMsg";
      this.lblStatusMsg.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler( this.lblStatusMsg_MouseDoubleClick );
      this.lblStatusMsg.MouseLeave += new System.EventHandler( this.lblStatusMsg_MouseLeave );
      // 
      // lnkVersion
      // 
      this.lnkVersion.ActiveLinkColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 255 ) ) ) ), ( (int)( ( (byte)( 128 ) ) ) ), ( (int)( ( (byte)( 128 ) ) ) ) );
      this.lnkVersion.BackColor = System.Drawing.Color.Transparent;
      this.lnkVersion.DisabledLinkColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ) );
      resources.ApplyResources( this.lnkVersion, "lnkVersion" );
      this.lnkVersion.ForeColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 255 ) ) ) ), ( (int)( ( (byte)( 64 ) ) ) ), ( (int)( ( (byte)( 64 ) ) ) ) );
      this.lnkVersion.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
      this.lnkVersion.LinkColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 255 ) ) ) ), ( (int)( ( (byte)( 64 ) ) ) ), ( (int)( ( (byte)( 64 ) ) ) ) );
      this.lnkVersion.Name = "lnkVersion";
      this.lnkVersion.TabStop = true;
      this.lnkVersion.VisitedLinkColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 255 ) ) ) ), ( (int)( ( (byte)( 64 ) ) ) ), ( (int)( ( (byte)( 64 ) ) ) ) );
      this.lnkVersion.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler( this.lnkVersion_LinkClicked );
      // 
      // btnLaunchGame
      // 
      resources.ApplyResources( this.btnLaunchGame, "btnLaunchGame" );
      this.btnLaunchGame.Image = global::BEGM.Properties.Resources.dropdown_arrow;
      this.btnLaunchGame.Name = "btnLaunchGame";
      this.btnLaunchGame.Click += new System.EventHandler( this.btnLaunchGame_Click );
      // 
      // lnkLaunchGame
      // 
      this.lnkLaunchGame.ActiveLinkColor = System.Drawing.Color.White;
      resources.ApplyResources( this.lnkLaunchGame, "lnkLaunchGame" );
      this.lnkLaunchGame.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
      this.lnkLaunchGame.LinkColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ) );
      this.lnkLaunchGame.Name = "lnkLaunchGame";
      this.lnkLaunchGame.TabStop = true;
      this.lnkLaunchGame.VisitedLinkColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ) );
      this.lnkLaunchGame.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler( this.lnkLaunchGame_LinkClicked );
      // 
      // cmsLaunchGame
      // 
      resources.ApplyResources( this.cmsLaunchGame, "cmsLaunchGame" );
      this.cmsLaunchGame.BackColor = System.Drawing.Color.Black;
      this.cmsLaunchGame.Items.AddRange( new System.Windows.Forms.ToolStripItem[] {
            this.miLaunchGameOnline,
            this.miLaunchGameOnlineLive,
            this.miLaunchGameOnlineTraining,
            this.miLaunchGameOnlineBeta,
            this.miLaunchGameOffline,
            this.miLaunchGameOfflineLive,
            this.miLaunchGameOfflineBeta} );
      this.cmsLaunchGame.Name = "contextMenuStrip1";
      this.cmsLaunchGame.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
      // 
      // miLaunchGameOnline
      // 
      resources.ApplyResources( this.miLaunchGameOnline, "miLaunchGameOnline" );
      this.miLaunchGameOnline.BackgroundImage = global::BEGM.Properties.Resources.vechbg;
      this.miLaunchGameOnline.ForeColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ) );
      this.miLaunchGameOnline.Name = "miLaunchGameOnline";
      this.miLaunchGameOnline.Padding = new System.Windows.Forms.Padding( 0 );
      // 
      // miLaunchGameOnlineLive
      // 
      resources.ApplyResources( this.miLaunchGameOnlineLive, "miLaunchGameOnlineLive" );
      this.miLaunchGameOnlineLive.ForeColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ) );
      this.miLaunchGameOnlineLive.Image = global::BEGM.Properties.Resources.server_other;
      this.miLaunchGameOnlineLive.Name = "miLaunchGameOnlineLive";
      this.miLaunchGameOnlineLive.Padding = new System.Windows.Forms.Padding( 0 );
      this.miLaunchGameOnlineLive.Click += new System.EventHandler( this.miLaunchGameOnlineLive_Click );
      // 
      // miLaunchGameOnlineTraining
      // 
      resources.ApplyResources( this.miLaunchGameOnlineTraining, "miLaunchGameOnlineTraining" );
      this.miLaunchGameOnlineTraining.ForeColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ) );
      this.miLaunchGameOnlineTraining.Image = global::BEGM.Properties.Resources.server_other;
      this.miLaunchGameOnlineTraining.Name = "miLaunchGameOnlineTraining";
      this.miLaunchGameOnlineTraining.Padding = new System.Windows.Forms.Padding( 0 );
      this.miLaunchGameOnlineTraining.Click += new System.EventHandler( this.miLaunchGameOnlineTraining_Click );
      // 
      // miLaunchGameOnlineBeta
      // 
      resources.ApplyResources( this.miLaunchGameOnlineBeta, "miLaunchGameOnlineBeta" );
      this.miLaunchGameOnlineBeta.ForeColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ) );
      this.miLaunchGameOnlineBeta.Image = global::BEGM.Properties.Resources.server_other;
      this.miLaunchGameOnlineBeta.Name = "miLaunchGameOnlineBeta";
      this.miLaunchGameOnlineBeta.Padding = new System.Windows.Forms.Padding( 0 );
      this.miLaunchGameOnlineBeta.Click += new System.EventHandler( this.miLaunchGameOnlineBeta_Click );
      // 
      // miLaunchGameOffline
      // 
      resources.ApplyResources( this.miLaunchGameOffline, "miLaunchGameOffline" );
      this.miLaunchGameOffline.BackgroundImage = global::BEGM.Properties.Resources.vechbg;
      this.miLaunchGameOffline.ForeColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ) );
      this.miLaunchGameOffline.Name = "miLaunchGameOffline";
      this.miLaunchGameOffline.Padding = new System.Windows.Forms.Padding( 0 );
      // 
      // miLaunchGameOfflineLive
      // 
      resources.ApplyResources( this.miLaunchGameOfflineLive, "miLaunchGameOfflineLive" );
      this.miLaunchGameOfflineLive.ForeColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ) );
      this.miLaunchGameOfflineLive.Image = global::BEGM.Properties.Resources.server_online;
      this.miLaunchGameOfflineLive.Name = "miLaunchGameOfflineLive";
      this.miLaunchGameOfflineLive.Padding = new System.Windows.Forms.Padding( 0 );
      this.miLaunchGameOfflineLive.Click += new System.EventHandler( this.miLaunchGameOfflineLive_Click );
      // 
      // miLaunchGameOfflineBeta
      // 
      resources.ApplyResources( this.miLaunchGameOfflineBeta, "miLaunchGameOfflineBeta" );
      this.miLaunchGameOfflineBeta.ForeColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ) );
      this.miLaunchGameOfflineBeta.Image = global::BEGM.Properties.Resources.server_online;
      this.miLaunchGameOfflineBeta.Name = "miLaunchGameOfflineBeta";
      this.miLaunchGameOfflineBeta.Padding = new System.Windows.Forms.Padding( 0 );
      this.miLaunchGameOfflineBeta.Click += new System.EventHandler( this.miLaunchGameOfflineBeta_Click );
      // 
      // tmrStatusBar
      // 
      this.tmrStatusBar.Tick += new System.EventHandler( this.tmrStatusBar_Tick );
      // 
      // bgwInit
      // 
      this.bgwInit.DoWork += new System.ComponentModel.DoWorkEventHandler( this.bgwInit_DoWork );
      this.bgwInit.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler( this.bgwInit_RunWorkerCompleted );
      // 
      // tmrEventLoop
      // 
      this.tmrEventLoop.Interval = 60000;
      this.tmrEventLoop.Tick += new System.EventHandler( this.tmrEventLoop_Tick );
      // 
      // bgwPoll
      // 
      this.bgwPoll.WorkerSupportsCancellation = true;
      this.bgwPoll.DoWork += new System.ComponentModel.DoWorkEventHandler( this.bgwPoll_DoWork );
      this.bgwPoll.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler( this.bgwPoll_RunWorkerCompleted );
      // 
      // bgwInitFactory
      // 
      this.bgwInitFactory.DoWork += new System.ComponentModel.DoWorkEventHandler( this.bgwInitFactory_DoWork );
      this.bgwInitFactory.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler( this.bgwInitFactory_RunWorkerCompleted );
      // 
      // pnlResizeLeft
      // 
      resources.ApplyResources( this.pnlResizeLeft, "pnlResizeLeft" );
      this.pnlResizeLeft.Cursor = System.Windows.Forms.Cursors.SizeWE;
      this.pnlResizeLeft.Name = "pnlResizeLeft";
      this.pnlResizeLeft.MouseDown += new System.Windows.Forms.MouseEventHandler( this.pnlResize_MouseDown );
      this.pnlResizeLeft.MouseUp += new System.Windows.Forms.MouseEventHandler( this.pnlResize_MouseUp );
      // 
      // tblLayout
      // 
      resources.ApplyResources( this.tblLayout, "tblLayout" );
      this.tblLayout.Controls.Add( this.pnlHeader, 0, 0 );
      this.tblLayout.Controls.Add( this.pnlFooter, 0, 2 );
      this.tblLayout.Controls.Add( this.pnlMainColumn, 0, 1 );
      this.tblLayout.GrowStyle = System.Windows.Forms.TableLayoutPanelGrowStyle.FixedSize;
      this.tblLayout.Name = "tblLayout";
      // 
      // pnlHeader
      // 
      this.pnlHeader.Controls.Add( this.picClose );
      this.pnlHeader.Controls.Add( this.lnkVersion );
      this.pnlHeader.Controls.Add( this.picBELogo );
      this.pnlHeader.Controls.Add( this.btnLaunchGame );
      this.pnlHeader.Controls.Add( this.lnkLaunchGame );
      this.pnlHeader.Controls.Add( this.lnkOptions );
      this.pnlHeader.Controls.Add( this.lnkAbout );
      this.pnlHeader.Controls.Add( this.lnkHelp );
      resources.ApplyResources( this.pnlHeader, "pnlHeader" );
      this.pnlHeader.Name = "pnlHeader";
      // 
      // pnlFooter
      // 
      resources.ApplyResources( this.pnlFooter, "pnlFooter" );
      this.pnlFooter.Controls.Add( this.lblStatusMsg );
      this.pnlFooter.Controls.Add( this.lblStatus );
      this.pnlFooter.Name = "pnlFooter";
      // 
      // pnlMainColumn
      // 
      resources.ApplyResources( this.pnlMainColumn, "pnlMainColumn" );
      this.pnlMainColumn.Controls.Add( this.tskMain );
      this.pnlMainColumn.Name = "pnlMainColumn";
      // 
      // tskMain
      // 
      this.tskMain.AllowExpandoDragging = true;
      resources.ApplyResources( this.tskMain, "tskMain" );
      this.tskMain.CustomSettings.GradientEndColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 20 ) ) ) ), ( (int)( ( (byte)( 20 ) ) ) ), ( (int)( ( (byte)( 20 ) ) ) ) );
      this.tskMain.CustomSettings.GradientStartColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 20 ) ) ) ), ( (int)( ( (byte)( 20 ) ) ) ), ( (int)( ( (byte)( 20 ) ) ) ) );
      this.tskMain.Expandos.Add( this.expServerStatus );
      this.tskMain.Expandos.Add( this.expRecentEvents );
      this.tskMain.Expandos.Add( this.expCurrentAttacks );
      this.tskMain.Expandos.Add( this.expTownStatus );
      this.tskMain.Expandos.Add( this.expGameMap );
      this.tskMain.Expandos.Add( this.expFactoryStatus );
      this.tskMain.Expandos.Add( this.expOrderOfBattle );
      this.tskMain.Expandos.Add( this.expBrigadeStatus );
      this.tskMain.Expandos.Add( this.expEquipment );
      this.tskMain.Name = "tskMain";
      this.tskMain.PreventAutoScroll = false;
      // 
      // expServerStatus
      // 
      resources.ApplyResources( this.expServerStatus, "expServerStatus" );
      this.expServerStatus.Animate = true;
      this.expServerStatus.Collapsed = true;
      this.expServerStatus.CustomHeaderSettings.BackImageHeight = 25;
      this.expServerStatus.CustomHeaderSettings.NormalArrowClose = global::BEGM.Properties.Resources.exparrow_close;
      this.expServerStatus.CustomHeaderSettings.NormalArrowDown = global::BEGM.Properties.Resources.exparrow_down;
      this.expServerStatus.CustomHeaderSettings.NormalArrowDownHot = global::BEGM.Properties.Resources.exparrow_down_hot;
      this.expServerStatus.CustomHeaderSettings.NormalArrowUp = global::BEGM.Properties.Resources.exparrow_up;
      this.expServerStatus.CustomHeaderSettings.NormalArrowUpHot = global::BEGM.Properties.Resources.exparrow_up_hot;
      this.expServerStatus.CustomHeaderSettings.NormalGradientEndColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 49 ) ) ) ), ( (int)( ( (byte)( 72 ) ) ) ), ( (int)( ( (byte)( 60 ) ) ) ) );
      this.expServerStatus.CustomHeaderSettings.NormalGradientStartColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 49 ) ) ) ), ( (int)( ( (byte)( 72 ) ) ) ), ( (int)( ( (byte)( 60 ) ) ) ) );
      this.expServerStatus.CustomHeaderSettings.NormalTitleColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 187 ) ) ) ), ( (int)( ( (byte)( 222 ) ) ) ), ( (int)( ( (byte)( 202 ) ) ) ) );
      this.expServerStatus.CustomHeaderSettings.NormalTitleHotColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 250 ) ) ) ), ( (int)( ( (byte)( 250 ) ) ) ), ( (int)( ( (byte)( 202 ) ) ) ) );
      this.expServerStatus.CustomHeaderSettings.TitleFont = new System.Drawing.Font( "Arial", 10.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ) );
      this.expServerStatus.CustomHeaderSettings.TitleGradient = true;
      this.expServerStatus.CustomHeaderSettings.TitleRadius = 5;
      this.expServerStatus.CustomSettings.NormalBackColor = System.Drawing.Color.Black;
      this.expServerStatus.CustomSettings.NormalBorderColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 49 ) ) ) ), ( (int)( ( (byte)( 72 ) ) ) ), ( (int)( ( (byte)( 60 ) ) ) ) );
      this.expServerStatus.DefaultTaskPane = null;
      this.expServerStatus.ExpandedHeight = 23;
      this.expServerStatus.Items.AddRange( new System.Windows.Forms.Control[] {
            this.wgtServerStatus} );
      this.expServerStatus.Name = "expServerStatus";
      // 
      // wgtServerStatus
      // 
      this.wgtServerStatus.BackColor = System.Drawing.Color.Black;
      resources.ApplyResources( this.wgtServerStatus, "wgtServerStatus" );
      this.wgtServerStatus.ForeColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ) );
      this.wgtServerStatus.GameStatus_RevealWidget = null;
      this.wgtServerStatus.Name = "wgtServerStatus";
      // 
      // expRecentEvents
      // 
      resources.ApplyResources( this.expRecentEvents, "expRecentEvents" );
      this.expRecentEvents.Animate = true;
      this.expRecentEvents.Collapsed = true;
      this.expRecentEvents.CustomHeaderSettings.BackImageHeight = 25;
      this.expRecentEvents.CustomHeaderSettings.NormalArrowClose = global::BEGM.Properties.Resources.exparrow_close;
      this.expRecentEvents.CustomHeaderSettings.NormalArrowDown = global::BEGM.Properties.Resources.exparrow_down;
      this.expRecentEvents.CustomHeaderSettings.NormalArrowDownHot = global::BEGM.Properties.Resources.exparrow_down_hot;
      this.expRecentEvents.CustomHeaderSettings.NormalArrowUp = global::BEGM.Properties.Resources.exparrow_up;
      this.expRecentEvents.CustomHeaderSettings.NormalArrowUpHot = global::BEGM.Properties.Resources.exparrow_up_hot;
      this.expRecentEvents.CustomHeaderSettings.NormalGradientEndColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 49 ) ) ) ), ( (int)( ( (byte)( 72 ) ) ) ), ( (int)( ( (byte)( 60 ) ) ) ) );
      this.expRecentEvents.CustomHeaderSettings.NormalGradientStartColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 49 ) ) ) ), ( (int)( ( (byte)( 72 ) ) ) ), ( (int)( ( (byte)( 60 ) ) ) ) );
      this.expRecentEvents.CustomHeaderSettings.NormalTitleColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 187 ) ) ) ), ( (int)( ( (byte)( 222 ) ) ) ), ( (int)( ( (byte)( 202 ) ) ) ) );
      this.expRecentEvents.CustomHeaderSettings.NormalTitleHotColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 250 ) ) ) ), ( (int)( ( (byte)( 250 ) ) ) ), ( (int)( ( (byte)( 202 ) ) ) ) );
      this.expRecentEvents.CustomHeaderSettings.TitleFont = new System.Drawing.Font( "Arial", 10.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ) );
      this.expRecentEvents.CustomHeaderSettings.TitleGradient = true;
      this.expRecentEvents.CustomHeaderSettings.TitleRadius = 5;
      this.expRecentEvents.CustomSettings.NormalBackColor = System.Drawing.Color.Black;
      this.expRecentEvents.CustomSettings.NormalBorderColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 49 ) ) ) ), ( (int)( ( (byte)( 72 ) ) ) ), ( (int)( ( (byte)( 60 ) ) ) ) );
      this.expRecentEvents.DefaultTaskPane = null;
      this.expRecentEvents.ExpandedHeight = 299;
      this.expRecentEvents.Items.AddRange( new System.Windows.Forms.Control[] {
            this.wgtRecentEvents} );
      this.expRecentEvents.Name = "expRecentEvents";
      // 
      // wgtRecentEvents
      // 
      this.wgtRecentEvents.BackColor = System.Drawing.Color.Black;
      resources.ApplyResources( this.wgtRecentEvents, "wgtRecentEvents" );
      this.wgtRecentEvents.ForeColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ) );
      this.wgtRecentEvents.GameStatus_RevealWidget = null;
      this.wgtRecentEvents.Name = "wgtRecentEvents";
      // 
      // expCurrentAttacks
      // 
      resources.ApplyResources( this.expCurrentAttacks, "expCurrentAttacks" );
      this.expCurrentAttacks.Animate = true;
      this.expCurrentAttacks.Collapsed = true;
      this.expCurrentAttacks.CustomHeaderSettings.BackImageHeight = 25;
      this.expCurrentAttacks.CustomHeaderSettings.NormalArrowClose = global::BEGM.Properties.Resources.exparrow_close;
      this.expCurrentAttacks.CustomHeaderSettings.NormalArrowDown = global::BEGM.Properties.Resources.exparrow_down;
      this.expCurrentAttacks.CustomHeaderSettings.NormalArrowDownHot = global::BEGM.Properties.Resources.exparrow_down_hot;
      this.expCurrentAttacks.CustomHeaderSettings.NormalArrowUp = global::BEGM.Properties.Resources.exparrow_up;
      this.expCurrentAttacks.CustomHeaderSettings.NormalArrowUpHot = global::BEGM.Properties.Resources.exparrow_up_hot;
      this.expCurrentAttacks.CustomHeaderSettings.NormalGradientEndColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 49 ) ) ) ), ( (int)( ( (byte)( 72 ) ) ) ), ( (int)( ( (byte)( 60 ) ) ) ) );
      this.expCurrentAttacks.CustomHeaderSettings.NormalGradientStartColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 49 ) ) ) ), ( (int)( ( (byte)( 72 ) ) ) ), ( (int)( ( (byte)( 60 ) ) ) ) );
      this.expCurrentAttacks.CustomHeaderSettings.NormalTitleColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 187 ) ) ) ), ( (int)( ( (byte)( 222 ) ) ) ), ( (int)( ( (byte)( 202 ) ) ) ) );
      this.expCurrentAttacks.CustomHeaderSettings.NormalTitleHotColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 250 ) ) ) ), ( (int)( ( (byte)( 250 ) ) ) ), ( (int)( ( (byte)( 202 ) ) ) ) );
      this.expCurrentAttacks.CustomHeaderSettings.TitleFont = new System.Drawing.Font( "Arial", 10.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ) );
      this.expCurrentAttacks.CustomHeaderSettings.TitleGradient = true;
      this.expCurrentAttacks.CustomHeaderSettings.TitleRadius = 5;
      this.expCurrentAttacks.CustomSettings.NormalBackColor = System.Drawing.Color.Black;
      this.expCurrentAttacks.CustomSettings.NormalBorderColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 49 ) ) ) ), ( (int)( ( (byte)( 72 ) ) ) ), ( (int)( ( (byte)( 60 ) ) ) ) );
      this.expCurrentAttacks.DefaultTaskPane = null;
      this.expCurrentAttacks.ExpandedHeight = 41;
      this.expCurrentAttacks.Items.AddRange( new System.Windows.Forms.Control[] {
            this.wgtCurrentAttacks} );
      this.expCurrentAttacks.Name = "expCurrentAttacks";
      // 
      // wgtCurrentAttacks
      // 
      this.wgtCurrentAttacks.BackColor = System.Drawing.Color.Black;
      resources.ApplyResources( this.wgtCurrentAttacks, "wgtCurrentAttacks" );
      this.wgtCurrentAttacks.ForeColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ) );
      this.wgtCurrentAttacks.GameStatus_RevealWidget = null;
      this.wgtCurrentAttacks.Name = "wgtCurrentAttacks";
      // 
      // expTownStatus
      // 
      resources.ApplyResources( this.expTownStatus, "expTownStatus" );
      this.expTownStatus.Animate = true;
      this.expTownStatus.Collapsed = true;
      this.expTownStatus.CustomHeaderSettings.BackImageHeight = 25;
      this.expTownStatus.CustomHeaderSettings.NormalArrowClose = global::BEGM.Properties.Resources.exparrow_close;
      this.expTownStatus.CustomHeaderSettings.NormalArrowDown = global::BEGM.Properties.Resources.exparrow_down;
      this.expTownStatus.CustomHeaderSettings.NormalArrowDownHot = global::BEGM.Properties.Resources.exparrow_down_hot;
      this.expTownStatus.CustomHeaderSettings.NormalArrowUp = global::BEGM.Properties.Resources.exparrow_up;
      this.expTownStatus.CustomHeaderSettings.NormalArrowUpHot = global::BEGM.Properties.Resources.exparrow_up_hot;
      this.expTownStatus.CustomHeaderSettings.NormalGradientEndColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 49 ) ) ) ), ( (int)( ( (byte)( 72 ) ) ) ), ( (int)( ( (byte)( 60 ) ) ) ) );
      this.expTownStatus.CustomHeaderSettings.NormalGradientStartColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 49 ) ) ) ), ( (int)( ( (byte)( 72 ) ) ) ), ( (int)( ( (byte)( 60 ) ) ) ) );
      this.expTownStatus.CustomHeaderSettings.NormalTitleColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 187 ) ) ) ), ( (int)( ( (byte)( 222 ) ) ) ), ( (int)( ( (byte)( 202 ) ) ) ) );
      this.expTownStatus.CustomHeaderSettings.NormalTitleHotColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 250 ) ) ) ), ( (int)( ( (byte)( 250 ) ) ) ), ( (int)( ( (byte)( 202 ) ) ) ) );
      this.expTownStatus.CustomHeaderSettings.TitleFont = new System.Drawing.Font( "Arial", 10.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ) );
      this.expTownStatus.CustomHeaderSettings.TitleGradient = true;
      this.expTownStatus.CustomHeaderSettings.TitleRadius = 5;
      this.expTownStatus.CustomSettings.NormalBackColor = System.Drawing.Color.Black;
      this.expTownStatus.CustomSettings.NormalBorderColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 49 ) ) ) ), ( (int)( ( (byte)( 72 ) ) ) ), ( (int)( ( (byte)( 60 ) ) ) ) );
      this.expTownStatus.DefaultTaskPane = null;
      this.expTownStatus.ExpandedHeight = 299;
      this.expTownStatus.Items.AddRange( new System.Windows.Forms.Control[] {
            this.wgtTownStatus} );
      this.expTownStatus.Name = "expTownStatus";
      this.expTownStatus.StateChanging += new XPExplorerBar.ExpandoEventHandler( this.expTownStatus_StateChanging );
      // 
      // wgtTownStatus
      // 
      this.wgtTownStatus.BackColor = System.Drawing.Color.Black;
      resources.ApplyResources( this.wgtTownStatus, "wgtTownStatus" );
      this.wgtTownStatus.ForeColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ) );
      this.wgtTownStatus.GameStatus_RevealWidget = null;
      this.wgtTownStatus.Name = "wgtTownStatus";
      // 
      // expGameMap
      // 
      resources.ApplyResources( this.expGameMap, "expGameMap" );
      this.expGameMap.Animate = true;
      this.expGameMap.Collapsed = true;
      this.expGameMap.CustomHeaderSettings.BackImageHeight = 25;
      this.expGameMap.CustomHeaderSettings.NormalArrowClose = global::BEGM.Properties.Resources.exparrow_close;
      this.expGameMap.CustomHeaderSettings.NormalArrowDown = global::BEGM.Properties.Resources.exparrow_down;
      this.expGameMap.CustomHeaderSettings.NormalArrowDownHot = global::BEGM.Properties.Resources.exparrow_down_hot;
      this.expGameMap.CustomHeaderSettings.NormalArrowUp = global::BEGM.Properties.Resources.exparrow_up;
      this.expGameMap.CustomHeaderSettings.NormalArrowUpHot = global::BEGM.Properties.Resources.exparrow_up_hot;
      this.expGameMap.CustomHeaderSettings.NormalGradientEndColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 49 ) ) ) ), ( (int)( ( (byte)( 72 ) ) ) ), ( (int)( ( (byte)( 60 ) ) ) ) );
      this.expGameMap.CustomHeaderSettings.NormalGradientStartColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 49 ) ) ) ), ( (int)( ( (byte)( 72 ) ) ) ), ( (int)( ( (byte)( 60 ) ) ) ) );
      this.expGameMap.CustomHeaderSettings.NormalTitleColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 187 ) ) ) ), ( (int)( ( (byte)( 222 ) ) ) ), ( (int)( ( (byte)( 202 ) ) ) ) );
      this.expGameMap.CustomHeaderSettings.NormalTitleHotColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 250 ) ) ) ), ( (int)( ( (byte)( 250 ) ) ) ), ( (int)( ( (byte)( 202 ) ) ) ) );
      this.expGameMap.CustomHeaderSettings.TitleFont = new System.Drawing.Font( "Arial", 10.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ) );
      this.expGameMap.CustomHeaderSettings.TitleGradient = true;
      this.expGameMap.CustomHeaderSettings.TitleRadius = 5;
      this.expGameMap.CustomSettings.NormalBackColor = System.Drawing.Color.Black;
      this.expGameMap.CustomSettings.NormalBorderColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 49 ) ) ) ), ( (int)( ( (byte)( 72 ) ) ) ), ( (int)( ( (byte)( 60 ) ) ) ) );
      this.expGameMap.DefaultTaskPane = null;
      this.expGameMap.ExpandedHeight = 468;
      this.expGameMap.Items.AddRange( new System.Windows.Forms.Control[] {
            this.wgtGameMap} );
      this.expGameMap.Name = "expGameMap";
      this.expGameMap.StateChanging += new XPExplorerBar.ExpandoEventHandler( this.expGameMap_StateChanging );
      this.expGameMap.StateChanged += new XPExplorerBar.ExpandoEventHandler( this.expGameMap_StateChanged );
      // 
      // wgtGameMap
      // 
      this.wgtGameMap.BackColor = System.Drawing.Color.Black;
      this.wgtGameMap.ForeColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ) );
      this.wgtGameMap.GameStatus_RevealWidget = null;
      resources.ApplyResources( this.wgtGameMap, "wgtGameMap" );
      this.wgtGameMap.MinimumSize = new System.Drawing.Size( 336, 444 );
      this.wgtGameMap.Name = "wgtGameMap";
      // 
      // expFactoryStatus
      // 
      resources.ApplyResources( this.expFactoryStatus, "expFactoryStatus" );
      this.expFactoryStatus.Animate = true;
      this.expFactoryStatus.Collapsed = true;
      this.expFactoryStatus.CustomHeaderSettings.BackImageHeight = 25;
      this.expFactoryStatus.CustomHeaderSettings.NormalArrowClose = global::BEGM.Properties.Resources.exparrow_close;
      this.expFactoryStatus.CustomHeaderSettings.NormalArrowDown = global::BEGM.Properties.Resources.exparrow_down;
      this.expFactoryStatus.CustomHeaderSettings.NormalArrowDownHot = global::BEGM.Properties.Resources.exparrow_down_hot;
      this.expFactoryStatus.CustomHeaderSettings.NormalArrowUp = global::BEGM.Properties.Resources.exparrow_up;
      this.expFactoryStatus.CustomHeaderSettings.NormalArrowUpHot = global::BEGM.Properties.Resources.exparrow_up_hot;
      this.expFactoryStatus.CustomHeaderSettings.NormalGradientEndColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 49 ) ) ) ), ( (int)( ( (byte)( 72 ) ) ) ), ( (int)( ( (byte)( 60 ) ) ) ) );
      this.expFactoryStatus.CustomHeaderSettings.NormalGradientStartColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 49 ) ) ) ), ( (int)( ( (byte)( 72 ) ) ) ), ( (int)( ( (byte)( 60 ) ) ) ) );
      this.expFactoryStatus.CustomHeaderSettings.NormalTitleColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 187 ) ) ) ), ( (int)( ( (byte)( 222 ) ) ) ), ( (int)( ( (byte)( 202 ) ) ) ) );
      this.expFactoryStatus.CustomHeaderSettings.NormalTitleHotColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 250 ) ) ) ), ( (int)( ( (byte)( 250 ) ) ) ), ( (int)( ( (byte)( 202 ) ) ) ) );
      this.expFactoryStatus.CustomHeaderSettings.TitleFont = new System.Drawing.Font( "Arial", 10.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ) );
      this.expFactoryStatus.CustomHeaderSettings.TitleGradient = true;
      this.expFactoryStatus.CustomHeaderSettings.TitleRadius = 5;
      this.expFactoryStatus.CustomSettings.NormalBackColor = System.Drawing.Color.Black;
      this.expFactoryStatus.CustomSettings.NormalBorderColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 49 ) ) ) ), ( (int)( ( (byte)( 72 ) ) ) ), ( (int)( ( (byte)( 60 ) ) ) ) );
      this.expFactoryStatus.DefaultTaskPane = null;
      this.expFactoryStatus.ExpandedHeight = 301;
      this.expFactoryStatus.Items.AddRange( new System.Windows.Forms.Control[] {
            this.wgtFactoryStatus} );
      this.expFactoryStatus.Name = "expFactoryStatus";
      // 
      // wgtFactoryStatus
      // 
      this.wgtFactoryStatus.BackColor = System.Drawing.Color.Black;
      resources.ApplyResources( this.wgtFactoryStatus, "wgtFactoryStatus" );
      this.wgtFactoryStatus.ForeColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ) );
      this.wgtFactoryStatus.GameStatus_RevealWidget = null;
      this.wgtFactoryStatus.Name = "wgtFactoryStatus";
      // 
      // expOrderOfBattle
      // 
      resources.ApplyResources( this.expOrderOfBattle, "expOrderOfBattle" );
      this.expOrderOfBattle.Animate = true;
      this.expOrderOfBattle.Collapsed = true;
      this.expOrderOfBattle.CustomHeaderSettings.BackImageHeight = 25;
      this.expOrderOfBattle.CustomHeaderSettings.NormalArrowClose = global::BEGM.Properties.Resources.exparrow_close;
      this.expOrderOfBattle.CustomHeaderSettings.NormalArrowDown = global::BEGM.Properties.Resources.exparrow_down;
      this.expOrderOfBattle.CustomHeaderSettings.NormalArrowDownHot = global::BEGM.Properties.Resources.exparrow_down_hot;
      this.expOrderOfBattle.CustomHeaderSettings.NormalArrowUp = global::BEGM.Properties.Resources.exparrow_up;
      this.expOrderOfBattle.CustomHeaderSettings.NormalArrowUpHot = global::BEGM.Properties.Resources.exparrow_up_hot;
      this.expOrderOfBattle.CustomHeaderSettings.NormalGradientEndColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 49 ) ) ) ), ( (int)( ( (byte)( 72 ) ) ) ), ( (int)( ( (byte)( 60 ) ) ) ) );
      this.expOrderOfBattle.CustomHeaderSettings.NormalGradientStartColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 49 ) ) ) ), ( (int)( ( (byte)( 72 ) ) ) ), ( (int)( ( (byte)( 60 ) ) ) ) );
      this.expOrderOfBattle.CustomHeaderSettings.NormalTitleColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 187 ) ) ) ), ( (int)( ( (byte)( 222 ) ) ) ), ( (int)( ( (byte)( 202 ) ) ) ) );
      this.expOrderOfBattle.CustomHeaderSettings.NormalTitleHotColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 250 ) ) ) ), ( (int)( ( (byte)( 250 ) ) ) ), ( (int)( ( (byte)( 202 ) ) ) ) );
      this.expOrderOfBattle.CustomHeaderSettings.TitleFont = new System.Drawing.Font( "Arial", 10.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ) );
      this.expOrderOfBattle.CustomHeaderSettings.TitleGradient = true;
      this.expOrderOfBattle.CustomHeaderSettings.TitleRadius = 5;
      this.expOrderOfBattle.CustomSettings.NormalBackColor = System.Drawing.Color.Black;
      this.expOrderOfBattle.CustomSettings.NormalBorderColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 49 ) ) ) ), ( (int)( ( (byte)( 72 ) ) ) ), ( (int)( ( (byte)( 60 ) ) ) ) );
      this.expOrderOfBattle.DefaultTaskPane = null;
      this.expOrderOfBattle.ExpandedHeight = 23;
      this.expOrderOfBattle.Items.AddRange( new System.Windows.Forms.Control[] {
            this.wgtOrderOfBattle} );
      this.expOrderOfBattle.Name = "expOrderOfBattle";
      // 
      // wgtOrderOfBattle
      // 
      this.wgtOrderOfBattle.BackColor = System.Drawing.Color.Black;
      this.wgtOrderOfBattle.ForeColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ) );
      this.wgtOrderOfBattle.GameStatus_RevealWidget = null;
      resources.ApplyResources( this.wgtOrderOfBattle, "wgtOrderOfBattle" );
      this.wgtOrderOfBattle.Name = "wgtOrderOfBattle";
      // 
      // expBrigadeStatus
      // 
      resources.ApplyResources( this.expBrigadeStatus, "expBrigadeStatus" );
      this.expBrigadeStatus.Animate = true;
      this.expBrigadeStatus.Collapsed = true;
      this.expBrigadeStatus.CustomHeaderSettings.BackImageHeight = 25;
      this.expBrigadeStatus.CustomHeaderSettings.NormalArrowClose = global::BEGM.Properties.Resources.exparrow_close;
      this.expBrigadeStatus.CustomHeaderSettings.NormalArrowDown = global::BEGM.Properties.Resources.exparrow_down;
      this.expBrigadeStatus.CustomHeaderSettings.NormalArrowDownHot = global::BEGM.Properties.Resources.exparrow_down_hot;
      this.expBrigadeStatus.CustomHeaderSettings.NormalArrowUp = global::BEGM.Properties.Resources.exparrow_up;
      this.expBrigadeStatus.CustomHeaderSettings.NormalArrowUpHot = global::BEGM.Properties.Resources.exparrow_up_hot;
      this.expBrigadeStatus.CustomHeaderSettings.NormalGradientEndColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 49 ) ) ) ), ( (int)( ( (byte)( 72 ) ) ) ), ( (int)( ( (byte)( 60 ) ) ) ) );
      this.expBrigadeStatus.CustomHeaderSettings.NormalGradientStartColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 49 ) ) ) ), ( (int)( ( (byte)( 72 ) ) ) ), ( (int)( ( (byte)( 60 ) ) ) ) );
      this.expBrigadeStatus.CustomHeaderSettings.NormalTitleColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 187 ) ) ) ), ( (int)( ( (byte)( 222 ) ) ) ), ( (int)( ( (byte)( 202 ) ) ) ) );
      this.expBrigadeStatus.CustomHeaderSettings.NormalTitleHotColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 250 ) ) ) ), ( (int)( ( (byte)( 250 ) ) ) ), ( (int)( ( (byte)( 202 ) ) ) ) );
      this.expBrigadeStatus.CustomHeaderSettings.TitleFont = new System.Drawing.Font( "Arial", 10.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ) );
      this.expBrigadeStatus.CustomHeaderSettings.TitleGradient = true;
      this.expBrigadeStatus.CustomHeaderSettings.TitleRadius = 5;
      this.expBrigadeStatus.CustomSettings.NormalBackColor = System.Drawing.Color.Black;
      this.expBrigadeStatus.CustomSettings.NormalBorderColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 49 ) ) ) ), ( (int)( ( (byte)( 72 ) ) ) ), ( (int)( ( (byte)( 60 ) ) ) ) );
      this.expBrigadeStatus.DefaultTaskPane = null;
      this.expBrigadeStatus.ExpandedHeight = 23;
      this.expBrigadeStatus.Items.AddRange( new System.Windows.Forms.Control[] {
            this.wgtBrigadeStatus} );
      this.expBrigadeStatus.Name = "expBrigadeStatus";
      // 
      // wgtBrigadeStatus
      // 
      this.wgtBrigadeStatus.BackColor = System.Drawing.Color.Black;
      resources.ApplyResources( this.wgtBrigadeStatus, "wgtBrigadeStatus" );
      this.wgtBrigadeStatus.ForeColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ) );
      this.wgtBrigadeStatus.GameStatus_RevealWidget = null;
      this.wgtBrigadeStatus.Name = "wgtBrigadeStatus";
      // 
      // expEquipment
      // 
      resources.ApplyResources( this.expEquipment, "expEquipment" );
      this.expEquipment.Animate = true;
      this.expEquipment.Collapsed = true;
      this.expEquipment.CustomHeaderSettings.BackImageHeight = 25;
      this.expEquipment.CustomHeaderSettings.NormalArrowClose = global::BEGM.Properties.Resources.exparrow_close;
      this.expEquipment.CustomHeaderSettings.NormalArrowDown = global::BEGM.Properties.Resources.exparrow_down;
      this.expEquipment.CustomHeaderSettings.NormalArrowDownHot = global::BEGM.Properties.Resources.exparrow_down_hot;
      this.expEquipment.CustomHeaderSettings.NormalArrowUp = global::BEGM.Properties.Resources.exparrow_up;
      this.expEquipment.CustomHeaderSettings.NormalArrowUpHot = global::BEGM.Properties.Resources.exparrow_up_hot;
      this.expEquipment.CustomHeaderSettings.NormalGradientEndColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 49 ) ) ) ), ( (int)( ( (byte)( 72 ) ) ) ), ( (int)( ( (byte)( 60 ) ) ) ) );
      this.expEquipment.CustomHeaderSettings.NormalGradientStartColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 49 ) ) ) ), ( (int)( ( (byte)( 72 ) ) ) ), ( (int)( ( (byte)( 60 ) ) ) ) );
      this.expEquipment.CustomHeaderSettings.NormalTitleColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 187 ) ) ) ), ( (int)( ( (byte)( 222 ) ) ) ), ( (int)( ( (byte)( 202 ) ) ) ) );
      this.expEquipment.CustomHeaderSettings.NormalTitleHotColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 250 ) ) ) ), ( (int)( ( (byte)( 250 ) ) ) ), ( (int)( ( (byte)( 202 ) ) ) ) );
      this.expEquipment.CustomHeaderSettings.TitleFont = new System.Drawing.Font( "Arial", 10.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ) );
      this.expEquipment.CustomHeaderSettings.TitleGradient = true;
      this.expEquipment.CustomHeaderSettings.TitleRadius = 5;
      this.expEquipment.CustomSettings.NormalBackColor = System.Drawing.Color.Black;
      this.expEquipment.CustomSettings.NormalBorderColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 49 ) ) ) ), ( (int)( ( (byte)( 72 ) ) ) ), ( (int)( ( (byte)( 60 ) ) ) ) );
      this.expEquipment.DefaultTaskPane = null;
      this.expEquipment.ExpandedHeight = 744;
      this.expEquipment.Items.AddRange( new System.Windows.Forms.Control[] {
            this.wgtEquipment} );
      this.expEquipment.Name = "expEquipment";
      // 
      // wgtEquipment
      // 
      this.wgtEquipment.BackColor = System.Drawing.Color.Black;
      resources.ApplyResources( this.wgtEquipment, "wgtEquipment" );
      this.wgtEquipment.ForeColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ) );
      this.wgtEquipment.GameStatus_RevealWidget = null;
      this.wgtEquipment.Name = "wgtEquipment";
      // 
      // pnlResizeRight
      // 
      resources.ApplyResources( this.pnlResizeRight, "pnlResizeRight" );
      this.pnlResizeRight.Cursor = System.Windows.Forms.Cursors.SizeWE;
      this.pnlResizeRight.Name = "pnlResizeRight";
      this.pnlResizeRight.MouseDown += new System.Windows.Forms.MouseEventHandler( this.pnlResize_MouseDown );
      this.pnlResizeRight.MouseUp += new System.Windows.Forms.MouseEventHandler( this.pnlResize_MouseUp );
      // 
      // picResizeCorner
      // 
      resources.ApplyResources( this.picResizeCorner, "picResizeCorner" );
      this.picResizeCorner.Cursor = System.Windows.Forms.Cursors.SizeNWSE;
      this.picResizeCorner.Image = global::BEGM.Properties.Resources.resize_corner;
      this.picResizeCorner.Name = "picResizeCorner";
      this.picResizeCorner.TabStop = false;
      this.picResizeCorner.MouseDown += new System.Windows.Forms.MouseEventHandler( this.pnlResize_MouseDown );
      this.picResizeCorner.MouseUp += new System.Windows.Forms.MouseEventHandler( this.pnlResize_MouseUp );
      // 
      // GameStatus
      // 
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
      this.BackColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 20 ) ) ) ), ( (int)( ( (byte)( 20 ) ) ) ), ( (int)( ( (byte)( 20 ) ) ) ) );
      resources.ApplyResources( this, "$this" );
      this.Controls.Add( this.picResizeCorner );
      this.Controls.Add( this.pnlResizeRight );
      this.Controls.Add( this.pnlResizeLeft );
      this.Controls.Add( this.lblInit );
      this.Controls.Add( this.picInit );
      this.Controls.Add( this.tblLayout );
      this.ForeColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ) );
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
      this.Name = "GameStatus";
      this.FormClosing += new System.Windows.Forms.FormClosingEventHandler( this.GameStatus_FormClosing );
      this.MouseEnter += new System.EventHandler( this.GameStatus_MouseEnter );
      this.Resize += new System.EventHandler( this.GameStatus_Resize );
      this.cmsTrayIcon.ResumeLayout( false );
      ( (System.ComponentModel.ISupportInitialize)( this.picClose ) ).EndInit();
      ( (System.ComponentModel.ISupportInitialize)( this.picBELogo ) ).EndInit();
      ( (System.ComponentModel.ISupportInitialize)( this.picInit ) ).EndInit();
      this.cmsLaunchGame.ResumeLayout( false );
      this.tblLayout.ResumeLayout( false );
      this.pnlHeader.ResumeLayout( false );
      this.pnlHeader.PerformLayout();
      this.pnlFooter.ResumeLayout( false );
      this.pnlMainColumn.ResumeLayout( false );
      ( (System.ComponentModel.ISupportInitialize)( this.tskMain ) ).EndInit();
      this.tskMain.ResumeLayout( false );
      ( (System.ComponentModel.ISupportInitialize)( this.expServerStatus ) ).EndInit();
      this.expServerStatus.ResumeLayout( false );
      ( (System.ComponentModel.ISupportInitialize)( this.expRecentEvents ) ).EndInit();
      this.expRecentEvents.ResumeLayout( false );
      ( (System.ComponentModel.ISupportInitialize)( this.expCurrentAttacks ) ).EndInit();
      this.expCurrentAttacks.ResumeLayout( false );
      ( (System.ComponentModel.ISupportInitialize)( this.expTownStatus ) ).EndInit();
      this.expTownStatus.ResumeLayout( false );
      ( (System.ComponentModel.ISupportInitialize)( this.expGameMap ) ).EndInit();
      this.expGameMap.ResumeLayout( false );
      ( (System.ComponentModel.ISupportInitialize)( this.expFactoryStatus ) ).EndInit();
      this.expFactoryStatus.ResumeLayout( false );
      ( (System.ComponentModel.ISupportInitialize)( this.expOrderOfBattle ) ).EndInit();
      this.expOrderOfBattle.ResumeLayout( false );
      ( (System.ComponentModel.ISupportInitialize)( this.expBrigadeStatus ) ).EndInit();
      this.expBrigadeStatus.ResumeLayout( false );
      ( (System.ComponentModel.ISupportInitialize)( this.expEquipment ) ).EndInit();
      this.expEquipment.ResumeLayout( false );
      ( (System.ComponentModel.ISupportInitialize)( this.picResizeCorner ) ).EndInit();
      this.ResumeLayout( false );
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.ContextMenuStrip cmsTrayIcon;
    private System.Windows.Forms.ToolStripMenuItem miTrayExit;
    private System.Windows.Forms.ToolStripMenuItem miTrayShowHide;
    private System.Windows.Forms.PictureBox picClose;
    private System.Windows.Forms.PictureBox picBELogo;
    private System.Windows.Forms.LinkLabel lnkOptions;
    private System.Windows.Forms.ToolStripMenuItem miTrayAbout;
    private System.Windows.Forms.ToolStripMenuItem miTrayOptions;
    private System.Windows.Forms.ToolStripSeparator miTraySep1;
    private System.Windows.Forms.Label lblStatus;
    private XPExplorerBar.TaskPane tskMain;
    private XPExplorerBar.Expando expRecentEvents;
    private XPExplorerBar.Expando expCurrentAttacks;
    private XPExplorerBar.Expando expFactoryStatus;
    private XPExplorerBar.Expando expOrderOfBattle;
    private System.Windows.Forms.LinkLabel lnkAbout;
    private System.Windows.Forms.LinkLabel lnkHelp;
    private BEGM.Widgets.TownStatus wgtTownStatus;
    private XPExplorerBar.Expando expTownStatus;
    private System.Windows.Forms.ToolTip toolTip;
    private BEGM.Widgets.FactoryStatus wgtFactoryStatus;
    private BEGM.Widgets.OrderOfBattle wgtOrderOfBattle;
    private System.Windows.Forms.Timer tmrStatusBar;
    private System.ComponentModel.BackgroundWorker bgwInit;
    private System.Windows.Forms.Timer tmrEventLoop;
    private System.Windows.Forms.ToolStripMenuItem miTraySleep;
    private System.ComponentModel.BackgroundWorker bgwPoll;
    public System.Windows.Forms.NotifyIcon trayIcon;
    private System.Windows.Forms.PictureBox picInit;
    private System.Windows.Forms.Label lblInit;
    private BEGM.Widgets.RecentEvents wgtRecentEvents;
    private BEGM.Widgets.CurrentAttacks wgtCurrentAttacks;
    private System.Windows.Forms.Label lblStatusMsg;
    private System.ComponentModel.BackgroundWorker bgwInitFactory;
    private System.Windows.Forms.ToolStripMenuItem miTrayShowLastAlerts;
    private System.Windows.Forms.ToolStripMenuItem miTrayDisableAllAlerts;
    private System.Windows.Forms.ToolStripSeparator miTraySep2;
    private XPExplorerBar.Expando expGameMap;
    private BEGM.Widgets.GameMap wgtGameMap;
    private System.Windows.Forms.LinkLabel lnkVersion;
    private System.Windows.Forms.Button btnLaunchGame;
    private System.Windows.Forms.LinkLabel lnkLaunchGame;
    private System.Windows.Forms.ContextMenuStrip cmsLaunchGame;
    private System.Windows.Forms.ToolStripMenuItem miLaunchGameOnlineLive;
    private System.Windows.Forms.ToolStripMenuItem miLaunchGameOfflineLive;
    private System.Windows.Forms.ToolStripMenuItem miLaunchGameOnlineTraining;
    private System.Windows.Forms.ToolStripMenuItem miLaunchGameOnlineBeta;
    private System.Windows.Forms.ToolStripMenuItem miLaunchGameOfflineBeta;
    private XPExplorerBar.Expando expServerStatus;
    private BEGM.Widgets.ServerStatus wgtServerStatus;
    private XPExplorerBar.Expando expEquipment;
    private BEGM.Widgets.Equipment wgtEquipment;
    private XPExplorerBar.Expando expBrigadeStatus;
    private BEGM.Widgets.BrigadeStatus wgtBrigadeStatus;
    private System.Windows.Forms.ToolStripMenuItem miLaunchGameOnline;
    private System.Windows.Forms.ToolStripMenuItem miLaunchGameOffline;
    private System.Windows.Forms.Panel pnlResizeLeft;
    private System.Windows.Forms.TableLayoutPanel tblLayout;
    private System.Windows.Forms.Panel pnlHeader;
    private System.Windows.Forms.Panel pnlFooter;
    private System.Windows.Forms.Panel pnlMainColumn;
    private System.Windows.Forms.Panel pnlResizeRight;
    private System.Windows.Forms.PictureBox picResizeCorner;
  }
}

