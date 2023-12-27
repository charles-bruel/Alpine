using UnityEngine;

public class LiftRoutingSegmentTemplate : LiftSegmentTemplate
{
    public enum RoutingSegmentType {
        STATION,
        MIDSTATION,
        TURN,
    }

    public APILiftRoutingSegment APILiftRoutingSegment;
    public LiftPathAccessDefinition[] DefaultUphillAccessPoints;
    public LiftPathAccessDefinition[] DefaultDownhillAccessPoints;

    private void Initialize() {
        if(APILiftRoutingSegment != null) return;
        
        APILiftRoutingSegment = LiftSegmentAPIDef.Fetch<APILiftRoutingSegment>();
        APILiftSegment = APILiftRoutingSegment;
    }

    public override IPoolable Clone() {
        Initialize();
        var temp = GameObject.Instantiate(this);
        temp.APILiftRoutingSegment = APILiftRoutingSegment;
        temp.APILiftSegment = APILiftSegment;
        return temp;
    }
}