using UnityEngine;

public static class Extensions {
    public static Vector2 ToHorizontal(this Vector3 v) {
        return new Vector2(v.x, v.z);
    }

    public static Vector3 DropW(this Vector4 v) {
        return new Vector3(v.x, v.y, v.z);
    }

    public static float NextFloat(this System.Random rand, float min, float max) {
        float val = (float) rand.NextDouble();
        val *= (max - min);
        val += min;
        return val;
    }

    public static float NextAngleRads(this System.Random rand) {
        return (float)(rand.NextDouble() * 2.0 * 3.141592653589793);
    }
}