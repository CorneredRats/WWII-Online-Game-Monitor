/* =============================================================================
 * Xiperware Wiretap API                            Copyright (c) 2013 Xiperware
 * http://begm.sourceforge.net/                              xiperware@gmail.com
 * 
 * This file is part of the Xiperware Wiretap API library for WW2 Online.
 * 
 * This library is free software; you can redistribute it and/or modify it under
 * the terms of the GNU Lesser General Public License version 2.1 as published
 * by the Free Software Foundation.
 * 
 * This program is distributed in the hope that it will be useful, but WITHOUT
 * ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 * FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more
 * details.
 * 
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program. If not, see <http://www.gnu.org/licenses/>.
 * =============================================================================
 */

using System;
using System.Drawing;

namespace Xiperware.WiretapAPI
{
  /// <summary>
  /// Encapsulates all the map size and conversion calculations based on a specific map size.
  /// </summary>
  public class MapMetrics
  {
    #region Constants

    /// <summary>
    /// The AWS grid dimensions in meters.
    /// </summary>
    public readonly RectangleGrid GridMeters = new RectangleGrid( -336400, 3663520, 16000, 16000, 101, 61 );

    #endregion

    #region Variables

    private readonly SizeF metersPerPixel;

    #endregion

    #region Properties

    /// <summary>
    /// The map dimensions in pixels.
    /// </summary>
    public Rectangle Pixels { get; private set; }

    /// <summary>
    /// The map dimensions in meters.
    /// </summary>
    public RectangleF Meters { get; private set; }

    /// <summary>
    /// The scale code used by this map size.
    /// </summary>
    public int ScaleCode { get; private set; }

    /// <summary>
    /// The map dimensions in degrees.
    /// </summary>
    public SizeF Degrees { get; private set; }

    /// <summary>
    /// The AWS grid dimensions in pixels.
    /// </summary>
    public RectangleGrid GridPixels { get; private set; }

    #endregion

    #region Constructors

    /// <summary>
    /// Create a new MapMetrics object.
    /// </summary>
    /// <param name="pixels">The size of the map in pixels.</param>
    /// <param name="meters">The size and location of the map in meters (relative to lat/long 0/0).</param>
    /// <param name="scaleCode">The map scale size (40, 60, 80, 100).</param>
    /// <remarks>
    /// meters.X  + = right, - = left
    /// meters.Y  + = down , - = up
    /// </remarks>
    public MapMetrics( Size pixels, RectangleF meters, int scaleCode )
    {
      this.metersPerPixel = new SizeF(
        meters.Width  / pixels.Width,
        meters.Height / pixels.Height
      );

      this.Pixels = new Rectangle(
        (int)( meters.X / this.metersPerPixel.Width ),
        (int)( meters.Y / this.metersPerPixel.Height ),
        pixels.Width,
        pixels.Height
      );
      this.Meters = meters;
      this.ScaleCode = scaleCode;

      this.Degrees = new SizeF(
        this.Meters.Width  / 8640000 * 180,
        this.Meters.Height / 4211200 * 70.1875F
      );

      this.GridPixels = new RectangleGrid(
         MetersToMapF( this.GridMeters.Cell ),
         MetersToMapF( this.GridMeters.Bounds ),
         this.GridMeters.CellCount
        );
    }

    #endregion

    #region Methods

    /// <summary>
    /// Convert game world octet coordinates (800x800m) to game world meters.
    /// </summary>
    /// <param name="octets">A Point in game world octets.</param>
    /// <returns>A Point in game world meters.</returns>
    public static Point OctetToMeters( Point octets )
    {
      return new Point( ( octets.X * 800 ) + 400,
                        ( octets.Y * 800 ) + 600 );
    }

    /// <summary>
    /// Convert game world octet coordinates (800x800m) to game world meters.
    /// </summary>
    /// <param name="octets">A Point in game world octets.</param>
    /// <returns>A Point in game world meters.</returns>
    public static Point OctetToMeters( PointF octets )
    {
      return new Point( (int)Math.Round( ( octets.X * 800 ) + 400 ),
                        (int)Math.Round( ( octets.Y * 800 ) + 600 ) );
    }

    /// <summary>
    /// Convert game world octet coordinates (800x800m) to map pixel coordinates.
    /// </summary>
    /// <param name="octets">A Point in game world octets.</param>
    /// <returns>A Point in map pixels.</returns>
    public Point OctetToMap( Point octets )
    {
      return OctetToMap( new PointF( octets.X, octets.Y ) );
    }

    /// <summary>
    /// Convert game world octet coordinates (800x800m) to map pixel coordinates.
    /// </summary>
    /// <param name="octets">A PointF in game world octets.</param>
    /// <returns>A Point in map pixels.</returns>
    public Point OctetToMap( PointF octets )
    {
      return MetersToMap( OctetToMeters( octets ) );
    }

    /// <summary>
    /// Convert game world meters coordinates to map pixels coordinates.
    /// </summary>
    /// <param name="meters">A Point in game world meters.</param>
    /// <returns>A Point in map pixels.</returns>
    public Point MetersToMap( Point meters )
    {
      PointF map = MetersToMapF( meters );
      return new Point( (int)Math.Round( map.X ), (int)Math.Round( map.Y ) );
    }

    /// <summary>
    /// Convert game world meters coordinates to map pixels coordinates.
    /// </summary>
    /// <param name="meters">A SizeF in game world meters.</param>
    /// <returns>A Size in map pixels.</returns>
    public Size MetersToMap( SizeF meters )
    {
      SizeF map = MetersToMapF( meters );
      return new Size( (int)Math.Round( map.Width ), (int)Math.Round( map.Height ) );
    }

    /// <summary>
    /// Convert game world meters coordinates to map pixels coordinates.
    /// </summary>
    /// <param name="meters">A Rectangle in game world meters.</param>
    /// <returns>A Rectangle in map pixels.</returns>
    public Rectangle MetersToMap( Rectangle meters )
    {
      Point location = MetersToMap( meters.Location );
      Size size = MetersToMap( meters.Size );

      return new Rectangle( location, size );
    }

    /// <summary>
    /// Convert game world meters coordinates to map pixels coordinates.
    /// </summary>
    /// <param name="meters">A Point in game world meters.</param>
    /// <returns>A PointF in map pixels.</returns>
    public PointF MetersToMapF( PointF meters )
    {
      float x = this.Pixels.X + ( ( +meters.X ) / this.metersPerPixel.Width  );
      float y = this.Pixels.Y + ( ( -meters.Y ) / this.metersPerPixel.Height );

      return new PointF( x, y );
    }

    /// <summary>
    /// Convert game world meters coordinates to map pixels coordinates.
    /// </summary>
    /// <param name="meters">A SizeF in game world meters.</param>
    /// <returns>A SizeF in map pixels.</returns>
    public SizeF MetersToMapF( SizeF meters )
    {
      float width  = meters.Width  / this.metersPerPixel.Width;
      float height = meters.Height / this.metersPerPixel.Height;

      return new SizeF( width, height );
    }

    /// <summary>
    /// Convert game world meters coordinates to map pixels coordinates.
    /// </summary>
    /// <param name="meters">A Rectangle in game world meters.</param>
    /// <returns>A Rectangle in map pixels.</returns>
    public RectangleF MetersToMapF( RectangleF meters )
    {
      PointF location = MetersToMapF( meters.Location );
      SizeF  size     = MetersToMapF( meters.Size );

      return new RectangleF( location, size );
    }

    /// <summary>
    /// Convert map pixels coordinates to game world meters coordinates.
    /// </summary>
    /// <param name="map">A Point in map pixels.</param>
    /// <returns>A Point in game world meters.</returns>
    public Point MapToMeters( Point map )
    {
      int x = (int)Math.Round( ( map.X - this.Pixels.X ) * +metersPerPixel.Width  );
      int y = (int)Math.Round( ( map.Y - this.Pixels.Y ) * -metersPerPixel.Height );

      return new Point( x, y );
    }

    /// <summary>
    /// Convert map pixels coordinates to AWS grid-style cell reference.
    /// </summary>
    /// <param name="map">A Point in map pixels.</param>
    /// <returns>Cell name, eg "AG13"</returns>
    public string MapToCell( Point map )
    {
      // convert map pixels to game world meters

      Point meters = MapToMeters( map );


      // calc cell number

      if( meters.X < this.GridMeters.Bounds.X || meters.Y > this.GridMeters.Bounds.Y ) return null;  // off top/left of grid

      int cellnumX = (int)( ( meters.X - this.GridMeters.Bounds.X ) / this.GridMeters.Cell.Width );  // 0 - 100
      if( cellnumX > this.GridMeters.CellCount.X ) return null;  // off right of grid

      int cellnumY = (int)( ( this.GridMeters.Bounds.Y - meters.Y ) / this.GridMeters.Cell.Height );  // 0 - 60
      if( cellnumY > this.GridMeters.CellCount.Y - 1 ) return null;  // off bottom of grid


      // return cell reference

      return String.Format( "{0}{1}{2}", (char)( ( cellnumX / 26 ) + 65 ), (char)( ( cellnumX % 26 ) + 65 ), cellnumY );
    }

    /// <summary>
    /// Convert map pixels coordinates to lat/long coordinate string.
    /// </summary>
    /// <param name="map">A Point in map pixels.</param>
    /// <returns>A formatted Lat/Long string.</returns>
    public string MapToLatLong( Point map )
    {
      // convert map pixels to game world meters

      double metersX = ( map.X - this.Pixels.X ) * +metersPerPixel.Width;
      double metersY = ( map.Y - this.Pixels.Y ) * -metersPerPixel.Height;


      // convert game meters to real-world degrees

      double degreesX = ( metersX / this.Meters.Width  ) * this.Degrees.Width;
      double degreesY = ( metersY / this.Meters.Height ) * this.Degrees.Height;


      return String.Format( new Misc.LatLongFormatter(), "{0:Y}, {1:X}", degreesY, degreesX );
    }

    /// <summary>
    /// Convert game world octet coordinates to decimal lat/long coordinates.
    /// </summary>
    /// <param name="octets">A Point in game world octets.</param>
    /// <param name="x">The decimal latitude.</param>
    /// <param name="y">The decimal longitude.</param>
    public void OctetToDecimalLatLong( Point octets, out double x, out double y )
    {
      Point meters = OctetToMeters( octets );
      x = ( meters.X / this.Meters.Width  ) * this.Degrees.Width;
      y = ( meters.Y / this.Meters.Height ) * this.Degrees.Height;
    }

    /// <summary>
    /// Convert decimal lat/long coordinates to map pixels.
    /// </summary>
    /// <param name="degrees">A PointF in decimal degrees.</param>
    /// <returns>A Point in map pixels.</returns>
    public Point DecimalLatLongToMap( PointF degrees )
    {
      Point meters = new Point();
      meters.X = (int)Math.Round( ( degrees.X / this.Degrees.Width  ) * this.Meters.Width  );
      meters.Y = (int)Math.Round( ( degrees.Y / this.Degrees.Height ) * this.Meters.Height );

      return MetersToMap( meters );
    }

    /// <summary>
    /// Scales a number up based on the size of the loaded map.
    /// </summary>
    /// <param name="value40">The value to use at 40%.</param>
    /// <returns>The value to use at the current map size.</returns>
    public float MapScaleUp( float value40 )
    {
      switch( this.ScaleCode )
      {
        case 40:
          return value40;
        case 60:
        case 80:
        case 100:
          //float scaleUpFactor = this.mapSize * 0.025F;   // 40 = 1.0,  60 = 1.5,  80 = 2.0,  100 = 2.5
          return value40 * this.ScaleCode * 0.025F;
      }
      throw new ApplicationException( "Invalid map scale" );
    }

    /// <summary>
    /// Scales a number up based on the size of the loaded map.
    /// </summary>
    /// <param name="value40">The value to use at 40%.</param>
    /// <param name="value100">The value to use at 100%.</param>
    /// <returns>The value to use at the current map size.</returns>
    public float MapScaleUp( float value40, float value100 )
    {
      switch( this.ScaleCode )
      {
        case 40:
          return value40;
        case 60:
        case 80:
          return value40 + ( ( ( value100 - value40 ) / 60F ) * ( this.ScaleCode - 40 ) );
        case 100:
          return value100;
      }
      throw new ApplicationException( "Invalid map scale" );
    }

    /// <summary>
    /// Scales a rectangle up based on the size of the loaded map.
    /// </summary>
    /// <param name="size">The original size of the rectangle.</param>
    /// <param name="w40">The width to use at 40%.</param>
    /// <param name="w100">The width to use at 100%.</param>
    /// <returns>The rectangle to use at the current map size.</returns>
    public SizeF MapScaleUp( Size size, float w40, float w100 )
    {
      float ratio = (float)size.Height / (float)size.Width;
      switch( this.ScaleCode )
      {
        case 40:
          return new SizeF( w40, ratio * w40 );
        case 60:
        case 80:
          float width = MapScaleUp( w40, w100 );
          return new SizeF( width, ratio * width );
        case 100:
          return new SizeF( w100, ratio * w100 );
      }
      throw new ApplicationException( "Invalid map scale" );
    }

    #endregion
  }


  /// <summary>
  /// Stores the bounds, cell size and cell count of a grid.
  /// </summary>
  public struct RectangleGrid
  {
    #region Fields

    /// <summary>
    /// The size of a cell.
    /// </summary>
    public SizeF Cell;

    /// <summary>
    /// The location and size of the grid.
    /// </summary>
    public RectangleF Bounds;

    /// <summary>
    /// The number of rows and columns.
    /// </summary>
    public Point CellCount;

    #endregion

    #region Constructors

    /// <summary>
    /// Create a new RectangleGrid.
    /// </summary>
    /// <param name="cell"></param>
    /// <param name="bounds"></param>
    /// <param name="cellCount"></param>
    public RectangleGrid( SizeF cell, RectangleF bounds, Point cellCount )
    {
      this.Cell = cell;
      this.Bounds = bounds;
      this.CellCount = cellCount;
    }

    /// <summary>
    /// Create a new RectangleGrid.
    /// </summary>
    /// <param name="x">The grid x coord of the top left corner.</param>
    /// <param name="y">The grid y coord of the top left corner.</param>
    /// <param name="width">The grid cell width.</param>
    /// <param name="height">The grid cell height.</param>
    /// <param name="cols">The number of columns in the grid.</param>
    /// <param name="rows">The number of rows in the grid.</param>
    public RectangleGrid( float x, float y, float width, float height, int cols, int rows )
    {
      this.Cell = new SizeF( width, height );
      this.Bounds = new RectangleF( x, y, width * cols, height * rows );
      this.CellCount = new Point( cols, rows );
    }

    #endregion
  }
}
