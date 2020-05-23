using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;

namespace XLib.Extensions
{
  public static class StringExt
  {
    /// <summary>
    /// Join a list of items into a delimited string.
    /// </summary>
    /// <param name="values">The list of items.</param>
    /// <param name="sep">The item separator to use (default ", ").</param>
    /// <returns>The concatenated string.</returns>
    public static string Join<T>( this IEnumerable<T> values, string sep = ", " )
    {
      return String.Join( sep, values );
    }

    /// <summary>
    /// Convert a delimited string into a list.
    /// </summary>
    /// <typeparam name="T">The type of list to return.</typeparam>
    /// <param name="input">The delimited string.</param>
    /// <param name="sep">The separator string or regex pattern.</param>
    /// <returns>A list of T.</returns>
    public static List<T> SplitRegex<T>( this string input, string sep = @"\s*,\s*" )
    {
      List<T> list = new List<T>();

      if( input == String.Empty )
        return list;

      foreach( String value in Regex.Split( input, sep ) )
        list.Add( value.ParseTo<T>() );

      return list;
    }

    /// <summary>
    /// Generic method to parse a string into a common type.
    /// </summary>
    /// <typeparam name="T">The type to parse to.</typeparam>
    /// <param name="value">The string to parse.</param>
    /// <returns>A value in the specific type, or an exception.</returns>
    public static T ParseTo<T>( this string value )
    {
      Type type = typeof( T );

      switch( type.Name )
      {
        case "String":
          return (T)(object)value;
        case "Int32":
          return (T)(object)Int32.Parse( value );
        case "Single":
          return (T)(object)Single.Parse( value );
        case "Double":
          return (T)(object)Double.Parse( value );
        case "DateTime":
          return (T)(object)DateTime.Parse( value );
        case "Boolean":
          return (T)(object)Boolean.Parse( value );
        default:
          throw new Exception( String.Format( "Unsupported type '{0}'.", type ) );
      }
    }

    /// <summary>
    /// Modifies a string in the format "there are 123 item(s) in the list" to have the correct plurality.
    /// </summary>
    /// <param name="text">The text to process.</param>
    /// <returns>The same text with 's' added or removed.</returns>
    public static string Pluralise( this string text )
    {
      return Regex.Replace( text, @"(\d+)(\D+?)(\(([a-z]{0,2}s)\))", match => match.Result( match.Groups[1].Value == "1" ? "$1$2" : "$1$2$4" ) );
    }

    /// <summary>
    /// Convert a string to title case.
    /// </summary>
    public static string ToTitleCase( this string text, bool toLowerFirst = false )
    {
      return System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase( toLowerFirst ? text.ToLower() : text );
    }
  }
}
