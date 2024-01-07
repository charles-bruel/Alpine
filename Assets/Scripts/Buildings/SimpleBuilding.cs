using System;
using System.Collections.Generic;
using UnityEngine;

public class SimpleBuilding : Building {
    public List<NavArea> NavAreas = new List<NavArea>();
    // Includes nav areas
    public List<AlpinePolygon> Polygons = new List<AlpinePolygon>();
    public BuildingFunctionality Functionality;
    public INavNode ServiceNode;
    public SimpleBuildingTemplate Template;

    public override void Advance(float delta) {
        foreach(NavArea area in NavAreas) {
            area.Advance(delta);
        }
    }

    void Update() {
        foreach(NavArea area in NavAreas) {
            if(area.Modified) {
                area.Modified = false;
                area.RecalculateSimpleLinks();
            }
        }
    }

    public override void Destroy() {
        Functionality.OnDestroy();

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