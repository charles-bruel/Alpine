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

﻿using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

class BullWheel : MonoBehaviour
{
    public float Radius;
    public GameObject Left;
    public GameObject Right;
    public SimpleSheaveControlScript ControllerLeft;
    public SimpleSheaveControlScript ControllerRight;
    public float TowerDist;
    public float BaseTowerLength;
    public bool LeftEnabled;
    public bool RightEnabled;

    void Update()
    {
        Left.SetActive(LeftEnabled);
        Right.SetActive(RightEnabled);
    }

    public List<Vector3> GetCablePoints(int length, float startAngle, float endAngle)
    {
        if(endAngle - startAngle < -180)
        {
            endAngle += 360;
        }
        if(endAngle - startAngle > 180)
        {
            endAngle -= 360;
        }
        List<Vector3> list = new List<Vector3>(length);
        for(int i = 0;i < length;i++) {
            float angle = startAngle + i * (endAngle - startAngle) / (length - 1);
            float theta = angle * Mathf.Deg2Rad;
            list.Add(new Vector3(Mathf.Cos(theta) * Radius + transform.localPosition.x, transform.localPosition.y, Mathf.Sin(theta) * Radius + transform.localPosition.z));
        }
        return list;
    }
}

