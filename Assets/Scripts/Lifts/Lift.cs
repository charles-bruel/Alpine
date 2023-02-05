using UnityEngine;
using System;
using System.Collections.Generic;

public class Lift : MonoBehaviour {
    public LiftTemplate Template;
    public PolygonsController.AlpinePolygon Footprint;
    public LineRenderer Line;

    private bool Initialized;

    public void Initialize() {
        Initialized = true;
    }

    public void Finish(PolygonsController.AlpinePolygon Footprint) {
        this.Footprint = Footprint;
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