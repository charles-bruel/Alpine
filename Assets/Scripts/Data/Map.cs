using UnityEngine;
using System;

[CreateAssetMenu(fileName = "Map", menuName = "Game Elements/Map", order = 1)]
public class Map : ScriptableObject {
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