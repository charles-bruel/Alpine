public struct LiftPassAccessDefinition {
    public Direction Side;
    public int Position;
    public bool Entry;
    public bool Exit;
    // TODO: Vehicle mask? (seperate loading areas for different vehicles)

    public enum Direction {
        Uphill, Downhill
    }
}