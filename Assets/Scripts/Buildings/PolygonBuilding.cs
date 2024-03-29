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

public abstract class PolygonBuilding : Building {
    public NavArea Footprint;
    public PolygonConstructionData Data;
    
    public override void Advance(float delta) {
        if(Footprint != null) {
            Footprint.Advance(delta);
        }
    }

    public void Inflate(List<NavPortal> portals) {
        foreach(NavPortal portal in portals) {
            GameObject temp = new GameObject();
            temp.transform.SetParent(transform);
            temp.name = "Portal";
            temp.layer = LayerMask.NameToLayer("2D");

            portal.gameObject = temp;
            portal.Inflate();

            portal.A.Nodes.Add(portal);
            portal.A.Modified = true;

            portal.B.Nodes.Add(portal);
            portal.B.Modified = true;
        }
    }

    public override void Destroy() {
        PolygonsController.Instance.DestroyPolygon(Footprint);

        base.Destroy();
    }
}