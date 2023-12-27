public class APILiftRoutingSegment : APILiftSegment {

    // This function will return the length from the origin to the end of the station
    // This function will be called after Build()
    // Default behavior is to return FloatParameters[0], if it exists, otherwise 0
    public virtual float GetLength(ICustomScriptable parent) {
        if(FloatParameters.Length > 0) {
            return FloatParameters[0];
        }
        return 0;
    }
    
    public virtual LiftPathAccessDefinition[] GetPathAccess(LiftRoutingSegmentType type) {
        switch(type) {
            case LiftRoutingSegmentType.FIRST:
            return new LiftPathAccessDefinition[] {
                new LiftPathAccessDefinition() {
                    Side = LiftPathAccessDefinition.Direction.UPHILL,
                    Pos = 0,
                    Entry = true,
                    Exit = true
                }
            };
            case LiftRoutingSegmentType.LAST:
            return new LiftPathAccessDefinition[] {
                new LiftPathAccessDefinition() {
                    Side = LiftPathAccessDefinition.Direction.DOWNHILL,
                    Pos = 0,
                    Entry = true,
                    Exit = true
                }
            };
            case LiftRoutingSegmentType.MIDDLE:
            return new LiftPathAccessDefinition[] {};
        }
        throw new System.Exception("Invalid LiftRoutingSegmentType");
    }
}