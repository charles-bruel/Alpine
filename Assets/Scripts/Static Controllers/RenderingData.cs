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

[ExecuteInEditMode]
public class RenderingData : MonoBehaviour
{
    [Header("2D view colors")]
    public Color UndevelopedBackgroundColor;

    [Tooltip("This color which have been aerially cleared, i.e. trees have been removed")]
    public Color DevelopedColor;

    [Tooltip("This color covers areas which been completely cleared, i.e. trees and rocks have been removed")]
    public Color ClearedColor;

    [Tooltip("This color covers areas which skiers navigate around without downhill skiing")]
    public Color SnowfrontColor;

    [Tooltip("This shows the region that will become a slope while building")]
    public Color SlopeDraftColor;

    [Tooltip("This color shows a portal between multiple navigable regions")]
    public Color PortalColor;
    public Color StructureColor;

    [Header("Slope Colors")]
    public Color GreenSlopeColor;
    public Color BlueSlopeColor;
    public Color BlackSlopeColor;
    public Color DoubleBlackSlopeColor;

    [Header("Materials")]
    public Material VertexColorMaterial;

    public void Initialize() {
        Instance = this;
    }

    void Update() {
        Initialize();
    }

    public static RenderingData Instance;
}
