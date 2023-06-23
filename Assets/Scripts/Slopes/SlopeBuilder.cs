using System;
using System.Collections.Generic;
using EPPZ.Geometry.Model;
using UnityEngine;

public class SlopeBuilder {
    public SlopeConstructionData Data;

    public Slope Result;
    public List<PolygonsController.PolygonSnappingResult> SnappedPoints;

    public void Initialize() {
        GameObject gameObject = new GameObject("Slope");
        Result = gameObject.AddComponent<Slope>();
        Result.Data = Data;
        BuildingsController.Instance.RegisterBuilding(Result);

        Data = new SlopeConstructionData();
        SnappedPoints = new List<PolygonsController.PolygonSnappingResult>();

        Result.Footprint = new NavArea();
        Result.Footprint.Owner = Result;

        Result.Footprint.Guid = Guid.NewGuid();
        Result.Footprint.Selectable = false;
        Result.Footprint.Level = 3;
        Result.Footprint.Flags = PolygonFlags.CLEARANCE | PolygonFlags.SLOPE_NAVIGABLE;
        Result.Footprint.Color = RenderingData.Instance.SlopeDraftColor;
        
        Result.AreaImplementation = new SlopeNavAreaImplementation(Result, default(Rect));
        Result.Footprint.Implementation = Result.AreaImplementation;
    }

    public void LightBuild() {
        if(Data.SlopePoints.Count > 2) {
            Result.Footprint.Polygon = Polygon.PolygonWithPoints(Data.GetPoints());
        }
    }

    public void Build() {
        LightBuild();
    }

    private List<NavPortal> PlacePortals() {
        List<NavPortal> toReturn = new List<NavPortal>();

        // Portals occur along straight segments of the polygon
        // If in x_y x is the point and y is the offset,
        // they are either of the form a_x to a_y or a_x to b_0
        // where b = a + 1.

        // Since the points in this polygon will have been snapped,
        // the portals would be of the form a_0 to b_0, where b = a + 1.

        // We first verify that it is valid on this polygon, then check
        // the snapped polygon.
        // Because the form is guarunteed, we only have to track the
        // start indices
        List<int> validPortals = new List<int>();

        for(int i = 0;i < Data.SlopePoints.Count;i ++) {
            SlopeConstructionData.SlopePoint current = Data.SlopePoints[i];
            int nextIndex;
            if(i == Data.SlopePoints.Count - 1) {
                nextIndex = 0;
            } else {
                nextIndex = i + 1;
            }
            SlopeConstructionData.SlopePoint next = Data.SlopePoints[nextIndex];
            if(current.Snapping.HasValue && next.Snapping.HasValue) {
                PolygonsController.PolygonSnappingResult currentSnap = current.Snapping.Value;
                PolygonsController.PolygonSnappingResult nextSnap = next.Snapping.Value;

                // First we have to verify the portal goes to the same polygon
                if(currentSnap.Target != nextSnap.Target) continue;

                if(!(currentSnap.Target is NavArea)) continue;

                bool valid = false;
                // There are three ways for it to be a valid snap
                // 1) the a_x to a_y case as described above
                // 2) the a_x to b_0 case as described above
                // 3) a b_0 to a_x, again with b = a + 1
                // If any are valid, valid = true

                // Case 1
                if(currentSnap.PointID == nextSnap.PointID) valid = true;

                // Case 2
                if(nextSnap.PointID == currentSnap.PointID + 1 && nextSnap.Offset == 0) valid = true;
                if(nextSnap.PointID == 0 && currentSnap.PointID == currentSnap.Target.Polygon.pointCount - 1 && nextSnap.Offset == 0) valid = true;

                // Case 3
                if(currentSnap.PointID == nextSnap.PointID + 1 && currentSnap.Offset == 0) valid = true;
                if(currentSnap.PointID == 0 && nextSnap.PointID == nextSnap.Target.Polygon.pointCount - 1 && currentSnap.Offset == 0) valid = true;

                if(!valid) continue;

                // We've found a valid portal solution, time to generate it
                NavPortal portal = new NavPortal();
                portal.A = Result.Footprint;
                portal.A1Index = i;
                portal.A1Offset = 0;
                portal.A2Index = nextIndex;
                portal.A2Offset = 0;

                portal.B = currentSnap.Target as NavArea;
                portal.B1Index = currentSnap.PointID;
                portal.B1Offset = currentSnap.Offset;
                portal.B2Index = nextSnap.PointID;
                portal.B2Offset = nextSnap.Offset;

                // We need to get the center and then get the height there
                Vector2 centerCoord = (portal.A1 + portal.A2 + portal.B1 + portal.B2) / 4;
                portal.Height = TerrainManager.Instance.Project(centerCoord).y;
                
                toReturn.Add(portal);
            }
        }
        return toReturn;
    }

    public void Finish() {
        Result.Inflate(PlacePortals());

        PolygonsController.Instance.RegisterPolygon(Result.Footprint);
    }

    public void Cancel() {
        //TODO: More elegant solution
        if(Result.Footprint.Filter != null && Result.Footprint.Filter.gameObject != null) GameObject.Destroy(Result.Footprint.Filter.gameObject);
        PolygonsController.Instance.MarkPolygonsDirty();
        GameObject.Destroy(Result.gameObject);
    }
}