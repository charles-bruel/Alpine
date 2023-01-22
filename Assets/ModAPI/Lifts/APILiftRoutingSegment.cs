public class APILiftRoutingSegment : APILiftSegment {

    // This function will return the length from the origin to the end of the station
    // This function will be called after Build()
    // Default behavior is to return FloatParameters[0], if it exists, otherwise 0
    public virtual float GetLength() {
        if(FloatParameters.Length > 0) {
            return FloatParameters[0];
        }
        return 0;
    }

}