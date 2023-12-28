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
}