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
using UnityEngine;

public class SimpleBuildingTemplate : MonoBehaviour, IUIToolbarItemProvider {
    public AlpinePolygonSource[] Polygons;
    public NavDestinationDefinition FunctionalityNode;
    public BuildingFunctionality Functionality;
    public Sprite BuildingIcon;
    

    [Header("2D Icon")]
    public Sprite Icon2D;
    public Vector2 IconSize;

    public Sprite GetSprite() {
        return BuildingIcon;
    }

    void OnDrawGizmos() {
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
        if(FunctionalityNode.PolygonDefinitionID < Polygons.Length) {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(FunctionalityNode.Pos.Inflate3rdDim(Polygons[FunctionalityNode.PolygonDefinitionID].Height), 1);
        }
    }

    public void OnToolEnabled(UIReferences uiReferences) {
        BuildingBuilderTool tool = new()
        {
            Template = this,
            WorldUICanvas = uiReferences.WorldCanvas
        };
        InterfaceController.Instance.SelectedTool = tool;
    }
}