using UnityEngine;

public class SlopeBuilderToolGrab : GenericGrab {
    public int SlopePointGroupIndex;
    public int SlopePointIndex;
    public SlopeConstructionData Data;
    public override void OnDragBehavior(Vector2 newPos) {

        Vector2? snapping = PolygonsController.Instance.CheckForSnapping(newPos, 6, PolygonFlags.NAVIGABLE_MASK);
        if(snapping != null) {
            newPos = snapping.Value;
            RectTransform.anchoredPosition = newPos;
        }

        Data.SlopePoints[SlopePointIndex] = newPos;
        PolygonsController.Instance.MarkPolygonsDirty();
    }
}