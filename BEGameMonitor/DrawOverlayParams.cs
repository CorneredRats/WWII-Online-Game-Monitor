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

using System.Windows.Forms;
using Microsoft.Win32;  // Registry
using Xiperware.WiretapAPI;

namespace BEGM
{
  /// <summary>
  /// Parameters used when drawing the game map overlay.
  /// </summary>
  public class DrawOverlayParams
  {
    #region Variables

    public bool showAttackObjectives = true;
    public bool showAttackObjectiveNames = false;
    public bool showFirebases = true;
    public bool showAirfields = true;
    public bool showSupplyLinks = false;
    public bool showBrigadeLinks = true;
    public bool showBrigadeLinksSelected = true;
    public bool showBrigadeLinksArmy = false;
    public bool showAirGrid = false;
    public bool showFrontlines = true;
    public bool showAttackArrows = false;
    public bool showAttackArrowsLinked = false;
    public bool showPlayerActivity = false;
    public bool showCountryBorders = false;
    public bool showCountryBordersNames = false;

    public ChokePoint selectedCP = null;
    public MapMetrics mm = null;

    #endregion

    #region Constructors



    #endregion

    #region Methods

    /// <summary>
    /// Populate this object with values stored in the registry.
    /// </summary>
    public void LoadFromRegistry( RegistryKey key )
    {
      string showAttackObjectives = (string)key.GetValue( "ShowAttackObjectives", "" );
      if( showAttackObjectives != "" ) this.showAttackObjectives = showAttackObjectives == "True";

      string showAttackObjectiveNames = (string)key.GetValue( "ShowAttackObjectiveNames", "" );
      if( showAttackObjectiveNames != "" ) this.showAttackObjectiveNames = showAttackObjectiveNames == "True";

      string showFirebases = (string)key.GetValue( "ShowFirebases", "" );
      if( showFirebases != "" ) this.showFirebases = showFirebases == "True";

      string showAirfields = (string)key.GetValue( "ShowAirfields", "" );
      if( showAirfields != "" ) this.showAirfields = showAirfields == "True";

      string showSupplyLinks = (string)key.GetValue( "ShowSupplyLinks", "" );
      if( showSupplyLinks != "" ) this.showSupplyLinks = showSupplyLinks == "True";

      string showBrigadeLinks = (string)key.GetValue( "ShowBrigadeLinks", "" );
      if( showBrigadeLinks != "" ) this.showBrigadeLinks = showBrigadeLinks == "True";

      string showBrigadeLinksSelected = (string)key.GetValue( "ShowBrigadeLinksSelected", "" );
      if( showBrigadeLinksSelected != "" ) this.showBrigadeLinksSelected = showBrigadeLinksSelected == "True";

      string showBrigadeLinksArmy = (string)key.GetValue( "ShowBrigadeLinksArmy", "" );
      if( showBrigadeLinksArmy != "" ) this.showBrigadeLinksArmy = showBrigadeLinksArmy == "True";

      string showAirGrid = (string)key.GetValue( "ShowAirGrid", "" );
      if( showAirGrid != "" ) this.showAirGrid = showAirGrid == "True";

      string showFrontlines = (string)key.GetValue( "ShowFrontlines", "" );
      if( showFrontlines != "" ) this.showFrontlines = showFrontlines == "True";

      string showAttackArrows = (string)key.GetValue( "ShowAttackArrows", "" );
      if( showAttackArrows != "" ) this.showAttackArrows = showAttackArrows == "True";

      string showAttackArrowsLinked = (string)key.GetValue( "ShowAttackArrowsLinked", "" );
      if( showAttackArrowsLinked != "" ) this.showAttackArrowsLinked = showAttackArrowsLinked == "True";

      string showPlayerActivity = (string)key.GetValue( "ShowPlayerActivity", "" );
      if( showPlayerActivity != "" ) this.showPlayerActivity = showPlayerActivity == "True";

      string showCountryBorders = (string)key.GetValue( "ShowCountryBorders", "" );
      if( showCountryBorders != "" ) this.showCountryBorders = showCountryBorders == "True";

      string showCountryBordersNames = (string)key.GetValue( "ShowCountryBordersNames", "" );
      if( showCountryBordersNames != "" ) this.showCountryBordersNames = showCountryBordersNames == "True";
    }

    /// <summary>
    /// Save the values in this object to the registry.
    /// </summary>
    public void SaveToRegistry( RegistryKey key )
    {
      key.SetValue( "ShowAttackObjectives", this.showAttackObjectives );
      key.SetValue( "ShowAttackObjectiveNames", this.showAttackObjectiveNames );
      key.SetValue( "ShowFirebases", this.showFirebases );
      key.SetValue( "ShowAirfields", this.showAirfields );
      key.SetValue( "ShowSupplyLinks", this.showSupplyLinks );
      key.SetValue( "ShowBrigadeLinks", this.showBrigadeLinks );
      key.SetValue( "ShowBrigadeLinksSelected", this.showBrigadeLinksSelected );
      key.SetValue( "ShowBrigadeLinksArmy", this.showBrigadeLinksArmy );
      key.SetValue( "ShowAirGrid", this.showAirGrid );
      key.SetValue( "ShowFrontlines", this.showFrontlines );
      key.SetValue( "ShowAttackArrows", this.showAttackArrows );
      key.SetValue( "ShowAttackArrowsLinked", this.showAttackArrowsLinked );
      key.SetValue( "ShowPlayerActivity", this.showPlayerActivity );
      key.SetValue( "ShowCountryBorders", this.showCountryBorders );
      key.SetValue( "ShowCountryBordersNames", this.showCountryBordersNames );
    }

    /// <summary>
    /// Updates a TreeView control with the values in this object.
    /// </summary>
    public void UpdateControl( TreeView tvwMapOptions )
    {
#if !MAC  ////
      tvwMapOptions.Nodes["nodeAttackObjectives"].Checked = this.showAttackObjectives;
      tvwMapOptions.Nodes["nodeAttackObjectives"].Nodes["nodeAttackObjectiveNames"].Checked = this.showAttackObjectiveNames;
      tvwMapOptions.Nodes["nodeFirebases"].Checked = this.showFirebases;
      tvwMapOptions.Nodes["nodeAirfields"].Checked = this.showAirfields;
      tvwMapOptions.Nodes["nodeSupplyLinks"].Checked = this.showSupplyLinks;
      tvwMapOptions.Nodes["nodeBrigadeLinks"].Checked = this.showBrigadeLinks;
      if( tvwMapOptions.Nodes["nodeBrigadeLinks"].Nodes["nodeBrigadeLinksSelected"] != null )
        tvwMapOptions.Nodes["nodeBrigadeLinks"].Nodes["nodeBrigadeLinksSelected"].Checked = this.showBrigadeLinksSelected;
      tvwMapOptions.Nodes["nodeBrigadeLinks"].Nodes["nodeBrigadeLinksArmy"].Checked = this.showBrigadeLinksArmy;
      tvwMapOptions.Nodes["nodeAirGrid"].Checked = this.showAirGrid;
      tvwMapOptions.Nodes["nodeFrontlines"].Checked = this.showFrontlines;
      tvwMapOptions.Nodes["nodeAttackArrows"].Checked = this.showAttackArrows;
      tvwMapOptions.Nodes["nodeAttackArrows"].Nodes["nodeAttackArrowsLinked"].Checked = this.showAttackArrowsLinked;
      tvwMapOptions.Nodes["nodePlayerActivity"].Checked = this.showPlayerActivity;
      tvwMapOptions.Nodes["nodeCountryBorders"].Checked = this.showCountryBorders;
      tvwMapOptions.Nodes["nodeCountryBorders"].Nodes["nodeCountryBordersNames"].Checked = this.showCountryBordersNames;
#endif
    }

    /// <summary>
    /// Tests whether the given DrawOverlayParams are equivalent.
    /// </summary>
    /// <param name="other">The object to compare.</param>
    /// <returns>False if changes exist that would draw differently.</returns>
    public bool IsEquivalent( DrawOverlayParams other )
    {
      if( this.showAttackObjectives != other.showAttackObjectives ) return false;
      if( ( this.showAttackObjectives && this.showAttackObjectiveNames ) != ( other.showAttackObjectives && other.showAttackObjectiveNames ) ) return false;
      if( this.showFirebases != other.showFirebases ) return false;
      if( this.showAirfields != other.showAirfields ) return false;
      if( this.showSupplyLinks != other.showSupplyLinks ) return false;
      if( this.showBrigadeLinks != other.showBrigadeLinks ) return false;
      if( ( this.showBrigadeLinks && this.showBrigadeLinksSelected ) != ( other.showBrigadeLinks && other.showBrigadeLinksSelected ) ) return false;
      if( ( this.showBrigadeLinks && this.showBrigadeLinksArmy ) != ( other.showBrigadeLinks && other.showBrigadeLinksArmy ) ) return false;
      if( this.showAirGrid != other.showAirGrid ) return false;
      if( this.showFrontlines != other.showFrontlines ) return false;
      if( this.showAttackArrows != other.showAttackArrows ) return false;
      if( ( this.showAttackArrows && this.showAttackArrowsLinked ) != ( other.showAttackArrows && other.showAttackArrowsLinked ) ) return false;
      if( this.showPlayerActivity != other.showPlayerActivity ) return false;
      if( this.showCountryBorders != other.showCountryBorders ) return false;
      if( ( this.showCountryBorders && this.showCountryBordersNames ) != ( other.showCountryBorders && other.showCountryBordersNames ) ) return false;

      return true;
    }

    /// <summary>
    /// Returns a value indicating whether this instance is equal to a specified object value.
    /// </summary>
    /// <param name="obj">An object to compare with this instance.</param>
    /// <returns>True if obj is an equivalent DrawOverlayParams.</returns>
    public override bool Equals( object obj )
    {
      if( !( obj is DrawOverlayParams ) ) return false;
      DrawOverlayParams other = (DrawOverlayParams)obj;

      if( this.showAttackObjectives != other.showAttackObjectives ) return false;
      if( this.showAttackObjectiveNames != other.showAttackObjectiveNames ) return false;
      if( this.showFirebases != other.showFirebases ) return false;
      if( this.showAirfields != other.showAirfields ) return false;
      if( this.showSupplyLinks != other.showSupplyLinks ) return false;
      if( this.showBrigadeLinks != other.showBrigadeLinks ) return false;
      if( this.showBrigadeLinksSelected != other.showBrigadeLinksSelected ) return false;
      if( this.showBrigadeLinksArmy != other.showBrigadeLinksArmy ) return false;
      if( this.showAirGrid != other.showAirGrid ) return false;
      if( this.showFrontlines != other.showFrontlines ) return false;
      if( this.showAttackArrows != other.showAttackArrows ) return false;
      if( this.showAttackArrowsLinked != other.showAttackArrowsLinked ) return false;
      if( this.showPlayerActivity != other.showPlayerActivity ) return false;
      if( this.showCountryBorders != other.showCountryBorders ) return false;
      if( this.showCountryBordersNames != other.showCountryBordersNames ) return false;

      return true;
    }

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    /// <returns>Object.GetHashCode().</returns>
    public override int GetHashCode()
    {
      return base.GetHashCode();
    }

    /// <summary>
    /// Support for the == operator.
    /// </summary>
    public static bool operator ==( DrawOverlayParams a, DrawOverlayParams b )
    {
      if( (object)a == null ) return (object)b == null;
      return a.Equals( b );
    }

    /// <summary>
    /// Support for the != operator.
    /// </summary>
    public static bool operator !=( DrawOverlayParams a, DrawOverlayParams b )
    {
      if( (object)a == null ) return (object)b != null;
      return !a.Equals( b );
    }

    #endregion
  }
}
