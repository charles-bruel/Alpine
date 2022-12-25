using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.Assertions;
using System.Threading;

public class TerrainManager : MonoBehaviour {
    public static float LOD1 = 100.0f;
    public static float LOD2 = 200.0f;
    public static float LOD3 = 400000.0f;
    public float TileSize = 400.0f;
    public float TileHeight = 4000.0f;
    public Material TerrainMaterial;
    public Material TreeMaterial;
    public Texture2D[] Textures;
    public int NumTilesX;
    public int NumTilesY;
    public Mesh[] TreeLODS1;
    public Mesh[] TreeLODS2;
    public Mesh RockModel;
    public Texture2D DecoMap;
    public int NumTrees = 16384;
    public int NumRocks = 16384;

    [NonSerialized]
    public List<TerrainTile> Tiles = new List<TerrainTile>();
    [NonSerialized]
    public Queue<TerrainTile> Dirty = new Queue<TerrainTile>();
    [NonSerialized]
    public TreePos[] TreesData;
    public RockPos[] RocksData;

    void Start() {
        // Assert.AreEqual(NumTilesX * NumTilesY, Textures.Length);

        Instance = this;
        int id = 0;
        for(int x = 0;x < NumTilesX;x ++) {
            for(int y = 0;y < NumTilesY;y ++,id ++) {
                TerrainTile temp = CreateTerrainTile(x - NumTilesX/2, y - NumTilesY/2);
                temp.id = id;
                Tiles.Add(temp);
                Dirty.Enqueue(temp);
            }
        }

        StartPlacementJobs();
    }

    private void StartPlacementJobs() {
        PlaceTreesJob job = new PlaceTreesJob();
        job.DecoMap = DecoMap.GetPixels();
        job.DecoMapSize = DecoMap.width;
        job.TreeCount = NumTrees;
        job.MapBounds = new Bounds(Vector3.zero, Vector3.zero);
        job.MapBounds.min = new Vector3((-NumTilesX/2) * TileSize, 0, (1 - NumTilesY/2) * TileSize);
        job.MapBounds.max = new Vector3((NumTilesX -NumTilesX/2) * TileSize, 0, (NumTilesY + 1 -NumTilesY/2) * TileSize);

        Thread thread = new Thread(new ThreadStart(job.Run));
		thread.Start();

        PlaceRocksJob job2 = new PlaceRocksJob();
        job2.DecoMap = job.DecoMap;
        job2.DecoMapSize = DecoMap.width;
        job2.RockCount = NumRocks;
        job2.MapBounds = job.MapBounds;

        Thread thread2 = new Thread(new ThreadStart(job2.Run));
		thread2.Start();
    }

    private TerrainTile CreateTerrainTile(int posx, int posy) {
        GameObject gameObject = new GameObject("TerrainTile: " + posx + ", " + posy);
        gameObject.transform.parent = transform;
        gameObject.transform.localPosition = new Vector3(posx * TileSize, 0, -posy * TileSize);

        TerrainTile terrainTile = gameObject.AddComponent<TerrainTile>();
        terrainTile.TerrainMaterial = TerrainMaterial;
        terrainTile.TreeMaterial = TreeMaterial;

        terrainTile.posx = posx;
        terrainTile.posy = posy;

        return terrainTile;
    }

    void Update() {
        if(Dirty.Count == 0) return;
        TerrainTile nextDirty = Dirty.Dequeue();
        if((nextDirty.DirtyStates & TerrainTile.TerrainTileDirtyStates.TERRAIN) != 0) {
            nextDirty.LoadTerrain(Textures[nextDirty.id]);
            nextDirty.DirtyStates &= ~TerrainTile.TerrainTileDirtyStates.TERRAIN;
        } else {
            Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
            bounds.min = new Vector3(nextDirty.posx * TileSize, -1, -nextDirty.posy * TileSize);
            bounds.max = new Vector3((nextDirty.posx + 1) * TileSize, TileSize + 1, (-nextDirty.posy + 1) * TileSize);
            if((nextDirty.DirtyStates & TerrainTile.TerrainTileDirtyStates.TREES) != 0) {
                nextDirty.RecreateTreeMesh(bounds, TreeLODS1[2], TreeLODS2[2]);
                nextDirty.DirtyStates &= ~TerrainTile.TerrainTileDirtyStates.TREES;
            }
            if((nextDirty.DirtyStates & TerrainTile.TerrainTileDirtyStates.ROCKS) != 0) {
                nextDirty.RecreateRockMesh(bounds, RockModel);
                nextDirty.DirtyStates &= ~TerrainTile.TerrainTileDirtyStates.ROCKS;
            }
        }
    }

    public Vector3 Project(Vector2 coord) {
        Vector3 startCoord = new Vector3(coord.x, TileHeight * 1.1f, coord.y);
        RaycastHit hit;
        if(Physics.Raycast(startCoord, Vector3.down, out hit, Mathf.Infinity)) {
            return hit.point;
        }
        return new Vector3(coord.x, 0, coord.y);
    }

    public RaycastHit? Raycast(Vector2 coord) {
        Vector3 startCoord = new Vector3(coord.x, TileHeight * 1.1f, coord.y);
        RaycastHit hit;
        if(Physics.Raycast(startCoord, Vector3.down, out hit, Mathf.Infinity)) {
            return hit;
        }
        return null;
    }

    #region sqr_lods

    public static float LOD1_sqr = LOD1 * LOD1;
    public static float LOD2_sqr = LOD2 * LOD2;
    public static float LOD3_sqr = LOD3 * LOD3;

    #endregion

    public static TerrainManager Instance;

}