using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.Assertions;
using System.Threading;

public class TerrainManager : MonoBehaviour {
    [Header("Map")]
    public Map Map;
    [Header("Materials & Shaders")]
    public Material TerrainMaterial;
    public Material ObjectMaterial;
    public Material ContourMaterial;
    public Material ObjectInstanceMaterial;
    public Material SnowCatcher;
    public Material SnowCatcherRecent;
    public ComputeShader CullingShader;
    [Header("Scatters")]
    public Mesh RockModel;
    public float RockSnowMultiplier;
    public TreeTypeDescriptor[] TreeTypeDescriptors;
    [Header("LOD settings")]
    public float LOD_Distance = 200.0f;
    [Header("Linking")]
    public WeatherController WeatherController;
    [Header("Other")]
    public ContourLayersDefinition ContourLayersDefinition;
    [NonSerialized]
    public Material RockMaterial;
    [NonSerialized]
    public List<TerrainTile> Tiles = new List<TerrainTile>();
    [NonSerialized]
    public Queue<TerrainTile> Dirty = new Queue<TerrainTile>();
    [NonSerialized]
    public bool TreeLODRenderersDirty = false;
    [NonSerialized]
    public GridArray<TreePos> TreesData;
    [NonSerialized]
    public GridArray<RockPos> RocksData;
    [NonSerialized]
    public TreeLODRenderer[] TreeLODRenderers;
    [NonSerialized]
    public Bounds TerrainBounds;

    [NonSerialized]
    public float TileSize;
    [NonSerialized]
    public float TileHeight;
    [NonSerialized]
    public Texture2D[] Textures;
    [NonSerialized]
    public int NumTilesX;
    [NonSerialized]
    public int NumTilesY;
    [NonSerialized]
    public Texture2D DecoMap;
    [NonSerialized]
    public int NumTrees;
    [NonSerialized]
    public int NumRocks;
    [NonSerialized]
    public Texture2D WeatherMap;
    [NonSerialized]
    public float MinTreeHeight;
    [NonSerialized]
    public float MaxTreeHeight;
    [NonSerialized]
    public float AltitudeAdjustFactor;

    private LayerMask TerrainLayerMask;


    void Start() {
        TerrainLayerMask = ~LayerMask.NameToLayer("Terrain");

        CopyMapData();

        CreateTreeLODRenderers();

        ObjectMaterial = new Material(ObjectMaterial);
        RockMaterial = new Material(ObjectMaterial);
        TerrainMaterial = new Material(TerrainMaterial);

        Vector4 bounds = new Vector4(
            (-NumTilesX/2) * TileSize, (-NumTilesY/2) * TileSize,
            (NumTilesX -NumTilesX/2) * TileSize, (NumTilesY -NumTilesY/2) * TileSize
        );

        UpdateMaterials(bounds, WeatherMap);

        TerrainBounds = new Bounds();
        TerrainBounds.min = new Vector3(bounds.x, 0, bounds.y);
        TerrainBounds.max = new Vector3(bounds.z, TileHeight, bounds.w);

        Instance = this;
        int id = 0;
        for(int y = 0;y < NumTilesY;y ++) {
            for(int x = 0;x < NumTilesX;x ++,id ++) {
                TerrainTile temp = CreateTerrainTile(x - NumTilesX/2, y - NumTilesY/2);
                temp.id = id;
                Tiles.Add(temp);
                Dirty.Enqueue(temp);
            }
        }

        StartPlacementJobs();
    }

    private void CopyMapData() {
        TileSize             = Map.TileSize;
        TileHeight           = Map.TileHeight;
        NumTilesX            = Map.NumTilesX;
        NumTilesY            = Map.NumTilesY;
        DecoMap              = Map.DecoMap;
        NumTrees             = Map.NumTrees;
        NumRocks             = Map.NumRocks;
        WeatherMap           = Map.WeatherMap;
        MinTreeHeight        = Map.MinTreeHeight;
        MaxTreeHeight        = Map.MaxTreeHeight;
        AltitudeAdjustFactor = Map.AltitudeAdjustFactor;

        Textures = new Texture2D[NumTilesX * NumTilesY];
        for(int i = 0;i < Textures.Length;i ++) {
            Textures[i] = Resources.Load<Texture2D>(Map.TexturePath + "\\height-" + i);
        }
    }

    private void CreateTreeLODRenderers() {
        TreeLODRenderers = new TreeLODRenderer[TreeTypeDescriptors.Length];
        for(uint i = 0;i < TreeTypeDescriptors.Length;i ++) {
            TreeLODRenderers[i] = CreateTreeLODRenderer(i, TreeTypeDescriptors[i].Mesh, TreeTypeDescriptors[i].SnowMultiplier);
            TreeLODRenderers[i].InstanceMaterial = new Material(ObjectInstanceMaterial);
        }
    }

    private void UpdateMaterials(Vector4 bounds, Texture2D weatherMap) {
        if(RockMaterial.HasVector("_Bounds")) {
            RockMaterial.SetVector("_Bounds", bounds);
        }

        if(RockMaterial.HasTexture("_SnowTex")) {
            RockMaterial.SetTexture("_SnowTex", weatherMap);
        }

        if(ObjectMaterial.HasVector("_Bounds")) {
            ObjectMaterial.SetVector("_Bounds", bounds);
        }

        if(ObjectMaterial.HasTexture("_SnowTex")) {
            ObjectMaterial.SetTexture("_SnowTex", weatherMap);
        }

        if(SnowCatcher.HasVector("_Bounds")) {
            SnowCatcher.SetVector("_Bounds", bounds);
        }

        if(SnowCatcher.HasTexture("_SnowTex")) {
            SnowCatcher.SetTexture("_SnowTex", weatherMap);
        }

        if(SnowCatcherRecent.HasVector("_Bounds")) {
            SnowCatcherRecent.SetVector("_Bounds", bounds);
        }

        if(SnowCatcherRecent.HasTexture("_SnowTex")) {
            SnowCatcherRecent.SetTexture("_SnowTex", weatherMap);
        }

        if(TerrainMaterial.HasVector("_Bounds")) {
            TerrainMaterial.SetVector("_Bounds", bounds);
        }

        if(TerrainMaterial.HasTexture("_SnowTex")) {
            TerrainMaterial.SetTexture("_SnowTex", weatherMap);
        }

        for(int i = 0;i < TreeLODRenderers.Length;i ++) {
            if(TreeLODRenderers[i].InstanceMaterial.HasVector("_Bounds")) {
                TreeLODRenderers[i].InstanceMaterial.SetVector("_Bounds", bounds);
            }

            if(TreeLODRenderers[i].InstanceMaterial.HasTexture("_SnowTex")) {
                TreeLODRenderers[i].InstanceMaterial.SetTexture("_SnowTex", weatherMap);
            }
        }
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

        job.DecoMap     = DecoMap.GetPixels();
        job.DecoMapSize = DecoMap.width;
        job.TreeCount   = NumTrees;
        job.MinHeight   = MinTreeHeight;
        job.MaxHeight   = MaxTreeHeight;

        job.MapBounds = new Bounds(Vector3.zero, Vector3.zero);
        job.MapBounds.min = new Vector3((-NumTilesX/2) * TileSize, 0, (-NumTilesY/2) * TileSize);
        job.MapBounds.max = new Vector3((NumTilesX -NumTilesX/2) * TileSize, 0, (NumTilesY -NumTilesY/2) * TileSize);

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
        gameObject.transform.localPosition = new Vector3(posx * TileSize, 0, posy * TileSize);

        TerrainTile terrainTile = gameObject.AddComponent<TerrainTile>();
        terrainTile.TerrainMaterial = TerrainMaterial;
        terrainTile.RockMaterial = RockMaterial;
        terrainTile.ObjectMaterial = ObjectMaterial;
        terrainTile.ContourMaterial = ContourMaterial;

        terrainTile.PosX = posx;
        terrainTile.PosY = posy;
        terrainTile.IndexX = (byte) (posx + NumTilesX / 2);
        terrainTile.IndexY = (byte) (posy + NumTilesY / 2);

        return terrainTile;
    }

    void Update() {
        UpdateSnowMaterials();

        if(TreeLODRenderersDirty) {
            UpdateTreeLODBuffers();
            return;
        }

        if(Dirty.Count == 0) return;
        TerrainTile nextDirty = Dirty.Dequeue();
        if((nextDirty.DirtyStates & TerrainTile.TerrainTileDirtyStates.TERRAIN) != 0) {
            Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
            bounds.min = new Vector3(nextDirty.PosX * TileSize, 0, nextDirty.PosY * TileSize);
            bounds.max = new Vector3((nextDirty.PosX + 1) * TileSize, TileHeight, (nextDirty.PosY + 1) * TileSize);
            nextDirty.LoadTerrain(Textures[nextDirty.id], bounds);
            nextDirty.DirtyStates &= ~TerrainTile.TerrainTileDirtyStates.TERRAIN;
        } else if((nextDirty.DirtyStates & TerrainTile.TerrainTileDirtyStates.CONTOURS) != 0) {
            Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
            bounds.min = new Vector3(nextDirty.PosX * TileSize, 0, nextDirty.PosY * TileSize);
            bounds.max = new Vector3((nextDirty.PosX + 1) * TileSize, TileHeight, (nextDirty.PosY + 1) * TileSize);
            nextDirty.RecreateContours(bounds, ContourLayersDefinition);
            nextDirty.DirtyStates &= ~TerrainTile.TerrainTileDirtyStates.CONTOURS;
        } else {
            Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
            bounds.min = new Vector3(nextDirty.PosX * TileSize, -128, nextDirty.PosY * TileSize);
            bounds.max = new Vector3((nextDirty.PosX + 1) * TileSize, TileHeight + 128, (nextDirty.PosY + 1) * TileSize);
            if((nextDirty.DirtyStates & TerrainTile.TerrainTileDirtyStates.TREES) != 0) {
                nextDirty.RecreateTreeMesh(bounds, TreeTypeDescriptors);
                nextDirty.DirtyStates &= ~TerrainTile.TerrainTileDirtyStates.TREES;
            }
            if((nextDirty.DirtyStates & TerrainTile.TerrainTileDirtyStates.ROCKS) != 0) {
                nextDirty.RecreateRockMesh(bounds, RockModel);
                nextDirty.DirtyStates &= ~TerrainTile.TerrainTileDirtyStates.ROCKS;
            }
        }
    }

    private void UpdateSnowMaterials() {
        WeatherController.UpdateMaterial(RockMaterial, WeatherController.SnowCatcherType.Base);
        WeatherController.UpdateMaterial(ObjectMaterial, WeatherController.SnowCatcherType.Recent);
        WeatherController.UpdateMaterial(TerrainMaterial, WeatherController.SnowCatcherType.Base);
        WeatherController.UpdateMaterial(SnowCatcher, WeatherController.SnowCatcherType.Base);
        WeatherController.UpdateMaterial(SnowCatcherRecent, WeatherController.SnowCatcherType.Base);

        for(int i = 0;i < TreeLODRenderers.Length;i ++) {
            WeatherController.UpdateMaterial(TreeLODRenderers[i].InstanceMaterial, WeatherController.SnowCatcherType.Recent);
        }
    }

    public Vector3 Project(Vector2 coord) {
        Vector3 startCoord = new Vector3(coord.x, TileHeight * 1.1f, coord.y);
        RaycastHit hit;
        if(Physics.Raycast(startCoord, Vector3.down, out hit, Mathf.Infinity, TerrainLayerMask)) {
            return hit.point;
        }
        return new Vector3(coord.x, 0, coord.y);
    }

    public RaycastHit? Raycast(Vector2 coord) {
        Vector3 startCoord = new Vector3(coord.x, TileHeight * 1.1f, coord.y);
        RaycastHit hit;
        if(Physics.Raycast(startCoord, Vector3.down, out hit, Mathf.Infinity, TerrainLayerMask)) {
            return hit;
        }
        return null;
    }

    private void UpdateTreeLODBuffers() {
        int numTrees = 0;
        //We go through things twice to reduce memory allocations
        for(int i = 0;i < Tiles.Count;i ++) {
            Tiles[i].AdjustTreeRendering();

            if(!Tiles[i].GetWithinLOD()/* || Tiles[i].DirtyStates != 0*/) continue;

            numTrees += TreesData.GetCountInCell(Tiles[i].IndexX, Tiles[i].IndexY);
        }
        TreePos[] treePosses = new TreePos[numTrees];
        Vector4 bounds = new Vector4();
        int id = 0;
        for(int i = 0;i < Tiles.Count;i ++) {
            if(!Tiles[i].GetWithinLOD()/* || Tiles[i].DirtyStates != 0*/) continue;

            bounds.x = Mathf.Min(bounds.x, Tiles[i].PosX * TileSize);
            bounds.y = Mathf.Min(bounds.y, (Tiles[i].PosY + 1) * TileSize);
            bounds.z = Mathf.Max(bounds.z, (Tiles[i].PosX + 1) * TileSize);
            bounds.w = Mathf.Max(bounds.w, (Tiles[i].PosY + 2) * TileSize);

            var enumerator = TreesData.GetEnumerator(Tiles[i].IndexX, Tiles[i].IndexY);
            while(enumerator.MoveNext()) {
                treePosses[id++] = enumerator.Current;
            }
        }

        Bounds boundsFinal = new Bounds();
        boundsFinal.min = new Vector3(bounds.x, 0, bounds.y);
        boundsFinal.max = new Vector3(bounds.z, TileHeight + 128, bounds.w);

        for(int i = 0;i < TreeLODRenderers.Length;i ++) {
            TreeLODRenderers[i].Bounds = boundsFinal;
            TreeLODRenderers[i].UpdateBuffers(treePosses);
        }

        TreeLODRenderersDirty = false;
    }

    public Vector2Int GetTilePos(Vector3 position) {
        int x = Mathf.FloorToInt(position.x / TileSize);
        int y = Mathf.FloorToInt(position.z / TileSize);
        x += NumTilesX / 2;
        y += NumTilesY / 2;
        return new Vector2Int(x, y);
    }

    public Vector2Int GetTilePos(Vector2 position) {
        int x = Mathf.FloorToInt(position.x / TileSize);
        int y = Mathf.FloorToInt(position.y / TileSize);
        x += NumTilesX / 2;
        y += NumTilesY / 2;
        return new Vector2Int(x, y);
    }

    public static TerrainManager Instance;

    [System.Serializable]
    public class TreeTypeDescriptor {
        public Mesh Mesh;
        public Mesh LODMesh;
        public float SnowMultiplier;
        public float LODSnowMultiplier;
    } 

}