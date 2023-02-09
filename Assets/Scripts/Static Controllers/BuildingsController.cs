using UnityEngine;
using System;
using System.Collections.Generic;

public class BuildingsController : MonoBehaviour {

    public List<Building> Buildings;

    public void Advance(float delta) {
        for(int i = 0;i < Buildings.Count;i ++) {
            Buildings[i].Advance(delta);
        }
    }

    public void RegisterBuilding(Building building) {
        building.Initialize();
        Buildings.Add(building);
    }

    public void Initialize() {
        Instance = this;
    }

    public static BuildingsController Instance;

}