using UnityEngine;

public class LiftBuilderToolGrab : GenericGrab {
    public int RoutingSegmentIndex;
    public LiftConstructionData Data;
    public override void OnDragBehavior(Vector2 newPos) {
        Data.RoutingSegments[RoutingSegmentIndex].Position = new Vector3(newPos.x, 0, newPos.y);
    }
}