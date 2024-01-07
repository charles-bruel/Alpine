using System.Collections.Generic;
using UnityEngine;

public class Snowfront : Building {
    public NavArea Footprint;
    public SlopeConstructionData Data;

    void Update() {
        if(Footprint.Modified) {
            Footprint.Modified = false;
            Footprint.RecalculateSimpleLinks();
        }
    }

    public override void Advance(float delta) {
        if(Footprint != null) {
            Footprint.Advance(delta);
        }
    }

    public void Inflate(List<NavPortal> portals) {
        foreach(NavPortal portal in portals) {
            GameObject temp = new GameObject();
            temp.transform.SetParent(transform);
            temp.name = "Portal";
            temp.layer = LayerMask.NameToLayer("2D");

            portal.gameObject = temp;
            portal.Inflate();

            portal.A.Nodes.Add(portal);
            portal.A.Modified = true;

            portal.B.Nodes.Add(portal);
            portal.B.Modified = true;
        }
    }

    public override void Destroy() {
        PolygonsController.Instance.DestroyPolygon(Footprint);

        base.Destroy();
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