using System;
using System.Collections.Generic;
using EPPZ.Geometry.Model;
using UnityEngine;

public class SlopeBuilder {
    public SlopeConstructionData Data;

    public Slope Result;

    public void Initialize() {
        GameObject gameObject = new GameObject("Slope");
        Result = gameObject.AddComponent<Slope>();
        Result.Data = Data;
        BuildingsController.Instance.RegisterBuilding(Result);

        Data = new SlopeConstructionData();

        Result.Footprint = new AlpinePolygon();

        Result.Footprint.Guid = Guid.NewGuid();
        Result.Footprint.Level = 3;
        Result.Footprint.Flags = PolygonFlags.CLEARANCE | PolygonFlags.SLOPE_NAVIGABLE;
        Result.Footprint.Color = RenderingData.Instance.SlopeDraftColor;
    }

    public void LightBuild() {
        if(Data.SlopePoints.Count > 2) {
            Result.Footprint.Polygon = Polygon.PolygonWithPoints(Data.SlopePoints.ToArray());
        }
    }

    public void Build() {
        LightBuild();
    }

    public void Finish() {
        PolygonsController.Instance.RegisterPolygon(Result.Footprint);
    }

    public void Cancel() {
        //TODO: More elegant solution
        if(Result.Footprint.Filter != null && Result.Footprint.Filter.gameObject != null) GameObject.Destroy(Result.Footprint.Filter.gameObject);
        PolygonsController.Instance.MarkPolygonsDirty();
        GameObject.Destroy(Result.gameObject);
    }
}