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

[System.Serializable]
public struct BuildingSaveDataV1 {
    public string Template;
    public Vector3POD Position;
    public float Rotation;
    public NavAreaGraphSaveDataV1[] NavAreaGraphs;

    public static BuildingSaveDataV1 FromSimpleBuilding(SimpleBuilding building, SavingContextV1 context) {
        BuildingSaveDataV1 result = new BuildingSaveDataV1();
        result.Template = building.Template.name;
        result.Position = building.transform.position;
        result.Rotation = building.transform.eulerAngles.y;
        result.NavAreaGraphs = new NavAreaGraphSaveDataV1[building.NavAreas.Count];

        for(int i = 0;i < building.NavAreas.Count;i ++) {
            result.NavAreaGraphs[i] = NavAreaGraphSaveDataV1.FromNavArea(building.NavAreas[i], context);
        }

        return result;
    }
}