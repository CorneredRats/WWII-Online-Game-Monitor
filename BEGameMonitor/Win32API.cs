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
using System.Runtime.InteropServices;

namespace BEGM
{
  /// <summary>
  /// A static class that contains imported user32.dll functions.
  /// </summary>
  public static class User32
  {
    // About.ScrollLogToBottom()

    public const int WM_VSCROLL = 0x115;
    public const int SB_BOTTOM = 7;

    [DllImport( "user32.dll" )]
    public static extern int SendMessage( IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam );


    // BegmMisc.IsFullScreen()

    [DllImport( "user32.dll" )]
    public static extern IntPtr GetForegroundWindow();

    [DllImport( "user32.dll" )]
    public static extern IntPtr FindWindow( string lpClassName, string lpWindowName );

    [DllImport( "user32.dll" )]
    public static extern bool GetWindowRect( IntPtr hWnd, out RECT lpRect );

    [StructLayout( LayoutKind.Sequential )]
    public struct RECT
    {
      public int Left;
      public int Top;
      public int Right;
      public int Bottom;
    }


    // BegmMisc.GetIdleTime()

    [DllImport( "user32.dll" )]
    public static extern bool GetLastInputInfo( ref LASTINPUTINFO ii );

    [StructLayout( LayoutKind.Sequential )]
    public struct LASTINPUTINFO
    {
      [MarshalAs( UnmanagedType.U4 )]
      public int cbSize;
      [MarshalAs( UnmanagedType.U4 )]
      public int dwTime;
    }


    // AlertWindow.ShowAlerts()

    [DllImport( "user32.dll" )]
    public static extern bool ShowWindow( IntPtr hWnd, WindowShowStyle nCmdShow );

    public enum WindowShowStyle : uint
    {
      Hide = 0,
      ShowNormal = 1,
      ShowMinimized = 2,
      ShowMaximized = 3,
      Maximize = 3,
      ShowNormalNoActivate = 4,
      Show = 5,
      Minimize = 6,
      ShowMinNoActivate = 7,
      ShowNoActivate = 8,
      Restore = 9,
      ShowDefault = 10,
      ForceMinimized = 11
    }


    // GameMap set wallpaper

    public const int SPI_SETDESKWALLPAPER = 20;
    public const int SPIF_UPDATEINIFILE = 0x01;
    public const int SPIF_SENDWININICHANGE = 0x02;

    [DllImport( "user32.dll" )]
    public static extern int SystemParametersInfo( int uAction, int uParam, string lpvParam, int fuWinIni );
  }

  /// <summary>
  /// A static class that contains imported gdi.dll functions.
  /// </summary>
  public static class Gdi32
  {
    [DllImport( "gdi32.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true )]
    public static extern bool BitBlt( IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, TernaryRasterOperations dwRop );

    [DllImport( "gdi32.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true )]
    public static extern IntPtr CreateCompatibleDC( IntPtr hdc );

    [DllImport( "gdi32.dll" )]
    public static extern IntPtr CreateCompatibleBitmap( IntPtr hdc, int nWidth, int nHeight );

    [DllImport( "gdi32.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true )]
    public static extern IntPtr DeleteDC( IntPtr hDc );

    [DllImport( "gdi32.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true )]
    public static extern IntPtr DeleteObject( IntPtr hObject );

    [DllImport( "gdi32.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true )]
    public static extern IntPtr SelectObject( IntPtr hdc, IntPtr hgdiobj );

    [DllImport( "gdi32.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true )]
    public static extern int SetStretchBltMode( IntPtr hdc, StretchMode iStretchMode );

    [DllImport( "gdi32.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true )]
    public static extern bool StretchBlt( IntPtr hdcDest, int nXOriginDest, int nYOriginDest, int nWidthDest, int nHeightDest,
      IntPtr hdcSrc, int nXOriginSrc, int nYOriginSrc, int nWidthSrc, int nHeightSrc, TernaryRasterOperations dwRop );

    [DllImport( "gdi32.dll", EntryPoint = "GdiAlphaBlend" )]
    public static extern bool AlphaBlend( IntPtr hdcDest, int nXOriginDest, int nYOriginDest, int nWidthDest, int nHeightDest,
        IntPtr hdcSrc, int nXOriginSrc, int nYOriginSrc, int nWidthSrc, int nHeightSrc, BLENDFUNCTION blendFunction );
    
    public enum TernaryRasterOperations
    {
      BLACKNESS = 0x42,
      DSTINVERT = 0x550009,
      MERGECOPY = 0xc000ca,
      MERGEPAINT = 0xbb0226,
      NOTSRCCOPY = 0x330008,
      NOTSRCERASE = 0x1100a6,
      PATCOPY = 0xf00021,
      PATINVERT = 0x5a0049,
      PATPAINT = 0xfb0a09,
      SRCAND = 0x8800c6,
      SRCCOPY = 0xcc0020,
      SRCERASE = 0x440328,
      SRCINVERT = 0x660046,
      SRCPAINT = 0xee0086,
      WHITENESS = 0xff0062
    }

    public enum StretchMode
    {
      STRETCH_ANDSCANS = 1,
      STRETCH_ORSCANS = 2,
      STRETCH_DELETESCANS = 3,
      STRETCH_HALFTONE = 4,
    }

    [StructLayout( LayoutKind.Sequential )]
    public struct BLENDFUNCTION
    {
      byte BlendOp;
      byte BlendFlags;
      byte SourceConstantAlpha;
      byte AlphaFormat;

      public BLENDFUNCTION( byte op, byte flags, byte alpha, byte format )
      {
        BlendOp = op;
        BlendFlags = flags;
        SourceConstantAlpha = alpha;
        AlphaFormat = format;
      }
    }

    public const byte AC_SRC_OVER = 0x00;
    public const byte AC_SRC_ALPHA = 0x01;
  }

  /// <summary>
  /// A static class that contains imported kernel32.dll functions.
  /// </summary>
  public static class Kernel32
  {
    [return: MarshalAs( UnmanagedType.Bool )]
    [DllImport( "kernel32.dll" )]
    public static extern bool GlobalMemoryStatusEx( [In, Out] MEMORYSTATUSEX lpBuffer );

    /// <summary>
    /// contains information about the current state of both physical and virtual memory, including extended memory
    /// </summary>
    [StructLayout( LayoutKind.Sequential )]
    public class MEMORYSTATUSEX
    {
      /// <summary>
      /// Size of the structure, in bytes. You must set this member before calling GlobalMemoryStatusEx. 
      /// </summary>
      public uint dwLength;

      /// <summary>
      /// Number between 0 and 100 that specifies the approximate percentage of physical memory that is in use (0 indicates no memory use and 100 indicates full memory use). 
      /// </summary>
      public uint dwMemoryLoad;

      /// <summary>
      /// Total size of physical memory, in bytes.
      /// </summary>
      public ulong ullTotalPhys;

      /// <summary>
      /// Size of physical memory available, in bytes. 
      /// </summary>
      public ulong ullAvailPhys;

      /// <summary>
      /// Size of the committed memory limit, in bytes. This is physical memory plus the size of the page file, minus a small overhead. 
      /// </summary>
      public ulong ullTotalPageFile;

      /// <summary>
      /// Size of available memory to commit, in bytes. The limit is ullTotalPageFile. 
      /// </summary>
      public ulong ullAvailPageFile;

      /// <summary>
      /// Total size of the user mode portion of the virtual address space of the calling process, in bytes. 
      /// </summary>
      public ulong ullTotalVirtual;

      /// <summary>
      /// Size of unreserved and uncommitted memory in the user mode portion of the virtual address space of the calling process, in bytes. 
      /// </summary>
      public ulong ullAvailVirtual;

      /// <summary>
      /// Size of unreserved and uncommitted memory in the extended portion of the virtual address space of the calling process, in bytes. 
      /// </summary>
      public ulong ullAvailExtendedVirtual;

      /// <summary>
      /// Initializes a new instance of the <see cref="MEMORYSTATUSEX"/> class.
      /// </summary>
      public MEMORYSTATUSEX()
      {
        this.dwLength = (uint)Marshal.SizeOf( typeof( MEMORYSTATUSEX ) );
      }

      public override string ToString()
      {
        return String.Format( "MemLoad {0}%, PhysicalMem {1}/{2}MB, PageFile {3}/{4}MB, VirtualMem {5}+{6}/{7}MB",
                              dwMemoryLoad,
                              ullAvailPhys / 1048576, ullTotalPhys / 1048576,
                              ullAvailPageFile / 1048576, ullTotalPageFile / 1048576,
                              ullAvailVirtual / 1048576, ullAvailExtendedVirtual / 1048576, ullTotalVirtual / 1048576 );
      }
    }

  }

}
