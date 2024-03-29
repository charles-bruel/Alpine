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

using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.Assertions;

public class Lift : Building {
    public LiftTemplate Template;
    public LiftConstructionData Data;
    // Includes nav areas
    public List<AlpinePolygon> Polygons;
    public LineRenderer Line;
    public LiftCablePoint[] CablePoints;
    public List<LiftVehicleSystem.LiftAccessNode> CableJoins;
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
                area.Advance(delta);
            }
        }
    }

    public void Finish() {
        VehicleSystem.TemplateVehicle = Data.PhysicalVehicle;
        VehicleSystem.Initialize(CableJoins);

        foreach(NavLink explicitLink in NavLinks) {
            if (explicitLink.Implementation is LiftNavLinkImplementation impl) {
                impl.LiftVehicleSystem = VehicleSystem;
            } else {
                Assert.IsTrue(false, "Lift nav link implementation is not a lift nav link implementation");
            }
        }
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

    public override void Destroy() {
        foreach(AlpinePolygon polygon in Polygons) {
            PolygonsController.Instance.DestroyPolygon(polygon);
        }

        base.Destroy();
    }

    public override string GetBuildingTypeName() {
        return Template.name;
    }

    public override void OnSelected() {
        BuildingsController.Instance.BuildingPanelUI.Inflate(this);
    }

    public override void OnDeselected() {
        BuildingsController.Instance.BuildingPanelUI.Hide();
    }
}