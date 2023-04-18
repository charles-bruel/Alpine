using UnityEngine;
using System.Collections.Generic;

public class SlopeConstructionData {
    public SlopeConstructionData() {
        SlopePoints = new List<Vector2>();
    }

    //TODO: Support polygons with holes, etc.
    public List<Vector2> SlopePoints;
}