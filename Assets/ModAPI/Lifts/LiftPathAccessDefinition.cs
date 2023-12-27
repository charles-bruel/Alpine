using System;

[Serializable]
public struct LiftPathAccessDefinition {
    public Direction Side;
    public int Pos;
    public bool Entry;
    public bool Exit;
    // TODO: Vehicle mask? (seperate loading areas for different vehicles)

    [Serializable]
    public enum Direction {
        UPHILL, DOWNHILL
    }
}