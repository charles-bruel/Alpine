using System;
using System.Collections.Generic;
using UnityEngine;

public class LiftVehicle : MonoBehaviour {
    public Transform[] Seats;
    public Transform CableAlignedComponent;

    [NonSerialized]
    public float Position;
}
