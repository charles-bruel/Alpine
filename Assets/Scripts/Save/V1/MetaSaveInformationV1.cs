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
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public struct MetaSaveInformationV1 {
    public int TimeSpeedIndex;
    public string MapID;
    public Vector3POD CameraPosition2d;
    public float CameraOrthographicSize2d;
    public Vector3POD CameraPosition3d;
    public QuaternionPOD CameraRotation3d;

    public static MetaSaveInformationV1 Create() {
        Dictionary<Toggle, int> togglesToId = new Dictionary<Toggle, int> {
            {UIsController.Instance.UITimeController.X0, 0},
            {UIsController.Instance.UITimeController.X1, 1},
            {UIsController.Instance.UITimeController.X10, 2},
            {UIsController.Instance.UITimeController.X100, 3}
        };

        return new MetaSaveInformationV1() {
            MapID = TerrainManager.TargetMap.GetID(),
            TimeSpeedIndex = togglesToId[UIsController.Instance.UITimeController.TimeToggleGroup.ActiveToggles().First()],
            CameraPosition3d = StateController.Instance.ThreeDCamera.transform.position,
            CameraRotation3d = StateController.Instance.ThreeDCamera.transform.rotation,
            CameraPosition2d = StateController.Instance.TwoDCamera.transform.position,
            CameraOrthographicSize2d = StateController.Instance.TwoDCamera.orthographicSize
        };
    }

    public void Restore() {
        Dictionary<int, Toggle> idToToggles = new Dictionary<int, Toggle> {
            {0, UIsController.Instance.UITimeController.X0},
            {1, UIsController.Instance.UITimeController.X1},
            {2, UIsController.Instance.UITimeController.X10},
            {3, UIsController.Instance.UITimeController.X100}
        };


        foreach(Toggle toggle in idToToggles.Values) {
            toggle.isOn = (toggle == idToToggles[TimeSpeedIndex]);
        }

        StateController.Instance.ThreeDCamera.transform.position = CameraPosition3d;
        StateController.Instance.ThreeDCamera.transform.rotation = CameraRotation3d;

        StateController.Instance.TwoDCamera.transform.position = CameraPosition2d;
        StateController.Instance.TwoDCamera.orthographicSize = CameraOrthographicSize2d;
    }

    public void RestoreMap() {
        foreach(IMap map in TerrainManager.GetAllMaps()) {
            if(map.GetID() == MapID) {
                TerrainManager.TargetMap = map;
            }
        }
    }
}