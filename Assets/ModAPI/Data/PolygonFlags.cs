[System.Flags]
public enum PolygonFlags : uint {
    AERIAL_CLEARANCE = 1,
    GROUND_CLEARANCE = 2,
    CLEARANCE = AERIAL_CLEARANCE | GROUND_CLEARANCE,
    FLAT_NAVIGABLE = 4,
    FLATTEN_DOWN = 8,
    FLATTEN_UP = 16,
    FLATTEN = FLATTEN_DOWN | FLATTEN_UP,
    SLOPE_NAVIGABLE = 32,
    NAVIGABLE_MASK = FLAT_NAVIGABLE | SLOPE_NAVIGABLE,
    // All slopes are of the form 0001 xy00 0000
    // Where xy determines the difficulty
    SLOPE_GREEN = 256,
    SLOPE_BLUE = 320,
    SLOPE_BLACK = 384,
    SLOPE_DOUBLE_BLACK = 448,
    SLOPE_MASK = 448,
}