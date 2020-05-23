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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using XLib.Extensions;

namespace Xiperware.WiretapAPI
{
  #region Exceptions

  /// <summary>
  /// Utility class for catching multiple exceptions and throwing the first.
  /// </summary>
  public class MultiExceptionHandler
  {
    #region Variables

    private const int NUM_EXCEPTIONS = 10;
    private Exception[] ex = new Exception[NUM_EXCEPTIONS];
    private int count = 0;

    #endregion

    #region Methods

    /// <summary>
    /// Add a new exception to the collection.
    /// </summary>
    /// <param name="ex">The exception to add.</param>
    public void Add( Exception ex )
    {
#if DEBUG
      if( !ex.Message.Contains( "missing attr" ) )
        this.ToString();  // <= breakpoint here
#endif

      if( this.count < NUM_EXCEPTIONS )
        this.ex[this.count] = ex;

      this.count++;
    }

    /// <summary>
    /// If any exceptions have occurred, throw the first.
    /// </summary>
    public void Throw()
    {
      if( this.count == 0 )
        return;

      if( this.count == 1 )
        throw new Exception( this.ex[0].Message, this.ex[0] );
      else
      {
#if DEBUG
        for( int i = 0; i < NUM_EXCEPTIONS && this.ex[i] != null; i++ )
          Log.AddError( "E{0:00}: {1}", i + 1, ex[i].Message );
#endif
        throw new Exception( String.Format( "{0} ({1} additional error(s))", this.ex[0].Message, this.count - 1 ).Pluralise(), this.ex[0] );
      }
    }

    #endregion
  }

  /// <summary>
  /// An exception thrown when an attribute has an invalid value.
  /// </summary>
  public class InvalidAttributeException : Exception
  {
    public InvalidAttributeException( string xmlname, string attr, object value )
      : base( String.Format( "Attribute '{0}:{1}' value '{2}' is invalid", xmlname, attr, value ) )
    {

    }
  }

  /// <summary>
  /// An exception thrown when an attribute has a duplicate value.
  /// </summary>
  public class DuplicateAttributeException : Exception
  {
    public DuplicateAttributeException( string xmlname, string attr, object value )
      : base( String.Format( "Attribute '{0}:{1}' value '{2}' already exists", xmlname, attr, value ) )
    {

    }
  }

  #endregion

  /// <summary>
  /// Miscellaneous static methods used throughout the library.
  /// </summary>
  public static class Misc
  {
    #region Graphics

    /// <summary>
    /// Generates a colour between white and yellow based on date where DateTime.Now is
    /// yellow, fading out to white after maxMinutes old.
    /// </summary>
    /// <param name="date">The reference DateTime.</param>
    /// <param name="maxMinutes">The age in minutes at which the colour fully fades to white.</param>
    /// <param name="white">The colour white to use.</param>
    /// <returns>The resulting Color.</returns>
    public static Color GetFadedYellow( DateTime date, int maxMinutes, int white )
    {
      TimeSpan ts = DateTime.Now - date;
      float percent = (float)( ts.TotalMinutes / maxMinutes );  // BlendColor will do bounds checking for us

      return BlendColour( Color.FromArgb( 224, 224, 100 ), Color.FromArgb( white, white, white ), percent );
    }

    /// <summary>
    /// Blend two colours together. 
    /// </summary>
    /// <param name="c1">The first colour.</param>
    /// <param name="c2">The second colour.</param>
    /// <param name="percent">The percentage of the second colour to mix with the first
    /// (0.0 = all first colour, 1.0 = all second colour).</param>
    /// <returns>A combination of the given colours according to percent.</returns>
    public static Color BlendColour( Color c1, Color c2, float percent )
    {
      if( percent < 0 ) percent = 0;
      if( percent > 1 ) percent = 1;

      int a = (int)Math.Round( ( ( c2.A - c1.A ) * percent ) + c1.A );
      int r = (int)Math.Round( ( ( c2.R - c1.R ) * percent ) + c1.R );
      int g = (int)Math.Round( ( ( c2.G - c1.G ) * percent ) + c1.G );
      int b = (int)Math.Round( ( ( c2.B - c1.B ) * percent ) + c1.B );

      return Color.FromArgb( a, r, g, b );
    }

    /// <summary>
    /// Applies a basic brightness filter to the given image while preserving the alpha channel.
    /// </summary>
    /// <param name="image">The image to adjust.</param>
    /// <param name="brightness">-255 - 255</param>
    /// <returns>The new modified image with the brightness adjusted.</returns>
    public static Bitmap AdjustBrightness( Bitmap image, int brightness )
    {
      if( image == null ) return null;

      Bitmap newbmp = new Bitmap( image.Width, image.Height );

      for( int x = 0; x < newbmp.Width; x++ )
      {
        for( int y = 0; y < newbmp.Height; y++ )
        {
          Color c = image.GetPixel( x, y );

          int r = c.R + brightness;
          int g = c.G + brightness;
          int b = c.B + brightness;

          if( r > 255 ) r = 255;
          if( g > 255 ) g = 255;
          if( b > 255 ) b = 255;

          newbmp.SetPixel( x, y, Color.FromArgb( c.A, r, g, b ) );
        }
      }

      return newbmp;
    }

    #endregion

    #region Date/Time

    /// <summary>
    /// Parse a timestamp in unix seconds format.
    /// </summary>
    /// <param name="epochSeconds">Seconds since epoch.</param>
    /// <returns>A DateTime in UTC.</returns>
    public static DateTime ParseTimestamp( string epochSeconds )
    {
      if( String.IsNullOrEmpty( epochSeconds ) )
        epochSeconds = "0";
      return ParseTimestamp( int.Parse( epochSeconds ) );
    }

    /// <summary>
    /// Parse a timestamp in unix seconds format.
    /// </summary>
    /// <param name="epochSeconds">Seconds since epoch.</param>
    /// <returns>A DateTime in UTC.</returns>
    public static DateTime ParseTimestamp( int epochSeconds )
    {
      if( epochSeconds == 0 )
        return DateTime.MinValue;
      else
        return new DateTime( 1970, 1, 1, 0, 0, 0, DateTimeKind.Utc ).AddSeconds( epochSeconds ).ToLocalTime();
    }

    /// <summary>
    /// Convert a DateTime to epoch seconds.
    /// </summary>
    /// <param name="date">The reference DateTime.</param>
    /// <returns>Seconds since epoch.</returns>
    public static int DateToSeconds( DateTime date )
    {
      TimeSpan span = date.ToUniversalTime() - new DateTime( 1970, 1, 1, 0, 0, 0, DateTimeKind.Utc );
      return (int)span.TotalSeconds;
    }

    /// <summary>
    /// Format a DateTime into a simple 12 or 24 hour timestamp.
    /// </summary>
    /// <param name="date">The reference DateTime.</param>
    /// <returns>A string in the format "[hh:mm]".</returns>
    public static string Timestamp( DateTime date )
    {
      if( DateTimeFormatInfo.CurrentInfo.LongTimePattern.Contains( "H" ) )  // ShortTimePattern is always 12 hour??
        return String.Format( "[{0:HH:mm}]", date );  // 24-hour
      else
        return String.Format( "[{0:hh:mm}]", date );  // 12-hour
    }

    /// <summary>
    /// Formats a date into a user-friendly string.
    /// </summary>
    /// <param name="date">The reference DateTime.</param>
    /// <returns>A string in the format "(today|yesterday|wed, 1 may), hh:mm [am|pm]".</returns>
    public static string FormatDateLong( DateTime date )
    {
      string dateString;

      if( date.DayOfYear == DateTime.Now.DayOfYear )
        dateString = Language.Time_Today;
      else if( date.DayOfYear == DateTime.Now.AddDays( -1 ).DayOfYear )
        dateString = Language.Time_Yesterday;
      else
        dateString = date.ToString( "ddd, d MMM" );

      if( DateTimeFormatInfo.CurrentInfo.LongTimePattern.Contains( "H" ) )  // ShortTimePattern is always 12 hour??
        dateString += String.Format( ", {0:HH:mm}", date );  // 24-hour
      else
        dateString += String.Format( ", {0:h:mm tt}", date );  // 12-hour

      return dateString;
    }

    /// <summary>
    /// Generate a string based on how old a date is (short format).
    /// </summary>
    /// <param name="date">The DateTime in the past to calculate mins since.</param>
    /// <returns>A string in the format "XdXh" or "XhXm" or "X mins".</returns>
    public static string MinsAgoShort( DateTime date )
    {
      return MinsAgoShort( date, DateTime.Now );
    }

    /// <summary>
    /// Generate a string based on how old a date is (short format).
    /// </summary>
    /// <param name="date">The DateTime in the past to calculate mins since.</param>
    /// <param name="reference">The reference DateTime (default Now).</param>
    /// <returns>A string in the format "XdXh" or "XhXm" or "X mins".</returns>
    public static string MinsAgoShort( DateTime date, DateTime reference )
    {
      TimeSpan ts = reference - date;

      if( ts.TotalSeconds <= 0 )
        return "0 " + Language.Time_Mins;
      else if( ts.Days > 0 )
        return String.Format( "{0}{1}{2}{3}", ts.Days, Language.Time_DaysLetter, ts.Hours, Language.Time_HoursLetter );
      else if( ts.Hours > 0 )
        return String.Format( "{0}{1}{2}{3}", ts.Hours, Language.Time_HoursLetter, ts.Minutes, Language.Time_MinsLetter );
      else
        return String.Format( "{0} {1}", ts.Minutes, ts.Minutes == 1 ? Language.Time_Min : Language.Time_Mins );
    }

    /// <summary>
    /// Generate a string based on how old a date is (long format).
    /// </summary>
    /// <param name="date">The DateTime in the past to calculate mins since.</param>
    /// <returns>A string in the format "x days, x hours, x mins" or "x hours, x mins" or "x mins".</returns>
    public static string MinsAgoLong( DateTime date )
    {
      TimeSpan ts = DateTime.Now - date;

      if( ts.TotalSeconds <= 0 )
        return "0 " + Language.Time_Mins;
      else if( ts.Days > 0 )
        return String.Format( "{0} {1}, {2} {3}",
                               ts.Days, ts.Days == 1 ? Language.Time_Day : Language.Time_Days,
                               ts.Hours, ts.Hours == 1 ? Language.Time_Hour : Language.Time_Hours );
      else if( ts.Hours > 0 )
        return String.Format( "{0} {1}, {2} {3}",
                              ts.Hours, ts.Hours == 1 ? Language.Time_Hour : Language.Time_Hours,
                              ts.Minutes, ts.Minutes == 1 ? Language.Time_Min : Language.Time_Mins );
      else
        return String.Format( "{0} {1}",
                              ts.Minutes, ts.Minutes == 1 ? Language.Time_Min : Language.Time_Mins );
    }

    /// <summary>
    /// Generate a string based on how long until a date is.
    /// </summary>
    /// <param name="date">The DateTime in the past to calculate mins until.</param>
    /// <returns>A string in the format "XdXhXm" or "XhXm".</returns>
    public static string MinsUntilShort( DateTime date )
    {
      TimeSpan ts = date - DateTime.Now.AddMinutes( -1 );  // add 1 min to account for rounding down to nearest minute

      if( ts.TotalSeconds <= 0 )
        return String.Format( "0{0}0{1}", Language.Time_HoursLetter, Language.Time_MinsLetter );
      else if( ts.Days > 0 )
        return String.Format( "{0}{1}{2}{3}{4}{5}", ts.Days, Language.Time_DaysLetter, ts.Hours, Language.Time_HoursLetter, ts.Minutes, Language.Time_MinsLetter );
      else if( ts.Hours > 0 )
        return String.Format( "{0}{1}{2}{3}", ts.Hours, Language.Time_HoursLetter, ts.Minutes, Language.Time_MinsLetter );
      else
        return String.Format( "{0} {1}", ts.Minutes, ts.Minutes == 1 ? Language.Time_Min : Language.Time_Mins );
    }

    /// <summary>
    /// Generate a string based on how long until a date is.
    /// </summary>
    /// <param name="date">The DateTime in the past to calculate mins until.</param>
    /// <returns>A string in the format "x days, x hours, x mins" or "x hours, x mins" or "x mins".</returns>
    public static string MinsUntilLong( DateTime date )
    {
      TimeSpan ts = date - DateTime.Now.AddMinutes( -1 );  // add 1 min to account for rounding down to nearest minute

      if( ts.TotalSeconds <= 0 )
        return "0 " + Language.Time_Mins;
      else if( ts.Days > 0 )
        return String.Format( "{0} {1}, {2} {3}",
                               ts.Days, ts.Days == 1 ? Language.Time_Day : Language.Time_Days,
                               ts.Hours, ts.Hours == 1 ? Language.Time_Hour : Language.Time_Hours );
      else if( ts.Hours > 0 )
        return String.Format( "{0} {1}, {2} {3}",
                              ts.Hours, ts.Hours == 1 ? Language.Time_Hour : Language.Time_Hours,
                              ts.Minutes, ts.Minutes == 1 ? Language.Time_Min : Language.Time_Mins );
      else
        return String.Format( "{0} {1}",
                              ts.Minutes, ts.Minutes == 1 ? Language.Time_Min : Language.Time_Mins );
    }

    #endregion

    #region Math/Trig

    /// <summary>
    /// Calculate the distance between two 2D points.
    /// </summary>
    /// <param name="pt1">The first coordinate.</param>
    /// <param name="pt2">The second coordinate.</param>
    /// <returns>The distance in a straight line.</returns>
    public static double DistanceBetween( Point pt1, Point pt2 )
    {
      // pythagoras: (x1,y1) - (x2,y2)  =  ( (x2-x1)^2 + (y2-y1)^2 )^1/2

      return Math.Sqrt( Math.Pow( pt2.X - pt1.X, 2 ) + Math.Pow( pt2.Y - pt1.Y, 2 ) );
    }

    /// <summary>
    /// Returns the angle of the vector (pt1,pt2) relative to 12oclock.
    /// </summary>
    /// <remarks>N = 0, E = 90, S = 180, W = 270</remarks>
    /// <param name="pt1">The first (center) point.</param>
    /// <param name="pt2">The second (outer) point.</param>
    /// <returns>The angle in degrees.</returns>
    public static double AngleBetween( PointF pt1, PointF pt2 )
    {
      double radians = Math.Atan2( pt2.Y - pt1.Y, pt2.X - pt1.X );
      double degrees = radians * ( 180 / Math.PI ) + 90;
      if( degrees < 0 ) degrees += 360;
      return degrees;
    }

    /// <summary>
    /// Returns the angle made by the three points.
    /// </summary>
    /// <param name="pt1">The first point.</param>
    /// <param name="pt2">The middle point.</param>
    /// <param name="pt3">The last point.</param>
    /// <returns>The angle in degrees.</returns>
    public static double AngleBetween( Point pt1, Point pt2, Point pt3 )
    {
      double ABx = pt1.X - pt2.X;
      double ABy = pt1.Y - pt2.Y;
      double BCx = pt3.X - pt2.X;
      double BCy = pt3.Y - pt2.Y;

      double dotProduct = ( ABx * BCx ) + ( ABy * BCy );
      double crossProduct = ( ABx * BCy ) - ( ABy * BCx );

      double radians = Math.Atan2( crossProduct, dotProduct );
      double degrees = radians * ( 180 / Math.PI );
      if( degrees < 0 ) degrees += 360;
      return degrees;
    }

    /// <summary>
    /// Calculate a new Point given a starting point, and an angle &amp; distance to travel.
    /// </summary>
    /// <param name="start">The starting point.</param>
    /// <param name="angle">The angle to travel (in degrees relative to 12oclock).</param>
    /// <param name="distance">The distance to travel.</param>
    /// <returns>The destination Point.</returns>
    public static Point AngleOffset( Point start, double angle, double distance )
    {
      PointF pt = AngleOffset( new PointF( start.X, start.Y ), angle, distance );
      return new Point( (int)pt.X, (int)pt.Y );
    }

    /// <summary>
    /// Calculate a new Point given a starting point, and an angle &amp; distance to travel.
    /// </summary>
    /// <param name="start">The starting point.</param>
    /// <param name="angle">The angle to travel (in degrees relative to 12oclock).</param>
    /// <param name="distance">The distance to travel.</param>
    /// <returns>The destination Point.</returns>
    public static PointF AngleOffset( PointF start, double angle, double distance )
    {
      double x = start.X + Math.Sin( -angle * ( Math.PI / 180 ) ) * distance;
      double y = start.Y + Math.Cos( -angle * ( Math.PI / 180 ) ) * distance;
      return new PointF( (float)x, (float)y );
    }

    /// <summary>
    /// Returns a point between the given two Points.
    /// </summary>
    /// <param name="pt1">The first point.</param>
    /// <param name="pt2">The second point.</param>
    /// <returns>The 50% midpoint.</returns>
    public static Point MidPoint( Point pt1, Point pt2 )
    {
      return new Point( (pt1.X + pt2.X) / 2, (pt1.Y + pt2.Y) / 2 );
    }

    /// <summary>
    /// Returns a point between the given two Points.
    /// </summary>
    /// <param name="pt1">The first point.</param>
    /// <param name="pt2">The second point.</param>
    /// <param name="percent">The percent distance between the two points.</param>
    /// <returns>The midpoint according to percent.</returns>
    public static PointF MidPoint( PointF pt1, PointF pt2, float percent )
    {
      float x = pt1.X + ( ( pt2.X - pt1.X ) * percent );
      float y = pt1.Y + ( ( pt2.Y - pt1.Y ) * percent );

      return new PointF( x, y );
    }

    /// <summary>
    /// Tests whether two finite line segments intersect.
    /// </summary>
    /// <param name="ptLine1a">The start point of the first line.</param>
    /// <param name="ptLine1b">The end point of the first line.</param>
    /// <param name="ptLine2a">The start point of the second line.</param>
    /// <param name="ptLine2b">The end point of the second line.</param>
    /// <returns>True if the lines touch or cross.</returns>
    public static bool LineIntersect( Point ptLine1a, Point ptLine1b, Point ptLine2a, Point ptLine2b )
    {
      if( ptLine1a == ptLine2a || ptLine1a == ptLine2b || ptLine1b == ptLine2a || ptLine1b == ptLine2b )
        return false;  // lines originate from same point

      if( TriClockDir( ptLine1a, ptLine1b, ptLine2a ) != TriClockDir( ptLine1a, ptLine1b, ptLine2b ) )
        if( TriClockDir( ptLine2a, ptLine2b, ptLine1a ) != TriClockDir( ptLine2a, ptLine2b, ptLine1b ) )
          return true;

      return false;
    }
    private static int TriClockDir( PointF pt1, PointF pt2, PointF pt3 )
    {
      float test = ( ( ( pt2.X - pt1.X ) * ( pt3.Y - pt1.Y ) ) - ( ( pt3.X - pt1.X ) * ( pt2.Y - pt1.Y ) ) );

      if     ( test > 0 ) return  1;  // counter clockwise
      else if( test < 0 ) return -1;  // clockwise
      else                return  0;  // line
    }

    /// <summary>
    /// Tests whether a Point lies within an irregular polygon.
    /// </summary>
    /// <param name="ptTest">The point to test.</param>
    /// <param name="poly">A list of Points that make up the polygon.</param>
    /// <returns>True if the point is inside.</returns>
    public static bool IsPointWithinPoly( Point ptTest, Point[] poly )
    {
      // do Jordan curve theorem

      bool inside = false;

      Point ptStart, ptEnd;
      for( int i = 0, j = poly.Length - 1; i < poly.Length; j = i++ )
      {
        ptStart = poly[j];
        ptEnd = poly[i];

        if( ( ( ( ptEnd.Y <= ptTest.Y ) && ( ptTest.Y < ptStart.Y ) ) || ( ( ptStart.Y <= ptTest.Y ) && ( ptTest.Y < ptEnd.Y ) ) )
            && ( ptTest.X < ( ptStart.X - ptEnd.X ) * ( ptTest.Y - ptEnd.Y ) / (float)( ptStart.Y - ptEnd.Y ) + ptEnd.X ) )
          inside = !inside;
      }

      return inside;
    }

    #endregion

    #region String

    /// <summary>
    /// Custom stringification of a Version object.
    /// </summary>
    /// <param name="version">The reference Version</param>
    /// <returns>eg, "v1.2" (no trailing zeros)</returns>
    public static string VersionToString( Version version )
    {
      return "v" + Regex.Replace( version.ToString(), @"(\.0){1,2}$", String.Empty );
    }


    /// <summary>
    /// Generic method to parse a string into a supported type.
    /// </summary>
    /// <typeparam name="T">The type to parse to.</typeparam>
    /// <param name="value">The string to parse.</param>
    /// <returns>A value in the specific type, or an exception.</returns>
    public static T ParseToType<T>( string value )
    {
      Type type = typeof( T );

      switch( type.Name )
      {
        case "Int32":
          if( value.Contains( "/" ) )  // own="4/2"  =>  "4"
            value = value.Substring( 0, value.IndexOf( "/" ) );
          return (T)(object)int.Parse( value );
        case "DateTime":
          return (T)(object)Misc.ParseTimestamp( value );
        case "String":
          return (T)(object)value;
        case "Boolean":
          return (T)(object)Misc.ParseBool( value );
        case "ServerState":
          return (T)Enum.Parse( typeof( ServerState ), Misc.SubstringBefore( value, ":" ) );
        case "ServerPopulation":
          return (T)Enum.Parse( typeof( ServerPopulation ), value.Replace( " ", "" ) );
        default:
          throw new Exception( String.Format( "Unsupported type '{0}'", type ) );
      }
    }

    /// <summary>
    /// Parse a boolean string using perl/tcl-ish boolean rules.
    /// </summary>
    /// <param name="value">The value to parse.</param>
    /// <returns>True/False/Exception</returns>
    public static bool ParseBool( string value )
    {
      if( value == null ) return false;

      switch( value.Trim().ToLower() )
      {
        case "y":
        case "yes":
        case "t":
        case "true":
        case "1":
        case "on":
          return true;
        case "":
        case "n":
        case "no":
        case "f":
        case "false":
        case "0":
        case "off":
          return false;
        default:
          return bool.Parse( value );
      }
    }

    /// <summary>
    /// Parse a '-123x456' string into a Point.
    /// </summary>
    /// <param name="value">The value to parse.</param>
    /// <returns>A new Point, or Exception</returns>
    public static Point ParsePoint( string value )
    {
      Match match = Regex.Match( value, @"^(-?\d+)x(-?\d+)$" );
      if( !match.Success )
        throw new Exception( "invalid format" );

      int x = int.Parse( match.Groups[1].Value );
      int y = int.Parse( match.Groups[2].Value );

      return new Point( x, y );
    }

    /// <summary>
    /// Get the substring before a separator character.
    /// </summary>
    /// <param name="value">The string to parse.</param>
    /// <param name="separator">The split char or string.</param>
    /// <returns>The string before the specified separator, or the original string if it is not present.</returns>
    public static string SubstringBefore( string value, string separator )
    {
      if( value == null )
        return null;
      if( !value.Contains( separator ) )
        return value;

      return value.Substring( 0, value.IndexOf( separator ) );
    }

    /// <summary>
    /// Get the substring after a separator character.
    /// </summary>
    /// <param name="value">The string to parse.</param>
    /// <param name="separator">The split char or string.</param>
    /// <returns>The string after the specified separator, or null if it is not present.</returns>
    public static string SubstringAfter( string value, string separator )
    {
      if( value == null )
        return null;
      if( !value.Contains( separator ) )
        return null;

      int i = value.IndexOf( separator );
      int length = value.Length - i - 1;
      if( length == 0 )  // seperator at end
        return null;

      return value.Substring( i + 1, length );
    }

    #endregion

    #region Enum

    /// <summary>
    /// Gets all flags contained in the specified Enum value.
    /// </summary>
    /// <typeparam name="T">The Enum type (must have [Flags] attribute).</typeparam>
    /// <param name="value">The Enum value (may include several bitwise-or flags).</param>
    /// <returns>A lazy collection of individual Enum flag values.</returns>
    public static IEnumerable<T> GetFlags<T>( Enum value )
    {
      int valueAsInt = Convert.ToInt32( value, CultureInfo.InvariantCulture );
      foreach( object item in Enum.GetValues( typeof( T ) ) )
      {
        int itemAsInt = Convert.ToInt32( item, CultureInfo.InvariantCulture );
        if( itemAsInt == ( valueAsInt & itemAsInt ) )
          yield return (T)item;
      }
    } 

    /// <summary>
    /// Get the localised name of the enum value.
    /// </summary>
    /// <param name="e">The enum value.</param>
    /// <returns>A localised string for the given value.</returns>
    public static string EnumString( Enum e )
    {
      return Language.ResourceManager.GetString( String.Format( "Enum_{0}_{1}", e.GetType().Name, e ) );
    }

    /// <summary>
    /// Get the localised name of the Side value, with MString support.
    /// </summary>
    /// <param name="e">The enum value.</param>
    /// <returns>A localised string for the given value.</returns>
    public static MString EnumString( Side e )
    {
      return new MString( EnumString( (Enum)e ) );
    }

    #endregion

    #region Sides

    /// <summary>
    /// Get the opposite side.
    /// </summary>
    /// <param name="side">A side.</param>
    /// <returns>Axis or Allied.</returns>
    public static Side OtherSide( Side side )
    {
      if( side == Side.Allied )
        return Side.Axis;
      else if( side == Side.Axis )
        return Side.Allied;
      else
        throw new ApplicationException( "Not active side" );
    }

    /// <summary>
    /// Gets whether the two countries are on friendly terms.
    /// </summary>
    public static bool AreFriendly( Country a, Country b )
    {
      if( a == b || a.Side == b.Side ) return true;                  // same countries/sides are always friendly
      if( a.Side == Side.None || b.Side == Side.None ) return true;  // one side inactive: always friendly
      if( a.Abbr == "NF" || b.Abbr == "NF" ) return true;            // one side Neutral Friendly: always friendly
      if( a.Abbr == "NH" || b.Abbr == "NH" ) return false;           // one side Netural Hostile: always enemy
      
      return a.Side == b.Side;
    }

    /// <summary>
    /// Gets whether the two countries are enemies.
    /// </summary>
    public static bool AreEnemy( Country a, Country b )
    {
      if( a == b || a.Side == b.Side ) return false;                  // same countries/sides are always friendly
      if( a.Side == Side.None || b.Side == Side.None ) return false;  // one side inactive: always friendly
      if( a.Abbr == "NF" || b.Abbr == "NF" ) return false;            // one side Neutral Friendly: always friendly
      if( a.Abbr == "NH" || b.Abbr == "NH" ) return true;             // one side Netural Hostile: always enemy

      return a.Side != b.Side;
    }

    /// <summary>
    /// Gets whether a country is friendly to a side.
    /// </summary>
    public static bool AreFriendly( Side a, Country b )
    {
      if( a == b.Side ) return true;                            // same sides are always friendly
      if( a == Side.None || b.Side == Side.None ) return true;  // one side inactive: always friendly
      if( b.Abbr == "NF" ) return true;                         // one side Neutral Friendly: always friendly
      if( b.Abbr == "NH" ) return false;                        // one side Netural Hostile: always enemy

      return a == b.Side;
    }

    /// <summary>
    /// Gets whether a country is enemy of a side.
    /// </summary>
    public static bool AreEnemy( Side a, Country b )
    {
      if( a == b.Side ) return false;                            // same sides are always friendly
      if( a == Side.None || b.Side == Side.None ) return false;  // one side inactive: always friendly
      if( b.Abbr == "NF" ) return false;                         // one side Neutral Friendly: always friendly
      if( b.Abbr == "NH" ) return true;                          // one side Netural Hostile: always enemy

      return a != b.Side;
    }

    #endregion

    #region Classes

    /// <summary>
    /// As some game events are generated when data changes (eg, AO's), their event time
    /// is based on the users local clock (DateTime.Now). Because most other events are
    /// based on the times sent from Wiretap, this introduces clock skew issues if the two
    /// are out of sync.
    /// We work around this by calculating an approximate offset, accurate to near 1 minute,
    /// and apply this to Wiretaps time to convert everything to the users view of the
    /// time.
    /// This is done based on the fact that, when polling for new captures every minute,
    /// any new capture times must have occurred in the past minute. We narrow this down
    /// over time to get the latest capture time in that 1 minute time window.
    /// </summary>
    public static class ClockSkew
    {
      #region Variables

      private static int dataPointCount;
      public static int CurrentSkewSeconds { get; set; }

      #endregion

      #region Methods

      /// <summary>
      /// Reset internal data and start calculations again from the beginning.
      /// </summary>
      public static void Reset()
      {
        dataPointCount = 0;
        CurrentSkewSeconds = 0;
      }

      /// <summary>
      /// Add a reference date/time data point to further refine the clock skew calculation.
      /// </summary>
      /// <param name="date">A UTC timestamp from Wiretap that is known to be less than a minute old.</param>
      /// <returns>True if clock skew has been updated.</returns>
      public static bool AddDataPoint( DateTime date )
      {
        bool updated = false;
        if( dataPointCount >= 20 ) return false;  // got enough


        // get skew

        int newSkewSeconds = (int)( DateTime.Now - date ).TotalSeconds;
        // >0 == local clock ahead of wiretap

        if( Math.Abs( newSkewSeconds ) > 60 * 15 )  // over 15 mins out
        {
          Log.AddError( "Excessive clock skew detected ({0}{1})", newSkewSeconds > 0 ? "+" : null, newSkewSeconds );
          MessageBox.Show( String.Format( Language.Error_ClockSkew,
                                          ( newSkewSeconds > 0 ? "+" : null ) + new TimeSpan( 0, 0, newSkewSeconds ),
                                          DateTime.Now,
                                          date ),
                           "Xiperware", MessageBoxButtons.OK, MessageBoxIcon.Warning );
          dataPointCount = 20;  // stop correcting
          return false;
        }


        // update skew

        int diff = newSkewSeconds - CurrentSkewSeconds;
        if( ( dataPointCount == 0 && diff != 0 ) || newSkewSeconds < CurrentSkewSeconds )
        {
          Log.AddEntry( "Clock skew correction: {0}{1}secs", diff > 0 ? "+" : null, diff );

          CurrentSkewSeconds = newSkewSeconds;
          updated = true;
        }


        // only process the first 20 data points

        dataPointCount++;

        if( dataPointCount == 20 )
          Log.AddEntry( "Calculated {0}{1} second clock skew from wiretap server",
                        CurrentSkewSeconds > 0 ? "+" : null, CurrentSkewSeconds );


        return updated;
      }

      #endregion
    }


    /// <summary>
    /// A FormatProvider that formats a decimal degrees value into degrees, minutes, seconds.
    /// Format should be X for longitude (E/W) or Y for latitude (N/S).
    /// </summary>
    public class LatLongFormatter : IFormatProvider, ICustomFormatter
    {
      #region IFormatProvider Members

      public object GetFormat( Type formatType )
      {
        if( formatType == typeof( ICustomFormatter ) )
          return this;
        else
          return null;
      }

      #endregion

      #region ICustomFormatter Members

      public string Format( string format, object arg, IFormatProvider formatProvider )
      {
        double degrees = (double)arg;

        string sign;

        if( degrees > 0 )
        {
          sign = format == "X" ? "E" : "N";
        }
        else  // < 0
        {
          sign = format == "X" ? "W" : "S";
          degrees = Math.Abs( degrees );
        }

        double deg = Math.Floor( degrees );
        degrees = ( degrees - deg ) * 60;
        double min = Math.Floor( degrees );
        degrees = ( degrees - min ) * 60;
        double sec = Math.Floor( degrees );

        return String.Format( "{0}°{1:00}'{2:00}\"{3}", deg, min, sec, sign );
      }

      #endregion
    }


    /// <summary>
    /// A collection of strings with custom format string support.
    /// </summary>
    /// <remarks>
    /// Sometimes when translating there can be multiple forms of the same word, which
    /// are treated differently between languages. This class provides a way for several
    /// versions to be supplied. One of these versions can then be specified via the
    /// format string:
    /// 
    /// MString demonym = new MString( "german,germans" );
    /// String.Format( "... has been captured {0:v1}.", demonym );
    /// String.Format( "... has been captured by the {0:v2}.", demonym );
    /// </remarks>
    public struct MString : IFormattable
    {
      #region Variables

      private readonly string[] values;

      #endregion

      #region Constructors

      /// <summary>
      /// Create a new MString list.
      /// </summary>
      /// <param name="values">A comma sperated list of values (no spaces).</param>
      public MString( string values )
      {
        if( values == null )
          this.values = new[] { "" };
        else
          this.values = values.Split( ',' );
      }

      #endregion

      #region Methods

      /// <summary>
      /// Indexer to get a specific value from the string collection.
      /// </summary>
      /// <param name="num">The version to get (first index = [1])</param>
      /// <returns>The specified value if it exists, otherwise the first.</returns>
      public string this[int num]
      {
        get
        {
          int i = num - 1;  // 1-indexed => 0-indexed

          if( i >= 0 && i < this.values.Length )
            return this.values[i];
          else
            return this.values[0];
        }
      }

      /// <summary>
      /// Support for implicit conversion to String.
      /// </summary>
      /// <returns>The default value.</returns>
      public static implicit operator string( MString mstring )
      {
        return mstring.ToString();
      }

      #endregion

      #region IFormattable Members

      /// <summary>
      /// Provides a string representation of this object.
      /// </summary>
      /// <returns>The default value.</returns>
      public override string ToString()
      {
        return this.values[0];
      }

      /// <summary>
      /// Provides a string representation of this object.
      /// </summary>
      /// <returns>The value specified by the given format.</returns>
      public string ToString( string format, IFormatProvider formatProvider )
      {
        if( format != null && format.StartsWith( "v" ) )
        {
          int num = int.Parse( format.Substring( 1 ) );
          return this[num];
        }

        return this.values[0];
      }

      #endregion
    }

    #endregion
  }

  /// <summary>
  /// A collection of extension methods.
  /// </summary>
  public static class Etensions
  {
    /// <summary>
    /// Filters a sequence of values to exclude nulls.
    /// </summary>
    public static IEnumerable<TSource> NotNull<TSource>( this IEnumerable<TSource> source ) where TSource : class 
    {
      return source.Where( item => item != null );
    }
  }

  #region Data Structures

  /// <summary>
  /// A generic two-dimensional hash with symmetrical keys (eg, hash[1,2] == hash[2,1]).
  /// </summary>
  /// <typeparam name="TKey">The type of both keys.</typeparam>
  /// <typeparam name="TValue">The type of the value.</typeparam>
  public class SymmetricHash<TKey, TValue> : IEnumerable<TValue>
    where TKey : IComparable<TKey>
  {
    private readonly Dictionary<TKey, Dictionary<TKey, TValue>> hash;

    /// <summary>
    /// Create a new SymmetricHash.
    /// </summary>
    public SymmetricHash()
    {
      this.hash = new Dictionary<TKey, Dictionary<TKey, TValue>>();
    }

    /// <summary>
    /// Add or replace an element in the hash.
    /// </summary>
    /// <param name="key1">The first key.</param>
    /// <param name="key2">The second key.</param>
    /// <param name="value">The value to store.</param>
    public void Add( TKey key1, TKey key2, TValue value )
    {
      TKey keyLower, keyUpper;
      if( key1.CompareTo( key2 ) < 0 ) { keyLower = key1; keyUpper = key2; }
      else                             { keyLower = key2; keyUpper = key1; }

      if( !this.hash.ContainsKey( keyLower ) )
        this.hash[keyLower] = new Dictionary<TKey, TValue>();

      this.hash[keyLower][keyUpper] = value;
    }

    /// <summary>
    /// Gets an element in the hash.
    /// </summary>
    /// <param name="key1">The first key.</param>
    /// <param name="key2">The second key.</param>
    /// <returns>The stored value, or a IndexOutOfRangeException.</returns>
    public TValue Get( TKey key1, TKey key2 )
    {
      TKey keyLower, keyUpper;
      if( key1.CompareTo( key2 ) < 0 ) { keyLower = key1; keyUpper = key2; }
      else                             { keyLower = key2; keyUpper = key1; }

      if( !this.hash.ContainsKey( keyLower ) )
        throw new IndexOutOfRangeException( "first key not found" );

      if( !this.hash[keyLower].ContainsKey( keyUpper ) )
        throw new IndexOutOfRangeException( "second key not found" );

      return this.hash[keyLower][keyUpper];
    }

    /// <summary>
    /// Clear all elements from the hash.
    /// </summary>
    public void Clear()
    {
      foreach( Dictionary<TKey, TValue> innerHash in this.hash.Values )
        innerHash.Clear();

      this.hash.Clear();
    }

    /// <summary>
    /// Gets or adds an element in the hash.
    /// </summary>
    /// <param name="key1">The first key.</param>
    /// <param name="key2">The second key.</param>
    /// <returns>The stored value, or a IndexOutOfRangeException.</returns>
    public TValue this[TKey key1, TKey key2]
    {
      get { return Get( key1, key2 ); }
      set { Add( key1, key2, value ); }
    }

    /// <summary>
    /// Returns an enumerator that iterates through all values in the hash.
    /// </summary>
    /// <returns>
    /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
    /// </returns>
    public IEnumerator<TValue> GetEnumerator()
    {
      return hash.Values.SelectMany( innerHash => innerHash.Values ).GetEnumerator();
    }

    /// <summary>
    /// Returns an enumerator that iterates through all values in the hash.
    /// </summary>
    /// <returns>
    /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
    /// </returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }
  }

  #endregion


  /// <summary>
  /// An object that supports notifying when a property has changed.
  /// </summary>
  public abstract class NotifyObject : INotifyPropertyChanged
  {
    #region Debugging Aides

    /// <summary>
    /// Warns the developer if this object does not have
    /// a public property with the specified name. This 
    /// method does not exist in a Release build.
    /// </summary>
    [Conditional( "DEBUG" )]
    [DebuggerStepThrough]
    public void VerifyPropertyName( string propertyName )
    {
      // Verify that the property name matches a real,  
      // public, instance property on this object.
      if( TypeDescriptor.GetProperties( this )[propertyName] == null )
      {
        string msg = "Invalid property name: " + propertyName;

        if( this.ThrowOnInvalidPropertyName )
          throw new Exception( msg );
        else
          Debug.Fail( msg );
      }
    }

    /// <summary>
    /// Returns whether an exception is thrown, or if a Debug.Fail() is used
    /// when an invalid property name is passed to the VerifyPropertyName method.
    /// The default value is false, but subclasses used by unit tests might 
    /// override this property's getter to return true.
    /// </summary>
    protected virtual bool ThrowOnInvalidPropertyName { get; private set; }

    #endregion // Debugging Aides

    #region INotifyPropertyChanged Members

    /// <summary>
    /// Raised when a property on this object has a new value.
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// Raises this object's PropertyChanged event.
    /// </summary>
    /// <param name="propertyName">The property that has a new value.</param>
    protected virtual void OnPropertyChanged( string propertyName )
    {
      this.VerifyPropertyName( propertyName );

      PropertyChangedEventHandler handler = this.PropertyChanged;
      if( handler != null )
      {
        var e = new PropertyChangedEventArgs( propertyName );
        handler( this, e );
      }
    }

    #endregion // INotifyPropertyChanged Members
  }

}
