[System.Flags]
public enum PolygonFlags : uint {
    AERIAL_CLEARANCE = 1,
    GROUND_CLEARANCE = 2,
    FLAT_NAVIGABLE = 4,
    FLATTEN_DOWN = 8,
    FLATTEN_UP = 16,
    FLATTEN = 24,//FLATTEN_DOWN | FLATTEN_UP
}