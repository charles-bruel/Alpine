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

//TODO: Polish this
public class TopCam : MonoBehaviour
{
    private Vector2 startScreenPos;
    private Vector3 startWorldPos;
    private int ticks = 0;
    private Camera Camera;

    private readonly float keyboardpanmultiplier = 3;

    void Start() {
        Camera = GetComponent<Camera>();
    }

    void LateUpdate() {
        if(Input.GetMouseButton(2)) {
            if(ticks++ == 0) {
                startScreenPos = Transform(Input.mousePosition);
                startWorldPos = transform.position;
            } else {
                Vector2 dif = Transform(Input.mousePosition) - startScreenPos;
                transform.position = startWorldPos - new Vector3(dif.x, 0, dif.y);
            }
        } else {
            ticks = 0;
        }

        float speedVal = Camera.orthographicSize * Time.deltaTime * keyboardpanmultiplier;

        if(Input.GetKey(KeyCode.W)) {
            transform.position = transform.position + new Vector3(0, 0,  speedVal);
        }
        if(Input.GetKey(KeyCode.S)) {
            transform.position = transform.position + new Vector3(0, 0, -speedVal);
        }
        if(Input.GetKey(KeyCode.A)) {
            transform.position = transform.position + new Vector3(-speedVal, 0, 0);
        }
        if(Input.GetKey(KeyCode.D)) {
            transform.position = transform.position + new Vector3( speedVal, 0, 0);
        }


        Camera.orthographicSize += -Input.mouseScrollDelta.y * 10;
        if(Camera.orthographicSize < 10) {
            Camera.orthographicSize = 10;
        }
        if(Camera.orthographicSize > 1000) {
            Camera.orthographicSize = 1000;
        }
    }

    private Vector2 Transform(Vector3 mp) {
        return ((Vector2) (mp) / Screen.height * Camera.orthographicSize * 2);
    }
}
