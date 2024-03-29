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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateController : MonoBehaviour {
    public bool Mode2D { get; private set; }
    public bool Mode3D { get { return !Mode2D; } }
    public GameObject[] TwoDOnlyObjects;
    public GameObject[] ThreeDOnlyObjects;
    public Camera TwoDCamera;
    public Camera ThreeDCamera;

    public void ToggleMode(bool mode) {
        for(int i = 0;i < TwoDOnlyObjects.Length;i ++) {
            TwoDOnlyObjects[i].SetActive(mode);
        }
        for(int i = 0;i < ThreeDOnlyObjects.Length;i ++) {
            ThreeDOnlyObjects[i].SetActive(!mode);
        }
        Mode2D = mode;
    }

    public void Initialize() {
        Instance = this;
    }

    public static StateController Instance;
}
