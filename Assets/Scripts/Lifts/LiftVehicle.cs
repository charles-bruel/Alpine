using System;
using System.Collections.Generic;
using UnityEngine;

public class LiftVehicle : MonoBehaviour {
    public Transform[] Seats;
    public Transform RotateTransform;
    public Transform DerotateTransform;

    [NonSerialized]
    public float Position;
}
