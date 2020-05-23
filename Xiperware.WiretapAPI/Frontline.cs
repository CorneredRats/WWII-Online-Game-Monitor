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
using System.Linq;

namespace Xiperware.WiretapAPI
{
  /// <summary>
  /// A frontline between friendly territory and "no-mans-land".
  /// </summary>
  /// <remarks>
  /// Generating a pair of frontlines from a list of chokepoints is non-trivial and
  /// took several attempts and rewrites to come up with an algorithm that was
  /// sufficiently robust.
  /// The method used here expands a polygon to create a hull that contains all friendly
  /// towns, then creates a line along the hull edge where it is near the enemy.
  /// </remarks>
  public class Frontline
  {
    #region Variables

    public const int MAX_DISTANCE = 25;        // octets, maximum length of a line segment
    public const double DUMMYCP_DISTANCE = 10;  // octets, offset distance for dummy cps
    public const int MAX_LINK_DIST = 34;       // octets, allow line to cross enemy links longer than this (eg, cross-channel links)

#if DEBUG_FRONTLINE
    private static int  // can change on the fly while debugging
#else
    private const int
#endif
      MIN_ANGLE                  =   35,  // degrees, the sharpest angle two line segments can make
      MIN_DISTANCE_FROM_ENEMY    = 3500,  // meters, distance line must be from enemy town or line
      MIN_DISTANCE_FROM_FRIENDLY = 2000;  // meters, distance line must be from friendly town or line

    private readonly Side side;
    private IList<ChokePoint> allChokepoints;

    private Dictionary<ChokePoint, bool> todo;  // bool unused
    private Dictionary<ChokePoint, bool> invalidStartingPoint;  // bool unused
    private List<List<ChokePoint>> hulls;
    private List<List<Point>> lines;

    private List<List<ChokePoint>> enemyHulls;  // for intersection tests
    private List<Point> enemyLinks;  // points are in pairs

#if DEBUG_FRONTLINE
    public int debugMaxStep = int.MaxValue;
    private int debugCurrentStep = 0;
#endif

    #endregion

    #region Constructors

    /// <summary>
    /// Create a new Frontline object (assign cps later).
    /// </summary>
    /// <param name="side">The Side this frontline surrounds.</param>
    public Frontline( Side side )
      : this( side, new List<ChokePoint>() )
    {

    }

    /// <summary>
    /// Create a new Frontline object.
    /// </summary>
    /// <param name="side">The Side this frontline surrounds.</param>
    /// <param name="allChokepoints">A list of all ChokePoints to be used in calculating this frontline (normal and dummy CPs).</param>
    public Frontline( Side side, IList<ChokePoint> allChokepoints )
    {
      this.side = side;
      this.allChokepoints = allChokepoints;
      this.todo = new Dictionary<ChokePoint, bool>();
      this.invalidStartingPoint = new Dictionary<ChokePoint, bool>();
      this.hulls = new List<List<ChokePoint>>();
      this.lines = new List<List<Point>>();
      this.enemyHulls = new List<List<ChokePoint>>();
      this.enemyLinks = new List<Point>();
    }

    #endregion

    #region Properties

    /// <summary>
    /// List of hulls (each a list of ChokePoints).
    /// </summary>
    public List<List<ChokePoint>> Hulls
    {
      get { return this.hulls; }
    }

    /// <summary>
    /// List of frontlines (each a list of Points, units are game world octets).
    /// </summary>
    public List<List<Point>> Lines
    {
      get { return this.lines; }
    }

    /// <summary>
    /// List of enemy links (Points are in pairs, units are game world octets).
    /// </summary>
    public List<Point> EnemyLinks
    {
      get { return this.enemyLinks; }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Assign the list of all ChokePoints (normal and dummy) used in the
    /// calculation of this frontline.
    /// </summary>
    /// <param name="allChokepoints">The list of ChokePoints to assign.</param>
    public void SetChokePointList( IList<ChokePoint> allChokepoints )
    {
      this.allChokepoints = allChokepoints;
    }

    /// <summary>
    /// Regenerate this frontline.
    /// </summary>
    public void Update()
    {
      Update( new List<List<ChokePoint>>() );
    }

    /// <summary>
    /// Regenerate this frontline, taking into account the given enemy
    /// frontline (avoid intersecting, going too close).
    /// </summary>
    public void Update( List<List<ChokePoint>> enemyHulls )
    {
      if( this.allChokepoints.Count == 0 ) return;  // allCPs not yet set

      this.enemyHulls = enemyHulls;

#if DEBUG_FRONTLINE
      this.debugCurrentStep = 0;
#endif

      ResetData();
      CreateHulls();
      CreateLines();
    }

    /// <summary>
    /// Remove references to local game objects.
    /// </summary>
    public void Cleanup()
    {
      this.allChokepoints = null;
      this.todo.Clear();
      this.invalidStartingPoint.Clear();
      foreach( List<ChokePoint> hull in this.hulls )
        hull.Clear();
      foreach( List<ChokePoint> hull in this.enemyHulls )
        hull.Clear();
    }

    /// <summary>
    /// Reset internal data to prepare for regenerating frontlines.
    /// </summary>
    private void ResetData()
    {
      this.hulls.Clear();
      this.lines.Clear();
      this.invalidStartingPoint.Clear();


      // regenerate todo list

      this.todo.Clear();
      Dictionary<Point, ChokePoint> dupeLocation = new Dictionary<Point, ChokePoint>();

      foreach( ChokePoint cp in this.allChokepoints )
      {
        if( cp == null ) continue;
        if( IsEnemy( cp ) )
        {
          if( dupeLocation.ContainsKey( cp.LocationOctets ) )
            this.todo.Remove( dupeLocation[cp.LocationOctets] );
          continue;
        }
        if( dupeLocation.ContainsKey( cp.LocationOctets ) )
          continue;  // skip towns within same octet

        this.todo.Add( cp, false );
        dupeLocation.Add( cp.LocationOctets, cp );
      }


      // regenerate list of enemy links to avoid intersecting

      this.enemyLinks.Clear();

      foreach( ChokePoint cp in this.allChokepoints )
      {
        if( cp == null ) continue;
        if( IsEnemy( cp ) ) continue;  // only search friendly towns

        foreach( ChokePoint linkedCP in cp.LinkedChokePoints )
        {
          if( linkedCP.Owner.Side != cp.Owner.Side )
          {
            if( Misc.DistanceBetween( cp.LocationOctets, linkedCP.LocationOctets ) > Frontline.MAX_LINK_DIST ) continue;  // don't include cross channel links

            enemyLinks.Add( cp.LocationOctets );
            enemyLinks.Add( linkedCP.LocationOctets );
          }
        }
      }

    }

    /// <summary>
    /// Create one or more hulls that encapsulates all friendly cps.
    /// </summary>
    private void CreateHulls()
    {
      int iHull = -1;

      while( true )
      {
#if DEBUG_FRONTLINE
        if( this.debugCurrentStep > this.debugMaxStep ) break;
#endif

        // start a new hull

        List<ChokePoint> hull = StartHull();
        if( hull == null )
          return;  // no more hulls, finished

        hulls.Add( hull );
        iHull++;


        // expand and refine hull (and any resulting subhulls)

        while( iHull < this.hulls.Count )
        {
#if DEBUG_FRONTLINE
          if( this.debugCurrentStep > this.debugMaxStep ) break;
#endif
          RefineHull( iHull );
          iHull++;
        }
        iHull--;
      }
    }

    /// <summary>
    /// Creates a new hull from the todo list.
    /// </summary>
    /// <returns>An initial 3-node hull, or null if no more starting hulls.</returns>
    private List<ChokePoint> StartHull()
    {
      while( true )
      {
        // get initial point

        ChokePoint cpStart = GetNewStartingPoint();
        if( cpStart == null )
          return null;  // no more starting points


        // try create initial triangle

        List<ChokePoint> hull = GetInitialTriangle( cpStart );
        if( hull.Count >= 3 )
          return hull;
        else
          this.invalidStartingPoint.Add( cpStart, false );
          // and loop again
      }
    }

    /// <summary>
    /// Gets a new starting node from the todo list.
    /// </summary>
    /// <returns>The chokepoint or null if no more are left.</returns>
    private ChokePoint GetNewStartingPoint()
    {
      ChokePoint cpStart = null;

      foreach( ChokePoint cp in this.todo.Keys )
      {
        if( this.invalidStartingPoint.ContainsKey( cp ) )
          continue;

        cpStart = cp;
        break;
      }

      return cpStart;
    }

    /// <summary>
    /// Attempts to create an initial 3-node triangle starting with the given cp.
    /// </summary>
    /// <param name="cpStart">The node to start at.</param>
    /// <returns></returns>
    private List<ChokePoint> GetInitialTriangle( ChokePoint cpStart )
    {
      /*       A
       *       o
       *      / \
       *     /   \
       *    o-----o
       *  C         B
       */

      List<ChokePoint> hull = new List<ChokePoint>();
      hull.Add( cpStart );


      // check each nearby cp for valid second node

      foreach( ChokePoint cpNear in cpStart.NearbyChokePoints )
      {
        if( !this.todo.ContainsKey( cpNear ) ) continue;  // B is enemy or already used
        if( IntersectsWithHull( cpStart, cpNear ) ) continue;  // AB would cross another hull
        if( IntersectsEnemyLink( cpStart, cpNear ) ) continue;  // AB would cross a frontline link


        // check each cp around B to find one that is also near A

        foreach( ChokePoint cpCommon in cpNear.NearbyChokePoints )
        {
          if( !this.todo.ContainsKey( cpCommon ) ) continue;  // C is enemy or already used
          if( !cpCommon.IsNear( cpStart ) ) continue;  // C isn't near A
          if( Misc.AngleBetween( cpStart.LocationOctets, cpNear.LocationOctets, cpCommon.LocationOctets ) < 180 ) continue;  // wrong winding order


          // check if valid triangle

          if( AngleTooSharp( cpStart, cpNear, cpCommon, cpStart, cpNear ) ) continue;  // angle ABC, BCA or CAB too sharp
          if( IntersectsWithHull( cpNear, cpCommon ) ) continue;  // BC would cross another hull
          if( IntersectsWithHull( cpCommon, cpStart ) ) continue;  // CA would cross another hull
          if( IntersectsEnemyLink( cpNear, cpCommon ) ) continue;  // BC would cross a frontline link
          if( IntersectsEnemyLink( cpCommon, cpStart ) ) continue;  // CA would cross a frontline link
          if( LineTooClose( cpCommon, cpStart, cpNear, cpCommon ) ) continue;  // too close to cp or enemy line
          if( LineTooClose( cpStart, cpNear, cpCommon, cpStart ) ) continue;  // too close to cp or enemy line
          if( LineTooClose( cpNear, cpCommon, cpStart, cpNear ) ) continue;  // too close to cp or enemy line


          // get cps within triangle

          List<ChokePoint> toRemove = new List<ChokePoint>();
          bool continueOuter = false;
          foreach( ChokePoint cpWithin in GetChokePointsWithinPoly( cpStart, cpNear, cpCommon ) )
          {
            if( IsEnemy( cpWithin ) )
            {
              continueOuter = true;
              break;
            }
            else  // friendly
            {
              if( this.todo.ContainsKey( cpWithin ) )
                toRemove.Add( cpWithin );  // flag for removal
            }
          }
          if( continueOuter ) continue;  // enemy inside, not valid triangle


          // create triangle

          hull.Add( cpNear );
          hull.Add( cpCommon );
          this.todo.Remove( cpStart );
          this.todo.Remove( cpNear );
          this.todo.Remove( cpCommon );

          foreach( ChokePoint cpWithin in toRemove )
            this.todo.Remove( cpWithin );

          return hull;
        }
      }

      return hull;  // hull < 3 nodes, not valid starting point
    }

    /// <summary>
    /// Takes an initial hull and "inflates" it to fill the available area of
    /// friendly cps using a combination of various algorithms.
    /// </summary>
    /// <param name="iHull">The index of the hull to refine.</param>
    private void RefineHull( int iHull )
    {
      List<ChokePoint> hull = this.hulls[iHull];


      // loop around hull until we complete a loop without making any changes

      bool modified = true;
      while( modified )
      {
        modified = false;

        for( int i = 0; i < hull.Count; i++ )
        {
#if DEBUG_FRONTLINE
          if( this.debugCurrentStep++ >= this.debugMaxStep )
          {
            modified = false;
            break;
          }
#endif
          if( hull.Count < 3 )  // should never happen
          {
            modified = false;
            break;
          }

          if( TryMergeExpand( hull, i ) )
          {
            modified = true;
            continue;
          }

          if( TryMergeTri( hull, i ) )
          {
            modified = true;
            continue;
          }
          /*
          if( TryMergeQuad( hull, i ) )
          {
            modified = true;
            continue;
          }
          */
          if( TryMerge( hull, i ) )
          {
            modified = true;
            continue;
          }

          if( TryExpand( hull, i ) )
          {
            modified = true;
            continue;
          }

          List<ChokePoint> subHull;
          if( TrySplit( hull, i, out subHull ) )
          {
            this.hulls.Add( subHull );
            modified = true;
            continue;
          }

        }  // end for
      }  // end while
    }

    /// <summary>
    /// Attempt to move a point on the hull further out.
    /// </summary>
    /// <param name="hull">The hull to act upon.</param>
    /// <param name="i1">The index of the starting node.</param>
    /// <returns>True if successful.</returns>
    private bool TryMergeExpand( IList<ChokePoint> hull, int i1 )
    {
      /*  A o-----o B
       *        /  .      ABCDE
       *     C o    o X    ==>
       *        \  .      ABXDE
       *  E o-----o D
       */

      if( hull.Count <= 4 ) return false;

      int i0 = i1 - 1; if( i0 < 0 ) i0 += hull.Count;
      int i2 = i1 + 1; if( i2 > hull.Count - 1 ) i2 -= hull.Count;
      int i3 = i1 + 2; if( i3 > hull.Count - 1 ) i3 -= hull.Count;
      int i4 = i1 + 3; if( i4 > hull.Count - 1 ) i4 -= hull.Count;


      // check each nearby cp for possible common neighbour

      foreach( ChokePoint cpNear in hull[i1].NearbyChokePoints )
      {
        if( !this.todo.ContainsKey( cpNear ) ) continue;  // enemy or already used
        if( !hull[i3].IsNear( cpNear ) ) continue;  // X isn't near D


        // check if valid expansion

        if( AngleTooSharp( hull[i0], hull[i1], cpNear, hull[i3], hull[i4] ) ) continue;  // angle ABX, BXD, or XDE too sharp
        if( Misc.AngleBetween( cpNear.LocationOctets, hull[i1].LocationOctets, hull[i2].LocationOctets ) < 10 ) continue;  // angle XBC too sharp
        if( Misc.AngleBetween( hull[i2].LocationOctets, hull[i3].LocationOctets, cpNear.LocationOctets ) < 10 ) continue;  // angle CDX too sharp
        if( IntersectsWithHull( hull[i1], cpNear ) ) continue;  // BX would cross another hull
        if( IntersectsWithHull( cpNear, hull[i3] ) ) continue;  // XD would cross another hull
        if( IntersectsEnemyLink( hull[i1], cpNear ) ) continue;  // BX crosses frontline link
        if( IntersectsEnemyLink( cpNear, hull[i3] ) ) continue;  // XD crosses frontline link
        if( LineTooClose( hull[i0], hull[i1], cpNear, hull[i3] ) ) continue;  // too close to cp or enemy line
        if( LineTooClose( hull[i1], cpNear, hull[i3], hull[i4] ) ) continue;  // too close to cp or enemy line


        // get cps within expand area (BCDX)

        List<ChokePoint> toRemove = new List<ChokePoint>();
        bool continueOuter = false;
        foreach( ChokePoint cpWithin in GetChokePointsWithinPoly( hull[i1], hull[i2], hull[i3], cpNear ) )
        {
          if( IsEnemy( cpWithin ) )  // enemy cp within BCDX
          {
            continueOuter = true;
            break;
          }
          if( cpWithin.IsFrontline )  // frontline town within BCDX
          {
            continueOuter = true;
            break;
          }

          if( this.todo.ContainsKey( cpWithin ) )  // flag for removal
          {
            toRemove.Add( cpWithin );
          }
          else if( hull.Contains( cpWithin ) )  // avoid small 3-node hull trying to expand/invert around self
          {
            continueOuter = true;
            break;
          }
        }
        if( continueOuter ) continue;


        // do expansion

        hull.RemoveAt( i2 );
        hull.Insert( i2, cpNear );
        this.todo.Remove( cpNear );

        foreach( ChokePoint cpWithin in toRemove )
          this.todo.Remove( cpWithin );

        return true;
      }

      return false;
    }

    /// <summary>
    /// Attempt to merge an inward triangle.
    /// </summary>
    /// <param name="hull">The hull to act upon.</param>
    /// <param name="i1">The index of the starting node.</param>
    /// <returns>True if successful.</returns>
    private bool TryMergeTri( IList<ChokePoint> hull, int i1 )
    {
      /*  A o-----o B
       *        / :     ABCDE
       *     C o  :      ==>
       *        \ :     ABDE
       *  E o-----o D
       */

      int i0 = i1 - 1; if( i0 < 0 ) i0 += hull.Count;
      int i2 = i1 + 1; if( i2 > hull.Count - 1 ) i2 -= hull.Count;
      int i3 = i1 + 2; if( i3 > hull.Count - 1 ) i3 -= hull.Count;
      int i4 = i1 + 3; if( i4 > hull.Count - 1 ) i4 -= hull.Count;


      // check if valid merge

      if( !hull[i1].IsNear( hull[i3] ) ) return false;  // BD not near each other
      if( !IsSegmentConcave( hull, i1, i3 ) ) return false;  // BCD is outward
      if( AngleTooSharp( hull[i0], hull[i1], hull[i3], hull[i4] ) ) return false;  // angle ABD or BDE too sharp
      if( IntersectsWithHull( hull[i1], hull[i3] ) ) return false;  // BD crosses another hull
      if( IntersectsEnemyLink( hull[i1], hull[i3] ) ) return false;  // BD crosses frontline link
      if( LineTooClose( hull[i0], hull[i1], hull[i3], hull[i4] ) ) return false;  // BD too close to cp or enemy line


      // get cps within merge area (BCD)

      List<ChokePoint> toRemove = new List<ChokePoint>();
      foreach( ChokePoint cpWithin in GetChokePointsWithinPoly( hull[i1], hull[i2], hull[i3] ) )
      {
        if( IsEnemy( cpWithin ) )
          return false;  // enemy cp within BCD

        if( cpWithin.IsFrontline )
          return false;  // frontline town within BCD

        if( this.todo.ContainsKey( cpWithin ) )
          toRemove.Add( cpWithin );  // flag for removal
      }


      // perform merge

      hull.RemoveAt( i2 );

      foreach( ChokePoint cpWithin in toRemove )
        this.todo.Remove( cpWithin );


      return true;
    }

    /// <summary>
    /// Attempt to merge an inward quad.
    /// </summary>
    /// <param name="hull">The hull to act upon.</param>
    /// <param name="i1">The index of the starting node.</param>
    /// <returns>True if successful.</returns>
    private bool TryMergeQuad( IList<ChokePoint> hull, int i1 )
    {
      /*    A o
       *       \
       *  C o---o B    ABCDEF
       *    |   :        =>
       *  D o---o E     ABEF
       *       /
       *    F o
       */

      if( hull.Count < 7 ) return false;

      int i0 = i1 - 1; if( i0 < 0 ) i0 += hull.Count;
      int i2 = i1 + 1; if( i2 > hull.Count - 1 ) i2 -= hull.Count;
      int i3 = i1 + 2; if( i3 > hull.Count - 1 ) i3 -= hull.Count;
      int i4 = i1 + 3; if( i4 > hull.Count - 1 ) i4 -= hull.Count;
      int i5 = i1 + 4; if( i5 > hull.Count - 1 ) i5 -= hull.Count;


      // check if valid merge

      if( !hull[i1].IsNear( hull[i4] ) ) return false;  // BE not near each other
      if( !IsSegmentConcave( hull, i1, i4 ) ) return false;  // BCDE is outward
      if( AngleTooSharp( hull[i0], hull[i1], hull[i4], hull[i5] ) ) return false;  // angle ABE or BEF too sharp
      if( IntersectsWithHull( hull[i1], hull[i4] ) ) return false;  // BE crosses another hull
      if( IntersectsEnemyLink( hull[i1], hull[i4] ) ) return false;  // BE crosses frontline link
      if( LineTooClose( hull[i0], hull[i1], hull[i4], hull[i5] ) ) return false;  // too close to cp or enemy line


      // get cps within merge area (BCDE)

      List<ChokePoint> toRemove = new List<ChokePoint>();
      foreach( ChokePoint cpWithin in GetChokePointsWithinPoly( hull[i1], hull[i2], hull[i3], hull[i4] ) )
      {
        if( IsEnemy( cpWithin ) )
          return false;  // enemy cp within BCDE

        if( cpWithin.IsFrontline )
          return false;  // frontline town within BCDE

        if( this.todo.ContainsKey( cpWithin ) )
          toRemove.Add( cpWithin );  // flag for removal
      }


      // perform merge

      hull.RemoveAt( i2 );
      if( i2 > hull.Count - 1 ) i2 -= hull.Count;
      hull.RemoveAt( i2 );

      foreach( ChokePoint cpWithin in toRemove )
        this.todo.Remove( cpWithin );

      return true;
    }

    /// <summary>
    /// Attempt to add a new point.
    /// </summary>
    /// <param name="hull">The hull to act upon.</param>
    /// <param name="i1">The index of the starting node.</param>
    /// <returns>True if successful.</returns>
    private bool TryExpand( IList<ChokePoint> hull, int i1 )
    {
      /*  A o-----o B
       *          | .       ABCD
       *          |  o X    =>
       *          | .       ABXCD
       *  D o-----o C
       */

      int i0 = i1 - 1; if( i0 < 0 ) i0 += hull.Count;
      int i2 = i1 + 1; if( i2 > hull.Count - 1 ) i2 -= hull.Count;
      int i3 = i1 + 2; if( i3 > hull.Count - 1 ) i3 -= hull.Count;


      // check each nearby cp for possible common neighbour

      foreach( ChokePoint cpNear in hull[i1].NearbyChokePoints )
      {
        if( !this.todo.ContainsKey( cpNear ) ) continue;  // enemy or already used
        if( !hull[i2].IsNear( cpNear ) ) continue;  // X isn't near C


        // check if valid expansion

        if( AngleTooSharp( hull[i0], hull[i1], cpNear, hull[i2], hull[i3] ) ) continue;  // angle ABX, BXC or XCD too sharp
        if( IntersectsWithHull( hull[i1], cpNear ) ) continue;  // BX crosses another hull
        if( IntersectsWithHull( cpNear, hull[i2] ) ) continue;  // XC crosses another hull
        if( IntersectsEnemyLink( hull[i1], cpNear ) ) continue;  // BX crosses frontline link
        if( IntersectsEnemyLink( cpNear, hull[i2] ) ) continue;  // XC crosses frontline link
        if( LineTooClose( hull[i0], hull[i1], cpNear, hull[i2] ) ) continue;  // BX too close to cp or enemy line
        if( LineTooClose( hull[i1], cpNear, hull[i2], hull[i3] ) ) continue;  // XC too close to cp or enemy line


        // get cps within expand area (BXC)

        List<ChokePoint> toRemove = new List<ChokePoint>();
        bool continueOuter = false;
        foreach( ChokePoint cpWithin in GetChokePointsWithinPoly( hull[i1], cpNear, hull[i2] ) )
        {
          if( IsEnemy( cpWithin ) )  // enemy cp within BXC
          {
            continueOuter = true;
            break;
          }

          if( cpWithin.IsFrontline )  // frontline town within BXC
          {
            continueOuter = true;
            break;
          }

          if( this.todo.ContainsKey( cpWithin ) )  // flag for removal
          {
            toRemove.Add( cpWithin );
          }
          else if( hull.Contains( cpWithin ) )  // avoid small 3-node hull trying to expand/invert around self
          {
            continueOuter = true;
            break;
          }
        }
        if( continueOuter ) continue;



        // do expansion

        hull.Insert( i2, cpNear );
        this.todo.Remove( cpNear );
        foreach( ChokePoint cpWithin in toRemove )
          this.todo.Remove( cpWithin );


        return true;
      }

      return false;
    }

    /// <summary>
    /// Attempts to merge an inward polygon (4+ points).
    /// </summary>
    /// <param name="hull">The hull to act upon.</param>
    /// <param name="i1">The index of the starting node.</param>
    /// <returns>True if successful.</returns>
    private bool TryMerge( List<ChokePoint> hull, int i1 )
    {
      /*       i1    i0
       *        o----o---
       *       . \
       *      .   \
       *  j1 o     o i2
       *     |\   /
       *     | \ /
       *     |  o j2
       *  j0 o
       *      \
       *       \
       */

      int i0 = i1 - 1; if( i0 < 0 ) i0 += hull.Count;
      int i2 = i1 + 1; if( i2 > hull.Count - 1 ) i2 -= hull.Count;


      // check each cp near i1 for a valid j1

      foreach( ChokePoint cpNear in hull[i1].NearbyChokePoints )
      {
        int j1 = hull.IndexOf( cpNear );
        if( j1 == -1 ) continue;  // not in current hull

        int j0 = j1 + 1; if( j0 > hull.Count - 1 ) j0 -= hull.Count;
        int j2 = j1 - 1; if( j2 < 0 ) j2 += hull.Count;


        // get length of subhull

        bool wrapsZero = false;
        int subHullLength = ( j1 - i1 ) + 1;
        if( subHullLength <= 0 )
        {
          subHullLength += hull.Count;  // warp around [0]
          wrapsZero = true;  // index 0 occurs within i2-j1
        }
        int outerHullLength = hull.Count - subHullLength + 2;

        if( subHullLength < 4 ) continue;
        if( outerHullLength < subHullLength ) continue;


        // check if valid merge

        if( !IsSegmentConcave( hull, i1, j1 ) ) continue;  // subhull isn't concave
        if( AngleTooSharp( hull[i0], hull[i1], hull[j1], hull[j0] ) ) continue;  // angle i0-i1-j1 or i1-j1-j0 too sharp
        if( IntersectsWithHull( hull[j1], hull[i1] ) ) continue;  // j1-i1 crosses another hull
        if( IntersectsEnemyLink( hull[j1], hull[i1] ) ) continue;  // j1-i1 crosses frontline link
        if( LineTooClose( hull[i0], hull[i1], hull[j1], hull[j0] ) ) continue;  // i1-j1 too close to cp or enemy line


        // create subHull

        List<ChokePoint> subHull = new List<ChokePoint>();
        if( wrapsZero )
        {
          subHull.AddRange( hull.GetRange( i1, hull.Count - i1 ) );
          subHull.AddRange( hull.GetRange( 0, j1 + 1 ) );
        }
        else
        {
          subHull.AddRange( hull.GetRange( i1, subHullLength ) );
        }


        // make sure no enemy cp(s) inside

        List<ChokePoint> toRemove = new List<ChokePoint>();
        bool continueOuter = false;
        foreach( ChokePoint cpWithin in GetChokePointsWithinPoly( subHull ) )
        {
          if( IsEnemy( cpWithin ) )  // enemy cp within merge area
          {
            continueOuter = true;
            break;
          }

          if( cpWithin.IsFrontline )  // frontline town within merge area
          {
            continueOuter = true;
            break;
          }

          if( this.todo.ContainsKey( cpWithin ) )  // flag friendly town for removal
            toRemove.Add( cpWithin );
        }
        if( continueOuter ) continue;


        // perform split

        if( wrapsZero )
        {
          if( i2 != 0 ) hull.RemoveRange( i2, hull.Count - i2 );
          hull.RemoveRange( 0, j1 );
        }
        else
        {
          hull.RemoveRange( i2, subHullLength - 2 );
        }

        foreach( ChokePoint cpWithin in toRemove )
          this.todo.Remove( cpWithin );

        return true;

      }  // end foreach cpNear


      return false;
    }

    /// <summary>
    /// Attempts to split the polygon into two when it folds around a pocket back on itself.
    /// </summary>
    /// <param name="hull">The hull to act upon.</param>
    /// <param name="i1">The index of the starting node.</param>
    /// <param name="subHull">The hull to populate with the inner "subhull" if found.</param>
    /// <returns>True if successful.</returns>
    private bool TrySplit( List<ChokePoint> hull, int i1, out List<ChokePoint> subHull )
    {
      /*             j3
       *     j2 o----o----o       When the polygon expands around a pocket of enemy towns,
       *      .  \          \     it folds back to meet itself. Where i1 is near j1, and i2
       *     .  j1 o---o j0  \    is near j2, we split the poly into two by joining i1 - j1
       * i2 o _   .      \    o   and i2 - j2. If successful, the sub-hull is pushed onto
       *    |   o i1 x    o   |   the hull list and will be refined next.
       *    |   |         |   |
       *    |   o i0 x    o   |
       * i3 o    \       /    o
       *     \     o---o     /
       *      \             /
       *        o----o----o
       */

      subHull = new List<ChokePoint>();

      int i0 = i1 - 1; if( i0 < 0 ) i0 += hull.Count;
      int i2 = i1 + 1; if( i2 > hull.Count - 1 ) i2 -= hull.Count;
      int i3 = i1 + 2; if( i3 > hull.Count - 1 ) i3 -= hull.Count;


      // check each cp near i1 for a valid j1

      foreach( ChokePoint cpNear in hull[i1].NearbyChokePoints )
      {
        int j1 = hull.IndexOf( cpNear );
        if( j1 == -1 ) continue;  // not in current hull

        int j0 = j1 + 1; if( j0 > hull.Count - 1 ) j0 -= hull.Count;
        int j2 = j1 - 1; if( j2 < 0 ) j2 += hull.Count;
        int j3 = j1 - 2; if( j3 < 0 ) j3 += hull.Count;

        if( !hull[i2].IsNear( hull[j2] ) ) continue;  // j2 not near i2


        // get length of subhull

        bool wrapsZero = false;
        int subHullLength = ( i1 - j1 ) + 1;
        if( subHullLength <= 0 )
        {
          subHullLength += hull.Count;  // warp around [0]
          wrapsZero = true;
        }
        int outerHullLength = hull.Count - subHullLength;

        if( subHullLength < 3 ) continue;
        if( outerHullLength < subHullLength ) continue;


        // check if valid merge

        if( !IsSegmentConcave( hull, j1, i1 ) ) continue;  // subhull isn't concave
        if( !IsSegmentConcave( hull, j2, i2 ) ) continue;  // subhull "entrance" isn't concave
        if( AngleTooSharp( hull[i0], hull[i1], hull[j1], hull[j0] ) ) continue;  // angle i0-i1-j1 or i1-j1-j0 too sharp
        if( IntersectsWithHull( hull[i1], hull[j1] ) ) continue;  // i1-j1 crosses another hull
        if( IntersectsWithHull( hull[i2], hull[j2] ) ) continue;  // i2-j2 crosses another hull
        if( IntersectsEnemyLink( hull[i1], hull[j1] ) ) continue;  // i1-j1 crosses frontline link
        if( IntersectsEnemyLink( hull[i2], hull[j2] ) ) continue;  // i2-j2 crosses frontline link
        if( LineTooClose( hull[i0], hull[i1], hull[j1], hull[j0] ) ) continue;  // i1-j1 too close to cp or enemy line
        if( LineTooClose( hull[j3], hull[j2], hull[i2], hull[i3] ) ) continue;  // j2-i2 too close to cp or enemy line


        // get cps within merge area (i1-i2-j2-j1)

        List<ChokePoint> toRemove = new List<ChokePoint>();
        bool continueOuter = false;
        foreach( ChokePoint cpWithin in GetChokePointsWithinPoly( hull[i1], hull[i2], hull[j2], hull[j1] ) )
        {
          if( IsEnemy( cpWithin ) )  // enemy cp within merge area
          {
            continueOuter = true;
            break;
          }

          if( cpWithin.IsFrontline )  // frontline town within merge area
          {
            continueOuter = true;
            break;
          }

          if( this.todo.ContainsKey( cpWithin ) )  // flag for removal
            toRemove.Add( cpWithin );
        }
        if( continueOuter ) continue;


        // populate subHull (will not be used unless we return true)

        subHull.Clear();
        if( wrapsZero )
        {
          subHull.AddRange( hull.GetRange( j1, hull.Count - j1 ) );
          subHull.AddRange( hull.GetRange( 0, i1 + 1 ) );
        }
        else
        {
          subHull.AddRange( hull.GetRange( j1, ( i1 - j1 ) + 1 ) );
        }


        // make sure enemy cp(s) inside

        bool enemyWithin = false;
        foreach( ChokePoint cpWithin in GetChokePointsWithinPoly( subHull ) )
        {
          if( IsEnemy( cpWithin ) )
          {
            enemyWithin = true;
            break;
          }
        }
        if( !enemyWithin ) continue;


        // perform split (subHull already assigned above)

        if( wrapsZero )
        {
          hull.RemoveRange( j1, hull.Count - j1 );
          hull.RemoveRange( 0, i1 + 1 );
        }
        else
        {
          hull.RemoveRange( j1, ( i1 - j1 ) + 1 );
        }

        foreach( ChokePoint cpWithin in toRemove )
          this.todo.Remove( cpWithin );

        return true;

      }  // end foreach cpNear


      return false;
    }

    /// <summary>
    /// Create frontlines from the hulls.
    /// </summary>
    private void CreateLines()
    {
      /*             x       x     x       x
       *                 x             x
       *                / \           / \
       *   o   o   o - O===O====o====O===O - o   o   o
       *           0   1   2    3    4   5   6
       *         tail lit lit unlit lit lit tail
       * 
       * A node of a hull only becomes part of a line ("lit up") if either it or one
       * of it's immediate neighbours are near an enemy cp. This results is a line
       * that has the first and last nodes not near the enemy. Theses tails are not
       * drawn but are required to shape the final smoothed line.
       */

      foreach( List<ChokePoint> hull in this.hulls )
      {
        // generate "lit up" parallel list

        List<bool> litUp = new List<bool>();
        foreach( ChokePoint cp in hull )
          litUp.Add( GetNodeLitUp( cp ) );


        // normalise hull order so that it starts with a gap (if present) to avoid
        // issues with a line wrapping around index [0]

        int firstIndex = -1;
        int currentGapSize = litUp[hull.Count - 1] ? 0 : 1;  // seed with value of last item
        int currentDummyNodeLength = 0;

        for( int i = 0; i < hull.Count; i++ )
        {
          int iNext = i + 1; if( iNext > hull.Count - 1 ) iNext -= hull.Count;

          if( litUp[i] ) currentGapSize = 0;
          else           currentGapSize++;

          if( hull[i].Type == ChokePointType.Dummy ) currentDummyNodeLength++;
          else                                       currentDummyNodeLength = 0;

          if( currentGapSize >= 2 )
          {
            firstIndex = i;
            break;
          }
          if( currentDummyNodeLength > 0 && hull[iNext].Type != ChokePointType.Dummy )  // end of 1+ dummy nodes
          {
            firstIndex = i;
            if( currentDummyNodeLength > 1 )  // end of 2+ dummy nodes
              break;
          }
        }

        if( firstIndex > 0 )
        {
          List<ChokePoint> tempHull = hull.GetRange( 0, firstIndex );
          List<bool> tempNear = litUp.GetRange( 0, firstIndex );
          hull.RemoveRange( 0, firstIndex );
          litUp.RemoveRange( 0, firstIndex );
          hull.AddRange( tempHull );
          litUp.AddRange( tempNear );
        }


        // loop over nodes and add buffer, creating a line when there's a break

        List<Point> lineBuffer = new List<Point>();
        int skippedNodes = 0;
        bool startsWithDummyNode = false, endsWithDummyNode = false;
        for( int i = 0; i < hull.Count; i++ )
        {
          int iPrev = i - 1; if( iPrev < 0              ) iPrev += hull.Count;
          int iNext = i + 1; if( iNext > hull.Count - 1 ) iNext -= hull.Count;

          if( litUp[iPrev] || litUp[i] || litUp[iNext] )  // valid node, add
          {
            if( hull[i].Type != ChokePointType.Dummy )
            {
              lineBuffer.Add( hull[i].LocationOctets );
              continue;
            }
            // else, dummy node

            if( hull[iPrev].Type != ChokePointType.Dummy && hull[iNext].Type != ChokePointType.Dummy
              && litUp[iPrev] && litUp[iNext] && hull.Count > 4 )
            {
              // single dummy inbetween two normal nodes: merge it
              skippedNodes++;
              continue;
            }

            // add dummy node, only continue line if this is the first node

            lineBuffer.Add( hull[i].LocationOctets );
            if( lineBuffer.Count == 1 )
            {
              startsWithDummyNode = true;
              continue;
            }
            else
            {
              endsWithDummyNode = true;
              // fall through to end line
            }
          }
          // else invalid node, end line


          // end line

          if( lineBuffer.Count >= 5 )  // long enough: keep
          {
            this.lines.Add( lineBuffer );
            lineBuffer = new List<Point>();
          }
          else if( lineBuffer.Count > 0 )  // too short: discard
          {
            lineBuffer.Clear();
          }
          startsWithDummyNode = endsWithDummyNode = false;
        }


        // if all hull nodes are used (no gaps), add tails, etc

        if( lineBuffer.Count + skippedNodes == hull.Count && lineBuffer.Count >= 3 && !startsWithDummyNode && !endsWithDummyNode )
        {
          // no dummy nodes: line is a closed loop

          Point last = lineBuffer[lineBuffer.Count - 1];

          lineBuffer.Add( lineBuffer[0] );  // close loop
          lineBuffer.Add( lineBuffer[1] );  // add tail to end
          lineBuffer.Insert( 0, last );     // add tail to start
        }
        else if( lineBuffer.Count + skippedNodes == hull.Count && lineBuffer.Count >= 4 && startsWithDummyNode )
        {
          // starts with dummy node

          lineBuffer.Add( lineBuffer[0] );  // add tail to end
        }

        if( lineBuffer.Count >= 5 )  // keep buffer if long enough
        {
          this.lines.Add( lineBuffer );
        }

      }  // end foreach hull
    }

    /// <summary>
    /// Gets whether a hull node should be "lit up" (part of a line).
    /// </summary>
    /// <param name="cp">The ChokePoint node.</param>
    /// <returns>True if should be part of a line.</returns>
    private bool GetNodeLitUp( ChokePoint cp )
    {
      // dummy nodes are always unlit

      if( cp.Type == ChokePointType.Dummy )
        return false;


      foreach( ChokePoint cpNear in cp.NearbyChokePoints )
      {
        // ignore if on other side of a hull

        if( IntersectsWithFriendlyHull( cp, cpNear ) )
          continue;


        // light up if near enemy

        if( IsEnemy2( cpNear ) )
          return true;


        // light up if near a friendly frontline town that isn't part of a hull

        if( cpNear.IsFrontline && !IsHullChokePoint( cpNear ) )
          return true;
      }

      return false;
    }

    /// <summary>
    /// Checks if the given ChokePoint is part of a hull.
    /// </summary>
    /// <param name="cp">The ChokePoint to test.</param>
    /// <returns>True if exists in any friendly hull.</returns>
    private bool IsHullChokePoint( ChokePoint cp )
    {
      return this.hulls.Any( hull => hull.Contains( cp ) );
    }

    /// <summary>
    /// Checks if the given ChokePoint is an enemy (draw hull).
    /// </summary>
    private bool IsEnemy( ChokePoint cp )
    {
      return this.side != cp.Owner.Side;
      // return Misc.AreEnemy( this.side, cp.Owner );
    }

    /// <summary>
    /// Checks if the given ChokePoint is an enemy (light up hull).
    /// </summary>
    private bool IsEnemy2( ChokePoint cp )
    {
      // return this.side != cp.Owner.Side;
      return Misc.AreEnemy( this.side, cp.Owner );
    }

    #region Geometry Methods

    /// <summary>
    /// Tests whether a line segment would intersect with a specific hull (or enemy).
    /// </summary>
    /// <param name="cp1">The chokepoint at the start of the line segment.</param>
    /// <param name="cp2">The chokepoint at the end of the line segment.</param>
    /// <returns>True if the line segment cp1-cp2 intersects with the given hull, or if provided, any enemy hull.</returns>
    private bool IntersectsWithHull( ChokePoint cp1, ChokePoint cp2 )
    {
      if( IntersectsWithFriendlyHull( cp1, cp2 ) )
        return true;
      if( IntersectsWithEnemyHull( cp1, cp2 ) )
        return true;

      return false;
    }

    /// <summary>
    /// Tests whether a line segment would intersect with a friendly hull.
    /// </summary>
    /// <param name="cp1">The chokepoint at the start of the line segment.</param>
    /// <param name="cp2">The chokepoint at the end of the line segment.</param>
    /// <returns>True if the line segment cp1-cp2 intersects with any friendly hull.</returns>
    private bool IntersectsWithFriendlyHull( ChokePoint cp1, ChokePoint cp2 )
    {
      foreach( List<ChokePoint> hull in this.hulls )
      {
        if( HullIntersect( hull, cp1.Location, cp2.Location ) )
          return true;
      }

      return false;
    }

    /// <summary>
    /// Tests whether a line segment would intersect with an enemy hull (if provided).
    /// </summary>
    /// <param name="cp1">The chokepoint at the start of the line segment.</param>
    /// <param name="cp2">The chokepoint at the end of the line segment.</param>
    /// <returns>True if the line segment cp1-cp2 intersects with any enemy hull.</returns>
    private bool IntersectsWithEnemyHull( ChokePoint cp1, ChokePoint cp2 )
    {
      foreach( List<ChokePoint> hull in this.enemyHulls )
      {
        if( HullIntersect( hull, cp1.Location, cp2.Location ) )
          return true;
      }

      return false;
    }

    /// <summary>
    /// Tests whether a line segment would intersect with a hull.
    /// </summary>
    /// <remarks>Used internally by IntersectsWithHull() and IntersectsWithAnyHull().</remarks>
    /// <param name="hull">The chokepoint hull to test.</param>
    /// <param name="lines">An array of Points in game meters that make up the line(s) to test.</param>
    /// <returns>True if the line segment pt1-pt2 intersects with the given hull.</returns>
    private bool HullIntersect( IList<ChokePoint> hull, params Point[] lines )
    {
      for( int iLine = 0; iLine < lines.Length - 1; iLine++ )
      {
        for( int iHull = 0; iHull < hull.Count; iHull++ )
        {
          int iHull2 = iHull + 1;
          if( iHull2 > hull.Count - 1 ) iHull2 -= hull.Count;

          if( Misc.LineIntersect( lines[iLine], lines[iLine+1], hull[iHull].Location, hull[iHull2].Location ) )
            return true;
        }
      }

      return false;
    }

    /// <summary>
    /// Gets any ChokePoints within a polygon.
    /// </summary>
    /// <param name="polyCPs">A series of ChokePoints that make up a polygon.</param>
    /// <returns>Any ChokePoints found to lie within the given polygon.</returns>
    private List<ChokePoint> GetChokePointsWithinPoly( IList<ChokePoint> polyCPs )
    {
      Point[] poly = new Point[polyCPs.Count];
      for( int i = 0; i < polyCPs.Count; i++ )
        poly[i] = polyCPs[i].Location;

      return GetChokePointsWithinPoly( poly );
    }

    /// <summary>
    /// Gets any ChokePoints within a polygon.
    /// </summary>
    /// <param name="polyCPs">A series of ChokePoints that make up a polygon.</param>
    /// <returns>Any ChokePoints found to lie within the given polygon.</returns>
    private List<ChokePoint> GetChokePointsWithinPoly( params ChokePoint[] polyCPs )
    {
      Point[] poly = new Point[polyCPs.Length];
      for( int i = 0; i < polyCPs.Length; i++ )
        poly[i] = polyCPs[i].Location;

      return GetChokePointsWithinPoly( poly );
    }

    /// <summary>
    /// Gets any ChokePoints within a polygon.
    /// </summary>
    /// <param name="poly">A series of Points in game meters that make up a polygon.</param>
    /// <returns>Any ChokePoints found to lie within the given polygon.</returns>
    private List<ChokePoint> GetChokePointsWithinPoly( params Point[] poly )
    {
      return GetChokePointsWithinPoly( false, poly );
    }

    /// <summary>
    /// Checks if any ChokePoints are within a polygon.
    /// </summary>
    /// <param name="poly">A series of Points in game meters that make up a polygon.</param>
    /// <returns>True when the first cp is found.</returns>
    private bool IsChokePointsWithinPoly( params Point[] poly )
    {
      return GetChokePointsWithinPoly( true, poly ).Count > 0;
    }

    /// <summary>
    /// Gets any ChokePoints within a polygon.
    /// </summary>
    /// <param name="onlyFirst">Only return the first cp.</param>
    /// <param name="poly">A series of Points in game meters that make up a polygon.</param>
    /// <returns>Any ChokePoints found to lie within the given polygon.</returns>
    private List<ChokePoint> GetChokePointsWithinPoly( bool onlyFirst, params Point[] poly )
    {
      List<ChokePoint> found = new List<ChokePoint>();
      Dictionary<Point, bool> polyLookup = new Dictionary<Point, bool>();  // bool unused


      // create lookup hash

      foreach( Point pt in poly )
        if( !polyLookup.ContainsKey( pt ) )  // may be duplicates (eg, EnemyNearLine())
          polyLookup.Add( pt, false );


      // get chokepoints within polygon

      foreach( ChokePoint cpTest in this.allChokepoints )
      {
        if( cpTest == null ) continue;
        if( polyLookup.ContainsKey( cpTest.Location ) ) continue;  // don't check if edge point is within poly

        if( Misc.IsPointWithinPoly( cpTest.Location, poly ) )
        {
          found.Add( cpTest );
          if( onlyFirst ) break;
        }
      }


      return found;
    }

    /// <summary>
    /// Tests whether the given segment of a hull is concave.
    /// </summary>
    /// <param name="hull">The chokepoint hull to test.</param>
    /// <param name="start">The index of the first node in the segment.</param>
    /// <param name="end">The index of the last node in the segment (may wrap around index 0).</param>
    /// <returns>True if the segment as a whole is concave.</returns>
    private bool IsSegmentConcave( IList<ChokePoint> hull, int start, int end )
    {
      // get segment length

      int length = ( end - start ) + 1;
      if( length <= 0 )
        length += hull.Count;


      // get average angle

      float angle = 0;
      int a = start;     if( a > hull.Count - 1 ) a -= hull.Count;
      int b = start + 1; if( b > hull.Count - 1 ) b -= hull.Count;
      int c = start + 2; if( c > hull.Count - 1 ) c -= hull.Count;

      for( int i = 0; i < length - 2; i++ )
      {
        angle += (float)Misc.AngleBetween( hull[a].LocationOctets, hull[b].LocationOctets, hull[c].LocationOctets );

        a++; if( a > hull.Count - 1 ) a -= hull.Count;
        b++; if( b > hull.Count - 1 ) b -= hull.Count;
        c++; if( c > hull.Count - 1 ) c -= hull.Count;
      }
      angle /= length - 2;


      // concave if average angle is less than 180

      return angle < 160;  // make sure at least 20deg concave
    }

    /// <summary>
    /// Tests whether a line segment would intersect with an enemy link.
    /// </summary>
    /// <param name="cp1">The chokepoint at the start of the line segment.</param>
    /// <param name="cp2">The chokepoint at the end of the line segment.</param>
    /// <returns>True if the line segment cp1-cp2 crosses an enemy link.</returns>
    private bool IntersectsEnemyLink( ChokePoint cp1, ChokePoint cp2 )
    {
      for( int i = 0; i < this.enemyLinks.Count; i += 2 )
        if( Misc.LineIntersect( cp1.LocationOctets, cp2.LocationOctets, this.enemyLinks[i], this.enemyLinks[i + 1] ) )
          return true;

      return false;
    }

    /// <summary>
    /// Check if the proposed frontline segment is too close to another cp or enemy frontline.
    /// </summary>
    /// <param name="cp0">The ChokePoint before the start of the line.</param>
    /// <param name="cp1">The ChokePoint at the start of the line.</param>
    /// <param name="cp2">The ChokePoint at the end of the line.</param>
    /// <param name="cp3">The ChokePoint after the end of the line.</param>
    /// <returns>True if there is a cp or enemy frontline nearby.</returns>
    private bool LineTooClose( ChokePoint cp0, ChokePoint cp1, ChokePoint cp2, ChokePoint cp3 )
    {
      if( FriendlyNearLine( cp0, cp1, cp2, cp3 ) )
        return true;

      if( EnemyNearLine( cp0, cp1, cp2, cp3 ) )
        return true;

      return false;
    }

    /// <summary>
    /// Creates a polygon between two cps covering an area that needs to be checked
    /// for "collisions" with other frontlines, cps, etc.
    /// </summary>
    /// <param name="cp0">The ChokePoint before the start of the line.</param>
    /// <param name="cp1">The ChokePoint at the start of the line.</param>
    /// <param name="cp2">The ChokePoint at the end of the line.</param>
    /// <param name="cp3">The ChokePoint after the end of the line.</param>
    /// <param name="friendly"></param>
    /// <returns>A list of Point's in game meters.</returns>
    /// <remarks>Enable DEBUG_FRONTLINE and select a frontline cp on the map to see the poly.</remarks>
    public static Point[] GetCollisionPoly( ChokePoint cp0, ChokePoint cp1, ChokePoint cp2, ChokePoint cp3, bool friendly )
    {
      /*        cp3 o      To check wether cp1-cp2 is too close to something,
       *           /       we create a simple 3-5 sided "collision" poly
       *          /        (depending on the angle of cp0 and cp3). This is
       *     cp2 o . . .   used to check whether any cps are within the poly,
       *         |     .   or if it intersects another hull.
       *         |     .
       *         |     .
       *   o - - o . . .
       *  cp0  cp1.   .
       *           . .
       *            .
       */


      List<Point> bounds = new List<Point>();
      int minDistance = friendly ? Frontline.MIN_DISTANCE_FROM_FRIENDLY : Frontline.MIN_DISTANCE_FROM_ENEMY;


      // calc angles

      double angleCp0Cp1 = Misc.AngleBetween( cp0.Location, cp1.Location );
      double angleCp1Cp2 = Misc.AngleBetween( cp1.Location, cp2.Location );
      double angleCp2Cp3 = Misc.AngleBetween( cp2.Location, cp3.Location );
      double rangeCp1 = Misc.AngleBetween( cp0.Location, cp1.Location, cp2.Location );
      double rangeCp2 = Misc.AngleBetween( cp1.Location, cp2.Location, cp3.Location );

      double angleMidCp1 = angleCp1Cp2 - ( rangeCp1 / 2 ) + 180;
      double angleMidCp2 = angleCp2Cp3 - ( rangeCp2 / 2 ) + 180;


      // calc point(s) around cp1

      bounds.Add( cp1.Location );

      if( rangeCp1 < 90 )
      {
        // hyp = adj / cos A
        double dist = minDistance / Math.Cos( ( 90 - rangeCp1 ) * ( Math.PI / 180 ) );
        bounds.Add( Misc.AngleOffset( cp1.Location, angleCp0Cp1, dist ) );
      }
      else if( rangeCp1 < 180 )
      {
        bounds.Add( Misc.AngleOffset( cp1.Location, angleCp1Cp2 + 90, minDistance ) );
      }
      else
      {
        bounds.Add( Misc.AngleOffset( cp1.Location, angleMidCp1, minDistance ) );
        bounds.Add( Misc.AngleOffset( cp1.Location, angleCp1Cp2 + 90, minDistance ) );
      }


      // calc point(s) around cp2

      if( rangeCp2 < 90 )
      {
        // hyp = adj / cos A
        double dist = minDistance / Math.Cos( ( 90 - rangeCp2 ) * ( Math.PI / 180 ) );
        bounds.Add( Misc.AngleOffset( cp2.Location, angleCp2Cp3 + 180, dist ) );
      }
      else if( rangeCp2 < 180 )
      {
        bounds.Add( Misc.AngleOffset( cp2.Location, angleCp1Cp2 + 90, minDistance ) );
      }
      else
      {
        bounds.Add( Misc.AngleOffset( cp2.Location, angleCp1Cp2 + 90, minDistance ) );
        bounds.Add( Misc.AngleOffset( cp2.Location, angleMidCp2, minDistance ) );
      }

      bounds.Add( cp2.Location );


      // convert to array

      Point[] array = new Point[bounds.Count];
      bounds.CopyTo( array );
      return array;
    }

    /// <summary>
    /// Check if the proposed frontline segment is too close to another cp.
    /// </summary>
    /// <param name="cp0">The ChokePoint before the start of the line.</param>
    /// <param name="cp1">The ChokePoint at the start of the line.</param>
    /// <param name="cp2">The ChokePoint at the end of the line.</param>
    /// <param name="cp3">The ChokePoint after the end of the line.</param>
    /// <returns>True if there is a cp nearby.</returns>
    private bool FriendlyNearLine( ChokePoint cp0, ChokePoint cp1, ChokePoint cp2, ChokePoint cp3 )
    {
      // calc poly

      Point[] poly = GetCollisionPoly( cp0, cp1, cp2, cp3, true );


      // check for cps within poly

      return IsChokePointsWithinPoly( poly );
    }

    /// <summary>
    /// Check if the proposed frontline segment is too close to the enemy.
    /// </summary>
    /// <param name="cp0">The ChokePoint before the start of the line.</param>
    /// <param name="cp1">The ChokePoint at the start of the line.</param>
    /// <param name="cp2">The ChokePoint at the end of the line.</param>
    /// <param name="cp3">The ChokePoint after the end of the line.</param>
    /// <returns>True if there is an enemy cp or frontline nearby.</returns>
    private bool EnemyNearLine( ChokePoint cp0, ChokePoint cp1, ChokePoint cp2, ChokePoint cp3 )
    {
      // calc poly

      Point[] poly = GetCollisionPoly( cp0, cp1, cp2, cp3, false );


      // check to see if poly intersects an enemy hull

      foreach( List<ChokePoint> hull in this.enemyHulls )
        if( HullIntersect( hull, poly ) )
          return true;


      // check for enemys within poly

      foreach( ChokePoint cpWithin in GetChokePointsWithinPoly( poly ) )
        if( IsEnemy( cpWithin ) )
          return true;


      return false;
    }

    /// <summary>
    /// Check if any angles in the proposed frontline segment are too sharp.
    /// </summary>
    /// <param name="cps">The ChokePoints to test (grouped into threes, eg, 012, 123, 234, etc).</param>
    /// <returns>True if any of the angles are less than 25deg.</returns>
    private bool AngleTooSharp( params ChokePoint[] cps )
    {
      for( int i = 0; i + 2 < cps.Length; i++ )
      {
        double angle = Misc.AngleBetween( cps[i].LocationOctets, cps[i + 1].LocationOctets, cps[i + 2].LocationOctets );
        if( angle < Frontline.MIN_ANGLE || angle > 360 - Frontline.MIN_ANGLE )
          return true;
      }

      return false;
    }

    #endregion

    #endregion
  }
}
