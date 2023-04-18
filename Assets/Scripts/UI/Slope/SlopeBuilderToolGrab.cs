using UnityEngine;

public class SlopeBuilderToolGrab : GenericGrab {
    public int SlopePointGroupIndex;
    public int SlopePointIndex;
    public SlopeConstructionData Data;
    public override void OnDragBehavior(Vector2 newPos) {
        Data.SlopePoints[SlopePointIndex] = newPos;
    }
}