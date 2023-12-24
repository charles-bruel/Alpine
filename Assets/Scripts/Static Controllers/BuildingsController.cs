using UnityEngine;
using System;
using System.Collections.Generic;

public class BuildingsController : MonoBehaviour {
    // Building templates functionality
    public List<LiftTemplate> LiftTemplates;
    public List<SimpleBuildingTemplate> BuildingTemplates;
    
    public LiftTemplate GetLiftTemplate(string name) {
        foreach(var template in LiftTemplates) {
            if(template.name == name) return template;
        }
        return null;
    }

    public SimpleBuildingTemplate GetBuildingTemplate(string name) {
        foreach(var template in BuildingTemplates) {
            if(template.name == name) return template;
        }
        return null;
    }

    // Built buildings functionality
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

    // Initialization; generic
    public void Initialize() {
        Instance = this;
    }

    public static BuildingsController Instance;

}