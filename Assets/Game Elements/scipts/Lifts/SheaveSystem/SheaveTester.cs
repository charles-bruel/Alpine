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

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class SheaveTester : MonoBehaviour
{
    public SheaveScript SheaveScript;
    [Range(0, 6)]
    public int SheaveLayout;
    public float Start = 180;
    public float End = 10;
    public float SheaveScale = 1;

    public FullSheaveLayoutDescriptor[] Descriptors;

    private int lwp;

    void Update()
    {
        if (SheaveLayout != lwp)
        {
            FullSheaveLayoutDescriptor descriptor = Descriptors[SheaveLayout];
            SheaveScript.NumWheels = descriptor.NumWheels;
            SheaveScript.Stage2Layout = descriptor.Stage2Layout;
            SheaveScript.Stage3Layout = descriptor.Stage3Layout;
            SheaveScript.Stage4Layout = descriptor.Stage4Layout;
            SheaveScript.Stage5Layout = descriptor.Stage5Layout;
            SheaveScript.UpdateToggle = !SheaveScript.UpdateToggle;
            lwp = SheaveLayout;
        }
        SheaveScript.StartAngle = Start;
        SheaveScript.EndAngle = End;

        SheaveScript.transform.position = new Vector3(0, SheaveScript.Radius * SheaveScale, 0);
    }
}