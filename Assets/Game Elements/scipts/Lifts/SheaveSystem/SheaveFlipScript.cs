using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SheaveFlipScript : MonoBehaviour
{

    public Sheave target;
    public Transform toFlip;

    private bool PastUphill;

    void Update()
    {
        if(PastUphill == target.ParentSheaveObject.Uphill)
        {
            return;
        }
        if (target.ParentSheaveObject.Uphill)
        {
            toFlip.localEulerAngles = new Vector3(-90, 0, 0);
            PastUphill = true;
        }
        else
        {
            toFlip.localEulerAngles = new Vector3(90, 0, 0);
            PastUphill = false;
        }
    }
}
