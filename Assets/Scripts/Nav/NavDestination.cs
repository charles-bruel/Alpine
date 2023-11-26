using System.Collections.Generic;
using UnityEngine;

public class NavDestination : INavNode {
    public NavArea Area;
    public Vector3 Pos;

    public float GetHeight()
    {
        return Pos.y;
    }

    public List<NavLink> GetLinks()
    {
        List<NavLink> toReturn = new List<NavLink>();
        foreach(var link in Area.Links) {
            if(link.A == this || link.B == this) {
                toReturn.Add(link);
            }
        }
        return toReturn;
    }

    public Vector2 GetPosition()
    {
        return Pos.ToHorizontal();
    }
}