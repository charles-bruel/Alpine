using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.Assertions;

public class TerrainManager : MonoBehaviour {
    public static float LOD1 = 100.0f;
    public static float LOD2 = 200.0f;
    public static float LOD3 = 400000.0f;
    public float TileSize = 400.0f;
    public float TileHeight = 4000.0f;
    public Material Material;

    public Texture2D[] Textures;
    public int NumTilesX;
    public int NumTilesY;

    [NonSerialized]
    public List<TerrainTile> tiles = new List<TerrainTile>();
    [NonSerialized]
    public Queue<TerrainTile> dirty = new Queue<TerrainTile>();

    void Start() {
        // Assert.AreEqual(NumTilesX * NumTilesY, Textures.Length);

        Instance = this;
        int id = 0;
        for(int x = 0;x < NumTilesX;x ++) {
            for(int y = 0;y < NumTilesY;y ++,id ++) {
                TerrainTile temp = CreateTerrainTile(x - NumTilesX/2, y - NumTilesY/2);
                temp.id = id;
                tiles.Add(temp);
                dirty.Enqueue(temp);
            }
        }
    }

    private TerrainTile CreateTerrainTile(int posx, int posy) {
        GameObject gameObject = new GameObject("TerrainTile: " + posx + ", " + posy);
        gameObject.transform.parent = transform;
        gameObject.transform.localPosition = new Vector3(posx * TileSize, 0, -posy * TileSize);

        TerrainTile terrainTile = gameObject.AddComponent<TerrainTile>();
        terrainTile.Material = Material;
        return terrainTile;
    }

    void Update() {
        if(dirty.Count == 0) return;
        TerrainTile nextDirty = dirty.Dequeue();
        nextDirty.LoadTerrain(Textures[nextDirty.id]);
    }

    #region sqr_lods

    public static float LOD1_sqr = LOD1 * LOD1;
    public static float LOD2_sqr = LOD2 * LOD2;
    public static float LOD3_sqr = LOD3 * LOD3;

    #endregion

    public static TerrainManager Instance;

}