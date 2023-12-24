using UnityEngine;

public struct Vector2POD {
    public float x;
    public float y;

    public Vector2POD(float x, float y) {
        this.x = x;
        this.y = y;
    }

    public static implicit operator Vector2(Vector2POD v2) => new Vector2(v2.x, v2.y);
    public static implicit operator Vector2POD(Vector2 v2) => new Vector2POD(v2.x, v2.y);
}