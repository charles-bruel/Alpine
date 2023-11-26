using System;
using System.Collections.Generic;
using UnityEngine;

public class SimpleBuildingTemplate : MonoBehaviour {
    public AlpinePolygonSource[] Polygons;
    public NavDestinationDefinition ServiceNode;
    public BuildingFunctionality Functionality;

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
        if(ServiceNode.PolygonDefinitionID < Polygons.Length) {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(ServiceNode.Pos.Inflate3rdDim(Polygons[ServiceNode.PolygonDefinitionID].Height), 1);
        }
    }

}