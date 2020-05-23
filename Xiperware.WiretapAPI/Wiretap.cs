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
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Xiperware.WiretapAPI
{
  #region Enums

  /// <summary>
  /// Data from Wiretap that changes infrequently and can be cached.
  /// </summary>
  /// <remarks>When subscribed will create MetaXmlFile objects.</remarks>
  [Flags]
  public enum Metadata
  {
    /// <summary>
    /// No subscribed meta data.
    /// </summary>
    None = 0,

    /// <summary>
    /// Chokepoint metadata (/xml/cplist.xml).
    /// </summary>
    MapChokePoint = 1<<0,
    /// <summary>
    /// Facility metadata (/xml/facilitylist.xml).
    /// </summary>
    MapFacility = 1<<1,
    /// <summary>
    /// Link metadata (/xml/links.xml).
    /// </summary>
    MapLinks = 1<<2,
    /// <summary>
    /// All map metadata (cps, facilities, links).
    /// </summary>
    Map = MapChokePoint | MapFacility | MapLinks,
    
    /// <summary>
    /// Vehicle category metadata (/xml/vehcat.xml).
    /// </summary>
    VehicleCategory = 1<<3,
    /// <summary>
    /// Vehicle class metadata (/xml/vehclass.xml).
    /// </summary>
    VehicleClass = 1<<4,
    /// <summary>
    /// Vehicle info metadata (/xml/vehicles.xml).
    /// </summary>
    VehicleInfo = 1<<5,
    /// <summary>
    /// All vehicle metadata (category, class, info).
    /// </summary>
    Vehicle = VehicleCategory | VehicleClass | VehicleInfo,

    /// <summary>
    /// Toe info metadata (/xml/toes.list.xml).
    /// </summary>
    ToesInfo = 1<<6,
    /// <summary>
    /// Toe content metadata (/xml/toes.sheet.xml).
    /// </summary>
    ToesContent = 1<<7,
    /// <summary>
    /// All toe metadata (info, content).
    /// </summary>
    Toes = ToesInfo | ToesContent,

    /// <summary>
    /// HCUnit info metadata (/xml/hcunitlist.xml).
    /// </summary>
    HCUnitList = 1<<8,
    /// <summary>
    /// HCUnit move results metadata (/xml/moveresults.xml).
    /// </summary>
    HCUnitMoveResults = 1<<9,
    /// <summary>
    /// All hcunit metadata (info, move results).
    /// </summary>
    HCUnit = HCUnitList | HCUnitMoveResults,

    /// <summary>
    /// Squad info metadata (/xml/squadlist.xml).
    /// </summary>
    Squad = 1<<10,

    /// <summary>
    /// Server config metadata (/xml/config.xml).
    /// </summary>
    Config = 1<<11,

    /// <summary>
    /// All available meta data.
    /// </summary>
    All = Map | Vehicle | Toes | HCUnit | Squad | Config,
  }

  /// <summary>
  /// Data from Wiretap that changes frequently and needs to be kept up-to-date.
  /// </summary>
  /// <remarks>When subscribed will create InitXmlFile and PollXmlFile objects.</remarks>
  [Flags]
  public enum Livedata
  {
    /// <summary>
    /// No subscribed live data.
    /// </summary>
    None = 0,

    /// <summary>
    /// Current CP owners and AO state (/xmlquery/cps.xml).
    /// </summary>
    ChokePoint = 1<<0,
    /// <summary>
    /// Past and future flag captures and AO changes (/xmlquery/captures.xml, /xmlquery/cps.xml?aos=true).
    /// </summary>
    Capture = 1<<1,
    /// <summary>
    /// Current HCUnit deployments (/xmlquery/hclocationsext.xml).
    /// </summary>
    HCUnit = 1<<2,
    /// <summary>
    /// Past and future brigade movements and requests (/xmlquery/hcmovereq.xml, /xmlquery/hcmoves.xml).
    /// </summary>
    Movement = 1<<3,
    /// <summary>
    /// Current and future fb ownerships (/xml/openfbs.xml).
    /// </summary>
    Firebase = 1<<4,
    /// <summary>
    /// Past and future player deaths (/xml/deathmap.1h.xml, /xml/deathmap.5m.xml, /xml/deathmap.1m.xml).
    /// </summary>
    Deathmap = 1<<5,
    /// <summary>
    /// Current and future server status (/xml/servers.xml).
    /// </summary>
    Servers = 1<<6,
    /// <summary>
    /// Past and future factory and rdp data (/xml/factorystats.xml, /xmlquery/factorylog.xml, /xml/rdp.xml).
    /// </summary>
    Factory = 1<<7,

    /// <summary>
    /// All available live data.
    /// </summary>
    All = ChokePoint | Capture | HCUnit | Movement | Firebase | Deathmap | Servers | Factory,
  }

  #endregion

  /// <summary>
  /// The Wiretap class is used to populate a GameState by downloading xml data from Wiretap,
  /// a service provided (currently unofficially) by CRS.
  /// </summary>
  public class Wiretap
  {
    #region Variables

    private readonly NetManager net;
    private GameState gameState;

    private readonly List<int> excludedChokepoints
      = new List<int>( new[] { 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 3, 5, 22 } );  // excluded range (camp/offline/fort/flak)


    // subscribed xmlfiles

    private Metadata subscribedMetadata;
    private Livedata subscribedLivedata;

    private List<MetaXmlFile> metafiles;
    private List<InitXmlFile> initfiles;
    private List<PollXmlFile> pollfiles;

    private PollXmlFile pollCaptures, pollAOs, pollFirebases, pollHCMoveReq, pollHCMoves, pollFactoryLog, pollRDP, pollDeathMap, pollServers;

    #endregion

    #region Constructors

    /// <summary>
    /// Create a new Wiretap object.
    /// </summary>
    public Wiretap()
      : this( new NetManager() )
    {

    }

    /// <summary>
    /// Create a new Wiretap object with the given NetManager.
    /// </summary>
    public Wiretap( NetManager net )
    {
      this.net = net;
      this.gameState = null;


      // generate cache path, make sure exists

      Wiretap.CachePath = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ), Application.ProductName );

      if( !Directory.Exists( Wiretap.CachePath ) )
        Directory.CreateDirectory( Wiretap.CachePath );

      
      // initialise subscriptions, xml data files

      this.subscribedMetadata = Metadata.None;
      this.subscribedLivedata = Livedata.None;

      this.metafiles = new List<MetaXmlFile>();
      this.initfiles = new List<InitXmlFile>();
      this.pollfiles = new List<PollXmlFile>();

      this.pollCaptures     =
        this.pollAOs        =
        this.pollFirebases  =
        this.pollHCMoveReq  =
        this.pollHCMoves    =
        this.pollFactoryLog =
        this.pollRDP        =
        this.pollDeathMap   =
        this.pollServers    = new PollXmlFile();  // empty object (ie, unsubscribed)
    }

    #endregion

    #region Properties

    /// <summary>
    /// The current NetManager object being used for web access. 
    /// </summary>
    public NetManager Net
    {
      get { return this.net; }
    }

    /// <summary>
    /// The current GameState object being used to store data, if any.
    /// </summary>
    public GameState GameState
    {
      get { return this.gameState; }
      set { this.gameState = value; }
    }

    /// <summary>
    /// The local filesystem path where xml files are cached.
    /// </summary>
    public static string CachePath
    {
      get; set;
    }

    /// <summary>
    /// True if we have received any new Capture, AO or Firebase events in the last
    /// round of polling.
    /// </summary>
    public bool NewCaptureData
    {
      get { return this.pollCaptures.NewData || this.pollAOs.NewData || this.pollFirebases.NewData; }
    }

    /// <summary>
    /// True if we have received any map-related events in the last round of polling.
    /// </summary>
    public bool NewMapData
    {
      get
      {
        return this.pollCaptures.NewData || this.pollAOs.NewData || this.pollFirebases.NewData
            || this.pollHCMoveReq.NewData || this.pollHCMoves.NewData || this.pollDeathMap.NewData;
      }
    }

    /// <summary>
    /// True if we have received any new HCUnit events in the last round of polling.
    /// </summary>
    public bool NewHCUnitData
    {
      get { return this.pollHCMoveReq.NewData || this.pollHCMoves.NewData; }
    }

    /// <summary>
    /// True if factory data is loaded and we have received any new Factory events in the last round of polling.
    /// </summary>
    public bool NewFactoryData
    {
      get
      {
        if( this.pollFactoryLog != null && this.pollRDP != null )
          return this.pollFactoryLog.NewData || this.pollRDP.NewData;
        else
          return false;  // factory data not loaded
      }
    }

    /// <summary>
    /// The number of new captures received in the last round of polling.
    /// </summary>
    public int NewCapturesCount
    {
      get { return this.pollCaptures.NewDataCount; }
    }

    /// <summary>
    /// The number of ao changes received in the last round of polling.
    /// </summary>
    public int NewAOsCount
    {
      get { return this.pollAOs.NewDataCount; }
    }

    /// <summary>
    /// The number of firebase changes received in the last round of polling.
    /// </summary>
    public int NewFirebaseCount
    {
      get { return this.pollFirebases.NewDataCount; }
    }

    /// <summary>
    /// The number of hcunit moves received in the last round of polling.
    /// </summary>
    public int NewHCMovesCount
    {
      get { return this.pollHCMoveReq.NewDataCount + this.pollHCMoves.NewDataCount; }
    }

    /// <summary>
    /// The number of new factory ticks received in the last round of polling.
    /// </summary>
    public int NewFactoryTickCount
    {
      get
      {
        if( this.pollFactoryLog != null )
          return this.pollFactoryLog.NewDataCount;
        else
          return 0;  // factory data not loaded
      }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Subscribe to one or more wiretap meta data feeds.
    /// </summary>
    /// <param name="flags">One or more Metadata enum values.</param>
    public void Subscribe( Metadata flags )
    {
      MetaXmlFile metafile;

      foreach( Metadata flag in Misc.GetFlags<Metadata>( flags ) )
      {
        if( IsSubscribed( flag ) ) continue;

        switch( flag )
        {
          case Metadata.MapChokePoint:

            /* <cplist>
             *   <cp id="1" name="Chichester" type="1" orig-country="1" orig-side="1" links="0" x="-44" y="3812" absx="-35220" absy="3050188" /> 
             *   ...
             * </cplist>
             */

            metafile = new MetaXmlFile( "/xml/cplist.xml", "/cplist/cp", true );
            metafile.PostParse += MetaXmlFile_ChokePoint_PostParse;
            metafile.AttrNames = new string[] { "id", "name", "type", "orig-country", "orig-side", "x", "y" };
            metafile.AttrDefaults = new object[] { null, "(unknown)", 1, 0, 0, 0, 0 };
            metafile.DataFlag = flag;
            this.metafiles.Add( metafile );

            break;


          case Metadata.MapFacility:

            /* <facilitylist>
             *   <fac id="1" name="Chichester City" cp="1" type="1" orig-country="1" orig-side="1" x="-44" y="3812" absx="-35378" absy="3050225" />
             *    ...
             * </facilitylist>
             */

            metafile = new MetaXmlFile( "/xml/facilitylist.xml", "/facilitylist/fac", true );
            metafile.PostParse += MetaXmlFile_Facility_PostParse;
            metafile.AttrNames = new string[] { "id", "name", "cp", "type", "absx", "absy" };
            metafile.AttrDefaults = new object[] { null, "(unknown)", null, 0, 0, 0 };
            metafile.DataFlag = flag;
            this.metafiles.Add( metafile );

            break;


          case Metadata.MapLinks:

            /* <links>
             *   <link lcp="7" ldep="14" fb="0" rdep="46" rcp="16" />
             *   ...
             */

            metafile = new MetaXmlFile( "/xml/links.xml", "/links/link", true );
            metafile.PostParse += MetaXmlFile_Links_PostParse;
            metafile.AttrNames = new string[] { "lcp", "ldep", "fb", "rdep", "rcp" };
            metafile.AttrDefaults = new object[] { null, null, null, null };
            metafile.DataFlag = flag;
            this.metafiles.Add( metafile );

            break;


          case Metadata.VehicleCategory:

            /* <vehcat>
             *   <r id="1" branch="2" abbr="AC" name="Aircraft" />
             *   ...
             * </vehcat>
             */

            metafile = new MetaXmlFile( "/xml/vehcat.xml", "/vehcat/r", true );
            metafile.PostParse += MetaXmlFile_VehCategory_PostParse;
            metafile.AttrNames = new string[] { "id", "branch", "abbr", "name" };
            metafile.AttrDefaults = new object[] { null, null, "(unknown)", "(unknown)" };
            metafile.DataFlag = flag;
            this.metafiles.Add( metafile );

            break;


          case Metadata.VehicleClass:

            /* <vehclass>
             *   <r id="1" name="bomber" cat="1" />
             *   ...
             * </vehclass>
             */

            metafile = new MetaXmlFile( "/xml/vehclass.xml", "/vehclass/r", true );
            metafile.PostParse += MetaXmlFile_VehClass_PostParse;
            metafile.AttrNames = new string[] { "id", "name", "cat" };
            metafile.AttrDefaults = new object[] { null, "(unknown)", 0 };
            metafile.DataFlag = flag;
            this.metafiles.Add( metafile );

            break;


          case Metadata.VehicleInfo:

            /* <vehicles>
             *   <v id="4" ctry="1" name="A13 Mk II" cat="2" class="4" rank="1" next="1" typeid="1" />
             *   ...
             * </vehicles>
             */

            metafile = new MetaXmlFile( "/xml/vehicles.xml", "/vehicles/v", false );
            metafile.PostParse += MetaXmlFile_Vehicles_PostParse;
            metafile.AttrNames = new string[] { "id", "ctry", "name", "class", "rank", "next", "typeid" };
            metafile.AttrDefaults = new object[] { null, 0, "(unknown)", 0, 1, 1, null };
            metafile.DataFlag = flag;
            this.metafiles.Add( metafile );

            break;


          case Metadata.ToesInfo:

            /* <toes>
             *   <r toe="deairfor" name="Luftwaffe Forces" ctry="4" br="2" cyc="3" cls="2" />
             *   ...
             * </toes>
             */

            metafile = new MetaXmlFile( "/xml/toes.list.xml", "/toes/r", false );
            metafile.PostParse += MetaXmlFile_Toes_PostParse;
            metafile.AttrNames = new string[] { "toe", "name", "ctry", "br", "cyc", "cls" };
            metafile.AttrDefaults = new object[] { null, "(unknown)", 0, 0, 0, 0 };
            metafile.DataFlag = flag;
            this.metafiles.Add( metafile );

            break;


          case Metadata.ToesContent:

            /* <toes>
             *   <r toe="deairfor" veh="23" c0="0" [adj="1:65,3:50,4:30"] cur="50" /> 
             *   ...
             * </toes>
             */

            metafile = new MetaXmlFile( "/xml/toes.sheet.xml", "/toes/r", false );
            metafile.PostParse += MetaXmlFile_Supply_PostParse;
            metafile.AttrNames = new string[] { "toe", "veh", "c0", "adj" };
            metafile.AttrDefaults = new object[] { null, null, "0", "" };
            metafile.DataFlag = flag;
            this.metafiles.Add( metafile );

            break;


          case Metadata.HCUnitList:

            /* <hcunitlist>
             *   <hcunit id="1005" level="6" short="2jg1g" nick="1.Gruppe" title="I.Gruppe 2.Jagdgeschwader" parent="1003" ctry="4" branch="2" moves="2" owner="leopold" toe="deairfor" />
             *   ...
             * </hcunitlist>
             */

            metafile = new MetaXmlFile( "/xml/hcunitlist.xml", "/hcunitlist/hcunit", true );
            metafile.PostParse += MetaXmlFile_HCUnit_PostParse;
            metafile.AttrNames = new string[] { "id", "level", "short", "nick", "title", "parent", "ctry", "branch", "moves", "owner", "toe" };
            metafile.AttrDefaults = new object[] { null, null, "(unknown)", "(unknown)", "(unknown)", null, 0, 0, 0, "", "" };
            metafile.DataFlag = flag;
            this.metafiles.Add( metafile );

            break;


          case Metadata.HCUnitMoveResults:

            /* <moveresults>
             *   <r id="0" description="Approved" />
             *   ...
             * </moveresults>
             */

            metafile = new MetaXmlFile( "/xml/moveresults.xml", "/moveresults/r", true );
            metafile.PostParse += MetaXmlFile_MoveResults_PostParse;
            metafile.AttrNames = new string[] { "id", "description" };
            metafile.AttrDefaults = new object[] { null, "(unknown)" };
            metafile.DataFlag = flag;
            this.metafiles.Add( metafile );

            break;


          case Metadata.Squad:

            /* <squadlist>
             *   <squad id="8883" handle="kaisers" name="" co="ryan5175" created="2003-11-04 21:39:30" pri-country="0" pri-branch="0" pri-brigade="0" />
             *   ...
             * </squadlist>
             */

            metafile = new MetaXmlFile( "/xml/squadlist.xml", "/squadlist/squad", false );
            metafile.PostParse += MetaXmlFile_Squad_PostParse;
            metafile.AttrNames = new string[] { "id", "handle", "name", "co" };
            metafile.AttrDefaults = new object[] { null, "(unknown)", "(unknown)", "" };
            metafile.DataFlag = flag;
            this.metafiles.Add( metafile );

            break;


          case Metadata.Config:

            /* <config>
             *   <r id="arena.campaign_id" val="45" desc="Current Campaign Number" />
             *   ...
             * </config>
             */

            metafile = new MetaXmlFile( "/xml/config.xml", "/config/r", false );
            metafile.PostParse += MetaXmlFile_Config_PostParse;
            metafile.AttrNames = new string[] { "id", "val", "desc" };
            metafile.AttrDefaults = new object[] { null, "(unknown)", "(unknown)" };
            metafile.DataFlag = flag;
            this.metafiles.Add( metafile );

            break;


          default:
            continue;

        }  // end switch flag

        this.subscribedMetadata |= flag;

      }  // end foreach flag
    }

    /// <summary>
    /// Subscribe to one or more wiretap live data feeds.
    /// </summary>
    /// <param name="flags">One or more Livedata enum values.</param>
    public void Subscribe( Livedata flags )
    {
      /* In order for cp.AddCaptureFrom() to work properly during InitParser_Captures,
       * InitParser_CPOwner & InitParser_HCStatus must have already set the inital AO's and 
       * hcunit deployment locations.
       */

      InitXmlFile initfile;

      foreach( Livedata flag in Misc.GetFlags<Livedata>( flags ) )
      {
        if( IsSubscribed( flag ) ) continue;

        switch( flag )
        {
          case Livedata.ChokePoint:

            /* <cps>
             *   <cp id="77" own="4/2" [ao="1048"] [ctrl="1/1"] [contention="true"] /> 
             *   ...
             * </cps>
             */

            initfile = new InitXmlFile( "/xmlquery/cps.xml", "/cps/cp" );
            initfile.PostParse += InitXmlFile_CPOwner_PostParse;
            initfile.AttrNames = new string[] { "id", "own", "ao", "ctrl" };
            initfile.AttrDefaults = new object[] { null, null, 0, 0 };
            initfile.DataFlag = flag;
            this.initfiles.Add( initfile );

            break;


          case Livedata.Capture:

            /* <captures>
             *   <fromid id="2184" />
             *   ...
             *   <cap id="2184" at="1183262529" fac="3913" from="4" to="3" by="cabby" brig="935" [con="Enter|End"] [ctrl="y"] [own="y"] />
             * </captures>
             */

            initfile = new InitXmlFile( "/xmlquery/captures.xml?hours=2", "/captures/cap" );
            initfile.PostParse += InitXmlFile_Captures_PostParse;
            initfile.AttrNames = new string[] { "id", "at", "fac", "from", "to", "by", "brig", "con", "ctrl", "own" };
            initfile.AttrDefaults = new object[] { null, null, null, null, null, "(unknown)", 0, "", false, false };
            initfile.DataFlag = flag;
            this.initfiles.Add( initfile );

            this.pollCaptures = new PollXmlFile( "/xmlquery/captures.xml?fromid=", "/captures/cap", 1 );
            this.pollCaptures.PostParse += PollXmlFile_Captures_PostParse;
            this.pollCaptures.AttrNames = new string[] { "id", "at", "fac", "from", "to", "by", "brig", "con", "ctrl", "own" };
            this.pollCaptures.AttrDefaults = new object[] { null, null, null, null, null, "(unknown)", 0, "", false, false };
            this.pollCaptures.DataFlag = flag;
            this.pollfiles.Add( this.pollCaptures );

            /* <cps>
             *   <cp id="77" own="4/2" ao="1048" [ctrl="1/1"] [contention="true"] />
             *   ...
             * </cps>
             */

            this.pollAOs = new PollXmlFile( "/xmlquery/cps.xml?aos=true", "/cps/cp", 1 );
            this.pollAOs.PostParse += PollXmlFile_AOs_PostParse;
            this.pollAOs.AttrNames = new string[] { "id", "ao" };
            this.pollAOs.AttrDefaults = new object[] { null, 0 };
            this.pollAOs.DataFlag = flag;
            this.pollfiles.Add( this.pollAOs );

            break;


          case Livedata.HCUnit:

            /* <hc-locations>
             *   <unit id="934" cp="0" moved="0" by="SYSTEM" nxtmv="0" /> 
             *   <unit id="935" cp="290" moved="1217348273" by="ltibbs" nxtmv="1217351873" /> 
             *   ...
             * </hc-locations>
             */

            initfile = new InitXmlFile( "/xmlquery/hclocationsext.xml", "/hc-locations/unit" );
            initfile.PostParse += InitXmlFile_HCLocations_PostParse;
            initfile.AttrNames = new string[] { "id", "cp", "moved", "by", "nxtmv" };
            initfile.AttrDefaults = new object[] { null, 0, DateTime.MinValue, "(unknown)", DateTime.MinValue };
            initfile.DataFlag = flag;
            this.initfiles.Add( initfile );

            break;


          case Livedata.Movement:

            /* <move-requests>
             *   <r at="1212970608" by="keitel66" ex="0" id="914" from="579" to="502" res="0" />
             *   <r at="1212970751" by="keitel66" ex="1" id="914" from="579" to="502" res="0" />
             *   ...
             * </move-requests>
             */

            initfile = new InitXmlFile( "/xmlquery/hcmovereq.xml?from=", "/move-requests/r" );  // from = 12 hours ago
            initfile.PreParse += InitXmlFile_HCMoveReq_PreParse;
            initfile.PostParse += InitXmlFile_HCMoveReq_PostParse;
            initfile.AttrNames = new string[] { "at", "by", "ex", "id", "from", "to", "res" };
            initfile.AttrDefaults = new object[] { null, "(unknown)", null, null, null, null, null };
            initfile.DataFlag = flag;
            this.initfiles.Add( initfile );

            this.pollHCMoveReq = new PollXmlFile( "/xmlquery/hcmovereq.xml?from=", "/move-requests/r", 1 );
            this.pollHCMoveReq.PostParse += PollXmlFile_HCMoveReq_PostParse;
            this.pollHCMoveReq.AttrNames = new string[] { "at", "by", "ex", "id", "from", "to", "res" };
            this.pollHCMoveReq.AttrDefaults = new object[] { null, "(unknown)", null, null, null, null, null };
            this.pollHCMoveReq.DataFlag = flag;
            this.pollfiles.Add( this.pollHCMoveReq );

            /* <hcmoves>
             *   <move id="1021" at="1217437384" from="351" to="350" by="SYSTEM" age="22688" delay="900" /> 
             *   ...
             * </hcmoves>
             */

            initfile = new InitXmlFile( "/xmlquery/hcmoves.xml?by=SYSTEM&from=", "/hcmoves/move" );  // from = 12 hours ago
            initfile.PreParse += InitXmlFile_HCMoves_PreParse;
            initfile.PostParse += InitXmlFile_HCMoves_PostParse;
            initfile.AttrNames = new string[] { "id", "at", "from", "to", "by" };
            initfile.AttrDefaults = new object[] { null, null, null, null, "(unknown)" };
            initfile.DataFlag = flag;
            this.initfiles.Add( initfile );

            this.pollHCMoves = new PollXmlFile( "/xmlquery/hcmoves.xml?by=SYSTEM&from=", "/hcmoves/move", 1 );
            this.pollHCMoves.PostParse += PollXmlFile_HCMoves_PostParse;
            this.pollHCMoves.AttrNames = new string[] { "id", "at", "from", "to", "by" };
            this.pollHCMoves.AttrDefaults = new object[] { null, null, null, null, "(unknown)" };
            this.pollHCMoves.DataFlag = flag;
            this.pollfiles.Add( this.pollHCMoves );

            break;


          case Livedata.Firebase:

            /* <openfbs>
             *   <fb id="1314" country="4" /> 
             *   ...
             * </openfbs>
             */

            initfile = new InitXmlFile( "/xml/openfbs.xml", "/openfbs/fb" );
            initfile.PostParse += InitXmlFile_Firebases_PostParse;
            initfile.AttrNames = new string[] { "id", "country" };
            initfile.AttrDefaults = new object[] { null, null };
            initfile.DataFlag = flag;
            this.initfiles.Add( initfile );

            this.pollFirebases = new PollXmlFile( "/xml/openfbs.xml", "/openfbs/fb", 1 );
            this.pollFirebases.PostParse += PollXmlFile_Firebases_PostParse;
            this.pollFirebases.AttrNames = new string[] { "id", "country" };
            this.pollFirebases.AttrDefaults = new object[] { null, null };
            this.pollFirebases.DataFlag = flag;
            this.pollfiles.Add( this.pollFirebases );

            break;


          case Livedata.Deathmap:

            /* <deathmap cell-width="500">
             *   <r x="248000" y="2962500" t="1202939400" ax="8" ad="13" /> 
             *   ...
             * </deathmap>
             */

            initfile = new InitXmlFile( "/xml/deathmap.1h.xml", "/deathmap/r" );  // group 10min x 6, update 5min
            initfile.PostParse += InitXmlFile_DeathMap_PostParse;
            initfile.AttrNames = new string[] { "x", "y", "t", "ax", "ad" };
            initfile.AttrDefaults = new object[] { null, null, null, 0, 0 };
            initfile.DataFlag = flag;
            this.initfiles.Add( initfile );

            initfile = new InitXmlFile( "/xml/deathmap.5m.xml", "/deathmap/r" );  // group  1min x 5, update 1min
            initfile.PostParse += InitXmlFile_DeathMap_PostParse;
            initfile.AttrNames = new string[] { "x", "y", "t", "ax", "ad" };
            initfile.AttrDefaults = new object[] { null, null, null, 0, 0 };
            initfile.DataFlag = flag;
            this.initfiles.Add( initfile );

            this.pollDeathMap = new PollXmlFile( "/xml/deathmap.1m.xml", "/deathmap/r", 1 );
            this.pollDeathMap.PostParse += PollXmlFile_DeathMap_PostParse;
            this.pollDeathMap.AttrNames = new string[] { "x", "y", "t", "ax", "ad" };
            this.pollDeathMap.AttrDefaults = new object[] { null, null, null, 0, 0 };
            this.pollDeathMap.DataFlag = flag;
            this.pollfiles.Add( this.pollDeathMap );

            break;


          case Livedata.Servers:

            /* <servers>
             *   <r arena="1" name="Blitzkrieg" state="Online" pop="Average" /> 
             *   ...
             * </servers>
             */

            initfile = new InitXmlFile( "/xml/servers.xml", "/servers/r" );
            initfile.PostParse += InitXmlFile_Servers_PostParse;
            initfile.AttrNames = new string[] { "arena", "name", "state", "pop" };
            initfile.AttrDefaults = new object[] { null, "(unknown)", ServerState.Unknown, ServerPopulation.Unknown };
            initfile.DataFlag = flag;
            this.initfiles.Add( initfile );

            this.pollServers = new PollXmlFile( "/xml/servers.xml", "/servers/r", 5 );
            this.pollServers.PostParse += PollXmlFile_Servers_PostParse;
            this.pollServers.AttrNames = new string[] { "arena", "state", "pop" };
            this.pollServers.AttrDefaults = new object[] { null, ServerState.Unknown, ServerPopulation.Unknown };
            this.pollServers.FlagAsChecked( 0 );  // don't poll straight away
            this.pollServers.DataFlag = flag;
            this.pollfiles.Add( this.pollServers );

            break;


          case Livedata.Factory:

            /* <factorystats>
                   *   <factory id="70" from="1183051374" to="1183262874" ticks="236" produced="236" />
                   *   ...
                   * </factorystats>
                   */

            initfile = new InitXmlFile( "/xml/factorystats.xml", "/factorystats/factory" );
            initfile.PostParse += InitXmlFile_FactoryStats_PostParse;
            initfile.AttrNames = new string[] { "id", "from", "to", "ticks", "produced" };
            initfile.AttrDefaults = new object[] { null, null, null, 0, 0 };
            initfile.DataFlag = flag;
            this.initfiles.Add( initfile );

            /* <rows>
             *   <row fid="3810" at="1172121700" damage="23" rdp="0" />
             *   ...
             * </rows>
             */

            initfile = new InitXmlFile( "/xmlquery/factorylog.xml?limit=2592", "/rows/row" );
            // 2592 = num factories * ticks p/ hour * hours p/ day = 1 days worth of data
            initfile.PostParse += InitXmlFile_FactoryLog_PostParse;
            initfile.AttrNames = new string[] { "fid", "at", "damage", "rdp" };
            initfile.AttrDefaults = new object[] { null, null, 0, 0 };
            initfile.DataFlag = flag;
            this.initfiles.Add( initfile );

            this.pollFactoryLog = new PollXmlFile( "/xmlquery/factorylog.xml?from=", "/rows/row", 15 );
            this.pollFactoryLog.PostParse += PollXmlFile_FactoryLog_PostParse;
            this.pollFactoryLog.AttrNames = new string[] { "fid", "at", "damage", "rdp" };
            this.pollFactoryLog.AttrDefaults = new object[] { null, null, 0, 0 };
            this.pollFactoryLog.DataFlag = flag;
            this.pollfiles.Add( this.pollFactoryLog );

            /* <rdp>
             *   <rdp id="1" cycle="2" cur_cycle_goal="10000" cur_cycle_produced="263" /> 
             * </rdp>
             */

            initfile = new InitXmlFile( "/xml/rdp.xml", "/rdp/rdp" );
            initfile.PostParse += PollXmlFile_RDP_PostParse;  // same as poll
            initfile.AttrNames = new string[] { "id", "cycle", "cur_cycle_goal", "cur_cycle_produced" };
            initfile.AttrDefaults = new object[] { null, 0, 0, 0 };
            initfile.DataFlag = flag;
            this.initfiles.Add( initfile );

            this.pollRDP = new PollXmlFile( "/xml/rdp.xml", "/rdp/rdp", 60 );
            this.pollRDP.PostParse += PollXmlFile_RDP_PostParse;
            this.pollRDP.AttrNames = new string[] { "id", "cycle", "cur_cycle_goal", "cur_cycle_produced" };
            this.pollRDP.AttrDefaults = new object[] { null, 0, 0, 0 };
            this.pollRDP.FlagAsChecked( 0 );  // don't poll straight away
            this.pollRDP.DataFlag = flag;
            this.pollfiles.Add( this.pollRDP );

            break;


          default:
            continue;

        }  // end switch flag

        this.subscribedLivedata |= flag;

      }  // end foreach flag
    }

    /// <summary>
    /// Check if currently subscribed to the given data.
    /// </summary>
    /// <param name="flag">The data to check.</param>
    /// <returns>True if currently subscribed.</returns>
    public bool IsSubscribed( Metadata flag )
    {
      return ( this.subscribedMetadata & flag ) > 0;
    }

    /// <summary>
    /// Check if currently subscribed to the given data.
    /// </summary>
    /// <param name="flag">The data to check.</param>
    /// <returns>True if currently subscribed.</returns>
    public bool IsSubscribed( Livedata flag )
    {
      return ( this.subscribedLivedata & flag ) > 0;
    }

    /// <summary>
    /// Checks all metadata on the wiretap server for updated versions.
    /// </summary>
    /// <returns>True if any files need re-downloading via GetMetaData().</returns>
    public bool CheckUpdatedMetadata()
    {
      return CheckUpdatedMetadata( Metadata.All );
    }

    /// <summary>
    /// Checks the specified metadata on the wiretap server for updated versions.
    /// </summary>
    /// <returns>True if any files need re-downloading via GetMetaData().</returns>
    public bool CheckUpdatedMetadata( Metadata flags )
    {
      bool logged = false;  // only log once

      foreach( MetaXmlFile metafile in metafiles )
      {
        if( ( flags & metafile.DataFlag ) <= 0 ) continue;

        if( !metafile.LocalFileInfo.Exists )
          return true;  // local file must exist

        if( !metafile.ReloadOnChange )
          continue;  // don't bother checking if we're not going to reload

        if( !metafile.NeedsChecking )  // once an hour
          continue;

        if( !logged )
        {
          Log.AddEntry( "Checking for updated metadata..." );
          logged = true;
        }

        try
        {
          DateTime remoteTime = net.GetLastModified( metafile );
          if( remoteTime > metafile.LocalFileInfo.LastWriteTime )
          {
            Log.AddEntry( "Newer version of {0} found", metafile );
            return true;
          }
          else
          {
            metafile.Touch();
            metafile.FlagAsChecked();
          }
        }
        catch( NetworkException ) { }  // ignore network errors
      }

      if( logged )
        Log.Okay();  // no updates found

      return false;
    }

    /// <summary>
    /// Make sure all cached metadata is up to date and load into memory.
    /// </summary>
    /// <param name="firstErrorAction">A delegate to run for the first error encountered.</param>
    public void GetMetaData( Action<string> firstErrorAction = null )
    {
      GetMetaData( Metadata.All, firstErrorAction );
    }

    /// <summary>
    /// Make sure the specified cached metadata is up to date and load into memory.
    /// </summary>
    /// <param name="flags">The flag(s) to process (default: all subscribed).</param>
    /// <param name="firstErrorAction">A delegate to run for the first error encountered.</param>
    public void GetMetaData( Metadata flags, Action<string> firstErrorAction = null )
    {
      // make sure up to date, download if necessary

      Log.AddEntry( "Checking for updated metadata:" );

      foreach( MetaXmlFile metafile in metafiles )
      {
        if( ( flags & metafile.DataFlag ) <= 0 ) continue;

        if( !metafile.LocalFileInfo.Exists )
        {
          Log.AddEntry( "  {0}: no local cache, downloading", metafile );
          net.DownloadMetaFile( metafile );
        }
        else if( !metafile.IsValid )
        {
          Log.AddEntry( "  {0}: local cache corrupt, downloading", metafile );
          net.DownloadMetaFile( metafile );
        }
        else if( metafile.NeedsChecking )
        {
          metafile.FlagAsChecked();

          DateTime remoteTime = net.GetLastModified( metafile );
          if( remoteTime > metafile.LocalFileInfo.LastWriteTime )
          {
            Log.AddEntry( "  {0}: newer version available, updating", metafile );
            net.DownloadMetaFile( metafile );
          }
          else
          {
            Log.AddEntry( "  {0}: up to date", metafile );
            metafile.Touch();
          }
        }
        else
        {
          Log.AddEntry( "  {0}: recently checked, skipping", metafile );
        }
      }  // end foreach metafile


      // skip parseing if not storing state in memory

      if( this.gameState == null ) return;


      // parse locally cached files

      Log.AddEntry( "Parsing metafiles:" );

      foreach( MetaXmlFile metafile in metafiles )
      {
        if( ( flags & metafile.DataFlag ) <= 0 ) continue;

        Log.AddEntry( "  {0}...", metafile );
        try
        {
          metafile.Parse();
          Log.Okay();
        }
        catch( Exception ex )
        {
          Log.Error();
          Log.AddException( ex );

          if( firstErrorAction != null )
          {
            firstErrorAction( String.Format( Language.Error_AnErrorOccuredParsing, metafile.Uri ) + ":\n\n" + ex.Message );
            firstErrorAction = null;
          }
        }
      }  // end foreach metafile

    }

    /// <summary>
    /// Establish initial game state by downloading and parsing all subscribed init files.
    /// </summary>
    /// <param name="firstErrorAction">A delegate to run for the first error encountered.</param>
    public void GetInitState( Action<string> firstErrorAction = null )
    {
      GetInitState( Livedata.All, firstErrorAction );
    }

    /// <summary>
    /// Establish initial game state by downloading and parsing the specified subscribed init files.
    /// </summary>
    /// <param name="flags">The flag(s) to process (default: all subscribed).</param>
    /// <param name="firstErrorAction">A delegate to run for the first error encountered.</param>
    public void GetInitState( Livedata flags, Action<string> firstErrorAction = null )
    {
      Log.AddEntry( "Downloading and parsing init files:" );

      foreach( InitXmlFile initfile in initfiles )
      {
        if( (flags & initfile.DataFlag) <= 0 ) continue;
        Log.AddEntry( "  {0}...", initfile );

        try
        {
          initfile.Parse( net );
          Log.Okay();
        }
        catch( Exception ex )
        {
          Log.Error();

          if( ex is NetworkException )
            throw;

          Log.AddException( ex );

          if( firstErrorAction != null )
          {
            firstErrorAction( String.Format( Language.Error_AnErrorOccuredParsing, initfile.Uri ) + ":\n\n" + ex.Message );
            firstErrorAction = null;
          }
        }
      }
    }

    /// <summary>
    /// Poll any xml files that need checking, and parse them for new events.
    /// </summary>
    public void GetPollData()
    {
      GetPollData( Livedata.All );
    }

    /// <summary>
    /// Poll any of the specified xml files that need checking, and parse them for new events.
    /// </summary>
    public void GetPollData( Livedata flags )
    {
      foreach( PollXmlFile pollfile in pollfiles )
      {
        if( ( flags & pollfile.DataFlag ) <= 0 ) continue;

        try
        {
          pollfile.NewDataCount = 0;  // reset

          if( pollfile.NeedsChecking )
            pollfile.Parse( net );  // parser is responsible for calling FlagAsChecked()
        }
        catch( Exception ex )
        {
          Log.AddException( ex );
        }
      }
    }

    /// <summary>
    /// Deletes the cached copies of all the xml metadata.
    /// </summary>
    public void FlushCache()
    {
      Log.AddEntry( "Flushing metadata cache" );

      foreach( MetaXmlFile metafile in metafiles )
        if( metafile.LocalFileInfo.Exists )
          metafile.LocalFileInfo.Delete();
    }

    /// <summary>
    /// Reset the NextUpdate times on all the pollxmlfiles to that next poll they all get checked.
    /// </summary>
    public void ResetPollTimes()
    {
      foreach( PollXmlFile pollfile in pollfiles )
      {
        pollfile.NextUpdate = DateTime.Now;
      }
    }

    #region MetaXmlFile Parsers

    /// <summary>
    /// Parses the chokepoint metadata and create new ChokePoint and Bridge objects.
    /// </summary>
    private void MetaXmlFile_ChokePoint_PostParse( object sender, PostParseEventArgs e )
    {
      /* <cplist>
       *   <cp id="1" name="Chichester" type="1" orig-country="1" orig-side="1" links="0" x="-44" y="3812" absx="-35220" absy="3050188" /> 
       *   ...
       * </cplist>
       */


      // initialise game object collections

      int max = e.Data.GetMaxValue( "id" );
      this.gameState.ChokePoints = new ChokePoint[max + 1];
      this.gameState.Bridges = new Bridge[max + 1];


      // loop over xml data

      MultiExceptionHandler mex = new MultiExceptionHandler();
      for( e.Data.Offset = 0; e.Data.Offset < e.Data.Count; e.Data.Offset++ )
      {
        try
        {
          // parse and validate attribute values

          int id = e.Data.GetValue<int>( "id" );
          if( this.excludedChokepoints.Contains( id ) ) continue;
          
          string name = e.Data.GetValue<string>( "name" );

          int typeid = e.Data.GetValue<int>( "type" );
          e.CheckAttribute( "type", typeid, typeof( ChokePointType ) );
          ChokePointType type = (ChokePointType)typeid;

          if( type == ChokePointType.Bridge )
            e.CheckAttributeDupe( "id", id, this.gameState.Bridges );
          else
            e.CheckAttributeDupe( "id", id, this.gameState.ChokePoints );

          int origCountry = e.Data.GetValue<int>( "orig-country" );
          e.CheckAttribute( "orig-country", origCountry, this.gameState.Countries );

          int x = e.Data.GetValue<int>( "x" );
          int y = e.Data.GetValue<int>( "y" );
          Point location = new Point( x, y );


          // create game objects

          if( type == ChokePointType.Bridge )
            this.gameState.Bridges[id] = new Bridge( id, name, location );
          else
            this.gameState.ChokePoints[id] = new ChokePoint( id, name, type, this.gameState.Countries[origCountry], location, this.gameState.Events );

        }
        catch( Exception ex ) { mex.Add( ex ); }

      }  // for e.Data


      // add additional chokepoint data

      try
      {
        this.gameState.GenerateCPData();
      }
      catch( Exception ex ) { mex.Add( ex ); }


      // if any errors, throw first

      mex.Throw();
    }

    /// <summary>
    /// Parses the facility list metadata and create new Facilitys, MilitaryFacilitys, Factorys, Depots &amp; Firebases.
    /// </summary>
    private void MetaXmlFile_Facility_PostParse( object sender, PostParseEventArgs e )
    {
      /* <facilitylist>
       *   <fac id="1" name="Chichester City" cp="1" type="1" orig-country="1" orig-side="1" x="-44" y="3812" absx="-35378" absy="3050225" />
       *    ...
       * </facilitylist>
       */


      // initialise game object collections

      int max = e.Data.GetMaxValue( "id" );
      this.gameState.Facilities = new Facility[max + 1];
      this.gameState.Factories.Clear();
      this.gameState.Firebases.Clear();


      // loop over xml data

      MultiExceptionHandler mex = new MultiExceptionHandler();
      for( e.Data.Offset = 0; e.Data.Offset < e.Data.Count; e.Data.Offset++ )
      {
        try
        {
          // parse and validate attribute values

          int cpid = e.Data.GetValue<int>( "cp" );
          if( this.excludedChokepoints.Contains( cpid ) ) continue;
          e.CheckAttribute( "cp", cpid, this.gameState.ChokePoints );

          int id = e.Data.GetValue<int>( "id" );

          string name = e.Data.GetValue<string>( "name" );
          string rawName = name;  // keep a copy of the original name
          name = name.Replace( "St. ", "St." );
          name = name.Replace( " - ", "-" );
          name = name.Replace( "- ", "-" );
          name = name.Replace( "  ", " " );

          int typeid = e.Data.GetValue<int>( "type" );
          e.CheckAttribute( "type", typeid, typeof( FacilityType ) );
          FacilityType type = (FacilityType)typeid;

          int x = e.Data.GetValue<int>( "absx" );
          int y = e.Data.GetValue<int>( "absy" );
          Point location = new Point( x, y );


          // create game objects

          if( type == FacilityType.Factory && name.Contains( "Production Facility" ) )  // don't include plants/refinery/etc
            this.gameState.Facilities[id] = this.gameState.Factories[id] = new Factory( id, name, this.gameState.ChokePoints[cpid], FacilityType.Factory, location, rawName );
          else if( type == FacilityType.Firebase )
            this.gameState.Firebases[id] = new Firebase( id, name, this.gameState.ChokePoints[cpid], location, rawName );
          else if( type == FacilityType.Depot )
            this.gameState.Facilities[id] = new Depot( id, name, this.gameState.ChokePoints[cpid], FacilityType.Depot, location, rawName );
          else if( type == FacilityType.Airbase || type == FacilityType.Armybase || type == FacilityType.Navalbase )
            this.gameState.Facilities[id] = new MilitaryFacility( id, name, this.gameState.ChokePoints[cpid], type, location, rawName );
          else
            this.gameState.Facilities[id] = new Facility( id, name, this.gameState.ChokePoints[cpid], type, location, rawName );

        }
        catch( Exception ex ) { mex.Add( ex ); }

      }  // for e.Data


      // if any errors, throw first

      mex.Throw();
    }

    /// <summary>
    /// Parses the links metadata and add the Depot - Firebase - Depot links.
    /// </summary>
    private void MetaXmlFile_Links_PostParse( object sender, PostParseEventArgs e )
    {
      /* <links>
       *   <link lcp="7" ldep="14" fb="0" rdep="46" rcp="16" />
       *   ...
       */

      // initialise game object collections

      this.gameState.Links.Clear();


      // loop over xml data

      List<KeyValuePair<int,int>> depotPairs = new List<KeyValuePair<int, int>>();

      MultiExceptionHandler mex = new MultiExceptionHandler();
      for( e.Data.Offset = 0; e.Data.Offset < e.Data.Count; e.Data.Offset++ )
      {
        try
        {
          // parse and validate attribute values

          int lcp = e.Data.GetValue<int>( "lcp" );
          int rcp = e.Data.GetValue<int>( "rcp" );
          if( this.excludedChokepoints.Contains( lcp ) || this.excludedChokepoints.Contains( rcp ) ) continue;

          int ldep = e.Data.GetValue<int>( "ldep" );
          e.CheckAttribute( "ldep", ldep, this.gameState.Facilities );

          int fbid = e.Data.GetValue<int>( "fb" );
          if( fbid > 0 )
            e.CheckAttribute( "fb", fbid, this.gameState.Firebases );
          Firebase fb = fbid == 0 ? null : this.gameState.Firebases[fbid];

          int rdep = e.Data.GetValue<int>( "rdep" );
          e.CheckAttribute( "rdep", rdep, this.gameState.Facilities );
          

          // add link to game objects

          ( (Depot)this.gameState.Facilities[ldep] ).AddLink( (Depot)this.gameState.Facilities[rdep], fb );
          
          if( ldep < rdep )
            depotPairs.Add( new KeyValuePair<int, int>( ldep, rdep ) );

        }
        catch( Exception ex ) { mex.Add( ex ); }

      }  // for e.Data


      try
      {
        // create game objects

        foreach( var depotPair in depotPairs )
        {
          SupplyLink link = new SupplyLink( (Depot)this.gameState.Facilities[depotPair.Key], (Depot)this.gameState.Facilities[depotPair.Value] );
          this.gameState.Links.Add( link.ChokePointA.ID, link.ChokePointB.ID, link );
        }
      }
      catch( Exception ex ) { mex.Add( ex ); }


      // if any errors, throw first

      mex.Throw();
    }

    /// <summary>
    /// Parses the vehcat metadata and create an array of VehicleCategory objects.
    /// </summary>
    private void MetaXmlFile_VehCategory_PostParse( object sender, PostParseEventArgs e )
    {
      /* <vehcat>
       *   <r id="1" branch="2" abbr="AC" name="Aircraft" />
       *   ...
       * </vehcat>
       */


      // initialise game object collections

      int max = e.Data.GetMaxValue( "id" );
      this.gameState.VehicleCategories = new VehicleCategory[max + 1];


      // loop over xml data

      MultiExceptionHandler mex = new MultiExceptionHandler();
      for( e.Data.Offset = 0; e.Data.Offset < e.Data.Count; e.Data.Offset++ )
      {
        try
        {
          // parse and validate attribute values

          int id = e.Data.GetValue<int>( "id" );
          e.CheckAttributeDupe( "id", id, this.gameState.VehicleCategories );

          int branch = e.Data.GetValue<int>( "branch" );
          e.CheckAttribute( "branch", branch, typeof( Branch ) );

          string abbr = e.Data.GetValue<string>( "abbr" );
          string name = e.Data.GetValue<string>( "name" );


          // create game objects

          this.gameState.VehicleCategories[id] = new VehicleCategory( id, (Branch)branch, abbr, name );
        }
        catch( Exception ex ) { mex.Add( ex ); }

      }  // for e.Data


      // if any errors, throw first

      mex.Throw();
    }

    /// <summary>
    /// Parses the vehcat metadata and create an array of VehicleClass objects.
    /// </summary>
    private void MetaXmlFile_VehClass_PostParse( object sender, PostParseEventArgs e )
    {
      /* <vehclass>
       *   <r id="1" name="bomber" cat="1" />
       *   ...
       * </vehclass>
       */


      // initialise game object collections

      int max = e.Data.GetMaxValue( "id" );
      this.gameState.VehicleClasses = new VehicleClass[max + 1];


      // loop over xml data

      MultiExceptionHandler mex = new MultiExceptionHandler();
      for( e.Data.Offset = 0; e.Data.Offset < e.Data.Count; e.Data.Offset++ )
      {
        try
        {
          // parse and validate attribute values

          int id = e.Data.GetValue<int>( "id" );
          e.CheckAttributeDupe( "id", id, this.gameState.VehicleClasses );

          string name = e.Data.GetValue<string>( "name" );

          int cat = e.Data.GetValue<int>( "cat" );
          e.CheckAttribute( "cat", cat, this.gameState.VehicleCategories );


          // create game objects

          this.gameState.VehicleClasses[id] = new VehicleClass( id, name, this.gameState.VehicleCategories[cat] );
        }
        catch( Exception ex ) { mex.Add( ex ); }

      }  // for e.Data


      // if any errors, throw first

      mex.Throw();
    }

    /// <summary>
    /// Parses the vehicle metadata and create an array of Vehicle objects.
    /// </summary>
    private void MetaXmlFile_Vehicles_PostParse( object sender, PostParseEventArgs e )
    {
      /* <vehicles>
       *   <v id="4" ctry="1" name="A13 Mk II" cat="2" class="4" rank="1" next="1" typeid="1" />
       *   ...
       * </vehclass>
       */


      // initialise game object collections

      int max = e.Data.GetMaxValue( "id" );
      this.gameState.Vehicles = new Vehicle[max + 1];


      // loop over xml data

      MultiExceptionHandler mex = new MultiExceptionHandler();
      for( e.Data.Offset = 0; e.Data.Offset < e.Data.Count; e.Data.Offset++ )
      {
        try
        {
          // parse and validate attribute values

          int id = e.Data.GetValue<int>( "id" );
          e.CheckAttributeDupe( "id", id, this.gameState.Vehicles );

          int countryid = e.Data.GetValue<int>( "ctry" );
          e.CheckAttribute( "ctry", countryid, this.gameState.Countries );

          string name = e.Data.GetValue<string>( "name" );

          int classid = e.Data.GetValue<int>( "class" );
          e.CheckAttribute( "class", classid, this.gameState.VehicleClasses );

          int curRankid = e.Data.GetValue<int>( "rank" );
          e.CheckAttribute( "rank", curRankid, this.gameState.Ranks );

          int nextRankid = e.Data.GetValue<int>( "next" );
          e.CheckAttribute( "next", nextRankid, this.gameState.Ranks );

          int typeid = e.Data.GetValue<int>( "typeid" );


          // create game objects

          this.gameState.Vehicles[id] = new Vehicle( id, this.gameState.Countries[countryid], name, this.gameState.VehicleClasses[classid], this.gameState.Ranks[curRankid], this.gameState.Ranks[nextRankid], typeid );

        }
        catch( Exception ex ) { mex.Add( ex ); }

      }  // for e.Data


      // if any errors, throw first

      mex.Throw();
    }

    /// <summary>
    /// Parses the toe list metadata and create a list of Toe objects.
    /// </summary>
    private void MetaXmlFile_Toes_PostParse( object sender, PostParseEventArgs e )
    {
      /* <toes>
       *   <r toe="deairfor" name="Luftwaffe Forces" ctry="4" br="2" cyc="3" cls="2" />
       *   ...
       * </toes>
       */


      // initialise game object collections

      this.gameState.Toes.Clear();


      // loop over xml data

      MultiExceptionHandler mex = new MultiExceptionHandler();
      for( e.Data.Offset = 0; e.Data.Offset < e.Data.Count; e.Data.Offset++ )
      {
        try
        {
          // parse and validate attribute values

          string code = e.Data.GetValue<string>( "toe" );
          e.CheckAttributeDupe( "toe", code, this.gameState.Toes );

          string name = e.Data.GetValue<string>( "name" );

          int countryid = e.Data.GetValue<int>( "ctry" );
          e.CheckAttribute( "ctry", countryid, this.gameState.Countries );

          int branch = e.Data.GetValue<int>( "br" );
          e.CheckAttribute( "br", branch, typeof( Branch ) );

          int currentCycle = e.Data.GetValue<int>( "cyc" );

          int classid = e.Data.GetValue<int>( "cls" );
          if( classid != 0 )  //// skip checking missing classes (not used)
            e.CheckAttribute( "cls", classid, this.gameState.VehicleClasses );


          // Toe.AddVehicle() in MetaParser_Supply needs the country's current cycle so
          // we set it here temporarily (overwritten later by PollParser_RDP).

          this.gameState.Countries[countryid].SetRDPState( currentCycle );


          // create game objects

          this.gameState.Toes.Add( code, new Toe( code, name, this.gameState.Countries[countryid], (Branch)branch, this.gameState.VehicleClasses[classid] ) );

        }
        catch( Exception ex ) { mex.Add( ex ); }

      }  // for e.Data


      // if any errors, throw first

      mex.Throw();
    }

    /// <summary>
    /// Parses the toe sheet metadata and add SupplyLevel objects to each Toe.
    /// </summary>
    private void MetaXmlFile_Supply_PostParse( object sender, PostParseEventArgs e )
    {
      /* <toes>
       *   <r toe="deairfor" veh="23" c0="0" [adj="1:65,3:50,4:30"] cur="50" /> 
       *   ...
       * </toes>
       */

      // loop over xml data

      MultiExceptionHandler mex = new MultiExceptionHandler();
      for( e.Data.Offset = 0; e.Data.Offset < e.Data.Count; e.Data.Offset++ )
      {
        try
        {
          // parse and validate attribute values

          string code = e.Data.GetValue<string>( "toe" );
          e.CheckAttribute( "toe", code, this.gameState.Toes );

          int vehicleid = e.Data.GetValue<int>( "veh" );
          e.CheckAttribute( "veh", vehicleid, this.gameState.Vehicles );

          int start = e.Data.GetValue<int>( "c0" );
          string adjustments = e.Data.GetValue<string>( "adj" );


          // update game objects

          this.gameState.Toes[code].AddVehicle( this.gameState.Vehicles[vehicleid], start, adjustments );
        }
        catch( Exception ex ) { mex.Add( ex ); }

      }  // for e.Data


      // if any errors, throw first

      mex.Throw();
    }

    /// <summary>
    /// Parses the hcunits metadata and create new HCUnits, then link them together into a tree.
    /// </summary>
    private void MetaXmlFile_HCUnit_PostParse( object sender, PostParseEventArgs e )
    {
      /* <hcunitlist>
       *   <hcunit id="1005" level="6" short="2jg1g" nick="1.Gruppe" title="I.Gruppe 2.Jagdgeschwader" parent="1003" ctry="4" branch="2" moves="2" owner="leopold" toe="deairfor" />
       *   ...
       * </hcunitlist>
       */

      // initialise game object collections

      this.gameState.HCUnits.Clear();
      this.gameState.HCUnits.Add( 0, new HCUnit( 0, HCUnitLevel.Top, null, null, null, null, this.gameState.Countries[0], Branch.None, MoveType.None, null, null ) );  // toplevel parent


      // loop over xml data

      MultiExceptionHandler mex = new MultiExceptionHandler();
      for( e.Data.Offset = 0; e.Data.Offset < e.Data.Count; e.Data.Offset++ )
      {
        try
        {
          // parse and validate attribute values

          int id = e.Data.GetValue<int>( "id" );
          e.CheckAttributeDupe( "id", id, this.gameState.HCUnits );

          int level = e.Data.GetValue<int>( "level" );
          string shortname = e.Data.GetValue<string>( "short" );
          string nick = e.Data.GetValue<string>( "nick" );
          string title = e.Data.GetValue<string>( "title" );

          int parentid = e.Data.GetValue<int>( "parent" );
          e.CheckAttribute( "parent", parentid, this.gameState.HCUnits );

          int countryid = e.Data.GetValue<int>( "ctry" );
          e.CheckAttribute( "ctry", countryid, this.gameState.Countries );

          int branch = e.Data.GetValue<int>( "branch" );
          e.CheckAttribute( "branch", branch, typeof( Branch ) );

          int movetype = e.Data.GetValue<int>( "moves" );
          e.CheckAttribute( "moves", movetype, typeof( MoveType ) );

          string owner = e.Data.GetValue<string>( "owner" );

          string toecode = e.Data.GetValue<string>( "toe" );
          Toe toe;
          if( this.gameState.Toes.Count == 0 || toecode == "" )
            toe = null;
          else
          {
            e.CheckAttribute( "toe", toecode, this.gameState.Toes );
            toe = this.gameState.Toes[toecode];
          }


          // create game objects

          this.gameState.HCUnits.Add( id, new HCUnit( id, (HCUnitLevel)level, shortname, nick, title, this.gameState.HCUnits[parentid],
                                           this.gameState.Countries[countryid], (Branch)branch, (MoveType)movetype, owner, toe ) );
        }
        catch( Exception ex ) { mex.Add( ex ); }

      }  // for e.Data


      // create hierarchy

      try
      {
        foreach( HCUnit unit in this.gameState.HCUnits.Values )
          if( unit.ParentUnit != null )
            unit.ParentUnit.AddChild( unit );  // add self to parent's child collection
      }
      catch( Exception ex ) { mex.Add( ex ); }


      // if any errors, throw first

      mex.Throw();
    }

    /// <summary>
    /// Parses the squadlist metadata and create new Squads.
    /// </summary>
    private void MetaXmlFile_Squad_PostParse( object sender, PostParseEventArgs e )
    {
      /* <squadlist>
       *   <squad id="8883" handle="kaisers" name="" co="ryan5175" created="2003-11-04 21:39:30" pri-country="0" pri-branch="0" pri-brigade="0" />
       *   ...
       * </squadlist>
       */


      // initialise game object collections

      this.gameState.Squads.Clear();
      this.gameState.Squads.Add( 0, null );


      // loop over xml data

      MultiExceptionHandler mex = new MultiExceptionHandler();
      for( e.Data.Offset = 0; e.Data.Offset < e.Data.Count; e.Data.Offset++ )
      {
        try
        {
          // parse and validate attribute values

          int id = e.Data.GetValue<int>( "id" );
          e.CheckAttributeDupe( "id", id, this.gameState.Squads );

          string handle = e.Data.GetValue<string>( "handle" );
          string name = e.Data.GetValue<string>( "name" );
          string co = e.Data.GetValue<string>( "co" );


          // create game objects

          this.gameState.Squads.Add( id, new Squad( id, handle, name, co ) );
        }
        catch( Exception ex ) { mex.Add( ex ); }

      }  // for e.Data


      // if any errors, throw first

      mex.Throw();
    }

    /// <summary>
    /// Parses the moveresults metadata and create an array of HCUnitMoveResult objects.
    /// </summary>
    private void MetaXmlFile_MoveResults_PostParse( object sender, PostParseEventArgs e )
    {
      /* <moveresults>
       *   <r id="0" description="Approved" />
       *   ...
       * </moveresults>
       */


      // initialise game object collections

      int max = e.Data.GetMaxValue( "id" );
      this.gameState.MoveResults = new HCUnitMoveResult[max + 1];


      // loop over xml data

      MultiExceptionHandler mex = new MultiExceptionHandler();
      for( e.Data.Offset = 0; e.Data.Offset < e.Data.Count; e.Data.Offset++ )
      {
        try
        {
          // parse and validate attribute values

          int id = e.Data.GetValue<int>( "id" );
          e.CheckAttributeDupe( "id", id, this.gameState.MoveResults );

          string desc = e.Data.GetValue<string>( "description" );


          // create game objects

          this.gameState.MoveResults[id] = new HCUnitMoveResult( id, desc );

        }
        catch( Exception ex ) { mex.Add( ex ); }

      }  // for e.Data


      // if any errors, throw first

      mex.Throw();
    }

    /// <summary>
    /// Parses the server config metadata and create a list of ServerParam objects.
    /// </summary>
    private void MetaXmlFile_Config_PostParse( object sender, PostParseEventArgs e )
    {
      /* <config>
       *   <r id="arena.campaign_id" val="45" desc="Current Campaign Number" />
       *   ...
       * </config>
       */


      // initialise game object collections

      this.gameState.ServerParams.Clear();


      // loop over xml data

      MultiExceptionHandler mex = new MultiExceptionHandler();
      for( e.Data.Offset = 0; e.Data.Offset < e.Data.Count; e.Data.Offset++ )
      {
        try
        {
          // parse and validate attribute values

          string key = e.Data.GetValue<string>( "id" );
          e.CheckAttributeDupe( "id", key, this.gameState.ServerParams );

          string value = e.Data.GetValue<string>( "val" );
          string desc = e.Data.GetValue<string>( "desc", key );


          // create game objects

          this.gameState.ServerParams.Add( key, new ServerParam( key, value, desc ) );

        }
        catch( Exception ex ) { mex.Add( ex ); }

      }  // for e.Data


      // if any errors, throw first

      mex.Throw();
    }

    #endregion

    #region InitXmlFile Parsers

    /* InitParser_CPOwner & InitParser_Captures
     * 
     * We first parse cps.xml (ignoring the facilities in contested cps) to get to an approximate
     * current state, then overwrite it with the past 2 hours of captures, generating events.
     * 
     * NOTE: This will break if there's a gap in capture data.
     */

    /// <summary>
    /// Parses the cp state data to set any non-default owner/controllers, and the
    /// initial AOs.
    /// </summary>
    private void InitXmlFile_CPOwner_PostParse( object sender, PostParseEventArgs e )
    {
      /* <cps>
       *   <cp id="77" own="4/2" [ao="1048"] [ctrl="1/1"] [contention="true"] /> 
       *   ...
       * </cps>
       */


      // loop over xml data

      MultiExceptionHandler mex = new MultiExceptionHandler();
      for( e.Data.Offset = 0; e.Data.Offset < e.Data.Count; e.Data.Offset++ )
      {
        try
        {
          // parse and validate attribute values

          int id = e.Data.GetValue<int>( "id" );
          if( this.excludedChokepoints.Contains( id ) || this.gameState.Bridges[id] != null ) continue;
          e.CheckAttribute( "id", id, this.gameState.ChokePoints );

          int owner = e.Data.GetValue<int>( "own" );
          e.CheckAttribute( "own", owner, this.gameState.Countries );

          int controller = e.Data.GetValue<int>( "ctrl", owner );
          e.CheckAttribute( "ctrl", controller, this.gameState.Countries );

          int aohcunitid = e.Data.GetValue<int>( "ao" );
          if( aohcunitid != 0 )
            e.CheckAttribute( "ao", aohcunitid, this.gameState.HCUnits );


          // update game objects

          this.gameState.ChokePoints[id].SetInitState( this.gameState.Countries[owner], this.gameState.Countries[controller], aohcunitid == 0 ? null : this.gameState.HCUnits[aohcunitid] );

        }
        catch( Exception ex ) { mex.Add( ex ); }

      }  // for e.Data


      // if any errors, throw first

      mex.Throw();
    }

    /// <summary>
    /// Parses the hcunit location data to set their initial location and last moved state.
    /// </summary>
    private void InitXmlFile_HCLocations_PostParse( object sender, PostParseEventArgs e )
    {
      /* <hc-locations>
       *   <unit id="934" cp="0" moved="0" by="SYSTEM" nxtmv="0" /> 
       *   <unit id="935" cp="290" moved="1217348273" by="ltibbs" nxtmv="1217351873" /> 
       *   ...
       * </hc-locations>
       */


      // loop over xml data

      MultiExceptionHandler mex = new MultiExceptionHandler();
      for( e.Data.Offset = 0; e.Data.Offset < e.Data.Count; e.Data.Offset++ )
      {
        try
        {
          // parse and validate attribute values

          int hcunitid = e.Data.GetValue<int>( "id" );
          e.CheckAttribute( "id", hcunitid, this.gameState.HCUnits );

          int cpid = e.Data.GetValue<int>( "cp" );
          if( this.excludedChokepoints.Contains( cpid ) ) continue;
          if( cpid > 0 )
            e.CheckAttribute( "cp", cpid, this.gameState.ChokePoints );
          ChokePoint cp = ( cpid == 0 || this.gameState.ChokePoints[cpid].IsTraining ) ? null : this.gameState.ChokePoints[cpid];

          DateTime lastMovedTime = e.Data.GetValue<DateTime>( "moved" );
          string lastMovedPlayer = e.Data.GetValue<string>( "by" );
          DateTime nextMoveTime = e.Data.GetValue<DateTime>( "nxtmv" );


          // update game objects

          this.gameState.HCUnits[hcunitid].SetInitState( cp, lastMovedTime, lastMovedPlayer, nextMoveTime );

        }
        catch( Exception ex ) { mex.Add( ex ); }

      }  // for e.Data


      // if any errors, throw first

      mex.Throw();
    }

    /// <summary>
    /// Parses the last 12 hours of hcmove requests, populating each
    /// units move history and generating historical events.
    /// </summary>
    private void InitXmlFile_HCMoveReq_PreParse( object sender, EventArgs e )
    {
      ((InitXmlFile)sender).QueryParam = Misc.DateToSeconds( DateTime.Now.AddHours( -12 ) );
    }
    private void InitXmlFile_HCMoveReq_PostParse( object sender, PostParseEventArgs e )
    {
      /* <move-requests>
       *   <r at="1212970608" by="keitel66" ex="0" id="914" from="579" to="502" res="0" />
       *   <r at="1212970751" by="keitel66" ex="1" id="914" from="579" to="502" res="0" />
       *   ...
       * </move-requests>
       */


      // loop over xml data

      MultiExceptionHandler mex = new MultiExceptionHandler();
      for( e.Data.Offset = 0; e.Data.Offset < e.Data.Count; e.Data.Offset++ )  // oldest-to-newest
      {
        try
        {
          // parse and validate attribute values

          int seconds = e.Data.GetValue<int>( "at" );
          DateTime time = Misc.ParseTimestamp( seconds );

          string player = e.Data.GetValue<string>( "by" );
          bool request = !e.Data.GetValue<bool>( "ex" );  // 0 = false = request

          int hcunitid = e.Data.GetValue<int>( "id" );
          e.CheckAttribute( "id", hcunitid, this.gameState.HCUnits );
          
          int fromid = e.Data.GetValue<int>( "from" );
          if( fromid == 0 || this.excludedChokepoints.Contains( fromid ) ) continue;
          e.CheckAttribute( "from", fromid, this.gameState.ChokePoints );

          int toid = e.Data.GetValue<int>( "to" );
          if( toid == 0 || this.excludedChokepoints.Contains( toid ) ) continue;
          e.CheckAttribute( "to", toid, this.gameState.ChokePoints );

          int resultid = e.Data.GetValue<int>( "res" );
          e.CheckAttribute( "res", resultid, this.gameState.MoveResults );


          // hcunit events are 1 minute behind capture events for some reason
          // (can be seen when a capture forces an automatic hcunit move)

          time = time.AddMinutes( 1 );


          // update game objects

          if( request )
            this.gameState.HCUnits[hcunitid].AddMoveRequest( this.gameState.ChokePoints[fromid], this.gameState.ChokePoints[toid], time, player, this.gameState.MoveResults[resultid] );
          else
            this.gameState.HCUnits[hcunitid].AddMoveAttempt( this.gameState.ChokePoints[fromid], this.gameState.ChokePoints[toid], time, player, this.gameState.MoveResults[resultid] );


          // remember latest move time for polling

          if( e.Data.Offset == e.Data.Count - 1 )  
            this.pollHCMoveReq.QueryParam = seconds;

        }
        catch( Exception ex ) { mex.Add( ex ); }

      }  // for e.Data


      // purge any stale pending requests

      try
      {
        DateTime purgeLimit = DateTime.Now.AddMinutes( -25 );  // 15 min delay + 3 min timeout + margin

        /* Normally we purge events in HCUnit.PruneMoveHistory() if they haven't been
         * completed within 4 mins of receiving them (this isn't affected by delay or
         * clock skew issues). However this won't handle stale pending moves on startup
         * as all moves have been recently received. Thus we purge here based on request
         * time with a purgelimit taking into account the data delay.
         */

        foreach( HCUnit hcunit in this.gameState.HCUnits.Values )
          foreach( HCUnitMove move in hcunit.Moves )
            if( move.State == HCUnitMoveState.Pending && move.RequestTime < purgeLimit )
              move.Cancel();
      }
      catch( Exception ex ) { mex.Add( ex ); }


      // if any errors, throw first

      mex.Throw();
    }

    /// <summary>
    /// Parses the last 12 hours of hcmoves by SYSTEM, populating each
    /// units move history and generating historical events.
    /// </summary>
    private void InitXmlFile_HCMoves_PreParse( object sender, EventArgs e )
    {
      ( (InitXmlFile)sender ).QueryParam = Misc.DateToSeconds( DateTime.Now.AddHours( -12 ) );
    }
    private void InitXmlFile_HCMoves_PostParse( object sender, PostParseEventArgs e )
    {
      /* <hcmoves>
       *   <move id="1021" at="1217437384" from="351" to="350" by="SYSTEM" age="22688" delay="900" /> 
       *   ...
       * </hcmoves>
       */

      // loop over xml data

      MultiExceptionHandler mex = new MultiExceptionHandler();
      for( e.Data.Offset = e.Data.Count - 1; e.Data.Offset >= 0; e.Data.Offset-- )  // reverse, oldest-to-newest
      {
        try
        {
          // parse and validate attribute values

          int hcunitid = e.Data.GetValue<int>( "id" );
          e.CheckAttribute( "id", hcunitid, this.gameState.HCUnits );

          int seconds = e.Data.GetValue<int>( "at" );
          DateTime time = Misc.ParseTimestamp( seconds );

          int fromid = e.Data.GetValue<int>( "from" );
          if( this.excludedChokepoints.Contains( fromid ) ) continue;
          e.CheckAttribute( "from", fromid, this.gameState.ChokePoints );

          int toid = e.Data.GetValue<int>( "to" );
          if( this.excludedChokepoints.Contains( toid ) ) continue;
          e.CheckAttribute( "to", toid, this.gameState.ChokePoints );


          // hcunit events are 1 minute behind capture events for some reason
          // (can be seen when a capture forces an automatic hcunit move)

          time = time.AddMinutes( 1 );


          // update game objects

          this.gameState.HCUnits[hcunitid].AddSystemMove( this.gameState.ChokePoints[fromid], this.gameState.ChokePoints[toid], time, this.gameState.MoveResults[0] );


          // remember latest move time for polling

          if( e.Data.Offset == 0 )
            this.pollHCMoves.QueryParam = seconds;

        }
        catch( Exception ex ) { mex.Add( ex ); }

      }  // for e.Data


      // prune any move history events older than limit

      try
      {
        this.gameState.PruneHCUnitMoveHistory();
      }
      catch( Exception ex ) { mex.Add( ex ); }


      // if any errors, throw first

      mex.Throw();
    }

    /// <summary>
    /// Parses the last 2 hours of capture data to generate historical events
    /// and establish current state.
    /// </summary>
    private void InitXmlFile_Captures_PostParse( object sender, PostParseEventArgs e )
    {
      /* <captures>
       *   <cap id="2184" at="1183262529" fac="3913" from="4" to="3" by="cabby" brig="935" [con="Enter|End"] [ctrl="y"] [own="y"] />
       *   ...
       * </captures>
       */


      List<ChokePoint> seenCPs = new List<ChokePoint>();


      // loop over xml data

      MultiExceptionHandler mex = new MultiExceptionHandler();
      for( e.Data.Offset = e.Data.Count - 1; e.Data.Offset >= 0; e.Data.Offset-- )  // reverse, oldest-to-newest
      {
        try
        {
          // parse and validate attribute values

          int capid = e.Data.GetValue<int>( "id" );
          DateTime time = e.Data.GetValue<DateTime>( "at" );

          int facilityid = e.Data.GetValue<int>( "fac" );
          e.CheckAttribute( "fac", facilityid, this.gameState.Facilities );

          int fromid = e.Data.GetValue<int>( "from" );
          e.CheckAttribute( "from", fromid, this.gameState.Countries );

          int toid = e.Data.GetValue<int>( "to" );
          e.CheckAttribute( "to", toid, this.gameState.Countries );

          string by = e.Data.GetValue<string>( "by" );

          int hcunitid = e.Data.GetValue<int>( "brig" );
          e.CheckAttribute( "id", hcunitid, this.gameState.HCUnits );

          bool nowContested = e.Data.GetValue<string>( "con" ) == "Enter";
          bool nowUncontested = e.Data.GetValue<string>( "con" ) == "End";
          bool newCPController = e.Data.GetValue<bool>( "ctrl" );
          bool newCPOwner = e.Data.GetValue<bool>( "own" );


          // apply capture event

          this.gameState.Facilities[facilityid].SetCaptured( time, this.gameState.Countries[fromid], this.gameState.Countries[toid], by, this.gameState.HCUnits[hcunitid],
                                                 nowContested, nowUncontested, newCPController, newCPOwner, true );


          // remember cp

          if( !seenCPs.Contains( this.gameState.Facilities[facilityid].ChokePoint ) )
            seenCPs.Add( this.gameState.Facilities[facilityid].ChokePoint );


          // remember newest capid for polling

          if( e.Data.Offset == 0 )
            pollCaptures.QueryParam = capid;
          // if we didn't get any data, PollParser_Captures with use "...&fromid=" (which gets the last hour)

        }
        catch( Exception ex ) { mex.Add( ex ); }

      }  // for e.Data


      // workaround in case, in the past 2 hours, an AO has been pulled while contested

      try
      {
        foreach( ChokePoint cp in seenCPs )
          if( !cp.HasAO && cp.IsContested )
            cp.Uncontest();
      }
      catch( Exception ex ) { mex.Add( ex ); }


      // if any errors, throw first

      mex.Throw();
    }

    /// <summary>
    /// Parses the current list of open firebases, and sets their initial state.
    /// </summary>
    private void InitXmlFile_Firebases_PostParse( object sender, PostParseEventArgs e )
    {
      /* <openfbs>
       *   <fb id="1314" country="4" /> 
       *   ...
       * </openfbs>
       */


      // loop over xml data

      MultiExceptionHandler mex = new MultiExceptionHandler();
      for( e.Data.Offset = 0; e.Data.Offset < e.Data.Count; e.Data.Offset++ )
      {
        try
        {
          // parse and validate attribute values

          int fbid = e.Data.GetValue<int>( "id" );
          e.CheckAttribute( "id", fbid, this.gameState.Firebases );

          int countryid = e.Data.GetValue<int>( "country" );
          e.CheckAttribute( "country", countryid, this.gameState.Countries );


          // update game objects

          this.gameState.Firebases[fbid].IsOpen = true;

        }
        catch( Exception ex ) { mex.Add( ex ); }

      }  // for e.Data


      // if any errors, throw first

      mex.Throw();
    }

    /// <summary>
    /// Parses the past hour of deathmap data.
    /// </summary>
    private void InitXmlFile_DeathMap_PostParse( object sender, PostParseEventArgs e )
    {
      /* <deathmap cell-width="500">
       *   <r x="248000" y="2962500" t="1202939400" ax="8" ad="13" /> 
       *   ...
       * </deathmap>
       */

      bool firstCall = ((InitXmlFile)sender).Uri.Contains( ".1h." );  // we are called twice, 1h then 5m


      // initialise game object collections

      if( firstCall )
      {
        this.gameState.MapCells.Clear();

        try
        {
          this.gameState.MapCellSize = int.Parse( e.Xml.SelectSingleNode( "/deathmap" ).Attributes["cell-width"].Value );
        }
        catch { }  // use default 1000 if fails
      }


      // loop over xml data

      MultiExceptionHandler mex = new MultiExceptionHandler();
      for( e.Data.Offset = 0; e.Data.Offset < e.Data.Count; e.Data.Offset++ )
      {
        try
        {
          // parse and validate attribute values

          int x = e.Data.GetValue<int>( "x" );
          int y = e.Data.GetValue<int>( "y" );
          Point ptCell = new Point( x, y );

          DateTime time = e.Data.GetValue<DateTime>( "t" );
          int axis = e.Data.GetValue<int>( "ax" );
          int allied = e.Data.GetValue<int>( "ad" );


          // update game objects

          if( !this.gameState.MapCells.ContainsKey( ptCell ) )  // create cell
            this.gameState.MapCells.Add( ptCell, new MapCell() );

          this.gameState.MapCells[ptCell].AddCount( time, allied, axis );

        }
        catch( Exception ex ) { mex.Add( ex ); }

      }  // for e.Data


      try
      {
        // purge old entries

        List<Point> toRemove = new List<Point>();  // can't remove while enumerating
        foreach( Point p in this.gameState.MapCells.Keys )
          if( this.gameState.MapCells[p].Purge() )
            toRemove.Add( p );
        foreach( Point p in toRemove )
          this.gameState.MapCells.Remove( p );


        // update activity levels for each cp

        if( !firstCall )
          this.gameState.UpdateActivityLevels();

      }
      catch( Exception ex ) { mex.Add( ex ); }


      // if any errors, throw first

      mex.Throw();
    }

    /// <summary>
    /// Parses the initial server list and state.
    /// </summary>
    private void InitXmlFile_Servers_PostParse( object sender, PostParseEventArgs e )
    {
      /* <servers>
       *   <r arena="1" name="Blitzkrieg" state="Online" pop="Average" /> 
       *   ...
       * </servers>
       * 
       * Valid states:
       *   Offline, Starting, Syncing, Online,
       *   Locked:NoCap, Locked:NoSpawn, Locked:NoKill, Locked:NoCap:NoSpawn:NoKill,
       *   Locked:NoCap:NoSpawn, Locked:NoCap:NoKill, Locked:NoSpawn:NoKill, Closed
       * 
       * Valid populations:
       *   Empty, Very Light, Low, Average, Good, High, Very High
       */


      // initialise game object collections

      this.gameState.Servers.Clear();


      // loop over xml data

      MultiExceptionHandler mex = new MultiExceptionHandler();
      for( e.Data.Offset = 0; e.Data.Offset < e.Data.Count; e.Data.Offset++ )
      {
        try
        {
          // parse and validate attribute values

          int id = e.Data.GetValue<int>( "arena" );
          e.CheckAttributeDupe( "arena", id, this.gameState.Servers );

          string name = e.Data.GetValue<string>( "name" );
          
          ServerState state = e.Data.GetValue<ServerState>( "state" );
          string stateInfo = Misc.SubstringAfter( e.Data.GetValue<string>( "state" ), ":" );
          ServerPopulation population = e.Data.GetValue<ServerPopulation>( "pop" );

          if( id == 1 )
            name = "Live";  // NOT localised


          // create new server

          this.gameState.Servers[id] = new Server( id, name, state, stateInfo, population );

        }
        catch( Exception ex ) { mex.Add( ex ); }

      }  // for e.Data


      // poll every minute if live server not online

      try
      {
        if( this.gameState.Servers.ContainsKey( 1 ) && !this.gameState.Servers[1].Online )
          pollServers.UpdateFreq = new TimeSpan( 0, 1, 0 );
      }
      catch( Exception ex ) { mex.Add( ex ); }


      // if any errors, throw first

      mex.Throw();
    }

    /// <summary>
    /// Parses the current summary stats for each factory.
    /// </summary>
    private void InitXmlFile_FactoryStats_PostParse( object sender, PostParseEventArgs e )
    {
      /* <factorystats>
       *   <factory id="70" from="1183051374" to="1183262874" ticks="236" produced="236" />
       *   ...
       * </factorystats>
       */

      // loop over xml data

      MultiExceptionHandler mex = new MultiExceptionHandler();
      for( e.Data.Offset = 0; e.Data.Offset < e.Data.Count; e.Data.Offset++ )
      {
        try
        {
          // parse and validate attribute values

          int id = e.Data.GetValue<int>( "id" );
          e.CheckAttribute( "id", id, this.gameState.Factories );

          DateTime from = e.Data.GetValue<DateTime>( "from" );
          DateTime to = e.Data.GetValue<DateTime>( "to" );
          int ticks = e.Data.GetValue<int>( "ticks" );
          int produced = e.Data.GetValue<int>( "produced" );


          // update game objects

          this.gameState.Factories[id].SetInitState( from, to, ticks, produced );
        }
        catch( Exception ex ) { mex.Add( ex ); }

      }  // for e.Data


      // if any errors, throw first

      mex.Throw();
    }

    /// <summary>
    /// Parses the last 24 hours of factory tick data, generating historical events.
    /// </summary>
    private void InitXmlFile_FactoryLog_PostParse( object sender, PostParseEventArgs e )
    {
      /* <rows>
       *   <row fid="3810" at="1172121700" damage="23" rdp="0" />
       *   ...
       * </rows>
       */


      // loop over xml data

      MultiExceptionHandler mex = new MultiExceptionHandler();
      for( e.Data.Offset = e.Data.Count - 1; e.Data.Offset >= 0; e.Data.Offset-- )  // reverse, oldest-to-newest
      {
        try
        {
          // parse and validate attribute values

          int id = e.Data.GetValue<int>( "fid" );
          e.CheckAttribute( "fid", id, this.gameState.Factories );

          int seconds = e.Data.GetValue<int>( "at" );
          DateTime stamp = Misc.ParseTimestamp( seconds );

          int damage = e.Data.GetValue<int>( "damage" );
          int rdp = e.Data.GetValue<int>( "rdp" );


          // update game objects

          this.gameState.Factories[id].AddTick( new FactoryTick( stamp, damage, rdp ) );
          
          
          // remember newest tick timestamp for polling

          if( e.Data.Offset == 0 )
            this.pollFactoryLog.QueryParam = seconds;

        }
        catch( Exception ex ) { mex.Add( ex ); }

      }  // for e.Data


      // if any errors, throw first

      mex.Throw();
    }

    #endregion

    #region PollXmlFile Parsers

    /// <summary>
    /// Called every minute, parses any new captures since last poll, generating
    /// new events.
    /// </summary>
    private void PollXmlFile_Captures_PostParse( object sender, PostParseEventArgs e )
    {
      /* <captures>
       *   <fromid id="2184" />
       *   ...
       *   <cap id="2184" at="1183262529" fac="3913" from="4" to="3" by="cabby" brig="935" [con="Enter|End"] [ctrl="y"] [own="y"] />
       * </captures>
       */


      // add clock skew correction data point

      if( e.Data.Count > 1 )
      {
        bool updated = Misc.ClockSkew.AddDataPoint( e.Data.GetValue<DateTime>( "at" ) ); // row 0 = newest timestamp
        if( updated )
          this.gameState.Events.UpdateSkewCorrection();
      }


      // loop over xml data

      MultiExceptionHandler mex = new MultiExceptionHandler();
      for( e.Data.Offset = e.Data.Count - 2; e.Data.Offset >= 0; e.Data.Offset-- )  // reverse, oldest-to-newest, skip oldest
      {
        try
        {
          // parse and validate attribute values

          int capid = e.Data.GetValue<int>( "id" );
          DateTime time = e.Data.GetValue<DateTime>( "at" );

          int facilityid = e.Data.GetValue<int>( "fac" );
          e.CheckAttribute( "fac", facilityid, this.gameState.Facilities );

          int fromid = e.Data.GetValue<int>( "from" );
          e.CheckAttribute( "from", fromid, this.gameState.Countries );

          int toid = e.Data.GetValue<int>( "to" );
          e.CheckAttribute( "to", toid, this.gameState.Countries );

          string by = e.Data.GetValue<string>( "by" );

          int hcunitid = e.Data.GetValue<int>( "brig" );
          e.CheckAttribute( "brig", hcunitid, this.gameState.HCUnits );

          bool nowContested = e.Data.GetValue<string>( "con" ) == "Enter";
          bool nowUncontested = e.Data.GetValue<string>( "con" ) == "End";
          bool newCPController = e.Data.GetValue<bool>( "ctrl" );
          bool newCPOwner = e.Data.GetValue<bool>( "own" );


          // apply capture event

          this.gameState.Facilities[facilityid].SetCaptured( time, this.gameState.Countries[fromid], this.gameState.Countries[toid], by, this.gameState.HCUnits[hcunitid],
                                                       nowContested, nowUncontested, newCPController, newCPOwner, false );


          // remember newest capid for next poll

          if( e.Data.Offset == 0 )
            this.pollCaptures.QueryParam = capid;

        }
        catch( Exception ex ) { mex.Add( ex ); }

      }  // for e.Data


      // flag xmlfile as checked

      this.pollCaptures.FlagAsChecked( e.Data.Count - 1 );


      // if any errors, throw first

      mex.Throw();
    }

    /// <summary>
    /// Called every minute, parses the current AO list, generating new events if
    /// any have changed.
    /// </summary>
    private void PollXmlFile_AOs_PostParse( object sender, PostParseEventArgs e )
    {
      /* <cps>
       *   <cp id="77" own="4/2" [ao="1048"] [ctrl="1/1"] [contention="true"] />
       *   ...
       * </cps>
       */


      // get list of what AO's should be

      int[] newAOs = new int[this.gameState.ChokePoints.Length];

      MultiExceptionHandler mex = new MultiExceptionHandler();
      for( e.Data.Offset = 0; e.Data.Offset < e.Data.Count; e.Data.Offset++ )
      {
        try
        {
          int cpid = e.Data.GetValue<int>( "id" );
          if( this.excludedChokepoints.Contains( cpid ) || this.gameState.Bridges[cpid] != null ) continue;
          e.CheckAttribute( "cpid", cpid, this.gameState.ChokePoints );

          int aohcunitid = e.Data.GetValue<int>( "ao" );  // should always be non zero
          if( aohcunitid != 0 )
            e.CheckAttribute( "ao", aohcunitid, this.gameState.HCUnits );

          newAOs[cpid] = aohcunitid;
        }
        catch( Exception ex ) { mex.Add( ex ); }

      }  // for e.Data


      try
      {
        // apply list against what we currently have

        int newDataCount = 0;

        for( int cpid = 0; cpid < this.gameState.ChokePoints.Length; cpid++ )
        {
          if( this.gameState.ChokePoints[cpid] == null ) continue;

          if(    ( this.gameState.ChokePoints[cpid].HasAO != ( newAOs[cpid] != 0 ) )                                  // AO added or removed
              || ( this.gameState.ChokePoints[cpid].HasAO && this.gameState.ChokePoints[cpid].AttackingHCUnit.ID != newAOs[cpid] ) )  // attacking brigade changed
          {
            this.gameState.ChokePoints[cpid].SetNewAOState( DateTime.Now, newAOs[cpid] == 0 ? null : this.gameState.HCUnits[newAOs[cpid]] );
            newDataCount++;
          }
        }


        // flag xmlfile as checked

        this.pollAOs.FlagAsChecked( newDataCount );

      }
      catch( Exception ex ) { mex.Add( ex ); }


      // if any errors, throw first

      mex.Throw();
    }

    /// <summary>
    /// Called every minute, parses any moves requests/attempts since
    /// last poll, generating new events.
    /// </summary>
    private void PollXmlFile_HCMoveReq_PostParse( object sender, PostParseEventArgs e )
    {
      /* <move-requests>
       *   <r at="1212970608" by="keitel66" ex="0" id="914" from="579" to="502" res="0" /> 
       *   <r at="1212970751" by="keitel66" ex="1" id="914" from="502" to="502" res="0" /> 
       *   ...
       * </move-requests>
       */


      // loop over xml data

      int newDataCount = 0;
      MultiExceptionHandler mex = new MultiExceptionHandler();
      for( e.Data.Offset = 0; e.Data.Offset < e.Data.Count; e.Data.Offset++ )
      {
        try
        {
          // parse and validate attribute values

          int seconds = e.Data.GetValue<int>( "at" );
          if( seconds == this.pollHCMoveReq.QueryParam )
            continue;  // already seen this value (may be more than one if have same timestamp)
          DateTime time = Misc.ParseTimestamp( seconds );

          string player = e.Data.GetValue<string>( "by" );
          bool request = !e.Data.GetValue<bool>( "ex" );  // 0 = false = request

          int hcunitid = e.Data.GetValue<int>( "id" );
          e.CheckAttribute( "id", hcunitid, this.gameState.HCUnits );

          int fromid = e.Data.GetValue<int>( "from" );
          if( this.excludedChokepoints.Contains( fromid ) ) continue;
          e.CheckAttribute( "from", fromid, this.gameState.ChokePoints );

          int toid = e.Data.GetValue<int>( "to" );
          if( this.excludedChokepoints.Contains( toid ) ) continue;
          e.CheckAttribute( "to", toid, this.gameState.ChokePoints );

          int resultid = e.Data.GetValue<int>( "res" );
          e.CheckAttribute( "res", resultid, this.gameState.MoveResults );


          // hcunit events are 1 minute behind capture events for some reason
          // (can be seen when a capture forces an automatic hcunit move)

          time = time.AddMinutes( 1 );


          // update game objects

          if( request )
            this.gameState.HCUnits[hcunitid].AddMoveRequest( this.gameState.ChokePoints[fromid], this.gameState.ChokePoints[toid], time, player, this.gameState.MoveResults[resultid] );
          else
            this.gameState.HCUnits[hcunitid].AddMoveAttempt( this.gameState.ChokePoints[fromid], this.gameState.ChokePoints[toid], time, player, this.gameState.MoveResults[resultid] );


          // count new data

          if( !request && resultid == 0 )  // successful move attempt
            newDataCount++;


          // remember newest move time for next poll

          if( e.Data.Offset == e.Data.Count - 1 )
            this.pollHCMoveReq.QueryParam = seconds;

        }
        catch( Exception ex ) { mex.Add( ex ); }

      }  // for e.Data


      // flag xmlfile as checked

      this.pollHCMoveReq.FlagAsChecked( newDataCount );


      // if any errors, throw first

      mex.Throw();
    }

    /// <summary>
    /// Called every minute, parses any SYSTEM moves since last poll,
    /// generating new events.
    /// </summary>
    private void PollXmlFile_HCMoves_PostParse( object sender, PostParseEventArgs e )
    {
      /* <hcmoves>
       *   <move id="1021" at="1217437384" from="351" to="350" by="SYSTEM" age="22688" delay="900" /> 
       *   ...
       * </hcmoves>
       */


      // loop over xml data

      int newDataCount = 0;
      MultiExceptionHandler mex = new MultiExceptionHandler();
      for( e.Data.Offset = e.Data.Count - 1; e.Data.Offset >= 0; e.Data.Offset-- )  // reverse, oldest-to-newest
      {
        try
        {
          // parse and validate attribute values

          int seconds = e.Data.GetValue<int>( "at" );
          if( seconds == this.pollHCMoves.QueryParam )
            continue;  // already seen this value (may be more than one if have same timestamp)
          DateTime time = Misc.ParseTimestamp( seconds );

          int hcunitid = e.Data.GetValue<int>( "id" );
          e.CheckAttribute( "id", hcunitid, this.gameState.HCUnits );

          int fromid = e.Data.GetValue<int>( "from" );
          if( this.excludedChokepoints.Contains( fromid ) ) continue;
          e.CheckAttribute( "from", fromid, this.gameState.ChokePoints );

          int toid = e.Data.GetValue<int>( "to" );
          if( this.excludedChokepoints.Contains( toid ) ) continue;
          e.CheckAttribute( "to", toid, this.gameState.ChokePoints );


          // hcunit events are 1 minute behind capture events for some reason
          // (can be seen when a capture forces an automatic hcunit move)

          time = time.AddMinutes( 1 );


          // update game objects

          this.gameState.HCUnits[hcunitid].AddSystemMove( this.gameState.ChokePoints[fromid], this.gameState.ChokePoints[toid], time, this.gameState.MoveResults[0] );


          // count new data

          newDataCount++;


          // remember latest move time for polling

          if( e.Data.Offset == 0 )
            this.pollHCMoves.QueryParam = seconds;

        }
        catch( Exception ex ) { mex.Add( ex ); }

      }  // for e.Data


      // flag xmlfile as checked

      this.pollHCMoves.FlagAsChecked( newDataCount );


      // prune any move history events older than limit

      try
      {
        this.gameState.PruneHCUnitMoveHistory();
      }
      catch( Exception ex ) { mex.Add( ex ); }


      // if any errors, throw first

      mex.Throw();
    }

    /// <summary>
    /// Called every minute, parses the current open firebase list, generating new
    /// events if any have changed.
    /// </summary>
    /// <remarks>Should be called after PollParser_HCMoves due to brigade fbs.</remarks>
    private void PollXmlFile_Firebases_PostParse( object sender, PostParseEventArgs e )
    {
      /* <openfbs>
       *   <fb id="1314" country="4" /> 
       *   ...
       * </openfbs>
       */


      // create hash of open fbs

      Dictionary<int, int> fbstate = new Dictionary<int, int>();
      MultiExceptionHandler mex = new MultiExceptionHandler();
      for( e.Data.Offset = 0; e.Data.Offset < e.Data.Count; e.Data.Offset++ )
      {
        try
        {
          int fbid = e.Data.GetValue<int>( "id" );
          e.CheckAttribute( "id", fbid, this.gameState.Firebases );

          int countryid = e.Data.GetValue<int>( "country" );
          e.CheckAttribute( "country", countryid, this.gameState.Countries );

          fbstate[fbid] = countryid;

        }
        catch( Exception ex ) { mex.Add( ex ); }

      }  // for e.Data


      try
      {
        // apply against current state

        int newDataCount = 0;

        foreach( Firebase fb in this.gameState.Firebases.Values )
        {
          bool newOpen = fbstate.ContainsKey( fb.ID );

          // Skip closing the blown side of an active fb pair, it will be closed when it's
          // partner is set to open. This way we can set the fb owner.
          if( fb.IsOpen && !newOpen && fb.Link.State != FirebaseState.Inactive )
            continue;

          int countryid;  // if closed, country is [0] "No Country" (which fb.SetNewState() ignores)
          fbstate.TryGetValue( fb.ID, out countryid );

          if( fb.IsOpen != newOpen )  // changed
          {
            fb.SetNewState( newOpen, this.gameState.Countries[countryid] );  // also updates linked fb

            if( fb.IsOpen == newOpen )
              newDataCount++;
  #if DEBUG_DISABLED
            // DEBUG: occurs when trying to open a brigade fb before the hcunit move event has arrived
            //        (we think the state should be inactive, so the change doesn't get applied)
            else  
              Log.AddError( "DEBUG: {0} open({1}=>{2}) failed: state({3})", fb, fb.IsOpen, newOpen, fb.Link.State );
  #endif
          }

        }  // foreach firebase


        // flag xmlfile as checked

        this.pollFirebases.FlagAsChecked( newDataCount );

      }
      catch( Exception ex ) { mex.Add( ex ); }


      // if any errors, throw first

      mex.Throw();
    }

    /// <summary>
    /// Called every minute, parses the last minute of deathmap data.
    /// </summary>
    private void PollXmlFile_DeathMap_PostParse( object sender, PostParseEventArgs e )
    {
      /* <deathmap>
       *   <r x="248000" y="2962500" t="1202939400" ax="8" ad="13" /> 
       *   ...
       * </deathmap>
       */


      // loop over xml data

      MultiExceptionHandler mex = new MultiExceptionHandler();
      for( e.Data.Offset = 0; e.Data.Offset < e.Data.Count; e.Data.Offset++ )
      {
        try
        {
          // parse and validate attribute values

          int x = e.Data.GetValue<int>( "x" );
          int y = e.Data.GetValue<int>( "y" );
          Point ptCell = new Point( x, y );

          DateTime time = e.Data.GetValue<DateTime>( "t" );
          int axis = e.Data.GetValue<int>( "ax" );
          int allied = e.Data.GetValue<int>( "ad" );


          // update game objects

          if( !this.gameState.MapCells.ContainsKey( ptCell ) )  // create cell
            this.gameState.MapCells.Add( ptCell, new MapCell() );

          this.gameState.MapCells[ptCell].AddCount( time, allied, axis );

        }
        catch( Exception ex ) { mex.Add( ex ); }

      }  // for e.Data


      try
      {
        // purge old entries

        List<Point> toRemove = new List<Point>();  // can't remove while enumerating
        foreach( Point p in this.gameState.MapCells.Keys )
          if( this.gameState.MapCells[p].Purge() )
            toRemove.Add( p );
        foreach( Point p in toRemove )
          this.gameState.MapCells.Remove( p );


        // update activity levels for each cp

        this.gameState.UpdateActivityLevels();

      }
      catch( Exception ex ) { mex.Add( ex ); }


      // flag xmlfile as checked

      this.pollDeathMap.FlagAsChecked( 1 );


      // if any errors, throw first

      mex.Throw();
    }

    /// <summary>
    /// Called on every 5 minutes, updates the state/population for each server.
    /// </summary>
    private void PollXmlFile_Servers_PostParse( object sender, PostParseEventArgs e )
    {
      /* <servers>
       *   <r arena="1" name="Blitzkrieg" state="Online" pop="Average" /> 
       *   ...
       * </servers>
       * 
       * Valid states:
       *   Offline, Starting, Syncing, Online,
       *   Locked:NoCap, Locked:NoSpawn, Locked:NoKill, Locked:NoCap:NoSpawn:NoKill,
       *   Locked:NoCap:NoSpawn, Locked:NoCap:NoKill, Locked:NoSpawn:NoKill, Closed
       * 
       * Valid populations:
       *   Empty, Very Light, Low, Average, Good, High, Very High
       */


      // loop over xml data

      MultiExceptionHandler mex = new MultiExceptionHandler();
      for( e.Data.Offset = 0; e.Data.Offset < e.Data.Count; e.Data.Offset++ )
      {
        try
        {
          // parse and validate attribute values

          int id = e.Data.GetValue<int>( "arena" );
          e.CheckAttribute( "arena", id, this.gameState.Servers );

          ServerState state = e.Data.GetValue<ServerState>( "state" );
          string stateInfo = Misc.SubstringAfter( e.Data.GetValue<string>( "state" ), ":" );
          ServerPopulation population = e.Data.GetValue<ServerPopulation>( "pop" );


          // update game objects

          this.gameState.Servers[id].UpdateStatus( state, stateInfo, population );

        }
        catch( Exception ex ) { mex.Add( ex ); }

      }  // for e.Data


      // poll every minute if live server not online

      try
      {
        if( this.gameState.Servers.ContainsKey( 1 ) && !this.gameState.Servers[1].Online )
          pollServers.UpdateFreq = new TimeSpan( 0, 1, 0 );
        else
          pollServers.UpdateFreq = new TimeSpan( 0, 5, 0 );
      }
      catch( Exception ex ) { mex.Add( ex ); }


      // flag xmlfile as checked

      this.pollServers.FlagAsChecked( 1 );


      // if any errors, throw first

      mex.Throw();
    }

    /// <summary>
    /// If factory data is loaded, called every 15 minutes to parse the latest
    /// factory tick(s). Uses the "from" query param to only download new data. Is initially
    /// called every minute until it receives the first tick.
    /// </summary>
    private void PollXmlFile_FactoryLog_PostParse( object sender, PostParseEventArgs e )
    {
      /* <rows>
       *   <row fid="3810" at="1172121700" damage="23" rdp="0" />
       *   ...
       * </rows>
       */

      // loop over xml data

      MultiExceptionHandler mex = new MultiExceptionHandler();
      for( e.Data.Offset = e.Data.Count - 1; e.Data.Offset >= 0; e.Data.Offset-- )  // reverse, oldest-to-newest
      {
        try
        {
          // parse and validate attribute values

          int id = e.Data.GetValue<int>( "fid" );
          e.CheckAttribute( "fid", id, this.gameState.Factories );

          int seconds = e.Data.GetValue<int>( "at" );
          DateTime stamp = Misc.ParseTimestamp( seconds );

          int damage = e.Data.GetValue<int>( "damage" );
          int rdp = e.Data.GetValue<int>( "rdp" );


          // update game objects

          this.gameState.Factories[id].AddTick( new FactoryTick( stamp, damage, rdp ) );


          // remember newest tick timestamp for next poll

          if( e.Data.Offset == 0 )
            this.pollFactoryLog.QueryParam = seconds;

        }
        catch( Exception ex ) { mex.Add( ex ); }

      }  // for e.Data


      // flag xmlfile as checked

      try
      {
        if( e.Data.Count > 0 )
        {
          int numberOfTicks = (int)Math.Ceiling( (double)e.Data.Count / (double)this.gameState.Factories.Count );  // or part thereof
          this.pollFactoryLog.FlagAsChecked( numberOfTicks );
        }
        // else, check again each event tick until we get data
      }
      catch( Exception ex ) { mex.Add( ex ); }


      // if any errors, throw first

      mex.Throw();
    }

    /// <summary>
    /// Parses the current rdp cycle info. (Used for init AND poll.)
    /// </summary>
    /// <remarks>Must run after InitParser_FactoryStats and InitParser_FactoryLog.</remarks>
    private void PollXmlFile_RDP_PostParse( object sender, PostParseEventArgs e )
    {
      /* <rdp>
       *   <rdp id="1" cycle="2" cur_cycle_goal="10000" cur_cycle_produced="263" /> 
       * </rdp>
       */

      bool polling = sender is PollXmlFile;  // called during init AND poll
      int newDataCount = 0;
      int driftUK = 0, driftFR = 0, driftDE = 0;


      // loop over xml data

      MultiExceptionHandler mex = new MultiExceptionHandler();
      for( e.Data.Offset = e.Data.Count - 1; e.Data.Offset >= 0; e.Data.Offset-- )  // reverse, oldest-to-newest
      {
        try
        {
          // parse and validate attribute values

          int id = e.Data.GetValue<int>( "id" );
          e.CheckAttribute( "id", id, this.gameState.Countries );

          int nextCycle = e.Data.GetValue<int>( "cycle" );
          int goal = e.Data.GetValue<int>( "cur_cycle_goal" );
          int totalPointsThisCycle = e.Data.GetValue<int>( "cur_cycle_produced" );


          // skip rdp info during intermission

          if( goal > 50000 )  // upon victory rdp goals get set to 100000
          {
            if( polling && this.gameState.Countries[id].RDPGoal <= 50000 )  // was enabled
              Log.AddEntry( "Disabling {0} RDP stats", this.gameState.Countries[id].Demonym );

            this.gameState.Countries[id].SetRDPState( -1, -1, 0 );
            continue;
          }


          // calculate the total RDP points produced so far by this country

          int totalPointsCampaign = 0;
          foreach( Factory fact in this.gameState.Factories.Values )
            if( fact.Country == this.gameState.Countries[id] )
              totalPointsCampaign += fact.TotalProduced;


          // subtract the points produced this cycle to get total for all prev cycles, and store this value

          int totalPointsPrevCycles = totalPointsCampaign - totalPointsThisCycle;

          /* We can now get the total points produced this cycle without polling rdp.xml again
           * by simply summing factory.TotalProduced to get totalPointsCampaign as above, and
           * subtracting country[id].RDPPrevCyclePoints.
           */


          // write log entries

          if( polling )
          {
            if( this.gameState.Countries[id].NextRDPCycle != nextCycle )
              Log.AddEntry( "Starting {0} RDP cycle {1}", this.gameState.Countries[id].Demonym, nextCycle );
            else if( this.gameState.Countries[id].RDPGoal != goal )
              Log.AddEntry( "Updating {0} RDP goal: {1} => {2}", this.gameState.Countries[id].Demonym, this.gameState.Countries[id].RDPGoal, goal );
          }


          // record drift

          if( this.gameState.Countries[id].RDPPrevCyclePoints != totalPointsPrevCycles )
          {
            int drift = this.gameState.Countries[id].RDPPrevCyclePoints - totalPointsPrevCycles;
            switch( this.gameState.Countries[id].Abbr )
            {
              case "UK": driftUK = drift; break;
              case "FR": driftFR = drift; break;
              case "DE": driftDE = drift; break;
            }
          }


          // update state

          if( this.gameState.Countries[id].NextRDPCycle != nextCycle || this.gameState.Countries[id].RDPGoal != goal || this.gameState.Countries[id].RDPPrevCyclePoints != totalPointsPrevCycles )
          {
            this.gameState.Countries[id].SetRDPState( nextCycle - 1, goal, totalPointsPrevCycles );
            newDataCount++;
          }

        }
        catch( Exception ex ) { mex.Add( ex ); }

      }  // for e.Data


      // log any recorded drift

      try
      {
        if( polling && ( driftUK != 0 || driftFR != 0 || driftDE != 0 ) && newDataCount == 0 )
        {
          StringBuilder logmsg = new StringBuilder( "Correcting RDP drift: " );
          if( driftUK != 0 ) logmsg.AppendFormat( "UK{0}{1}, ", driftUK > 0 ? "+" : null, driftUK );
          if( driftFR != 0 ) logmsg.AppendFormat( "FR{0}{1}, ", driftFR > 0 ? "+" : null, driftFR );
          if( driftDE != 0 ) logmsg.AppendFormat( "DE{0}{1}, ", driftDE > 0 ? "+" : null, driftDE );
          logmsg.Length -= 2;  // remove trailing ", "

          Log.AddEntry( logmsg.ToString() );  // eg, "Correcting RDP drift: UK+1, DE-5"
        }
      }
      catch( Exception ex ) { mex.Add( ex ); }


      // if polling, flag xmlfile as checked

      if( polling )
        this.pollRDP.FlagAsChecked( newDataCount );


      // if any errors, throw first

      mex.Throw();
    }

    #endregion

    #endregion
  }
}
