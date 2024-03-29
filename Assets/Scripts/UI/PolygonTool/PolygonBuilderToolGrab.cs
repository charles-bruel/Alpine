using UnityEngine;

public class PolygonBuilderToolGrab : GenericGrab {
    public int SlopePointGroupIndex;
    public int SlopePointIndex;
    public PolygonConstructionData Data;
    public AlpinePolygon Footprint;

    public override void OnDragBehavior(Vector2 newPos) {

        PolygonsController.PolygonSnappingResult? snapping = PolygonsController.Instance.CheckForSnapping(newPos, 10, 7, PolygonFlags.NAVIGABLE_MASK, Footprint);
        if(snapping != null) {
            newPos = snapping.Value.Pos;
            RectTransform.anchoredPosition = newPos;

            Data.SlopePoints[SlopePointIndex] = new PolygonConstructionData.SlopePoint(newPos, snapping.Value);
        } else {
            Data.SlopePoints[SlopePointIndex] = new PolygonConstructionData.SlopePoint(newPos);
        }
        PolygonsController.Instance.MarkPolygonsDirty();
    }
}