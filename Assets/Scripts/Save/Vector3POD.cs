using UnityEngine;

public struct Vector3POD {
    public float x;
    public float y;
    public float z;

    public Vector3POD(float x, float y, float z) {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public static implicit operator Vector3(Vector3POD v3) => new Vector3(v3.x, v3.y, v3.z);
    public static implicit operator Vector3POD(Vector3 v3) => new Vector3POD(v3.x, v3.y, v3.z);
}