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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using Xiperware.WiretapAPI.Properties;
using XLib.Extensions;
using XLib.ThirdParty;

namespace Xiperware.WiretapAPI
{
  #region Constants

  /// <summary>
  /// A class for storing constant game settings.
  /// </summary>
  public static class WW2
  {
    /// <summary>
    /// The time in minutes a hcunit must remain deployed after moving to a frontline cp.
    /// </summary>
    public const int BRIGADE_MOVEMENT_DELAY_FRONTLINE = 60;

    /// <summary>
    /// The time in minutes a hcunit must remain deployed after moving to a non frontline cp.
    /// </summary>
    public const int BRIGADE_MOVEMENT_DELAY_NONFRONTLINE = 30;

    /// <summary>
    /// The time in minutes a hcunit must spend in training after being routed before
    /// being able to be deployed on map again.
    /// </summary>
    public const int BRIGADE_ROUTED_DELAY = 12 * 60;
  }

  #endregion

  #region Enums

  /// <summary>
  /// The two opposing sides in game.
  /// </summary>
  /// <remarks>Any inactive countries are Side.None.</remarks>
  public enum Side
  {
    None    = 0,
    Allied  = 1,
    Axis    = 2,
    Neutral = 3,
  }

  /// <summary>
  /// The three military branches.
  /// </summary>
  public enum Branch
  {
    None     = 0,
    Army     = 1,
    Airforce = 2,
    Navy     = 3
  }

  /// <summary>
  /// The way a HCUnit is allowed to move.
  /// </summary>
  public enum MoveType
  {
    None = 0,
    Land = 1,
    Air  = 2,
    Sea  = 3
  }

  /// <summary>
  /// The types of ChokePoints used internally by CRS.
  /// </summary>
  /// <remarks>The only type used is Bridge, to separate them from ChokePoints.</remarks>
  public enum ChokePointType
  {
    Undefined = 0,  // unused
    Generic   = 1,  // most cps
    Small     = 2,  // x1 (givet)
    Medium    = 3,  // x7
    Large     = 4,  // unused
    Bridge    = 5,  // will become a Bridge, not a ChokePoint
    Dummy     = -1  // used for frontline generation
  }

  /// <summary>
  /// The types of Facilitys used internally by CRS.
  /// </summary>
  public enum FacilityType
  {
    Undefined = 0,  // unused
    City      = 1,  // inc railway stations (plus 3 x "AI Testing Facility")
    Factory   = 2,  // production facilities, plants/refinery/factory/brewery/etc
    Flak      = 3,  // x3
    Depot     = 4,
    OMT       = 5,  // unused
    Training  = 6,  // x1, "Offline training facility"
    Firebase  = 7,  // will become a Firebase, not a Facility
    Airbase   = 8,
    Armybase  = 9,  // (plus 3 x "Training Grounds", 3 x "Army Training Grounds")
    Navalbase = 10
  }

  /// <summary>
  /// The five levels of the HC hierarchy.
  /// </summary>
  public enum HCUnitLevel
  {
    Top      = 2,
    Branch   = 3,
    Corps    = 4,
    Division = 5,
    Brigade  = 6
  }

  /// <summary>
  /// The possible states of each firebase pair.
  /// </summary>
  public enum FirebaseState
  {
    /// <summary>
    /// Chokepoints have same owner/controller.
    /// </summary>
    Inactive,
    /// <summary>
    /// Chokepoints have different owning sides.
    /// </summary>
    Offensive,
    /// <summary>
    /// Chokepoints have different controlling sides.
    /// </summary>
    Defensive,
    /// <summary>
    /// Chokepoints have same owner/controller, one is frontline with no deployed
    /// hcunits, the other with an army brigade.
    /// </summary>
    Brigade
  }

  /// <summary>
  /// The abstracted levels of player population on a server.
  /// </summary>
  public enum ServerPopulation
  {
    Unknown   = -1,
    Empty     =  0,
    VeryLight =  1,
    Low       =  2,
    Average   =  3,
    Good      =  4,
    High      =  5,
    VeryHigh  =  6
  }

  /// <summary>
  /// The states a game server can be in.
  /// </summary>
  public enum ServerState
  {
    Unknown,
    Offline,
    Starting,
    Syncing,
    Online,
    Locked,
    Closed
  }

  /// <summary>
  /// The level of player activity around a chokepoint.
  /// </summary>
  public enum ActivityLevel
  {
    None     = 0,
    Low      = 1,
    Light    = 2,
    Moderate = 3,
    Heavy    = 4
  }

  /// <summary>
  /// The states a hcunit move request can be in.
  /// </summary>
  public enum HCUnitMoveState
  {
    /// <summary>
    /// Unknown state.
    /// </summary>
    Unknown,
    /// <summary>
    /// Request made and failed.
    /// </summary>
    RequestFailed,
    /// <summary>
    /// Request made and succeeded, waiting for completion attempt.
    /// </summary>
    Pending,
    /// <summary>
    /// Request successful, completion attempt failed.
    /// </summary>
    CompletionFailed,
    /// <summary>
    /// Request successful, completion successful.
    /// </summary>
    Success,
    /// <summary>
    /// Request successful, but another request made before completion attempted.
    /// </summary>
    Cancelled
  }

  /// <summary>
  /// Vehicle access level.
  /// </summary>
  public enum AccessLevel
  {
    Public  = 0,
    Group1  = 1,
    Group2  = 2,
    Group3  = 3,
    Group4  = 4,
    Group5  = 5,
    Group6  = 6,
    Group7  = 7,
    Buzzard = 8,
    Skull   = 9
  }

  #endregion

  #region Types

  /// <summary>
  /// A four-integer identifier.
  /// </summary>
  public class QuadID : Tuple<int,int,int,int>
  {
    #region Constructors

    /// <summary>
    /// Create a new QuadID with the given values.
    /// </summary>
    public QuadID( int item1, int item2, int item3, int item4 )
      : base( item1, item2, item3, item4 )
    {

    }

    /// <summary>
    /// Create a new empty QuadID
    /// </summary>
    public static QuadID FromString( string str )
    {
      Match match = Regex.Match( str, @"^(\d+)\.(\d+)\.(\d+)\.(\d+)$" );
      if( !match.Success )
        throw new ApplicationException( "not a valid QuadID string" );

      int i1 = int.Parse( match.Groups[1].Value );
      int i2 = int.Parse( match.Groups[2].Value );
      int i3 = int.Parse( match.Groups[3].Value );
      int i4 = int.Parse( match.Groups[4].Value );
      return new QuadID( i1, i2, i3, i4 );
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Provides a string representation of this object.
    /// </summary>
    /// <returns>eg, 1.2.3.4</returns>
    public override string ToString()
    {
      return String.Format( "{0}.{1}.{2}.{3}", this.Item1, this.Item2, this.Item3, this.Item4 );
    }

    /// <summary>
    /// Returns a value indicating whether this instance is equal to a specified object value.
    /// </summary>
    /// <param name="obj">An object to compare with this instance.</param>
    /// <returns>True if obj is the same Country.</returns>
    public override bool Equals( object obj )
    {
      if( !( obj is QuadID ) ) return false;
      QuadID other = (QuadID)obj;

      if( this.Item1 != other.Item1 ) return false;
      if( this.Item2 != other.Item2 ) return false;
      if( this.Item3 != other.Item3 ) return false;
      if( this.Item4 != other.Item4 ) return false;

      return true;
    }

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    public override int GetHashCode()
    {
      return base.GetHashCode();
    }

    /// <summary>
    /// Support for the == operator.
    /// </summary>
    public static bool operator ==( QuadID a, QuadID b )
    {
      if( (object)a == null ) return (object)b == null;
      return a.Equals( b );
    }

    /// <summary>
    /// Support for the != operator.
    /// </summary>
    public static bool operator !=( QuadID a, QuadID b )
    {
      if( (object)a == null ) return (object)b != null;
      return !a.Equals( b );
    }

    #endregion
  }

  #endregion

  #region Structs

  /// <summary>
  /// A structure to store information about a single, per-factory, tick.
  /// </summary>
  public struct FactoryTick
  {
    #region Variables

    private readonly DateTime stamp;
    private readonly int damage;
    private readonly int rdp;

    #endregion

    #region Constructors

    /// <summary>
    /// Create a new FactoryTick.
    /// </summary>
    /// <param name="stamp">The date/time of the FactoryTick.</param>
    /// <param name="damage">The percent damage of the Factory during the FactoryTick (0 = healthy, 100 = damaged).</param>
    /// <param name="rdp">0 or 1, depending if the Factory contributed to RDP during the FactoryTick.</param>
    public FactoryTick( DateTime stamp, int damage, int rdp )
    {
      this.stamp = stamp;
      this.damage = damage;
      this.rdp = rdp;
    }

    #endregion

    #region Properties

    /// <summary>
    /// The date/time of this FactoryTick.
    /// </summary>
    public DateTime TimeStamp
    {
      get { return this.stamp; }
    }

    /// <summary>
    /// The percent damage of the Factory during this FactoryTick.
    /// </summary>
    public int Damage
    {
      get { return this.damage; }
    }

    /// <summary>
    /// The number of resource points (0 or 1) the Factory produced during this FactoryTick.
    /// </summary>
    public int RDP
    {
      get { return this.rdp; }
    }

    #endregion
  }


  /// <summary>
  /// A structure for holding a count for both sides.
  /// </summary>
  public struct SideCount
  {
    #region Variables

    private int allied;
    private int axis;

    #endregion

    #region Constructors

    /// <summary>
    /// Create a new SideCount with the given values.
    /// </summary>
    /// <param name="allied">The Allied value.</param>
    /// <param name="axis">The Axis value.</param>
    public SideCount( int allied, int axis )
    {
      this.allied = allied;
      this.axis = axis;
    }

    #endregion

    #region Properties

    /// <summary>
    /// The count for the Allied side.
    /// </summary>
    public int Allied
    {
      get { return allied; }
      set { this.allied = value; }
    }

    /// <summary>
    /// The count for the Axis side.
    /// </summary>
    public int Axis
    {
      get { return this.axis; }
      set { this.axis = value; }
    }

    /// <summary>
    /// The total count for all sides.
    /// </summary>
    public int Total
    {
      get { return this.allied + this.axis; }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Increments this object by the given SideCount.
    /// </summary>
    /// <param name="other">The side count to add.</param>
    public void Add( SideCount other )
    {
      this.allied += other.Allied;
      this.axis += other.Axis;
    }

    /// <summary>
    /// Increments this object by the given values.
    /// </summary>
    /// <param name="allied">The allied count to add.</param>
    /// <param name="axis">The axis count to add.</param>
    public void Add( int allied, int axis )
    {
      this.allied += allied;
      this.axis += axis;
    }

    /// <summary>
    /// Provides a string representation of this object.
    /// </summary>
    /// <returns>The allied/axis counts.</returns>
    public override string ToString()
    {
      return String.Format( "{0}/{1}", this.allied, this.axis );
    }

    /// <summary>
    /// Support for implicit conversion to SideCountF.
    /// </summary>
    public static implicit operator SideCountF( SideCount sc )
    {
      return new SideCountF( sc.allied, sc.axis );
    }

    #endregion
  }


  /// <summary>
  /// A structure for holding a count for both sides.
  /// </summary>
  public struct SideCountF
  {
    #region Variables

    private float allied;
    private float axis;

    #endregion

    #region Constructors

    /// <summary>
    /// Create a new SideCountF with the given values.
    /// </summary>
    /// <param name="allied">The Allied value.</param>
    /// <param name="axis">The Axis value.</param>
    public SideCountF( float allied, float axis )
    {
      this.allied = allied;
      this.axis = axis;
    }

    #endregion

    #region Properties

    /// <summary>
    /// The count for the Allied side.
    /// </summary>
    public float Allied
    {
      get { return allied; }
      set { this.allied = value; }
    }

    /// <summary>
    /// The count for the Axis side.
    /// </summary>
    public float Axis
    {
      get { return this.axis; }
      set { this.axis = value; }
    }

    /// <summary>
    /// The total count for all sides.
    /// </summary>
    public float Total
    {
      get { return this.allied + this.axis; }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Increments this object by the given SideCount.
    /// </summary>
    /// <param name="other">The side count to add.</param>
    public void Add( SideCountF other )
    {
      this.allied += other.Allied;
      this.axis += other.Axis;
    }

    /// <summary>
    /// Increments this object by the given values.
    /// </summary>
    /// <param name="allied">The allied count to add.</param>
    /// <param name="axis">The axis count to add.</param>
    public void Add( float allied, float axis )
    {
      this.allied += allied;
      this.axis += axis;
    }

    /// <summary>
    /// Provides a string representation of this object.
    /// </summary>
    /// <returns>The allied/axis counts.</returns>
    public override string ToString()
    {
      return String.Format( "{0}/{1}", this.allied, this.axis );
    }

    #endregion
  }


  /// <summary>
  /// A structure for holding the per-cycle supply levels for a particular vehicle.
  /// </summary>
  public struct SupplyLevel
  {
    #region Variables

    private readonly int[] cycleValues;

    #endregion

    #region Constructors

    /// <summary>
    /// Create a new supply list.
    /// </summary>
    public SupplyLevel( int cycleZeroQuantity )
    {
      this.cycleValues = new int[1];
      this.cycleValues[0] = cycleZeroQuantity;
    }

    /// <summary>
    /// Create a new supply list.
    /// </summary>
    /// <param name="start">The starting vehicle count in cycle 0.</param>
    /// <param name="adjustments">Any adjustments to future cycles.</param>
    /// <param name="cycleCount">The number of cycles to fill out.</param>
    public SupplyLevel( int start, string adjustments, int cycleCount )
    {
      this.cycleValues = new int[cycleCount];
      this.cycleValues[0] = start;
      for( int i = 1; i < this.cycleValues.Length; i++ )
        this.cycleValues[i] = -1;

      foreach( string item in adjustments.Split( ',' ) )
      {
        if( item == "" ) break;

        string[] kv = item.Split( ':' );
        int cycle = int.Parse( kv[0] );
        int value = int.Parse( kv[1] );

        this.cycleValues[cycle] = value;
      }


      // fill in gaps

      for( int i = 0; i < this.cycleValues.Length; i++ )
        if( this.cycleValues[i] < 0 )
          this.cycleValues[i] = this.cycleValues[i - 1];
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets the supply level for a specific cycle.
    /// </summary>
    public int[] Cycle
    {
      get { return this.cycleValues; }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Provides a string representation of this object.
    /// </summary>
    /// <returns>A comma separated list of values.</returns>
    public override string ToString()
    {
      return this.cycleValues.Join();
    }

    #endregion
  }


  /// <summary>
  /// A structure that contains player rank levels and the names used in each branch/country.
  /// </summary>
  public struct Rank
  {
    #region Variables

    private readonly int id;
    private readonly string type;
    private readonly string[,] names;  // countryid,branchid

    #endregion

    #region Constructors

    /// <summary>
    /// Create a new Rank.
    /// </summary>
    /// <param name="id">The unique rank identifier.</param>
    /// <param name="type">The rank type (enlisted, officer, command, staff).</param>
    /// <param name="names">A two dimensional array of rank names, keyed by country and branch.</param>
    public Rank( int id, string type, string[,] names )
    {
      this.id = id;
      this.type = type;
      this.names = names;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets the name used by this rank in the given country and branch (rank.Name[country,branch] = "rankname").
    /// </summary>
    public string[,] Name
    {
      get { return this.names; }
    }

    /// <summary>
    /// Gets the rank code for this rank (eg, E1, O9, C14, R23).
    /// </summary>
    public string Code
    {
      get
      {
        string letter = this.type[0].ToString().ToUpper();

        if( letter == "S" )  // staff
          letter = "R";      // => rat

        int num = 0;
        switch( letter )
        {
          case "E":  // enlisted (1-8)
            num = this.id;
            break;
          case "O":  // officer (9-13)
            num = this.id - 8;
            break;
          case "C":  // command (14-22)
            num = this.id - 13;
            break;
          case "R":  // rat (23-25)
            num = this.id - 22;
            break;
        }

        return String.Format( "{0}{1}", letter, num );
      }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Provides a string representation of this object.
    /// </summary>
    /// <returns>The name used by the british army.</returns>
    public override string ToString()
    {
      return this.names[1, 1];
    }

    /// <summary>
    /// Returns a value indicating whether this instance is equal to a specified object value.
    /// </summary>
    /// <param name="obj">An object to compare with this instance.</param>
    /// <returns>True if obj is the same Rank.</returns>
    public override bool Equals( object obj )
    {
      return obj is Rank && this.id == ( (Rank)obj ).id;
    }

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    /// <returns>The rank id.</returns>
    public override int GetHashCode()
    {
      return this.id;
    }

    /// <summary>
    /// Support for the == operator.
    /// </summary>
    public static bool operator ==( Rank a, Rank b )
    {
      if( (object)a == null ) return (object)b == null;
      return a.Equals( b );
    }

    /// <summary>
    /// Support for the != operator.
    /// </summary>
    public static bool operator !=( Rank a, Rank b )
    {
      if( (object)a == null ) return (object)b != null;
      return !a.Equals( b );
    }

    #endregion
  }


  /// <summary>
  /// A structure for storing country border data, and converting to map pixels.
  /// </summary>
  public struct CountryBorder
  {
    #region Variables

    private readonly string countryName;
    private readonly PointF center;
    private readonly List<PointF[]> borders;  // decimal degrees

    private Dictionary<int, List<Point[]>> cachedMapBorders; // map pixels, keyed by map size

    #endregion

    #region Constructors

    /// <summary>
    /// Create a new CountryBorder.
    /// </summary>
    /// <param name="countryName">The country name (should match Language.Enum_CountryName_...).</param>
    /// <param name="center">A PointF in decimal degrees for the position of the name label.</param>
    /// <param name="borders">A list of polygon arrays in decimal degrees.</param>
    public CountryBorder( string countryName, PointF center, List<PointF[]> borders )
    {
      this.countryName = countryName.ToUpper();
      this.center = center;
      this.borders = borders;

      this.cachedMapBorders = new Dictionary<int, List<Point[]>>();

      string localName = Language.ResourceManager.GetString( String.Format( "Enum_CountryName_{0}", countryName ) );
      if( localName != null )
        this.countryName = localName;
    }

    #endregion

    #region Properties

    /// <summary>
    /// The name of the country.
    /// </summary>
    public string CountryName
    {
      get { return this.countryName; }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Gets the center point in map pixels.
    /// </summary>
    /// <param name="mm">The MapMetrics object used for the conversion.</param>
    /// <returns>A PointF in map pixels.</returns>
    public PointF GetCenter( MapMetrics mm )
    {
      return mm.DecimalLatLongToMap( this.center );
    }

    /// <summary>
    /// Gets a list of polygons in map pixels that make up the country border (cached).
    /// </summary>
    /// <param name="mm">The MapMetrics object used for the conversion.</param>
    /// <returns>A list of Point[] arrays in map pixels.</returns>
    public List<Point[]> GetBorders( MapMetrics mm )
    {
      // return cached version

      if( this.cachedMapBorders.ContainsKey( mm.ScaleCode ) )
        return this.cachedMapBorders[mm.ScaleCode];


      // convert latlong polygon to map pixels

      List<Point[]> mapPolys = new List<Point[]>();
      foreach( PointF[] degrees in this.borders )
      {
        Point[] mapPoly = new Point[degrees.Length];
        for( int i = 0; i < mapPoly.Length; i++ )
          mapPoly[i] = mm.DecimalLatLongToMap( degrees[i] );
        mapPolys.Add( mapPoly );
      }


      // cache value for next time and return

      this.cachedMapBorders.Add( mm.ScaleCode, mapPolys );
      return mapPolys;
    }

    /// <summary>
    /// Provides a string representation of this object.
    /// </summary>
    /// <returns>The country name.</returns>
    public override string ToString()
    {
      return this.countryName;
    }

    #endregion
  }

  #endregion

  #region Classes

  /// <summary>
  /// A structure to store information about one of the countries used in game.
  /// </summary>
  public class Country : IComparable<Country>, IComparable
  {
    #region Variables

    private readonly int id;
    private readonly string abbr;
    private readonly string name;
    private readonly Misc.MString demonym;
    private readonly Side side;

    private int rdpCycle = -1;
    private int rdpGoal = -1;
    private int rdpPrevCyclePoints = 0;

    private readonly Bitmap countryFlag;
    private readonly Bitmap brigadeFlag;

    #endregion

    #region Constructors

    /// <summary>
    /// Create a new Country.
    /// </summary>
    /// <param name="id">The unique Country id.</param>
    /// <param name="abbr">The two-letter Country abbreviation.</param>
    /// <param name="name">The Country's name (eg, "Germany").</param>
    /// <param name="demonym">The name of the Country's people (eg, "German") (can be multiple values separated by a comma).</param>
    /// <param name="side">Allied/Axis</param>
    public Country( int id, string abbr, string name, string demonym, Side side )
    {
      this.id = id;
      this.abbr = abbr;
      this.name = name;
      this.demonym = new Misc.MString( demonym );
      this.side = side;

      switch( this.abbr )
      {
        case "UK":
          this.countryFlag = Resources.flag_country_british;
          this.brigadeFlag = Resources.flag_brigade_british;
          break;
        case "US":
          this.countryFlag = Resources.flag_country_usa;
          this.brigadeFlag = Resources.flag_brigade_usa;
          break;
        case "FR":
          this.countryFlag = Resources.flag_country_french;
          this.brigadeFlag = Resources.flag_brigade_french;
          break;
        case "DE":
          this.countryFlag = Resources.flag_country_german;
          this.brigadeFlag = Resources.flag_brigade_german;
          break;
        case "NF":
          this.countryFlag = Resources.flag_country_nf;
          this.brigadeFlag = new Bitmap( 41, 21 );
          break;
        case "NH":
          this.countryFlag = Resources.flag_country_nh;
          this.brigadeFlag = new Bitmap( 41, 21 );
          break;

        default:  // empty image

          this.countryFlag = new Bitmap( 32, 19 );
          Graphics g = Graphics.FromImage( this.countryFlag );
          g.Clear( Color.Black );
          g.Dispose();

          this.brigadeFlag = new Bitmap( 41, 21 );

          break;
      }
    }

    /// <summary>
    /// Get a Country object that represents "No Country".
    /// </summary>
    public static Country None
    {
      get
      {
        return new Country( 0, "NONE", Language.Country_Name_NoCountry, Language.Country_Name_NoCountry, Side.None );
      }
    }

    #endregion

    #region Properties

    /// <summary>
    /// The unique Country identifier.
    /// </summary>
    public int ID
    {
      get { return this.id; }
    }

    /// <summary>
    /// The two-letter abbreviation of the Country.
    /// </summary>
    public string Abbr
    {
      get { return this.abbr; }
    }

    /// <summary>
    /// The name of the Country.
    /// </summary>
    public string Name
    {
      get { return this.name; }
    }

    /// <summary>
    /// The word describing the Countrys people.
    /// </summary>
    public Misc.MString Demonym
    {
      get { return this.demonym; }
    }

    /// <summary>
    /// The side this Country belongs to.
    /// </summary>
    public Side Side
    {
      get { return this.side; }
    }

    /// <summary>
    /// The RDP cycle currently being used (-1 if not loaded).
    /// </summary>
    public int CurrentRDPCycle
    {
      get { return this.rdpCycle; }
    }

    /// <summary>
    /// The RDP cycle currently in production (0 if not loaded).
    /// </summary>
    public int NextRDPCycle
    {
      get { return this.rdpCycle + 1; }
    }

    /// <summary>
    /// The current RDP goal in points (-1 if not loaded).
    /// </summary>
    public int RDPGoal
    {
      get { return this.rdpGoal; }
    }

    /// <summary>
    /// The total number of RDP points in all previous cycles.
    /// </summary>
    public int RDPPrevCyclePoints
    {
      get { return this.rdpPrevCyclePoints; }
    }

    /// <summary>
    /// If this Country is currently used in game.
    /// </summary>
    public bool IsActive
    {
      get { return this.side != Side.None && this.side != Side.Neutral; }
    }

    /// <summary>
    /// The flag for this Country.
    /// </summary>
    public Image CountryFlag
    {
      get { return this.countryFlag; }
    }

    /// <summary>
    /// The flag for this Country's brigade.
    /// </summary>
    public Image BrigadeFlag
    {
      get { return this.brigadeFlag; }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Sets or updates the RDP stats for this Country.
    /// </summary>
    /// <param name="cycle">The number of the current in-use RDP cycle.</param>
    public void SetRDPState( int cycle )
    {
      this.rdpCycle = cycle;
    }

    /// <summary>
    /// Sets or updates the RDP stats for this Country.
    /// </summary>
    /// <param name="cycle">The number of the current in-use RDP cycle.</param>
    /// <param name="goal">The number of RDP points to reach to complete the cycle.</param>
    /// <param name="prevCyclePoints">The total points for all previous cycles.</param>
    public void SetRDPState( int cycle, int goal, int prevCyclePoints )
    {
      this.rdpCycle = cycle;
      this.rdpGoal = goal;
      this.rdpPrevCyclePoints = prevCyclePoints;
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Provides a string representation of this object.
    /// </summary>
    /// <returns>The Country name.</returns>
    public override string ToString()
    {
      return this.name;
    }

    /// <summary>
    /// Returns a value indicating whether this instance is equal to a specified object value.
    /// </summary>
    /// <param name="obj">An object to compare with this instance.</param>
    /// <returns>True if obj is the same Country.</returns>
    public override bool Equals( object obj )
    {
      return obj is Country && this.id == ( (Country)obj ).id;
    }

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    /// <returns>The Country id.</returns>
    public override int GetHashCode()
    {
      return this.id;
    }

    /// <summary>
    /// Support for the == operator.
    /// </summary>
    public static bool operator ==( Country a, Country b )
    {
      if( (object)a == null ) return (object)b == null;
      return a.Equals( b );
    }

    /// <summary>
    /// Support for the != operator.
    /// </summary>
    public static bool operator !=( Country a, Country b )
    {
      if( (object)a == null ) return (object)b != null;
      return !a.Equals( b );
    }

    /// <summary>
    /// Compare this object with another Country object.
    /// </summary>
    /// <param name="other">The other Country to compare with.</param>
    /// <returns>-1, 0, 1</returns>
    public int CompareTo( Country other )
    {
      if( other == null ) return 1;
      return this.id.CompareTo( other.id );
    }

    /// <summary>
    /// Compare this object with another Country object.
    /// </summary>
    /// <param name="obj">The other Country to compare with.</param>
    /// <returns>-1, 0, 1</returns>
    public int CompareTo( object obj )
    {
      Country other = obj as Country;
      if( other == null ) return -1;
      return this.CompareTo( other );
    }

    #endregion
  }


  /// <summary>
  /// A ChokePoint is a town that can be attacked &amp; captured to move the front line.
  /// </summary>
  /// <remarks>Doesn't include Bridges.</remarks>
  public class ChokePoint : IComparable<ChokePoint>
  {
    #region Variables

    private readonly int id;
    private readonly string name;
    private readonly ChokePointType type;
    private Country origCountry;
    private readonly Point locationOctets;
    private readonly Point locationMeters;
    private int altitude = 0;  // in meters, 0 = not available

    private Country owner;
    private Country controller;
    private HCUnit aoHCUnit;  // null if no AO

    private List<Facility> facilities;
    private List<Firebase> firebases;
    private List<SupplyLink> links;
    private List<Depot> depots;
    private List<HCUnit> hcunits;

    private List<HCUnit> cacheDeployedHCUnits;        // cached value
    private List<ChokePoint> cacheLinkedChokePoints;  // cached value
    private List<Bridge> cacheNearbyBridges;          // cached value
    private Dictionary<ChokePoint, bool> nearbyChokePoints;  // bool value is unused
    
    private DateTime lastOwnerChanged;
    private DateTime lastControllerChanged;
    private DateTime lastContestedChanged;
    private DateTime lastAOChanged;

    private SortedList<DateTime, ChokePoint> captureFromCps;
    private ActivityLevel activity = ActivityLevel.None;

    private GameEventCollection eventTracker;

    #endregion

    #region Constructors

    /// <summary>
    /// Create a new ChokePoint.
    /// </summary>
    /// <param name="id">The unique ChokePoint id.</param>
    /// <param name="name">The name of the ChokePoint.</param>
    /// <param name="type">The type of ChokePoint.</param>
    /// <param name="origCountry">The Country that owns the ChokePoint at the start of the campaign.</param>
    /// <param name="locationOctets">The location of the ChokePoint in game world octets.</param>
    /// <param name="eventTracker">The event collection to use to track game events for this ChokePoint (may be null).</param>
    public ChokePoint( int id, string name, ChokePointType type, Country origCountry, Point locationOctets, GameEventCollection eventTracker )
    {
      this.id = id;
      this.name = name;
      this.type = type;
      this.origCountry = origCountry;
      this.locationOctets = locationOctets;
      this.locationMeters = MapMetrics.OctetToMeters( locationOctets );
      this.eventTracker = eventTracker;

      this.owner = this.controller = origCountry;
      this.aoHCUnit = null;

      this.facilities = new List<Facility>();
      this.firebases = new List<Firebase>();
      this.links = new List<SupplyLink>();
      this.depots = new List<Depot>();
      this.hcunits = new List<HCUnit>();

      this.cacheDeployedHCUnits = null;
      this.cacheLinkedChokePoints = null;
      this.cacheNearbyBridges = null;
      this.nearbyChokePoints = new Dictionary<ChokePoint, bool>();

      this.captureFromCps = new SortedList<DateTime, ChokePoint>();
    }

    #endregion

    #region Properties

    /// <summary>
    /// The unique ChokePoint identifier.
    /// </summary>
    public int ID
    {
      get { return this.id; }
    }

    /// <summary>
    /// The ChokePoint name.
    /// </summary>
    public string Name
    {
      get { return this.name; }
    }

    /// <summary>
    /// The type of this ChokePoint (eg, Generic, Bridge, ...)
    /// </summary>
    public ChokePointType Type
    {
      get { return this.type; }
    }

    /// <summary>
    /// The Country that originally owns this ChokePoint at the beginning of a campaign.
    /// </summary>
    public Country OriginalCountry
    {
      get { return this.origCountry; }
    }

    /// <summary>
    /// The coordinates of the ChokePoint on the map in game world octets.
    /// </summary>
    public Point LocationOctets
    {
      get { return this.locationOctets; }
    }

    /// <summary>
    /// The coordinates of the ChokePoint on the map in game world meters.
    /// </summary>
    public Point Location
    {
      get { return this.locationMeters; }
    }

    /// <summary>
    /// The event collection to use to track game events for this ChokePoint (or null if not tracked).
    /// </summary>
    public GameEventCollection EventTracker
    {
      get { return this.eventTracker; }
    }

    /// <summary>
    /// An iterator for all Game Events related to this ChokePoint.
    /// </summary>
    public IEnumerable<GameEvent> Events
    {
      get
      {
        if( this.eventTracker == null ) yield break;

        foreach( GameEvent gameEvent in this.eventTracker )
          if( gameEvent.ChokePoints.Contains( this ) )
            yield return gameEvent;
      }
    }

    /// <summary>
    /// The average altitude of this town in meters.
    /// </summary>
    public int AltitudeMeters
    {
      get { return this.altitude; }
      set { this.altitude = value; }
    }

    /// <summary>
    /// The average altitude of this town in feet.
    /// </summary>
    public int AltitudeFeet
    {
      get { return (int)Math.Round( this.altitude * 3.2808399F ); }
    }

    /// <summary>
    /// Set or get the Country that currently owns this ChokePoint.
    /// </summary>
    /// <remarks>The owner is the last Country that owned all Facilities in the ChokePoint.</remarks>
    public Country Owner
    {
      get { return this.owner; }
      set
      {
        if( value == this.owner ) return;
        this.owner = value;
        OnOwnerChanged();
      }
    }

    /// <summary>
    /// The Country that currently controls this ChokePoint.
    /// </summary>
    /// <remarks>The controller is the last Country that owned all Armybases in the ChokePoint.</remarks>
    public Country Controller
    {
      get { return this.controller; }
    }

    /// <summary>
    /// Gets whether this ChokePoint currently has an Attack Objective placed on it.
    /// May be set to false to clear the AO.
    /// </summary>
    public bool HasAO
    {
      get { return this.aoHCUnit != null; }
      set
      {
        if( !value )
        {
          this.aoHCUnit = null;
          this.captureFromCps.Clear();
        }
      }
    }

    /// <summary>
    /// Gets the HCUnit that placed the AO on this cp (null when no AO).
    /// </summary>
    public HCUnit AttackingHCUnit
    {
      get { return this.aoHCUnit; }
    }

    /// <summary>
    /// Gets whether the ChokePoint is currently contested.
    /// </summary>
    public bool IsContested
    {
      get
      {
        return this.facilities.Any( fac => fac.Owner.Side != this.owner.Side );
      }
      set
      {
        if( value ) return;  // can only set to false

        this.Uncontest();
      }
    }

    /// <summary>
    /// Gets whether this ChokePoint has been contested in the past two hours.
    /// </summary>
    public bool ContestedRecently
    {
      get
      {
        return this.Events.OfType<ChokePointRegainedGameEvent>().Any();
      }
    }

    /// <summary>
    /// The level of player activity around this ChokePoint.
    /// </summary>
    public ActivityLevel Activity
    {
      get { return this.activity; }
      set { this.activity = value; }
    }

    /// <summary>
    /// A list of all Facilities in this ChokePoint.
    /// </summary>
    public ReadOnlyCollection<Facility> Facilities
    {
      get { return this.facilities.AsReadOnly(); }
    }

    /// <summary>
    /// A list of all Firebases attached to this ChokePoint.
    /// </summary>
    public ReadOnlyCollection<Firebase> Firebases
    {
      get { return this.firebases.AsReadOnly(); }
    }

    /// <summary>
    /// A list of all Links attached to this ChokePoint.
    /// </summary>
    public ReadOnlyCollection<SupplyLink> Links
    {
      get { return this.links.AsReadOnly(); }
    }

    /// <summary>
    /// A list of all Depots in this ChokePoint.
    /// </summary>
    public ReadOnlyCollection<Depot> Depots
    {
      get { return this.depots.AsReadOnly(); }
    }

    /// <summary>
    /// A list of all HCUnits deployed in this ChokePoint.
    /// </summary>
    /// <remarks>
    /// Only returns units of the same side as the cp owner, enemy units can be present
    /// in this.hcunits if they haven't been kicked out yet due to 5 minute delay.
    /// </remarks>
    public ReadOnlyCollection<HCUnit> DeployedHCUnits
    {
      get
      {
        // return cached copy

        if( this.cacheDeployedHCUnits != null )
          return this.cacheDeployedHCUnits.AsReadOnly();


        // not cached, regenerate

        this.cacheDeployedHCUnits = new List<HCUnit>();

        foreach( HCUnit hcunit in this.hcunits )
          if( hcunit.Country.Side == this.owner.Side && hcunit.Country.Side == this.controller.Side )
            cacheDeployedHCUnits.Add( hcunit );

        cacheDeployedHCUnits.Sort();

        return cacheDeployedHCUnits.AsReadOnly();
      }
    }

    /// <summary>
    /// A list of all linked neighbouring ChokePoints.
    /// </summary>
    public ReadOnlyCollection<ChokePoint> LinkedChokePoints
    {
      get
      {
        // return cached copy

        if( this.cacheLinkedChokePoints != null )
          return this.cacheLinkedChokePoints.AsReadOnly();  


        // not cached, regenerate

        this.cacheLinkedChokePoints = new List<ChokePoint>();

        foreach( Depot depot in this.depots )
          if( depot.LinkedDepot != null )
            this.cacheLinkedChokePoints.Add( depot.LinkedDepot.ChokePoint );

        this.cacheLinkedChokePoints.Sort();

        return this.cacheLinkedChokePoints.AsReadOnly();
      }
    }

    /// <summary>
    /// A list of all neighbouring ChokePoints.
    /// </summary>
    public ICollection<ChokePoint> NearbyChokePoints
    {
      get { return this.nearbyChokePoints.Keys; }
      set
      {
        this.nearbyChokePoints = new Dictionary<ChokePoint, bool>();

        foreach( ChokePoint cp in value )
          this.nearbyChokePoints.Add( cp, false );
      }
    }

    /// <summary>
    /// True if one of the three training cp's.
    /// </summary>
    public bool IsTraining
    {
      get { return this.id == 4 || this.id == 6 || this.id == 21; }  // British/French/German
    }
    
    /// <summary>
    /// The last time the ChokePoint was captured (if known).
    /// </summary>
    public DateTime LastOwnerChanged
    {
      get { return this.lastOwnerChanged; }
      set
      {
        this.lastOwnerChanged = value;
        this.lastControllerChanged = DateTime.MinValue;
        this.lastContestedChanged = DateTime.MinValue;
        this.lastAOChanged = DateTime.MinValue;
      }
    }

    /// <summary>
    /// The last time control was lost or regained (if known).
    /// </summary>
    public DateTime LastControllerChanged
    {
      get { return this.lastControllerChanged; }
      set { this.lastControllerChanged = value; }
    }

    /// <summary>
    /// The last time the ChokePoint was contested or regained (if known).
    /// </summary>
    public DateTime LastContestedChanged
    {
      get { return this.lastContestedChanged; }
      set { this.lastContestedChanged = value; }
    }

    /// <summary>
    /// The last time an AO was placed or withdrawn (if known).
    /// </summary>
    public DateTime LastAOChanged
    {
      get { return this.lastAOChanged; }
    }

    /// <summary>
    /// Gets the current percentage of friendly facilities, weighted by 'importance'.
    /// </summary>
    public float PercentOwnership
    {
      get
      {
        float friendly = 0, enemy = 0;

        foreach( Facility facility in this.facilities )
        {
          int importance = 0;
          switch( facility.Type )
          {
            case FacilityType.City:
            case FacilityType.Factory:
              importance = 1;
              break;

            case FacilityType.Depot:
              if( facility.EnemySpawnable )
              {
                if( facility.Owner.Side == ( (Depot)facility ).LinkedDepot.Owner.Side )
                  importance = 4;  // enemy spawnable
                else
                  importance = 3;  // friendly owned spawnable (or enemy-owned, but can't spawn)
              }
              else
                importance = 2;  // non-spawnable depot
              break;

            case FacilityType.Airbase:
            case FacilityType.Navalbase:
              importance = 3;
              break;

            case FacilityType.Armybase:
              importance = 5;
              break;
          }

          if( facility.Owner.Side == this.owner.Side )
            friendly += importance;
          else
            enemy += importance;
        }

        return friendly / ( friendly + enemy ) * 100;
      }
    }

    /// <summary>
    /// True if linked to an enemy Chokepoint.
    /// </summary>
    public bool IsFrontline
    {
      get { return this.LinkedChokePoints.Any( cp => Misc.AreEnemy( cp.Owner, this.owner ) ); }
    }

    /// <summary>
    /// Gets a hash of ChokePoint:percent values of where the current attack is coming from.
    /// </summary>
    /// <remarks>Should always be empty if no AO.</remarks>
    public SortedList<float, ChokePoint> AttackFrom
    {
      get
      {
        Dictionary<ChokePoint, float> cpCount = new Dictionary<ChokePoint, float>();
        foreach( ChokePoint cp in this.captureFromCps.Values )
        {
          if( !cpCount.ContainsKey( cp ) )
            cpCount.Add( cp, 0 );

          cpCount[cp]++;
        }

        SortedList<float, ChokePoint> attackFrom = new SortedList<float, ChokePoint>();
        foreach( ChokePoint cp in cpCount.Keys )
        {
          float percent = ( cpCount[cp] / captureFromCps.Count ) * 100;

          while( attackFrom.ContainsKey( percent ) )
            percent += 0.00001F;  // make unique

          attackFrom.Add( percent, cp );
        }

        return attackFrom;
      }
    }

    /// <summary>
    /// True if this ChokePoint has a friendly deployed brigade, excluding paras
    /// (required for spawnable depot).
    /// </summary>
    public bool HasSpawnableHCUnit
    {
      get
      {
        foreach( HCUnit hcunit in this.DeployedHCUnits )
          if( hcunit.Level == HCUnitLevel.Brigade && hcunit.MoveType == MoveType.Land )
            return true;

        return false;
      }
    }

    /// <summary>
    /// True if this ChokePoint has a friendly deployed army brigade (excluding paras).
    /// </summary>
    public bool HasArmyBrigade
    {
      get
      {
        foreach( HCUnit hcunit in this.DeployedHCUnits )
          if( hcunit.Branch == Branch.Army && hcunit.Level == HCUnitLevel.Brigade && !hcunit.IsPara )
            return true;

        return false;
      }
    }

    /// <summary>
    /// True if this ChokePoint has a friendly deployed air brigade/division.
    /// </summary>
    public bool HasAirUnit
    {
      get
      {
        foreach( HCUnit hcunit in this.DeployedHCUnits )
          if( hcunit.Branch == Branch.Airforce )
            return true;

        return false;
      }
    }

    /// <summary>
    /// True if this ChokePoint has a friendly deployed navy brigade/division.
    /// </summary>
    public bool HasNavyUnit
    {
      get
      {
        foreach( HCUnit hcunit in this.DeployedHCUnits )
          if( hcunit.Branch == Branch.Navy )
            return true;

        return false;
      }
    }

    /// <summary>
    /// True if this ChokePoint contains an airfield facility.
    /// </summary>
    public bool HasArmybase
    {
      get
      {
        foreach( Facility facility in this.facilities )
          if( facility.Type == FacilityType.Armybase )
            return true;

        return false;
      }
    }

    /// <summary>
    /// True if this ChokePoint contains an airfield facility.
    /// </summary>
    public bool HasAirbase
    {
      get
      {
        foreach( Facility facility in this.facilities )
          if( facility.Type == FacilityType.Airbase )
            return true;

        return false;
      }
    }

    /// <summary>
    /// True if this ChokePoint contains a docks facility.
    /// </summary>
    public bool HasNavalbase
    {
      get
      {
        foreach( Facility facility in this.facilities )
          if( facility.Type == FacilityType.Navalbase )
            return true;

        return false;
      }
    }

    /// <summary>
    /// Generate a flag image for the ChokePoint, showing owner, controller &amp; contested state.
    /// </summary>
    public Image FlagImage
    {
      get
      {
        Image bitmap = new Bitmap( 41, 21 );
        Graphics g = Graphics.FromImage( bitmap );

        if( this.IsContested ) // add contested icon, centered-right
          g.DrawImage( Resources.contested, 20, 2, 21, 17 );

        g.DrawImage( this.owner.BrigadeFlag, 0, 0, 41, 21 ); // add full-size owner flag, top-left

        if( this.controller != this.owner ) // add smaller controller flag, half-size, bottom-right
          g.DrawImage( this.controller.BrigadeFlag, 21, 11, 20, 10 );

        if( this.HasEnemySpawnable )  // exclaimation mark far right
          g.DrawImage( Resources.spawnable, 36, 2.5F, 5, 16 );

        g.Dispose();

        return bitmap;
      }
    }

    /// <summary>
    /// A tooltip for the ChokePoints flag image.
    /// </summary>
    public string FlagTooltip
    {
      get
      {
        string tooltip = String.Format( "{0} {1}", Language.Misc_OwnedBy, this.owner );
        if( this.controller != this.owner )
          tooltip += String.Format( ", {0} {1}", Language.Misc_ControlledBy, this.controller );
        if( this.IsContested )
          tooltip += String.Format( " ({0})", Language.Misc_Contested.ToUpper() );
        return tooltip;
      }
    }

    /// <summary>
    /// Gets the led graph image for the current activity level.
    /// </summary>
    public Image ActivityImage
    {
      get
      {
        switch( this.activity )
        {
          case ActivityLevel.None:
            return Resources.ledgraph0;
          case ActivityLevel.Low:
            return Resources.ledgraph1;
          case ActivityLevel.Light:
            return Resources.ledgraph2;
          case ActivityLevel.Moderate:
            return Resources.ledgraph3;
          case ActivityLevel.Heavy:
            return Resources.ledgraph4;
          default:
            throw new ArgumentOutOfRangeException();
        }
      }
    }

    /// <summary>
    /// True if the enemy own a spawnable depot.
    /// </summary>
    public bool HasEnemySpawnable
    {
      get
      {
        foreach( Depot depot in this.depots )
          if( depot.Owner.Side != this.owner.Side && depot.OwnerCanSpawn )
            return true;

        return false;
      }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Adds the given ChokePoint from the list of NearbyChokePoints.
    /// </summary>
    /// <param name="cp">The ChokePoint to add.</param>
    public void AddNearCP( ChokePoint cp )
    {
      this.nearbyChokePoints.Add( cp, false );
    }

    /// <summary>
    /// Removes the given ChokePoint from the list of NearbyChokePoints.
    /// </summary>
    /// <param name="cp">The ChokePoint to remove.</param>
    public void RemoveNearCP( ChokePoint cp )
    {
      this.nearbyChokePoints.Remove( cp );
    }

    /// <summary>
    /// Associate a Facility with this ChokePoint.
    /// </summary>
    /// <param name="facility">The new Facility to associate.</param>
    public void AddFacility( Facility facility )
    {
      this.facilities.Add( facility );
      this.facilities.Sort();  // keep sorted

      if( facility is Depot )
      {
        this.depots.Add( (Depot)facility );
        this.depots.Sort();  // keep sorted
        this.cacheLinkedChokePoints = null;  // flush cache
      }
    }

    /// <summary>
    /// Associate a Firebase with this ChokePoint.
    /// </summary>
    /// <param name="firebase">The new Firebase to associate.</param>
    public void AddFirebase( Firebase firebase )
    {
      this.firebases.Add( firebase );
    }

    /// <summary>
    /// Associate a cp link with this ChokePoint.
    /// </summary>
    /// <param name="link">The new SupplyLink to associate.</param>
    public void AddLink( SupplyLink link )
    {
      this.links.Add( link );
    }

    /// <summary>
    /// Set the initial state of this ChokePoint.
    /// </summary>
    /// <param name="owner">The Country that currently owns the ChokePoint.</param>
    public void SetInitState( Country owner )
    {
      foreach( Facility facility in this.facilities )
        facility.SetInitState( owner );

      this.controller = owner;
      this.Owner = owner;
    }

    /// <summary>
    /// Set the initial state of this ChokePoint.
    /// </summary>
    /// <param name="owner">The Country that currently owns the ChokePoint.</param>
    /// <param name="controller">The Country that currently controls the ChokePoint.</param>
    public void SetInitState( Country owner, Country controller )
    {
      this.Owner = owner;
      this.controller = controller;

      foreach( Facility facility in this.facilities )
        facility.SetInitState( owner );
    }

    /// <summary>
    /// Set the initial state of this ChokePoint.
    /// </summary>
    /// <param name="owner">The Country that currently owns the ChokePoint.</param>
    /// <param name="controller">The Country that currently controls the ChokePoint.</param>
    /// <param name="aoHCUnit">The HCUnit that placed the AO (null if no AO).</param>
    public void SetInitState( Country owner, Country controller, HCUnit aoHCUnit )
    {
      this.Owner = owner;
      this.controller = controller;
      this.aoHCUnit = aoHCUnit;

      foreach( Facility facility in this.facilities )
        facility.SetInitState( owner );
    }

    /// <summary>
    /// Set a new owner for this ChokePoint when all facilities in the cp have been captured.
    /// </summary>
    /// <remarks>Raises the ChokePointCapturedGameEvent.</remarks>
    /// <param name="time">The time the capture event occurred.</param>
    /// <param name="newOwner">The new owning Country.</param>
    public void SetNewOwner( DateTime time, Country newOwner )
    {
      if( this.eventTracker != null )
        this.eventTracker.Add( new ChokePointCapturedGameEvent( time, this, newOwner, this.owner, this.LastContestedChanged ) );

      this.Owner = newOwner;
      this.LastOwnerChanged = time;  // resets all other last...changed times
    }

    /// <summary>
    /// Set a new controller for this ChokePoint when all abs in the cp have been captured.
    /// </summary>
    /// <param name="time">The time the capture event occurred.</param>
    /// <param name="newController">The new controlling Country.</param>
    /// <param name="raiseEvent">True if the ChokePointControllerChangedGameEvent should be raised.</param>
    public void SetNewController( DateTime time, Country newController, bool raiseEvent )
    {
      if( raiseEvent )
      {
        if( this.eventTracker != null )
          this.eventTracker.Add( new ChokePointControllerChangedGameEvent( time, this, newController, this.controller ) );
        this.lastControllerChanged = time;
      }

      this.controller = newController;
    }

    /// <summary>
    /// Sets a new AO state for this ChokePoint.
    /// </summary>
    /// <param name="time">The time the AO change occurred.</param>
    /// <param name="aoHCUnit">The HCUnit that placed the AO, or null if AO withdrawn.</param>
    public void SetNewAOState( DateTime time, HCUnit aoHCUnit )
    {
      if( this.aoHCUnit == aoHCUnit ) return;

      if( this.aoHCUnit != null && aoHCUnit != null )  // attacking hcunit changed: update but don't raise event
      {
        this.aoHCUnit = aoHCUnit;
        return;
      }

      this.aoHCUnit = aoHCUnit;

      if( this.eventTracker != null )
        this.eventTracker.Add( new AttackObjectiveChangedGameEvent( time, this, aoHCUnit ) );
      this.lastAOChanged = time;


      // if AO removed while contested, uncontest

      if( !this.HasAO && this.IsContested )
      {
        this.Uncontest();
        this.lastContestedChanged = time;
      }


      // AO removed, clear AttackFrom

      if( aoHCUnit == null )
        this.captureFromCps.Clear();
    }

    /// <summary>
    /// Add a HCUnit to the list of units deployed at this ChokePoint.
    /// </summary>
    /// <param name="hcunit">The HCUnit that is deployed at this ChokePoint.</param>
    public void AddHCUnit( HCUnit hcunit )
    {
      this.hcunits.Add( hcunit );
      this.cacheDeployedHCUnits = null;  // flush cache
    }

    /// <summary>
    /// Remove a HCUnit to the list of units deployed at this ChokePoint.
    /// </summary>
    /// <param name="hcunit">The HCUnit that is no longer deployed at this ChokePoint.</param>
    public void RemoveHCUnit( HCUnit hcunit )
    {
      if( this.hcunits.Remove( hcunit ) )
        this.cacheDeployedHCUnits = null;  // flush cache
    }

    /// <summary>
    /// Removes any hcunits of the specified branch from the currently deployed list.
    /// </summary>
    /// <param name="branch">The branch to undeploy.</param>
    public void UndeployBranch( Branch branch )
    {
      for( int i = 0; i < this.hcunits.Count; i++ )
      {
        if( this.hcunits[i].Branch == branch )
        {
          this.hcunits.RemoveAt( i );
          this.cacheDeployedHCUnits = null;  // flush cache
          i--;
        }
      }
    }

    /// <summary>
    /// Checks whether the enemy has captured all bases in this ChokePoint.
    /// </summary>
    /// <param name="branch">The type of base to check.</param>
    /// <returns>True if all bases are owned by the enemy, false if one or more are friendly or none exist.</returns>
    public bool AllBasesCaptured( Branch branch )
    {
      FacilityType baseType;
      switch( branch )
      {
        case Branch.Army:
          baseType = FacilityType.Armybase;
          break;
        case Branch.Airforce:
          baseType = FacilityType.Airbase;
          break;
        case Branch.Navy:
          baseType = FacilityType.Navalbase;
          break;
        default:
          return false;
      }

      bool enemyFound = false;
      foreach( Facility facility in this.facilities )
      {
        if( facility.Type != baseType ) continue;

        if( facility.Owner.Side == this.owner.Side )
          return false;  // one (or more) bases are friendly owned
        else
          enemyFound = true;  // enemy base exists
      }

      return enemyFound;
      // if false, no bases exists
      // if true, all bases are owned by the enemy
    }

    /// <summary>
    /// Returns a list of Bridges that are within a 5km radius of this ChokePoint.
    /// </summary>
    /// <param name="bridges">An array of bridges to search.</param>
    /// <returns>Readonly list of Bridges (cached).</returns>
    public ReadOnlyCollection<Bridge> GetNearbyBridges( Bridge[] bridges )
    {
      if( this.cacheNearbyBridges != null )
        return this.cacheNearbyBridges.AsReadOnly();


      // not cached, regenerate

      this.cacheNearbyBridges = new List<Bridge>();

      foreach( Bridge bridge in bridges )
        if( bridge != null && Misc.DistanceBetween( this.locationOctets, bridge.Location ) < 6.5 )  // 6.5 x 800m = 5.2km
          cacheNearbyBridges.Add( bridge );

      this.cacheNearbyBridges.Sort();

      return this.cacheNearbyBridges.AsReadOnly();
    }

    /// <summary>
    /// Forcibly uncontests this ChokePoint by changing the owner of any enemy child
    /// facilities to newOwner (default: cp owner).
    /// </summary>
    /// <remarks>Doesn't raise any events.</remarks>
    public void Uncontest()
    {
      Uncontest( this.owner );
    }

    /// <summary>
    /// Forcibly uncontests this ChokePoint by changing the owner of any enemy child
    /// facilities to newOwner (default: cp owner).
    /// </summary>
    /// <remarks>Doesn't raise any events.</remarks>
    /// <param name="newOwner">The Country to set facilities to.</param>
    public void Uncontest( Country newOwner )
    {
      // make sure all facilities belong to same side

      foreach( Facility facility in this.facilities )
        if( facility.Owner.Side != newOwner.Side )
          facility.Owner = newOwner;


      // update owner/controller

      if( this.owner.Side != newOwner.Side )
      {
        this.Owner = newOwner;
        this.controller = newOwner;
        this.lastOwnerChanged = this.lastControllerChanged = DateTime.MinValue;
      }

      if( this.controller != this.owner )  // match to owning country
      {
        this.controller = this.owner;
        this.lastControllerChanged = DateTime.MinValue;
      }


      // reset capture times for facilities

      foreach( Facility facility in this.facilities )
        facility.ResetCaptureTime();
    }

    /// <summary>
    /// Tests whether this ChokePoint is linked to another ChokePoint.
    /// </summary>
    /// <param name="cp">The other cp to test.</param>
    /// <returns>True if the cps are linked to each other.</returns>
    public bool IsLinked( ChokePoint cp )
    {
      return this.LinkedChokePoints.Contains( cp );
    }

    /// <summary>
    /// Tests whether this ChokePoint is near another ChokePoint.
    /// </summary>
    /// <param name="cp">The other cp to test.</param>
    /// <returns>True if the cps are near each other.</returns>
    public bool IsNear( ChokePoint cp )
    {
      return this.nearbyChokePoints.ContainsKey( cp );
    }

    /// <summary>
    /// Add a new data point to calculate where captures are originating from.
    /// </summary>
    /// <param name="time">The time the capture occurred (must be local time).</param>
    /// <param name="cp">The ChokePoint the capturing player's brigade is deployed at.</param>
    public void AddCaptureFrom( DateTime time, ChokePoint cp )
    {
      if( cp == null || cp.IsTraining ) return;  // hcunit not deployed
      if( !this.HasAO ) return;    // don't add if no AO
      if( cp == this ) return;  // don't add self

      while( captureFromCps.ContainsKey( time ) )
        time = time.AddMilliseconds( 1 );  // make unique

      captureFromCps.Add( time, cp );

      DateTime oneHourAgo = DateTime.Now.AddHours( -1 );
      while( captureFromCps.Count > 0 && captureFromCps.Keys[0] < oneHourAgo )
        captureFromCps.RemoveAt( 0 );
    }

    /// <summary>
    /// Check if currently allowed to perform a manual hcunit move to the given linked
    /// ChokePoint.
    /// </summary>
    /// <param name="linkCP">The linked cp to test.</param>
    /// <returns>True if allowed to move, false if not (enemy controlled, firebase, depot on link).</returns>
    public bool CanMoveUnitTo( ChokePoint linkCP )
    {
      if( this.owner.Side != linkCP.owner.Side || this.controller.Side != linkCP.Controller.Side )
        return false;  // must own/control dest cp


      // get the depot that links to linkCP

      Depot linkDepot = null;
      foreach( Depot depot in this.depots )
      {
        if( depot.LinkedDepot != null && depot.LinkedDepot.ChokePoint == linkCP )
        {
          linkDepot = depot;
          break;
        }
      }

      if( linkDepot == null )
        return false;  // no link?!

      if( linkDepot.Owner.Side != this.owner.Side || linkDepot.LinkedDepot.Owner.Side != this.owner.Side )
        return false;  // depot on either end of link is enemy owned

      if( linkDepot.Firebase != null && ( linkDepot.Firebase.IsOpen || linkDepot.Firebase.LinkedFirebase.IsOpen ) && linkDepot.Firebase.Link.Side != this.owner.Side )
        return false;  // enemy fb on link

      return true;
    }

    /// <summary>
    /// Gets the outbound Firebase that is near the given linked ChokePoint.
    /// </summary>
    /// <param name="linkCP">The linked ChokePoint.</param>
    /// <returns>The Firebase nearest linkCP.</returns>
    public Firebase GetFirebaseTo( ChokePoint linkCP )
    {
      return this.firebases.FirstOrDefault( fb => fb.LinkedChokePoint == linkCP );
    }

    /// <summary>
    /// Gets the inbound Firebase that is near this ChokePoint.
    /// </summary>
    /// <param name="linkCP">The linked ChokePoint.</param>
    /// <returns>The Firebase nearest this cp.</returns>
    public Firebase GetFirebaseFrom( ChokePoint linkCP )
    {
      Firebase fbTo = GetFirebaseTo( linkCP );
      return fbTo != null ? fbTo.LinkedFirebase : null;
    }

    /// <summary>
    /// Checks if this ChokePoint is near a point, used to calc activity levels.
    /// </summary>
    /// <param name="refPoint">The reference Point in game world meters.</param>
    /// <returns>True if this cp or any of it's open firebases are nearby.</returns>
    public bool IsNearPoint( Point refPoint )
    {
      const int CP_RADIUS = 3000;
      const int FB_RADIUS = 1000;


      // check if near cp

      if( Misc.DistanceBetween( this.locationMeters, refPoint ) < CP_RADIUS )
        return true;


      // check if near any open fb's

      for( int i = 0; i < this.firebases.Count; i++ )
      {
        if( this.firebases[i].Link.State == FirebaseState.Inactive ) continue;

        Point fbPoint = this.firebases[i].IsOpen ? this.firebases[i].Location : this.firebases[i].LinkedFirebase.Location;
        if( Misc.DistanceBetween( fbPoint, refPoint ) < FB_RADIUS )
          return true;
      }

      return false;
    }

    /// <summary>
    /// Calculates the bounds of the CP marker and all facilities.
    /// </summary>
    /// <returns>A Rectangle in game world meters.</returns>
    public Rectangle CalcBounds()
    {
      int minX, maxX, minY, maxY;
      minX = maxX = this.locationMeters.X;
      minY = maxY = this.locationMeters.Y;

      foreach( Facility facility in this.facilities )
      {
        if( facility.Location.X < minX ) minX = facility.Location.X;
        if( facility.Location.X > maxX ) maxX = facility.Location.X;
        if( facility.Location.Y < minY ) minY = facility.Location.Y;
        if( facility.Location.Y > maxY ) maxY = facility.Location.Y;
      }

      return new Rectangle( minX, minY, maxX - minX, maxY - minY );
    }

    /// <summary>
    /// Remove any circular references to allow garbage collection.
    /// </summary>
    public void Cleanup()
    {
      this.origCountry = null;
      this.owner = null;
      this.controller = null;
      this.aoHCUnit = null;
      this.facilities = null;
      this.firebases = null;
      this.links = null;
      this.depots = null;
      this.hcunits = null;
      this.cacheDeployedHCUnits = null;
      this.cacheLinkedChokePoints = null;
      this.cacheNearbyBridges = null;
      this.nearbyChokePoints = null;
      this.captureFromCps = null;
      this.eventTracker = null;
    }
    
    #endregion

    #region Utility Methods

    /// <summary>
    /// Provides a string representation of this object.
    /// </summary>
    /// <returns>The ChokePoint name.</returns>
    public override string ToString()
    {
      return this.name;
    }

    /// <summary>
    /// Returns a value indicating whether this instance is equal to a specified object value.
    /// </summary>
    /// <param name="obj">An object to compare with this instance.</param>
    /// <returns>True if obj is the same ChokePoint.</returns>
    public override bool Equals( object obj )
    {
      return obj is ChokePoint && this.id == ( (ChokePoint)obj ).id;
    }

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    /// <returns>The ChokePoint id.</returns>
    public override int GetHashCode()
    {
      return this.id;
    }

    /// <summary>
    /// Support for the == operator.
    /// </summary>
    public static bool operator ==( ChokePoint a, ChokePoint b )
    {
      if( (object)a == null ) return (object)b == null;
      return a.Equals( b );
    }

    /// <summary>
    /// Support for the != operator.
    /// </summary>
    public static bool operator !=( ChokePoint a, ChokePoint b )
    {
      if( (object)a == null ) return (object)b != null;
      return !a.Equals( b );
    }

    /// <summary>
    /// Compare this object with another ChokePoint object.
    /// </summary>
    /// <param name="other">The other ChokePoint to compare with.</param>
    /// <returns>-1, 0, 1</returns>
    public int CompareTo( ChokePoint other )
    {
      if( other == null ) return 1;
      return this.name.CompareTo( other.name );
    }

    #endregion

    #region Events

    /// <summary>
    /// The ChokePoint's owner has changed to a new Country.
    /// </summary>
    public event EventHandler OwnerChanged;

    /// <summary>
    /// Raises the OwnerChanged event.
    /// </summary>
    protected virtual void OnOwnerChanged()
    {
      EventHandler handler = this.OwnerChanged;
      if( handler != null )
        handler( this, new EventArgs() );
    }

    #endregion
  }


  /// <summary>
  /// A SupplyLink represents the link between two adjacent ChokePoints.
  /// It contains the two CPs, their linked Depots, and any Firebases.
  /// </summary>
  public class SupplyLink : IComparable<SupplyLink>, IEnumerable<ChokePoint>
  {
    #region Variables

    private ChokePoint cpA, cpB;
    private Depot depotA, depotB;  // depotA is in cpA
    private Firebase fbA, fbB;  // fbA is nearest cpB

    #endregion

    #region Constructors

    /// <summary>
    /// Create a new SupplyLink from a pair of linked Depots.
    /// </summary>
    /// <param name="depot1">The first Depot.</param>
    /// <param name="depot2">The second Depot.</param>
    public SupplyLink( Depot depot1, Depot depot2 )
    {
      // get parent cps

      ChokePoint cp1 = depot1.ChokePoint;
      ChokePoint cp2 = depot2.ChokePoint;


      // make sure link is valid

      Debug.Assert( cp1.LinkedChokePoints.Contains( cp2 ), "cps not linked" );
      Debug.Assert( cp2.LinkedChokePoints.Contains( cp1 ), "cps not linked" );


      // make sure cpA has the lower ID for consistancy

      if( cp1.ID < cp2.ID )
      {
        this.cpA = cp1;
        this.cpB = cp2;
        this.depotA = depot1;
        this.depotB = depot2;
      }
      else
      {
        this.cpA = cp2;
        this.cpB = cp1;
        this.depotA = depot2;
        this.depotB = depot1;
      }

      this.fbA = this.ChokePointA.GetFirebaseTo( this.ChokePointB );
      this.fbB = this.fbA != null ? this.fbA.LinkedFirebase : null;


      // add self to cp and firebase objects

      this.cpA.AddLink( this );
      this.cpB.AddLink( this );

      if( this.HasFirebases )
      {
        this.fbA.AddLink( this );
        this.fbB.AddLink( this );
      }
    }

    #endregion

    #region Properties

    /// <summary>
    /// The unique link identifier (generated from CP ids).
    /// </summary>
    public int ID
    {
      get
      {
        return ( this.cpA.ID * 10000 ) + this.cpB.ID;  // 1234,23 -> 12340023
      }
    }

    /// <summary>
    /// The ChokePoint on the A end of the link.
    /// </summary>
    public ChokePoint ChokePointA
    {
      get { return this.cpA; }
    }

    /// <summary>
    /// The ChokePoint on the B end of the link.
    /// </summary>
    public ChokePoint ChokePointB
    {
      get { return this.cpB; }
    }

    /// <summary>
    /// The Depot on the A end of the link (in ChokePointA).
    /// </summary>
    public Depot DepotA
    {
      get { return this.depotA; }
    }

    /// <summary>
    /// The Depot on the B end of the link (in ChokePointB).
    /// </summary>
    public Depot DepotB
    {
      get { return this.depotB; }
    }

    /// <summary>
    /// The Firebase on the A end of the link (nearest ChokePointB).
    /// </summary>
    public Firebase FirebaseA
    {
      get { return this.fbA; }
    }

    /// <summary>
    /// The Firebase on the B end of the link (nearest ChokePointA).
    /// </summary>
    public Firebase FirebaseB
    {
      get { return this.fbB; }
    }

    /// <summary>
    /// Gets whether this link has active FBs.
    /// </summary>
    public bool IsActive
    {
      get { return this.State != FirebaseState.Inactive; }
    }

    /// <summary>
    /// Gets whether this link is between friendly towns.
    /// </summary>
    public bool IsFriendly
    {
      get
      {
        return Misc.AreFriendly( this.cpA.Owner, this.cpB.Owner );
      }
    }

    /// <summary>
    /// Gets whether this link has firebases (not necessarily active).
    /// </summary>
    public bool HasFirebases
    {
      get { return this.fbA != null && this.fbB != null; }
    }

    /// <summary>
    /// Gets the currently open firebase, otherwise null.
    /// </summary>
    public Firebase OpenFirebase
    {
      get
      {
        if( !this.HasFirebases ) return null;
        if( !this.IsActive     ) return null;
        if( this.fbA.IsOpen    ) return this.fbA;
        if( this.fbB.IsOpen    ) return this.fbB;

        return null;
      }
    }

    /// <summary>
    /// Gets the current state of this link.
    /// </summary>
    public FirebaseState State
    {
      get
      {
        if( this.cpA.Owner.Abbr == "NF" || this.cpB.Owner.Abbr == "NF" )
          return FirebaseState.Inactive;  // no fbs if either side is netural friendly
        if( this.cpA.Owner.Side != this.cpA.Controller.Side && this.cpB.Owner.Side != this.cpB.Controller.Side )
          return FirebaseState.Inactive;  // if both towns have lost control, spawning is impossible, FB uneccessary
        if( this.cpA.Owner.Side != this.cpB.Owner.Side )
          return FirebaseState.Offensive;
        if( this.cpA.Controller.Side != this.cpB.Controller.Side )
          return FirebaseState.Defensive;

        // has same owner/controller

        if( this.cpA.HasArmyBrigade && this.cpB.IsFrontline && this.cpB.DeployedHCUnits.Count == 0 )
          return FirebaseState.Brigade;
        if( this.cpB.HasArmyBrigade && this.cpA.IsFrontline && this.cpA.DeployedHCUnits.Count == 0 )
          return FirebaseState.Brigade;

        return FirebaseState.Inactive;
      }
    }

    /// <summary>
    /// Gets the country that currently owns the Firebase pair, or Country.None if inactive.
    /// </summary>
    /// <remarks>Dynamically generated; the Wiretap owner is ignored.</remarks>
    public Country Owner
    {
      get
      {
        if( this.State == FirebaseState.Inactive || this.OpenFirebase == null )
          return Country.None;


        /*  OFFENSIVE FIREBASE
         * 
         *  friendly cp           enemy cp
         *  (A)---[ ]------------[ ]---(B)
         *                        ^ friendly fb (owner = A)
         *         ^ enemy fb (owner = B)
         */

        if( this.State == FirebaseState.Offensive )  // different owners
          return this.OpenFirebase.ChokePoint.Owner;


        /*  DEFENSIVE FIREBASE
         * 
         *  friendly cp        friendly cp (enemy controlled)
         *  (A)---[ ]------------[ ]---(B)
         *                        ^ friendly fb (owner = A)
         *         ^ enemy fb (owner = B)
         */

        if( this.State == FirebaseState.Defensive )  // different controllers
          return this.OpenFirebase.ChokePoint.Controller;


        /*  BRIGADE FIREBASE
         * 
         *  friendly cp      friendly cp (frontline)       enemy cp
         *  (A)---[ ]------------[ ]---(B)----------------------(C)
         *                        ^ friendly fb (owner = B)
         *         ^ enemy fb (owner = C)
         */

        Firebase friendlyFirebase;
        if( this.ChokePointA.HasArmyBrigade && this.ChokePointB.IsFrontline && this.ChokePointB.DeployedHCUnits.Count == 0 )
          friendlyFirebase = this.fbA;
        else
          friendlyFirebase = this.fbB;

        ChokePoint frontlineCP = friendlyFirebase.LinkedChokePoint;
        if( friendlyFirebase.IsOpen )  // owner is (B)
          return frontlineCP.Owner;
        else  // owner is (C)
          return frontlineCP.LinkedChokePoints.First( cp => cp.Owner.Side != frontlineCP.Owner.Side ).Owner;
      }
    }

    /// <summary>
    /// Gets the country that currently *does not* owns the Firebase pair (aka, the owner of the inactive fb).
    /// </summary>
    /// <remarks>Dynamically generated; the Wiretap owner is ignored.</remarks>
    public Country NonOwner
    {
      get
      {
        if( this.State == FirebaseState.Inactive || this.OpenFirebase == null )
          return Country.None;


        /*  OFFENSIVE FIREBASE
         * 
         *  friendly cp           enemy cp
         *  (A)---[ ]------------[ ]---(B)
         *                        ^ friendly fb (nonowner = B)
         *         ^ enemy fb (nonowner = A)
         */

        if( this.State == FirebaseState.Offensive )  // different owners
          return this.OpenFirebase.LinkedChokePoint.Owner;


        /*  DEFENSIVE FIREBASE
         * 
         *  friendly cp        friendly cp (enemy controlled)
         *  (A)---[ ]------------[ ]---(B)
         *                        ^ friendly fb (nonowner = B)
         *         ^ enemy fb (nonowner = A)
         */

        if( this.State == FirebaseState.Defensive )  // different controllers
          return this.OpenFirebase.LinkedChokePoint.Controller;


        /*  BRIGADE FIREBASE
         * 
         *  friendly cp      friendly cp (frontline)       enemy cp
         *  (A)---[ ]------------[ ]---(B)----------------------(C)
         *                        ^ friendly fb (nonowner = C)
         *         ^ enemy fb (nonowner = B)
         */

        Firebase friendlyFirebase;
        if( this.ChokePointA.HasArmyBrigade && this.ChokePointB.IsFrontline && this.ChokePointB.DeployedHCUnits.Count == 0 )
          friendlyFirebase = this.fbA;
        else
          friendlyFirebase = this.fbB;

        ChokePoint frontlineCP = friendlyFirebase.LinkedChokePoint;
        if( friendlyFirebase.IsOpen )  // nonowner is (C)
          return frontlineCP.LinkedChokePoints.First( cp => cp.Owner.Side != frontlineCP.Owner.Side ).Owner;
        else  // nonowner is (B)
          return frontlineCP.Owner;
      }
    }

    /// <summary>
    /// Gets or sets the side that owns the Firebase pair.
    /// </summary>
    /// <remarks>Set to Side.None to toggle.</remarks>
    public Side Side
    {
      get
      {
        return this.Owner.Side;
      }
      set
      {
        if( value == this.Side || value == Side.Neutral ) return;

        this.fbA.IsOpen = !this.fbA.IsOpen;
      }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Get the firebase away from the given cp.
    /// </summary>
    /// <param name="cp">The cp on one end of the link.</param>
    /// <returns>The far Firebase.</returns>
    public Firebase GetFirebaseFrom( ChokePoint cp )
    {
      if( cp == this.cpA ) return this.fbA;
      if( cp == this.cpB ) return this.fbB;
      throw new ArgumentException();
    }

    /// <summary>
    /// Get the firebase near to the given cp.
    /// </summary>
    /// <param name="cp">The cp on one end of the link.</param>
    /// <returns>The near Firebase.</returns>
    public Firebase GetFirebaseTo( ChokePoint cp )
    {
      if( cp == this.cpA ) return this.fbB;
      if( cp == this.cpB ) return this.fbA;
      throw new ArgumentException();
    }

    /// <summary>
    /// Remove any circular references to allow garbage collection.
    /// </summary>
    public void Cleanup()
    {
      this.cpA = null;
      this.cpB = null;
      this.depotA = null;
      this.depotB = null;
      this.fbA = null;
      this.fbB = null;
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Provides a string representation of this object.
    /// </summary>
    /// <returns>"cpA - cpB"</returns>
    public override string ToString()
    {
      return String.Format( "{0} - {1}", this.ChokePointA, this.ChokePointB );
    }

    /// <summary>
    /// Returns a value indicating whether this instance is equal to a specified object value.
    /// </summary>
    /// <param name="obj">An object to compare with this instance.</param>
    /// <returns>True if obj is the same SupplyLink.</returns>
    public override bool Equals( object obj )
    {
      return obj is SupplyLink && this.ID == ( (SupplyLink)obj ).ID;
    }

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    /// <returns>The Facility id.</returns>
    public override int GetHashCode()
    {
      return this.ID;
    }

    /// <summary>
    /// Support for the == operator.
    /// </summary>
    public static bool operator ==( SupplyLink a, SupplyLink b )
    {
      if( (object)a == null ) return (object)b == null;
      return a.Equals( b );
    }

    /// <summary>
    /// Support for the != operator.
    /// </summary>
    public static bool operator !=( SupplyLink a, SupplyLink b )
    {
      if( (object)a == null ) return (object)b != null;
      return !a.Equals( b );
    }

    /// <summary>
    /// Compare this object with another SupplyLink object.
    /// </summary>
    /// <param name="other">The other SupplyLink to compare with.</param>
    /// <returns>-1, 0, 1</returns>
    public int CompareTo( SupplyLink other )
    {
      if( other == null ) return 1;
      return this.ID.CompareTo( other.ID );
    }

    /// <summary>
    /// Returns an enumerator that iterates through both cps in the link.
    /// </summary>
    /// <returns>
    /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
    /// </returns>
    public IEnumerator<ChokePoint> GetEnumerator()
    {
      yield return this.cpA;
      yield return this.cpB;
    }

    /// <summary>
    /// Returns an enumerator that iterates through both cps in the link.
    /// </summary>
    /// <returns>
    /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
    /// </returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #endregion
  }


  /// <summary>
  /// A Facility is a captureable building within a ChokePoint. Only city-type facilities
  /// use this base class directly; the others have more specialised versions.
  /// </summary>
  /// <remarks>Doesn't include Firebases</remarks>
  public class Facility : IComparable<Facility>
  {
    #region Variables

    private readonly int id;
    private readonly string name;
    private readonly string rawName;
    private ChokePoint cp;
    private readonly FacilityType type;
    private readonly Point location;

    private Country owner;

    private DateTime lastOwnerChanged;
    private string lastCappedPlayer;

    #endregion

    #region Constructors

    /// <summary>
    /// Create a new Facility.
    /// </summary>
    /// <param name="id">The unique Facility id.</param>
    /// <param name="name">The name of the Facility.</param>
    /// <param name="cp">The ChokePoint this Facility belongs to.</param>
    /// <param name="type">The type of Facility.</param>
    /// <param name="location">The location of the Facility in game world meters.</param>
    /// <param name="rawName">The name of the Facility (direct from wiretap, no 'cleanup').</param>
    public Facility( int id, string name, ChokePoint cp, FacilityType type, Point location, string rawName = null )
    {
      this.id = id;
      this.name = name;
      this.rawName = String.IsNullOrEmpty( rawName ) ? name : rawName;
      this.cp = cp;
      this.type = type;
      this.location = location;

      this.owner = cp.Owner;

      this.lastOwnerChanged = DateTime.MinValue;
      this.lastCappedPlayer = null;

      cp.AddFacility( this );
    }

    #endregion

    #region Properties

    /// <summary>
    /// The unique Facility identifier.
    /// </summary>
    public int ID
    {
      get { return this.id; }
    }

    /// <summary>
    /// The Facility name.
    /// </summary>
    public string Name
    {
      get { return this.name; }
    }

    /// <summary>
    /// The raw Facility name (direct from wiretap, no 'cleanup').
    /// </summary>
    public string RawName
    {
      get { return this.rawName; }
    }

    /// <summary>
    /// The ChokePoint this Facility belongs to.
    /// </summary>
    public ChokePoint ChokePoint
    {
      get { return this.cp; }
    }

    /// <summary>
    /// The type of this Facility (eg, Armybase, Depot, City, ...)
    /// </summary>
    public FacilityType Type
    {
      get { return this.type; }
    }

    /// <summary>
    /// The coordinates of the Facility on the map in game meters.
    /// </summary>
    public Point Location
    {
      get { return this.location; }
    }

    /// <summary>
    /// The coordinates of the Facility on the map in game meters, including DrawOffset.
    /// </summary>
    public Point DrawLocation
    {
      get
      {
        return this.location + this.DrawOffset;
      }
    }

    /// <summary>
    /// An x,y offset to draw this facility in a better location (to avoid overlapping facilities).
    /// </summary>
    public Size DrawOffset { get; set; }

    /// <summary>
    /// Gets or sets the Country that currently owns this Facility.
    /// </summary>
    /// <remarks>Doesn't raise events, use SetCaptured() instead.</remarks>
    public Country Owner
    {
      get { return this.owner; }
      set { this.owner = value; }
    }

    /// <summary>
    /// The last time the Facility changed ownership.
    /// </summary>
    public DateTime LastOwnerChanged
    {
      get { return this.lastOwnerChanged; }
    }

    /// <summary>
    /// The name of the last player to capture this Facility.
    /// </summary>
    public string LastCapper
    {
      get { return this.lastCappedPlayer; }
    }

    /// <summary>
    /// Get the facility icon associated with this FacilityType.
    /// </summary>
    public Image Icon
    {
      get
      {
        switch( this.type )
        {
          case FacilityType.City:
            return Resources.facility_city;
          case FacilityType.Factory:
            return Resources.facility_factory;
          case FacilityType.Depot:
            return Resources.facility_depot;
          case FacilityType.Firebase:
            return Resources.facility_firebase;
          case FacilityType.Airbase:
            return Resources.facility_airbase;
          case FacilityType.Armybase:
            return Resources.facility_armybase;
          case FacilityType.Navalbase:
            return Resources.facility_navalbase;
          default:
            return new Bitmap( 32, 26 );  // empty image
        }
      }
    }

    /// <summary>
    /// True is this Facility's capture table has been fully built (ie, radio up &amp; captureable).
    /// </summary>
    public bool RadioTableUp
    {
      get
      {
        if( this is MilitaryFacility )
          return ( (MilitaryFacility)this ).RadioTableUp;

        if( !this.cp.HasAO )
          return false;  // no Attack Objective
        if( this.cp.LastAOChanged > DateTime.Now.AddMinutes( -10 ) )
          return false;  // AO placed less than 10 mins ago
        if( this.LastOwnerChanged > DateTime.Now.AddMinutes( -1 ) )
          return false;  // capped in past minute

        return true;
      }
    }

    /// <summary>
    /// True if this type of Facility can be spawned from.
    /// </summary>
    public bool SpawnableFacility
    {
      get
      {
        switch( this.type )
        {
          case FacilityType.Depot:
          case FacilityType.Airbase:
          case FacilityType.Armybase:
          case FacilityType.Navalbase:
            return true;
          default:
            return false;
        }
      }
    }

    /// <summary>
    /// True if the current owner of this Facility is allowed to spawn.
    /// </summary>
    public bool OwnerCanSpawn
    {
      get
      {
        /*  leave this out as it may be confusing
        if( this.lastOwnerChanged > DateTime.Now.AddSeconds( 90 ) )
          return false;  // not spawnable for 90 seconds after cap
        */

        if( this is MilitaryFacility )
          return ( (MilitaryFacility)this ).OwnerCanSpawn;

        if( this is Depot )
          return ( (Depot)this ).OwnerCanSpawn;

        return false;  // non-spawnable facility type
      }
    }

    /// <summary>
    /// True if this Facility is a Depot that satisfies the conditions that, if captured,
    /// will be spawnable by the enemy.
    /// </summary>
    public bool EnemySpawnable
    {
      get
      {
        if( this is Depot )
          return ( (Depot)this ).EnemySpawnable;

        return false;  // not a depot
      }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Set the initial owner of this Facility.
    /// </summary>
    /// <param name="owner">The Country that currently owns the Facility.</param>
    public void SetInitState( Country owner )
    {
      this.owner = owner;
    }

    /// <summary>
    /// Applies a capture event to a Facility and updates the state of game objects.
    /// </summary>
    /// <remarks>Raises the relevant captured/under attack/regained/now own/control/etc game events.</remarks>
    /// <param name="time">The time the capture event occurred.</param>
    /// <param name="prevOwner">The previous owning Country.</param>
    /// <param name="newOwner">The new owning Country.</param>
    /// <param name="by">The name of the player that made the capture.</param>
    /// <param name="hcunit">The hcunit the player's mission originated from.</param>
    /// <param name="nowContested">True if the parent ChokePoint is now contested.</param>
    /// <param name="nowUncontested">True if the parent ChokePoint is no longer contested.</param>
    /// <param name="newCPController">True if the parent ChokePoint's controller has changed.</param>
    /// <param name="newCPOwner">True if the parent ChokePoint's owner has changed (ie, captured).</param>
    /// <param name="init">True if generating historical events (will not auto-remove AOs or HCUnits).</param>
    public void SetCaptured( DateTime time, Country prevOwner, Country newOwner, string by, HCUnit hcunit,
                             bool nowContested, bool nowUncontested, bool newCPController, bool newCPOwner, bool init )
    {
      // cp under attack

      if( nowContested )
      {
        this.cp.Uncontest( prevOwner );  // safety check, make sure cp was uncontested

        if( this.cp.EventTracker != null )
          this.cp.EventTracker.Add( new ChokePointUnderAttackGameEvent( time, this.cp ) );
        this.cp.LastContestedChanged = time;
      }


      // facility captured

      this.owner = newOwner;
      this.lastOwnerChanged = time;
      this.lastCappedPlayer = by;


      // raise event

      if( this.cp.EventTracker != null )
      {
        // only raise spawnable event if we are a spawnable depot and is now/was spawnable
        if( this.EnemySpawnable && ( ( (Depot)this ).LinkedDepot.Owner.Side == newOwner.Side || ( (Depot)this ).LinkedDepot.Owner.Side == prevOwner.Side ) )
          this.cp.EventTracker.Add( new SpawnableDepotCapturedGameEvent( time, (Depot)this, newOwner, prevOwner, by ) );
        else
          this.cp.EventTracker.Add( new FacilityCapturedGameEvent( time, this, newOwner, prevOwner, by ) );
      }


      // manually kick out any brigades (event will arrive 10 mins later)

      if( !init )  // don't do during init
      {
        foreach( Branch branch in Enum.GetValues( typeof( Branch ) ) )
        {
          if( branch == Branch.None ) continue;

          if( this.cp.AllBasesCaptured( branch ) )
            this.cp.UndeployBranch( branch );
        }
      }


      // new cp owner/controller

      if( newCPOwner )
        this.cp.SetNewOwner( time, newOwner );

      if( newCPController )
        this.cp.SetNewController( time, newOwner, !newCPOwner && !nowUncontested );


      // cp regained

      if( nowUncontested && !newCPOwner )
      {
        if( this.cp.EventTracker != null )
          this.cp.EventTracker.Add( new ChokePointRegainedGameEvent( time, this.cp, this.cp.LastContestedChanged ) );
        this.cp.LastContestedChanged = time;
      }


      // if uncontested, clear last capped times (facilities done within Uncontest())

      if( nowUncontested )
      {
        this.cp.Uncontest( newOwner );  // safety check, make sure cp now uncontested
        this.cp.LastControllerChanged = DateTime.MinValue;
      }


      // if any fb's now inactive, set state to closed

      if( newCPOwner || newCPController )
      {
        foreach( Firebase fb in this.cp.Firebases )
          if( fb.Link.State == FirebaseState.Inactive )
            fb.IsOpen = false;  // will set both
      }


      // silently remove AO after cp capture or when regained by recapping ab last

      if( nowUncontested && ( newCPOwner || this.type == FacilityType.Armybase ) && !init )  // don't do during init
      {
        if( this.cp.HasAO )
          this.cp.HasAO = false;
      }


      // add capturefrom data point

      if( newOwner.Side != this.cp.Owner.Side )  // enemy cap, not a recap
        this.cp.AddCaptureFrom( time, hcunit.DeployedChokePoint );
    }

    /// <summary>
    /// Resets the last capped time/player/squad for this Facility.
    /// </summary>
    public void ResetCaptureTime()
    {
      this.lastOwnerChanged = DateTime.MinValue;
      this.lastCappedPlayer = null;
    }

    /// <summary>
    /// Remove any circular references to allow garbage collection.
    /// </summary>
    public void Cleanup()
    {
      this.cp = null;
      this.owner = null;

      if( this is Depot )
        ( (Depot)this ).Cleanup();
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Provides a string representation of this object.
    /// </summary>
    /// <returns>The Facility name.</returns>
    public override string ToString()
    {
      return this.name;
    }

    /// <summary>
    /// Returns a value indicating whether this instance is equal to a specified object value.
    /// </summary>
    /// <param name="obj">An object to compare with this instance.</param>
    /// <returns>True if obj is the same Facility.</returns>
    public override bool Equals( object obj )
    {
      return obj is Facility && this.id == ( (Facility)obj ).id;
    }

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    /// <returns>The Facility id.</returns>
    public override int GetHashCode()
    {
      return this.id;
    }

    /// <summary>
    /// Support for the == operator.
    /// </summary>
    public static bool operator ==( Facility a, Facility b )
    {
      if( (object)a == null ) return (object)b == null;
      return a.Equals( b );
    }

    /// <summary>
    /// Support for the != operator.
    /// </summary>
    public static bool operator !=( Facility a, Facility b )
    {
      if( (object)a == null ) return (object)b != null;
      return !a.Equals( b );
    }

    /// <summary>
    /// Compare this object with another Facility object.
    /// </summary>
    /// <param name="other">The other Facility to compare with.</param>
    /// <returns>-1, 0, 1</returns>
    public int CompareTo( Facility other )
    {
      if( other == null ) return 1;
      return this.name.CompareTo( other.name );
    }

    #endregion
  }


  /// <summary>
  /// A MilitaryFacility is either an Armybase, Airbase (airfield), or Navalbase (docks).
  /// They are different from normal Facilitys in their capture &amp; spawning behaviour.
  /// </summary>
  public class MilitaryFacility : Facility, IComparable<MilitaryFacility>
  {
    #region Constructors

    /// <summary>
    /// Create a new MilitaryFacility (ab/af/docks).
    /// </summary>
    /// <param name="id">The unique Facility id.</param>
    /// <param name="name">The name of the MilitaryFacility.</param>
    /// <param name="cp">The ChokePoint this MilitaryFacility belongs to.</param>
    /// <param name="type">The type of Facility (Airbase/Armybase/Navalbase).</param>
    /// <param name="location">The location of the MilitaryFacility in game world meters.</param>
    /// <param name="rawName">The name of the MilitaryFacility (direct from wiretap, no 'cleanup').</param>
    public MilitaryFacility( int id, string name, ChokePoint cp, FacilityType type, Point location, string rawName = null )
      : base( id, name, cp, type, location, rawName )
    {

    }

    #endregion

    #region Properties

    /// <summary>
    /// True is this MilitaryFacility's capture table has been fully built (ie, radio up
    /// &amp; captureable).
    /// </summary>
    public new bool RadioTableUp
    {
      get
      {
        if( !this.ChokePoint.IsContested )
          return false;  // not contested
        if( this.ChokePoint.LastContestedChanged > DateTime.Now.AddMinutes( -10 ) )
          return false;  // contested for less than 10 mins
        if( this.LastOwnerChanged > DateTime.Now.AddMinutes( -1 ) )
          return false;  // capped in past minute

        return true;
      }
    }

    /// <summary>
    /// True if the current owner of this MilitaryFacility is allowed to spawn.
    /// </summary>
    public new bool OwnerCanSpawn
    {
      get
      {
        if( this.Owner != this.ChokePoint.Owner || this.Owner.Side != this.ChokePoint.Controller.Side )
          return false;  // must own & control
        if( this.ChokePoint.DeployedHCUnits.Count == 0 )
          return false;  // no brigade

        return true;
      }
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Compare this object with another MilitaryFacility object.
    /// </summary>
    /// <param name="other">The other MilitaryFacility to compare with.</param>
    /// <returns>-1, 0, 1</returns>
    public int CompareTo( MilitaryFacility other )
    {
      if( other == null ) return 1;
      return base.CompareTo( other );
    }

    #endregion
  }


  /// <summary>
  /// A Depot is a specialised type of Facility that can have a link to another ChokePoint
  /// with Firebases in between. It also allows spawning in an enemy town if the appropriate
  /// conditions are met.
  /// </summary>
  public class Depot : Facility, IComparable<Depot>
  {
    #region Variables

    private Depot linkedDepot;
    private Firebase firebase;

    #endregion

    #region Constructors

    /// <summary>
    /// Create a new Depot.
    /// </summary>
    /// <param name="id">The unique Facility id.</param>
    /// <param name="name">The name of the Depot.</param>
    /// <param name="cp">The ChokePoint this Depot belongs to.</param>
    /// <param name="type">FacilityType.Depot</param>
    /// <param name="location">The location of the Depot in game world meters.</param>
    /// <param name="rawName">The name of the Depot (direct from wiretap, no 'cleanup').</param>
    public Depot( int id, string name, ChokePoint cp, FacilityType type, Point location, string rawName = null )
      : base( id, name, cp, type, location, rawName )
    {

    }

    #endregion

    #region Properties

    /// <summary>
    /// If this Depot is linked, the Depot on the other end of the link.
    /// </summary>
    public Depot LinkedDepot
    {
      get { return this.linkedDepot; }
    }

    /// <summary>
    /// If this Depot is linked, the outbound Firebase on the link (if any).
    /// </summary>
    public Firebase Firebase
    {
      get { return this.firebase; }
    }

    /// <summary>
    /// True if the current owner of this Depot is allowed to spawn.
    /// </summary>
    public new bool OwnerCanSpawn
    {
      get
      {
        if( this.Owner.Side == this.ChokePoint.Owner.Side )  // in friendly town
        {
          if( this.ChokePoint.DeployedHCUnits.Count == 0 )  // no brigade
          {
            // still spawnable if depot is linked to brigade and cp has AO

            if( !this.ChokePoint.HasAO )
              return false;  // no AO
            if( this.linkedDepot == null )
              return false;  // not linked
            if( this.linkedDepot.Owner.Side != this.Owner.Side )
              return false;  // linked depot not same owner
            if( this.linkedDepot.ChokePoint.Owner.Side != this.Owner.Side )
              return false;  // linked chokepoint not same owner
            if( !this.linkedDepot.ChokePoint.HasSpawnableHCUnit )
              return false;  // no brigade in linked cp
          }
        }
        else  // in enemy town
        {
          if( !this.EnemySpawnable )
            return false;  // not a "spawnable" depot
          if( this.Owner.Side != this.linkedDepot.Owner.Side )
            return false;  // not owned by enemy
        }

        return true;
      }
    }

    /// <summary>
    /// True if this Depot satisfies the conditions that, if captured,
    /// will be spawnable by the enemy.
    /// </summary>
    public new bool EnemySpawnable
    {
      get
      {
        if( this.linkedDepot == null )
          return false;  // not linked
        if( this.linkedDepot.ChokePoint.Owner.Side == this.ChokePoint.Owner.Side )
          return false;  // not in enemy town
        if( this.linkedDepot.Owner.Side != this.linkedDepot.ChokePoint.Owner.Side )
          return false;  // linked depot captured (in enemy town)
        if( !this.linkedDepot.ChokePoint.HasSpawnableHCUnit )
          return false;  // no bridage in linked cp
        if( this.firebase != null && this.firebase.IsOpen )
          return false;  // outbound (enemy) fb is open

        return true;
      }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Create a link between this Depot and another, optionally with a Firebase.
    /// </summary>
    /// <param name="linkedDepot">The Depot on the other end of the link.</param>
    /// <param name="firebase">The outbound Firebase on the link (may be null).</param>
    public void AddLink( Depot linkedDepot, Firebase firebase )
    {
      this.linkedDepot = linkedDepot;
      this.firebase = firebase;

      if( firebase != null )
        firebase.AddDepots( this, linkedDepot );
    }

    /// <summary>
    /// Remove any circular references to allow garbage collection.
    /// </summary>
    public new void Cleanup()
    {
      this.linkedDepot = null;
      this.firebase = null;
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Compare this object with another Depot object.
    /// </summary>
    /// <param name="other">The other Depot to compare with.</param>
    /// <returns>-1, 0, 1</returns>
    public int CompareTo( Depot other )
    {
      if( other == null ) return 1;
      return base.CompareTo( other );
    }

    #endregion
  }


  /// <summary>
  /// A Factory is a specialised type of Facility that produces resource points that go
  /// towards completing an RDP cycle.
  /// </summary>
  /// <remarks>This doesn't include all FacilityType.Factory's.</remarks>
  public class Factory : Facility, IComparable<Factory>
  {
    #region Variables

    private DateTime firstTickStamp;
    private DateTime lastTickStamp;
    private int ticksTotal = 0;
    private int ticksProduced = 0;
    private List<FactoryTick> ticks;

    #endregion

    #region Constructors

    /// <summary>
    /// Create a new Factory.
    /// </summary>
    /// <param name="id">The unique Facility id.</param>
    /// <param name="name">The name of the Factory.</param>
    /// <param name="cp">The ChokePoint this Factory belongs to.</param>
    /// <param name="type">FacilityType.Factory</param>
    /// <param name="location">The location of the Factory in game world meters.</param>
    /// <param name="rawName">The name of the Factory (direct from wiretap, no 'cleanup').</param>
    public Factory( int id, string name, ChokePoint cp, FacilityType type, Point location, string rawName = null )
      : base( id, name, cp, type, location, rawName )
    {
      ticks = new List<FactoryTick>( 96 );  // 4 ticks p/ hour x 24 hours
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets the total number of RDP points produced by this factory this campaign.
    /// </summary>
    public int TotalProduced
    {
      get { return this.ticksProduced; }
    }

    /// <summary>
    /// A list of (up to) the past 24-hours worth of factory ticks (96).
    /// </summary>
    public ReadOnlyCollection<FactoryTick> Ticks
    {
      get { return this.ticks.AsReadOnly(); }
    }

    /// <summary>
    /// The current health of the Factory (0 = damaged, 100 = healthy).
    /// </summary>
    public int CurrentHealth
    {
      get
      {
        if( this.ticks.Count > 0 )
          return 100 - this.ticks[this.ticks.Count - 1].Damage;
        else
          return 100;
      }
    }

    /// <summary>
    /// If the Factory is currently contributing to RDP.
    /// </summary>
    public bool CurrentRDP
    {
      get
      {
        if( this.ticks.Count > 0 )
          return this.ticks[this.ticks.Count - 1].RDP == 1;
        else
          return true;
      }
    }

    /// <summary>
    /// The percent of total possible output this Factory has produced this campaign.
    /// </summary>
    public int OutputPercentCampaign
    {
      get
      {
        if( ticksTotal == 0 )
          return 0;
        else
          return (int)( ( (float)ticksProduced / (float)ticksTotal ) * 100 );
      }
    }

    /// <summary>
    /// The percent of total possible output this Factory has produced for the past day.
    /// </summary>
    public int OutputPercentPastDay
    {
      get
      {
        if( this.ticks.Count == 0 )
          return 0;

        int produced = 0;

        foreach( FactoryTick tick in this.ticks )
          produced += tick.RDP;

        return (int)( ( (float)produced / (float)this.ticks.Count ) * 100 );
      }
    }

    /// <summary>
    /// The Country this factory belongs to (not necessarily its current owner).
    /// </summary>
    public Country Country
    {
      get { return this.ChokePoint.OriginalCountry; }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Set the initial state of this Factory.
    /// </summary>
    /// <param name="firstTick">The date/time of the first factory tick of the campaign.</param>
    /// <param name="lastTick">The date/time of the latest factory tick.</param>
    /// <param name="ticksTotal">The total number of ticks so far this campaign.</param>
    /// <param name="ticksProduced">The total number of ticks of which this Factory contributed to RDP.</param>
    public void SetInitState( DateTime firstTick, DateTime lastTick, int ticksTotal, int ticksProduced )
    {
      this.firstTickStamp = firstTick;
      this.lastTickStamp = lastTick;
      this.ticksTotal = ticksTotal;
      this.ticksProduced = ticksProduced;
    }

    /// <summary>
    /// Add the results of a new factory tick.
    /// </summary>
    /// <param name="tick">The FactoryTick to add.</param>
    public void AddTick( FactoryTick tick )
    {
      ticks.Add( tick );

      while( ticks.Count > 96 )
        ticks.RemoveAt( 0 );

      if( tick.TimeStamp > this.lastTickStamp )  // tick is newer than lasttick in /xml/factorystats.xml
      {
        this.ticksTotal++;
        this.ticksProduced += tick.RDP;
        this.lastTickStamp = tick.TimeStamp;
      }


      // raise events

      if( this.ChokePoint.EventTracker != null && this.ticks.Count > 1 )  // prev tick to compare against
      {
        FactoryTick prevTick = this.ticks[this.ticks.Count - 2];

        if( tick.Damage - 20 >= prevTick.Damage )  // damage has increased by 20% or more in the past tick
        {
          if( tick.Damage == 100 )
            this.ChokePoint.EventTracker.Add( new FactoryDestroyedGameEvent( tick.TimeStamp, this ) );
          else
            this.ChokePoint.EventTracker.Add( new FactoryDamagedGameEvent( tick.TimeStamp, this, prevTick.Damage, tick.Damage ) );
        }

        if( prevTick.Damage > 0 && tick.Damage == 0 )
        {
          // only add event if health has been below 80%
          bool belowThreshold = false;
          for( int i = ticks.Count - 2; i >= 0; i-- )  // reverse, from prevTick
          {
            if( ticks[i].Damage == 0 ) break;  // healthy
            if( ticks[i].Damage > 20 )
            {
              belowThreshold = true;
              break;
            }
          }

          if( belowThreshold )
            this.ChokePoint.EventTracker.Add( new FactoryHealthyGameEvent( tick.TimeStamp, this ) );
        }

        if( prevTick.RDP != tick.RDP )
          this.ChokePoint.EventTracker.Add( new FactoryRDPChangedGameEvent( tick.TimeStamp, this, tick.RDP ) );
      }
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Compare this object with another Factory object.
    /// </summary>
    /// <param name="other">The other Factory to compare with.</param>
    /// <returns>-1, 0, 1</returns>
    public int CompareTo( Factory other )
    {
      if( other == null ) return 1;
      return base.CompareTo( other );
    }

    #endregion
  }


  /// <summary>
  /// A Firebase (or forward base) is a semi-permanent spawnable camp located on the
  /// outskirts of an enemy town (offensive), or a friendly town (defensive).
  /// </summary>
  public class Firebase : IComparable<Firebase>
  {
    #region Variables

    private readonly int id;
    private readonly string name;
    private readonly string rawName;
    private ChokePoint cp;
    private readonly Point location;

    private SupplyLink link;
    private Depot depot;
    private Depot linkedDepot;
    private bool open;

    #endregion

    #region Constructors

    /// <summary>
    /// Create a new Firebase.
    /// </summary>
    /// <param name="id">The unique Facility id.</param>
    /// <param name="name">The name of the Firebase.</param>
    /// <param name="cp">The ChokePoint this Firebase belongs to.</param>
    /// <param name="location">The location of the Firebase in game world meters.</param>
    /// <param name="rawName">The name of the Firebase (direct from wiretap, no 'cleanup').</param>
    public Firebase( int id, string name, ChokePoint cp, Point location, string rawName = null )
    {
      this.id = id;
      this.name = name;
      this.rawName = String.IsNullOrEmpty( rawName ) ? name : rawName;
      this.cp = cp;
      this.location = location;

      cp.AddFirebase( this );
    }

    #endregion

    #region Properties

    /// <summary>
    /// The unique facility identifier.
    /// </summary>
    public int ID
    {
      get { return this.id; }
    }

    /// <summary>
    /// The Firebase name.
    /// </summary>
    public string Name
    {
      get { return this.name; }
    }

    /// <summary>
    /// The raw Firebase name (direct from wiretap, no 'cleanup').
    /// </summary>
    public string RawName
    {
      get { return this.rawName; }
    }

    /// <summary>
    /// The ChokePoint this Firebase belongs to (is furthest from).
    /// </summary>
    public ChokePoint ChokePoint
    {
      get { return this.cp; }
    }

    /// <summary>
    /// The ChokePoint this Firebase is located near.
    /// </summary>
    public ChokePoint LinkedChokePoint
    {
      get { return this.linkedDepot.ChokePoint; }
    }

    /// <summary>
    /// The Depot on the local side of the link (far).
    /// </summary>
    public Depot Depot
    {
      get { return this.depot; }
    }

    /// <summary>
    /// The Depot on the remote side of the link (near).
    /// </summary>
    public Depot LinkedDepot
    {
      get { return this.linkedDepot; }
    }

    /// <summary>
    /// The coordinates of the Firebase on the map.
    /// </summary>
    public Point Location
    {
      get { return this.location; }
    }

    /// <summary>
    /// The equivalent partner to this Firebase.
    /// </summary>
    public Firebase LinkedFirebase
    {
      get { return this.linkedDepot.Firebase; }
    }

    /// <summary>
    /// Sets or gets the current state of this Firebase (also updates the linked fb).
    /// </summary>
    public bool IsOpen
    {
      get { return this.Link.State != FirebaseState.Inactive && this.open; }
      set
      {
        this.open = value;

        if( this.Link.State == FirebaseState.Inactive )
          this.LinkedFirebase.open = false;
        else
          this.LinkedFirebase.open = !value;
      }
    }

    /// <summary>
    /// Gets the cp link object for this firebase.
    /// </summary>
    public SupplyLink Link
    {
      get { return this.link; }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Associate the two local/remote Depots with this Firebase.
    /// </summary>
    /// <param name="depot">The local Depot on the link (furthest from).</param>
    /// <param name="linkedDepot">The remote Depot on the link (closest to).</param>
    public void AddDepots( Depot depot, Depot linkedDepot )
    {
      this.depot = depot;
      this.linkedDepot = linkedDepot;
    }

    /// <summary>
    /// Associate the given cp link with this firebase.
    /// </summary>
    /// <param name="link">The SupplyLink this fb is a member of.</param>
    public void AddLink( SupplyLink link )
    {
      this.link = link;
    }

    /// <summary>
    /// Set a new Firebase state/owner and raise the appropriate event. Also updates the linked firebase.
    /// </summary>
    /// <param name="open">True is this fb is now open, false to close.</param>
    /// <param name="owner">The owner of the newly open firebase (ignored when closing a fb).</param>
    public void SetNewState( bool open, Country owner )
    {
      // skip trying to set an inactive fb pair to open (occurs when brigade fb appears but
      // we havn't received the hcunit move event yet)

      if( open && this.Link.State == FirebaseState.Inactive )
        return;


      // if setting this fb to open, record wether partner was open

      bool partnerBlown = open && this.LinkedFirebase.open;


      // update state of fb pair

      this.IsOpen = open;


      // raise event

      if( this.ChokePoint.EventTracker == null ) return;

      if( this.Link.State == FirebaseState.Brigade && !partnerBlown )  // raise new brigade firebase event
      {
        this.ChokePoint.EventTracker.Add( new NewBrigadeFirebaseGameEvent( DateTime.Now, open ? this : this.LinkedFirebase ) );
        return;
      }


      // skip raising event if fb change was automatic (ie, not player-blown)

      if( !partnerBlown )
        return;  // fb appeared automatically (partner was already closed)

      switch( this.Link.State )
      {
        case FirebaseState.Inactive:
          return;  // fb pair has become inactive due to enemy town being captured

        case FirebaseState.Offensive:
          if( this.LinkedChokePoint.LastOwnerChanged > DateTime.Now.AddMinutes( -3 ) )
            return;  // enemy offensive fb has appeared due to town recently being captured
          else
            break;

        case FirebaseState.Defensive:
          if( this.LinkedChokePoint.LastControllerChanged > DateTime.Now.AddMinutes( -3 ) )
            return;  // friendly defensive fb has appeared due to enemy recently taking control of town
          else
            break;
      }

      this.ChokePoint.EventTracker.Add( new FirebaseBlownGameEvent( DateTime.Now, open ? this.LinkedFirebase : this ) );
    }

    /// <summary>
    /// Remove any circular references to allow garbage collection.
    /// </summary>
    public void Cleanup()
    {
      this.cp = null;
      this.depot = null;
      this.linkedDepot = null;
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Provides a string representation of this object.
    /// </summary>
    /// <returns>The Firebase name.</returns>
    public override string ToString()
    {
      return this.name;
    }

    /// <summary>
    /// Returns a value indicating whether this instance is equal to a specified object value.
    /// </summary>
    /// <param name="obj">An object to compare with this instance.</param>
    /// <returns>True if obj is the same Firebase.</returns>
    public override bool Equals( object obj )
    {
      return obj is Firebase && this.id == ( (Firebase)obj ).id;
    }

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    /// <returns>The Firebase id.</returns>
    public override int GetHashCode()
    {
      return this.id;
    }

    /// <summary>
    /// Support for the == operator.
    /// </summary>
    public static bool operator ==( Firebase a, Firebase b )
    {
      if( (object)a == null ) return (object)b == null;
      return a.Equals( b );
    }

    /// <summary>
    /// Support for the != operator.
    /// </summary>
    public static bool operator !=( Firebase a, Firebase b )
    {
      if( (object)a == null ) return (object)b != null;
      return !a.Equals( b );
    }

    /// <summary>
    /// Compare this object with another Firebase object.
    /// </summary>
    /// <param name="other">The other Firebase to compare with.</param>
    /// <returns>-1, 0, 1</returns>
    public int CompareTo( Firebase other )
    {
      if( other == null ) return 1;
      return this.name.CompareTo( other.name );
    }

    #endregion
  }


  /// <summary>
  /// A Bridge is a river crossing that can be destroyed or repaired for strategic
  /// purposes.
  /// </summary>
  public class Bridge : IComparable<Bridge>
  {
    #region Variables

    private readonly int id;
    private readonly string name;
    private bool ao;
    private readonly Point location;

    #endregion

    #region Constructors

    /// <summary>
    /// Create a new Bridge.
    /// </summary>
    /// <param name="id">The unique Bridge id.</param>
    /// <param name="name">The name of the Bridge.</param>
    /// <param name="location">The location of the Bridge in game world octets.</param>
    public Bridge( int id, string name, Point location )
    {
      this.id = id;
      this.name = name;
      this.ao = false;
      this.location = location;
    }

    #endregion

    #region Properties

    /// <summary>
    /// The unique Bridge identifier.
    /// </summary>
    public int ID
    {
      get { return id; }
    }

    /// <summary>
    /// The ChokePoint name.
    /// </summary>
    public string Name
    {
      get { return this.name; }
    }

    /// <summary>
    /// Sets or gets whether this ChokePoint currently has an Attack Objective placed on it.
    /// </summary>
    public bool AO
    {
      get { return this.ao; }
    }

    /// <summary>
    /// The coordinates of the Bridge on the map.
    /// </summary>
    public Point Location
    {
      get { return this.location; }
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Provides a string representation of this object.
    /// </summary>
    /// <returns>The Bridge name.</returns>
    public override string ToString()
    {
      return this.name;
    }

    /// <summary>
    /// Returns a value indicating whether this instance is equal to a specified object value.
    /// </summary>
    /// <param name="obj">An object to compare with this instance.</param>
    /// <returns>True if obj is the same Bridge.</returns>
    public override bool Equals( object obj )
    {
      return obj is Bridge && this.id == ( (Bridge)obj ).id;
    }

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    /// <returns>The Bridge id.</returns>
    public override int GetHashCode()
    {
      return this.id;
    }

    /// <summary>
    /// Support for the == operator.
    /// </summary>
    public static bool operator ==( Bridge a, Bridge b )
    {
      if( (object)a == null ) return (object)b == null;
      return a.Equals( b );
    }

    /// <summary>
    /// Support for the != operator.
    /// </summary>
    public static bool operator !=( Bridge a, Bridge b )
    {
      if( (object)a == null ) return (object)b != null;
      return !a.Equals( b );
    }

    /// <summary>
    /// Compare this object with another Bridge object.
    /// </summary>
    /// <param name="other">The other Bridge to compare with.</param>
    /// <returns>-1, 0, 1</returns>
    public int CompareTo( Bridge other )
    {
      if( other == null ) return 1;
      return this.name.CompareTo( other.name );
    }

    #endregion
  }


  /// <summary>
  /// A High Command Unit is a unit in the order of battle hierarchy of each military branch.
  /// A division or one of its brigades must be deployed at a chokepoint to allow spawning.
  /// </summary>
  public class HCUnit : NotifyObject, IComparable<HCUnit>
  {
    #region Variables

    private readonly int id;
    private readonly HCUnitLevel level;
    private readonly string shortName;
    private readonly string nick;
    private readonly string title;
    private Country country;
    private readonly Branch branch;
    private MoveType moveType;
    private readonly MoveType defaultMoveType;
    private readonly string owner;
    private Toe toe;

    private HCUnit parent;
    private List<HCUnit> children;
    private readonly int unitNum;  // any leading number in title, used for sorting

    private ChokePoint deployedcp;
    private ChokePoint routedcp;
    private DateTime lastMovedTime;
    private string lastMovedPlayer;
    private DateTime nextMoveTime;

    private SortedList<DateTime,HCUnitMove> moves;

    #endregion

    #region Constructors

    /// <summary>
    /// Create a new HCUnit.
    /// </summary>
    /// <param name="id">The unique HCUnit id.</param>
    /// <param name="level">The level of the HCUnit.</param>
    /// <param name="shortName">The short name of the HCUnit.</param>
    /// <param name="nick">The short nick of the HCUnit.</param>
    /// <param name="title">The long title of the HCUnit.</param>
    /// <param name="parent">The parent HCUnit.</param>
    /// <param name="country">The Country the HCUnit belongs to.</param>
    /// <param name="branch">The Branch the HCUnit belongs to.</param>
    /// <param name="moveType">The way this HCUnit is allowed to move.</param>
    /// <param name="owner">The name of the commanding player.</param>
    /// <param name="toe">The Toe associated with this unit.</param>
    public HCUnit( int id, HCUnitLevel level, string shortName, string nick, string title, HCUnit parent,
                   Country country, Branch branch, MoveType moveType, string owner, Toe toe )
    {
      this.id = id;
      this.level = level;
      this.shortName = shortName;
      this.nick = nick;
      this.title = title;
      this.parent = parent;
      this.country = country;
      this.branch = branch;
      this.defaultMoveType = this.moveType = moveType;
      this.owner = owner;
      this.toe = toe;

      this.children = new List<HCUnit>();
      this.moves = new SortedList<DateTime, HCUnitMove>();

      this.unitNum = 999;  // if no number, sort last
      if( title != null )
      {
        Match m = Regex.Match( this.title, @"^(\d+)" );
        if( m.Success )
          this.unitNum = int.Parse( m.Captures[0].Value );
      }
    }

    #endregion

    #region Properties

    /// <summary>
    /// The unique hcunit identifier.
    /// </summary>
    public int ID
    {
      get { return this.id; }
    }

    /// <summary>
    /// The level of this HCUnit (Corps, Division, Brigade, etc...).
    /// </summary>
    public HCUnitLevel Level
    {
      get { return this.level; }
    }

    /// <summary>
    /// The short name of this HCUnit.
    /// </summary>
    public string ShortName
    {
      get { return this.shortName; }
    }

    /// <summary>
    /// The nick of this HCUnit.
    /// </summary>
    public string Nick
    {
      get { return this.nick; }
    }

    /// <summary>
    /// The long title of this HCUnit.
    /// </summary>
    public string Title
    {
      get { return this.title; }
    }

    /// <summary>
    /// Returns the HCUnit title if it's within 21 chars, otherwise the nick.
    /// </summary>
    public string NickOrTitle
    {
      get
      {
        if( this.title.Length > 21 )
          return this.nick;
        else
          return this.title;
      }
    }

    /// <summary>
    /// The Country this HCUnit belongs to.
    /// </summary>
    public Country Country
    {
      get { return this.country; }
    }

    /// <summary>
    /// The Branch this HCUnit belongs to.
    /// </summary>
    public Branch Branch
    {
      get { return this.branch; }
    }

    /// <summary>
    /// The way this HCUnit moves.
    /// </summary>
    public MoveType MoveType
    {
      get { return this.moveType; }
      set
      {
        if( value == this.moveType ) return;
        this.moveType = value;
        OnPropertyChanged( "MoveType" );
        OnPropertyChanged( "IsDefaultMoveType" );
      }
    }

    /// <summary>
    /// The way this HCUnit moves (inital value).
    /// </summary>
    public MoveType DefaultMoveType
    {
      get { return this.defaultMoveType; }
    }

    /// <summary>
    /// This HCUnit's parent HCUnit.
    /// </summary>
    public HCUnit ParentUnit
    {
      get { return this.parent; }
    }

    /// <summary>
    /// A list of any child unit(s) that belong to this HCUnit.
    /// </summary>
    public ReadOnlyCollection<HCUnit> ChildUnits
    {
      get { return this.children.AsReadOnly(); }
    }

    /// <summary>
    /// The name of the player current in command of this HCUnit.
    /// </summary>
    public string Owner
    {
      get { return this.owner; }
    }

    /// <summary>
    /// The TOE associated with this unit, or null if none.
    /// </summary>
    public Toe Toe
    {
      get { return this.toe; }
      set
      {
        if( value == this.toe ) return;
        this.toe = value;
        OnPropertyChanged( "Toe" );
      }
    }

    /// <summary>
    /// The ChokePoint this HCUnit is currently deployed in (null if routed).
    /// </summary>
    public ChokePoint DeployedChokePoint
    {
      get { return this.deployedcp; }
      set
      {
        if( value == this.deployedcp ) return;
        this.deployedcp = value;
        OnPropertyChanged( "DeployedChokePoint" );
      }
    }

    /// <summary>
    /// When routed, the last ChokePoint this HCUnit was deployed in (null if deployed or unavailable).
    /// </summary>
    public ChokePoint RoutedFromChokePoint
    {
      get { return this.routedcp; }
    }

    /// <summary>
    /// True if this unit is a deployable unit level (Division or Brigade).
    /// </summary>
    public bool IsDeployable
    {
      get { return this.level == HCUnitLevel.Division || this.level == HCUnitLevel.Brigade; }
    }

    /// <summary>
    /// If this HCUnit is currently deployed.
    /// </summary>
    public bool IsDeployed
    {
      get { return this.deployedcp != null; }
    }

    /// <summary>
    /// Time since this HCUnit was last moved (check HasLastMoved to see if available).
    /// </summary>
    public TimeSpan DeployedTime
    {
      get { return DateTime.Now - this.lastMovedTime; }
    }

    /// <summary>
    /// Gets whether this HCUnit has a valid LastMovedTime.
    /// </summary>
    public bool HasLastMoved
    {
      get { return this.lastMovedTime != DateTime.MinValue; }
    }

    /// <summary>
    /// The date/time this HCUnit was last deployed (check HasLastMoved to see if available).
    /// </summary>
    public DateTime LastMovedTime
    {
      get { return this.lastMovedTime; }
    }

    /// <summary>
    /// The name of the player that last moved this HCUnit.
    /// </summary>
    public string LastMovedPlayer
    {
      get { return this.lastMovedPlayer; }
    }

    /// <summary>
    /// The date/time this HCUnit is next allowed to move.
    /// </summary>
    public DateTime NextMoveTime
    {
      get { return this.nextMoveTime; }
    }

    /// <summary>
    /// True if this HCUnit is allowed to move/deploy.
    /// </summary>
    public bool CanMove
    {
      get { return this.nextMoveTime < DateTime.Now; }
    }

    /// <summary>
    /// Gets the percentage of deployed time remaining until the unit can move
    /// (0 = can move, 100 = just moved).
    /// </summary>
    public float NextMovePercent
    {
      get
      {
        if( this.CanMove )
          return 0;

        float minsUntilMove = (float)( ( this.nextMoveTime - DateTime.Now ).TotalMinutes );

        if( this.IsDeployed )  // deployed, percent of BRIGADE_MOVEMENT_DELAY_FRONTLINE minutes
          return ( minsUntilMove / WW2.BRIGADE_MOVEMENT_DELAY_FRONTLINE ) * 100;
        else                 // routed, percent of BRIGADE_ROUTED_DELAY minutes
          return ( minsUntilMove / WW2.BRIGADE_ROUTED_DELAY ) * 100;
      }
    }

    /// <summary>
    /// True if there is currently a pending move request.
    /// </summary>
    public bool MovePending
    {
      get
      {
        if( this.moves.Count == 0 ) return false;

        return this.moves.Values[this.moves.Count - 1].State == HCUnitMoveState.Pending;
      }
    }

    /// <summary>
    /// A list of all HCUnitMoves in the past 
    /// </summary>
    public IList<HCUnitMove> Moves
    {
      get { return moves.Values; }
    }

    /// <summary>
    /// Gets tooltip text about the units current deployment/last moved status.
    /// </summary>
    public string Tooltip
    {
      get
      {
        if( !this.IsDeployable )
          return null;

        if( this.IsDeployed )
        {
          if( !this.HasLastMoved )
            return null;
          
          return String.Format( Language.OrderOfBattle_DeployTooltip_DeployedMoved,
                                Misc.EnumString( this.Level ), this.deployedcp, this.lastMovedPlayer, Misc.MinsAgoLong( this.LastMovedTime ) );
        }
        else
        {
          if( !this.HasLastMoved )
            return String.Format( Language.OrderOfBattle_DeployTooltip_NotDeployed, Misc.EnumString( this.Level ) );

          if( this.routedcp == null )
            return String.Format( Language.OrderOfBattle_DeployTooltip_NotDeployedRouted, Misc.EnumString( this.Level ), Misc.MinsAgoLong( this.LastMovedTime ) );

          return String.Format( Language.OrderOfBattle_DeployTooltip_NotDeployedRoutedFrom, Misc.EnumString( this.Level ), this.routedcp, Misc.MinsAgoLong( this.LastMovedTime ) );
        }
      }
    }

    /// <summary>
    /// Gets tooltip text about how long until the unit can be redeployed.
    /// </summary>
    public string TooltipRedeploy
    {
      get
      {
        if( !this.IsDeployable )
          return null;

        if( this.MovePending )
        {
          return String.Format( Language.OrderOfBattle_RedeployTooltip_MovePending, this.moves.Values[this.moves.Count - 1].To );
        }
        else if( this.IsDeployed )
        {
          if( this.CanMove )
            return String.Format( Language.OrderOfBattle_RedeployTooltip_DeployedCanMove, Misc.EnumString( this.Level ) );
          else
            return String.Format( Language.OrderOfBattle_RedeployTooltip_DeployedCanMoveIn, Misc.EnumString( this.Level ), Misc.MinsUntilLong( this.nextMoveTime ) );
        }
        else  // routed
        {
          if( this.CanMove )
            return String.Format( Language.OrderOfBattle_RedeployTooltip_UndeployedCanDeploy, Misc.EnumString( this.Level ) );
          else
            return String.Format( Language.OrderOfBattle_RedeployTooltip_UndeployedCanDeployIn, Misc.EnumString( this.Level ), Misc.MinsUntilLong( this.nextMoveTime ) );
        }
      }
    }

    /// <summary>
    /// True if this is one of the army paratroop brigades/divisions.
    /// </summary>
    public bool IsPara
    {
      get { return this.branch == Branch.Army && this.defaultMoveType == MoveType.Air; }
    }

    /// <summary>
    /// Get a string of this unit's country and branch (eg, "UK Army").
    /// </summary>
    public string CountryBranch
    {
      get
      {
        return String.Format( "{0} {1}", this.country != null ? this.country.Abbr : null, this.branch );
      }
    }

    /// <summary>
    /// The title of this unit's Division (used for sorting).
    /// </summary>
    public string DivisionTitle
    {
      get
      {
        if( this.level == HCUnitLevel.Brigade )
          return this.parent.Title;
        else if( this.level == HCUnitLevel.Division )
          return this.Title;
        else
          return null;
      }
    }

    /// <summary>
    /// True if this unit still has it's default MoveType.
    /// </summary>
    public bool IsDefaultMoveType
    {
      get { return this.moveType == this.defaultMoveType; }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Set the initial state of this HCUnit.
    /// </summary>
    /// <param name="cp">The ChokePoint where the HCUnit is currently deployed.</param>
    /// <param name="lastMovedTime">The time the unit was last redeployed, otherwise DateTime.MinValue.</param>
    /// <param name="lastMovedPlayer">The player that performed the last move, otherwise "SYSTEM".</param>
    /// <param name="nextMoveTime">The time the unit is next allowed to move, otherwise DateTime.MinValue.</param>
    public void SetInitState( ChokePoint cp, DateTime lastMovedTime, string lastMovedPlayer, DateTime nextMoveTime )
    {
      this.deployedcp = cp;

      if( this.deployedcp != null )
        this.deployedcp.AddHCUnit( this );

      this.lastMovedTime   = lastMovedTime;
      this.lastMovedPlayer = lastMovedPlayer;
      this.nextMoveTime    = nextMoveTime;
    }

    /// <summary>
    /// Add a move request to this unit's move history.
    /// </summary>
    /// <param name="from">The ChokePoint the unit is to be moved from.</param>
    /// <param name="to">The ChokePoint the unit is to be moved to.</param>
    /// <param name="time">The time the request was made.</param>
    /// <param name="player">The name of the player that requested the move.</param>
    /// <param name="result">The result of the move request.</param>
    public void AddMoveRequest( ChokePoint from, ChokePoint to, DateTime time, string player, HCUnitMoveResult result )
    {
      // cancel any pending moves

      this.CancelPendingMoves( time );


      // add move to history

      while( this.moves.ContainsKey( time ) )
        time = time.AddMilliseconds( 1 );  // make unique

      this.moves.Add( time, new HCUnitMove( from, to, player, time, result ) );
    }

    /// <summary>
    /// Add a completion attempt to this unit's move history.
    /// </summary>
    /// <param name="from">The ChokePoint the unit is to be moved from.</param>
    /// <param name="to">The ChokePoint the unit is to be moved to.</param>
    /// <param name="time">The time the completion attempt was made.</param>
    /// <param name="player">The name of the player that requested the move.</param>
    /// <param name="result">The result of the completion attempt.</param>
    public void AddMoveAttempt( ChokePoint from, ChokePoint to, DateTime time, string player, HCUnitMoveResult result )
    {
      // update matching move request

      int iMatch = -1;
      for( int i = this.moves.Count - 1; i >= 0; i-- )  // newest-to-oldest
      {
        if( this.moves.Values[i].IsMatch( from, to, player ) )
        {
          iMatch = i;
          break;
        }
      }
      if( iMatch < 0 )  // not found, may happen on init
      {
#if DEBUG
        Log.AddError( "No match for completion attempt: {0} {1} {2}=>{3} {4}", Misc.DateToSeconds( time ), this.shortName, from, to, player );
#endif
        return;
      }

      this.moves.Values[iMatch].AddCompletion( time, result );


      // if success, perform move

      if( this.moves.Values[iMatch].State == HCUnitMoveState.Success )
        this.Move( from, to, time, player );
    }

    /// <summary>
    /// Add a system move to this unit's move history.
    /// </summary>
    /// <param name="from">The ChokePoint the unit is to be moved from.</param>
    /// <param name="to">The ChokePoint the unit is to be moved to.</param>
    /// <param name="time">The time the move was made.</param>
    /// <param name="result">The result of the move (always successful).</param>
    public void AddSystemMove( ChokePoint from, ChokePoint to, DateTime time, HCUnitMoveResult result )
    {
      // cancel any pending moves

      this.CancelPendingMoves( time );


      // add move to history

      while( this.moves.ContainsKey( time ) )
        time = time.AddMilliseconds( 1 );  // make unique

      this.moves.Add( time, new HCUnitMove( from, to, time, result ) );


      // perform move

      this.Move( from, to, time, "SYSTEM" );
    }

    /// <summary>
    /// Cancels any pending move requests for this HCUnit.
    /// </summary>
    /// <param name="time">The time the move should be listed as cancelled.</param>
    private void CancelPendingMoves( DateTime time )
    {
      foreach( HCUnitMove move in this.moves.Values )
        if( move.State == HCUnitMoveState.Pending )
          move.Cancel( time );
    }

    /// <summary>
    /// Removes any entries from the move history older than 12 hours. Also cancels
    /// stale pending moves.
    /// </summary>
    public void PruneMoveHistory()
    {
      // purge any old entries

      DateTime purgeLimit = DateTime.Now.AddHours( -12 );
      while( this.moves.Count > 0 && this.moves.Keys[0] < purgeLimit )
        this.moves.RemoveAt( 0 );


      // cancel any stale pending moves

      purgeLimit = DateTime.Now.AddMinutes( -4 );  // 3 min timeout + margin

      foreach( HCUnitMove move in this.moves.Values )
      {
        if( move.State == HCUnitMoveState.Pending && move.ReceivedTime < purgeLimit )
        {
          move.Cancel();
#if DEBUG
          Log.AddError( "Cancelling stale pending move: {0} {1} {2}=>{3} {4}", Misc.DateToSeconds( move.RequestTime ), this.shortName, move.From, move.To, move.Player );
          Log.AddError( "  {0} older than {1} by {2}", move.ReceivedTime, purgeLimit, purgeLimit - move.ReceivedTime );
#endif
        }
      }
    }

    /// <summary>
    /// Move the HCUnit to another ChokePoint.
    /// </summary>
    /// <param name="from">The previously deployed ChokePoint.</param>
    /// <param name="to">The ChokePoint to move to (can be a training cp to undeploy).</param>
    /// <param name="time">The date/time the move occurred.</param>
    /// <param name="player">The name of the player that ordered the move.</param>
    private void Move( ChokePoint from, ChokePoint to, DateTime time, string player )
    {
      // update cp hcunits
      // NOTE: we need to assume the from cp may not be the same as deployedcp during init

      if( this.deployedcp != null )
        this.deployedcp.RemoveHCUnit( this );

      this.deployedcp = to.IsTraining ? null : to;

      if( this.deployedcp != null )
        this.deployedcp.AddHCUnit( this );


      // update state data

      this.lastMovedTime = time;
      this.lastMovedPlayer = player;

      DateTime newNextMoveTime;
      if( player == "SYSTEM" && to.IsTraining )  // routed
      {
        this.routedcp = from; // remember routed from cp
        newNextMoveTime = this.lastMovedTime.AddMinutes( WW2.BRIGADE_ROUTED_DELAY );
      }
      else
      {
        this.routedcp = null;

        int delay = ( to.IsFrontline || from.IsFrontline ) ? WW2.BRIGADE_MOVEMENT_DELAY_FRONTLINE : WW2.BRIGADE_MOVEMENT_DELAY_NONFRONTLINE;
        newNextMoveTime = this.lastMovedTime.AddMinutes( delay );
      }
      if( newNextMoveTime > this.nextMoveTime )  // on init, don't overwrite newer time set by SetInitState()
        this.nextMoveTime = newNextMoveTime;


      // raise events

      ChokePoint cpEvent;  // the cp that 'owns' the event

      if     ( player != "SYSTEM" ) cpEvent = to;
      else if( to.IsTraining      ) cpEvent = from;
      else if( from.IsTraining    ) cpEvent = to;
      else                          cpEvent = from;

      if( cpEvent.EventTracker != null )
      {
        if     ( player != "SYSTEM" ) cpEvent.EventTracker.Add( new HCUnitMovedGameEvent   ( time, this, from, to, player ) );
        else if( to.IsTraining      ) cpEvent.EventTracker.Add( new HCUnitRoutedGameEvent  ( time, this, from             ) );
        else if( from.IsTraining    ) cpEvent.EventTracker.Add( new HCUnitDeployedGameEvent( time, this, to               ) );
        else                          cpEvent.EventTracker.Add( new HCUnitRetreatGameEvent ( time, this, from, to         ) );
      }
    }

    /// <summary>
    /// Associate another HCUnit as a child of this HCUnit.
    /// </summary>
    /// <param name="hcunit">The child HCUnit.</param>
    public void AddChild( HCUnit hcunit )
    {
      this.children.Add( hcunit );
      this.children.Sort();
    }

    /// <summary>
    /// Generates a dummy brigade called "Division HQ".
    /// </summary>
    /// <returns></returns>
    public HCUnit GetDummyDivHQBrigade()
    {
      return new HCUnit( -1, HCUnitLevel.Brigade, null, null, Language.Common_DivisionHQ,
                         this.level == HCUnitLevel.Brigade ? this.parent : this,
                         this.country, this.branch, this.moveType, null, null );
    }

    /// <summary>
    /// Checks whether a ChokePoint is a currently valid deployment location.
    /// </summary>
    /// <param name="cp">The proposed destination ChokePoint.</param>
    /// <returns>True if the destination is valid.</returns>
    public string IsValidDest( ChokePoint cp )
    {
      // must be a brigade or division hq

      if( !this.IsDeployable )
        return "Unit isn't a brigade or division";


      // must be same side

      if( this.country.Side != cp.Owner.Side )
        return string.Format( "Destination town must be {0}", this.country.Side );


      // must have correct military facility

      if( ( this.branch == Branch.Army && !this.IsPara ) && !cp.HasArmybase )
        return "Army unit requires an Armybase";
      else if( this.IsPara && !cp.HasAirbase )
        return "Para unit requires an Airfield";
      else if( this.branch == Branch.Airforce && !cp.HasAirbase )
        return "Air unit requires an Airfield";
      else if( this.branch == Branch.Navy && !cp.HasNavalbase )
        return "Navy unit requires a Docks";


      return null;
    }

    /// <summary>
    /// Checks if a HCUnit can be 'moved'.
    /// </summary>
    /// <remarks>The usual destination and movement rules apply (excluding timers).</remarks>
    /// <param name="cp">The proposed destination ChokePoint.</param>
    /// <returns>True if the move is allowed.</returns>
    public string CanMoveTo( ChokePoint cp )
    {
      // must be a valid destination

      string error = IsValidDest( cp );
      if( error != null )
        return error;


      if( this.IsDeployed )  // moving from map => map
      {
        // current location is always valid (ie, no move)

        if( cp == this.deployedcp )
          return null;


        // land brigades

        if( this.moveType == MoveType.Land )
        {
          // if brigade w/ deployed div hq

          if( this.level == HCUnitLevel.Brigade && this.parent.IsDeployed )
          {
            // current location must be linked to divhq to move at all

            if( this.deployedcp != this.parent.DeployedChokePoint && !this.deployedcp.IsLinked( this.parent.DeployedChokePoint ) )
              return String.Format( "Brigade is not linked to Div HQ in {0} (can't move at all)", this.parent.DeployedChokePoint );


            // dest cp must be one link from div hq

            if( cp != this.parent.DeployedChokePoint && !this.parent.DeployedChokePoint.IsLinked( cp ) )
              return String.Format( "{0} is not linked to Div HQ in {1}", cp, this.parent.DeployedChokePoint );
          }
          // else, div hq's or brigades w/o a div hq are free to move


          // must be one link from current location

          if( !this.deployedcp.IsLinked( cp ) )
            return String.Format( "Can only move one link from {0}", this.deployedcp );
        }
        // else, air/sea brigades/divisions can move whereever

      }
      else  // not deployed, moving from training => map
      {
        if( this.level == HCUnitLevel.Division )
        {
          // can't deploy to frontline

          if( cp.IsFrontline )
            return "Can't deploy to frontline town";
        }
        else  // Brigade
        {
          // division must be deployed

          if( !this.parent.IsDeployed )
            return "Div HQ must be deployed first";


          // must go to same cp as div hq

          if( cp != this.parent.DeployedChokePoint )
            return String.Format( "Can only deploy to Div HQ located in {0}", this.parent.DeployedChokePoint );
        }
      }


      return null;
    }

    /// <summary>
    /// Remove any circular references to allow garbage collection.
    /// </summary>
    public void Cleanup()
    {
      this.country = null;
      this.parent = null;
      this.children = null;
      this.deployedcp = null;
      this.routedcp = null;

      if( this.moves != null )
      {
        foreach( HCUnitMove move in this.moves.Values )
          move.Cleanup();
        this.moves.Clear();
        this.moves = null;
      }
    }

    #endregion

    #region Utility Methods
    /// <summary>
    /// Provides a string representation of this object.
    /// </summary>
    /// <returns>The HCUnit's long title.</returns>
    public override string ToString()
    {
      return this.title;
    }

    /// <summary>
    /// Returns a value indicating whether this instance is equal to a specified object value.
    /// </summary>
    /// <param name="obj">An object to compare with this instance.</param>
    /// <returns>True if obj is the same HCUnit.</returns>
    public override bool Equals( object obj )
    {
      return obj is HCUnit && this.id == ( (HCUnit)obj ).id;
    }

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    /// <returns>The HCUnit id.</returns>
    public override int GetHashCode()
    {
      return this.id;
    }

    /// <summary>
    /// Support for the == operator.
    /// </summary>
    public static bool operator ==( HCUnit a, HCUnit b )
    {
      if( (object)a == null ) return (object)b == null;
      return a.Equals( b );
    }

    /// <summary>
    /// Support for the != operator.
    /// </summary>
    public static bool operator !=( HCUnit a, HCUnit b )
    {
      if( (object)a == null ) return (object)b != null;
      return !a.Equals( b );
    }

    /// <summary>
    /// Compare this object with another HCUnit object.
    /// </summary>
    /// <param name="other">The other HCUnit to compare with.</param>
    /// <returns>-1, 0, 1</returns>
    public int CompareTo( HCUnit other )
    {
      if( other == null ) return 1;


      // sort by country

      if( this.country != other.country )
        return this.country.CompareTo( other.country );


      // then by branch

      if( this.Branch != other.Branch )
        return this.Branch.CompareTo( other.Branch );


      // then by name within level

      HCUnit div1 = this.level == HCUnitLevel.Division ? this : this.parent;
      HCUnit div2 = other.level == HCUnitLevel.Division ? other : other.parent;

      if( div1 != div2 )
        return div1.CompareToName( div2 );  // diff division, sort by div name
      else if( this.Level != other.Level )
        return this.Level.CompareTo( other.Level );  // same division diff level, sort by level
      else
        return this.CompareToName( other );  // same division, same level, sort by name
    }

    /// <summary>
    /// Compare this object with another HCUnit object by name.
    /// </summary>
    /// <param name="other">The other HCUnit to compare with.</param>
    /// <returns>-1, 0, 1</returns>
    private int CompareToName( HCUnit other )
    {
      // sort by number prefix (no number = last)

      if( this.unitNum != other.unitNum )
        return this.unitNum.CompareTo( other.unitNum );


      // then by name

      return this.title.CompareTo( other.title );
    }

    #endregion
  }

  
  /// <summary>
  /// A Squad is a self-managed group of players that choose to play together.
  /// </summary>
  public class Squad : IComparable<Squad>
  {
    #region Variables
		
    private readonly int id;
    private readonly string handle;
    private readonly string name;
    private readonly string co;

    #endregion

    #region Constructors

    /// <summary>
    /// Create a new Squad.
    /// </summary>
    /// <param name="id">The unique id of the Squad.</param>
    /// <param name="handle">The short name of the Squad.</param>
    /// <param name="name">The full name of the Squad.</param>
    /// <param name="co">The name of the Squad's CO.</param>
    public Squad( int id, string handle, string name, string co )
    {
      this.id = id;
      this.handle = handle;
      this.name = name;
      this.co = co;
    }

    #endregion

    #region Properties

    /// <summary>
    /// The unique Squad identifier.
    /// </summary>
    public int ID
    {
      get { return id; }
    }

    /// <summary>
    /// The short name of this Squad.
    /// </summary>
    public string Handle
    {
      get { return this.handle; }
    }

    /// <summary>
    /// The full name of this Squad.
    /// </summary>
    public string Name
    {
      get { return this.name; }
    }

    /// <summary>
    /// The player name of the Squad's current CO.
    /// </summary>
    public string CO
    {
      get { return this.co; }
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Provides a string representation of this object.
    /// </summary>
    /// <returns>The Squad's full name.</returns>
    public override string ToString()
    {
      return this.name;
    }

    /// <summary>
    /// Returns a value indicating whether this instance is equal to a specified object value.
    /// </summary>
    /// <param name="obj">An object to compare with this instance.</param>
    /// <returns>True if obj is the same Squad.</returns>
    public override bool Equals( object obj )
    {
      return obj is Squad && this.id == ( (Squad)obj ).id;
    }

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    public override int GetHashCode()
    {
      return this.id;
    }

    /// <summary>
    /// Support for the == operator.
    /// </summary>
    public static bool operator ==( Squad a, Squad b )
    {
      if( (object)a == null ) return (object)b == null;
      return a.Equals( b );
    }

    /// <summary>
    /// Support for the != operator.
    /// </summary>
    public static bool operator !=( Squad a, Squad b )
    {
      if( (object)a == null ) return (object)b != null;
      return !a.Equals( b );
    }

    /// <summary>
    /// Compare this object with another Squad object.
    /// </summary>
    /// <param name="other">The other Squad to compare with.</param>
    /// <returns>-1, 0, 1</returns>
    public int CompareTo( Squad other )
    {
      if( other == null ) return 1;
      return this.name.CompareTo( other.name );
    }

    #endregion
  }


  /// <summary>
  /// A MapCell is a region on the map, currently 500x500m, that is used to store player deaths.
  /// </summary>
  public class MapCell
  {
    #region Variables

    private SortedList<DateTime, SideCount> deathHistory;

    #endregion

    #region Constructors

    /// <summary>
    /// Create a new MapCell.
    /// </summary>
    public MapCell()
    {
      this.deathHistory = new SortedList<DateTime, SideCount>();
    }

    #endregion

    #region Properties

    /// <summary>
    /// The total number of deaths that have occurred in this cell in the past 60 mins.
    /// </summary>
    public SideCount Deaths
    {
      get
      {
        DateTime start = DateTime.Now.AddMinutes( -60 );
        SideCount deaths = new SideCount();

        foreach( KeyValuePair<DateTime, SideCount> pair in deathHistory )
          if( pair.Key > start )
            deaths.Add( pair.Value );

        return deaths;
      }
    }

    /// <summary>
    /// The total number of deaths that have occurred in this cell in the past 20 mins.
    /// </summary>
    public int RecentDeaths
    {
      get
      {
        DateTime start = DateTime.Now.AddMinutes( -20 );
        int deaths = 0;

        foreach( KeyValuePair<DateTime, SideCount> pair in deathHistory )
          if( pair.Key > start )
            deaths += pair.Value.Total;

        return deaths;
      }
    }

    /// <summary>
    /// The deaths that have occurred in the past 5-65 mins, grouped into 5 minute intervals.
    /// </summary>
    public SideCount[] DeathsGrouped
    {
      get
      {
        SideCount[] deaths = new SideCount[12];

        DateTime now = DateTime.Now;
        DateTime end = new DateTime( now.Year, now.Month, now.Day, now.Hour, ( now.Minute / 5 ) * 5, 30 );  // interger division = round to last 5 min interval
        DateTime start = end.AddHours( -1 );

        foreach( KeyValuePair<DateTime, SideCount> pair in deathHistory )
        {
          DateTime time = pair.Key;

          if( time < start || time > end ) continue;

          int age = (int)Math.Round( ( end - time ).TotalMinutes / 5 ) - 1;  // 0 - 11
          if( age < 0 || age > 11 ) continue;  // shouldn't happen

          deaths[age].Add( pair.Value );
        }

        return deaths;
      }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Add a death count to this cell.
    /// </summary>
    /// <param name="time">The time the deaths occurred.</param>
    /// <param name="allied">The number of allied deaths.</param>
    /// <param name="axis">The number of axis deaths.</param>
    public void AddCount( DateTime time, int allied, int axis )
    {
      if( this.deathHistory.ContainsKey( time ) )
        return;  // already have this value

      this.deathHistory.Add( time, new SideCount( allied, axis ) );
    }

    /// <summary>
    /// Removes all entries older than an hour.
    /// </summary>
    /// <returns>True if no more entries left.</returns>
    public bool Purge()
    {
      DateTime anHourAgo = DateTime.Now.AddMinutes( -65 );
      while( this.deathHistory.Count > 0 && this.deathHistory.Keys[0] < anHourAgo )
        this.deathHistory.RemoveAt( 0 );

      return this.deathHistory.Count == 0;
    }

    /// <summary>
    /// Calculates the colour this cell should be displayed based on the number and age
    /// of the deaths it contains.
    /// </summary>
    /// <returns>A colour between yellow (lots of deaths) and white (few deaths), light (recent) and dark (old).</returns>
    public Color GetCellColour()
    {
      // group deaths into 10 minute buckets

      int[] groupedDeaths = new int[6];
      DateTime now = DateTime.Now;
      foreach( KeyValuePair<DateTime, SideCount> keyvalue in deathHistory )
      {
        DateTime time = keyvalue.Key;
        int count = keyvalue.Value.Allied + keyvalue.Value.Axis;

        int age = (int)Math.Round( ( now - time ).TotalMinutes / 10 );  // 1 - 6
        age--;
        if( age < 0 ) age = 0;
        if( age > 5 ) age = 5;

        groupedDeaths[age] += count;
      }


      // calc age index

      float ageIndex = 0;  // 0.77 - 1.00
      float ageWeight = 1;
      for( int i = 0; i < groupedDeaths.Length; i++ )
      {
        if( groupedDeaths[i] > 0 )
        {
          ageIndex = ageWeight;
          break;
        }
        ageWeight *= 0.95F;
      }
      if( ageIndex == 0 )
        return Color.Transparent;  // no deaths


      // calc color index

      float colourIndex = 0;
      float deathWeight = 10F;
      for( int i = 0; i < groupedDeaths.Length; i++ )
      {
        colourIndex += ( groupedDeaths[i] * deathWeight );
        deathWeight *= 0.85F;
      }


      // calc base color (white = 1 death, yellow = maxIndex+ deaths)

      const float maxIndex = 45;
      if( colourIndex > maxIndex )
        colourIndex = maxIndex;
      Color color = Misc.BlendColour( Color.White, Color.Yellow, colourIndex / maxIndex );


      // adjust based on age (bright = recent, darker = older)

      int r = (int)Math.Round( color.R * ageIndex );
      int g = (int)Math.Round( color.G * ageIndex );
      int b = (int)Math.Round( color.B * ageIndex );

      return Color.FromArgb( r, g, b );
    }

    #endregion
  }


  /// <summary>
  /// A Server object represents an instance of a Battleground Europe game server cluster.
  /// </summary>
  public class Server : IComparable<Server>
  {
    #region Variables

    private readonly int id;
    private readonly string name;
    private ServerState state;
    private string stateInfo;
    private ServerPopulation population;

    #endregion

    #region Constructors

    /// <summary>
    /// Create a new Server object.
    /// </summary>
    /// <param name="id">The arena id of the server.</param>
    /// <param name="name">The server name (should match Language.ServerName_...).</param>
    /// <param name="state">The initial ServerState.</param>
    /// <param name="stateInfo">The optional raw state info string.</param>
    /// <param name="population">The initial ServerPopulation.</param>
    public Server( int id, string name, ServerState state, string stateInfo, ServerPopulation population )
    {
      this.id = id;
      this.name = name;
      this.state = state;
      this.stateInfo = LocaliseStateInfo( stateInfo );
      this.population = population;

      string localName = Language.ResourceManager.GetString( String.Format( "ServerName_{0}", name ) );
      if( localName != null )
        this.name = localName;
    }

    #endregion

    #region Properties

    /// <summary>
    /// The server arena id (used when launching playgate).
    /// </summary>
    public int ID
    {
      get { return this.id; }
    }

    /// <summary>
    /// The server name.
    /// </summary>
    public string Name
    {
      get { return this.name; }
    }

    /// <summary>
    /// The current server player population.
    /// </summary>
    public ServerPopulation Population
    {
      get { return this.population; }
    }

    /// <summary>
    /// The current server state (online, locked, etc).
    /// </summary>
    public string State
    {
      get { return Misc.EnumString( this.state ); }
    }

    /// <summary>
    /// Gets a tooltip for the current server state.
    /// </summary>
    public string StateInfo
    {
      get { return this.stateInfo; }
    }

    /// <summary>
    /// True if the server is online and unlocked.
    /// </summary>
    public bool Online
    {
      get { return this.state == ServerState.Online; }
    }

    /// <summary>
    /// True if the server is online and locked.
    /// </summary>
    public bool Locked
    {
      get { return this.state == ServerState.Locked; }
    }

    /// <summary>
    /// True if the server if offline or closed.
    /// </summary>
    public bool Offline
    {
      get { return this.state == ServerState.Offline || this.state == ServerState.Closed; }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Update the state and population of this server.
    /// </summary>
    /// <param name="state">The current ServerState.</param>
    /// <param name="stateInfo">The optional raw state info string.</param>
    /// <param name="population">The current ServerPopulation.</param>
    public void UpdateStatus( ServerState state, string stateInfo, ServerPopulation population )
    {
      this.state = state;
      this.stateInfo = LocaliseStateInfo( stateInfo );
      this.population = population;
    }

    /// <summary>
    /// Generates a localised string for the given raw stateinfo.
    /// </summary>
    /// <param name="stateInfo">The state info string.</param>
    /// <returns>A localised version of each component, if available.</returns>
    private string LocaliseStateInfo( string stateInfo )
    {
      if( stateInfo == null )
        return null;

      string[] items = stateInfo.Split( ':' );

      for( int i = 0; i < items.Length; i++ )
      {
        string localState = Language.ResourceManager.GetString( String.Format( "Enum_ServerState_Info_{0}", items[i] ) );
        if( localState != null )
          items[i] = localState;
      }

      return String.Join( ", ", items );
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Provides a string representation of this object.
    /// </summary>
    /// <returns>The server name.</returns>
    public override string ToString()
    {
      return this.name;
    }

    /// <summary>
    /// Returns a value indicating whether this instance is equal to a specified object value.
    /// </summary>
    /// <param name="obj">An object to compare with this instance.</param>
    /// <returns>True if obj is the same Server.</returns>
    public override bool Equals( object obj )
    {
      return obj is Server && this.id == ( (Server)obj ).id;
    }

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    public override int GetHashCode()
    {
      return this.id;
    }

    /// <summary>
    /// Support for the == operator.
    /// </summary>
    public static bool operator ==( Server a, Server b )
    {
      if( (object)a == null ) return (object)b == null;
      return a.Equals( b );
    }

    /// <summary>
    /// Support for the != operator.
    /// </summary>
    public static bool operator !=( Server a, Server b )
    {
      if( (object)a == null ) return (object)b != null;
      return !a.Equals( b );
    }

    /// <summary>
    /// Compare this object with another Server object.
    /// </summary>
    /// <param name="other">The other Server to compare with.</param>
    /// <returns>-1, 0, 1</returns>
    public int CompareTo( Server other )
    {
      if( other == null ) return 1;
      return this.id.CompareTo( other.id );
    }

    #endregion
  }


  /// <summary>
  /// A category of Vehicle (eg, Infantry, Ground Vehicle, Aircraft, etc).
  /// </summary>
  public class VehicleCategory : IComparable<VehicleCategory>, IComparable
  {
    #region Variables

    private readonly int id;
    private readonly Branch branch;
    private readonly string abbr;
    private readonly string name;

    #endregion

    #region Constructors

    /// <summary>
    /// Create a new VehicleCategory.
    /// </summary>
    /// <param name="id">The unique category identifier.</param>
    /// <param name="branch">The Branch the category is associated with.</param>
    /// <param name="abbr">The category two-letter abbreviation.</param>
    /// <param name="name">The category name (should match Language.Enum_VehicleCategory_...).</param>
    public VehicleCategory( int id, Branch branch, string abbr, string name )
    {
      this.id = id;
      this.branch = branch;
      this.abbr = abbr;
      this.name = name;
      
      string localName = Language.ResourceManager.GetString( String.Format( "Enum_VehicleCategory_{0}", name.Replace( " ", "" ) ) );
      if( localName != null )
        this.name = localName;
    }

    #endregion

    #region Properties

    /// <summary>
    /// The unique VehicleCategory identifier.
    /// </summary>
    public int ID
    {
      get { return this.id; }
    }

    /// <summary>
    /// The Branch associated with this category.
    /// </summary>
    public Branch Branch
    {
      get { return this.branch; }
    }

    /// <summary>
    /// The category's two-letter abbreviation.
    /// </summary>
    public string Abbr
    {
      get { return this.abbr; }
    }

    /// <summary>
    /// The category's name.
    /// </summary>
    public string Name
    {
      get { return this.name; }
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Provides a string representation of this object.
    /// </summary>
    /// <returns>The category name.</returns>
    public override string ToString()
    {
      return this.name;
    }

    /// <summary>
    /// Returns a value indicating whether this instance is equal to a specified object value.
    /// </summary>
    /// <param name="obj">An object to compare with this instance.</param>
    /// <returns>True if obj is the same VehicleCategory.</returns>
    public override bool Equals( object obj )
    {
      return obj is VehicleCategory && this.id == ( (VehicleCategory)obj ).id;
    }

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    public override int GetHashCode()
    {
      return this.id;
    }

    /// <summary>
    /// Support for the == operator.
    /// </summary>
    public static bool operator ==( VehicleCategory a, VehicleCategory b )
    {
      if( (object)a == null ) return (object)b == null;
      return a.Equals( b );
    }

    /// <summary>
    /// Support for the != operator.
    /// </summary>
    public static bool operator !=( VehicleCategory a, VehicleCategory b )
    {
      if( (object)a == null ) return (object)b != null;
      return !a.Equals( b );
    }

    /// <summary>
    /// Compare this object with another VehicleCategory object.
    /// </summary>
    /// <param name="obj">The other VehicleCategory to compare with.</param>
    /// <returns>-1, 0, 1</returns>
    public int CompareTo( object obj )
    {
      VehicleCategory other = obj as VehicleCategory;
      if( other == null ) return -1;
      return this.CompareTo( other );
    }

    /// <summary>
    /// Compare this object with another VehicleCategory object.
    /// </summary>
    /// <param name="other">The other VehicleCategory to compare with.</param>
    /// <returns>-1, 0, 1</returns>
    public int CompareTo( VehicleCategory other )
    {
      return this.name.CompareTo( other.name );
    }

    #endregion
  }


  /// <summary>
  /// A class of Vehicle (eg, Tank, Artillery, Truck, etc)
  /// </summary>
  public class VehicleClass : IComparable<VehicleClass>, IComparable
  {
    #region Variables

    private readonly int id;
    private readonly string code;
    private readonly string name;
    private VehicleCategory category;

    #endregion

    #region Constructors

    /// <summary>
    /// Create a new VehicleClass.
    /// </summary>
    /// <param name="id">The unique class id.</param>
    /// <param name="name">The class name.</param>
    /// <param name="category">The category this class belongs to.</param>
    public VehicleClass( int id, string name, VehicleCategory category )
    {
      this.id = id;
      this.code = name;
      this.name = name.ToTitleCase();
      this.category = category;

      string localName = Language.ResourceManager.GetString( String.Format( "Enum_VehicleClass_{0}", this.name ) );
      if( localName != null )
        this.name = localName;
    }

    #endregion

    #region Properties

    /// <summary>
    /// The unique class identifier.
    /// </summary>
    public int ID
    {
      get { return this.id; }
    }

    /// <summary>
    /// The class raw-code.
    /// </summary>
    public string Code
    {
      get { return this.code; }
    }

    /// <summary>
    /// The class friendly name.
    /// </summary>
    public string Name
    {
      get { return this.name; }
    }

    /// <summary>
    /// The category this class belongs to.
    /// </summary>
    public VehicleCategory Category
    {
      get { return this.category; }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Remove any circular references to allow garbage collection.
    /// </summary>
    public void Cleanup()
    {
      this.category = null;
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Provides a string representation of this object.
    /// </summary>
    /// <returns>The class name.</returns>
    public override string ToString()
    {
      return String.Format( "{0}/{1}", this.category, this.name );
    }

    /// <summary>
    /// Returns a value indicating whether this instance is equal to a specified object value.
    /// </summary>
    /// <param name="obj">An object to compare with this instance.</param>
    /// <returns>True if obj is the same VehicleClass.</returns>
    public override bool Equals( object obj )
    {
      return obj is VehicleClass && this.id == ( (VehicleClass)obj ).id;
    }

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    public override int GetHashCode()
    {
      return this.id;
    }

    /// <summary>
    /// Support for the == operator.
    /// </summary>
    public static bool operator ==( VehicleClass a, VehicleClass b )
    {
      if( (object)a == null ) return (object)b == null;
      return a.Equals( b );
    }

    /// <summary>
    /// Support for the != operator.
    /// </summary>
    public static bool operator !=( VehicleClass a, VehicleClass b )
    {
      if( (object)a == null ) return (object)b != null;
      return !a.Equals( b );
    }

    /// <summary>
    /// Compare this object with another VehicleClass object.
    /// </summary>
    /// <param name="obj">The other VehicleClass to compare with.</param>
    /// <returns>-1, 0, 1</returns>
    public int CompareTo( object obj )
    {
      VehicleClass other = obj as VehicleClass;
      if( other == null ) return -1;
      return this.CompareTo( other );
    }

    /// <summary>
    /// Compare this object with another VehicleClass object.
    /// </summary>
    /// <param name="other">The other VehicleClass to compare with.</param>
    /// <returns>-1, 0, 1</returns>
    public int CompareTo( VehicleClass other )
    {
      if( other == null ) return 1;
      return this.name.CompareTo( other.name );
    }

    #endregion
  }


  /// <summary>
  /// A Vehicle represents a object that may be controlled by the player in game, such
  /// as tanks, aircraft, and infantry.
  /// </summary>
  public class Vehicle : IComparable<Vehicle>
  {
    #region Variables

    private readonly int id;
    private Country country;
    private readonly string name;
    private VehicleClass vehClass;
    private readonly Rank curRank;
    private readonly Rank nextRank;
    private readonly int typeid;
    private readonly QuadID quad;

    #endregion

    #region Constructors

    /// <summary>
    /// Creates a new Vehicle.
    /// </summary>
    /// <param name="id">The unique vehicle id.</param>
    /// <param name="country">The Country that uses this vehicle.</param>
    /// <param name="name">The vehicle name.</param>
    /// <param name="vehClass">The class this vehicle belongs to.</param>
    /// <param name="curRank">The current rank required to spawn the vehicle.</param>
    /// <param name="nextRank">The rank required next cycle (may be lower).</param>
    /// <param name="typeid">The internal veh id (last segment of quad).</param>
    public Vehicle( int id, Country country, string name, VehicleClass vehClass, Rank curRank, Rank nextRank, int typeid )
    {
      this.id = id;
      this.country = country;
      this.name = name;
      this.vehClass = vehClass;
      this.curRank = curRank;
      this.nextRank = nextRank;
      this.typeid = typeid;
      this.quad = new QuadID( country.ID, vehClass.Category.ID, vehClass.ID, typeid );
    }

    #endregion

    #region Properties

    /// <summary>
    /// The unique vehicle identifier.
    /// </summary>
    public int ID
    {
      get { return this.id; }
    }

    /// <summary>
    /// The country that uses this vehicle.
    /// </summary>
    public Country Country
    {
      get { return this.country; }
    }

    /// <summary>
    /// The vehicle name.
    /// </summary>
    public string Name
    {
      get { return this.name; }
    }

    /// <summary>
    /// The class this vehicle belongs to.
    /// </summary>
    public VehicleClass Class
    {
      get { return this.vehClass; }
    }

    /// <summary>
    /// The current rank required to spawn this vehicle.
    /// </summary>
    public Rank CurrentRank
    {
      get { return this.curRank; }
    }

    /// <summary>
    /// The rank required to spawn this vehicle in the next RDP cycle.
    /// </summary>
    public Rank NextRank
    {
      get { return this.nextRank; }
    }

    /// <summary>
    /// The internal vehicle quad identifier.
    /// </summary>
    public QuadID InternalID
    {
      get { return this.quad; }
    }

    /// <summary>
    /// Get the default AccessLevel (public, or Buzzard for gm vehicles).
    /// </summary>
    public AccessLevel DefaultAccessLevel
    {
      get
      {
        return this.vehClass.Code == "gm" ? AccessLevel.Buzzard : AccessLevel.Public;
      }
    }

    #endregion

    #region Methods

    /// <summary>
    /// The name of the rank required as per this vehicle's country and branch.
    /// </summary>
    public string GetCurrentRankName( Branch branch )
    {
      return this.curRank.Name[this.country.ID, (int)branch];
    }

    /// <summary>
    /// The name of the rank required next cycle as per this vehicle's country and branch.
    /// </summary>
    public string GetNextRankName( Branch branch )
    {
      return this.nextRank.Name[this.country.ID, (int)branch];
    }

    /// <summary>
    /// Remove any circular references to allow garbage collection.
    /// </summary>
    public void Cleanup()
    {
      this.country = null;
      this.vehClass = null;
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Provides a string representation of this object.
    /// </summary>
    /// <returns>The vehicle name.</returns>
    public override string ToString()
    {
      return this.name;
    }

    /// <summary>
    /// Returns a value indicating whether this instance is equal to a specified object value.
    /// </summary>
    /// <param name="obj">An object to compare with this instance.</param>
    /// <returns>True if obj is the same Vehicle.</returns>
    public override bool Equals( object obj )
    {
      return obj is Vehicle && this.id == ( (Vehicle)obj ).id;
    }

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    public override int GetHashCode()
    {
      return this.id;
    }

    /// <summary>
    /// Support for the == operator.
    /// </summary>
    public static bool operator ==( Vehicle a, Vehicle b )
    {
      if( (object)a == null ) return (object)b == null;
      return a.Equals( b );
    }

    /// <summary>
    /// Support for the != operator.
    /// </summary>
    public static bool operator !=( Vehicle a, Vehicle b )
    {
      if( (object)a == null ) return (object)b != null;
      return !a.Equals( b );
    }

    /// <summary>
    /// Compare this object with another Vehicle object.
    /// </summary>
    /// <param name="other">The other Vehicle to compare with.</param>
    /// <returns>-1, 0, 1</returns>
    public int CompareTo( Vehicle other )
    {
      if( other == null ) return 1;
      return this.name.CompareTo( other.name );
    }

    #endregion
  }


  /// <summary>
  /// An item in a TOE spawnlist (vehicle, count).
  /// </summary>
  public class ToeItem : NotifyObject, IComparable<ToeItem>
  {
    #region Fields

    private readonly Vehicle vehicle;
    private readonly SupplyLevel supply;
    private AccessLevel access;
    private string comment;

    #endregion

    #region Constructors

    /// <summary>
    /// Create a new ToeItem (first cycle only).
    /// </summary>
    /// <param name="vehicle">The vehicle this ToeItem represents.</param>
    /// <param name="cycleZeroQuantity">The starting cycle vehicle quantity.</param>
    /// <param name="access">The vehicle access level (default Public).</param>
    /// <param name="comment">A comment associated with thie ToeItem.</param>
    public ToeItem( Vehicle vehicle, int cycleZeroQuantity = 1, AccessLevel access = AccessLevel.Public, string comment = null )
    {
      this.vehicle = vehicle;
      this.supply = new SupplyLevel( cycleZeroQuantity );
      this.access = access;
      this.comment = comment;
    }

    /// <summary>
    /// Create a new ToeItem (multiple cycles).
    /// </summary>
    /// <param name="vehicle">The vehicle this ToeItem represents.</param>
    /// <param name="supply">The supply level for this vehicle.</param>
    public ToeItem( Vehicle vehicle, SupplyLevel supply )
    {
      this.vehicle = vehicle;
      this.supply = supply;
      this.access = vehicle.DefaultAccessLevel;
      this.comment = null;
    }

    #endregion

    #region Properties

    /// <summary>
    /// The vehicle this ToeItem represents.
    /// </summary>
    public Vehicle Vehicle
    {
      get { return this.vehicle; }
    }

    /// <summary>
    /// The supply level for this vehicle.
    /// </summary>
    public SupplyLevel Supply
    {
      get { return this.supply; }
    }

    /// <summary>
    /// The minimum access level required to spawn this vehicle.
    /// </summary>
    public AccessLevel Access
    {
      get { return this.access; }
      set
      {
        if( value == this.access ) return;
        this.access = value;
        OnPropertyChanged( "Access" );
      }
    }

    /// <summary>
    /// Arbitrary user comment for this ToeItem instance.
    /// </summary>
    public string Comment
    {
      get { return this.comment; }
      set
      {
        if( value == this.comment ) return;
        this.comment = value;
        OnPropertyChanged( "Comment" );
      }
    }

    /// <summary>
    /// Gets or sets the starting quantity for this vehicle.
    /// </summary>
    public int CycleZeroQuantity
    {
      get
      {
        if( this.supply.Cycle.Length == 0 ) return -1;
        return this.supply.Cycle[0];
      }
      set
      {
        if( this.supply.Cycle.Length <= 0 ) return;
        if( value == this.supply.Cycle[0] ) return;

        this.supply.Cycle[0] = value;
        OnPropertyChanged( "CycleZeroQuantity" );
      }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Create a copy of this ToeItem object.
    /// </summary>
    /// <returns>A new ToeItem object.</returns>
    public ToeItem Clone()
    {
      return new ToeItem( this.vehicle, this.CycleZeroQuantity, this.access, this.comment );
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Provides a string representation of this object.
    /// </summary>
    /// <returns>The vehicle name.</returns>
    public override string ToString()
    {
      return String.Format( "{0} x {1}", this.Supply, this.Vehicle );
    }

    /// <summary>
    /// Returns a value indicating whether this instance is equal to a specified object value.
    /// </summary>
    /// <param name="obj">An object to compare with this instance.</param>
    /// <returns>True if obj is the same ToeItem.</returns>
    public override bool Equals( object obj )
    {
      ToeItem other = obj as ToeItem;
      if( (object)other == null ) return false;

      return this.Vehicle == other.Vehicle && this.Supply.Equals( other.Supply ) && this.Access == other.Access;  //// supply == supply?
    }

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    public override int GetHashCode()
    {
      return base.GetHashCode(); ////
    }

    /// <summary>
    /// Support for the == operator.
    /// </summary>
    public static bool operator ==( ToeItem a, ToeItem b )
    {
      if( (object)a == null ) return (object)b == null;
      return a.Equals( b );
    }

    /// <summary>
    /// Support for the != operator.
    /// </summary>
    public static bool operator !=( ToeItem a, ToeItem b )
    {
      if( (object)a == null ) return (object)b != null;
      return !a.Equals( b );
    }

    /// <summary>
    /// Compare this object with another ToeItem object.
    /// </summary>
    /// <param name="other">The other ToeItem to compare with.</param>
    /// <returns>-1, 0, 1</returns>
    public int CompareTo( ToeItem other )
    {
      if( other == null ) return 1;
      return this.Vehicle.CompareTo( other.Vehicle );
    }

    #endregion
  }


  /// <summary>
  /// A TOE (Table of Organisation and Equipment) contains a list of vehicles and 
  /// their supply numbers for each RDP cycle, and is assigned to a hcunit.
  /// </summary>
  public class Toe : NotifyObject, IComparable<Toe>
  {
    #region Variables

    private string code;
    private string name;
    private Country country;
    private Branch branch;
    private VehicleClass vehClass;
    private string comment;

    private ObservableDictionary<Vehicle, ToeItem> supply;

    #endregion

    #region Constructors

    /// <summary>
    /// Create a new Toe.
    /// </summary>
    /// <param name="code">The unique toe code.</param>
    /// <param name="name">The full name of the toe.</param>
    /// <param name="country">The country the toe belongs to.</param>
    /// <param name="branch">The branch the toe belongs to.</param>
    /// <param name="vehClass">The main vehicle class of this toe.</param>
    public Toe( string code, string name, Country country, Branch branch, VehicleClass vehClass, string comment = null )
    {
      this.code = code;
      this.name = name;
      this.country = country;
      this.branch = branch;
      this.vehClass = vehClass;
      this.comment = null;

      this.supply = new ObservableDictionary<Vehicle, ToeItem>();
    }

    #endregion

    #region Properties

    /// <summary>
    /// The unique TOE code.
    /// </summary>
    public string Code
    {
      get { return this.code; }
      set
      {
        if( value == this.code ) return;
        this.code = value;
        OnPropertyChanged( "Code" );
      }
    }

    /// <summary>
    /// The TOE's full name.
    /// </summary>
    public string Name
    {
      get { return this.name; }
      set
      {
        if( value == this.name ) return;
        this.name = value;
        OnPropertyChanged( "Name" );
      }
    }

    /// <summary>
    /// The Country this TOE is associated with.
    /// </summary>
    public Country Country
    {
      get { return this.country; }
    }

    /// <summary>
    /// The main branch for this Toe (doen't affect contents).
    /// </summary>
    public Branch Branch
    {
      get { return this.branch; }
      set
      {
        if( value == this.branch ) return;
        this.branch = value;
        OnPropertyChanged( "Branch" );
      }
    }

    /// <summary>
    /// The main vehicle class of this Toe (doen't affect contents).
    /// </summary>
    public VehicleClass Class
    {
      get { return this.vehClass; }
      set
      {
        if( value == this.vehClass ) return;
        this.vehClass = value;
        OnPropertyChanged( "Class" );
      }
    }

    /// <summary>
    /// Gets the supply of all Vehicles in this Toe.
    /// </summary>
    public ObservableDictionary<Vehicle, ToeItem> Supply
    {
      get { return this.supply; }
    }

    /// <summary>
    /// Gets supply for the given vehicle.
    /// </summary>
    public SupplyLevel this[Vehicle vehicle]
    {
      get { return this.supply[vehicle].Supply; }
    }

    /// <summary>
    /// A list of all vehicles contained in this TOE.
    /// </summary>
    public ICollection<Vehicle> Vehicles
    {
      get { return this.supply.Keys;  }
    }

    /// <summary>
    /// A list of all ToeItems contained in this Toe.
    /// </summary>
    public ICollection<ToeItem> Items
    {
      get { return this.supply.Values; }
    }

    /// <summary>
    /// Arbitrary user comment for this Toe instance.
    /// </summary>
    public string Comment
    {
      get { return this.comment; }
      set
      {
        if( value == this.comment ) return;
        this.comment = value;
        OnPropertyChanged( "Comment" );
      }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Add a vehicle and it's supply details to this TOE.
    /// </summary>
    /// <param name="vehicle">The vehicle to add.</param>
    /// <param name="start"></param>
    /// <param name="adjustments"></param>
    public void AddVehicle( Vehicle vehicle, int start, string adjustments )
    {
      this.supply[vehicle] = new ToeItem( vehicle, new SupplyLevel( start, adjustments, this.country.NextRDPCycle + 1 ) );
    }

    /// <summary>
    /// Create a copy of this Toe object.
    /// </summary>
    /// <param name="includeSupply">If true, also include ToeItems.</param>
    /// <returns>A new Toe object.</returns>
    public Toe Clone( bool includeSupply )
    {
      Toe cloneToe = new Toe( this.code, this.name, this.country, this.branch, this.vehClass, this.comment );

      if( includeSupply )
        foreach( ToeItem toeItem in this.Items )
          cloneToe.supply.Add( toeItem.Vehicle, toeItem.Clone() );

      return cloneToe;
    }

    /// <summary>
    /// Remove any circular references to allow garbage collection.
    /// </summary>
    public void Cleanup()
    {
      this.country = null;
      this.supply.Clear();
      this.supply = null;
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Provides a string representation of this object.
    /// </summary>
    /// <returns>The TOE name.</returns>
    public override string ToString()
    {
      return this.name;
    }

    /// <summary>
    /// Returns a value indicating whether this instance is equal to a specified object value.
    /// </summary>
    /// <param name="obj">An object to compare with this instance.</param>
    /// <returns>True if obj is the same Toe.</returns>
    public override bool Equals( object obj )
    {
      return obj is Toe && this.code == ( (Toe)obj ).code;
    }

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    public override int GetHashCode()
    {
      return this.code.GetHashCode();
    }

    /// <summary>
    /// Support for the == operator.
    /// </summary>
    public static bool operator ==( Toe a, Toe b )
    {
      if( (object)a == null ) return (object)b == null;
      return a.Equals( b );
    }

    /// <summary>
    /// Support for the != operator.
    /// </summary>
    public static bool operator !=( Toe a, Toe b )
    {
      if( (object)a == null ) return (object)b != null;
      return !a.Equals( b );
    }

    /// <summary>
    /// Compare this object with another Toe object.
    /// </summary>
    /// <param name="other">The other Toe to compare with.</param>
    /// <returns>-1, 0, 1</returns>
    public int CompareTo( Toe other )
    {
      if( other == null ) return 1;
      return this.name.CompareTo( other.name );
    }

    #endregion
  }


  /// <summary>
  /// A HCUnitMove represents a brigade move request made by a high command player
  /// and it's associated completion attempt ~2.5mins later.
  /// </summary>
  public class HCUnitMove : IComparable<HCUnitMove>
  {
    #region Variables

    private HCUnitMoveState state = HCUnitMoveState.Unknown;

    private DateTime receivedTime;
    private ChokePoint from;
    private ChokePoint to;
    private string player;

    private DateTime requestTime;
    private DateTime completedTime;
    private HCUnitMoveResult requestResult;
    private HCUnitMoveResult completedResult;

    #endregion

    #region Constructors

    /// <summary>
    /// Create a new HCUnitMove with the initial request details.
    /// </summary>
    /// <param name="from">The ChokePoint the unit is to be moved from.</param>
    /// <param name="to">The ChokePoint the unit is to be moved to.</param>
    /// <param name="player">The name of the player that requested the move.</param>
    /// <param name="requestTime">The time the request was made.</param>
    /// <param name="requestResult">The result of the move request.</param>
    public HCUnitMove( ChokePoint from, ChokePoint to, string player, DateTime requestTime, HCUnitMoveResult requestResult )
    {
      this.receivedTime = DateTime.Now;
      this.from = from;
      this.to = to;
      this.player = player;
      this.requestTime = requestTime;
      this.requestResult = requestResult;
      this.state = requestResult.ID == 0 ? HCUnitMoveState.Pending : HCUnitMoveState.RequestFailed;
    }

    /// <summary>
    /// Create a new system HCUnitMove.
    /// </summary>
    /// <param name="from">The ChokePoint the unit is to be moved from.</param>
    /// <param name="to">The ChokePoint the unit is to be moved to.</param>
    /// <param name="time">The time the request was made.</param>
    /// <param name="result">The result of the move (always successful).</param>
    public HCUnitMove( ChokePoint from, ChokePoint to, DateTime time, HCUnitMoveResult result )
    {
      this.receivedTime = DateTime.Now;
      this.from = from;
      this.to = to;
      this.player = "SYSTEM";
      this.requestTime = this.completedTime = time;
      this.requestResult = this.completedResult = result;
      this.state = HCUnitMoveState.Success;
    }

    #endregion

    #region Properties

    /// <summary>
    /// The current state of this move request.
    /// </summary>
    public HCUnitMoveState State
    {
      get { return this.state; }
    }

    /// <summary>
    /// The time the initial request was received.
    /// </summary>
    public DateTime ReceivedTime
    {
      get { return this.receivedTime; }
    }

    /// <summary>
    /// The ChokePoint the unit was moved from.
    /// </summary>
    public ChokePoint From
    {
      get { return this.from; }
    }

    /// <summary>
    /// The ChokePoint the unit was moved to.
    /// </summary>
    public ChokePoint To
    {
      get { return this.to; }
    }

    /// <summary>
    /// The name of the player that requested the move.
    /// </summary>
    public string Player
    {
      get { return this.player; }
    }

    /// <summary>
    /// The time the initial request was made.
    /// </summary>
    public DateTime RequestTime
    {
      get { return this.requestTime; }
    }

    /// <summary>
    /// The time the unit was moved.
    /// </summary>
    public DateTime CompletedTime
    {
      get { return this.completedTime; }
    }

    /// <summary>
    /// The time of this event, based on state.
    /// </summary>
    public DateTime Time
    {
      get
      {
        if( this.state == HCUnitMoveState.RequestFailed || this.state == HCUnitMoveState.Pending )
          return this.requestTime;
        else
          return this.completedTime;
      }
    }

    /// <summary>
    /// The result of the move request.
    /// </summary>
    public HCUnitMoveResult RequestResult
    {
      get { return this.requestResult; }
    }

    /// <summary>
    /// The result of the completion attempt.
    /// </summary>
    public HCUnitMoveResult CompletedResult
    {
      get { return this.completedResult; }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Checks to see if this is a pending request that matches the given details.
    /// Used to pair requests to completion attempts.
    /// </summary>
    /// <param name="from">The ChokePoint the unit is to be moved from.</param>
    /// <param name="to">The ChokePoint the unit is to be moved to.</param>
    /// <param name="player">The name of the player that requested the move.</param>
    /// <returns>True if this is the matching request.</returns>
    public bool IsMatch( ChokePoint from, ChokePoint to, string player )
    {
      if( this.RequestResult.ID != 0 || this.CompletedTime != DateTime.MinValue )
        return false;

      return this.from == from && this.To == to && this.Player == player;
    }

    /// <summary>
    /// Adds completion details to a pending move request.
    /// </summary>
    /// <param name="completedTime">The time the completion attempt was made.</param>
    /// <param name="completedResult">The result of the completion attempt.</param>
    public void AddCompletion( DateTime completedTime, HCUnitMoveResult completedResult )
    {
      this.completedTime = completedTime;
      this.completedResult = completedResult;

      this.state = completedResult.ID == 0 ? HCUnitMoveState.Success : HCUnitMoveState.CompletionFailed;
    }

    /// <summary>
    /// Cancels a pending request when another request has been made.
    /// </summary>
    public void Cancel()
    {
      Cancel( this.requestTime.AddMinutes( 3 ) );
    }

    /// <summary>
    /// Cancels a pending request when another request has been made.
    /// </summary>
    /// <param name="time">The time the move was cancelled.</param>
    public void Cancel( DateTime time )
    {
      this.state = HCUnitMoveState.Cancelled;
      this.completedTime = time;
    }

    /// <summary>
    /// Remove any circular references to allow garbage collection.
    /// </summary>
    public void Cleanup()
    {
      this.from = null;
      this.to = null;
      this.requestResult = null;
      this.completedResult = null;
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Provides a string representation of this object.
    /// </summary>
    /// <returns>"From->To = Result"</returns>
    public override string ToString()
    {
      string result;
      switch( this.State )
      {
        case HCUnitMoveState.RequestFailed:
          result = this.RequestResult.ToString();
          break;
        case HCUnitMoveState.CompletionFailed:
        case HCUnitMoveState.Success:
          result = this.CompletedResult.ToString();
          break;
        default:
          result = this.State.ToString();
          break;
      }

      return String.Format( "{0}->{1} = {2}", this.From, this.To, result );
    }

    /// <summary>
    /// Returns a value indicating whether this instance is equal to a specified object value.
    /// </summary>
    /// <param name="obj">An object to compare with this instance.</param>
    /// <returns>True if obj is the same HCUnitMove.</returns>
    public override bool Equals( object obj )
    {
      HCUnitMove other = obj as HCUnitMove;
      if( other == null ) return false;

      if( this.receivedTime != other.receivedTime ) return false;
      if( this.from != other.from ) return false;
      if( this.to != other.to ) return false;
      if( this.player != other.player ) return false;
      if( this.requestTime != other.requestTime ) return false;
      if( this.completedTime != other.completedTime ) return false;
      if( this.requestResult != other.requestResult ) return false;
      if( this.completedResult != other.completedResult ) return false;
      
      return true;
    }

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    public override int GetHashCode()
    {
      return base.GetHashCode();
    }

    /// <summary>
    /// Support for the == operator.
    /// </summary>
    public static bool operator ==( HCUnitMove a, HCUnitMove b )
    {
      if( (object)a == null ) return (object)b == null;
      return a.Equals( b );
    }

    /// <summary>
    /// Support for the != operator.
    /// </summary>
    public static bool operator !=( HCUnitMove a, HCUnitMove b )
    {
      if( (object)a == null ) return (object)b != null;
      return !a.Equals( b );
    }

    /// <summary>
    /// Compare this object with another HCUnitMove object.
    /// </summary>
    /// <param name="other">The other HCUnitMove to compare with.</param>
    /// <returns>-1, 0, 1</returns>
    public int CompareTo( HCUnitMove other )
    {
      if( other == null ) return 1;
      return this.requestTime.CompareTo( other.requestTime );
    }

    #endregion
  }


  /// <summary>
  /// The result of a brigade move request.
  /// </summary>
  public class HCUnitMoveResult : IComparable<HCUnitMoveResult>
  {
    #region Variables

    private readonly int id;
    private readonly string description;

    #endregion

    #region Constructors

    /// <summary>
    /// Create a new HCUnitMoveResult.
    /// </summary>
    /// <param name="id">The move result id (should match Language.Enum_HCMoveResult_...).</param>
    /// <param name="desc">The move result description.</param>
    public HCUnitMoveResult( int id, string desc )
    {
      this.id = id;
      this.description = desc;

      string localDescription = Language.ResourceManager.GetString( String.Format( "Enum_HCMoveResult_{0:000}", id ) );
      if( localDescription != null )
        this.description = localDescription;
    }

    #endregion

    #region Properties

    /// <summary>
    /// The unique move result identifier.
    /// </summary>
    public int ID
    {
      get { return this.id; }
    }

    /// <summary>
    /// The move result description.
    /// </summary>
    public string Description
    {
      get { return this.description; }
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Provides a string representation of this object.
    /// </summary>
    /// <returns>The move result description.</returns>
    public override string ToString()
    {
      return this.description;
    }

    /// <summary>
    /// Returns a value indicating whether this instance is equal to a specified object value.
    /// </summary>
    /// <param name="obj">An object to compare with this instance.</param>
    /// <returns>True if obj is the same HCUnitMoveResult.</returns>
    public override bool Equals( object obj )
    {
      return obj is HCUnitMoveResult && this.id == ( (HCUnitMoveResult)obj ).id;
    }

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    public override int GetHashCode()
    {
      return this.id;
    }

    /// <summary>
    /// Support for the == operator.
    /// </summary>
    public static bool operator ==( HCUnitMoveResult a, HCUnitMoveResult b )
    {
      if( (object)a == null ) return (object)b == null;
      return a.Equals( b );
    }

    /// <summary>
    /// Support for the != operator.
    /// </summary>
    public static bool operator !=( HCUnitMoveResult a, HCUnitMoveResult b )
    {
      if( (object)a == null ) return (object)b != null;
      return !a.Equals( b );
    }

    /// <summary>
    /// Compare this object with another HCUnitMoveResult object.
    /// </summary>
    /// <param name="other">The other HCUnitMoveResult to compare with.</param>
    /// <returns>-1, 0, 1</returns>
    public int CompareTo( HCUnitMoveResult other )
    {
      if( other == null ) return 1;
      return this.id.CompareTo( other.id );
    }

    #endregion
  }


  /// <summary>
  /// A entry in ww2online's server configuration.
  /// </summary>
  public class ServerParam : IComparable<ServerParam>
  {
    #region Variables

    private readonly string key;
    private readonly string value;
    private readonly string desc;

    private readonly string section;

    #endregion

    #region Constructors

    /// <summary>
    /// Create a new ServerParam.
    /// </summary>
    /// <param name="key">The item key.</param>
    /// <param name="value">The item value.</param>
    /// <param name="desc">The item long description.</param>
    public ServerParam( string key, string value, string desc )
    {
      this.key = key;
      this.value = value;
      this.desc = desc;

      int idx = key.LastIndexOf( '.' );
      if( idx < 0 )
        this.section = "Misc";
      else
        this.section = key.Substring( 0, idx ).ToTitleCase();
    }

    #endregion

    #region Properties

    /// <summary>
    /// The full key of the config item.
    /// </summary>
    public string Key
    {
      get { return this.key; }
    }

    /// <summary>
    /// The value of the config item.
    /// </summary>
    public string Value
    {
      get { return this.value; }
    }

    /// <summary>
    /// The long description of the config item.
    /// </summary>
    public string Description
    {
      get { return this.desc; }
    }

    /// <summary>
    /// The config item's section.
    /// </summary>
    public string Section
    {
      get { return this.section; }
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Provides a string representation of this object.
    /// </summary>
    /// <returns>"key = value"</returns>
    public override string ToString()
    {
      return String.Format( "{0} = {1}", this.key, this.value );
    }

    /// <summary>
    /// Returns a value indicating whether this instance is equal to a specified object value.
    /// </summary>
    /// <param name="obj">An object to compare with this instance.</param>
    /// <returns>True if obj is the same ServerParam.</returns>
    public override bool Equals( object obj )
    {
      if( !( obj is ServerParam ) ) return false;
      ServerParam other = (ServerParam)obj;

      if( this.key   != other.key   ) return false;
      if( this.value != other.value ) return false;

      return true;
    }

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    public override int GetHashCode()
    {
      return base.GetHashCode();
    }

    /// <summary>
    /// Support for the == operator.
    /// </summary>
    public static bool operator ==( ServerParam a, ServerParam b )
    {
      if( (object)a == null ) return (object)b == null;
      return a.Equals( b );
    }

    /// <summary>
    /// Support for the != operator.
    /// </summary>
    public static bool operator !=( ServerParam a, ServerParam b )
    {
      if( (object)a == null ) return (object)b != null;
      return !a.Equals( b );
    }

    /// <summary>
    /// Compare this object with another ServerParam object.
    /// </summary>
    /// <param name="other">The other ServerParam to compare with.</param>
    /// <returns>-1, 0, 1</returns>
    public int CompareTo( ServerParam other )
    {
      if( other == null ) return 1;

      int idx = this.section.CompareTo( other.section );

      if( idx == 0 )  // same section, sort by key name
        return this.key.CompareTo( other.key );

      if( idx != 0 && ( this.section == "Misc" || other.section == "Misc" ) )  // diff section, sort "Misc" last
        return this.section == "Misc" ? 1 : -1;

      return idx;
    }

    #endregion
  }

  #endregion
}
