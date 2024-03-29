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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiftSegmentTemplate : MonoBehaviour, IPoolable, ICustomScriptable {
    public float Gauge;
    public Transform CableAimingPoint;
    public Transform UphillCablePoint;
    public Transform DownhillCablePoint;
    public AlpinePolygonSource[] Polygons;
    public APIDef LiftSegmentAPIDef;
    public APILiftSegment APILiftSegment;
    public Dictionary<string, object> CustomScriptPersistentData = new Dictionary<string, object>();

    private void Initialize() {
        if(APILiftSegment != null) return;

        APILiftSegment = LiftSegmentAPIDef.Fetch<APILiftSegment>();
    }

    public virtual IPoolable Clone() {
        Initialize();
        var temp = GameObject.Instantiate(this);
        temp.APILiftSegment = APILiftSegment;
        return temp;
    }

    public void Destroy() {
        GameObject.Destroy(this);
    }

    public void Disable() {
        gameObject.SetActive(false);
    }

    public void Enable() {
        gameObject.SetActive(true);
    }

    protected void OnDrawGizmos() {
        if(Polygons == null) return;
        for(int i = 0;i < Polygons.Length;i ++) {
            AlpinePolygonSource poly = Polygons[i];
            if(poly.Points.Length == 0) continue;
            Gizmos.color = PolygonsController.ColorFromFlags(poly.Flags);
            for(int j = 1;j < poly.Points.Length;j ++) {
                Gizmos.DrawLine(
                    new Vector3(poly.Points[j - 1].x, poly.Height, poly.Points[j - 1].y),
                    new Vector3(poly.Points[j].x, poly.Height, poly.Points[j].y)
                );
            }
            int final = poly.Points.Length - 1;
            Gizmos.DrawLine(
                new Vector3(poly.Points[final].x, poly.Height, poly.Points[final].y),
                new Vector3(poly.Points[0].x, poly.Height, poly.Points[0].y)
            );
        }
    }

    public Dictionary<string, object> PersistentData()
    {
        return CustomScriptPersistentData;
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }
}
