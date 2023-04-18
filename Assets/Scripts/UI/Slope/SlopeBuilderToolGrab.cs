using UnityEngine;

public class SlopeBuilderToolGrab : GenericGrab {
    public int SlopePointGroupIndex;
    public int SlopePointIndex;
    public SlopeConstructionData Data;
    public override void OnDragBehavior(Vector2 newPos) {

        PolygonsController.PolygonSnappingResult? snapping = PolygonsController.Instance.CheckForSnapping(newPos, 10, 7, PolygonFlags.NAVIGABLE_MASK);
        if(snapping != null) {
            newPos = snapping.Value.Pos;
            RectTransform.anchoredPosition = newPos;
        }

        Data.SlopePoints[SlopePointIndex] = newPos;
        PolygonsController.Instance.MarkPolygonsDirty();
    }
}