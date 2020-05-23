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
using System.ComponentModel;
using System.Diagnostics;  // Process
using System.Reflection;  // BindingFlags
using System.Windows.Forms;
using BEGM.Properties;
using Xiperware.WiretapAPI;

namespace BEGM
{
  /// <summary>
  /// A stripped down applet that consists of a tray icon, context menu, and timer that
  /// live in a System.ComponentModel.Container. If autowake enabled, simply waits for
  /// ww2online to not be running for WAIT_TIME consecutive minutes before launching
  /// the main program.
  /// </summary>
  public class SleepTrayIcon
  {
    #region Variables

    private const int WAIT_TIME = 3;  // number of minutes to wait before autowakeup

    private readonly IContainer container;
    private readonly NotifyIcon trayIcon;
    private readonly Timer tmrAutoWake;
    private bool autoWakeup;
    private int autoWakeupCount = 0;

    #endregion

    #region Constructors

    /// <summary>
    /// Create a new LauncherTrayIcon applet.
    /// </summary>
    /// <param name="autoWakeup">True if autowakeup should be turned on by default.</param>
    public SleepTrayIcon( bool autoWakeup )
    {
      this.autoWakeup = autoWakeup;
      
      this.container = new Container();
      this.trayIcon = new NotifyIcon( container );
      this.tmrAutoWake = new Timer( container );

      trayIcon.Icon = Resources.trayicon_sleep;
      trayIcon.Text = String.Format( Language.Sleep_TrayIcon_Tooltip, "Battleground Europe Game Monitor" );
      trayIcon.Visible = true;
      trayIcon.MouseClick += trayIcon_MouseClick;

      tmrAutoWake.Interval = 60000;  // 1 min
      tmrAutoWake.Tick += tmrAutoWake_Tick;

      ToolStripMenuItem miTrayWakeUp = new ToolStripMenuItem( Language.Sleep_TrayIcon_WakeUpNow, null, miTrayWakeUp_Click );
      ToolStripMenuItem miTrayAutoWake = new ToolStripMenuItem( autoWakeup ? Language.Sleep_TrayIcon_DisableAutoWakeup : Language.Sleep_TrayIcon_EnableAutoWakeup, null, miTrayAutoWake_Click );
      ToolStripMenuItem miTrayExit = new ToolStripMenuItem( Language.Sleep_TrayIcon_Exit, null, miTrayExit_Click );

      trayIcon.ContextMenuStrip = new ContextMenuStrip();
      trayIcon.ContextMenuStrip.Items.AddRange( new ToolStripItem[] { miTrayWakeUp, miTrayAutoWake, miTrayExit } );

      if( autoWakeup )
        tmrAutoWake.Start();
    }

    #endregion

    #region Methods

    /// <summary>
    /// Attempts to start the main program and, if successful, exits.
    /// </summary>
    /// <param name="startMinimised">If true, will be started as tray icon only.</param>
    private void WakeUp( bool startMinimised )
    {
      try
      {
        Process.Start( Application.ExecutablePath, startMinimised ? "minimised" : "" );
      }
      catch( Exception ex )
      {
        MessageBox.Show( Language.Error_Wakeup + ":\n\n" + ex.Message, Language.Error_Error, MessageBoxButtons.OK, MessageBoxIcon.Error );
        return;
      }

      trayIcon.Visible = false;
      Application.Exit();
    }

    #endregion

    #region Event Handlers

    // also show menu on left click
    private void trayIcon_MouseClick( object sender, MouseEventArgs e )
    {
      if( e.Button == MouseButtons.Left )
      {
        //cmsTrayIcon.Show();  // causes empty window on taskbar?

        // call private ShowContextMenu() method via reflection
        trayIcon.GetType().InvokeMember( "ShowContextMenu",
                                         BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.NonPublic,
                                         null, trayIcon, null );
      }
    }

    // context menu items
    private void miTrayWakeUp_Click( object sender, EventArgs e )
    {
      Log.AddEntry( "User requested wakeup" );
      WakeUp( false );
    }
    private void miTrayAutoWake_Click( object sender, EventArgs e )
    {
      autoWakeup = !autoWakeup;

      ( (ToolStripMenuItem)sender ).Text = autoWakeup ? Language.Sleep_TrayIcon_DisableAutoWakeup : Language.Sleep_TrayIcon_EnableAutoWakeup;

      if( autoWakeup )
        tmrAutoWake.Start();
      else
        tmrAutoWake.Stop();

      autoWakeupCount = 0;
    }
    private void miTrayExit_Click( object sender, EventArgs e )
    {
      trayIcon.Visible = false;
      Application.Exit();
    }

    // autowake timer
    private void tmrAutoWake_Tick( object sender, EventArgs e )
    {
      if( BegmMisc.WW2Running() )
        autoWakeupCount = 0;  // reset count
      else
        autoWakeupCount++;

      if( autoWakeupCount >= WAIT_TIME )  // ww2online hasn't been running for at least WAIT_TIME consecutive minutes
      {
        autoWakeup = false;
        trayIcon.ContextMenuStrip.Items[1].Text = Language.Sleep_TrayIcon_EnableAutoWakeup;
        tmrAutoWake.Stop();

        Log.AddEntry( "Doing autowakeup after {0} minutes", WAIT_TIME );
        WakeUp( true );
      }
    }

    #endregion
  }
}
