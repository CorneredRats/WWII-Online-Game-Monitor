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
using System.Collections;
using System.Drawing;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using XPExplorerBar;
using Xiperware.WiretapAPI;

namespace BEGM
{
  #region Delegates

  public delegate void RevealDelegate( WidgetType widgetType, object arg );

  #endregion

  #region Enums

  /// <summary>
  /// The widgets used in the GameStatus window.
  /// </summary>
  public enum WidgetType
  {
    ServerStatus   = 0,
    RecentEvents   = 1,
    CurrentAttacks = 2,
    TownStatus     = 3,
    GameMap        = 4,
    FactoryStatus  = 5,
    OrderOfBattle  = 6,
    BrigadeStatus  = 7,
    Equipment      = 8,
  }

  #endregion

  /// <summary>
  /// Miscellaneous static methods used throughout BEGM.
  /// </summary>
  public static class BegmMisc
  {
    #region Forms

    /// <summary>
    /// Focus()'s the control without the implied ScrollControlIntoView(). This prevents things
    /// jumping around while doing focus-follows-cursor.
    /// </summary>
    /// <param name="control">The control to Focus().</param>
    public static void FocusWithoutScroll( Control control )
    {
      Control focusControl = control;

      Form form = control.FindForm();
      if( form != null && !form.ContainsFocus ) return;  // form doesn't have focus, don't steal

      TaskPane taskpane = null;
      while( control != null )
      {
        if( control is TaskPane )
          taskpane = control as TaskPane;
        control = control.Parent;
      }

      if( taskpane != null ) taskpane.PreventAutoScroll = true;
      focusControl.Focus();
      if( taskpane != null ) taskpane.PreventAutoScroll = false;
    }

    /// <summary>
    /// Focus()'s the outer-most TaskPane that contains the given control.
    /// </summary>
    /// <param name="control">The child control to start searching with.</param>
    public static void FocusTaskPane( Control control )
    {
      Form form = control.FindForm();
      if( form != null && !form.ContainsFocus ) return;  // form doesn't have focus, don't steal

      TaskPane taskpane = null;
      while( control != null )
      {
        if( control is TaskPane )
          taskpane = control as TaskPane;
        control = control.Parent;
      }

      if( taskpane != null ) taskpane.Focus();
    }

    /// <summary>
    /// Localises a TreeView's nodes by updating their Text property with the
    /// values for the current culture.
    /// </summary>
    /// <remarks>
    /// By default, TreeNode localised settings are stored in the resx xml as
    /// base64 encoded streams, which isn't very friendly for translators.
    /// Instead we use the values in Language.resx with the name format
    /// "TreeView_(treename)_(nodename)".
    /// </remarks>
    /// <param name="treeviews">One or more TreeView controls to localise.</param>
    public static void LocaliseTreeView( params TreeView[] treeviews )
    {
      foreach( TreeView treeview in treeviews )
        foreach( TreeNode parentNode in treeview.Nodes )
          LocaliseTreeNode( parentNode );
    }

    /// <summary>
    /// Private recursive method used by LocaliseTreeView().
    /// </summary>
    /// <param name="parentNode">The parent tree node to recurse.</param>
    private static void LocaliseTreeNode( TreeNode parentNode )
    {
      parentNode.Text = Language.ResourceManager.GetString( String.Format( "TreeView_{0}_{1}", parentNode.TreeView.Name, parentNode.Name ) );

      foreach( TreeNode childNode in parentNode.Nodes )
        LocaliseTreeNode( childNode );
    }

    /// <summary>
    /// Show a generic error dialog.
    /// </summary>
    /// <param name="message">The error message.</param>
    public static void ShowError( string message )
    {
      return;  // don't show errors

      if( Application.OpenForms.Count == 0 )
        MessageBox.Show( message, Language.Error_Error, MessageBoxButtons.OK, MessageBoxIcon.Error );
      else if( Application.OpenForms[0].InvokeRequired )
        Application.OpenForms[0].Invoke( new Action<string>( ShowError ), message );
      else
        MessageBox.Show( Application.OpenForms[0], message, Language.Error_Error,MessageBoxButtons.OK, MessageBoxIcon.Error );
    }

    #endregion

    #region System

    /// <summary>
    /// Check to see if the game is currently running.
    /// </summary>
    /// <returns>True if process is detected.</returns>
    public static bool WW2Running()
    {
#if MAC
      return false;
#else
      return Process.GetProcessesByName( "WW2_sse2" ).Length > 0 || Process.GetProcessesByName( "WW2_x86" ).Length > 0;
#endif
    }

    /// <summary>
    /// Check to see whether the user is currently running a full-screen app (eg, game, movie, etc).
    /// </summary>
    /// <param name="displayIndex">The number of the display to test.</param>
    /// <remarks>Used to avoid popping up alerts.</remarks>
    /// <returns>True if the active window has the same bounds as the primary screen.</returns>
    public static bool IsFullScreen( int displayIndex )
    {
      Rectangle screen = Screen.AllScreens[displayIndex].Bounds;
      IntPtr activeWindow = User32.GetForegroundWindow();
      User32.RECT rect;

      if( !User32.GetWindowRect( activeWindow, out rect ) )
        return false;  // failed?

      if( rect.Left == screen.Left && rect.Top == screen.Top
       && rect.Right == screen.Right && rect.Bottom == screen.Bottom )
      {
        IntPtr desktopWindow = User32.FindWindow( "progman", null );
        if( activeWindow != desktopWindow )  // don't include desktop
          return true;
      }

      return false;
    }

    /// <summary>
    /// Get the amount of time the user has been idle.
    /// </summary>
    /// <returns>The time span since last mouse or keyboard input.</returns>
    public static TimeSpan GetIdleTime()
    {
      User32.LASTINPUTINFO ii = new User32.LASTINPUTINFO();
      ii.cbSize = Marshal.SizeOf( ii );
      ii.dwTime = 0;

      long idleTicks = 0;
      if( User32.GetLastInputInfo( ref ii ) )
        idleTicks = Environment.TickCount - ii.dwTime;

      return new TimeSpan( idleTicks * 10000 );
    }

    #endregion

    #region Classes

    /// <summary>
    /// Utility class for measuring the maximum width of text strings.
    /// Used when manually formatting text columns.
    /// </summary>
    public class TextWidth
    {
      #region Variables

      private readonly Font font;
      private readonly int min;
      private readonly int max;

      private int maxWidth;

      #endregion

      #region Constructors

      /// <summary>
      /// Create a new TextWidth using the given font.
      /// </summary>
      /// <param name="font">The font to use when measuring.</param>
      public TextWidth( Font font )
        : this( font, -1, -1 )
      {
        
      }

      /// <summary>
      /// Create a new TextWidth using the given font and min/max ranges.
      /// </summary>
      /// <param name="font">The font to use when measuring.</param>
      /// <param name="min">The minimum value of MaxWidth.</param>
      /// <param name="max">The maximum value of MaxWidth.</param>
      public TextWidth( Font font, int min, int max )
      {
        this.font = font;
        this.min = min;
        this.max = max;
      }

      #endregion

      #region Properties

      /// <summary>
      /// Gets the longest string measured.
      /// </summary>
      public int MaxWidth
      {
        get
        {
          if( this.min >= 0 && this.maxWidth < this.min )
            return this.min;

          if( this.max >= 0 && this.maxWidth > this.max )
            return this.max;

          return this.maxWidth;
        }
        set
        {
          this.maxWidth = value;
        }
      }

      #endregion

      #region Methods

      /// <summary>
      /// Measure a single object.
      /// </summary>
      /// <param name="obj">The object to measure.</param>
      public void Measure( object obj )
      {
        int width = TextRenderer.MeasureText( obj.ToString(), this.font ).Width;
        if( width > this.maxWidth )
          this.maxWidth = width;
      }

      /// <summary>
      /// Measure a collection of strings.
      /// </summary>
      /// <param name="objects">The collection of objects to measure.</param>
      /// <returns>The current TextWidth instance (for chaining commands).</returns>
      public TextWidth MeasureAll( IEnumerable objects )
      {
        foreach( object obj in objects )
          Measure( obj );

        return this;
      }

      #endregion
    }

    #endregion
  }
}
