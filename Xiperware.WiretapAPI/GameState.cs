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
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using XLib.ThirdParty;

namespace Xiperware.WiretapAPI
{
  /// <summary>
  /// The GameState class encapsulates all information to do with the current state of the
  /// game. This includes all Countrys, ChokePoints and their Facility's, etc.
  /// </summary>
  public class GameState
  {
    #region Variables

    private Wiretap wiretap;
    private List<ChokePoint> dummyCPs;

    #endregion

    #region AutoProperties

    /// <summary>
    /// An array of all Countrys, indexed by country id.
    /// </summary>
    public Country[] Countries { get; private set; }

    /// <summary>
    /// An array of all player Ranks, indexed by rank level.
    /// </summary>
    public Rank[] Ranks { get; private set; }

    /// <summary>
    /// An array of all ChokePoints, indexed by chokepoint id.
    /// </summary>
    public ChokePoint[] ChokePoints { get; set; }

    /// <summary>
    /// An array of all Facilitys, indexed by facility id.
    /// </summary>
    public Facility[] Facilities { get; set; }

    /// <summary>
    /// An array of all Bridges, indexed by chokepoint id.
    /// </summary>
    public Bridge[] Bridges { get; set; }

    /// <summary>
    /// A hash of all HCUnits, keyed by unit id.
    /// </summary>
    public Dictionary<int, HCUnit> HCUnits { get; private set; }

    /// <summary>
    /// A hash of all Factorys, keyed by facility id.
    /// </summary>
    /// <remarks>Factorys also appear in the Facility array.</remarks>
    public Dictionary<int, Factory> Factories { get; private set; }

    /// <summary>
    /// A hash of all ChokePoint Links, keyed by both cp ids.
    /// </summary>
    public SymmetricHash<int, SupplyLink> Links { get; private set; }

    /// <summary>
    /// A hash of all Firebases, keyed by facility id.
    /// </summary>
    public Dictionary<int, Firebase> Firebases { get; private set; }

    /// <summary>
    /// A hash of all Squads, keyed by squad id.
    /// </summary>
    public Dictionary<int, Squad> Squads { get; private set; }

    /// <summary>
    /// A hash of cells on the map, keyed by x/y Point in game meters.
    /// </summary>
    public Dictionary<Point, MapCell> MapCells { get; private set; }

    /// <summary>
    /// A hash of game server instances, keyed by server id.
    /// </summary>
    public Dictionary<int, Server> Servers { get; private set; }

    /// <summary>
    /// An array of all Move Results, indexed by id.
    /// </summary>
    public HCUnitMoveResult[] MoveResults { get; set; }

    /// <summary>
    /// An array of Vehicle Categories, indexed by id.
    /// </summary>
    public VehicleCategory[] VehicleCategories { get; set; }

    /// <summary>
    /// An array of Vehicle Classes, indexed by id.
    /// </summary>
    public VehicleClass[] VehicleClasses { get; set; }

    /// <summary>
    /// An array of Vehicles, indexed by vehicle id.
    /// </summary>
    public Vehicle[] Vehicles { get; set; }

    /// <summary>
    /// A hash of all Toes, keyed by toe code.
    /// </summary>
    public ObservableDictionary<string, Toe> Toes { get; private set; }

    /// <summary>
    /// A hash of all ww2 config items, keyed by item name.
    /// </summary>
    public Dictionary<string, ServerParam> ServerParams { get; private set; }

    /// <summary>
    /// Gets the cell width of the deathmap data, as defined in the xml.
    /// </summary>
    public int MapCellSize { get; set; }

    /// <summary>
    /// The current Allied frontline.
    /// </summary>
    public Frontline AlliedFrontline { get; private set; }

    /// <summary>
    /// The current Axis frontline.
    /// </summary>
    public Frontline AxisFrontline { get; private set; }

    /// <summary>
    /// A list of both normal and dummy ChokePoints.
    /// </summary>
    public List<ChokePoint> AllCPs { get; private set; }

    /// <summary>
    /// The collection of events related to this game state, or null if not tracking events.
    /// </summary>
    public GameEventCollection Events { get; set; }

    #endregion

    #region Constructors

    /// <summary>
    /// Create a new Wiretap object.
    /// </summary>
    public GameState()
    {
      this.wiretap = null;
      this.dummyCPs = new List<ChokePoint>();


      // define countries

      this.Countries    = new Country[10];
      this.Countries[0] = new Country( 0, "NONE", Language.Country_Name_NoCountry      , Language.Country_Name_NoCountry      , Side.None    );
      this.Countries[1] = new Country( 1, "UK"  , Language.Country_Name_England        , Language.Country_Demonym_England     , Side.Allied  );
      this.Countries[2] = new Country( 2, "US"  , Language.Country_Name_USA            , Language.Country_Demonym_USA         , Side.Allied  );  // also bridge destroyed state
      this.Countries[3] = new Country( 3, "FR"  , Language.Country_Name_France         , Language.Country_Demonym_France      , Side.Allied  );
      this.Countries[4] = new Country( 4, "DE"  , Language.Country_Name_Germany        , Language.Country_Demonym_Germany     , Side.Axis    );
      this.Countries[5] = new Country( 5, "IT"  , Language.Country_Name_Italy          , Language.Country_Demonym_Italy       , Side.None    );
      this.Countries[6] = new Country( 6, "JP"  , Language.Country_Name_Japan          , Language.Country_Demonym_Japan       , Side.None    );
      this.Countries[7] = new Country( 7, "CW"  , Language.Country_Name_Commonwealth   , Language.Country_Demonym_Commonwealth, Side.None    );
      this.Countries[8] = new Country( 8, "NF"  , Language.Country_Name_NeutralFriendly, Language.Country_Name_NeutralFriendly, Side.Neutral );  // was China
      this.Countries[9] = new Country( 9, "NH"  , Language.Country_Name_NeutralHostile , Language.Country_Name_NeutralHostile , Side.Neutral );  // was Russia


      // get ranks

      this.Ranks = Data.GetRanks();


      // initialise empty game object collections

      this.ChokePoints       = new ChokePoint[0];
      this.Facilities        = new Facility[0];
      this.Bridges           = new Bridge[0];
      this.HCUnits           = new Dictionary<int, HCUnit>();
      this.Factories         = new Dictionary<int, Factory>();
      this.Links             = new SymmetricHash<int, SupplyLink>();
      this.Firebases         = new Dictionary<int, Firebase>();
      this.Squads            = new Dictionary<int, Squad>();
      this.MapCells          = new Dictionary<Point, MapCell>();
      this.Servers           = new Dictionary<int, Server>();
      this.MoveResults       = new HCUnitMoveResult[0];
      this.VehicleCategories = new VehicleCategory[0];
      this.VehicleClasses    = new VehicleClass[0];
      this.Vehicles          = new Vehicle[0];
      this.Toes              = new ObservableDictionary<string, Toe>();
      this.ServerParams      = new Dictionary<string, ServerParam>();
      this.AlliedFrontline   = new Frontline( Side.Allied );
      this.AxisFrontline     = new Frontline( Side.Axis );

      this.MapCellSize = 1000;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the wiretap object currently being used to update this game state, if any.
    /// </summary>
    public Wiretap Wiretap
    {
      get { return this.wiretap; }
      set
      {
        this.wiretap = value;
        this.wiretap.GameState = this;
      }
    }

    /// <summary>
    /// The top-level HCUnit that contains all other HCUnits.
    /// </summary>
    public HCUnit TopHCUnit
    {
      get { return this.HCUnits.ContainsKey( 0 ) ? this.HCUnits[0] : null; }
    }

    /// <summary>
    /// An enumerable collection of valid (non-null) ChokePoints.
    /// </summary>
    public IEnumerable<ChokePoint> ValidChokePoints
    {
      get { return this.ChokePoints.Where( cp => cp != null ); }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Generate additional ChokePoint data (nearby cps, dummy cps, frontlines, alt).
    /// </summary>
    public void GenerateCPData()
    {
      // generate the nearby cps for normal chokepoints
      GenerateNearbyChokePoints( this.ChokePoints, this.ChokePoints );

      // create dummy cps
      GenerateDummyChokePoints();

      // get list of all normal and dummy cps
      this.AllCPs = new List<ChokePoint>( this.ChokePoints );
      AllCPs.AddRange( this.dummyCPs );

      // generate the nearby cps for dummy chokepoints
      GenerateNearbyChokePoints( this.dummyCPs, this.AllCPs );

      // assign cps to frontlines
      this.AlliedFrontline.SetChokePointList( this.AllCPs );
      this.AxisFrontline.SetChokePointList( this.AllCPs );
      
      // add the altitude to each cp
      AddAltitudeData();
    }

    /// <summary>
    /// Remove references to local game objects.
    /// </summary>
    public void Cleanup()
    {
      if( this.dummyCPs != null )
      {
        foreach( ChokePoint cp in this.dummyCPs )
          if( cp != null )
            cp.Cleanup();
        this.dummyCPs = new List<ChokePoint>();
      }

      if( this.ChokePoints != null )
      {
        foreach( ChokePoint cp in this.ChokePoints )
          if( cp != null )
            cp.Cleanup();
        this.ChokePoints = new ChokePoint[0];
      }

      if( this.Facilities != null )
      {
        foreach( Facility facility in this.Facilities )
          if( facility != null )
            facility.Cleanup();
        this.Facilities = new Facility[0];
      }

      this.Bridges = new Bridge[0];

      if( this.HCUnits != null )
      {
        foreach( HCUnit hcunit in this.HCUnits.Values )
          hcunit.Cleanup();
        this.HCUnits.Clear();
        this.HCUnits = new Dictionary<int, HCUnit>();
      }

      if( this.Factories != null )
      {
        this.Factories.Clear();
        this.Factories = new Dictionary<int, Factory>();
      }

      if( this.Links != null )
      {
        foreach( SupplyLink link in this.Links )
          link.Cleanup();
        this.Links.Clear();
        this.Links = new SymmetricHash<int, SupplyLink>();
      }

      if( this.Firebases != null )
      {
        foreach( Firebase fb in this.Firebases.Values )
          fb.Cleanup();
        this.Firebases.Clear();
        this.Firebases = new Dictionary<int, Firebase>();
      }

      if( this.Squads != null )
      {
        this.Squads.Clear();
        this.Squads = new Dictionary<int, Squad>();
      }

      if( this.MapCells != null )
      {
        this.MapCells.Clear();
        this.MapCells = new Dictionary<Point, MapCell>();
      }

      if( this.Servers != null )
      {
        this.Servers.Clear();
        this.Servers = new Dictionary<int, Server>();
      }

      this.MoveResults = new HCUnitMoveResult[0];

      this.VehicleCategories = new VehicleCategory[0];

      if( this.VehicleClasses != null )
      {
        foreach( VehicleClass vehclass in this.VehicleClasses )
          if( vehclass != null )
            vehclass.Cleanup();
        this.VehicleClasses = new VehicleClass[0];
      }

      if( this.Vehicles != null )
      {
        foreach( Vehicle vehicle in this.Vehicles )
          if( vehicle != null )
            vehicle.Cleanup();
        this.Vehicles = new Vehicle[0];
      }

      if( this.Toes != null )
      {
        foreach( Toe toe in this.Toes.Values )
          toe.Cleanup();
        this.Toes.Clear();
        this.Toes = new ObservableDictionary<string, Toe>();
      }

      if( this.ServerParams != null )
      {
        this.ServerParams.Clear();
        this.ServerParams = new Dictionary<string, ServerParam>();
      }

      if( this.AlliedFrontline != null )
      {
        this.AlliedFrontline.Cleanup();
        this.AlliedFrontline = new Frontline( Side.Allied );
      }

      if( this.AxisFrontline != null )
      {
        this.AxisFrontline.Cleanup();
        this.AxisFrontline = new Frontline( Side.Axis );
      }

      if( this.AllCPs != null )
      {
        foreach( ChokePoint cp in this.AllCPs )
          if( cp != null )
            cp.Cleanup();
        this.AllCPs = new List<ChokePoint>();
      }

      if( this.Events != null )
      {
        this.Events.Cleanup();
        this.Events = new GameEventCollection();
      }
    }

    /// <summary>
    /// Calculate the neighbouring ChokePoints for each cp.
    /// </summary>
    private void GenerateNearbyChokePoints( IList<ChokePoint> todo, IList<ChokePoint> allCps )
    {
      for( int iCurrent = 0; iCurrent < todo.Count; iCurrent++ )
      {
        if( todo[iCurrent] == null ) continue;
        ChokePoint cpCurrent = todo[iCurrent];


        // get list of all nearby cps, sorted by angle, and the distance to each

        SortedList<double, ChokePoint> allnearby = new SortedList<double, ChokePoint>();
        Dictionary<ChokePoint, double> distances = new Dictionary<ChokePoint, double>();


        // initially populate with linked cps

        foreach( ChokePoint cpLinked in cpCurrent.LinkedChokePoints )
        {
          double distance = Misc.DistanceBetween( cpCurrent.LocationOctets, cpLinked.LocationOctets );
          double angle = Misc.AngleBetween( cpCurrent.LocationOctets, cpLinked.LocationOctets );

          if( distance > Frontline.MAX_LINK_DIST )
            continue;  // don't include eg cross channel links

          if( allnearby.ContainsKey( angle ) )
            angle += 0.001;  // allow dupes

          allnearby.Add( angle, cpLinked );
          distances.Add( cpLinked, distance );
        }


        // then add any other cps within MAXDISTANCE

        for( int iNear = 0; iNear < allCps.Count; iNear++ )
        {
          if( allCps[iNear] == null ) continue;
          ChokePoint cpNear = allCps[iNear];

          if( cpNear.ID == cpCurrent.ID ) continue;  // self
          if( distances.ContainsKey( cpNear ) ) continue;  // linked cp



          // get distance

          double distance = Misc.DistanceBetween( cpCurrent.LocationOctets, cpNear.LocationOctets );
          if( distance > Frontline.MAX_DISTANCE )
            continue;


          // check angle

          double angle = Misc.AngleBetween( cpCurrent.LocationOctets, cpNear.LocationOctets );
          if( allnearby.ContainsKey( angle ) )  // dupe angle
          {
            ChokePoint cpDupe = allnearby[angle];

            if( cpCurrent.LinkedChokePoints.Contains( cpDupe ) )
              continue;                   // cpDupe is a linked cp, skip cpNear
            else if( distance < distances[cpDupe] )
              allnearby.Remove( angle );  // cpNear is closer, remove cpDupe
            else
              continue;                   // cpDupe is closer, skip cpNear
          }


          // append to lists

          allnearby.Add( angle, cpNear );
          distances.Add( cpNear, distance );
        }


        // assign result to chokepoint object

        cpCurrent.NearbyChokePoints = allnearby.Values;

      }  // end cpCurrent


      // correct all 1-way links to make symmetric

      foreach( ChokePoint cpCurrent in todo )
      {
        if( cpCurrent == null ) continue;

        List<ChokePoint> toFix = new List<ChokePoint>();

        foreach( ChokePoint cpNear in cpCurrent.NearbyChokePoints )
          if( !cpNear.IsNear( cpCurrent ) )
            toFix.Add( cpNear );

        foreach( ChokePoint cpNear in toFix )
        {
          if( cpCurrent.ID >= this.ChokePoints.Length )  // dummy cp, add reverse link
            cpNear.AddNearCP( cpCurrent );
          else                                      // normal cp, remove 1-way link
            cpCurrent.RemoveNearCP( cpNear );
        }
      }

    }

    /// <summary>
    /// Generate a list of "dummy cps" that surround the outline of normal cps (used for frontline).
    /// </summary>
    private void GenerateDummyChokePoints()
    {
      this.dummyCPs.Clear();


      // create outline hull

      Frontline outline = new Frontline( Side.Neutral, this.ChokePoints );
      outline.Update();


      // for each cp pair in each hull, create dummy cp between them offset 90deg

      int id = this.ChokePoints.Length;
      foreach( List<ChokePoint> hull in outline.Hulls )
      {
        ChokePoint prevCp = hull[hull.Count - 1];  // last item
        foreach( ChokePoint cp in hull )
        {
          Point pt1 = prevCp.LocationOctets;
          Point pt2 = cp.LocationOctets;

          double distance = Misc.DistanceBetween( pt1, pt2 );
          double angle = Misc.AngleBetween( pt1, pt2 );

          Point ptMid = Misc.AngleOffset( cp.LocationOctets, angle, distance / 2 );
          Point ptMidOffset = Misc.AngleOffset( ptMid, angle + 90, Frontline.DUMMYCP_DISTANCE );

          this.dummyCPs.Add( new ChokePoint( id, String.Format( "({0}-{1})", prevCp, cp ), ChokePointType.Dummy, this.Countries[0], ptMidOffset, null ) );

          prevCp = cp;
          id++;
        }
      }
    }

    /// <summary>
    /// Populate the cp.AltitudeMeters property for each ChokePoint.
    /// </summary>
    private void AddAltitudeData()
    {
      int[] alt = Data.GetAltitudes( this.ChokePoints.Length );

      for( int i = 0; i < alt.Length; i++ )
      {
        if( alt[i] == 0 || this.ChokePoints[i] == null ) continue;

        this.ChokePoints[i].AltitudeMeters = alt[i];
      }
    }

    /// <summary>
    /// Updates the activity level for each cp based on recent nearby deaths and captures.
    /// </summary>
    public void UpdateActivityLevels()
    {
      // counts deaths near each cp

      int[] deaths = new int[this.ChokePoints.Length];
      foreach( KeyValuePair<Point, MapCell> cell in this.MapCells )
        foreach( ChokePoint cp in this.ValidChokePoints.Where( cp => cp.IsNearPoint( cell.Key ) ) )  // AccessToModifiedClosure okay
          deaths[cp.ID] += cell.Value.RecentDeaths;


      // count capture and fb blown events for each cp

      DateTime start = DateTime.Now.AddMinutes( -20 );
      int[] captures = new int[this.ChokePoints.Length];
      foreach( GameEvent gameEvent in this.Events )
      {
        if( gameEvent.EventTime < start ) continue;

        if( gameEvent is ICaptureFacilityGameEvent )
          captures[gameEvent.ChokePoints[0].ID]++;

        if( gameEvent.Type == GameEventType.Firebase )
          foreach( ChokePoint cp in gameEvent.ChokePoints )
            captures[cp.ID] += 2;  // fb blown = 2 captures
      }


      // update each cp's activity level

      for( int i = 0; i < this.ChokePoints.Length; i++ )
      {
        if( this.ChokePoints[i] == null ) continue;

        int activity = deaths[i] + ( captures[i] * 4 );  // capture = 4 deaths

        if( activity == 0 ) this.ChokePoints[i].Activity = ActivityLevel.None;
        else if( activity > 80 ) this.ChokePoints[i].Activity = ActivityLevel.Heavy;
        else if( activity > 40 ) this.ChokePoints[i].Activity = ActivityLevel.Moderate;
        else if( activity > 20 ) this.ChokePoints[i].Activity = ActivityLevel.Light;
        else this.ChokePoints[i].Activity = ActivityLevel.Low;
      }

    }

    /// <summary>
    /// Prunes the move history for all HCUnits.
    /// </summary>
    public void PruneHCUnitMoveHistory()
    {
      foreach( HCUnit hcunit in this.HCUnits.Values )
        hcunit.PruneMoveHistory();
    }

    /// <summary>
    /// Updates the pair of frontlines to match current cp ownership.
    /// </summary>
    public void UpdateFrontlines()
    {
      this.AlliedFrontline.Update();
      this.AxisFrontline.Update( this.AlliedFrontline.Hulls );
    }

    /// <summary>
    /// Checks each firebase pair and makes sure each active link has an open firebase.
    /// </summary>
    /// <returns>A list of firebases that have been opened.</returns>
    public List<Firebase> OpenFirebases()
    {
      List<Firebase> openedFirebases = new List<Firebase>();

      foreach( Firebase fb in this.Firebases.Values )
      {
        if( fb.ID < fb.LinkedFirebase.ID ) continue;  // only process each pair once

        if( fb.Link.State == FirebaseState.Inactive )
        {
          Debug.Assert( !fb.IsOpen && !fb.LinkedFirebase.IsOpen, "inactive link has open fb" );
          continue;  // ignore inactive fbs
        }
        Debug.Assert( !(fb.IsOpen && fb.LinkedFirebase.IsOpen), "active link has both fbs open" );
        if( fb.IsOpen || fb.LinkedFirebase.IsOpen ) continue;  // all ok


        // determine which firebase to open

        Firebase newOpenFb;

        if( fb.Link.State == FirebaseState.Brigade )  // open friendly frontline fb
          newOpenFb = fb.LinkedChokePoint.IsFrontline ? fb : fb.LinkedFirebase;
        else // pseudo-randomly open either fb
          newOpenFb = (fb.ID + fb.LinkedFirebase.ID) % 2 == 0 ? fb : fb.LinkedFirebase;


        // open the fb

        newOpenFb.IsOpen = true;
        openedFirebases.Add( newOpenFb );
        Log.AddEntry( "Opened {0} as {1}", newOpenFb, newOpenFb.Link.Side );
      }

      return openedFirebases;
    }

    #endregion
  }
}
