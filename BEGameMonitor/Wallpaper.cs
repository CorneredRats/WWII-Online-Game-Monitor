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

using System.IO;
using Microsoft.Win32;  // Registry

namespace BEGM
{
  /// <summary>
  /// The wallpaper that windows displays on the users desktop.
  /// </summary>
  public class Wallpaper
  {
    #region Variables

    private string path;
    private string style;
    private string tile;

    #endregion

    #region Constructors

    /// <summary>
    /// Create a new Wallpaper object (used internally).
    /// </summary>
    /// <param name="path">The path of the wallpaper file.</param>
    /// <param name="style">1 = tiled/centered, 2 = stretched</param>
    /// <param name="tile">0 = stretched/centered, 1 = tiled</param>
    private Wallpaper( string path, string style, string tile )
    {
      this.path = path;
      this.style = style;
      this.tile = tile;
    }

    /// <summary>
    /// Get a new Wallpaper object based on BEGM's game map wallpaper.
    /// </summary>
    public static Wallpaper BEGM
    {
      get
      {
        return new Wallpaper( Path.Combine( Path.GetTempPath(), "BEGameMonitor_wallpaper.bmp" ), "1", "0" );
      }
    }

    /// <summary>
    /// Get a new Wallpaper object based on the current wallpaper.
    /// </summary>
    public static Wallpaper Current
    {
      get
      {
        string path = null, style = null, tile = null;

        try
        {
          RegistryKey key = Registry.CurrentUser.OpenSubKey( "Control Panel\\Desktop", false );
          if( key != null )
          {
            path = (string)key.GetValue( "Wallpaper", null );
            style = (string)key.GetValue( "WallpaperStyle", null );
            tile = (string)key.GetValue( "TileWallpaper", null );

            key.Close();
          }
        }
        catch { }

        return new Wallpaper( path, style, tile );
      }
    }

    #endregion

    #region Properties

    /// <summary>
    /// The path of the wallpaper file.
    /// </summary>
    public string FilePath
    {
      get { return this.path; }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Applies the current wallpaper settings to the registry (doesn't update the wallpaper).
    /// </summary>
    public void Apply()
    {
      try
      {
        RegistryKey key = Registry.CurrentUser.OpenSubKey( "Control Panel\\Desktop", true );
        if( key != null )
        {
          key.SetValue( "WallpaperStyle", this.style );
          key.SetValue( "TileWallpaper", this.tile );
          key.Close();
        }
      }
      catch { }
    }

    /// <summary>
    /// Updates the wallpaper with the current content of the wallpaper file.
    /// </summary>
    public void Update()
    {
      User32.SystemParametersInfo( User32.SPI_SETDESKWALLPAPER, 0, this.path, User32.SPIF_UPDATEINIFILE | User32.SPIF_SENDWININICHANGE );
    }

    /// <summary>
    /// Populate this object with values stored in the registry.
    /// </summary>
    public void LoadFromRegistry()
    {
      try
      {
        RegistryKey key = Registry.CurrentUser.OpenSubKey( "Software\\BEGameMonitor" );
        if( key == null ) return;

        this.path = (string)key.GetValue( "OriginalWallpaperPath", "" );
        this.style = (string)key.GetValue( "OriginalWallpaperStyle", "1" );
        this.tile = (string)key.GetValue( "OriginalWallpaperTile", "0" );

        key.Close();
      }
      catch { }
    }

    /// <summary>
    /// Save the values in this object to the registry.
    /// </summary>
    public void SaveToRegistry()
    {
      try
      {
        RegistryKey key = Registry.CurrentUser.CreateSubKey( "Software\\BEGameMonitor" );
        if( key != null )
        {
          key.SetValue( "OriginalWallpaperPath", this.path );
          key.SetValue( "OriginalWallpaperStyle", this.style );
          key.SetValue( "OriginalWallpaperTile", this.tile );
          key.Close();
        }
      }
      catch { }
    }

    #endregion
  }
}
