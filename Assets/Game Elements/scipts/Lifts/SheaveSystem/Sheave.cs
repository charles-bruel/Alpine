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

public class Sheave : MonoBehaviour
{
    [Header("Config Data")]
    public float Length;
    public int Level;
    public Transform TowerAttachPoint;

    [Header("Runtime Data")]
    public bool Open = true;
    public Sheave Parent;
    public Sheave ChildAtA;
    public Sheave ChildAtB;
    public int ID;
    public float StartPos;
    public float EndPos;
    public float TotalSheaveRotation;
    public SheaveScript ParentSheaveObject;
}