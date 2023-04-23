using UnityEngine;
using System.Collections.Generic;

public class NavArea : AlpinePolygon {
    public List<NavPortal> Portals;
    public List<NavLink> Links;
    public Building Owner;
    public bool Modified = false;

    public NavArea() {
        Portals = new List<NavPortal>();
        Links = new List<NavLink>();
    }
}