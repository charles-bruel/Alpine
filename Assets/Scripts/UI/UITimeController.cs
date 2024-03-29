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

public class UITimeController : MonoBehaviour {
    public ToggleGroup TimeToggleGroup;
    public Toggle X0;
    public Toggle X1;
    public Toggle X10;
    public Toggle X100;

    [NonSerialized]
    public Dictionary<Toggle, float> ToggleToTimeMultiplier;
    [NonSerialized]
    public Dictionary<int, Toggle> IndexToToggle;
    [NonSerialized]
    public Dictionary<Toggle, int> ToggleToIndex;

    private int LastTimeIndex = 1;

    private bool Initialized = false;

    private void Initialize() {
        if(Initialized) return;
        Initialized = true;
        // TODO: Don't hardcode this
        ToggleToTimeMultiplier = new Dictionary<Toggle, float> {
            {X0, 0},
            {X1, 1},
            {X10, 10},
            {X100, 100}
        };
        IndexToToggle = new Dictionary<int, Toggle> {
            {0, X0},
            {1, X1},
            {2, X10},
            {3, X100}
        };
        ToggleToIndex = new Dictionary<Toggle, int> {
            {X0, 0},
            {X1, 1},
            {X10, 2},
            {X100, 3}
        };
    }

    void Update() {
        Initialize();

        for(int i = 0; i < ToggleToTimeMultiplier.Count; i++) {
            if(Input.GetKeyDown("" + i)) {
                IndexToToggle[i].isOn = true;
            }
        }

        if (Input.GetKeyDown(KeyCode.Space)) {
            if (GameController.Instance.TimeMultiplier == 0) {
                IndexToToggle[LastTimeIndex].isOn = true;
            } else {
                IndexToToggle[0].isOn = true;
            }
        }
    }

    public void OnTimeToggleChanged() {        
        float newTimeMultiplier = ToggleToTimeMultiplier[TimeToggleGroup.ActiveToggles().First()];

        if (newTimeMultiplier != 0) {
            foreach (var key_value in ToggleToTimeMultiplier) {
                if (key_value.Value == GameController.Instance.TimeMultiplier) {
                    LastTimeIndex = ToggleToIndex[key_value.Key];
                }
            }
        }

        // It can be null right on startup
        if(GameController.Instance != null) {
            GameController.Instance.TimeMultiplier = newTimeMultiplier;
        }
    }
}