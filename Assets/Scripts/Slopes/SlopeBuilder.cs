using System;
using System.Collections.Generic;
using EPPZ.Geometry.Model;
using UnityEngine;

public class SlopeBuilder {
    public SlopeConstructionData Data;

    private Slope Result;

    public void Initialize() {
        GameObject gameObject = new GameObject("Slope");
        Result = gameObject.AddComponent<Slope>();
        Result.Data = Data;
        BuildingsController.Instance.RegisterBuilding(Result);

        Data = new SlopeConstructionData();

        Result.Footprint = new AlpinePolygon();

        Result.Footprint.Guid = Guid.NewGuid();
        Result.Footprint.Level = 4;
        Result.Footprint.Flags = PolygonFlags.CLEARANCE;
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
}