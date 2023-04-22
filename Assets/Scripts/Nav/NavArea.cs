using UnityEngine;
using System.Collections.Generic;

public class NavArea {
    public List<NavPortal> Portals;
    public List<NavLink> Links;
    public AlpinePolygon Polygon;
    public Building Owner;

    public NavArea() {
        Portals = new List<NavPortal>();
        Links = new List<NavLink>();
    }
}