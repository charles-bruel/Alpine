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

using System;
using System.Collections.Generic;
using EPPZ.Geometry.Model;
using UnityEngine;

public class PolygonEditor : MonoBehaviour {
    public PolygonsController Controller;
    public Canvas Canvas;
    public GameObject GrabTemplate;
    public List<PolygonEditorGrab> Grabs;


    public void Reinflate() {
        Reset();

        Guid selectedGuid = Controller.SelectedPolygon;
        AlpinePolygon polygon = null;
        for(int i = 0;i < Controller.PolygonObjects.Count;i ++) {
            if(Controller.PolygonObjects[i].Guid == selectedGuid) {
                polygon = Controller.PolygonObjects[i];
                break;
            }
        }

        if(polygon == null) {
            //Nothing is selected
            return;
        }

        if(!polygon.ArbitrarilyEditable) {
            //Not allowed to edit
            return;
        }

        for(int i = 0;i < polygon.Polygon.vertexCount;i ++) {
            //Get or initialize grab
            PolygonEditorGrab grab;
            if(i == Grabs.Count) {
                GameObject temp = Instantiate<GameObject>(GrabTemplate);
                temp.transform.SetParent(Canvas.transform, false);
                grab = temp.GetComponent<PolygonEditorGrab>();

                grab.VertexIndex = i;
                grab.Editor = this;

                Grabs.Add(grab);
            } else {
                grab = Grabs[i];
                grab.gameObject.SetActive(true);
            }

            grab.RectTransform.anchoredPosition = polygon.Polygon.vertices[i].point;
            grab.polygon = polygon;
        }
    }

    private void Reset() {
        foreach(PolygonEditorGrab grab in Grabs) {
            grab.gameObject.SetActive(false);
        }
    }

    public void MoveAll() {
        
    }
}
