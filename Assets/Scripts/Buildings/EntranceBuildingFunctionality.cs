using System;
using System.Collections.Generic;
using UnityEngine;

public class EntranceBuildingFunctionality : BuildingFunctionality {
    
    public override void OnFinishConstruction() {
        VisitorController.Instance.SpawnPoints.Add(Building.ServiceNode);
    }
}