using UnityEngine;

public struct QuaternionPOD {
    public float x;
    public float y;
    public float z;
    public float w;

    public QuaternionPOD(float x, float y, float z, float w) {
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = w;
    }

    
    public static implicit operator Quaternion(QuaternionPOD q) => new Quaternion(q.x, q.y, q.z, q.w);
    public static implicit operator QuaternionPOD(Quaternion q) => new QuaternionPOD(q.x, q.y, q.z, q.w);
}