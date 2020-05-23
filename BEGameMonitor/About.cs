/* =============================================================================
 * BEGameMonitor                                    Copyright (c) 2013 Xiperware
 * http://begm.sourceforge.net/                              xiperware@gmail.com
 * 
 * This file is part of Battleground Europe Game Monitor.
 * 
 * Battleground Europe Game Monitor is free software; you can redistribute it
 * and/or modify it under the terms of the GNU General Public License v2 as
 * published by the Free Software Foundation.
 * 
 * This program is distributed in the hope that it will be useful, but WITHOUT
 * ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 * FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License along with
 * this program. If not, see <http://www.gnu.org/licenses/>.
 * =============================================================================
 */

using System;
using System.Diagnostics;  // Process
using System.Drawing;
using System.Windows.Forms;
using BEGM.Properties;
using Xiperware.WiretapAPI;

namespace BEGM
{
  /// <summary>
  /// An about dialog showing basic program info (inc network usage) and log window listing
  /// each log entry with its timestamp.
  /// </summary>
  public partial class About : Form
  {
    #region Variables

    /// <summary>
    /// Total usage from previous sessions, loaded at startup.
    /// </summary>
    private int prevTotalDownload;

#if !MAC
    private WebBrowser webCredits;
#endif

    #endregion

    #region Constructors

    /// <summary>
    /// Create a new About dialog.
    /// </summary>
    public About()
    {
      InitializeComponent();
      InitializeComponentNonMac();


      // set verison

      lblVersion.Text = Program.versionString;


      // append lang code to begm url

      string langCode = Program.uiCulture.TwoLetterISOLanguageName;

      switch( langCode )
      {
        case "de":
        case "fr":
        case "es":
          lnkHomepage.Text += langCode;
          break;
      }

    }

    /// <summary>
    /// Required method for Designer support - non mac only controls.
    /// </summary>
    private void InitializeComponentNonMac()
    {
#if !MAC
      this.webCredits = new WebBrowser();

      this.gbCredits.Controls.Add( this.webCredits );

      this.webCredits.AllowNavigation = false;
      this.webCredits.AllowWebBrowserDrop = false;
      this.webCredits.Dock = DockStyle.Bottom;
      this.webCredits.Height = 69;
      this.webCredits.IsWebBrowserContextMenuEnabled = false;
      this.webCredits.Name = "webCredits";
      this.webCredits.ScriptErrorsSuppressed = true;
      this.webCredits.ScrollBarsEnabled = false;
      this.webCredits.TabStop = false;
      this.webCredits.WebBrowserShortcutsEnabled = false;
      this.webCredits.DocumentCompleted += this.webCredits_DocumentCompleted;
#endif
    }

    #endregion

    #region Properties

    /// <summary>
    /// Total KB downloaded by this program, not including this session.
    /// </summary>
    public int PrevTotalDownload
    {
      get { return this.prevTotalDownload; }
      set { this.prevTotalDownload = value; }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Update the label showing download stats.
    /// </summary>
    /// <param name="kbytes">KB downloaded this session.</param>
    public void UpdateKBytes( int kbytes )
    {
      if( lblKBytesValues.InvokeRequired )  // we are in a different thread
        lblKBytesValues.Invoke( new UpdateKBytesDelegate( UpdateKBytes ), new object[] { kbytes } );  // call self from correct thread
      else
        lblKBytesValues.Text = String.Format( "{0:N0} KB\r\n{1:N0} KB", kbytes, kbytes + prevTotalDownload );
    }

    /// <summary>
    /// Private delegate for UpdateKBytes() above.
    /// </summary>
    private delegate void UpdateKBytesDelegate( int kbytes );

    /// <summary>
    /// Scrolls the log window to the bottom.
    /// </summary>
    private void ScrollLogToBottom()
    {
#if !MAC
      User32.SendMessage( txtLog.Handle, User32.WM_VSCROLL, (IntPtr)User32.SB_BOTTOM, IntPtr.Zero );
#endif
    }

    /// <summary>
    /// Add an entry to the log window.
    /// </summary>
    /// <remarks>If the text is "OK", or "ERROR", no preceding newline is added.</remarks>
    /// <param name="entry">The text of the log entry.</param>
    /// <param name="error">True if the text should appear highlighted red.</param>
    public void AddLogEntry( string entry, bool error )
    {
      // remember start index

      int start = txtLog.Text.Length + 1;


      // add log entry text

      if( entry == "OK" || entry == "ERROR" )  // no timestamp or preceding newline
      {
        txtLog.AppendText( " " + entry );
      }
      else  // normal log entry
      {
        entry = String.Format( "{0} {1}", Xiperware.WiretapAPI.Misc.Timestamp( DateTime.Now ), entry );
        
        if( txtLog.Text.Length == 0 )
          txtLog.AppendText( entry );
        else
          txtLog.AppendText( "\r\n" + entry );
      }


      // set colour

      txtLog.Select( start, entry.Length );
      txtLog.SelectionColor = error ? Color.Red : SystemColors.WindowText;


      // truncate to 100 lines

      if( txtLog.Lines.Length > 100 )
      {
        int endIndex = txtLog.GetFirstCharIndexFromLine( txtLog.Lines.Length - 100 );
        txtLog.Select( 0, endIndex );
        txtLog.ReadOnly = false;
        txtLog.SelectedText = String.Empty;
        txtLog.ReadOnly = true;
      }


      // select none & scroll to bottom

      txtLog.Select( txtLog.Text.Length, 0 );
      ScrollLogToBottom();
    }

    #endregion

    #region Event Handlers

    // FORM

    // change window title on tab change, auto focus log control
    private void tabControl_SelectedIndexChanged( object sender, EventArgs e )
    {
      this.Text = tabControl.SelectedTab.Text;

      if( tabControl.SelectedTab == tabLog )
        txtLog.Focus();
    }

    // about window shown/hidden
    private void About_VisibleChanged( object sender, EventArgs e )
    {
#if !MAC
      if( this.Visible )  // opening
      {
        ScrollLogToBottom();  // scroll log to bottom
        webCredits.AllowNavigation = true;
        webCredits.DocumentText = Resources.credits;  // start scrolling credits
      }
      else  // closing
      {
        webCredits.AllowNavigation = true;
        webCredits.DocumentText = null;  // stop scrolling credits
      }
#endif
    }

    // hide instead of destroying when user clicks [X] or alt-f4's
    private void About_FormClosing( object sender, FormClosingEventArgs e )
    {
      if( e.CloseReason == CloseReason.UserClosing )
      {
        this.Visible = false;
        e.Cancel = true;
      }
    }
    
    // hide on OK
    private void btnOK_Click( object sender, EventArgs e )
    {
      this.Visible = false;
    }


    // ABOUT

    // links
    private void lnkHomepage_LinkClicked( object sender, LinkLabelLinkClickedEventArgs e )
    {
      try
      {
        Process.Start( lnkHomepage.Text );
      }
      catch( Exception ex )
      {
        MessageBox.Show( ex.Message, Language.Error_Error, MessageBoxButtons.OK, MessageBoxIcon.Error );
      }
    }
    private void lnkEmail_LinkClicked( object sender, LinkLabelLinkClickedEventArgs e )
    {
      try
      {
        Process.Start( "mailto:" + lnkEmail.Text );
      }
      catch( Exception ex )
      {
        MessageBox.Show( ex.Message, Language.Error_Error, MessageBoxButtons.OK, MessageBoxIcon.Error );
      }
    }
    private void lnkBEHomepage_LinkClicked( object sender, LinkLabelLinkClickedEventArgs e )
    {
      try
      {
        Process.Start( lnkBEHomepage.Text );
      }
      catch( Exception ex )
      {
        MessageBox.Show( ex.Message, Language.Error_Error, MessageBoxButtons.OK, MessageBoxIcon.Error );
      }
    }

#if !MAC
    // prevent navigation after setting DocumentText has completed
    private void webCredits_DocumentCompleted( object sender, WebBrowserDocumentCompletedEventArgs e )
    {
      webCredits.AllowNavigation = false;
      if( webCredits.Document != null )
        webCredits.Document.BackColor = tabAbout.BackColor;
    }
#endif

    #endregion
  }
}
