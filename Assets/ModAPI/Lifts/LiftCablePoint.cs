using UnityEngine;

public struct LiftCablePoint {
    
    public Vector3 pos;
    public float speed;

    public LiftCablePoint(Vector3 pos, float speed) {
        this.pos = pos;
        this.speed = speed;
    }
}