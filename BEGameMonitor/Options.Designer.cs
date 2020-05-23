namespace BEGM
{
  partial class Options
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager( typeof( Options ) );
      this.tabControl = new System.Windows.Forms.TabControl();
      this.tabNetwork = new System.Windows.Forms.TabPage();
      this.lblProxyServerHttp = new System.Windows.Forms.Label();
      this.lblWiretapServerHttp = new System.Windows.Forms.Label();
      this.lblForceNoProxy = new System.Windows.Forms.Label();
      this.lblWiretapPort = new System.Windows.Forms.Label();
      this.txtWiretapPort = new System.Windows.Forms.TextBox();
      this.lblProxyPort = new System.Windows.Forms.Label();
      this.txtProxyPort = new System.Windows.Forms.TextBox();
      this.lblTestResult = new System.Windows.Forms.Label();
      this.btnTestConn = new System.Windows.Forms.Button();
      this.txtProxyHost = new System.Windows.Forms.TextBox();
      this.rbUseCustomProxy = new System.Windows.Forms.RadioButton();
      this.rbUseIEProxy = new System.Windows.Forms.RadioButton();
      this.txtWiretapHost = new System.Windows.Forms.TextBox();
      this.lblWiretapServer = new System.Windows.Forms.Label();
      this.tabStartup = new System.Windows.Forms.TabPage();
      this.cbCheckVersion = new System.Windows.Forms.CheckBox();
      this.cbLoadFactoryData = new System.Windows.Forms.CheckBox();
      this.gbSleepMode = new System.Windows.Forms.GroupBox();
      this.lblSleepMode = new System.Windows.Forms.Label();
      this.lblSleepWhenIdle = new System.Windows.Forms.Label();
      this.cbWakeAfterPlay = new System.Windows.Forms.CheckBox();
      this.cbSleepWhenPlay = new System.Windows.Forms.CheckBox();
      this.cbSleepWhenIdle = new System.Windows.Forms.CheckBox();
      this.cbStartMinimised = new System.Windows.Forms.CheckBox();
      this.cbRunOnStartup = new System.Windows.Forms.CheckBox();
      this.tabAlerts = new System.Windows.Forms.TabPage();
      this.cbShowAlerts = new System.Windows.Forms.CheckBox();
      this.cbAlertFilters = new System.Windows.Forms.CheckBox();
      this.pnlAlertsOptions = new System.Windows.Forms.Panel();
      this.lblAutoNextTime = new System.Windows.Forms.Label();
      this.lblAutoNext = new System.Windows.Forms.Label();
      this.btnTestAlert = new System.Windows.Forms.Button();
      this.lblPostponeIdle = new System.Windows.Forms.Label();
      this.cbPostponeIdle = new System.Windows.Forms.CheckBox();
      this.cbPostponeFullscreen = new System.Windows.Forms.CheckBox();
      this.lblPostponeAlerts = new System.Windows.Forms.Label();
      this.cbPlayAlertSound = new System.Windows.Forms.CheckBox();
      this.lblAlertPosition = new System.Windows.Forms.Label();
      this.rbPositionBottom = new System.Windows.Forms.RadioButton();
      this.rbPositionTop = new System.Windows.Forms.RadioButton();
      this.pnlAlertsFilters = new System.Windows.Forms.Panel();
      this.cbAlwaysAlertChokePointCaptured = new System.Windows.Forms.CheckBox();
      this.tvwFilterEventType = new System.Windows.Forms.TreeView();
      this.cbAlwaysAlertUnderAttack = new System.Windows.Forms.CheckBox();
      this.tvwFilterChokePoint = new System.Windows.Forms.TreeView();
      this.cmsChokePoint = new System.Windows.Forms.ContextMenuStrip( this.components );
      this.itmChokePointSelectAll = new System.Windows.Forms.ToolStripMenuItem();
      this.itmChokePointClearAll = new System.Windows.Forms.ToolStripMenuItem();
      this.cbFilterCountry = new System.Windows.Forms.CheckBox();
      this.cbFilterChokePoint = new System.Windows.Forms.CheckBox();
      this.cbFilterEventType = new System.Windows.Forms.CheckBox();
      this.tvwFilterCountry = new System.Windows.Forms.TreeView();
      this.lblAlwaysAlert = new System.Windows.Forms.Label();
      this.gbFilterBy = new System.Windows.Forms.GroupBox();
      this.tabMap = new System.Windows.Forms.TabPage();
      this.cbWallOptions = new System.Windows.Forms.CheckBox();
      this.cbShowWallpaper = new System.Windows.Forms.CheckBox();
      this.pnlMapOptions = new System.Windows.Forms.Panel();
      this.rbMapSize40 = new System.Windows.Forms.RadioButton();
      this.gbFreeMem = new System.Windows.Forms.GroupBox();
      this.lblFreeMem = new System.Windows.Forms.Label();
      this.lblMapSize = new System.Windows.Forms.Label();
      this.cbAlwaysUseDefaultMapSize = new System.Windows.Forms.CheckBox();
      this.rbMapSize60 = new System.Windows.Forms.RadioButton();
      this.lblMemUsage100 = new System.Windows.Forms.Label();
      this.rbMapSize80 = new System.Windows.Forms.RadioButton();
      this.lblMemUsage80 = new System.Windows.Forms.Label();
      this.rbMapSize100 = new System.Windows.Forms.RadioButton();
      this.lblMemUsage60 = new System.Windows.Forms.Label();
      this.lblMemUsage40 = new System.Windows.Forms.Label();
      this.lblMemUsage = new System.Windows.Forms.Label();
      this.lnkMapPluginInfo = new System.Windows.Forms.LinkLabel();
      this.lblMapInfo = new System.Windows.Forms.Label();
      this.pnlMapWallOptions = new System.Windows.Forms.Panel();
      this.tvwMapOptions = new System.Windows.Forms.TreeView();
      this.lblWallOptions = new System.Windows.Forms.Label();
      this.btnApplyWallpaper = new System.Windows.Forms.Button();
      this.lblWallZoomMin = new System.Windows.Forms.Label();
      this.lblWallZoomMax = new System.Windows.Forms.Label();
      this.lblWallZoom = new System.Windows.Forms.Label();
      this.cbWallRemove = new System.Windows.Forms.CheckBox();
      this.lblWallUpdateTime = new System.Windows.Forms.Label();
      this.lblWallUpdate = new System.Windows.Forms.Label();
      this.tabMisc = new System.Windows.Forms.TabPage();
      this.cbDockWindow = new System.Windows.Forms.CheckBox();
      this.cmbAlertDisplay = new System.Windows.Forms.ComboBox();
      this.lblAlertDisplay = new System.Windows.Forms.Label();
      this.lblEventSort = new System.Windows.Forms.Label();
      this.pnlDockWindow = new System.Windows.Forms.Panel();
      this.rbDockWindowLeft = new System.Windows.Forms.RadioButton();
      this.rbDockWindowRight = new System.Windows.Forms.RadioButton();
      this.cmbGameStatusDisplay = new System.Windows.Forms.ComboBox();
      this.pnlEventSort = new System.Windows.Forms.Panel();
      this.rbEventSortBottom = new System.Windows.Forms.RadioButton();
      this.rbEventSortTop = new System.Windows.Forms.RadioButton();
      this.tabUpdates = new System.Windows.Forms.TabPage();
      this.gbResetAllData = new System.Windows.Forms.GroupBox();
      this.btnResetAllData = new System.Windows.Forms.Button();
      this.lblResetAllData = new System.Windows.Forms.Label();
      this.gbCheckEvents = new System.Windows.Forms.GroupBox();
      this.btnCheckEvents = new System.Windows.Forms.Button();
      this.lblCheckEvents = new System.Windows.Forms.Label();
      this.tabLanguage = new System.Windows.Forms.TabPage();
      this.radioButton1 = new System.Windows.Forms.RadioButton();
      this.rbLangSpanish = new System.Windows.Forms.RadioButton();
      this.rbLangGerman = new System.Windows.Forms.RadioButton();
      this.rbLangEnglishUS = new System.Windows.Forms.RadioButton();
      this.rbLangEnglishUK = new System.Windows.Forms.RadioButton();
      this.btnOK = new System.Windows.Forms.Button();
      this.btnDefaults = new System.Windows.Forms.Button();
      this.errorProvider = new System.Windows.Forms.ErrorProvider( this.components );
      this.btnCancel = new System.Windows.Forms.Button();
      this.tmrEnableNewEvents = new System.Windows.Forms.Timer( this.components );
      this.trkSleepWhenIdle = new Dotnetrix.Controls.TrackBar();
      this.trkAutoNextTime = new Dotnetrix.Controls.TrackBar();
      this.trkPostponeIdle = new Dotnetrix.Controls.TrackBar();
      this.trkWallZoom = new Dotnetrix.Controls.TrackBar();
      this.trkWallUpdateTime = new Dotnetrix.Controls.TrackBar();
      this.tabControl.SuspendLayout();
      this.tabNetwork.SuspendLayout();
      this.tabStartup.SuspendLayout();
      this.gbSleepMode.SuspendLayout();
      this.tabAlerts.SuspendLayout();
      this.pnlAlertsOptions.SuspendLayout();
      this.pnlAlertsFilters.SuspendLayout();
      this.cmsChokePoint.SuspendLayout();
      this.tabMap.SuspendLayout();
      this.pnlMapOptions.SuspendLayout();
      this.gbFreeMem.SuspendLayout();
      this.pnlMapWallOptions.SuspendLayout();
      this.tabMisc.SuspendLayout();
      this.pnlDockWindow.SuspendLayout();
      this.pnlEventSort.SuspendLayout();
      this.tabUpdates.SuspendLayout();
      this.gbResetAllData.SuspendLayout();
      this.gbCheckEvents.SuspendLayout();
      this.tabLanguage.SuspendLayout();
      ( (System.ComponentModel.ISupportInitialize)( this.errorProvider ) ).BeginInit();
      ( (System.ComponentModel.ISupportInitialize)( this.trkSleepWhenIdle ) ).BeginInit();
      ( (System.ComponentModel.ISupportInitialize)( this.trkAutoNextTime ) ).BeginInit();
      ( (System.ComponentModel.ISupportInitialize)( this.trkPostponeIdle ) ).BeginInit();
      ( (System.ComponentModel.ISupportInitialize)( this.trkWallZoom ) ).BeginInit();
      ( (System.ComponentModel.ISupportInitialize)( this.trkWallUpdateTime ) ).BeginInit();
      this.SuspendLayout();
      // 
      // tabControl
      // 
      resources.ApplyResources( this.tabControl, "tabControl" );
      this.tabControl.Controls.Add( this.tabNetwork );
      this.tabControl.Controls.Add( this.tabStartup );
      this.tabControl.Controls.Add( this.tabAlerts );
      this.tabControl.Controls.Add( this.tabMap );
      this.tabControl.Controls.Add( this.tabMisc );
      this.tabControl.Controls.Add( this.tabUpdates );
      this.tabControl.Controls.Add( this.tabLanguage );
      this.tabControl.Name = "tabControl";
      this.tabControl.SelectedIndex = 0;
      this.tabControl.SelectedIndexChanged += new System.EventHandler( this.tabControl_SelectedIndexChanged );
      // 
      // tabNetwork
      // 
      this.tabNetwork.Controls.Add( this.lblProxyServerHttp );
      this.tabNetwork.Controls.Add( this.lblWiretapServerHttp );
      this.tabNetwork.Controls.Add( this.lblForceNoProxy );
      this.tabNetwork.Controls.Add( this.lblWiretapPort );
      this.tabNetwork.Controls.Add( this.txtWiretapPort );
      this.tabNetwork.Controls.Add( this.lblProxyPort );
      this.tabNetwork.Controls.Add( this.txtProxyPort );
      this.tabNetwork.Controls.Add( this.lblTestResult );
      this.tabNetwork.Controls.Add( this.btnTestConn );
      this.tabNetwork.Controls.Add( this.txtProxyHost );
      this.tabNetwork.Controls.Add( this.rbUseCustomProxy );
      this.tabNetwork.Controls.Add( this.rbUseIEProxy );
      this.tabNetwork.Controls.Add( this.txtWiretapHost );
      this.tabNetwork.Controls.Add( this.lblWiretapServer );
      resources.ApplyResources( this.tabNetwork, "tabNetwork" );
      this.tabNetwork.Name = "tabNetwork";
      this.tabNetwork.UseVisualStyleBackColor = true;
      // 
      // lblProxyServerHttp
      // 
      resources.ApplyResources( this.lblProxyServerHttp, "lblProxyServerHttp" );
      this.lblProxyServerHttp.Name = "lblProxyServerHttp";
      // 
      // lblWiretapServerHttp
      // 
      resources.ApplyResources( this.lblWiretapServerHttp, "lblWiretapServerHttp" );
      this.lblWiretapServerHttp.Name = "lblWiretapServerHttp";
      // 
      // lblForceNoProxy
      // 
      resources.ApplyResources( this.lblForceNoProxy, "lblForceNoProxy" );
      this.lblForceNoProxy.ForeColor = System.Drawing.SystemColors.ControlDark;
      this.lblForceNoProxy.Name = "lblForceNoProxy";
      // 
      // lblWiretapPort
      // 
      resources.ApplyResources( this.lblWiretapPort, "lblWiretapPort" );
      this.lblWiretapPort.Name = "lblWiretapPort";
      // 
      // txtWiretapPort
      // 
      resources.ApplyResources( this.txtWiretapPort, "txtWiretapPort" );
      this.txtWiretapPort.Name = "txtWiretapPort";
      this.txtWiretapPort.Validating += new System.ComponentModel.CancelEventHandler( this.txtWiretapPort_Validating );
      // 
      // lblProxyPort
      // 
      resources.ApplyResources( this.lblProxyPort, "lblProxyPort" );
      this.lblProxyPort.Name = "lblProxyPort";
      // 
      // txtProxyPort
      // 
      resources.ApplyResources( this.txtProxyPort, "txtProxyPort" );
      this.txtProxyPort.Name = "txtProxyPort";
      this.txtProxyPort.Validating += new System.ComponentModel.CancelEventHandler( this.txtProxyPort_Validating );
      // 
      // lblTestResult
      // 
      resources.ApplyResources( this.lblTestResult, "lblTestResult" );
      this.lblTestResult.Name = "lblTestResult";
      // 
      // btnTestConn
      // 
      resources.ApplyResources( this.btnTestConn, "btnTestConn" );
      this.btnTestConn.Name = "btnTestConn";
      this.btnTestConn.UseVisualStyleBackColor = true;
      this.btnTestConn.Click += new System.EventHandler( this.btnTestConn_Click );
      // 
      // txtProxyHost
      // 
      resources.ApplyResources( this.txtProxyHost, "txtProxyHost" );
      this.txtProxyHost.Name = "txtProxyHost";
      // 
      // rbUseCustomProxy
      // 
      resources.ApplyResources( this.rbUseCustomProxy, "rbUseCustomProxy" );
      this.rbUseCustomProxy.Name = "rbUseCustomProxy";
      this.rbUseCustomProxy.UseVisualStyleBackColor = true;
      this.rbUseCustomProxy.CheckedChanged += new System.EventHandler( this.rbProxyServer_CheckedChanged );
      // 
      // rbUseIEProxy
      // 
      resources.ApplyResources( this.rbUseIEProxy, "rbUseIEProxy" );
      this.rbUseIEProxy.Checked = true;
      this.rbUseIEProxy.Name = "rbUseIEProxy";
      this.rbUseIEProxy.TabStop = true;
      this.rbUseIEProxy.UseVisualStyleBackColor = true;
      // 
      // txtWiretapHost
      // 
      resources.ApplyResources( this.txtWiretapHost, "txtWiretapHost" );
      this.txtWiretapHost.Name = "txtWiretapHost";
      // 
      // lblWiretapServer
      // 
      resources.ApplyResources( this.lblWiretapServer, "lblWiretapServer" );
      this.lblWiretapServer.Name = "lblWiretapServer";
      // 
      // tabStartup
      // 
      this.tabStartup.Controls.Add( this.cbCheckVersion );
      this.tabStartup.Controls.Add( this.cbLoadFactoryData );
      this.tabStartup.Controls.Add( this.gbSleepMode );
      this.tabStartup.Controls.Add( this.lblSleepWhenIdle );
      this.tabStartup.Controls.Add( this.cbWakeAfterPlay );
      this.tabStartup.Controls.Add( this.cbSleepWhenPlay );
      this.tabStartup.Controls.Add( this.cbSleepWhenIdle );
      this.tabStartup.Controls.Add( this.cbStartMinimised );
      this.tabStartup.Controls.Add( this.cbRunOnStartup );
      this.tabStartup.Controls.Add( this.trkSleepWhenIdle );
      resources.ApplyResources( this.tabStartup, "tabStartup" );
      this.tabStartup.Name = "tabStartup";
      this.tabStartup.UseVisualStyleBackColor = true;
      // 
      // cbCheckVersion
      // 
      resources.ApplyResources( this.cbCheckVersion, "cbCheckVersion" );
      this.cbCheckVersion.Name = "cbCheckVersion";
      this.cbCheckVersion.UseVisualStyleBackColor = true;
      // 
      // cbLoadFactoryData
      // 
      resources.ApplyResources( this.cbLoadFactoryData, "cbLoadFactoryData" );
      this.cbLoadFactoryData.Name = "cbLoadFactoryData";
      this.cbLoadFactoryData.UseVisualStyleBackColor = true;
      this.cbLoadFactoryData.CheckedChanged += new System.EventHandler( this.cbLoadFactoryData_CheckedChanged );
      // 
      // gbSleepMode
      // 
      resources.ApplyResources( this.gbSleepMode, "gbSleepMode" );
      this.gbSleepMode.Controls.Add( this.lblSleepMode );
      this.gbSleepMode.Name = "gbSleepMode";
      this.gbSleepMode.TabStop = false;
      // 
      // lblSleepMode
      // 
      resources.ApplyResources( this.lblSleepMode, "lblSleepMode" );
      this.lblSleepMode.Name = "lblSleepMode";
      // 
      // lblSleepWhenIdle
      // 
      resources.ApplyResources( this.lblSleepWhenIdle, "lblSleepWhenIdle" );
      this.lblSleepWhenIdle.Name = "lblSleepWhenIdle";
      // 
      // cbWakeAfterPlay
      // 
      resources.ApplyResources( this.cbWakeAfterPlay, "cbWakeAfterPlay" );
      this.cbWakeAfterPlay.Checked = true;
      this.cbWakeAfterPlay.CheckState = System.Windows.Forms.CheckState.Checked;
      this.cbWakeAfterPlay.Name = "cbWakeAfterPlay";
      this.cbWakeAfterPlay.UseVisualStyleBackColor = true;
      // 
      // cbSleepWhenPlay
      // 
      resources.ApplyResources( this.cbSleepWhenPlay, "cbSleepWhenPlay" );
      this.cbSleepWhenPlay.Checked = true;
      this.cbSleepWhenPlay.CheckState = System.Windows.Forms.CheckState.Checked;
      this.cbSleepWhenPlay.Name = "cbSleepWhenPlay";
      this.cbSleepWhenPlay.UseVisualStyleBackColor = true;
      this.cbSleepWhenPlay.CheckedChanged += new System.EventHandler( this.cbSleepWhenPlay_CheckedChanged );
      // 
      // cbSleepWhenIdle
      // 
      resources.ApplyResources( this.cbSleepWhenIdle, "cbSleepWhenIdle" );
      this.cbSleepWhenIdle.Checked = true;
      this.cbSleepWhenIdle.CheckState = System.Windows.Forms.CheckState.Checked;
      this.cbSleepWhenIdle.Name = "cbSleepWhenIdle";
      this.cbSleepWhenIdle.UseVisualStyleBackColor = true;
      this.cbSleepWhenIdle.CheckedChanged += new System.EventHandler( this.cbSleepWhenIdle_CheckedChanged );
      // 
      // cbStartMinimised
      // 
      resources.ApplyResources( this.cbStartMinimised, "cbStartMinimised" );
      this.cbStartMinimised.Name = "cbStartMinimised";
      this.cbStartMinimised.UseVisualStyleBackColor = true;
      // 
      // cbRunOnStartup
      // 
      resources.ApplyResources( this.cbRunOnStartup, "cbRunOnStartup" );
      this.cbRunOnStartup.Name = "cbRunOnStartup";
      this.cbRunOnStartup.UseVisualStyleBackColor = true;
      // 
      // tabAlerts
      // 
      this.tabAlerts.Controls.Add( this.cbShowAlerts );
      this.tabAlerts.Controls.Add( this.cbAlertFilters );
      this.tabAlerts.Controls.Add( this.pnlAlertsOptions );
      this.tabAlerts.Controls.Add( this.pnlAlertsFilters );
      resources.ApplyResources( this.tabAlerts, "tabAlerts" );
      this.tabAlerts.Name = "tabAlerts";
      this.tabAlerts.UseVisualStyleBackColor = true;
      // 
      // cbShowAlerts
      // 
      resources.ApplyResources( this.cbShowAlerts, "cbShowAlerts" );
      this.cbShowAlerts.Checked = true;
      this.cbShowAlerts.CheckState = System.Windows.Forms.CheckState.Checked;
      this.cbShowAlerts.Name = "cbShowAlerts";
      this.cbShowAlerts.UseVisualStyleBackColor = true;
      this.cbShowAlerts.CheckedChanged += new System.EventHandler( this.cbShowAlerts_CheckedChanged );
      // 
      // cbAlertFilters
      // 
      resources.ApplyResources( this.cbAlertFilters, "cbAlertFilters" );
      this.cbAlertFilters.Name = "cbAlertFilters";
      this.cbAlertFilters.UseVisualStyleBackColor = true;
      this.cbAlertFilters.CheckedChanged += new System.EventHandler( this.cbAlertFilters_CheckedChanged );
      // 
      // pnlAlertsOptions
      // 
      this.pnlAlertsOptions.Controls.Add( this.lblAutoNextTime );
      this.pnlAlertsOptions.Controls.Add( this.trkAutoNextTime );
      this.pnlAlertsOptions.Controls.Add( this.lblAutoNext );
      this.pnlAlertsOptions.Controls.Add( this.btnTestAlert );
      this.pnlAlertsOptions.Controls.Add( this.lblPostponeIdle );
      this.pnlAlertsOptions.Controls.Add( this.trkPostponeIdle );
      this.pnlAlertsOptions.Controls.Add( this.cbPostponeIdle );
      this.pnlAlertsOptions.Controls.Add( this.cbPostponeFullscreen );
      this.pnlAlertsOptions.Controls.Add( this.lblPostponeAlerts );
      this.pnlAlertsOptions.Controls.Add( this.cbPlayAlertSound );
      this.pnlAlertsOptions.Controls.Add( this.lblAlertPosition );
      this.pnlAlertsOptions.Controls.Add( this.rbPositionBottom );
      this.pnlAlertsOptions.Controls.Add( this.rbPositionTop );
      resources.ApplyResources( this.pnlAlertsOptions, "pnlAlertsOptions" );
      this.pnlAlertsOptions.Name = "pnlAlertsOptions";
      // 
      // lblAutoNextTime
      // 
      resources.ApplyResources( this.lblAutoNextTime, "lblAutoNextTime" );
      this.lblAutoNextTime.Name = "lblAutoNextTime";
      // 
      // lblAutoNext
      // 
      resources.ApplyResources( this.lblAutoNext, "lblAutoNext" );
      this.lblAutoNext.Name = "lblAutoNext";
      // 
      // btnTestAlert
      // 
      resources.ApplyResources( this.btnTestAlert, "btnTestAlert" );
      this.btnTestAlert.Name = "btnTestAlert";
      this.btnTestAlert.UseVisualStyleBackColor = true;
      this.btnTestAlert.Click += new System.EventHandler( this.btnTestAlert_Click );
      // 
      // lblPostponeIdle
      // 
      resources.ApplyResources( this.lblPostponeIdle, "lblPostponeIdle" );
      this.lblPostponeIdle.Name = "lblPostponeIdle";
      // 
      // cbPostponeIdle
      // 
      resources.ApplyResources( this.cbPostponeIdle, "cbPostponeIdle" );
      this.cbPostponeIdle.Checked = true;
      this.cbPostponeIdle.CheckState = System.Windows.Forms.CheckState.Checked;
      this.cbPostponeIdle.Name = "cbPostponeIdle";
      this.cbPostponeIdle.UseVisualStyleBackColor = true;
      this.cbPostponeIdle.CheckedChanged += new System.EventHandler( this.cbPostponeIdle_CheckedChanged );
      // 
      // cbPostponeFullscreen
      // 
      resources.ApplyResources( this.cbPostponeFullscreen, "cbPostponeFullscreen" );
      this.cbPostponeFullscreen.Checked = true;
      this.cbPostponeFullscreen.CheckState = System.Windows.Forms.CheckState.Checked;
      this.cbPostponeFullscreen.Name = "cbPostponeFullscreen";
      this.cbPostponeFullscreen.UseVisualStyleBackColor = true;
      // 
      // lblPostponeAlerts
      // 
      resources.ApplyResources( this.lblPostponeAlerts, "lblPostponeAlerts" );
      this.lblPostponeAlerts.Name = "lblPostponeAlerts";
      // 
      // cbPlayAlertSound
      // 
      resources.ApplyResources( this.cbPlayAlertSound, "cbPlayAlertSound" );
      this.cbPlayAlertSound.Name = "cbPlayAlertSound";
      this.cbPlayAlertSound.UseVisualStyleBackColor = true;
      // 
      // lblAlertPosition
      // 
      resources.ApplyResources( this.lblAlertPosition, "lblAlertPosition" );
      this.lblAlertPosition.Name = "lblAlertPosition";
      // 
      // rbPositionBottom
      // 
      resources.ApplyResources( this.rbPositionBottom, "rbPositionBottom" );
      this.rbPositionBottom.Checked = true;
      this.rbPositionBottom.Name = "rbPositionBottom";
      this.rbPositionBottom.TabStop = true;
      this.rbPositionBottom.UseVisualStyleBackColor = true;
      // 
      // rbPositionTop
      // 
      resources.ApplyResources( this.rbPositionTop, "rbPositionTop" );
      this.rbPositionTop.Name = "rbPositionTop";
      this.rbPositionTop.TabStop = true;
      this.rbPositionTop.UseVisualStyleBackColor = true;
      // 
      // pnlAlertsFilters
      // 
      this.pnlAlertsFilters.Controls.Add( this.cbAlwaysAlertChokePointCaptured );
      this.pnlAlertsFilters.Controls.Add( this.tvwFilterEventType );
      this.pnlAlertsFilters.Controls.Add( this.cbAlwaysAlertUnderAttack );
      this.pnlAlertsFilters.Controls.Add( this.tvwFilterChokePoint );
      this.pnlAlertsFilters.Controls.Add( this.cbFilterCountry );
      this.pnlAlertsFilters.Controls.Add( this.cbFilterChokePoint );
      this.pnlAlertsFilters.Controls.Add( this.cbFilterEventType );
      this.pnlAlertsFilters.Controls.Add( this.tvwFilterCountry );
      this.pnlAlertsFilters.Controls.Add( this.lblAlwaysAlert );
      this.pnlAlertsFilters.Controls.Add( this.gbFilterBy );
      resources.ApplyResources( this.pnlAlertsFilters, "pnlAlertsFilters" );
      this.pnlAlertsFilters.Name = "pnlAlertsFilters";
      // 
      // cbAlwaysAlertChokePointCaptured
      // 
      resources.ApplyResources( this.cbAlwaysAlertChokePointCaptured, "cbAlwaysAlertChokePointCaptured" );
      this.cbAlwaysAlertChokePointCaptured.Checked = true;
      this.cbAlwaysAlertChokePointCaptured.CheckState = System.Windows.Forms.CheckState.Checked;
      this.cbAlwaysAlertChokePointCaptured.Name = "cbAlwaysAlertChokePointCaptured";
      this.cbAlwaysAlertChokePointCaptured.UseVisualStyleBackColor = true;
      // 
      // tvwFilterEventType
      // 
      this.tvwFilterEventType.BackColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 238 ) ) ) ), ( (int)( ( (byte)( 239 ) ) ) ), ( (int)( ( (byte)( 247 ) ) ) ) );
      this.tvwFilterEventType.CheckBoxes = true;
      resources.ApplyResources( this.tvwFilterEventType, "tvwFilterEventType" );
      this.tvwFilterEventType.Name = "tvwFilterEventType";
      this.tvwFilterEventType.Nodes.AddRange( new System.Windows.Forms.TreeNode[] {
            ((System.Windows.Forms.TreeNode)(resources.GetObject("tvwFilterEventType.Nodes"))),
            ((System.Windows.Forms.TreeNode)(resources.GetObject("tvwFilterEventType.Nodes1"))),
            ((System.Windows.Forms.TreeNode)(resources.GetObject("tvwFilterEventType.Nodes2"))),
            ((System.Windows.Forms.TreeNode)(resources.GetObject("tvwFilterEventType.Nodes3"))),
            ((System.Windows.Forms.TreeNode)(resources.GetObject("tvwFilterEventType.Nodes4")))} );
      this.tvwFilterEventType.ShowNodeToolTips = true;
      this.tvwFilterEventType.AfterCheck += new System.Windows.Forms.TreeViewEventHandler( this.tvwFilterEventType_AfterCheck );
      // 
      // cbAlwaysAlertUnderAttack
      // 
      resources.ApplyResources( this.cbAlwaysAlertUnderAttack, "cbAlwaysAlertUnderAttack" );
      this.cbAlwaysAlertUnderAttack.Checked = true;
      this.cbAlwaysAlertUnderAttack.CheckState = System.Windows.Forms.CheckState.Checked;
      this.cbAlwaysAlertUnderAttack.Name = "cbAlwaysAlertUnderAttack";
      this.cbAlwaysAlertUnderAttack.UseVisualStyleBackColor = true;
      // 
      // tvwFilterChokePoint
      // 
      this.tvwFilterChokePoint.BackColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 238 ) ) ) ), ( (int)( ( (byte)( 239 ) ) ) ), ( (int)( ( (byte)( 247 ) ) ) ) );
      this.tvwFilterChokePoint.CheckBoxes = true;
      this.tvwFilterChokePoint.ContextMenuStrip = this.cmsChokePoint;
      resources.ApplyResources( this.tvwFilterChokePoint, "tvwFilterChokePoint" );
      this.tvwFilterChokePoint.Name = "tvwFilterChokePoint";
      this.tvwFilterChokePoint.ShowLines = false;
      this.tvwFilterChokePoint.ShowPlusMinus = false;
      this.tvwFilterChokePoint.ShowRootLines = false;
      // 
      // cmsChokePoint
      // 
      this.cmsChokePoint.Items.AddRange( new System.Windows.Forms.ToolStripItem[] {
            this.itmChokePointSelectAll,
            this.itmChokePointClearAll} );
      this.cmsChokePoint.Name = "cmsChokePoint";
      this.cmsChokePoint.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
      this.cmsChokePoint.ShowImageMargin = false;
      resources.ApplyResources( this.cmsChokePoint, "cmsChokePoint" );
      // 
      // itmChokePointSelectAll
      // 
      this.itmChokePointSelectAll.Name = "itmChokePointSelectAll";
      resources.ApplyResources( this.itmChokePointSelectAll, "itmChokePointSelectAll" );
      this.itmChokePointSelectAll.Click += new System.EventHandler( this.itmChokePointSelectAll_Click );
      // 
      // itmChokePointClearAll
      // 
      this.itmChokePointClearAll.Name = "itmChokePointClearAll";
      resources.ApplyResources( this.itmChokePointClearAll, "itmChokePointClearAll" );
      this.itmChokePointClearAll.Click += new System.EventHandler( this.itmChokePointClearAll_Click );
      // 
      // cbFilterCountry
      // 
      resources.ApplyResources( this.cbFilterCountry, "cbFilterCountry" );
      this.cbFilterCountry.Name = "cbFilterCountry";
      this.cbFilterCountry.UseVisualStyleBackColor = true;
      this.cbFilterCountry.CheckedChanged += new System.EventHandler( this.cbFilterCountry_CheckedChanged );
      // 
      // cbFilterChokePoint
      // 
      resources.ApplyResources( this.cbFilterChokePoint, "cbFilterChokePoint" );
      this.cbFilterChokePoint.Name = "cbFilterChokePoint";
      this.cbFilterChokePoint.UseVisualStyleBackColor = true;
      this.cbFilterChokePoint.CheckedChanged += new System.EventHandler( this.cbFilterChokePoint_CheckedChanged );
      // 
      // cbFilterEventType
      // 
      resources.ApplyResources( this.cbFilterEventType, "cbFilterEventType" );
      this.cbFilterEventType.Name = "cbFilterEventType";
      this.cbFilterEventType.UseVisualStyleBackColor = true;
      this.cbFilterEventType.CheckedChanged += new System.EventHandler( this.cbFilterEventType_CheckedChanged );
      // 
      // tvwFilterCountry
      // 
      this.tvwFilterCountry.BackColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 238 ) ) ) ), ( (int)( ( (byte)( 239 ) ) ) ), ( (int)( ( (byte)( 247 ) ) ) ) );
      this.tvwFilterCountry.CheckBoxes = true;
      resources.ApplyResources( this.tvwFilterCountry, "tvwFilterCountry" );
      this.tvwFilterCountry.Name = "tvwFilterCountry";
      this.tvwFilterCountry.Nodes.AddRange( new System.Windows.Forms.TreeNode[] {
            ((System.Windows.Forms.TreeNode)(resources.GetObject("tvwFilterCountry.Nodes"))),
            ((System.Windows.Forms.TreeNode)(resources.GetObject("tvwFilterCountry.Nodes1"))),
            ((System.Windows.Forms.TreeNode)(resources.GetObject("tvwFilterCountry.Nodes2")))} );
      this.tvwFilterCountry.ShowLines = false;
      this.tvwFilterCountry.ShowPlusMinus = false;
      this.tvwFilterCountry.ShowRootLines = false;
      // 
      // lblAlwaysAlert
      // 
      resources.ApplyResources( this.lblAlwaysAlert, "lblAlwaysAlert" );
      this.lblAlwaysAlert.Name = "lblAlwaysAlert";
      // 
      // gbFilterBy
      // 
      resources.ApplyResources( this.gbFilterBy, "gbFilterBy" );
      this.gbFilterBy.Name = "gbFilterBy";
      this.gbFilterBy.TabStop = false;
      // 
      // tabMap
      // 
      this.tabMap.Controls.Add( this.cbWallOptions );
      this.tabMap.Controls.Add( this.cbShowWallpaper );
      this.tabMap.Controls.Add( this.pnlMapOptions );
      this.tabMap.Controls.Add( this.pnlMapWallOptions );
      resources.ApplyResources( this.tabMap, "tabMap" );
      this.tabMap.Name = "tabMap";
      this.tabMap.UseVisualStyleBackColor = true;
      // 
      // cbWallOptions
      // 
      resources.ApplyResources( this.cbWallOptions, "cbWallOptions" );
      this.cbWallOptions.Name = "cbWallOptions";
      this.cbWallOptions.UseVisualStyleBackColor = true;
      this.cbWallOptions.CheckedChanged += new System.EventHandler( this.cbWallOptions_CheckedChanged );
      // 
      // cbShowWallpaper
      // 
      resources.ApplyResources( this.cbShowWallpaper, "cbShowWallpaper" );
      this.cbShowWallpaper.Name = "cbShowWallpaper";
      this.cbShowWallpaper.UseVisualStyleBackColor = true;
      this.cbShowWallpaper.CheckedChanged += new System.EventHandler( this.cbShowWallpaper_CheckedChanged );
      // 
      // pnlMapOptions
      // 
      this.pnlMapOptions.Controls.Add( this.rbMapSize40 );
      this.pnlMapOptions.Controls.Add( this.gbFreeMem );
      this.pnlMapOptions.Controls.Add( this.lblMapSize );
      this.pnlMapOptions.Controls.Add( this.cbAlwaysUseDefaultMapSize );
      this.pnlMapOptions.Controls.Add( this.rbMapSize60 );
      this.pnlMapOptions.Controls.Add( this.lblMemUsage100 );
      this.pnlMapOptions.Controls.Add( this.rbMapSize80 );
      this.pnlMapOptions.Controls.Add( this.lblMemUsage80 );
      this.pnlMapOptions.Controls.Add( this.rbMapSize100 );
      this.pnlMapOptions.Controls.Add( this.lblMemUsage60 );
      this.pnlMapOptions.Controls.Add( this.lblMemUsage40 );
      this.pnlMapOptions.Controls.Add( this.lblMemUsage );
      this.pnlMapOptions.Controls.Add( this.lnkMapPluginInfo );
      this.pnlMapOptions.Controls.Add( this.lblMapInfo );
      resources.ApplyResources( this.pnlMapOptions, "pnlMapOptions" );
      this.pnlMapOptions.Name = "pnlMapOptions";
      // 
      // rbMapSize40
      // 
      resources.ApplyResources( this.rbMapSize40, "rbMapSize40" );
      this.rbMapSize40.Checked = true;
      this.rbMapSize40.Name = "rbMapSize40";
      this.rbMapSize40.TabStop = true;
      this.rbMapSize40.UseVisualStyleBackColor = true;
      // 
      // gbFreeMem
      // 
      this.gbFreeMem.Controls.Add( this.lblFreeMem );
      resources.ApplyResources( this.gbFreeMem, "gbFreeMem" );
      this.gbFreeMem.Name = "gbFreeMem";
      this.gbFreeMem.TabStop = false;
      // 
      // lblFreeMem
      // 
      resources.ApplyResources( this.lblFreeMem, "lblFreeMem" );
      this.lblFreeMem.Name = "lblFreeMem";
      // 
      // lblMapSize
      // 
      resources.ApplyResources( this.lblMapSize, "lblMapSize" );
      this.lblMapSize.Name = "lblMapSize";
      // 
      // cbAlwaysUseDefaultMapSize
      // 
      resources.ApplyResources( this.cbAlwaysUseDefaultMapSize, "cbAlwaysUseDefaultMapSize" );
      this.cbAlwaysUseDefaultMapSize.Checked = true;
      this.cbAlwaysUseDefaultMapSize.CheckState = System.Windows.Forms.CheckState.Checked;
      this.cbAlwaysUseDefaultMapSize.Name = "cbAlwaysUseDefaultMapSize";
      this.cbAlwaysUseDefaultMapSize.UseVisualStyleBackColor = true;
      // 
      // rbMapSize60
      // 
      resources.ApplyResources( this.rbMapSize60, "rbMapSize60" );
      this.rbMapSize60.Name = "rbMapSize60";
      this.rbMapSize60.UseVisualStyleBackColor = true;
      // 
      // lblMemUsage100
      // 
      resources.ApplyResources( this.lblMemUsage100, "lblMemUsage100" );
      this.lblMemUsage100.Name = "lblMemUsage100";
      // 
      // rbMapSize80
      // 
      resources.ApplyResources( this.rbMapSize80, "rbMapSize80" );
      this.rbMapSize80.Name = "rbMapSize80";
      this.rbMapSize80.UseVisualStyleBackColor = true;
      // 
      // lblMemUsage80
      // 
      resources.ApplyResources( this.lblMemUsage80, "lblMemUsage80" );
      this.lblMemUsage80.Name = "lblMemUsage80";
      // 
      // rbMapSize100
      // 
      resources.ApplyResources( this.rbMapSize100, "rbMapSize100" );
      this.rbMapSize100.Name = "rbMapSize100";
      this.rbMapSize100.UseVisualStyleBackColor = true;
      // 
      // lblMemUsage60
      // 
      resources.ApplyResources( this.lblMemUsage60, "lblMemUsage60" );
      this.lblMemUsage60.Name = "lblMemUsage60";
      // 
      // lblMemUsage40
      // 
      resources.ApplyResources( this.lblMemUsage40, "lblMemUsage40" );
      this.lblMemUsage40.Name = "lblMemUsage40";
      // 
      // lblMemUsage
      // 
      resources.ApplyResources( this.lblMemUsage, "lblMemUsage" );
      this.lblMemUsage.Name = "lblMemUsage";
      // 
      // lnkMapPluginInfo
      // 
      resources.ApplyResources( this.lnkMapPluginInfo, "lnkMapPluginInfo" );
      this.lnkMapPluginInfo.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
      this.lnkMapPluginInfo.Name = "lnkMapPluginInfo";
      this.lnkMapPluginInfo.TabStop = true;
      this.lnkMapPluginInfo.UseCompatibleTextRendering = true;
      this.lnkMapPluginInfo.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler( this.lnkMapPluginInfo_LinkClicked );
      // 
      // lblMapInfo
      // 
      resources.ApplyResources( this.lblMapInfo, "lblMapInfo" );
      this.lblMapInfo.Name = "lblMapInfo";
      // 
      // pnlMapWallOptions
      // 
      this.pnlMapWallOptions.Controls.Add( this.tvwMapOptions );
      this.pnlMapWallOptions.Controls.Add( this.lblWallOptions );
      this.pnlMapWallOptions.Controls.Add( this.btnApplyWallpaper );
      this.pnlMapWallOptions.Controls.Add( this.lblWallZoomMin );
      this.pnlMapWallOptions.Controls.Add( this.lblWallZoomMax );
      this.pnlMapWallOptions.Controls.Add( this.trkWallZoom );
      this.pnlMapWallOptions.Controls.Add( this.lblWallZoom );
      this.pnlMapWallOptions.Controls.Add( this.cbWallRemove );
      this.pnlMapWallOptions.Controls.Add( this.lblWallUpdateTime );
      this.pnlMapWallOptions.Controls.Add( this.trkWallUpdateTime );
      this.pnlMapWallOptions.Controls.Add( this.lblWallUpdate );
      resources.ApplyResources( this.pnlMapWallOptions, "pnlMapWallOptions" );
      this.pnlMapWallOptions.Name = "pnlMapWallOptions";
      // 
      // tvwMapOptions
      // 
      this.tvwMapOptions.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.tvwMapOptions.CheckBoxes = true;
      resources.ApplyResources( this.tvwMapOptions, "tvwMapOptions" );
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
      this.tvwMapOptions.ShowLines = false;
      this.tvwMapOptions.ShowPlusMinus = false;
      this.tvwMapOptions.ShowRootLines = false;
      this.tvwMapOptions.AfterCheck += new System.Windows.Forms.TreeViewEventHandler( this.tvwMapOptions_AfterCheck );
      this.tvwMapOptions.BeforeCollapse += new System.Windows.Forms.TreeViewCancelEventHandler( this.tvwMapOptions_BeforeCollapse );
      this.tvwMapOptions.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler( this.tvwMapOptions_BeforeSelect );
      // 
      // lblWallOptions
      // 
      resources.ApplyResources( this.lblWallOptions, "lblWallOptions" );
      this.lblWallOptions.Name = "lblWallOptions";
      // 
      // btnApplyWallpaper
      // 
      resources.ApplyResources( this.btnApplyWallpaper, "btnApplyWallpaper" );
      this.btnApplyWallpaper.Name = "btnApplyWallpaper";
      this.btnApplyWallpaper.UseVisualStyleBackColor = true;
      this.btnApplyWallpaper.Click += new System.EventHandler( this.btnApplyWallpaper_Click );
      // 
      // lblWallZoomMin
      // 
      resources.ApplyResources( this.lblWallZoomMin, "lblWallZoomMin" );
      this.lblWallZoomMin.Name = "lblWallZoomMin";
      // 
      // lblWallZoomMax
      // 
      resources.ApplyResources( this.lblWallZoomMax, "lblWallZoomMax" );
      this.lblWallZoomMax.Name = "lblWallZoomMax";
      // 
      // lblWallZoom
      // 
      resources.ApplyResources( this.lblWallZoom, "lblWallZoom" );
      this.lblWallZoom.Name = "lblWallZoom";
      // 
      // cbWallRemove
      // 
      resources.ApplyResources( this.cbWallRemove, "cbWallRemove" );
      this.cbWallRemove.Name = "cbWallRemove";
      this.cbWallRemove.UseVisualStyleBackColor = true;
      // 
      // lblWallUpdateTime
      // 
      resources.ApplyResources( this.lblWallUpdateTime, "lblWallUpdateTime" );
      this.lblWallUpdateTime.Name = "lblWallUpdateTime";
      // 
      // lblWallUpdate
      // 
      resources.ApplyResources( this.lblWallUpdate, "lblWallUpdate" );
      this.lblWallUpdate.Name = "lblWallUpdate";
      // 
      // tabMisc
      // 
      this.tabMisc.Controls.Add( this.cbDockWindow );
      this.tabMisc.Controls.Add( this.cmbAlertDisplay );
      this.tabMisc.Controls.Add( this.lblAlertDisplay );
      this.tabMisc.Controls.Add( this.lblEventSort );
      this.tabMisc.Controls.Add( this.pnlDockWindow );
      this.tabMisc.Controls.Add( this.pnlEventSort );
      resources.ApplyResources( this.tabMisc, "tabMisc" );
      this.tabMisc.Name = "tabMisc";
      this.tabMisc.UseVisualStyleBackColor = true;
      // 
      // cbDockWindow
      // 
      resources.ApplyResources( this.cbDockWindow, "cbDockWindow" );
      this.cbDockWindow.Checked = true;
      this.cbDockWindow.CheckState = System.Windows.Forms.CheckState.Checked;
      this.cbDockWindow.Name = "cbDockWindow";
      this.cbDockWindow.UseVisualStyleBackColor = true;
      this.cbDockWindow.CheckedChanged += new System.EventHandler( this.cbDockWindow_CheckedChanged );
      // 
      // cmbAlertDisplay
      // 
      this.cmbAlertDisplay.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cmbAlertDisplay.FormattingEnabled = true;
      resources.ApplyResources( this.cmbAlertDisplay, "cmbAlertDisplay" );
      this.cmbAlertDisplay.Name = "cmbAlertDisplay";
      // 
      // lblAlertDisplay
      // 
      resources.ApplyResources( this.lblAlertDisplay, "lblAlertDisplay" );
      this.lblAlertDisplay.Name = "lblAlertDisplay";
      // 
      // lblEventSort
      // 
      resources.ApplyResources( this.lblEventSort, "lblEventSort" );
      this.lblEventSort.Name = "lblEventSort";
      // 
      // pnlDockWindow
      // 
      this.pnlDockWindow.Controls.Add( this.rbDockWindowLeft );
      this.pnlDockWindow.Controls.Add( this.rbDockWindowRight );
      this.pnlDockWindow.Controls.Add( this.cmbGameStatusDisplay );
      resources.ApplyResources( this.pnlDockWindow, "pnlDockWindow" );
      this.pnlDockWindow.Name = "pnlDockWindow";
      // 
      // rbDockWindowLeft
      // 
      resources.ApplyResources( this.rbDockWindowLeft, "rbDockWindowLeft" );
      this.rbDockWindowLeft.Name = "rbDockWindowLeft";
      this.rbDockWindowLeft.UseVisualStyleBackColor = true;
      // 
      // rbDockWindowRight
      // 
      resources.ApplyResources( this.rbDockWindowRight, "rbDockWindowRight" );
      this.rbDockWindowRight.Checked = true;
      this.rbDockWindowRight.Name = "rbDockWindowRight";
      this.rbDockWindowRight.TabStop = true;
      this.rbDockWindowRight.UseVisualStyleBackColor = true;
      // 
      // cmbGameStatusDisplay
      // 
      this.cmbGameStatusDisplay.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cmbGameStatusDisplay.FormatString = "Display 0";
      this.cmbGameStatusDisplay.FormattingEnabled = true;
      resources.ApplyResources( this.cmbGameStatusDisplay, "cmbGameStatusDisplay" );
      this.cmbGameStatusDisplay.Name = "cmbGameStatusDisplay";
      // 
      // pnlEventSort
      // 
      this.pnlEventSort.Controls.Add( this.rbEventSortBottom );
      this.pnlEventSort.Controls.Add( this.rbEventSortTop );
      resources.ApplyResources( this.pnlEventSort, "pnlEventSort" );
      this.pnlEventSort.Name = "pnlEventSort";
      // 
      // rbEventSortBottom
      // 
      resources.ApplyResources( this.rbEventSortBottom, "rbEventSortBottom" );
      this.rbEventSortBottom.Checked = true;
      this.rbEventSortBottom.Name = "rbEventSortBottom";
      this.rbEventSortBottom.TabStop = true;
      this.rbEventSortBottom.UseVisualStyleBackColor = true;
      // 
      // rbEventSortTop
      // 
      resources.ApplyResources( this.rbEventSortTop, "rbEventSortTop" );
      this.rbEventSortTop.Name = "rbEventSortTop";
      this.rbEventSortTop.UseVisualStyleBackColor = true;
      // 
      // tabUpdates
      // 
      this.tabUpdates.Controls.Add( this.gbResetAllData );
      this.tabUpdates.Controls.Add( this.gbCheckEvents );
      resources.ApplyResources( this.tabUpdates, "tabUpdates" );
      this.tabUpdates.Name = "tabUpdates";
      this.tabUpdates.UseVisualStyleBackColor = true;
      // 
      // gbResetAllData
      // 
      resources.ApplyResources( this.gbResetAllData, "gbResetAllData" );
      this.gbResetAllData.Controls.Add( this.btnResetAllData );
      this.gbResetAllData.Controls.Add( this.lblResetAllData );
      this.gbResetAllData.Name = "gbResetAllData";
      this.gbResetAllData.TabStop = false;
      // 
      // btnResetAllData
      // 
      resources.ApplyResources( this.btnResetAllData, "btnResetAllData" );
      this.btnResetAllData.Name = "btnResetAllData";
      this.btnResetAllData.UseVisualStyleBackColor = true;
      this.btnResetAllData.Click += new System.EventHandler( this.btnResetAllData_Click );
      // 
      // lblResetAllData
      // 
      resources.ApplyResources( this.lblResetAllData, "lblResetAllData" );
      this.lblResetAllData.Name = "lblResetAllData";
      // 
      // gbCheckEvents
      // 
      resources.ApplyResources( this.gbCheckEvents, "gbCheckEvents" );
      this.gbCheckEvents.Controls.Add( this.btnCheckEvents );
      this.gbCheckEvents.Controls.Add( this.lblCheckEvents );
      this.gbCheckEvents.Name = "gbCheckEvents";
      this.gbCheckEvents.TabStop = false;
      // 
      // btnCheckEvents
      // 
      resources.ApplyResources( this.btnCheckEvents, "btnCheckEvents" );
      this.btnCheckEvents.Name = "btnCheckEvents";
      this.btnCheckEvents.UseVisualStyleBackColor = true;
      this.btnCheckEvents.Click += new System.EventHandler( this.btnCheckEvents_Click );
      // 
      // lblCheckEvents
      // 
      resources.ApplyResources( this.lblCheckEvents, "lblCheckEvents" );
      this.lblCheckEvents.Name = "lblCheckEvents";
      // 
      // tabLanguage
      // 
      this.tabLanguage.Controls.Add( this.radioButton1 );
      this.tabLanguage.Controls.Add( this.rbLangSpanish );
      this.tabLanguage.Controls.Add( this.rbLangGerman );
      this.tabLanguage.Controls.Add( this.rbLangEnglishUS );
      this.tabLanguage.Controls.Add( this.rbLangEnglishUK );
      resources.ApplyResources( this.tabLanguage, "tabLanguage" );
      this.tabLanguage.Name = "tabLanguage";
      this.tabLanguage.UseVisualStyleBackColor = true;
      // 
      // radioButton1
      // 
      this.radioButton1.Image = global::BEGM.Properties.Resources.flag_country_french;
      resources.ApplyResources( this.radioButton1, "radioButton1" );
      this.radioButton1.Name = "radioButton1";
      this.radioButton1.Tag = "fr";
      this.radioButton1.UseVisualStyleBackColor = true;
      // 
      // rbLangSpanish
      // 
      this.rbLangSpanish.Image = global::BEGM.Properties.Resources.flag_language_spain;
      resources.ApplyResources( this.rbLangSpanish, "rbLangSpanish" );
      this.rbLangSpanish.Name = "rbLangSpanish";
      this.rbLangSpanish.Tag = "es";
      this.rbLangSpanish.UseVisualStyleBackColor = true;
      // 
      // rbLangGerman
      // 
      this.rbLangGerman.Image = global::BEGM.Properties.Resources.flag_language_germany;
      resources.ApplyResources( this.rbLangGerman, "rbLangGerman" );
      this.rbLangGerman.Name = "rbLangGerman";
      this.rbLangGerman.Tag = "de";
      this.rbLangGerman.UseVisualStyleBackColor = true;
      // 
      // rbLangEnglishUS
      // 
      this.rbLangEnglishUS.Image = global::BEGM.Properties.Resources.flag_language_unitedstates;
      resources.ApplyResources( this.rbLangEnglishUS, "rbLangEnglishUS" );
      this.rbLangEnglishUS.Name = "rbLangEnglishUS";
      this.rbLangEnglishUS.Tag = "en-US";
      this.rbLangEnglishUS.UseVisualStyleBackColor = true;
      // 
      // rbLangEnglishUK
      // 
      this.rbLangEnglishUK.Checked = true;
      this.rbLangEnglishUK.Image = global::BEGM.Properties.Resources.flag_country_british;
      resources.ApplyResources( this.rbLangEnglishUK, "rbLangEnglishUK" );
      this.rbLangEnglishUK.Name = "rbLangEnglishUK";
      this.rbLangEnglishUK.TabStop = true;
      this.rbLangEnglishUK.Tag = "en";
      this.rbLangEnglishUK.UseVisualStyleBackColor = true;
      // 
      // btnOK
      // 
      resources.ApplyResources( this.btnOK, "btnOK" );
      this.btnOK.Name = "btnOK";
      this.btnOK.UseVisualStyleBackColor = true;
      this.btnOK.Click += new System.EventHandler( this.btnOK_Click );
      // 
      // btnDefaults
      // 
      resources.ApplyResources( this.btnDefaults, "btnDefaults" );
      this.btnDefaults.Name = "btnDefaults";
      this.btnDefaults.UseVisualStyleBackColor = true;
      this.btnDefaults.Click += new System.EventHandler( this.btnDefaults_Click );
      // 
      // errorProvider
      // 
      this.errorProvider.BlinkRate = 100;
      this.errorProvider.ContainerControl = this;
      // 
      // btnCancel
      // 
      resources.ApplyResources( this.btnCancel, "btnCancel" );
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      this.btnCancel.Click += new System.EventHandler( this.btnCancel_Click );
      // 
      // tmrEnableNewEvents
      // 
      this.tmrEnableNewEvents.Enabled = true;
      this.tmrEnableNewEvents.Interval = 30000;
      this.tmrEnableNewEvents.Tick += new System.EventHandler( this.tmrEnableNewEvents_Tick );
      // 
      // trkSleepWhenIdle
      // 
      resources.ApplyResources( this.trkSleepWhenIdle, "trkSleepWhenIdle" );
      this.trkSleepWhenIdle.LargeChange = 1;
      this.trkSleepWhenIdle.Maximum = 5;
      this.trkSleepWhenIdle.Minimum = 1;
      this.trkSleepWhenIdle.Name = "trkSleepWhenIdle";
      this.trkSleepWhenIdle.Value = 3;
      this.trkSleepWhenIdle.ValueChanged += new System.EventHandler( this.trkSleepWhenIdle_ValueChanged );
      // 
      // trkAutoNextTime
      // 
      resources.ApplyResources( this.trkAutoNextTime, "trkAutoNextTime" );
      this.trkAutoNextTime.LargeChange = 1;
      this.trkAutoNextTime.Minimum = 1;
      this.trkAutoNextTime.Name = "trkAutoNextTime";
      this.trkAutoNextTime.Value = 5;
      this.trkAutoNextTime.ValueChanged += new System.EventHandler( this.trkAutoNextTime_ValueChanged );
      // 
      // trkPostponeIdle
      // 
      resources.ApplyResources( this.trkPostponeIdle, "trkPostponeIdle" );
      this.trkPostponeIdle.LargeChange = 2;
      this.trkPostponeIdle.Maximum = 12;
      this.trkPostponeIdle.Minimum = 2;
      this.trkPostponeIdle.Name = "trkPostponeIdle";
      this.trkPostponeIdle.TickFrequency = 2;
      this.trkPostponeIdle.Value = 4;
      this.trkPostponeIdle.ValueChanged += new System.EventHandler( this.trkPostponeIdle_ValueChanged );
      // 
      // trkWallZoom
      // 
      resources.ApplyResources( this.trkWallZoom, "trkWallZoom" );
      this.trkWallZoom.LargeChange = 1;
      this.trkWallZoom.Maximum = 6;
      this.trkWallZoom.Name = "trkWallZoom";
      // 
      // trkWallUpdateTime
      // 
      resources.ApplyResources( this.trkWallUpdateTime, "trkWallUpdateTime" );
      this.trkWallUpdateTime.LargeChange = 1;
      this.trkWallUpdateTime.Minimum = 1;
      this.trkWallUpdateTime.Name = "trkWallUpdateTime";
      this.trkWallUpdateTime.Value = 5;
      this.trkWallUpdateTime.ValueChanged += new System.EventHandler( this.trkWallUpdateTime_ValueChanged );
      // 
      // Options
      // 
      resources.ApplyResources( this, "$this" );
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.Controls.Add( this.btnDefaults );
      this.Controls.Add( this.btnCancel );
      this.Controls.Add( this.tabControl );
      this.Controls.Add( this.btnOK );
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "Options";
      this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
      this.Activated += new System.EventHandler( this.Options_Activated );
      this.FormClosing += new System.Windows.Forms.FormClosingEventHandler( this.Options_FormClosing );
      this.tabControl.ResumeLayout( false );
      this.tabNetwork.ResumeLayout( false );
      this.tabNetwork.PerformLayout();
      this.tabStartup.ResumeLayout( false );
      this.tabStartup.PerformLayout();
      this.gbSleepMode.ResumeLayout( false );
      this.tabAlerts.ResumeLayout( false );
      this.tabAlerts.PerformLayout();
      this.pnlAlertsOptions.ResumeLayout( false );
      this.pnlAlertsOptions.PerformLayout();
      this.pnlAlertsFilters.ResumeLayout( false );
      this.pnlAlertsFilters.PerformLayout();
      this.cmsChokePoint.ResumeLayout( false );
      this.tabMap.ResumeLayout( false );
      this.tabMap.PerformLayout();
      this.pnlMapOptions.ResumeLayout( false );
      this.pnlMapOptions.PerformLayout();
      this.gbFreeMem.ResumeLayout( false );
      this.pnlMapWallOptions.ResumeLayout( false );
      this.pnlMapWallOptions.PerformLayout();
      this.tabMisc.ResumeLayout( false );
      this.tabMisc.PerformLayout();
      this.pnlDockWindow.ResumeLayout( false );
      this.pnlDockWindow.PerformLayout();
      this.pnlEventSort.ResumeLayout( false );
      this.pnlEventSort.PerformLayout();
      this.tabUpdates.ResumeLayout( false );
      this.gbResetAllData.ResumeLayout( false );
      this.gbCheckEvents.ResumeLayout( false );
      this.tabLanguage.ResumeLayout( false );
      ( (System.ComponentModel.ISupportInitialize)( this.errorProvider ) ).EndInit();
      ( (System.ComponentModel.ISupportInitialize)( this.trkSleepWhenIdle ) ).EndInit();
      ( (System.ComponentModel.ISupportInitialize)( this.trkAutoNextTime ) ).EndInit();
      ( (System.ComponentModel.ISupportInitialize)( this.trkPostponeIdle ) ).EndInit();
      ( (System.ComponentModel.ISupportInitialize)( this.trkWallZoom ) ).EndInit();
      ( (System.ComponentModel.ISupportInitialize)( this.trkWallUpdateTime ) ).EndInit();
      this.ResumeLayout( false );

}

    #endregion

    private System.Windows.Forms.Label lblWiretapServer;
    private System.Windows.Forms.Button btnOK;
    private System.Windows.Forms.TextBox txtWiretapHost;
    private System.Windows.Forms.TextBox txtProxyHost;
    private System.Windows.Forms.RadioButton rbUseCustomProxy;
    private System.Windows.Forms.RadioButton rbUseIEProxy;
    private System.Windows.Forms.Button btnTestConn;
    private System.Windows.Forms.Label lblTestResult;
    private System.Windows.Forms.Button btnDefaults;
    private System.Windows.Forms.Label lblProxyPort;
    private System.Windows.Forms.TextBox txtProxyPort;
    private System.Windows.Forms.ErrorProvider errorProvider;
    public System.Windows.Forms.TabControl tabControl;
    private System.Windows.Forms.CheckBox cbSleepWhenPlay;
    private System.Windows.Forms.CheckBox cbSleepWhenIdle;
    private System.Windows.Forms.CheckBox cbStartMinimised;
    private System.Windows.Forms.CheckBox cbRunOnStartup;
    private System.Windows.Forms.CheckBox cbWakeAfterPlay;
    private System.Windows.Forms.Label lblSleepWhenIdle;
    private Dotnetrix.Controls.TrackBar trkSleepWhenIdle;
    private System.Windows.Forms.GroupBox gbSleepMode;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Label lblWiretapPort;
    private System.Windows.Forms.TextBox txtWiretapPort;
    private System.Windows.Forms.TabPage tabUpdates;
    private System.Windows.Forms.Button btnCheckEvents;
    private System.Windows.Forms.Label lblCheckEvents;
    private System.Windows.Forms.GroupBox gbCheckEvents;
    private System.Windows.Forms.GroupBox gbResetAllData;
    private System.Windows.Forms.Button btnResetAllData;
    private System.Windows.Forms.Label lblResetAllData;
    private System.Windows.Forms.Timer tmrEnableNewEvents;
    private System.Windows.Forms.CheckBox cbLoadFactoryData;
    private System.Windows.Forms.Label lblSleepMode;
    private System.Windows.Forms.Label lblForceNoProxy;
    public System.Windows.Forms.TabPage tabNetwork;
    private System.Windows.Forms.TabPage tabStartup;
    private System.Windows.Forms.CheckBox cbShowAlerts;
    private System.Windows.Forms.RadioButton rbPositionBottom;
    private System.Windows.Forms.RadioButton rbPositionTop;
    private System.Windows.Forms.CheckBox cbFilterEventType;
    private System.Windows.Forms.TreeView tvwFilterCountry;
    private System.Windows.Forms.TreeView tvwFilterChokePoint;
    private System.Windows.Forms.TreeView tvwFilterEventType;
    private System.Windows.Forms.CheckBox cbFilterCountry;
    private System.Windows.Forms.CheckBox cbFilterChokePoint;
    private System.Windows.Forms.CheckBox cbAlwaysAlertUnderAttack;
    private System.Windows.Forms.CheckBox cbAlwaysAlertChokePointCaptured;
    private System.Windows.Forms.Label lblAlwaysAlert;
    private System.Windows.Forms.ContextMenuStrip cmsChokePoint;
    private System.Windows.Forms.ToolStripMenuItem itmChokePointSelectAll;
    private System.Windows.Forms.ToolStripMenuItem itmChokePointClearAll;
    private System.Windows.Forms.TabPage tabAlerts;
    private System.Windows.Forms.Panel pnlAlertsFilters;
    private System.Windows.Forms.Panel pnlAlertsOptions;
    private System.Windows.Forms.GroupBox gbFilterBy;
    private System.Windows.Forms.Label lblAlertPosition;
    private System.Windows.Forms.CheckBox cbPostponeIdle;
    private System.Windows.Forms.CheckBox cbPostponeFullscreen;
    private System.Windows.Forms.Label lblPostponeAlerts;
    private System.Windows.Forms.CheckBox cbPlayAlertSound;
    private Dotnetrix.Controls.TrackBar trkPostponeIdle;
    private System.Windows.Forms.Label lblPostponeIdle;
    private System.Windows.Forms.Button btnTestAlert;
    private Dotnetrix.Controls.TrackBar trkAutoNextTime;
    private System.Windows.Forms.Label lblAutoNext;
    private System.Windows.Forms.Label lblAutoNextTime;
    private System.Windows.Forms.CheckBox cbAlertFilters;
    private System.Windows.Forms.TabPage tabMisc;
    private System.Windows.Forms.RadioButton rbEventSortBottom;
    private System.Windows.Forms.RadioButton rbEventSortTop;
    private System.Windows.Forms.Label lblEventSort;
    private System.Windows.Forms.CheckBox cbCheckVersion;
    private System.Windows.Forms.ComboBox cmbGameStatusDisplay;
    private System.Windows.Forms.ComboBox cmbAlertDisplay;
    private System.Windows.Forms.Label lblAlertDisplay;
    private System.Windows.Forms.RadioButton rbDockWindowRight;
    private System.Windows.Forms.RadioButton rbDockWindowLeft;
    private System.Windows.Forms.Panel pnlDockWindow;
    private System.Windows.Forms.Panel pnlEventSort;
    private System.Windows.Forms.TabPage tabMap;
    private System.Windows.Forms.RadioButton rbMapSize100;
    private System.Windows.Forms.RadioButton rbMapSize80;
    private System.Windows.Forms.RadioButton rbMapSize60;
    private System.Windows.Forms.Label lblMapSize;
    private System.Windows.Forms.RadioButton rbMapSize40;
    private System.Windows.Forms.Label lblMemUsage100;
    private System.Windows.Forms.Label lblMemUsage80;
    private System.Windows.Forms.Label lblMemUsage60;
    private System.Windows.Forms.Label lblMemUsage40;
    private System.Windows.Forms.Label lblMemUsage;
    private System.Windows.Forms.CheckBox cbAlwaysUseDefaultMapSize;
    private System.Windows.Forms.Label lblMapInfo;
    private System.Windows.Forms.GroupBox gbFreeMem;
    private System.Windows.Forms.Label lblFreeMem;
    private System.Windows.Forms.LinkLabel lnkMapPluginInfo;
    private System.Windows.Forms.CheckBox cbShowWallpaper;
    private System.Windows.Forms.CheckBox cbWallOptions;
    private System.Windows.Forms.Panel pnlMapOptions;
    private System.Windows.Forms.Panel pnlMapWallOptions;
    private System.Windows.Forms.CheckBox cbWallRemove;
    private System.Windows.Forms.Label lblWallUpdateTime;
    private Dotnetrix.Controls.TrackBar trkWallUpdateTime;
    private System.Windows.Forms.Label lblWallUpdate;
    private System.Windows.Forms.Label lblWallZoomMin;
    private System.Windows.Forms.Label lblWallZoomMax;
    private Dotnetrix.Controls.TrackBar trkWallZoom;
    private System.Windows.Forms.Label lblWallZoom;
    private System.Windows.Forms.Label lblWallOptions;
    private System.Windows.Forms.Button btnApplyWallpaper;
    private System.Windows.Forms.TreeView tvwMapOptions;
    private System.Windows.Forms.Label lblProxyServerHttp;
    private System.Windows.Forms.Label lblWiretapServerHttp;
    private System.Windows.Forms.TabPage tabLanguage;
    private System.Windows.Forms.RadioButton rbLangEnglishUK;
    private System.Windows.Forms.RadioButton rbLangEnglishUS;
    private System.Windows.Forms.RadioButton rbLangGerman;
    private System.Windows.Forms.RadioButton rbLangSpanish;
    private System.Windows.Forms.RadioButton radioButton1;
    private System.Windows.Forms.CheckBox cbDockWindow;
  }
}