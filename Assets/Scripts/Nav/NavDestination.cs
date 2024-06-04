//>============================================================================<
//
//    Alpine, Ski Resort Tycoon Game
//    Copyright (C) 2024  Charles Bruel
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see <https://www.gnu.org/licenses/>.
//>============================================================================<

using System.Collections.Generic;
using UnityEngine;

public class NavDestination : INavNode {
    public NavArea Area;
    public Vector2 Pos;
    
    public List<NavLink> ExplicitNavLinks = new List<NavLink>();

    public float GetHeight() {
        return Area.Height;
    }

    public List<NavLink> GetLinks() {
        List<NavLink> toReturn = new List<NavLink>();
        foreach(var link in Area.Links) {
            if(link.A == this || link.B == this) {
                toReturn.Add(link);
            }
        }
        toReturn.AddRange(ExplicitNavLinks);
        return toReturn;
    }

    public Vector2 GetPosition() {
        return Pos;
    }

    public void AddExplictNavLink(NavLink link) {
        ExplicitNavLinks.Add(link);
        GlobalNavController.MarkGraphDirty();
    }

    public void RemoveExplicitNavLink(NavLink link) {
        ExplicitNavLinks.Remove(link);
        GlobalNavController.MarkGraphDirty();
    }

    public List<NavLink> GetExplicitLinksForSerialization() {
        List<NavLink> toReturn = new List<NavLink>();

        foreach(var link in ExplicitNavLinks) {
            if(link.A == this) {
                toReturn.Add(link);
            }
        }

        return toReturn;
    }

    private bool Dead = false;

    public void Destroy() {
        Area.Nodes.Remove(this);

        Area.Modified = true;

        Dead = true;
    }

    public bool IsDead() {
        return Dead;
    }
}