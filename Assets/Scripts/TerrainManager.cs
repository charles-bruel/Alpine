using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.Assertions;
using System.Threading;

public class TerrainManager : MonoBehaviour {
    [Header("Tile & Map Information")]
    public float TileSize = 400.0f;
    public float TileHeight = 4000.0f;
    public Texture2D[] Textures;
    public int NumTilesX;
    public int NumTilesY;
    [Header("Materials & Shaders")]
    public Material TerrainMaterial;
    public Material ObjectMaterial;
    public Material ObjectInstanceMaterial;
    public ComputeShader CullingShader;
    [Header("Scatter Models")]
    public Mesh Tree1;
    public Mesh Tree2;
    public Mesh TreeLOD1;
    public Mesh TreeLOD2;
    public Mesh RockModel;
    public float RockSnowMultiplier;
    public float Tree1SnowMultiplier;
    public float Tree2SnowMultiplier;
    public float TreeLOD1SnowMultiplier;
    public float TreeLOD2SnowMultiplier;

    [Header("Scatter Settings")]
    public Texture2D DecoMap;
    public int NumTrees = 16384;
    public int NumRocks = 16384;
    [Header("LOD distances")]
    public float LOD_Distance = 200.0f;
    [NonSerialized]
    // We create a copy of the ObjectMaterial so we can give it settings without messing up the main material
    public Material SharedRuntimeObjectMaterial;
    [NonSerialized]
    public List<TerrainTile> Tiles = new List<TerrainTile>();
    [NonSerialized]
    public Queue<TerrainTile> Dirty = new Queue<TerrainTile>();
    [NonSerialized]
    public bool TreeLODRenderersDirty = false;
    [NonSerialized]
    public TreePos[] TreesData;
    [NonSerialized]
    public RockPos[] RocksData;
    [NonSerialized]
    public TreeLODRenderer TreeLODRenderer1;
    [NonSerialized]
    public TreeLODRenderer TreeLODRenderer2;


    void Start() {
        // Assert.AreEqual(NumTilesX * NumTilesY, Textures.Length);
        CreateTreeLODRenderers();

        SharedRuntimeObjectMaterial = new Material(ObjectMaterial);

        Vector4 bounds = new Vector4(
            (-NumTilesX/2) * TileSize, (1 - NumTilesY/2) * TileSize,
            (NumTilesX -NumTilesX/2) * TileSize, (NumTilesY + 1 -NumTilesY/2) * TileSize
        );

        if(SharedRuntimeObjectMaterial.HasVector("_Bounds")) {
            SharedRuntimeObjectMaterial.SetVector("_Bounds", bounds);
        }
        if(TreeLODRenderer1.InstanceMaterial.HasVector("_Bounds")) {
            TreeLODRenderer1.InstanceMaterial.SetVector("_Bounds", bounds);
        }
        if(TreeLODRenderer2.InstanceMaterial.HasVector("_Bounds")) {
            TreeLODRenderer2.InstanceMaterial.SetVector("_Bounds", bounds);
        }

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

    private void CreateTreeLODRenderers() {
        TreeLODRenderer1 = CreateTreeLODRenderer(1, Tree1, Tree1SnowMultiplier);
        TreeLODRenderer2 = CreateTreeLODRenderer(2, Tree2, Tree2SnowMultiplier);

        TreeLODRenderer1.InstanceMaterial = new Material(ObjectInstanceMaterial);
        TreeLODRenderer2.InstanceMaterial = new Material(ObjectInstanceMaterial);
    }

    private TreeLODRenderer CreateTreeLODRenderer(uint type, Mesh mesh, float snowMultiplier) {
        TreeLODRenderer treeLODRenderer = gameObject.AddComponent<TreeLODRenderer>();

        treeLODRenderer.instanceMesh = mesh;
        treeLODRenderer.subMeshIndex = 0;
        treeLODRenderer.Parameters = new TreeLODRenderer.Params();
        treeLODRenderer.Parameters.SnowMultiplier = snowMultiplier;
        treeLODRenderer.CullingShader = CullingShader;
        treeLODRenderer.TargetType = type;

        return treeLODRenderer;
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
        terrainTile.ObjectMaterial = SharedRuntimeObjectMaterial;

        terrainTile.posx = posx;
        terrainTile.posy = posy;

        return terrainTile;
    }

    void Update() {
        if(TreeLODRenderersDirty) {
            UpdateTreeLODBuffers();
            return;
        }

        if(Dirty.Count == 0) return;
        TerrainTile nextDirty = Dirty.Dequeue();
        if((nextDirty.DirtyStates & TerrainTile.TerrainTileDirtyStates.TERRAIN) != 0) {
            nextDirty.LoadTerrain(Textures[nextDirty.id]);
            nextDirty.DirtyStates &= ~TerrainTile.TerrainTileDirtyStates.TERRAIN;
        } else {
            Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
            bounds.min = new Vector3(nextDirty.posx * TileSize, -128, -nextDirty.posy * TileSize);
            bounds.max = new Vector3((nextDirty.posx + 1) * TileSize, TileSize + 128, (-nextDirty.posy + 1) * TileSize);
            if((nextDirty.DirtyStates & TerrainTile.TerrainTileDirtyStates.TREES) != 0) {
                nextDirty.RecreateTreeMesh(bounds, TreeLOD1, TreeLOD2);
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

    private void UpdateTreeLODBuffers() {
        int numTrees = 0;
        //We go through things twice to reduce memory allocations
        for(int i = 0;i < Tiles.Count;i ++) {
            Tiles[i].AdjustTreeRendering();

            if(!Tiles[i].GetWithinLOD() || Tiles[i].DirtyStates != 0) continue;
            
            for(int j = 0;j < Tiles[i].LocalTreeData.Length;j ++) {
                numTrees++;
            }
        }
        TreePos[] treePosses = new TreePos[numTrees];
        Vector4 bounds = new Vector4();
        int id = 0;
        for(int i = 0;i < Tiles.Count;i ++) {
            if(!Tiles[i].GetWithinLOD() || Tiles[i].DirtyStates != 0) continue;

            for(int j = 0;j < Tiles[i].LocalTreeData.Length;j ++) {
                bounds.x = Mathf.Min(bounds.x, Tiles[i].posx * TileSize);
                bounds.y = Mathf.Min(bounds.y, -Tiles[i].posy * TileSize);
                bounds.z = Mathf.Max(bounds.z, (Tiles[i].posx + 1) * TileSize);
                bounds.w = Mathf.Max(bounds.w, (-Tiles[i].posy + 1) * TileSize);

                treePosses[id++] = Tiles[i].LocalTreeData[j];
            }
        }

        Bounds boundsFinal = new Bounds();
        boundsFinal.min = new Vector3(bounds.x, 0, bounds.y);
        boundsFinal.max = new Vector3(bounds.z, TileHeight + 128, bounds.w);
        TreeLODRenderer1.Bounds = boundsFinal;
        TreeLODRenderer2.Bounds = boundsFinal;

        TreeLODRenderer1.UpdateBuffers(treePosses);
        TreeLODRenderer2.UpdateBuffers(treePosses);

        TreeLODRenderersDirty = false;
    }

    public static TerrainManager Instance;

}