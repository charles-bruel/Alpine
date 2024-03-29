using System.Collections.Generic;
using UnityEngine;

public class Snowfront : PolygonBuilding {
    void Update() {
        if(Footprint.Modified) {
            Footprint.Modified = false;
            Footprint.RecalculateSimpleLinks();
        }
    }

    public override string GetBuildingTypeName() {
        return "Snowfront";
    }

    public override void OnDeselected() {
        BuildingsController.Instance.SnowfrontPanelUI.Hide();
    }

    public override void OnSelected() {
        BuildingsController.Instance.SnowfrontPanelUI.Inflate(this);
    }
}