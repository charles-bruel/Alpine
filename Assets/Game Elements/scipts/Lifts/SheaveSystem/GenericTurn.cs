using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

abstract class GenericTurn : MonoBehaviour
{
    public Transform IncomingCablePoint;
    public Transform OutgoingCablePoint;
    public List<Transform> CablePointsUp;
    public List<Transform> CablePointsDown;

    public abstract void Reset();

    public abstract float GetLength();
}
