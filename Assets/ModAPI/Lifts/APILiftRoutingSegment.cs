using System.Linq;

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
    
    public virtual LiftPathAccessDefinition[] GetPathAccess(LiftRoutingSegmentType type, LiftPathAccessDefinition[] defaultUphillAccessPoints, LiftPathAccessDefinition[] defaultDownhillAccessPoints) {
        switch(type) {
            case LiftRoutingSegmentType.FIRST:
            return defaultUphillAccessPoints;
            case LiftRoutingSegmentType.LAST:
            return defaultDownhillAccessPoints;
            case LiftRoutingSegmentType.MIDDLE:
            return defaultDownhillAccessPoints.Concat(defaultUphillAccessPoints).ToArray();
        }
        throw new System.Exception("Invalid LiftRoutingSegmentType");
    }
}