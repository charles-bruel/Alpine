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

using System;
using System.Collections.Generic;
using UnityEngine;

public class SimpleBuilding : Building {
    public List<NavArea> NavAreas = new List<NavArea>();
    // Includes nav areas
    public List<AlpinePolygon> Polygons = new List<AlpinePolygon>();
    public BuildingFunctionality Functionality;
    public INavNode ServiceNode;
    public SimpleBuildingTemplate Template;

    public RectTransform WorldUIIcon;

    public override void Advance(float delta) {
        foreach(NavArea area in NavAreas) {
            area.Advance(delta);
        }
    }

    void Update() {
        foreach(NavArea area in NavAreas) {
            if(area.Modified) {
                area.Modified = false;
                area.RecalculateSimpleLinks();
            }
        }
    }

    public override void Destroy() {
        Functionality.OnDestroy();

        foreach(AlpinePolygon polygon in Polygons) {
            PolygonsController.Instance.DestroyPolygon(polygon);
        }

        Destroy(WorldUIIcon.gameObject);

        base.Destroy();
    }

    public override string GetBuildingTypeName() {
        return Template.name;
    }

    public override void OnSelected() {
        BuildingsController.Instance.BuildingPanelUI.Inflate(this);
    }

    public override void OnDeselected() {
        BuildingsController.Instance.BuildingPanelUI.Hide();
    }
}