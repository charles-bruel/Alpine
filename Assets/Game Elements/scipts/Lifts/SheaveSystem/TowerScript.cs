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

﻿using System.Collections.Generic;
using UnityEngine;
using System;

public class TowerScript : MonoBehaviour
{
    public float Width;
    public GameObject PaddingSource;
    public List<GameObject> PaddingObjects;
    public Transform BaseItems;
    public Transform Target;
    public Transform WirePoint;

    private static int layer = -1;

    void Update()
    {
        //Ugly and possibly slow but gets the job done.
        if (PaddingSource != null)
        {
            foreach (GameObject obj in PaddingObjects){
                if(obj.activeSelf != PaddingSource.activeSelf)
                    obj.SetActive(PaddingSource.activeSelf);
            }
        }
    }

    public void Reset()
    {
        if(layer == -1) {
            layer = 1 << LayerMask.NameToLayer("Terrain");
        }
        if(BaseItems != null)
        {
            RaycastHit hitInfo = default(RaycastHit);
            Physics.Raycast(transform.position, Target.position - transform.position, out hitInfo, float.MaxValue, layer);
            BaseItems.position = hitInfo.point;
        }
    }
}