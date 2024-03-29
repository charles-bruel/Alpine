using System;
using System.Collections.Generic;
using EPPZ.Geometry.Model;
using UnityEngine;

[System.Serializable]
public struct SlopeSaveDataV1 {
    public List<Vector2POD> Vertices;
    public NavAreaGraphSaveDataV1 NavAreaGraphs;
    public SlopeDifficultySetting CurrentDifficulty;
    public SlopeDifficulty IntrinsicDifficulty;

    public static SlopeSaveDataV1 FromSlope(Slope slope, SavingContextV1 context) {
        SlopeSaveDataV1 result = new SlopeSaveDataV1();
        result.Vertices = new List<Vector2POD>();
        result.NavAreaGraphs = NavAreaGraphSaveDataV1.FromNavArea(slope.Footprint, context);
        result.CurrentDifficulty = slope.CurrentDifficultySetting;
        result.IntrinsicDifficulty = slope.IntrinsicDifficulty;

        foreach(Vertex vertex in slope.Footprint.Polygon.vertices) {
            result.Vertices.Add(new Vector2(vertex.x, vertex.y));
        }

        return result;
    }

    public PolygonConstructionData ToConstructionData() {
        PolygonConstructionData result = new PolygonConstructionData();
        result.SlopePoints = new List<PolygonConstructionData.SlopePoint>();

        foreach(Vector2POD point in Vertices) {
            result.SlopePoints.Add(new PolygonConstructionData.SlopePoint(point));
        }

        return result;
    }
}