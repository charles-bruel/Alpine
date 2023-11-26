using UnityEngine;
using System;
using System.Collections.Generic;

public class Lift : Building {
    public LiftTemplate Template;
    public LiftConstructionData Data;
    public AlpinePolygon Footprint;
    public LineRenderer Line;
    public LiftCablePoint[] CablePoints;
    public LiftVehicleSystem VehicleSystem;
    public List<NavArea> NavAreas;
    // These are just the links between stations, not any internal ones
    // in areas
    public List<NavLink> NavLinks;

    private bool Initialized;

    public override void Initialize() {
        VehicleSystem = new LiftVehicleSystem();
        VehicleSystem.Parent = this;
        VehicleSystem.Speed = Template.MaxSpeed;

        Initialized = true;
    }

    void Update() {
        if(NavAreas == null) return;
        
        foreach(NavArea area in NavAreas) {
            if(area.Modified) {
                area.Modified = false;
                area.RecalculateSimpleLinks();
            }
        }
    }

    public override void Advance(float delta) {
        if(!Initialized) return;

        VehicleSystem.Advance(delta);

        if(NavAreas != null) {
            foreach(var area in NavAreas) {
                // Disabled for now because lifts lack implementations and stuff
                // area.Advance(delta);
            }
        }
    }

    public void Finish() {
        VehicleSystem.TemplateVehicle = Data.PhysicalVehicle;
        VehicleSystem.Initialize();
    }

    public void CreateSubObjects() {
        GameObject gameObject = new GameObject("Line");
        gameObject.transform.SetParent(transform, true);
        gameObject.layer = LayerMask.NameToLayer("2D");
        Line = gameObject.AddComponent<LineRenderer>();

        Line.widthMultiplier = 5f;
        Line.material = RenderingData.Instance.VertexColorMaterial;
        Line.startColor = Color.red;
        Line.endColor = Color.red;
    }
}