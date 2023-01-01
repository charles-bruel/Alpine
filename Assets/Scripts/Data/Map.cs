using UnityEngine;
using System;

[System.Serializable]
public struct Map {
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
    [Header("Weather Settings")]
    public Texture2D WeatherMap;
}