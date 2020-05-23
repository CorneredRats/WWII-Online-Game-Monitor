namespace BEGM
{
  partial class AlertWindow
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager( typeof( AlertWindow ) );
      this.lblTitle = new System.Windows.Forms.Label();
      this.lblTimestamp = new System.Windows.Forms.Label();
      this.picClose = new System.Windows.Forms.PictureBox();
      this.lblMinsAgo = new System.Windows.Forms.Label();
      this.tmrFadein = new System.Windows.Forms.Timer( this.components );
      this.lblCountCurrent = new System.Windows.Forms.Label();
      this.lblCountDivider = new System.Windows.Forms.Label();
      this.lblCountTotal = new System.Windows.Forms.Label();
      this.picPrev = new System.Windows.Forms.PictureBox();
      this.picNext = new System.Windows.Forms.PictureBox();
      this.tmrFadeout = new System.Windows.Forms.Timer( this.components );
      this.picLine = new System.Windows.Forms.PictureBox();
      this.picCornerBottom = new System.Windows.Forms.PictureBox();
      this.picCornerTop = new System.Windows.Forms.PictureBox();
      this.lblDescription = new System.Windows.Forms.Label();
      this.tmrAutoNext = new System.Windows.Forms.Timer( this.components );
      this.picIcon = new System.Windows.Forms.PictureBox();
      this.cbShowAlerts = new System.Windows.Forms.CheckBox();
      this.picFlags = new System.Windows.Forms.PictureBox();
      this.tmrDetectLeave = new System.Windows.Forms.Timer( this.components );
      ( (System.ComponentModel.ISupportInitialize)( this.picClose ) ).BeginInit();
      ( (System.ComponentModel.ISupportInitialize)( this.picPrev ) ).BeginInit();
      ( (System.ComponentModel.ISupportInitialize)( this.picNext ) ).BeginInit();
      ( (System.ComponentModel.ISupportInitialize)( this.picLine ) ).BeginInit();
      ( (System.ComponentModel.ISupportInitialize)( this.picCornerBottom ) ).BeginInit();
      ( (System.ComponentModel.ISupportInitialize)( this.picCornerTop ) ).BeginInit();
      ( (System.ComponentModel.ISupportInitialize)( this.picIcon ) ).BeginInit();
      ( (System.ComponentModel.ISupportInitialize)( this.picFlags ) ).BeginInit();
      this.SuspendLayout();
      // 
      // lblTitle
      // 
      resources.ApplyResources( this.lblTitle, "lblTitle" );
      this.lblTitle.Name = "lblTitle";
      this.lblTitle.MouseClick += new System.Windows.Forms.MouseEventHandler( this.lblTitle_MouseClick );
      this.lblTitle.MouseEnter += new System.EventHandler( this.lblTitle_MouseEnter );
      // 
      // lblTimestamp
      // 
      resources.ApplyResources( this.lblTimestamp, "lblTimestamp" );
      this.lblTimestamp.Name = "lblTimestamp";
      this.lblTimestamp.MouseClick += new System.Windows.Forms.MouseEventHandler( this.lblTimestamp_MouseClick );
      this.lblTimestamp.MouseEnter += new System.EventHandler( this.lblTimestamp_MouseEnter );
      // 
      // picClose
      // 
      resources.ApplyResources( this.picClose, "picClose" );
      this.picClose.Cursor = System.Windows.Forms.Cursors.Hand;
      this.picClose.Image = global::BEGM.Properties.Resources.alertclose_disabled;
      this.picClose.Name = "picClose";
      this.picClose.TabStop = false;
      this.picClose.MouseLeave += new System.EventHandler( this.picClose_MouseLeave );
      this.picClose.MouseClick += new System.Windows.Forms.MouseEventHandler( this.picClose_MouseClick );
      this.picClose.MouseDown += new System.Windows.Forms.MouseEventHandler( this.picClose_MouseDown );
      this.picClose.MouseUp += new System.Windows.Forms.MouseEventHandler( this.picClose_MouseUp );
      this.picClose.MouseEnter += new System.EventHandler( this.picClose_MouseEnter );
      // 
      // lblMinsAgo
      // 
      resources.ApplyResources( this.lblMinsAgo, "lblMinsAgo" );
      this.lblMinsAgo.BackColor = System.Drawing.Color.Transparent;
      this.lblMinsAgo.Name = "lblMinsAgo";
      this.lblMinsAgo.MouseClick += new System.Windows.Forms.MouseEventHandler( this.lblMinsAgo_MouseClick );
      this.lblMinsAgo.MouseEnter += new System.EventHandler( this.lblMinsAgo_MouseEnter );
      // 
      // tmrFadein
      // 
      this.tmrFadein.Interval = 30;
      this.tmrFadein.Tick += new System.EventHandler( this.tmrFadein_Tick );
      // 
      // lblCountCurrent
      // 
      resources.ApplyResources( this.lblCountCurrent, "lblCountCurrent" );
      this.lblCountCurrent.Name = "lblCountCurrent";
      this.lblCountCurrent.MouseClick += new System.Windows.Forms.MouseEventHandler( this.lblCountCurrent_MouseClick );
      this.lblCountCurrent.MouseEnter += new System.EventHandler( this.lblCountCurrent_MouseEnter );
      // 
      // lblCountDivider
      // 
      resources.ApplyResources( this.lblCountDivider, "lblCountDivider" );
      this.lblCountDivider.Name = "lblCountDivider";
      this.lblCountDivider.MouseClick += new System.Windows.Forms.MouseEventHandler( this.lblCountDivider_MouseClick );
      this.lblCountDivider.MouseEnter += new System.EventHandler( this.lblCountDivider_MouseEnter );
      // 
      // lblCountTotal
      // 
      resources.ApplyResources( this.lblCountTotal, "lblCountTotal" );
      this.lblCountTotal.Name = "lblCountTotal";
      this.lblCountTotal.MouseClick += new System.Windows.Forms.MouseEventHandler( this.lblCountTotal_MouseClick );
      this.lblCountTotal.MouseEnter += new System.EventHandler( this.lblCountTotal_MouseEnter );
      // 
      // picPrev
      // 
      resources.ApplyResources( this.picPrev, "picPrev" );
      this.picPrev.Cursor = System.Windows.Forms.Cursors.Hand;
      this.picPrev.Image = global::BEGM.Properties.Resources.alertarrow_left_disabled;
      this.picPrev.Name = "picPrev";
      this.picPrev.TabStop = false;
      this.picPrev.MouseLeave += new System.EventHandler( this.picPrev_MouseLeave );
      this.picPrev.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler( this.picPrev_MouseDoubleClick );
      this.picPrev.MouseClick += new System.Windows.Forms.MouseEventHandler( this.picPrev_MouseClick );
      this.picPrev.MouseDown += new System.Windows.Forms.MouseEventHandler( this.picPrev_MouseDown );
      this.picPrev.MouseUp += new System.Windows.Forms.MouseEventHandler( this.picPrev_MouseUp );
      this.picPrev.MouseEnter += new System.EventHandler( this.picPrev_MouseEnter );
      // 
      // picNext
      // 
      resources.ApplyResources( this.picNext, "picNext" );
      this.picNext.Cursor = System.Windows.Forms.Cursors.Hand;
      this.picNext.Image = global::BEGM.Properties.Resources.alertarrow_right_normal;
      this.picNext.Name = "picNext";
      this.picNext.TabStop = false;
      this.picNext.MouseLeave += new System.EventHandler( this.picNext_MouseLeave );
      this.picNext.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler( this.picNext_MouseDoubleClick );
      this.picNext.MouseClick += new System.Windows.Forms.MouseEventHandler( this.picNext_MouseClick );
      this.picNext.MouseDown += new System.Windows.Forms.MouseEventHandler( this.picNext_MouseDown );
      this.picNext.MouseUp += new System.Windows.Forms.MouseEventHandler( this.picNext_MouseUp );
      this.picNext.MouseEnter += new System.EventHandler( this.picNext_MouseEnter );
      // 
      // tmrFadeout
      // 
      this.tmrFadeout.Interval = 30;
      this.tmrFadeout.Tick += new System.EventHandler( this.tmrFadeout_Tick );
      // 
      // picLine
      // 
      resources.ApplyResources( this.picLine, "picLine" );
      this.picLine.Image = global::BEGM.Properties.Resources.alert_line;
      this.picLine.Name = "picLine";
      this.picLine.TabStop = false;
      this.picLine.MouseClick += new System.Windows.Forms.MouseEventHandler( this.picLine_MouseClick );
      this.picLine.MouseEnter += new System.EventHandler( this.picLine_MouseEnter );
      // 
      // picCornerBottom
      // 
      resources.ApplyResources( this.picCornerBottom, "picCornerBottom" );
      this.picCornerBottom.BackColor = System.Drawing.Color.Fuchsia;
      this.picCornerBottom.Image = global::BEGM.Properties.Resources.alert_corner_bottom;
      this.picCornerBottom.Name = "picCornerBottom";
      this.picCornerBottom.TabStop = false;
      this.picCornerBottom.MouseClick += new System.Windows.Forms.MouseEventHandler( this.picCornerBottom_MouseClick );
      this.picCornerBottom.MouseEnter += new System.EventHandler( this.picCornerBottom_MouseEnter );
      // 
      // picCornerTop
      // 
      this.picCornerTop.BackColor = System.Drawing.Color.Fuchsia;
      this.picCornerTop.Image = global::BEGM.Properties.Resources.alert_corner_top;
      resources.ApplyResources( this.picCornerTop, "picCornerTop" );
      this.picCornerTop.Name = "picCornerTop";
      this.picCornerTop.TabStop = false;
      this.picCornerTop.MouseClick += new System.Windows.Forms.MouseEventHandler( this.picCornerTop_MouseClick );
      this.picCornerTop.MouseEnter += new System.EventHandler( this.picCornerTop_MouseEnter );
      // 
      // lblDescription
      // 
      resources.ApplyResources( this.lblDescription, "lblDescription" );
      this.lblDescription.Name = "lblDescription";
      this.lblDescription.MouseClick += new System.Windows.Forms.MouseEventHandler( this.lblDescription_MouseClick );
      this.lblDescription.MouseEnter += new System.EventHandler( this.lblDescription_MouseEnter );
      // 
      // tmrAutoNext
      // 
      this.tmrAutoNext.Interval = 5000;
      this.tmrAutoNext.Tick += new System.EventHandler( this.tmrAutoNext_Tick );
      // 
      // picIcon
      // 
      this.picIcon.Cursor = System.Windows.Forms.Cursors.Hand;
      this.picIcon.Image = global::BEGM.Properties.Resources.facility_armybase;
      resources.ApplyResources( this.picIcon, "picIcon" );
      this.picIcon.Name = "picIcon";
      this.picIcon.TabStop = false;
      this.picIcon.MouseLeave += new System.EventHandler( this.picIcon_MouseLeave );
      this.picIcon.MouseClick += new System.Windows.Forms.MouseEventHandler( this.picIcon_MouseClick );
      this.picIcon.MouseEnter += new System.EventHandler( this.picIcon_MouseEnter );
      // 
      // cbShowAlerts
      // 
      resources.ApplyResources( this.cbShowAlerts, "cbShowAlerts" );
      this.cbShowAlerts.Name = "cbShowAlerts";
      this.cbShowAlerts.UseVisualStyleBackColor = true;
      this.cbShowAlerts.MouseEnter += new System.EventHandler( this.cbShowAlerts_MouseEnter );
      this.cbShowAlerts.CheckedChanged += new System.EventHandler( this.cbShowAlerts_CheckedChanged );
      // 
      // picFlags
      // 
      resources.ApplyResources( this.picFlags, "picFlags" );
      this.picFlags.Name = "picFlags";
      this.picFlags.TabStop = false;
      this.picFlags.MouseClick += new System.Windows.Forms.MouseEventHandler( this.picFlags_MouseClick );
      this.picFlags.MouseEnter += new System.EventHandler( this.picFlags_MouseEnter );
      // 
      // tmrDetectLeave
      // 
      this.tmrDetectLeave.Interval = 200;
      this.tmrDetectLeave.Tick += new System.EventHandler( this.tmrDetectLeave_Tick );
      // 
      // AlertWindow
      // 
      resources.ApplyResources( this, "$this" );
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 20 ) ) ) ), ( (int)( ( (byte)( 20 ) ) ) ), ( (int)( ( (byte)( 20 ) ) ) ) );
      this.Controls.Add( this.picIcon );
      this.Controls.Add( this.cbShowAlerts );
      this.Controls.Add( this.picFlags );
      this.Controls.Add( this.lblMinsAgo );
      this.Controls.Add( this.lblDescription );
      this.Controls.Add( this.picCornerTop );
      this.Controls.Add( this.picCornerBottom );
      this.Controls.Add( this.picLine );
      this.Controls.Add( this.picClose );
      this.Controls.Add( this.picNext );
      this.Controls.Add( this.picPrev );
      this.Controls.Add( this.lblCountTotal );
      this.Controls.Add( this.lblCountDivider );
      this.Controls.Add( this.lblCountCurrent );
      this.Controls.Add( this.lblTitle );
      this.Controls.Add( this.lblTimestamp );
      this.ForeColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ) );
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
      this.Name = "AlertWindow";
      this.ShowInTaskbar = false;
      this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
      this.TopMost = true;
      this.TransparencyKey = System.Drawing.Color.Fuchsia;
      this.MouseClick += new System.Windows.Forms.MouseEventHandler( this.AlertWindow_MouseClick );
      this.MouseEnter += new System.EventHandler( this.AlertWindow_MouseEnter );
      ( (System.ComponentModel.ISupportInitialize)( this.picClose ) ).EndInit();
      ( (System.ComponentModel.ISupportInitialize)( this.picPrev ) ).EndInit();
      ( (System.ComponentModel.ISupportInitialize)( this.picNext ) ).EndInit();
      ( (System.ComponentModel.ISupportInitialize)( this.picLine ) ).EndInit();
      ( (System.ComponentModel.ISupportInitialize)( this.picCornerBottom ) ).EndInit();
      ( (System.ComponentModel.ISupportInitialize)( this.picCornerTop ) ).EndInit();
      ( (System.ComponentModel.ISupportInitialize)( this.picIcon ) ).EndInit();
      ( (System.ComponentModel.ISupportInitialize)( this.picFlags ) ).EndInit();
      this.ResumeLayout( false );
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label lblTitle;
    private System.Windows.Forms.Label lblTimestamp;
    private System.Windows.Forms.PictureBox picClose;
    private System.Windows.Forms.Label lblMinsAgo;
    private System.Windows.Forms.Timer tmrFadein;
    private System.Windows.Forms.Label lblCountCurrent;
    private System.Windows.Forms.Label lblCountDivider;
    private System.Windows.Forms.Label lblCountTotal;
    private System.Windows.Forms.PictureBox picPrev;
    private System.Windows.Forms.PictureBox picNext;
    private System.Windows.Forms.Timer tmrFadeout;
    private System.Windows.Forms.PictureBox picLine;
    private System.Windows.Forms.PictureBox picCornerBottom;
    private System.Windows.Forms.PictureBox picCornerTop;
    private System.Windows.Forms.Label lblDescription;
    private System.Windows.Forms.Timer tmrAutoNext;
    private System.Windows.Forms.PictureBox picIcon;
    private System.Windows.Forms.CheckBox cbShowAlerts;
    private System.Windows.Forms.PictureBox picFlags;
    private System.Windows.Forms.Timer tmrDetectLeave;
  }
}