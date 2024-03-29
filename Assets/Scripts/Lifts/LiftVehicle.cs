//>============================================================================<
//
//    Alpine, Ski Resort Tycoon Game
//    Copyright (C) 2024  Charles Bruel
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see <https://www.gnu.org/licenses/>.
//>============================================================================<

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
    [NonSerialized]
    public Visitor[] Visitors;

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
