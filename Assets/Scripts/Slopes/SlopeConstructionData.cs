using UnityEngine;
using System.Collections.Generic;

public class SlopeConstructionData {
    public SlopeConstructionData() {
        SlopePoints = new List<SlopePoint>();
    }

    //TODO: Support polygons with holes, etc.
    public List<SlopePoint> SlopePoints;

    public Vector2[] GetPoints() {
        Vector2[] toReturn = new Vector2[SlopePoints.Count];
        for(int i = 0;i < toReturn.Length;i ++) {
            toReturn[i] = SlopePoints[i].Pos;
        }
        return toReturn;
    }

    public struct SlopePoint {
        public SlopePoint(Vector2 pos) {
            Pos = pos;
            Snapping = null;
        }
        public SlopePoint(Vector2 pos, PolygonsController.PolygonSnappingResult snapping) {
            Pos = pos;
            Snapping = snapping;
        }
        public Vector2 Pos;
        public PolygonsController.PolygonSnappingResult? Snapping;
    }
}