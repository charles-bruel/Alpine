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

public class PolygonBuilder {
    public PolygonConstructionData Data;
    public PolygonBuilding Result;

    public void Initialize(PolygonBuilding result, PolygonFlags flags) {
        Result = result;
        Result.Data = Data;

        Data = new PolygonConstructionData();

        Result.Footprint = new NavArea();
        Result.Footprint.Owner = Result;

        Result.Footprint.Guid = Guid.NewGuid();
        Result.Footprint.Selectable = false;
        Result.Footprint.Level = 3;
        Result.Footprint.Flags = flags;
        Result.Footprint.Color = RenderingData.Instance.SlopeDraftColor;
    }

    public void LightBuild() {
        if(Data.SlopePoints.Count > 2) {
            Result.Footprint.Polygon = Polygon.PolygonWithPoints(Data.GetPoints());
            if(Result.Footprint.Polygon.area < 0) {
                Result.Footprint.Polygon.Reverse();
            }
        }
    }

    public void Build() {
        LightBuild();
    }

    public static List<NavPortal> PlacePortals(NavArea selfNavArea, PolygonConstructionData data) {
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

        for(int i = 0;i < data.SlopePoints.Count;i ++) {
            PolygonConstructionData.SlopePoint current = data.SlopePoints[i];
            int nextIndex;
            if(i == data.SlopePoints.Count - 1) {
                nextIndex = 0;
            } else {
                nextIndex = i + 1;
            }
            PolygonConstructionData.SlopePoint next = data.SlopePoints[nextIndex];
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
                portal.A = selfNavArea;
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
        Result.Inflate(PlacePortals(Result.Footprint, Data));

        PolygonsController.Instance.RegisterPolygon(Result.Footprint);
        BuildingsController.Instance.RegisterBuilding(Result);
    }

    public void Cancel() {
        //TODO: More elegant solution
        if(Result.Footprint.Filter != null && Result.Footprint.Filter.gameObject != null) GameObject.Destroy(Result.Footprint.Filter.gameObject);
        PolygonsController.Instance.MarkPolygonsDirty();
        GameObject.Destroy(Result.gameObject);
    }

    public static void FindSnapping(float epsilon, NavArea selfNavArea, PolygonConstructionData data) {
        for(int i = 0;i < data.SlopePoints.Count;i ++) {
            PolygonsController.PolygonSnappingResult? snapping = PolygonsController.Instance.CheckForSnapping(data.SlopePoints[i].Pos, epsilon, epsilon, PolygonFlags.NAVIGABLE_MASK, selfNavArea);
            if(snapping != null) {
                data.SlopePoints[i] = new PolygonConstructionData.SlopePoint(snapping.Value.Pos, snapping.Value);
            }
        }
    }

    // TODO: Refactor out the nav area implementation initialization. Also possibly make it consistent?
    public static Slope BuildFromSave(PolygonConstructionData data, NavAreaGraphSaveDataV1 navData, SlopeDifficultySetting currentDifficulty, SlopeDifficulty intrinsicDifficulty, LoadingContextV1 loadingContext) {
        PolygonBuilder builder = new PolygonBuilder();

        GameObject gameObject = new GameObject("Slope");
        Slope slope = gameObject.AddComponent<Slope>();
        builder.Initialize(slope, PolygonFlags.CLEARANCE | PolygonFlags.SLOPE_NAVIGABLE);
        builder.Data = data;
        
        slope.AreaImplementation = new SlopeNavAreaImplementation(slope, default(Rect));
        slope.Footprint.Implementation = slope.AreaImplementation;

        builder.Build();
        builder.Finish();

        builder.Result.Footprint.ID = navData.ID;
        loadingContext.navAreas.Add(navData.ID, builder.Result.Footprint);

        slope.IntrinsicDifficulty = intrinsicDifficulty;
        slope.CurrentDifficultySetting = currentDifficulty;
        slope.UpdateDifficulty();

        return slope;
    }

    public static Snowfront BuildFromSave(PolygonConstructionData data, NavAreaGraphSaveDataV1 navData, LoadingContextV1 loadingContext) {
        PolygonBuilder builder = new PolygonBuilder();
        
        GameObject gameObject = new GameObject("Snowfront");
        Snowfront snowfront = gameObject.AddComponent<Snowfront>();
        builder.Initialize(snowfront, PolygonFlags.CLEARANCE | PolygonFlags.FLAT_NAVIGABLE);
        builder.Data = data;

        builder.Build();

        NavArea temp = new NavArea();

        temp.Guid                = snowfront.Footprint.Guid;
        temp.Level               = snowfront.Footprint.Level;
        temp.Polygon             = snowfront.Footprint.Polygon;
        temp.Filter              = snowfront.Footprint.Filter;
        temp.Renderer            = snowfront.Footprint.Renderer;
        temp.Color               = snowfront.Footprint.Color;
        temp.ArbitrarilyEditable = snowfront.Footprint.ArbitrarilyEditable;
        temp.Flags               = snowfront.Footprint.Flags;
        temp.Height              = snowfront.Footprint.Height;

        temp.Implementation = new SnowfrontNavAreaImplementation(snowfront);

        temp.Owner = snowfront;

        snowfront.Footprint = temp;


        builder.Finish();

        builder.Result.Footprint.ID = navData.ID;
        loadingContext.navAreas.Add(navData.ID, builder.Result.Footprint);

        return snowfront;
    }
}