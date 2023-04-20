using UnityEngine;

public class SlopeBuilderToolGrab : GenericGrab {
    public int SlopePointGroupIndex;
    public int SlopePointIndex;
    public SlopeConstructionData Data;
    public SlopeBuilder Builder;

    public override void OnDragBehavior(Vector2 newPos) {

        PolygonsController.PolygonSnappingResult? snapping = PolygonsController.Instance.CheckForSnapping(newPos, 10, 7, PolygonFlags.NAVIGABLE_MASK, Builder.Result.Footprint);
        if(snapping != null) {
            newPos = snapping.Value.Pos;
            RectTransform.anchoredPosition = newPos;

            Data.SlopePoints[SlopePointIndex] = new SlopeConstructionData.SlopePoint(newPos, snapping.Value);
        } else {
            Data.SlopePoints[SlopePointIndex] = new SlopeConstructionData.SlopePoint(newPos);
        }
        PolygonsController.Instance.MarkPolygonsDirty();
    }
}