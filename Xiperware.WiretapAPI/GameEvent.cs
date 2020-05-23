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
using System.Drawing;
using Xiperware.WiretapAPI.Properties;

namespace Xiperware.WiretapAPI
{
  #region Enums

  /// <summary>
  /// The category of GameEvent.
  /// </summary>
  public enum GameEventType
  {
    Capture,
    AttackObjective,
    Firebase,
    HCUnit,
    Factory
  }

  #endregion

  #region Interfaces

  /// <summary>
  /// An interface for GameEvents that represent a facility capture.
  /// </summary>
  public interface ICaptureFacilityGameEvent
  {
    /// <summary>
    /// The Country that now owns the Facility.
    /// </summary>
    Country NewOwner
    {
      get;
    }
  }


  /// <summary>
  /// An interface for GameEvents that have a from/to country.
  /// </summary>
  public interface ICountryChangeGameEvent
  {
    /// <summary>
    /// The Country after the change.
    /// </summary>
    Country NewCountry
    {
      get;
    }

    /// <summary>
    /// The Country before the change.
    /// </summary>
    Country PrevCountry
    {
      get;
    }
  }


  /// <summary>
  /// An interface for hcunit-related GameEvents.
  /// </summary>
  public interface IHCUnitGameEvent
  {
    /// <summary>
    /// Gets the HCUnit associated with this HCUnitGameEvent.
    /// </summary>
    HCUnit HCUnit
    {
      get;
    }
  }


  /// <summary>
  /// An interface for factory-related GameEvents.
  /// </summary>
  public interface IFactoryGameEvent
  {
    /// <summary>
    /// Gets the Factory associated with this FactoryGameEvent.
    /// </summary>
    Factory Factory
    {
      get;
    }
  }

  #endregion

  #region Base class

  /// <summary>
  /// An abstract class that defines the common GameEvent properties that are then
  /// implemented by each event type.
  /// </summary>
  public abstract class GameEvent : IComparable
  {
    #region Variables

    private DateTime eventTime;  // either Local or (skew-correction-required) UTC
    private DateTime eventReceived;
    protected string title;
    protected string description;
    protected List<ChokePoint> chokepoints = new List<ChokePoint>();

    /// <summary>
    /// If true, the EventReceived timestamp will be set to the EventTime
    /// </summary>
    public static bool syncEventReceivedTime = false;

    #endregion

    #region Constructors

    /// <summary>
    /// Base GameEvent constructor, used by child classes.
    /// </summary>
    /// <param name="eventTime">The time the event occurred (UTC if from wiretap and skew-correction required).</param>
    protected GameEvent( DateTime eventTime )
    {
      this.eventTime = eventTime;
      this.eventReceived = syncEventReceivedTime ? DateTime.MinValue : DateTime.Now;
    }

    #endregion

    #region Properties

    /// <summary>
    /// The time the event occurred, in the users local time (skew corrected).
    /// </summary>
    public DateTime EventTime
    {
      get
      {
        if( this.eventTime.Kind == DateTimeKind.Local )
          return this.eventTime;
        else
          return this.eventTime.ToLocalTime().AddSeconds( Misc.ClockSkew.CurrentSkewSeconds );
      }
    }

    /// <summary>
    /// The time the event was received, in the users local time.
    /// </summary>
    /// <remarks>As some of the xml feeds present delayed views of game data,
    /// this may be later than the event time.</remarks>
    public DateTime EventReceived
    {
      get
      {
        if( this.eventReceived == DateTime.MinValue )  // synced to event time
          return this.EventTime;
        else
          return this.eventReceived;
      }
    }

    /// <summary>
    /// The event category this GameEvent belongs to.
    /// </summary>
    public abstract GameEventType Type
    {
      get;
    }

    /// <summary>
    /// The short event title to display to the user.
    /// </summary>
    public abstract string Title
    {
      get;
    }

    /// <summary>
    /// The long description of the event to display to the user.
    /// </summary>
    public abstract string Description
    {
      get;
    }

    /// <summary>
    /// One or more ChokePoints associated with this event.
    /// </summary>
    public abstract List<ChokePoint> ChokePoints
    {
      get;
    }

    /// <summary>
    /// The Country associated with this event.
    /// </summary>
    public abstract Country Country
    {
      get;
    }

    /// <summary>
    /// The icon associated with this type (and instance) of GameEvent.
    /// </summary>
    public abstract Image Icon
    {
      get;
    }

    /// <summary>
    /// The icon associated with this type (and instance) of GameEvent to display
    /// in the pop-up alert window.
    /// </summary>
    public abstract Image AlertIcon
    {
      get;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Bump the GameEvent's event time up a millisecond.
    /// </summary>
    /// <remarks>Used to make it unique so duplicate times can be added to a sorted list.</remarks>
    /// <returns>The new DateTime (local, corrected).</returns>
    public DateTime IncrementEventTime()
    {
      this.eventTime = this.eventTime.AddMilliseconds( 1 );
      return this.EventTime;
    }

    /// <summary>
    /// Provides a string representation of this object.
    /// </summary>
    /// <returns>The event title.</returns>
    public override string ToString()
    {
      return this.title;
    }

    /// <summary>
    /// Compare this object with another GameEvent object.
    /// </summary>
    /// <remarks>Not generic so we can also compare subclasses of GameEvent.</remarks>
    /// <param name="obj">The other GameEvent to compare with.</param>
    /// <returns>-1, 0, 1</returns>
    public int CompareTo( object obj )
    {
      if( obj is GameEvent )
        return this.EventTime.CompareTo( ( (GameEvent)obj ).EventTime );
      else
        throw new ArgumentException( "Object is not a GameEvent" );
    }

    /// <summary>
    /// Remove any circular references to allow garbage collection.
    /// </summary>
    public abstract void CleanUp();

    #endregion
  }

  #endregion

  #region Sub classes

  /// <summary>
  /// Occurs when a player has captured a Facility.
  /// </summary>
  public class FacilityCapturedGameEvent : GameEvent, ICaptureFacilityGameEvent, ICountryChangeGameEvent
  {
    #region Variables

    private Facility facility;
    private Country newOwner;
    private Country prevOwner;
    private string by;
    private bool recaptured;

    #endregion

    #region Constructors

    /// <summary>
    /// Create a new FacilityCapturedGameEvent.
    /// </summary>
    /// <param name="eventTime">The time the event occurred.</param>
    /// <param name="facility">The Facility that was captured.</param>
    /// <param name="newOwner">The Country that now owns the Facility.</param>
    /// <param name="prevOwner">The Country that previously owned the Facility.</param>
    /// <param name="by">The name of the player that performed the capture.</param>
    public FacilityCapturedGameEvent( DateTime eventTime, Facility facility, Country newOwner, Country prevOwner, string by )
      : base( eventTime.ToUniversalTime() )
    {
      this.facility = facility;
      this.newOwner = newOwner;
      this.prevOwner = prevOwner;
      this.by = by;
      this.recaptured = newOwner.Side == facility.ChokePoint.Owner.Side;

      string strFaclilty = facility.ToString();
      if( !facility.Name.Contains( facility.ChokePoint.Name ) )
        strFaclilty += " " + String.Format( Language.Event_Misc_InChokePoint, facility.ChokePoint.Name );

      if( this.recaptured )
      {
        this.title = String.Format( Language.Event_Title_FacilityRecaptured, facility );
        this.description = String.Format( Language.Event_Desc_FacilityRecaptured, strFaclilty, newOwner.Demonym, by );
      }
      else
      {
        this.title = String.Format( Language.Event_Title_FacilityCaptured, facility );
        this.description = String.Format( Language.Event_Desc_FacilityCaptured, strFaclilty, newOwner.Demonym, by );
      }

      this.chokepoints.Add( facility.ChokePoint );
    }

    #endregion

    #region Properties

    /// <summary>
    /// The Facility that was captured. 
    /// </summary>
    public Facility Facility
    {
      get { return this.facility; }
    }

    /// <summary>
    /// The Country that now owns the Facility.
    /// </summary>
    public Country NewOwner
    {
      get { return this.newOwner; }
    }

    /// <summary>
    /// The Country that previously owned the Facility.
    /// </summary>
    public Country PrevOwner
    {
      get { return this.prevOwner; }
    }

    /// <summary>
    /// The name of the player that performed the capture.
    /// </summary>
    public string CapturedBy
    {
      get { return this.by; }
    }

    /// <summary>
    /// True if this Facility now belongs to the ChokePoint owners side.
    /// </summary>
    public bool Recaptured
    {
      get { return this.recaptured; }
    }

    /// <summary>
    /// The Country after the change.
    /// </summary>
    public Country NewCountry
    {
      get { return this.NewOwner; }
    }

    /// <summary>
    /// The Country before the change.
    /// </summary>
    public Country PrevCountry
    {
      get { return this.PrevOwner; }
    }

    /// <summary>
    /// The event category this GameEvent belongs to.
    /// </summary>
    public override GameEventType Type
    {
      get { return GameEventType.Capture; }
    }

    /// <summary>
    /// The short event title to display to the user.
    /// </summary>
    public override string Title
    {
      get { return this.title; }
    }

    /// <summary>
    /// The long description of the event to display to the user. 
    /// </summary>
    public override string Description
    {
      get { return this.description; }
    }

    /// <summary>
    /// One or more ChokePoints associated with this event.
    /// </summary>
    public override List<ChokePoint> ChokePoints
    {
      get { return this.chokepoints; }
    }

    /// <summary>
    /// The Country associated with this event.
    /// </summary>
    public override Country Country
    {
      get { return this.newOwner; }
    }

    /// <summary>
    /// The icon associated with this type (and instance) of GameEvent.
    /// </summary>
    public override Image Icon
    {
      get
      {
        if( this.newOwner.Abbr == "UK" )
          return Resources.event_facilitycapture_british;
        else if( this.newOwner.Abbr == "US" )
          return Resources.event_facilitycapture_usa;
        else if( this.newOwner.Abbr == "FR" )
          return Resources.event_facilitycapture_french;
        else if( this.newOwner.Abbr == "DE" )
          return Resources.event_facilitycapture_german;
        else
          return new Bitmap( 1, 1 );  // no image
      }
    }

    /// <summary>
    /// The icon associated with this type (and instance) of GameEvent to display
    /// in the pop-up alert window.
    /// </summary>
    public override Image AlertIcon
    {
      get { return this.facility.Icon; }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Remove any circular references to allow garbage collection.
    /// </summary>
    public override void CleanUp()
    {
      this.facility = null;
      this.newOwner = null;
      this.prevOwner = null;
    }

    #endregion
  }


  /// <summary>
  /// Occurs when the enemy has captured a spawnable Depot.
  /// </summary>
  public class SpawnableDepotCapturedGameEvent : GameEvent, ICaptureFacilityGameEvent, ICountryChangeGameEvent
  {
    #region Variables

    private Depot depot;
    private Country newOwner;
    private Country prevOwner;
    private string by;
    private bool recaptured;

    #endregion

    #region Constructors

    /// <summary>
    /// Create a new SpawnableDepotCapturedGameEvent.
    /// </summary>
    /// <param name="eventTime">The time the event occurred.</param>
    /// <param name="depot">The Depot that was captured.</param>
    /// <param name="newOwner">The Country that now owns the Depot.</param>
    /// <param name="prevOwner">The Country that previously owned the Depot.</param>
    /// <param name="by">The name of the player that performed the capture.</param>
    public SpawnableDepotCapturedGameEvent( DateTime eventTime, Depot depot, Country newOwner, Country prevOwner, string by )
      : base( eventTime.ToUniversalTime() )
    {
      this.depot = depot;
      this.newOwner = newOwner;
      this.prevOwner = prevOwner;
      this.by = by;
      this.recaptured = newOwner.Side == depot.ChokePoint.Owner.Side;

      if( this.recaptured )
      {
        this.title = String.Format( Language.Event_Title_SpawnableRecaptured, depot );
        this.description = String.Format( Language.Event_Desc_SpawnableRecaptured,
                                          depot.LinkedDepot.ChokePoint,
                                          depot.ChokePoint,
                                          newOwner.Demonym,
                                          by );
      }
      else
      {
        this.title = String.Format( Language.Event_Title_SpawnableCaptured, depot );
        this.description = String.Format( Language.Event_Desc_SpawnableCaptured,
                                          depot.LinkedDepot.ChokePoint,
                                          depot.ChokePoint,
                                          newOwner.Demonym,
                                          by );
      }

      this.chokepoints.Add( depot.ChokePoint );
    }

    #endregion

    #region Properties

    /// <summary>
    /// The Facility that was captured. 
    /// </summary>
    public Depot Depot
    {
      get { return this.depot; }
    }

    /// <summary>
    /// The Country that now owns the Facility.
    /// </summary>
    public Country NewOwner
    {
      get { return this.newOwner; }
    }

    /// <summary>
    /// The Country that previously owned the Facility.
    /// </summary>
    public Country PrevOwner
    {
      get { return this.prevOwner; }
    }

    /// <summary>
    /// The name of the player that performed the capture.
    /// </summary>
    public string CapturedBy
    {
      get { return this.by; }
    }

    /// <summary>
    /// True if this Facility now belongs to the ChokePoint owners side.
    /// </summary>
    public bool Recaptured
    {
      get { return this.recaptured; }
    }

    /// <summary>
    /// The Country after the change.
    /// </summary>
    public Country NewCountry
    {
      get { return this.NewOwner; }
    }

    /// <summary>
    /// The Country before the change.
    /// </summary>
    public Country PrevCountry
    {
      get { return this.PrevOwner; }
    }

    /// <summary>
    /// The event category this GameEvent belongs to.
    /// </summary>
    public override GameEventType Type
    {
      get { return GameEventType.Capture; }
    }

    /// <summary>
    /// The short event title to display to the user.
    /// </summary>
    public override string Title
    {
      get { return this.title; }
    }

    /// <summary>
    /// The long description of the event to display to the user. 
    /// </summary>
    public override string Description
    {
      get { return this.description; }
    }

    /// <summary>
    /// One or more ChokePoints associated with this event.
    /// </summary>
    public override List<ChokePoint> ChokePoints
    {
      get { return this.chokepoints; }
    }

    /// <summary>
    /// The Country associated with this event.
    /// </summary>
    public override Country Country
    {
      get { return this.newOwner; }
    }

    /// <summary>
    /// The icon associated with this type (and instance) of GameEvent.
    /// </summary>
    public override Image Icon
    {
      get
      {
        if( this.recaptured )
        {
          if( this.newOwner.Abbr == "UK" )
            return Resources.event_facilitycapture_british;
          else if( this.newOwner.Abbr == "US" )
            return Resources.event_facilitycapture_usa;
          else if( this.newOwner.Abbr == "FR" )
            return Resources.event_facilitycapture_french;
          else if( this.newOwner.Abbr == "DE" )
            return Resources.event_facilitycapture_german;
          else
            return new Bitmap( 1, 1 );  // no image
        }
        else // captured
        {
          if( this.newOwner.Abbr == "UK" )
            return Resources.event_spawnablecapture_british;
          else if( this.newOwner.Abbr == "US" )
            return Resources.event_spawnablecapture_usa;
          else if( this.newOwner.Abbr == "FR" )
            return Resources.event_spawnablecapture_french;
          else if( this.newOwner.Abbr == "DE" )
            return Resources.event_spawnablecapture_german;
          else
            return new Bitmap( 1, 1 );  // no image
        }
      }
    }

    /// <summary>
    /// The icon associated with this type (and instance) of GameEvent to display
    /// in the pop-up alert window.
    /// </summary>
    public override Image AlertIcon
    {
      get
      {
        if( this.recaptured )
          return Resources.facility_depot;
        else
          return Resources.event_spawnablecapture;
      }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Remove any circular references to allow garbage collection.
    /// </summary>
    public override void CleanUp()
    {
      this.depot = null;
      this.newOwner = null;
      this.prevOwner = null;
    }

    #endregion
  }


  /// <summary>
  /// Occurs when all Facilitys in a ChokePoint have been captured.
  /// </summary>
  public class ChokePointCapturedGameEvent : GameEvent, ICountryChangeGameEvent
  {
    #region Variables

    private ChokePoint cp;
    private Country newOwner;
    private Country prevOwner;

    #endregion

    #region Constructors

    /// <summary>
    /// Create a new ChokePointCapturedGameEvent.
    /// </summary>
    /// <param name="eventTime">The time the event occurred.</param>
    /// <param name="cp">The ChokePoint that was captured.</param>
    /// <param name="newOwner">The Country that now owns the ChokePoint.</param>
    /// <param name="prevOwner">The Country that previously owned the ChokePoint.</param>
    /// <param name="attackStarted">The DateTime the ChokePoint was initially contested (in local time).</param>
    public ChokePointCapturedGameEvent( DateTime eventTime, ChokePoint cp, Country newOwner, Country prevOwner, DateTime attackStarted )
      : base( eventTime.ToUniversalTime() )
    {
      this.cp = cp;
      this.newOwner = newOwner;
      this.prevOwner = prevOwner;

      this.title = String.Format( Language.Event_Title_ChokePointCaptured, cp );
      if( attackStarted > DateTime.Now.AddDays( -1 ) && attackStarted < this.EventTime.AddMinutes( -10 ) )  // longer than 10 minutes
        this.description = String.Format( Language.Event_Desc_ChokePointCapturedAfter,
                                          cp, newOwner, Misc.MinsAgoShort( attackStarted, this.EventTime ) );
      else
        this.description = String.Format( Language.Event_Desc_ChokePointCaptured, cp, newOwner );

      this.chokepoints.Add( cp );
    }

    #endregion

    #region Properties

    /// <summary>
    /// The ChokePoint that was captured.
    /// </summary>
    public ChokePoint ChokePoint
    {
      get { return this.cp; }
    }

    /// <summary>
    /// The Country that now owns the ChokePoint.
    /// </summary>
    public Country NewOwner
    {
      get { return this.newOwner; }
    }

    /// <summary>
    /// The Country that previously owned the ChokePoint.
    /// </summary>
    public Country PrevOwner
    {
      get { return this.prevOwner; }
    }

    /// <summary>
    /// The Country after the change.
    /// </summary>
    public Country NewCountry
    {
      get { return this.NewOwner; }
    }

    /// <summary>
    /// The Country before the change.
    /// </summary>
    public Country PrevCountry
    {
      get { return this.PrevOwner; }
    }

    /// <summary>
    /// The event category this GameEvent belongs to.
    /// </summary>
    public override GameEventType Type
    {
      get { return GameEventType.Capture; }
    }

    /// <summary>
    /// The short event title to display to the user.
    /// </summary>
    public override string Title
    {
      get { return this.title; }
    }

    /// <summary>
    /// The long description of the event to display to the user. 
    /// </summary>
    public override string Description
    {
      get { return this.description; }
    }

    /// <summary>
    /// One or more ChokePoints associated with this event.
    /// </summary>
    public override List<ChokePoint> ChokePoints
    {
      get { return this.chokepoints; }
    }

    /// <summary>
    /// The Country associated with this event.
    /// </summary>
    public override Country Country
    {
      get { return this.newOwner; }
    }

    /// <summary>
    /// The icon associated with this type (and instance) of GameEvent.
    /// </summary>
    public override Image Icon
    {
      get
      {
        switch( this.newOwner.Abbr )
        {
          case "UK":
            return Resources.event_chokepointcapture_british;
          case "US":
            return Resources.event_chokepointcapture_usa;
          case "FR":
            return Resources.event_chokepointcapture_french;
          case "DE":
            return Resources.event_chokepointcapture_german;
          default:
            return null;
        }
      }
    }

    /// <summary>
    /// The icon associated with this type (and instance) of GameEvent to display
    /// in the pop-up alert window.
    /// </summary>
    public override Image AlertIcon
    {
      get { return this.Icon; }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Remove any circular references to allow garbage collection.
    /// </summary>
    public override void CleanUp()
    {
      this.cp = null;
      this.newOwner = null;
      this.prevOwner = null;
    }

    #endregion
  }


  /// <summary>
  /// Occurs when a ChokePoint becomes contested due to a Facility being captured.
  /// </summary>
  public class ChokePointUnderAttackGameEvent : GameEvent
  {
    #region Variables

    private ChokePoint cp;
    private bool newAttack;
    private Country country;

    #endregion

    #region Constructors

    /// <summary>
    /// Create a new ChokePointUnderAttackGameEvent.
    /// </summary>
    /// <param name="eventTime">The time the event occurred.</param>
    /// <param name="cp">The ChokePoint that is under attack.</param>
    public ChokePointUnderAttackGameEvent( DateTime eventTime, ChokePoint cp )
      : base( eventTime.ToUniversalTime() )
    {
      this.cp = cp;
      this.newAttack = !cp.ContestedRecently;
      this.country = cp.Owner;

      this.title = String.Format( Language.Event_Title_ChokePointUnderAttack, cp );
      if( newAttack )
        this.description = String.Format( Language.Event_Desc_ChokePointUnderAttack, cp, Misc.EnumString( Misc.OtherSide( cp.Owner.Side ) ) );
      else
        this.description = String.Format( Language.Event_Desc_ChokePointUnderAttackAgain, cp, Misc.EnumString( Misc.OtherSide( cp.Owner.Side ) ) );
      this.chokepoints.Add( cp );
    }

    #endregion

    #region Properties

    /// <summary>
    /// The ChokePoint that is under attack.
    /// </summary>
    public ChokePoint ChokePoint
    {
      get { return this.cp; }
    }

    /// <summary>
    /// Wether or not this ChokePoint has been attacked recently.
    /// </summary>
    public bool NewAttack
    {
      get { return this.newAttack; }
    }

    /// <summary>
    /// The event category this GameEvent belongs to.
    /// </summary>
    public override GameEventType Type
    {
      get { return GameEventType.Capture; }
    }

    /// <summary>
    /// The short event title to display to the user.
    /// </summary>
    public override string Title
    {
      get { return this.title; }
    }

    /// <summary>
    /// The long description of the event to display to the user. 
    /// </summary>
    public override string Description
    {
      get { return this.description; }
    }

    /// <summary>
    /// One or more ChokePoints associated with this event.
    /// </summary>
    public override List<ChokePoint> ChokePoints
    {
      get { return this.chokepoints; }
    }

    /// <summary>
    /// The Country associated with this event.
    /// </summary>
    public override Country Country
    {
      get { return this.country; }
    }

    /// <summary>
    /// The icon associated with this type (and instance) of GameEvent.
    /// </summary>
    public override Image Icon
    {
      get { return Resources.event_underattack; }
    }

    /// <summary>
    /// The icon associated with this type (and instance) of GameEvent to display
    /// in the pop-up alert window.
    /// </summary>
    public override Image AlertIcon
    {
      get
      {
        switch( this.country.Abbr )
        {
          case "UK":
            return Resources.event_underattack_british;
          case "US":
            return Resources.event_underattack_usa;
          case "FR":
            return Resources.event_underattack_french;
          case "DE":
            return Resources.event_underattack_german;
          default:
            return null;
        }
      }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Remove any circular references to allow garbage collection.
    /// </summary>
    public override void CleanUp()
    {
      this.cp = null;
      this.country = null;
    }

    #endregion
  }


  /// <summary>
  /// Occurs when a ChokePoint becomes uncontested when the last enemy Facility is recaptured.
  /// </summary>
  public class ChokePointRegainedGameEvent : GameEvent
  {
    #region Variables

    private ChokePoint cp;
    private Country country;

    #endregion

    #region Constructors

    /// <summary>
    /// Create a new ChokePointRegainedGameEvent.
    /// </summary>
    /// <param name="eventTime">The time the event occurred.</param>
    /// <param name="cp">The ChokePoint that has been regained.</param>
    /// <param name="attackStarted">The DateTime the ChokePoint was initially contested (in local time).</param>
    public ChokePointRegainedGameEvent( DateTime eventTime, ChokePoint cp, DateTime attackStarted )
      : base( eventTime.ToUniversalTime() )
    {
      this.cp = cp;
      this.country = cp.Owner;
      this.title = String.Format( Language.Event_Title_ChokePointRegained, cp );

      if( attackStarted > DateTime.Now.AddDays( -1 ) && attackStarted < this.EventTime.AddMinutes( -15 ) )  // longer than 15 minutes
        this.description = String.Format( Language.Event_Desc_ChokePointRegainedAfter,
                                          cp, Misc.EnumString( cp.Owner.Side ), Misc.MinsAgoShort( attackStarted, this.EventTime ) );
      else
        this.description = String.Format( Language.Event_Desc_ChokePointRegained, cp, Misc.EnumString( cp.Owner.Side ) );

      this.chokepoints.Add( cp );
    }

    #endregion

    #region Properties

    /// <summary>
    /// The ChokePoint that has been regained.
    /// </summary>
    public ChokePoint ChokePoint
    {
      get { return this.cp; }
    }

    /// <summary>
    /// The event category this GameEvent belongs to.
    /// </summary>
    public override GameEventType Type
    {
      get { return GameEventType.Capture; }
    }

    /// <summary>
    /// The short event title to display to the user.
    /// </summary>
    public override string Title
    {
      get { return this.title; }
    }

    /// <summary>
    /// The long description of the event to display to the user. 
    /// </summary>
    public override string Description
    {
      get { return this.description; }
    }

    /// <summary>
    /// One or more ChokePoints associated with this event.
    /// </summary>
    public override List<ChokePoint> ChokePoints
    {
      get { return this.chokepoints; }
    }

    /// <summary>
    /// The Country associated with this event.
    /// </summary>
    public override Country Country
    {
      get { return this.country; }
    }

    /// <summary>
    /// The icon associated with this type (and instance) of GameEvent.
    /// </summary>
    public override Image Icon
    {
      get { return Resources.event_regained; }
    }

    /// <summary>
    /// The icon associated with this type (and instance) of GameEvent to display
    /// in the pop-up alert window.
    /// </summary>
    public override Image AlertIcon
    {
      get
      {
        switch( this.country.Abbr )
        {
          case "UK":
            return Resources.event_regained_british;
          case "US":
            return Resources.event_regained_usa;
          case "FR":
            return Resources.event_regained_french;
          case "DE":
            return Resources.event_regained_german;
          default:
            return null;
        }
      }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Remove any circular references to allow garbage collection.
    /// </summary>
    public override void CleanUp()
    {
      this.cp = null;
      this.country = null;
    }

    #endregion
  }


  /// <summary>
  /// Occurs when control of the ChokePoint has been lost or regained.
  /// </summary>
  /// <remarks>Doesn't occur upon ChokePoint capture, or in single-ab ChokePoints.</remarks>
  public class ChokePointControllerChangedGameEvent : GameEvent, ICountryChangeGameEvent
  {
    #region Variables

    private ChokePoint cp;
    private Country newController;
    private Country prevController;

    #endregion

    #region Constructors

    /// <summary>
    /// Create a new ChokePointControllerChangedGameEvent.
    /// </summary>
    /// <param name="eventTime">The time the event occurred.</param>
    /// <param name="cp">The ChokePoint that has had control changed.</param>
    /// <param name="newController">The Country that now controls the ChokePoint.</param>
    /// <param name="prevController">The Country that previously controlled the ChokePoint.</param>
    public ChokePointControllerChangedGameEvent( DateTime eventTime, ChokePoint cp, Country newController, Country prevController )
      : base( eventTime.ToUniversalTime() )
    {
      this.cp = cp;
      this.newController = newController;
      this.prevController = prevController;

      this.title = String.Format( Language.Event_Title_ChokePointControlChanged, cp );
      if( newController.Side == cp.Owner.Side )
        this.description = String.Format( Language.Event_Desc_ChokePointControlRegained, Misc.EnumString( newController.Side ), cp );
      else
        this.description = String.Format( Language.Event_Desc_ChokePointControlGained, Misc.EnumString( newController.Side ), cp );
      this.chokepoints.Add( cp );
    }

    #endregion

    #region Properties

    /// <summary>
    /// The ChokePoint that has had control changed.
    /// </summary>
    public ChokePoint ChokePoint
    {
      get { return this.cp; }
    }

    /// <summary>
    /// The Country that now controls the ChokePoint.
    /// </summary>
    public Country NewController
    {
      get { return this.newController; }
    }

    /// <summary>
    /// The Country that previously controlled the ChokePoint.
    /// </summary>
    public Country PrevController
    {
      get { return this.prevController; }
    }

    /// <summary>
    /// The Country after the change.
    /// </summary>
    public Country NewCountry
    {
      get { return this.NewController; }
    }

    /// <summary>
    /// The Country before the change.
    /// </summary>
    public Country PrevCountry
    {
      get { return this.PrevController; }
    }

    /// <summary>
    /// The event category this GameEvent belongs to.
    /// </summary>
    public override GameEventType Type
    {
      get { return GameEventType.Capture; }
    }

    /// <summary>
    /// The short event title to display to the user.
    /// </summary>
    public override string Title
    {
      get { return this.title; }
    }

    /// <summary>
    /// The long description of the event to display to the user. 
    /// </summary>
    public override string Description
    {
      get { return this.description; }
    }

    /// <summary>
    /// One or more ChokePoints associated with this event.
    /// </summary>
    public override List<ChokePoint> ChokePoints
    {
      get { return this.chokepoints; }
    }

    /// <summary>
    /// The Country associated with this event.
    /// </summary>
    public override Country Country
    {
      get { return this.newController; }
    }

    /// <summary>
    /// The icon associated with this type (and instance) of GameEvent.
    /// </summary>
    public override Image Icon
    {
      get { return Resources.event_controlchanged; }
    }

    /// <summary>
    /// The icon associated with this type (and instance) of GameEvent to display
    /// in the pop-up alert window.
    /// </summary>
    public override Image AlertIcon
    {
      get
      {
        switch( this.newController.Abbr )
        {
          case "UK":
            return Resources.event_controlchanged_british;
          case "US":
            return Resources.event_controlchanged_usa;
          case "FR":
            return Resources.event_controlchanged_french;
          case "DE":
            return Resources.event_controlchanged_german;
          default:
            return null;
        }
      }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Remove any circular references to allow garbage collection.
    /// </summary>
    public override void CleanUp()
    {
      this.cp = null;
      this.newController = null;
      this.prevController = null;
    }

    #endregion
  }


  /// <summary>
  /// Occurs when an Attack Objective has been placed or withdrawn on a ChokePoint.
  /// </summary>
  public class AttackObjectiveChangedGameEvent : GameEvent
  {
    #region Variables

    private ChokePoint cp;
    private Country country;
    private bool ao;
    private HCUnit aoHCUnit;

    #endregion

    #region Constructors

    /// <summary>
    /// Create a new AttackObjectiveChangedGameEvent.
    /// </summary>
    /// <param name="eventTime">The time the event occurred.</param>
    /// <param name="cp">The ChokePoint the Attack Objective was changed on.</param>
    /// <param name="aoHCUnit">The HCUnit that placed the AO, or null if the AO was withdrawn.</param>
    public AttackObjectiveChangedGameEvent( DateTime eventTime, ChokePoint cp, HCUnit aoHCUnit )
      : base( eventTime )
    {
      this.cp = cp;
      this.country = cp.Owner;
      this.ao = aoHCUnit != null;
      this.aoHCUnit = aoHCUnit;

      if( ao )
      {
        this.title = String.Format( Language.Event_Title_AOPlaced, cp );
        if( aoHCUnit.DeployedChokePoint == null )
        {
          this.description = String.Format( Language.Event_Desc_AOPlaced,
                                            Misc.EnumString( aoHCUnit.Country.Side ),
                                            aoHCUnit,
                                            cp );
        }
        else
        {
          this.description = String.Format( Language.Event_Desc_AOPlacedDeployed,
                                            Misc.EnumString( aoHCUnit.Country.Side ),
                                            aoHCUnit,
                                            aoHCUnit.DeployedChokePoint,
                                            cp );
        }
      }
      else
      {
        this.title = String.Format( Language.Event_Title_AORecalled, cp );
        this.description = String.Format( Language.Event_Desc_AORecalled,
                                          Misc.EnumString( Misc.OtherSide( cp.Owner.Side ) ), cp );
      }
      this.chokepoints.Add( cp );
    }

    #endregion

    #region Properties

    /// <summary>
    /// The ChokePoint the Attack Objective was changed on.
    /// </summary>
    public ChokePoint ChokePoint
    {
      get { return this.cp; }
    }

    /// <summary>
    /// True if placed, false if withdrawn.
    /// </summary>
    public bool AOPlaced
    {
      get { return this.ao; }
    }

    /// <summary>
    /// The HCUnit that placed the AO (null if AO withdrawn).
    /// </summary>
    public HCUnit PlacedBy
    {
      get { return this.aoHCUnit; }
    }

    /// <summary>
    /// The event category this GameEvent belongs to.
    /// </summary>
    public override GameEventType Type
    {
      get { return GameEventType.AttackObjective; }
    }

    /// <summary>
    /// The short event title to display to the user.
    /// </summary>
    public override string Title
    {
      get { return this.title; }
    }

    /// <summary>
    /// The long description of the event to display to the user. 
    /// </summary>
    public override string Description
    {
      get { return this.description; }
    }

    /// <summary>
    /// One or more ChokePoints associated with this event.
    /// </summary>
    public override List<ChokePoint> ChokePoints
    {
      get { return this.chokepoints; }
    }

    /// <summary>
    /// The Country associated with this event.
    /// </summary>
    public override Country Country
    {
      get { return this.country; }
    }

    /// <summary>
    /// The icon associated with this type (and instance) of GameEvent.
    /// </summary>
    public override Image Icon
    {
      get
      {
        if( this.ao )
          return Resources.event_ao_placed;
        else
          return Resources.event_ao_withdrawn;
      }
    }

    /// <summary>
    /// The icon associated with this type (and instance) of GameEvent to display
    /// in the pop-up alert window.
    /// </summary>
    public override Image AlertIcon
    {
      get
      {
        if( this.ao )
          return Resources.event_ao_placed_small;
        else
          return Resources.event_ao_withdrawn_small;
      }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Remove any circular references to allow garbage collection.
    /// </summary>
    public override void CleanUp()
    {
      this.cp = null;
      this.country = null;
      this.aoHCUnit = null;
    }

    #endregion
  }


  /// <summary>
  /// Occurs when a Firebase has been blown by the enemy.
  /// </summary>
  public class FirebaseBlownGameEvent : GameEvent, ICountryChangeGameEvent
  {
    #region Variables

    private Firebase fb;
    private Country newOwner;
    private Country prevOwner;

    #endregion

    #region Constructors

    /// <summary>
    /// Create a new FirebaseBlownGameEvent.
    /// </summary>
    /// <param name="eventTime">The time the event occurred.</param>
    /// <param name="fb">The firebase that is now closed.</param>
    public FirebaseBlownGameEvent( DateTime eventTime, Firebase fb )
      : base( eventTime )
    {
      this.fb = fb;
      this.newOwner = fb.Link.Owner;
      this.prevOwner = fb.Link.NonOwner;

      this.title = String.Format( Language.Event_Title_FirebaseBlown, fb );

      string format = null;
      switch( fb.Link.State )
      {
        case FirebaseState.Inactive:  // shouldn't happen
          break;

        case FirebaseState.Offensive:
          format = Language.Event_Desc_FirebaseBlownOffensive;
          break;

        case FirebaseState.Defensive:
          format = Language.Event_Desc_FirebaseBlownDefensive;
          break;

        case FirebaseState.Brigade:
          if( fb.LinkedChokePoint.Owner.Side != fb.Link.Side )  // enemy held brigade fb between 2 friendly cps: can't spawn
            format = Language.Event_Desc_FirebaseBlownBrigadeNonspawnable;
          else
            format = Language.Event_Desc_FirebaseBlownBrigade;
          break;

        default:
          throw new ArgumentOutOfRangeException();
      }
      if( format != null )
        this.description = String.Format( format, fb, fb.LinkedFirebase.Link.Owner.Demonym, fb.ChokePoint );

      this.chokepoints.Add( fb.ChokePoint );
      this.chokepoints.Add( fb.LinkedChokePoint );
    }

    #endregion

    #region Properties

    /// <summary>
    /// The Firebase that was blown and is now closed.
    /// </summary>
    public Firebase Firebase
    {
      get { return this.fb; }
    }

    /// <summary>
    /// The Country after the change.
    /// </summary>
    public Country NewCountry
    {
      get { return this.newOwner; }
    }

    /// <summary>
    /// The Country before the change.
    /// </summary>
    public Country PrevCountry
    {
      get { return this.prevOwner; }
    }

    /// <summary>
    /// The event category this GameEvent belongs to.
    /// </summary>
    public override GameEventType Type
    {
      get { return GameEventType.Firebase; }
    }

    /// <summary>
    /// The short event title to display to the user.
    /// </summary>
    public override string Title
    {
      get { return this.title; }
    }

    /// <summary>
    /// The long description of the event to display to the user. 
    /// </summary>
    public override string Description
    {
      get { return this.description; }
    }

    /// <summary>
    /// One or more ChokePoints associated with this event.
    /// </summary>
    public override List<ChokePoint> ChokePoints
    {
      get { return this.chokepoints; }
    }

    /// <summary>
    /// The Country associated with this event.
    /// </summary>
    public override Country Country
    {
      get { return this.newOwner; }
    }

    /// <summary>
    /// The icon associated with this type (and instance) of GameEvent.
    /// </summary>
    public override Image Icon
    {
      get { return Resources.event_firebase_blown; }
    }

    /// <summary>
    /// The icon associated with this type (and instance) of GameEvent to display
    /// in the pop-up alert window.
    /// </summary>
    public override Image AlertIcon
    {
      get { return this.Icon; }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Remove any circular references to allow garbage collection.
    /// </summary>
    public override void CleanUp()
    {
      this.fb = null;
      this.newOwner = null;
      this.prevOwner = null;
    }

    #endregion
  }

  /// <summary>
  /// Occurs when a brigade firebase becomes active.
  /// </summary>
  public class NewBrigadeFirebaseGameEvent : GameEvent
  {
    #region Variables

    private Firebase fb;
    private Country country;

    #endregion

    #region Constructors

    /// <summary>
    /// Create a new NewBrigadeFirebaseGameEvent.
    /// </summary>
    /// <param name="eventTime">The time the event occurred.</param>
    /// <param name="fb">The brigade firebase that is now open.</param>
    public NewBrigadeFirebaseGameEvent( DateTime eventTime, Firebase fb )
      : base( eventTime )
    {
      this.fb = fb;
      this.country = fb.Link.Owner;

      this.title = String.Format( Language.Event_Title_NewBrigadeFirebase, fb.LinkedChokePoint );
      this.description = String.Format( Language.Event_Desc_NewBrigadeFirebase,
                                        fb.Link.Owner,
                                        fb.ChokePoint,
                                        fb.LinkedChokePoint );

      this.chokepoints.Add( fb.LinkedChokePoint );
      this.chokepoints.Add( fb.ChokePoint );
    }

    #endregion

    #region Properties

    /// <summary>
    /// The Firebase that was blown and is now closed.
    /// </summary>
    public Firebase Firebase
    {
      get { return this.fb; }
    }

    /// <summary>
    /// The event category this GameEvent belongs to.
    /// </summary>
    public override GameEventType Type
    {
      get { return GameEventType.Firebase; }
    }

    /// <summary>
    /// The short event title to display to the user.
    /// </summary>
    public override string Title
    {
      get { return this.title; }
    }

    /// <summary>
    /// The long description of the event to display to the user. 
    /// </summary>
    public override string Description
    {
      get { return this.description; }
    }

    /// <summary>
    /// One or more ChokePoints associated with this event.
    /// </summary>
    public override List<ChokePoint> ChokePoints
    {
      get { return this.chokepoints; }
    }

    /// <summary>
    /// The Country associated with this event.
    /// </summary>
    public override Country Country
    {
      get { return this.country; }
    }

    /// <summary>
    /// The icon associated with this type (and instance) of GameEvent.
    /// </summary>
    public override Image Icon
    {
      get { return Resources.event_firebase_blown; }
    }

    /// <summary>
    /// The icon associated with this type (and instance) of GameEvent to display
    /// in the pop-up alert window.
    /// </summary>
    public override Image AlertIcon
    {
      get { return this.Icon; }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Remove any circular references to allow garbage collection.
    /// </summary>
    public override void CleanUp()
    {
      this.fb = null;
      this.country = null;
    }

    #endregion
  }


  /// <summary>
  /// Occurs when a HCUnit has been redeployed by a HC player.
  /// </summary>
  public class HCUnitMovedGameEvent : GameEvent, IHCUnitGameEvent
  {
    #region Variables

    private HCUnit hcunit;
    private ChokePoint from;
    private ChokePoint to;
    private string player;

    #endregion

    #region Constructors

    /// <summary>
    /// Create a new HCUnitMovedGameEvent.
    /// </summary>
    /// <param name="eventTime">The time the event occurred.</param>
    /// <param name="hcunit">The HCUnit that was redeployed.</param>
    /// <param name="from">The ChokePoint the unit was moved from.</param>
    /// <param name="to">The ChokePoint the unit was moved to.</param>
    /// <param name="player">The name of the player that ordered the move.</param>
    public HCUnitMovedGameEvent( DateTime eventTime, HCUnit hcunit, ChokePoint from, ChokePoint to, string player )
      : base( eventTime.ToUniversalTime() )
    {
      this.hcunit = hcunit;
      this.from = from;
      this.to = to;
      this.player = player;

      this.title = String.Format( Language.Event_Title_HCUnitMoved, hcunit.NickOrTitle );

      string unitName = hcunit.Title;
      if( hcunit.Level == HCUnitLevel.Division && !hcunit.Title.Contains( "Division" ) )
        unitName += " " + Language.Event_Misc_DivisionHQ;

      this.description = String.Format( Language.Event_Desc_HCUnitMoved, unitName, from, to, player );

      this.chokepoints.Add( to );
      this.chokepoints.Add( from );
    }

    #endregion

    #region Properties

    /// <summary>
    /// The HCUnit that was redeployed.
    /// </summary>
    public HCUnit HCUnit
    {
      get { return this.hcunit; }
    }

    /// <summary>
    /// The ChokePoint the unit was moved from.
    /// </summary>
    public ChokePoint MovedFrom
    {
      get { return this.from; }
    }

    /// <summary>
    /// The ChokePoint the unit was moved to.
    /// </summary>
    public ChokePoint MovedTo
    {
      get { return this.to; }
    }

    /// <summary>
    /// The name of the player that ordered the move.
    /// </summary>
    public string MovedBy
    {
      get { return this.player; }
    }

    /// <summary>
    /// The event category this GameEvent belongs to.
    /// </summary>
    public override GameEventType Type
    {
      get { return GameEventType.HCUnit; }
    }

    /// <summary>
    /// The short event title to display to the user.
    /// </summary>
    public override string Title
    {
      get { return this.title; }
    }

    /// <summary>
    /// The long description of the event to display to the user. 
    /// </summary>
    public override string Description
    {
      get { return this.description; }
    }

    /// <summary>
    /// One or more ChokePoints associated with this event.
    /// </summary>
    public override List<ChokePoint> ChokePoints
    {
      get { return this.chokepoints; }
    }

    /// <summary>
    /// The Country associated with this event.
    /// </summary>
    public override Country Country
    {
      get { return this.hcunit.Country; }
    }

    /// <summary>
    /// The icon associated with this type (and instance) of GameEvent.
    /// </summary>
    public override Image Icon
    {
      get
      {
        switch( this.hcunit.Country.Abbr )
        {
          case "UK":
            return Resources.event_hcunit_moved_british;
          case "US":
            return Resources.event_hcunit_moved_usa;
          case "FR":
            return Resources.event_hcunit_moved_french;
          case "DE":
            return Resources.event_hcunit_moved_german;
          default:
            return null;
        }
      }
    }

    /// <summary>
    /// The icon associated with this type (and instance) of GameEvent to display
    /// in the pop-up alert window.
    /// </summary>
    public override Image AlertIcon
    {
      get { return this.Icon; }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Remove any circular references to allow garbage collection.
    /// </summary>
    public override void CleanUp()
    {
      this.hcunit = null;
      this.from = null;
      this.to = null;
    }

    #endregion
  }


  /// <summary>
  /// Occurs when a HCUnit has been deployed from off-map.
  /// </summary>
  public class HCUnitDeployedGameEvent : GameEvent, IHCUnitGameEvent
  {
    #region Variables

    private HCUnit hcunit;
    private ChokePoint to;

    #endregion

    #region Constructors

    /// <summary>
    /// Create a new HCUnitDeployedGameEvent.
    /// </summary>
    /// <param name="eventTime">The time the event occurred.</param>
    /// <param name="hcunit">The HCUnit that was deployed.</param>
    /// <param name="to">The ChokePoint the unit was moved to.</param>
    public HCUnitDeployedGameEvent( DateTime eventTime, HCUnit hcunit, ChokePoint to )
      : base( eventTime )
    {
      this.hcunit = hcunit;
      this.to = to;

      this.title = String.Format( Language.Event_Title_HCUnitDeployed, hcunit.NickOrTitle );
      this.description = String.Format( Language.Event_Desc_HCUnitDeployed, hcunit, to );
      this.chokepoints.Add( to );
    }

    #endregion

    #region Properties

    /// <summary>
    /// The HCUnit that was deployed.
    /// </summary>
    public HCUnit HCUnit
    {
      get { return this.hcunit; }
    }

    /// <summary>
    /// The ChokePoint the unit was moved to.
    /// </summary>
    public ChokePoint MovedTo
    {
      get { return this.to; }
    }

    /// <summary>
    /// The event category this GameEvent belongs to.
    /// </summary>
    public override GameEventType Type
    {
      get { return GameEventType.HCUnit; }
    }

    /// <summary>
    /// The short event title to display to the user.
    /// </summary>
    public override string Title
    {
      get { return this.title; }
    }

    /// <summary>
    /// The long description of the event to display to the user. 
    /// </summary>
    public override string Description
    {
      get { return this.description; }
    }

    /// <summary>
    /// One or more ChokePoints associated with this event.
    /// </summary>
    public override List<ChokePoint> ChokePoints
    {
      get { return this.chokepoints; }
    }

    /// <summary>
    /// The Country associated with this event.
    /// </summary>
    public override Country Country
    {
      get { return this.hcunit.Country; }
    }

    /// <summary>
    /// The icon associated with this type (and instance) of GameEvent.
    /// </summary>
    public override Image Icon
    {
      get
      {
        switch( this.hcunit.Country.Abbr )
        {
          case "UK":
            return Resources.event_hcunit_deployed_british;
          case "US":
            return Resources.event_hcunit_deployed_usa;
          case "FR":
            return Resources.event_hcunit_deployed_french;
          case "DE":
            return Resources.event_hcunit_deployed_german;
          default:
            return null;
        }
      }
    }

    /// <summary>
    /// The icon associated with this type (and instance) of GameEvent to display
    /// in the pop-up alert window.
    /// </summary>
    public override Image AlertIcon
    {
      get { return this.Icon; }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Remove any circular references to allow garbage collection.
    /// </summary>
    public override void CleanUp()
    {
      this.hcunit = null;
      this.to = null;
    }

    #endregion
  }


  /// <summary>
  /// Occurs when a HCUnit has been kicked out of a ChokePoint when it has been captured
  /// by the enemy.
  /// </summary>
  public class HCUnitRetreatGameEvent : GameEvent, IHCUnitGameEvent
  {
    #region Variables

    private HCUnit hcunit;
    private ChokePoint from;
    private ChokePoint to;

    #endregion

    #region Constructors

    /// <summary>
    /// Create a new HCUnitRetreatGameEvent.
    /// </summary>
    /// <param name="eventTime">The time the event occurred.</param>
    /// <param name="hcunit">The HCUnit that has fallen back.</param>
    /// <param name="from">The ChokePoint the unit has moved from.</param>
    /// <param name="to">The ChokePoint the unit has moved to.</param>
    public HCUnitRetreatGameEvent( DateTime eventTime, HCUnit hcunit, ChokePoint from, ChokePoint to )
      : base( eventTime )
    {
      this.hcunit = hcunit;
      this.from = from;
      this.to = to;

      this.title = String.Format( Language.Event_Title_HCUnitRetreat, hcunit.NickOrTitle );

      string unitName = hcunit.Title;
      if( hcunit.Level == HCUnitLevel.Division && !hcunit.Title.Contains( "Division" ) )
        unitName += " " + Language.Event_Misc_DivisionHQ;

      this.description = String.Format( Language.Event_Desc_HCUnitRetreat, unitName, from, to );

      this.chokepoints.Add( to );
      this.chokepoints.Add( from );
    }

    #endregion

    #region Properties

    /// <summary>
    /// The HCUnit that has fallen back.
    /// </summary>
    public HCUnit HCUnit
    {
      get { return this.hcunit; }
    }

    /// <summary>
    /// The ChokePoint the unit has moved from.
    /// </summary>
    public ChokePoint MovedFrom
    {
      get { return this.from; }
    }

    /// <summary>
    /// The ChokePoint the unit has moved to.
    /// </summary>
    public ChokePoint MovedTo
    {
      get { return this.to; }
    }

    /// <summary>
    /// The event category this GameEvent belongs to.
    /// </summary>
    public override GameEventType Type
    {
      get { return GameEventType.HCUnit; }
    }

    /// <summary>
    /// The short event title to display to the user.
    /// </summary>
    public override string Title
    {
      get { return this.title; }
    }

    /// <summary>
    /// The long description of the event to display to the user. 
    /// </summary>
    public override string Description
    {
      get { return this.description; }
    }

    /// <summary>
    /// One or more ChokePoints associated with this event.
    /// </summary>
    public override List<ChokePoint> ChokePoints
    {
      get { return this.chokepoints; }
    }

    /// <summary>
    /// The Country associated with this event.
    /// </summary>
    public override Country Country
    {
      get { return this.hcunit.Country; }
    }

    /// <summary>
    /// The icon associated with this type (and instance) of GameEvent.
    /// </summary>
    public override Image Icon
    {
      get
      {
        switch( this.hcunit.Country.Abbr )
        {
          case "UK":
            return Resources.event_hcunit_moved_british;
          case "US":
            return Resources.event_hcunit_moved_usa;
          case "FR":
            return Resources.event_hcunit_moved_french;
          case "DE":
            return Resources.event_hcunit_moved_german;
          default:
            return null;
        }
      }
    }

    /// <summary>
    /// The icon associated with this type (and instance) of GameEvent to display
    /// in the pop-up alert window.
    /// </summary>
    public override Image AlertIcon
    {
      get { return this.Icon; }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Remove any circular references to allow garbage collection.
    /// </summary>
    public override void CleanUp()
    {
      this.hcunit = null;
      this.from = null;
      this.to = null;
    }

    #endregion
  }


  /// <summary>
  /// Occurs when a HCUnit has been routed to off-map.
  /// </summary>
  public class HCUnitRoutedGameEvent : GameEvent, IHCUnitGameEvent
  {
    #region Variables

    private HCUnit hcunit;
    private ChokePoint from;

    #endregion

    #region Constructors

    /// <summary>
    /// Create a new HCUnitRoutedGameEvent.
    /// </summary>
    /// <param name="eventTime">The time the event occurred.</param>
    /// <param name="hcunit">The HCUnit that was routed.</param>
    /// <param name="from">The ChokePoint the unit was moved from.</param>
    public HCUnitRoutedGameEvent( DateTime eventTime, HCUnit hcunit, ChokePoint from )
      : base( eventTime )
    {
      this.hcunit = hcunit;
      this.from = from;

      this.title = String.Format( Language.Event_Title_HCUnitRouted, hcunit.NickOrTitle );
      this.description = String.Format( Language.Event_Desc_HCUnitRouted, hcunit, from );
      this.chokepoints.Add( from );
    }

    #endregion

    #region Properties

    /// <summary>
    /// The HCUnit that was routed.
    /// </summary>
    public HCUnit HCUnit
    {
      get { return this.hcunit; }
    }

    /// <summary>
    /// The ChokePoint the unit was moved from.
    /// </summary>
    public ChokePoint MovedFrom
    {
      get { return this.from; }
    }

    /// <summary>
    /// The event category this GameEvent belongs to.
    /// </summary>
    public override GameEventType Type
    {
      get { return GameEventType.HCUnit; }
    }

    /// <summary>
    /// The short event title to display to the user.
    /// </summary>
    public override string Title
    {
      get { return this.title; }
    }

    /// <summary>
    /// The long description of the event to display to the user. 
    /// </summary>
    public override string Description
    {
      get { return this.description; }
    }

    /// <summary>
    /// One or more ChokePoints associated with this event.
    /// </summary>
    public override List<ChokePoint> ChokePoints
    {
      get { return this.chokepoints; }
    }

    /// <summary>
    /// The Country associated with this event.
    /// </summary>
    public override Country Country
    {
      get { return this.hcunit.Country; }
    }

    /// <summary>
    /// The icon associated with this type (and instance) of GameEvent.
    /// </summary>
    public override Image Icon
    {
      get
      {
        switch( this.hcunit.Country.Abbr )
        {
          case "UK":
            return Resources.event_hcunit_routed_british;
          case "US":
            return Resources.event_hcunit_routed_usa;
          case "FR":
            return Resources.event_hcunit_routed_french;
          case "DE":
            return Resources.event_hcunit_routed_german;
          default:
            return null;
        }
      }
    }

    /// <summary>
    /// The icon associated with this type (and instance) of GameEvent to display
    /// in the pop-up alert window.
    /// </summary>
    public override Image AlertIcon
    {
      get { return this.Icon; }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Remove any circular references to allow garbage collection.
    /// </summary>
    public override void CleanUp()
    {
      this.hcunit = null;
      this.from = null;
    }

    #endregion
  }


  /// <summary>
  /// Occurs when a Factory has taken 20% or more damage within 15 minutes.
  /// </summary>
  public class FactoryDamagedGameEvent : GameEvent, IFactoryGameEvent
  {
    #region Variables

    private Factory factory;
    private int was;
    private int now;

    #endregion

    #region Constructors

    /// <summary>
    /// Create a new FactoryDamagedGameEvent.
    /// </summary>
    /// <param name="eventTime">The time the event occurred.</param>
    /// <param name="factory">The Factory that was damaged.</param>
    /// <param name="was">The previous damage percent.</param>
    /// <param name="now">The current damage percent.</param>
    public FactoryDamagedGameEvent( DateTime eventTime, Factory factory, int was, int now )
      : base( eventTime.ToUniversalTime() )
    {
      this.factory = factory;
      this.was = was;
      this.now = now;

      this.title = String.Format( Language.Event_Title_FactoryDamaged, factory.ChokePoint.OriginalCountry.Demonym );
      this.description = String.Format( Language.Event_Desc_FactoryDamaged, factory, factory.ChokePoint, now - was, 100 - now );
    }

    #endregion

    #region Properties

    /// <summary>
    /// The Factory that was damaged.
    /// </summary>
    public Factory Factory
    {
      get { return this.factory; }
    }

    /// <summary>
    /// The previous damage percent.
    /// </summary>
    public int DamageWas
    {
      get { return this.was; }
    }

    /// <summary>
    /// The current damage percent.
    /// </summary>
    public int DamageNow
    {
      get { return this.now; }
    }

    /// <summary>
    /// The event category this GameEvent belongs to.
    /// </summary>
    public override GameEventType Type
    {
      get { return GameEventType.Factory; }
    }

    /// <summary>
    /// The short event title to display to the user.
    /// </summary>
    public override string Title
    {
      get { return this.title; }
    }

    /// <summary>
    /// The long description of the event to display to the user. 
    /// </summary>
    public override string Description
    {
      get { return this.description; }
    }

    /// <summary>
    /// One or more ChokePoints associated with this event.
    /// </summary>
    public override List<ChokePoint> ChokePoints
    {
      get { return this.chokepoints; }
    }

    /// <summary>
    /// The Country associated with this event.
    /// </summary>
    public override Country Country
    {
      get { return this.factory.Country; }
    }

    /// <summary>
    /// The icon associated with this type (and instance) of GameEvent.
    /// </summary>
    public override Image Icon
    {
      get { return Resources.event_factory_damaged; }
    }

    /// <summary>
    /// The icon associated with this type (and instance) of GameEvent to display
    /// in the pop-up alert window.
    /// </summary>
    public override Image AlertIcon
    {
      get { return this.Icon; }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Remove any circular references to allow garbage collection.
    /// </summary>
    public override void CleanUp()
    {
      this.factory = null;
    }

    #endregion
  }


  /// <summary>
  /// Occurs when a Factory has been repaired to 100% health, after having been below 80%.
  /// </summary>
  public class FactoryHealthyGameEvent : GameEvent, IFactoryGameEvent
  {
    #region Variables

    private Factory factory;

    #endregion

    #region Constructors

    /// <summary>
    /// Create a new FactoryHealthyGameEvent.
    /// </summary>
    /// <param name="eventTime">The time the event occurred.</param>
    /// <param name="factory">The Factory that was repaired.</param>
    public FactoryHealthyGameEvent( DateTime eventTime, Factory factory )
      : base( eventTime.ToUniversalTime() )
    {
      this.factory = factory;

      this.title = String.Format( Language.Event_Title_FactoryHealthy, factory.Country.Demonym );
      this.description = String.Format( Language.Event_Desc_FactoryHealthy, factory, factory.ChokePoint );
    }

    #endregion

    #region Properties

    /// <summary>
    /// The Factory that was repaired.
    /// </summary>
    public Factory Factory
    {
      get { return this.factory; }
    }

    /// <summary>
    /// The event category this GameEvent belongs to.
    /// </summary>
    public override GameEventType Type
    {
      get { return GameEventType.Factory; }
    }

    /// <summary>
    /// The short event title to display to the user.
    /// </summary>
    public override string Title
    {
      get { return this.title; }
    }

    /// <summary>
    /// The long description of the event to display to the user. 
    /// </summary>
    public override string Description
    {
      get { return this.description; }
    }

    /// <summary>
    /// One or more ChokePoints associated with this event.
    /// </summary>
    public override List<ChokePoint> ChokePoints
    {
      get { return this.chokepoints; }
    }

    /// <summary>
    /// The Country associated with this event.
    /// </summary>
    public override Country Country
    {
      get { return this.factory.Country; }
    }

    /// <summary>
    /// The icon associated with this type (and instance) of GameEvent.
    /// </summary>
    public override Image Icon
    {
      get { return Resources.event_factory_healthy; }
    }

    /// <summary>
    /// The icon associated with this type (and instance) of GameEvent to display
    /// in the pop-up alert window.
    /// </summary>
    public override Image AlertIcon
    {
      get { return this.Icon; }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Remove any circular references to allow garbage collection.
    /// </summary>
    public override void CleanUp()
    {
      this.factory = null;
    }

    #endregion
  }


  /// <summary>
  /// Occurs when a Factory has taken 100% damage.
  /// </summary>
  public class FactoryDestroyedGameEvent : GameEvent, IFactoryGameEvent
  {
    #region Variables

    private Factory factory;

    #endregion

    #region Constructors

    /// <summary>
    /// Create a new FactoryDestroyedGameEvent.
    /// </summary>
    /// <param name="eventTime">The time the event occurred.</param>
    /// <param name="factory">The Factory that was destroyed.</param>
    public FactoryDestroyedGameEvent( DateTime eventTime, Factory factory )
      : base( eventTime.ToUniversalTime() )
    {
      this.factory = factory;

      this.title = String.Format( Language.Event_Title_FactoryDestroyed, factory.Country.Demonym );
      this.description = String.Format( Language.Event_Desc_FactoryDestroyed, factory, factory.ChokePoint );
    }

    #endregion

    #region Properties

    /// <summary>
    /// The Factory that was destroyed.
    /// </summary>
    public Factory Factory
    {
      get { return this.factory; }
    }

    /// <summary>
    /// The event category this GameEvent belongs to.
    /// </summary>
    public override GameEventType Type
    {
      get { return GameEventType.Factory; }
    }

    /// <summary>
    /// The short event title to display to the user.
    /// </summary>
    public override string Title
    {
      get { return this.title; }
    }

    /// <summary>
    /// The long description of the event to display to the user. 
    /// </summary>
    public override string Description
    {
      get { return this.description; }
    }

    /// <summary>
    /// One or more ChokePoints associated with this event.
    /// </summary>
    public override List<ChokePoint> ChokePoints
    {
      get { return this.chokepoints; }
    }

    /// <summary>
    /// The Country associated with this event.
    /// </summary>
    public override Country Country
    {
      get { return this.factory.Country; }
    }

    /// <summary>
    /// The icon associated with this type (and instance) of GameEvent.
    /// </summary>
    public override Image Icon
    {
      get { return Resources.event_factory_destroyed; }
    }

    /// <summary>
    /// The icon associated with this type (and instance) of GameEvent to display
    /// in the pop-up alert window.
    /// </summary>
    public override Image AlertIcon
    {
      get { return this.Icon; }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Remove any circular references to allow garbage collection.
    /// </summary>
    public override void CleanUp()
    {
      this.factory = null;
    }

    #endregion
  }


  /// <summary>
  /// Occurs when a Factory has started or stopped production.
  /// </summary>
  public class FactoryRDPChangedGameEvent : GameEvent, IFactoryGameEvent
  {
    #region Variables

    private Factory factory;
    private bool rdp;

    #endregion

    #region Constructors

    /// <summary>
    /// Create a new FactoryRDPChangedGameEvent.
    /// </summary>
    /// <param name="eventTime">The time the event occurred.</param>
    /// <param name="factory">The Factory that has changed production.</param>
    /// <param name="rdp">True if started, false if stopped.</param>
    public FactoryRDPChangedGameEvent( DateTime eventTime, Factory factory, int rdp )
      : base( eventTime.ToUniversalTime() )
    {
      this.factory = factory;
      this.rdp = rdp == 1;

      if( this.rdp )
      {
        this.title = String.Format( Language.Event_Title_FactoryRDPStart, factory.Country.Demonym );
        this.description = String.Format( Language.Event_Desc_FactoryRDPStart, factory, factory.ChokePoint );
      }
      else
      {
        this.title = String.Format( Language.Event_Title_FactoryRDPStop, factory.Country.Demonym );
        if( factory.Country.Side != factory.Owner.Side )
          this.description = String.Format( Language.Event_Desc_FactoryRDPStopCaptured, factory, factory.ChokePoint );
        else
          this.description = String.Format( Language.Event_Desc_FactoryRDPStopDamaged, factory, factory.ChokePoint );
      }
    }

    #endregion

    #region Properties

    /// <summary>
    /// The Factory that has changed production.
    /// </summary>
    public Factory Factory
    {
      get { return this.factory; }
    }

    /// <summary>
    /// True if RDP resumed, false if halted.
    /// </summary>
    public bool Resumed
    {
      get { return this.rdp; }
    }

    /// <summary>
    /// The event category this GameEvent belongs to.
    /// </summary>
    public override GameEventType Type
    {
      get { return GameEventType.Factory; }
    }

    /// <summary>
    /// The short event title to display to the user.
    /// </summary>
    public override string Title
    {
      get { return this.title; }
    }

    /// <summary>
    /// The long description of the event to display to the user. 
    /// </summary>
    public override string Description
    {
      get { return this.description; }
    }

    /// <summary>
    /// One or more ChokePoints associated with this event.
    /// </summary>
    public override List<ChokePoint> ChokePoints
    {
      get { return this.chokepoints; }
    }

    /// <summary>
    /// The Country associated with this event.
    /// </summary>
    public override Country Country
    {
      get { return this.factory.Country; }
    }

    /// <summary>
    /// The icon associated with this type (and instance) of GameEvent.
    /// </summary>
    public override Image Icon
    {
      get { return this.rdp ? Resources.event_factory_rdpon : Resources.event_factory_rdpoff; }
    }

    /// <summary>
    /// The icon associated with this type (and instance) of GameEvent to display
    /// in the pop-up alert window.
    /// </summary>
    public override Image AlertIcon
    {
      get { return this.Icon; }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Remove any circular references to allow garbage collection.
    /// </summary>
    public override void CleanUp()
    {
      this.factory = null;
    }

    #endregion
  }

  #endregion

  #region Collection class

  /// <summary>
  /// A static class that encapsulates a collection of the most recent two hours of GameEvent's.
  /// </summary>
  public class GameEventCollection : IEnumerable<GameEvent>
  {
    #region Variables

    /// <summary>
    /// The maximum age of the event list, in hours.
    /// </summary>
    private const int MAX_AGE = 2;

    /// <summary>
    /// A sorted list of all GameEvents, where index 0 = oldest.
    /// </summary>
    private SortedList<DateTime, GameEvent> allEvents;

    /// <summary>
    /// A sorted list of new GameEvents from the most recent poll, where index 0 = oldest.
    /// </summary>
    private SortedList<DateTime, GameEvent> newEvents;

    #endregion

    #region Properties

    /// <summary>
    /// An list of all current GameEvents.
    /// </summary>
    public IList<GameEvent> List
    {
      get { return this.allEvents.Values; }
    }

    /// <summary>
    /// An list of all GameEvents from the most recent poll.
    /// </summary>
    public IList<GameEvent> New
    {
      get { return this.newEvents.Values; }
    }

    #endregion

    #region Constructors

    /// <summary>
    /// Create a new GameEventCollection object.
    /// </summary>
    public GameEventCollection()
    {
      this.allEvents = new SortedList<DateTime, GameEvent>();
      this.newEvents = new SortedList<DateTime, GameEvent>();
    }

    #endregion

    #region Methods

    /// <summary>
    /// Add a new GameEvent to the list, if it's within the past two hours.
    /// </summary>
    /// <remarks>Also ages the list.</remarks>
    /// <param name="gameEvent">The new GameEvent to add.</param>
    public void Add( GameEvent gameEvent )
    {
      DateTime oldestTime = DateTime.Now.AddHours( -MAX_AGE );
      DateTime eventTime = gameEvent.EventTime;

      if( eventTime < oldestTime ) return;

      while( this.allEvents.ContainsKey( eventTime ) )
        eventTime = gameEvent.IncrementEventTime();  // SortedList doesn't allow duplicate keys

      this.allEvents.Add( eventTime, gameEvent );
      this.newEvents.Add( eventTime, gameEvent );

      while( this.allEvents.Values[0].EventTime < oldestTime )
        this.allEvents.RemoveAt( 0 );
    }

    /// <summary>
    /// Reset the new events list.
    /// </summary>
    public void ResetNewEvents()
    {
      this.newEvents.Clear();
    }

    /// <summary>
    /// Age the list to remove any entries that are now older than two hours.
    /// </summary>
    public void Prune()
    {
      DateTime oldest = DateTime.Now.AddHours( -MAX_AGE );

      while( this.allEvents.Count > 0 && this.allEvents.Values[0].EventTime < oldest )
        this.allEvents.RemoveAt( 0 );
    }

    /// <summary>
    /// Delete all entries and remove any circular references to allow garbage collection.
    /// </summary>
    public void Cleanup()
    {
      this.newEvents.Clear();

      foreach( GameEvent gameEvent in this.allEvents.Values )
        gameEvent.CleanUp();
      this.allEvents.Clear();
    }

    /// <summary>
    /// Repopulates the allEvents/newEvents sorted lists with the current skew correction.
    /// </summary>
    public void UpdateSkewCorrection()
    {
      SortedList<DateTime, GameEvent> updatedAllEvents = new SortedList<DateTime, GameEvent>();
      SortedList<DateTime, GameEvent> updatedNewEvents = new SortedList<DateTime, GameEvent>();

      foreach( GameEvent gameEvent in this.allEvents.Values )
      {
        DateTime time = gameEvent.EventTime;
        while( updatedAllEvents.ContainsKey( time ) )
          time = time.AddMilliseconds( 1 );  // make unique

        updatedAllEvents.Add( time, gameEvent );
      }

      foreach( GameEvent gameEvent in this.newEvents.Values )
      {
        DateTime time = gameEvent.EventTime;
        while( updatedNewEvents.ContainsKey( time ) )
          time = time.AddMilliseconds( 1 );  // make unique

        updatedNewEvents.Add( time, gameEvent );
      }

      this.allEvents = updatedAllEvents;
      this.newEvents = updatedNewEvents;
    }

    /// <summary>
    /// Returns an enumerator that iterates through all game events in the collection.
    /// </summary>
    /// <returns>
    /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
    /// </returns>
    public IEnumerator<GameEvent> GetEnumerator()
    {
      return this.allEvents.Values.GetEnumerator();
    }

    /// <summary>
    /// Returns an enumerator that iterates through all game events in the collection.
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

  #endregion
}
