//>============================================================================<
//
//    Alpine, Ski Resort Tycoon Game
//    Copyright (C) 2024  Charles Bruel
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see <https://www.gnu.org/licenses/>.
//>============================================================================<

using UnityEngine;
using System;
using System.Collections.Generic;

public class BuildingsController : MonoBehaviour {
    [Header("UI")]
    public SlopePanelUI SlopePanelUI;
    public SnowfrontPanelUI SnowfrontPanelUI;
    public BuildingPanelUI BuildingPanelUI;
    
    [Header("Templates")]
    // Building templates functionality
    public List<LiftTemplate> LiftTemplates;
    public List<SimpleBuildingTemplate> BuildingTemplates;
    public List<ServiceInformation> ServiceTemplates;
    
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

    [Header("Current Game State")]
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

    public void UnregisterBuilding(Building building) {
        Buildings.Remove(building);
    }

    // Initialization; generic
    public void Initialize() {
        Instance = this;
    }

    public static BuildingsController Instance;

}