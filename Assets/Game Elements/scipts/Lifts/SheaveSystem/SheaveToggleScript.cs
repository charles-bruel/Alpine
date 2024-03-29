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

public class SheaveToggleScript : MonoBehaviour
{
    public Sheave target;
    public Transform Lower;
    public Transform Higher;
    public int Threshold;

    private bool PastStatus;

    void Update()
    {
        bool status = target.Parent != null && target.Parent.Level >= Threshold;
        if(PastStatus = status)
        {
            return;
        }
        PastStatus = status;
        if (status)
        {
            Lower.gameObject.SetActive(false);
            Higher.gameObject.SetActive(true);
        } 
        else
        {
            Lower.gameObject.SetActive(true);
            Higher.gameObject.SetActive(false);
        }
    }
}
