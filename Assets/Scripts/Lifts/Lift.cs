using UnityEngine;
using System;
using System.Collections.Generic;

public class Lift : MonoBehaviour {
    public LiftTemplate Template;
    public LiftConstructionData Data;
    public PolygonsController.AlpinePolygon Footprint;
    public LineRenderer Line;
    public LiftCablePoint[] CablePoints;
    public LiftVehicleSystem VehicleSystem;

    private bool Initialized;

    public void Initialize() {
        VehicleSystem = new LiftVehicleSystem();
        VehicleSystem.Parent = this;
        VehicleSystem.TemplateVehicle = Data.SelectedVehicle;

        Initialized = true;
    }

    public void Finish(PolygonsController.AlpinePolygon Footprint) {
        this.Footprint = Footprint;
        
        VehicleSystem.Initialize();
    }

    public void CreateSubObjects() {
        GameObject gameObject = new GameObject("Line");
        gameObject.transform.SetParent(transform, true);
        gameObject.layer = LayerMask.NameToLayer("2D");
        Line = gameObject.AddComponent<LineRenderer>();

        Line.widthMultiplier = 5f;
        Line.material = RenderingData.Instance.LiftLineMaterial;
        Line.startColor = Color.red;
        Line.endColor = Color.red;
    }
}