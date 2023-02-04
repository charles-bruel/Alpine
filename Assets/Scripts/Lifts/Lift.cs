using UnityEngine;
using System;
using System.Collections.Generic;

public class Lift : MonoBehaviour {
    public LiftTemplate Template;
    public PolygonsController.AlpinePolygon Footprint;

    private bool Initialized;

    public void Initialize() {
        Initialized = true;
    }

    public void Finish(PolygonsController.AlpinePolygon Footprint) {
        this.Footprint = Footprint;
    }
}