using System;
using System.Collections.Generic;
using EPPZ.Geometry.Model;
using UnityEngine;

[System.Serializable]
public struct SnowfrontSaveDataV1 {
    public List<Vector2POD> Vertices;
    public NavAreaGraphSaveDataV1 NavAreaGraphs;

    public static SnowfrontSaveDataV1 FromSnowfront(Snowfront snowfront, SavingContextV1 context) {
        SnowfrontSaveDataV1 result = new SnowfrontSaveDataV1();
        result.Vertices = new List<Vector2POD>();
        result.NavAreaGraphs = NavAreaGraphSaveDataV1.FromNavArea(snowfront.Footprint, context);

        foreach(Vertex vertex in snowfront.Footprint.Polygon.vertices) {
            result.Vertices.Add(new Vector2(vertex.x, vertex.y));
        }

        return result;
    }

    public SlopeConstructionData ToConstructionData() {
        SlopeConstructionData result = new SlopeConstructionData();
        result.SlopePoints = new List<SlopeConstructionData.SlopePoint>();

        foreach(Vector2POD point in Vertices) {
            result.SlopePoints.Add(new SlopeConstructionData.SlopePoint(point));
        }

        return result;
    }
}