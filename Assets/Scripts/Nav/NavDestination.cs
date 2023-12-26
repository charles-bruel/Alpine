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
}