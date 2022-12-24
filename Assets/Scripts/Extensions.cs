using UnityEngine;

public static class Extensions {
    public static Vector2 ToHorizontal(this Vector3 v) {
        return new Vector2(v.x, v.z);
    }
}