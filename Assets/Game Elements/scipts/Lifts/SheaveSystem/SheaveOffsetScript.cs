using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SheaveOffsetScript : MonoBehaviour
{
    public Sheave target;
    public Transform toOffset;
    public Vector3 offsetBy;

    void Update()
    {
        if (!target.ParentSheaveObject.Uphill)
        {
            toOffset.localPosition = offsetBy;
        }
    }
}
