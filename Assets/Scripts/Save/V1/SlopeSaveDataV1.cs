using System.Collections.Generic;
using EPPZ.Geometry.Model;
using UnityEngine;

[System.Serializable]
public struct SlopeSaveDataV1 {
    public List<Vector2POD> vertices;
    public NavAreaGraphSaveDataV1 navData;

    // unused for now
    public SlopeDifficulty? manaulDifficulty;

    public static SlopeSaveDataV1 FromSlope(Slope slope, SavingContextV1 context) {
        SlopeSaveDataV1 result = new SlopeSaveDataV1();
        result.vertices = new List<Vector2POD>();
        result.navData = NavAreaGraphSaveDataV1.FromNavArea(slope.Footprint, context);

        foreach(Vertex vertex in slope.Footprint.Polygon.vertices) {
            result.vertices.Add(new Vector2(vertex.x, vertex.y));
        }

        return result;
    }
}