using UnityEngine;
using System;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "Map", menuName = "Game Elements/Map", order = 1)]
public class AlpineMap : ScriptableObject, IMap {
    [Header("Meta")]
    public bool Include;
    public string MapName;
    public string Guid = System.Guid.NewGuid().ToString();

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

    public void Load(TerrainManager terrainManager) {
        terrainManager.CopyMapData(this);
    }
}