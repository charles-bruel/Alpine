using UnityEngine;
using System.Collections.Generic;

public class Slope : Building {
    public SlopeConstructionData Data;
    public AlpinePolygon Footprint;
    public NavArea Area;

    public void Inflate(List<NavPortal> portals) {
        Area = new NavArea();
        Area.Owner = this;
        Area.Polygon = Footprint;
        Area.Portals.AddRange(portals);

        foreach(NavPortal portal in portals) {
            GameObject temp = new GameObject();
            temp.transform.SetParent(transform);
            temp.name = "Portal";
            temp.layer = LayerMask.NameToLayer("2D");

            portal.gameObject = temp;
            portal.Inflate();
        }
    }
}