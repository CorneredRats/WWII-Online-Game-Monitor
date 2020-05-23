namespace BEGM
{
  partial class About
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager( typeof( About ) );
      this.picLogo = new System.Windows.Forms.PictureBox();
      this.tabControl = new System.Windows.Forms.TabControl();
      this.tabAbout = new System.Windows.Forms.TabPage();
      this.lblTitle = new System.Windows.Forms.Label();
      this.lblKBytesValues = new System.Windows.Forms.Label();
      this.lblKBytesHead = new System.Windows.Forms.Label();
      this.lnkBEHomepage = new System.Windows.Forms.LinkLabel();
      this.lblDisclaimer = new System.Windows.Forms.Label();
      this.lnkEmail = new System.Windows.Forms.LinkLabel();
      this.lnkHomepage = new System.Windows.Forms.LinkLabel();
      this.lblCopyright = new System.Windows.Forms.Label();
      this.gbCredits = new System.Windows.Forms.GroupBox();
      this.lblCredits = new System.Windows.Forms.Label();
      this.tabLog = new System.Windows.Forms.TabPage();
      this.txtLog = new System.Windows.Forms.RichTextBox();
      this.btnOK = new System.Windows.Forms.Button();
      this.lblVersion = new System.Windows.Forms.Label();
      ( (System.ComponentModel.ISupportInitialize)( this.picLogo ) ).BeginInit();
      this.tabControl.SuspendLayout();
      this.tabAbout.SuspendLayout();
      this.gbCredits.SuspendLayout();
      this.tabLog.SuspendLayout();
      this.SuspendLayout();
      // 
      // picLogo
      // 
      resources.ApplyResources( this.picLogo, "picLogo" );
      this.picLogo.Image = global::BEGM.Properties.Resources.xiperware_logo;
      this.picLogo.Name = "picLogo";
      this.picLogo.TabStop = false;
      // 
      // tabControl
      // 
      resources.ApplyResources( this.tabControl, "tabControl" );
      this.tabControl.Controls.Add( this.tabAbout );
      this.tabControl.Controls.Add( this.tabLog );
      this.tabControl.Name = "tabControl";
      this.tabControl.SelectedIndex = 0;
      this.tabControl.SelectedIndexChanged += new System.EventHandler( this.tabControl_SelectedIndexChanged );
      // 
      // tabAbout
      // 
      this.tabAbout.BackColor = System.Drawing.SystemColors.Window;
      this.tabAbout.Controls.Add( this.lblVersion );
      this.tabAbout.Controls.Add( this.gbCredits );
      this.tabAbout.Controls.Add( this.lblTitle );
      this.tabAbout.Controls.Add( this.lblKBytesValues );
      this.tabAbout.Controls.Add( this.lblKBytesHead );
      this.tabAbout.Controls.Add( this.lnkBEHomepage );
      this.tabAbout.Controls.Add( this.lblDisclaimer );
      this.tabAbout.Controls.Add( this.lnkEmail );
      this.tabAbout.Controls.Add( this.lnkHomepage );
      this.tabAbout.Controls.Add( this.picLogo );
      this.tabAbout.Controls.Add( this.lblCopyright );
      resources.ApplyResources( this.tabAbout, "tabAbout" );
      this.tabAbout.Name = "tabAbout";
      // 
      // lblTitle
      // 
      resources.ApplyResources( this.lblTitle, "lblTitle" );
      this.lblTitle.Name = "lblTitle";
      // 
      // lblKBytesValues
      // 
      resources.ApplyResources( this.lblKBytesValues, "lblKBytesValues" );
      this.lblKBytesValues.Name = "lblKBytesValues";
      // 
      // lblKBytesHead
      // 
      resources.ApplyResources( this.lblKBytesHead, "lblKBytesHead" );
      this.lblKBytesHead.Name = "lblKBytesHead";
      // 
      // lnkBEHomepage
      // 
      resources.ApplyResources( this.lnkBEHomepage, "lnkBEHomepage" );
      this.lnkBEHomepage.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
      this.lnkBEHomepage.LinkColor = System.Drawing.Color.Blue;
      this.lnkBEHomepage.Name = "lnkBEHomepage";
      this.lnkBEHomepage.TabStop = true;
      this.lnkBEHomepage.VisitedLinkColor = System.Drawing.Color.Blue;
      this.lnkBEHomepage.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler( this.lnkBEHomepage_LinkClicked );
      // 
      // lblDisclaimer
      // 
      resources.ApplyResources( this.lblDisclaimer, "lblDisclaimer" );
      this.lblDisclaimer.Name = "lblDisclaimer";
      // 
      // lnkEmail
      // 
      resources.ApplyResources( this.lnkEmail, "lnkEmail" );
      this.lnkEmail.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
      this.lnkEmail.LinkColor = System.Drawing.Color.Blue;
      this.lnkEmail.Name = "lnkEmail";
      this.lnkEmail.TabStop = true;
      this.lnkEmail.VisitedLinkColor = System.Drawing.Color.Blue;
      this.lnkEmail.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler( this.lnkEmail_LinkClicked );
      // 
      // lnkHomepage
      // 
      resources.ApplyResources( this.lnkHomepage, "lnkHomepage" );
      this.lnkHomepage.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
      this.lnkHomepage.LinkColor = System.Drawing.Color.Blue;
      this.lnkHomepage.Name = "lnkHomepage";
      this.lnkHomepage.TabStop = true;
      this.lnkHomepage.VisitedLinkColor = System.Drawing.Color.Blue;
      this.lnkHomepage.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler( this.lnkHomepage_LinkClicked );
      // 
      // lblCopyright
      // 
      resources.ApplyResources( this.lblCopyright, "lblCopyright" );
      this.lblCopyright.Name = "lblCopyright";
      // 
      // gbCredits
      // 
      resources.ApplyResources( this.gbCredits, "gbCredits" );
      this.gbCredits.Controls.Add( this.lblCredits );
      this.gbCredits.ForeColor = System.Drawing.Color.Transparent;
      this.gbCredits.Name = "gbCredits";
      this.gbCredits.TabStop = false;
      // 
      // lblCredits
      // 
      resources.ApplyResources( this.lblCredits, "lblCredits" );
      this.lblCredits.ForeColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 0 ) ) ) ), ( (int)( ( (byte)( 70 ) ) ) ), ( (int)( ( (byte)( 213 ) ) ) ) );
      this.lblCredits.Name = "lblCredits";
      // 
      // tabLog
      // 
      this.tabLog.BackColor = System.Drawing.SystemColors.Window;
      this.tabLog.Controls.Add( this.txtLog );
      resources.ApplyResources( this.tabLog, "tabLog" );
      this.tabLog.Name = "tabLog";
      // 
      // txtLog
      // 
      this.txtLog.BackColor = System.Drawing.SystemColors.Window;
      this.txtLog.BorderStyle = System.Windows.Forms.BorderStyle.None;
      resources.ApplyResources( this.txtLog, "txtLog" );
      this.txtLog.Name = "txtLog";
      this.txtLog.ReadOnly = true;
      // 
      // btnOK
      // 
      resources.ApplyResources( this.btnOK, "btnOK" );
      this.btnOK.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnOK.Name = "btnOK";
      this.btnOK.UseVisualStyleBackColor = true;
      this.btnOK.Click += new System.EventHandler( this.btnOK_Click );
      // 
      // lblVersion
      // 
      resources.ApplyResources( this.lblVersion, "lblVersion" );
      this.lblVersion.Name = "lblVersion";
      // 
      // About
      // 
      this.AcceptButton = this.btnOK;
      resources.ApplyResources( this, "$this" );
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnOK;
      this.Controls.Add( this.tabControl );
      this.Controls.Add( this.btnOK );
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "About";
      this.VisibleChanged += new System.EventHandler( this.About_VisibleChanged );
      this.FormClosing += new System.Windows.Forms.FormClosingEventHandler( this.About_FormClosing );
      ( (System.ComponentModel.ISupportInitialize)( this.picLogo ) ).EndInit();
      this.tabControl.ResumeLayout( false );
      this.tabAbout.ResumeLayout( false );
      this.tabAbout.PerformLayout();
      this.gbCredits.ResumeLayout( false );
      this.gbCredits.PerformLayout();
      this.tabLog.ResumeLayout( false );
      this.ResumeLayout( false );

    }

    #endregion

    private System.Windows.Forms.PictureBox picLogo;
    private System.Windows.Forms.TabPage tabAbout;
    private System.Windows.Forms.TabPage tabLog;
    private System.Windows.Forms.LinkLabel lnkEmail;
    private System.Windows.Forms.LinkLabel lnkHomepage;
    private System.Windows.Forms.Label lblCopyright;
    private System.Windows.Forms.Button btnOK;
    private System.Windows.Forms.Label lblDisclaimer;
    private System.Windows.Forms.LinkLabel lnkBEHomepage;
    private System.Windows.Forms.Label lblKBytesValues;
    private System.Windows.Forms.Label lblKBytesHead;
    private System.Windows.Forms.Label lblTitle;
    private System.Windows.Forms.RichTextBox txtLog;
    public System.Windows.Forms.TabControl tabControl;
    private System.Windows.Forms.GroupBox gbCredits;
    private System.Windows.Forms.Label lblCredits;
    private System.Windows.Forms.Label lblVersion;
  }
}