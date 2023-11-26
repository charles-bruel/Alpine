using System;
using System.Collections.Generic;
using UnityEngine;

public class SimpleBuilding : Building {
    public List<NavArea> NavAreas = new List<NavArea>();

    void Update() {
        foreach(NavArea area in NavAreas) {
            if(area.Modified) {
                area.RecalculateSimpleLinks();
            }
        }
    }
}