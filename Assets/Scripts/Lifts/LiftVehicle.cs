using System;
using System.Collections.Generic;
using UnityEngine;

public class LiftVehicle : MonoBehaviour {
    [Header("Basic Information")]
    public Transform[] Seats;
    public Transform RotateTransform;
    public Transform DerotateTransform;
    
    [Header("Physics")]
    public float CoMOffset;
    public float Mass;
    public float Friction;

    private const float epsilon = 0.01f;

    public void UpdateSwing(float delta) {
        // Point mass
        float I = CoMOffset * CoMOffset * Mass;

        float force_gravity = Mass * -9.81f;
        float tau_gravity = force_gravity * CoMOffset * Mathf.Sin(Theta);

        float force_applied = Mass * Acceleration;
        float tau_applied = force_applied * CoMOffset * Mathf.Cos(Theta);

        float tau_friction;
        if(Mathf.Abs(Omega) < epsilon && Mathf.Abs(Theta) < epsilon) {
            // Static friction
            tau_friction = Mathf.Min(Friction, Mathf.Max(tau_gravity + tau_applied)) * -Mathf.Sign(tau_gravity + tau_applied);
        } else {
            // Kinetic friction
            tau_friction = Friction * -Mathf.Sign(Omega);
        }

        float tau = tau_friction + tau_gravity + tau_applied;
        float alpha = tau / I;
        float deltaOmega = alpha * delta;

        Omega += deltaOmega;
        Theta += Omega * delta;

        if(Theta > 0.5f) {
            Theta = 0.5f;
            Omega = 0f;
        } else if (Theta < -0.5f) {
            Theta = -0.5f;
            Omega = 0f;
        }
    }


    [NonSerialized]
    public float Position;
    [NonSerialized]
    public float Velocity;
    [NonSerialized]
    public float Acceleration;
    [NonSerialized]
    public float Theta;
    [NonSerialized]
    public float Omega;
}
