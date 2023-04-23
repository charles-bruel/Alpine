using UnityEngine;
using System.Collections.Generic;

public class Slope : Building {
    public SlopeConstructionData Data;
    public NavArea Footprint;

    public void Inflate(List<NavPortal> portals) {
        Footprint.Portals.AddRange(portals);

        foreach(NavPortal portal in portals) {
            GameObject temp = new GameObject();
            temp.transform.SetParent(transform);
            temp.name = "Portal";
            temp.layer = LayerMask.NameToLayer("2D");

            portal.gameObject = temp;
            portal.Inflate();

            portal.A.Portals.Add(portal);
            portal.B.Portals.Add(portal);
        }
    }
}