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
using System.Collections.Generic;
using System;
using UnityEngine.Assertions;
using System.Threading;
using System.Linq;
using UnityEngine.AI;
using UnityEngine.Tilemaps;

public class TerrainManager : MonoBehaviour {
    [Header("Materials & Shaders")]
    public Material TerrainMaterial;
    public Material ObjectMaterial;
    public Material ContourMaterial;
    public Material ObjectInstanceMaterial;
    public Material SnowCatcher;
    public Material SnowCatcherRecent;
    public Material VertexColorWroldOverlay;
    public Material OverlayTreeMaterial;
    public Material OverlayRockMaterial;
    public ComputeShader CullingShader;
    public ComputeShader CullingShaderOverlay;
    [Header("Scatters")]
    public Mesh RockModel;
    public float RockSnowMultiplier;
    public TreeTypeDescriptor[] TreeTypeDescriptors;
    [Header("LOD settings")]
    public float LOD_Distance = 200.0f;
    [Header("Linking")]
    public WeatherController WeatherController;
    [Header("Overlay Layer Settings")]
    public OverlayCamera OverlayCamera;
    public GameObject Overlays;
    public Mesh OverlayRenderMesh;
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
    public OverlayRenderer OverlayRendererTree;
    [NonSerialized]
    public OverlayRenderer OverlayRendererRock;
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
    public float MinRockSize;
    [NonSerialized]
    public float MaxRockSize;
    [NonSerialized]
    public float AltitudeAdjustFactor;

    public LayerMask InverseTerrainLayerMask;

    private bool Initialized = false;

    public static IMap TargetMap;

    public void Initialize() {
        // Check if we are loading from a save
        if(GameController.TargetSaveGame != null) {
            SaveManager.LoadMap(GameController.TargetSaveGame);
        }

        // Load a default map
        // We are probably in the unity editor testing something
        if(TargetMap == null) {
            TargetMap = GetAllMaps()[0];
        }
        TargetMap.Load(this);

        CreateTreeLODRenderers();
        CreateOverlayRenderers();

        ObjectMaterial = new Material(ObjectMaterial);
        RockMaterial = new Material(ObjectMaterial);
        TerrainMaterial = new Material(TerrainMaterial);

        Vector4 bounds = new Vector4(
            (-NumTilesX/2) * TileSize, (-NumTilesY/2) * TileSize,
            (NumTilesX -NumTilesX/2) * TileSize, (NumTilesY -NumTilesY/2) * TileSize
        );

        TerrainBounds = new Bounds();
        TerrainBounds.min = new Vector3(bounds.x, 0, bounds.y);
        TerrainBounds.max = new Vector3(bounds.z, TileHeight, bounds.w);

        UpdateMaterials(bounds, WeatherMap);

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

        LOD_Distance = ConfigHelper.GetFile(ConfigHelper.CONFIG_NAME).GetFloat("tree_lod_distance");

        Initialized = true;
    }

    public void CopyMapData(AlpineMap alpineMap) {
        TileSize             = alpineMap.TileSize;
        TileHeight           = alpineMap.TileHeight;
        NumTilesX            = alpineMap.NumTilesX;
        NumTilesY            = alpineMap.NumTilesY;
        DecoMap              = alpineMap.DecoMap;
        NumTrees             = alpineMap.NumTrees;
        NumRocks             = alpineMap.NumRocks;
        WeatherMap           = alpineMap.WeatherMap;
        MinTreeHeight        = alpineMap.MinTreeHeight;
        MaxTreeHeight        = alpineMap.MaxTreeHeight;
        MinRockSize          = alpineMap.MinRockSize;
        MaxRockSize          = alpineMap.MaxRockSize;
        AltitudeAdjustFactor = alpineMap.AltitudeAdjustFactor;

        Textures = new Texture2D[NumTilesX * NumTilesY];
        for(int i = 0;i < Textures.Length;i ++) {
            Textures[i] = Resources.Load<Texture2D>(alpineMap.TexturePath + "\\height-" + i);
        }
    }

    public void CopyMapData(AvalancheMap map) {
        map.LoadTextures();

        // Use 4x4 tile grid
        TileSize             = map.Size / 4;
        TileHeight           = map.Height;
        NumTilesX            = 4;
        NumTilesY            = 4;
        DecoMap              = map.DecorationMaps;
        NumTrees             = map.Trees;
        NumRocks             = map.Rocks;
        WeatherMap           = map.GenerateWeatherMap();
        MinTreeHeight        = 1;
        MaxTreeHeight        = 4;
        MinRockSize          = 3;
        MaxRockSize          = 1;
        AltitudeAdjustFactor = 0.5f;

        Textures = new Texture2D[NumTilesX * NumTilesY];
        Textures = map.SplitIntoTiles(4);
    }

    private void CreateTreeLODRenderers() {
        TreeLODRenderers = new TreeLODRenderer[TreeTypeDescriptors.Length];
        for(uint i = 0;i < TreeTypeDescriptors.Length;i ++) {
            TreeLODRenderers[i] = CreateTreeLODRenderer(i, TreeTypeDescriptors[i].Mesh, TreeTypeDescriptors[i].SnowMultiplier);
            TreeLODRenderers[i].InstanceMaterial = new Material(ObjectInstanceMaterial);
        }
    }

    private void CreateOverlayRenderers() {
        OverlayRendererTree = Overlays.AddComponent<OverlayRenderer>();

        OverlayRendererTree.instanceMesh = OverlayRenderMesh;
        OverlayRendererTree.subMeshIndex = 0;
        OverlayRendererTree.CullingShader = CullingShaderOverlay;
        OverlayRendererTree.OverlayCamera = OverlayCamera.Camera;
        OverlayRendererTree.InstanceMaterial = OverlayTreeMaterial;

        OverlayRendererRock = Overlays.AddComponent<OverlayRenderer>();

        OverlayRendererRock.instanceMesh = OverlayRenderMesh;
        OverlayRendererRock.subMeshIndex = 0;
        OverlayRendererRock.CullingShader = CullingShaderOverlay;
        OverlayRendererRock.OverlayCamera = OverlayCamera.Camera;
        OverlayRendererRock.InstanceMaterial = OverlayRockMaterial;
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

        if(VertexColorWroldOverlay.HasVector("_Bounds")) {
            VertexColorWroldOverlay.SetVector("_Bounds", bounds);
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
        job.TreeCount   = (int) (NumTrees * ConfigHelper.GetFile(ConfigHelper.CONFIG_NAME).GetFloat("tree_mul"));
        job.MinHeight   = MinTreeHeight;
        job.MaxHeight   = MaxTreeHeight;

        job.MapBounds = new Bounds(Vector3.zero, Vector3.zero);
        job.MapBounds.min = new Vector3((-NumTilesX/2) * TileSize, 0, (-NumTilesY/2) * TileSize);
        job.MapBounds.max = new Vector3((NumTilesX -NumTilesX/2) * TileSize, 0, (NumTilesY -NumTilesY/2) * TileSize);

        job.Initialize();

        Thread thread = new Thread(new ThreadStart(job.Run));
		thread.Start();

        PlaceRocksJob job2 = new PlaceRocksJob();
        job2.DecoMap     = job.DecoMap;
        job2.DecoMapSize = DecoMap.width;
        job2.RockCount   = (int) (NumRocks * ConfigHelper.GetFile(ConfigHelper.CONFIG_NAME).GetFloat("rock_mul"));
        job2.MapBounds   = job.MapBounds;
        job2.MinSize     = MinRockSize;
        job2.MaxSize     = MaxRockSize;

        job2.Initialize();

        Thread thread2 = new Thread(new ThreadStart(job2.Run));
		thread2.Start();
    }

    private TerrainTile CreateTerrainTile(int posx, int posy) {
        GameObject gameObject = new GameObject("TerrainTile: " + posx + ", " + posy);
        gameObject.transform.parent = transform;
        gameObject.transform.localPosition = new Vector3(posx * TileSize, 0, posy * TileSize);

        TerrainTile terrainTile = gameObject.AddComponent<TerrainTile>();
        terrainTile.TerrainMaterial = TerrainMaterial;
        terrainTile.RockMaterial = SnowCatcher;
        terrainTile.ObjectMaterial = ObjectMaterial;
        terrainTile.ContourMaterial = ContourMaterial;

        terrainTile.PosX = posx;
        terrainTile.PosY = posy;
        terrainTile.IndexX = (byte) (posx + NumTilesX / 2);
        terrainTile.IndexY = (byte) (posy + NumTilesY / 2);

        return terrainTile;
    }

    void Update() {
        if(!Initialized) return;

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
            nextDirty.RecreateContours(bounds, ContourLayersDefinition.Convert(TileHeight));
            nextDirty.DirtyStates &= ~TerrainTile.TerrainTileDirtyStates.CONTOURS;
        } else {
            Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
            bounds.min = new Vector3(nextDirty.PosX * TileSize, -128, nextDirty.PosY * TileSize);
            bounds.max = new Vector3((nextDirty.PosX + 1) * TileSize, TileHeight + 128, (nextDirty.PosY + 1) * TileSize);
            if((nextDirty.DirtyStates & TerrainTile.TerrainTileDirtyStates.TREES) != 0) {
                nextDirty.RecreateTreeMesh(bounds, TreeTypeDescriptors);
                nextDirty.DirtyStates &= ~TerrainTile.TerrainTileDirtyStates.TREES;

                UpdateOverlayLODBuffers();
            }
            if((nextDirty.DirtyStates & TerrainTile.TerrainTileDirtyStates.ROCKS) != 0) {
                nextDirty.RecreateRockMesh(bounds, RockModel);
                nextDirty.DirtyStates &= ~TerrainTile.TerrainTileDirtyStates.ROCKS;

                UpdateOverlayLODBuffers();
            }
            nextDirty.HasFullyInitialized = true;
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
        if(Physics.Raycast(startCoord, Vector3.down, out hit, Mathf.Infinity, InverseTerrainLayerMask)) {
            return hit.point;
        }
        return new Vector3(coord.x, 0, coord.y);
    }

    public RaycastHit? Raycast(Vector2 coord) {
        Vector3 startCoord = new Vector3(coord.x, TileHeight * 1.1f, coord.y);
        RaycastHit hit;
        if(Physics.Raycast(startCoord, Vector3.down, out hit, Mathf.Infinity, InverseTerrainLayerMask)) {
            return hit;
        }
        return null;
    }

    private void UpdateTreeLODBuffers() {
        if(TreesData == null) return;
        
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
            bounds.y = Mathf.Min(bounds.y, Tiles[i].PosY * TileSize);
            bounds.z = Mathf.Max(bounds.z, (Tiles[i].PosX + 1) * TileSize);
            bounds.w = Mathf.Max(bounds.w, (Tiles[i].PosY + 1) * TileSize);

            var enumerator = TreesData.GetEnumerator(Tiles[i].IndexX, Tiles[i].IndexY);
            while(enumerator.MoveNext()) {
                treePosses[id++] = enumerator.Current;
            }
        }

        Bounds boundsFinal = new()
        {
            min = new Vector3(bounds.x, 0, bounds.y),
            max = new Vector3(bounds.z, TileHeight + 100, bounds.w)
        };

        for (int i = 0;i < TreeLODRenderers.Length;i ++) {
            TreeLODRenderers[i].Bounds = boundsFinal;
            TreeLODRenderers[i].UpdateBuffers(treePosses);
        }

        TreeLODRenderersDirty = false;
    }

    private void UpdateOverlayLODBuffers() {
        // TODO: Clean up

        // Tree overlay
        if(TreesData != null) {
            int numTrees = 0;
            //We go through things twice to reduce memory allocations
            for(int i = 0;i < Tiles.Count;i ++) {
                numTrees += TreesData.GetCountInCell(Tiles[i].IndexX, Tiles[i].IndexY);
            }

            Vector3[] data = new Vector3[numTrees];
            Vector4 bounds = new Vector4();
            int id = 0;
            for(int i = 0;i < Tiles.Count;i ++) {
                bounds.x = Mathf.Min(bounds.x, Tiles[i].PosX * TileSize);
                bounds.y = Mathf.Min(bounds.y, Tiles[i].PosY * TileSize);
                bounds.z = Mathf.Max(bounds.z, (Tiles[i].PosX + 1) * TileSize);
                bounds.w = Mathf.Max(bounds.w, (Tiles[i].PosY + 1) * TileSize);

                var enumerator = TreesData.GetEnumerator(Tiles[i].IndexX, Tiles[i].IndexY);
                while(enumerator.MoveNext()) {
                    TreePos current = enumerator.Current;
                    data[id++] = new Vector3(current.pos.x, current.pos.z, current.scale);
                }
            }

            Bounds boundsFinal = new()
            {
                min = new Vector3(bounds.x, 0, bounds.y),
                max = new Vector3(bounds.z, TileHeight + 100, bounds.w)
            };

            
            OverlayRendererTree.Bounds = boundsFinal;
            OverlayRendererTree.UpdateBuffers(data);
        }

        // Tree overlay
        if(TreesData != null) {
            int numTrees = 0;
            //We go through things twice to reduce memory allocations
            for(int i = 0;i < Tiles.Count;i ++) {
                numTrees += RocksData.GetCountInCell(Tiles[i].IndexX, Tiles[i].IndexY);
            }

            Vector3[] data = new Vector3[numTrees];
            Vector4 bounds = new Vector4();
            int id = 0;
            for(int i = 0;i < Tiles.Count;i ++) {
                bounds.x = Mathf.Min(bounds.x, Tiles[i].PosX * TileSize);
                bounds.y = Mathf.Min(bounds.y, Tiles[i].PosY * TileSize);
                bounds.z = Mathf.Max(bounds.z, (Tiles[i].PosX + 1) * TileSize);
                bounds.w = Mathf.Max(bounds.w, (Tiles[i].PosY + 1) * TileSize);

                var enumerator = RocksData.GetEnumerator(Tiles[i].IndexX, Tiles[i].IndexY);
                while(enumerator.MoveNext()) {
                    RockPos current = enumerator.Current;
                    data[id++] = new Vector3(current.pos.x, current.pos.z, current.scale);
                }
            }

            Bounds boundsFinal = new()
            {
                min = new Vector3(bounds.x, 0, bounds.y),
                max = new Vector3(bounds.z, TileHeight + 100, bounds.w)
            };

            
            OverlayRendererRock.Bounds = boundsFinal;
            OverlayRendererRock.UpdateBuffers(data);
        }
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

    public void TestInitialization() {
        for(int i = 0;i < Tiles.Count;i ++) {
            if(!Tiles[i].HasFullyInitialized) return;
        }
        GameController.Instance.TerrainManagerDoneCallback();
    }

    public static TerrainManager Instance;

    public static List<IMap> GetAllMaps() {
        List<IMap> maps = new List<IMap>();

        // Get all AlpineMap objects
        AlpineMap[] alpineMaps = Resources.LoadAll<AlpineMap>("Maps");
        maps.AddRange(from alpineMap in alpineMaps where alpineMap.Include select alpineMap as IMap);

        // Load all AvalancheMap objects from disk
        List<AvalancheMap> avalancheMaps = AvalancheMap.GetMaps();
        maps.AddRange(avalancheMaps);

        return maps;
    }

    [System.Serializable]
    public class TreeTypeDescriptor {
        public Mesh Mesh;
        public Mesh LODMesh;
        public float SnowMultiplier;
        public float LODSnowMultiplier;
    } 

}