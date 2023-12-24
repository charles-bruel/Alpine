using System;
using System.Collections.Generic;
using EPPZ.Geometry.Model;
using UnityEngine;

[System.Serializable]
public struct SlopeSaveDataV1 {
    public List<Vector2POD> Vertices;
    public NavAreaGraphSaveDataV1 NavAreaGraphs;

    // unused for now
    public SlopeDifficulty? ManualDifficulty;

    public static SlopeSaveDataV1 FromSlope(Slope slope, SavingContextV1 context) {
        SlopeSaveDataV1 result = new SlopeSaveDataV1();
        result.Vertices = new List<Vector2POD>();
        result.NavAreaGraphs = NavAreaGraphSaveDataV1.FromNavArea(slope.Footprint, context);

        foreach(Vertex vertex in slope.Footprint.Polygon.vertices) {
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