using System;
using System.Collections.Generic;
using UnityEngine;

public class EntranceBuildingFunctionality : BuildingFunctionality
{
    public override void OnFinishConstruction()
    {
        Debug.Log("foo");
    }
}