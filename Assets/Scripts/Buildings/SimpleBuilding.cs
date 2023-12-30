using System;
using System.Collections.Generic;
using UnityEngine;

public class SimpleBuilding : Building {
    public List<NavArea> NavAreas = new List<NavArea>();
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

    // TODO: Support non-NavArea polygons
    public override void Destroy() {
        Functionality.OnDestroy();

        foreach(NavArea area in NavAreas) {
            PolygonsController.Instance.DestroyPolygon(area);
        }

        base.Destroy();
    }

    public override string GetBuildingTypeName() {
        return Template.name;
    }
}