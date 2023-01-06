using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class SheaveTester : MonoBehaviour
{
    public SheaveScript SheaveScript;
    [Range(0, 6)]
    public int SheaveLayout;
    public float Start = 180;
    public float End = 10;
    public float SheaveScale = 1;

    public FullSheaveLayoutDescriptor[] Descriptors;

    private int lwp;

    void Update()
    {
        if (SheaveLayout != lwp)
        {
            FullSheaveLayoutDescriptor descriptor = Descriptors[SheaveLayout];
            SheaveScript.NumWheels = descriptor.NumWheels;
            SheaveScript.Stage2Layout = descriptor.Stage2Layout;
            SheaveScript.Stage3Layout = descriptor.Stage3Layout;
            SheaveScript.Stage4Layout = descriptor.Stage4Layout;
            SheaveScript.Stage5Layout = descriptor.Stage5Layout;
            SheaveScript.UpdateToggle = !SheaveScript.UpdateToggle;
            lwp = SheaveLayout;
        }
        SheaveScript.StartAngle = Start;
        SheaveScript.EndAngle = End;

        SheaveScript.transform.position = new Vector3(0, SheaveScript.Radius * SheaveScale, 0);
    }
}