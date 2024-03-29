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

using UnityEngine;
using System;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "Map", menuName = "Game Elements/Map", order = 1)]
public class AlpineMap : ScriptableObject, IMap {
    [Header("Meta")]
    public bool Include;
    public string MapName;
    public string Guid = System.Guid.NewGuid().ToString();
    public Sprite Thumb;

    [Header("Tile & Map Information")]
    public float TileSize;
    public float TileHeight;
    public String TexturePath;
    public int NumTilesX;
    public int NumTilesY;

    [Header("Scatter Placement Settings")]
    public Texture2D DecoMap;
    public int NumTrees;
    public int NumRocks;
    public float MinTreeHeight = 0.5f;
    public float MaxTreeHeight = 1.5f;
    public float AltitudeAdjustFactor = 0.5f;
    public float MinRockSize = 0.5f;
    public float MaxRockSize = 0.5f;
    [Header("Weather Settings")]
    public Texture2D WeatherMap;

    public string GetID() {
        return "alp-" + Guid;
    }

    public string GetName() {
        return MapName;
    }

    public Sprite GetThumbnail() {
        return Thumb;
    }

    public void Load(TerrainManager terrainManager) {
        terrainManager.CopyMapData(this);
    }
}