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

using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class TerrainTile : MonoBehaviour {
    
    public int PosX;
    public int PosY;
    public byte IndexX;
    public byte IndexY;
    public int id;
    [NonSerialized]
    public TerrainTileDirtyStates DirtyStates = TerrainTileDirtyStates.TERRAIN;
    [NonSerialized]
    public Terrain TerrainComponent;
    [NonSerialized]
    public MeshFilter TreesComponent;
    [NonSerialized]
    public MeshFilter RocksComponent;
    [NonSerialized]
    public MeshFilter ContoursComponent;
    [NonSerialized]
    public Material TerrainMaterial;
    [NonSerialized]
    public Material ObjectMaterial;
    [NonSerialized]
    public Material RockMaterial;
    [NonSerialized]
    public Material ContourMaterial;
    [NonSerialized]
    public float[,] HeightData;
    [NonSerialized]
    public ContourDefinition Contours;

    private bool hasFullyInitialized;
    public bool HasFullyInitialized {
        set {
            bool flag = value && value != hasFullyInitialized;
            hasFullyInitialized = value;
            if(flag) {
                TerrainManager.Instance.TestInitialization();
            }
        }
        get { return hasFullyInitialized; }
    }

    private bool Initialized = false;

    private void Initialize(int texSize) {
        if(Initialized) {
            return;
        }
        Initialized = true;

        GameObject terrain = new GameObject("Terrain");
        terrain.transform.parent = transform;
        terrain.transform.localPosition = Vector3.zero;
        terrain.layer = LayerMask.NameToLayer("Terrain");

        TerrainComponent = terrain.AddComponent<Terrain>();
        TerrainComponent.terrainData = new TerrainData();
        TerrainComponent.materialTemplate = TerrainMaterial;
        // We want resolution/pixelError = 128
        // Thus we want pixelError = resolution/128
        TerrainComponent.heightmapPixelError = texSize / 128f;

        TerrainCollider collider = terrain.AddComponent<TerrainCollider>();
        collider.terrainData = TerrainComponent.terrainData;

        GameObject trees = new GameObject("Trees");
        trees.transform.parent = transform;
        trees.transform.position = Vector3.zero;

        GameObject rocks = new GameObject("Rocks");
        rocks.transform.parent = transform;
        rocks.transform.position = Vector3.zero;

        MeshRenderer treeMeshRenderer = trees.AddComponent<MeshRenderer>();
        treeMeshRenderer.material = ObjectMaterial;

        TreesComponent = trees.AddComponent<MeshFilter>();
        Mesh treeMesh = new Mesh();
        TreesComponent.mesh = treeMesh;
        treeMesh.indexFormat = IndexFormat.UInt32;
        treeMesh.MarkDynamic();

        MeshRenderer rockMeshRenderer = rocks.AddComponent<MeshRenderer>();
        rockMeshRenderer.material = RockMaterial;

        RocksComponent = rocks.AddComponent<MeshFilter>();
        Mesh rockMesh = new Mesh();
        RocksComponent.mesh = rockMesh;
        rockMesh.indexFormat = IndexFormat.UInt32;
        rockMesh.MarkDynamic();

        LayerMask treeRockLayer = LayerMask.NameToLayer("Trees & Rocks");
        trees.layer = treeRockLayer;
        rocks.layer = treeRockLayer;

        GameObject contours = new GameObject("Contours");
        contours.transform.parent = transform;
        contours.transform.position = Vector3.zero;

        MeshRenderer contoursMeshRenderer = contours.AddComponent<MeshRenderer>();
        contoursMeshRenderer.material = ContourMaterial;

        ContoursComponent = contours.AddComponent<MeshFilter>();
        Mesh contoursMesh = new Mesh();
        ContoursComponent.mesh = contoursMesh;
        contoursMesh.indexFormat = IndexFormat.UInt32;
        contoursMesh.MarkDynamic();

        contours.SetActive(false);
        contours.layer = LayerMask.NameToLayer("Both");
    }

    public void LoadTerrain(Texture2D texture2D, Bounds bounds) {
        Initialize(texture2D.width);
		TerrainComponent.terrainData.heightmapResolution = texture2D.width;
		Vector3 size = TerrainComponent.terrainData.size;
		size.y = TerrainManager.Instance.TileHeight;
		size.x = (float)TerrainManager.Instance.TileSize;
		size.z = (float)TerrainManager.Instance.TileSize;
		TerrainComponent.terrainData.size = size;
		int width = texture2D.width;
		float[,] array = new float[width, width];
		Color[] pixels = texture2D.GetPixels();
		
		LoadTerrainJob job = new LoadTerrainJob();
		job.InputData = pixels;
		job.OutputData = array;
		job.Width = width;
		job.TerrainData = TerrainComponent.terrainData;
        job.Reference = this;

        job.Initialize();

		Thread thread = new Thread(new ThreadStart(job.Run));
		thread.Start();
	}

    public void RecreateTreeMesh(Bounds bounds, TerrainManager.TreeTypeDescriptor[] descriptors) {
        GridArray<TreePos> Data = TerrainManager.Instance.TreesData;

        //First we need to do our raycasts to assign height
        //We also use this to count the number of trees we need to place
        int[] numTrees = new int[descriptors.Length];
        int totalTrees = 0;

        var enumerator = Data.GetEnumerator(IndexX, IndexY);
        while(enumerator.MoveNext()) {
            TreePos temp = enumerator.Current;
                if(temp.pos.y == 0) {
                temp.pos = TerrainManager.Instance.Project(enumerator.Current.pos.ToHorizontal());
                float v = 1 - (temp.pos.y / TerrainManager.Instance.TileHeight);
                float altitudeFactor = TerrainManager.Instance.AltitudeAdjustFactor;
                temp.scale *= v * (1 - altitudeFactor) + altitudeFactor;
                enumerator.CurrentMut = temp;
            }
            numTrees[enumerator.Current.type]++;
            totalTrees++;
        }

        CreateTreeMeshJob job = new CreateTreeMeshJob();

		job.Bounds = bounds;
        job.PosX = IndexX;
        job.PosY = IndexY;
        job.MeshTarget = TreesComponent.mesh;
        job.Descriptors = new CreateTreeMeshJob.TreeTypeDescriptorForJob[descriptors.Length];
        for(int i = 0;i < descriptors.Length;i ++) {
            job.Descriptors[i] = new CreateTreeMeshJob.TreeTypeDescriptorForJob();
            job.Descriptors[i].NumTrees = numTrees[i];

            job.Descriptors[i].SnowMultiplier = descriptors[i].LODSnowMultiplier;

            job.Descriptors[i].OldNormals   = descriptors[i].LODMesh.normals;
            job.Descriptors[i].OldTriangles = descriptors[i].LODMesh.triangles;
            job.Descriptors[i].OldUVs       = descriptors[i].LODMesh.uv;
            job.Descriptors[i].OldVertices  = descriptors[i].LODMesh.vertices;
        }

        job.Initialize();
		Thread thread = new Thread(new ThreadStart(job.Run));
		thread.Start();
    }

    public void RecreateRockMesh(Bounds bounds, Mesh template) {
        GridArray<RockPos> Data = TerrainManager.Instance.RocksData;

        //First we need to do our raycasts to assign height and angle
        //We also use this to count the number of rocks we need to place
        int numRocks = 0;

        var enumerator = Data.GetEnumerator(IndexX, IndexY);
        while(enumerator.MoveNext()) {
            RaycastHit? hit = TerrainManager.Instance.Raycast(enumerator.Current.pos.ToHorizontal());
            numRocks ++;
            if(hit == null) {
                continue;
            }
            RockPos temp = enumerator.Current;
            temp.pos = hit.Value.point;
            temp.normal = hit.Value.normal;
            enumerator.CurrentMut = temp;
        }
        
        CreateRockMeshJob job = new CreateRockMeshJob();

		job.Bounds = bounds;
        job.PosX = IndexX;
        job.PosY = IndexY;
        job.MeshTarget = RocksComponent.mesh;
        job.NumRocks = numRocks;
        job.OldVertices = template.vertices;
        job.OldUVs = template.uv;
        job.OldTriangles = template.triangles;
        job.OldNormals = template.normals;

        job.Initialize();
		Thread thread = new Thread(new ThreadStart(job.Run));
		thread.Start();
    }

    public void RecreateContours(Bounds bounds, ContourLayersDefinition layers) {
        RecreateContourMeshJob job = new RecreateContourMeshJob();

		job.Bounds = bounds;
        job.PosX = IndexX;
        job.PosY = IndexY;
        job.MeshTarget = ContoursComponent.mesh;
        job.Tile = this;
        job.LayersDefinition = layers;

        job.Initialize();
		Thread thread = new Thread(new ThreadStart(job.Run));
		thread.Start();
    }

    private bool PrevLODLevel = false;
    
    void Update() {
        if(DirtyStates == 0) {
            bool currentLODLevel = GetWithinLOD();
            if(currentLODLevel != PrevLODLevel) {
                TerrainManager.Instance.TreeLODRenderersDirty = true;
                PrevLODLevel = currentLODLevel;
            }
        }
    }

    public void AdjustTreeRendering() {
        if(TreesComponent == null) return;
        
        bool currentLODLevel = GetWithinLOD();
        // TreesComponent.gameObject.SetActive(!currentLODLevel);
        TreesComponent.gameObject.layer = currentLODLevel ? LayerMask.NameToLayer("Trees & Rocks Overlay Only") : LayerMask.NameToLayer("Trees & Rocks");
    }

    public bool GetWithinLOD() {
        Vector3 centerPos = transform.position;
        centerPos.x += TerrainManager.Instance.TileSize / 2;
        centerPos.y += TerrainManager.Instance.TileHeight / 2;
        centerPos.z += TerrainManager.Instance.TileSize / 2;
        float sqrDist = (centerPos - Camera.main.transform.position).sqrMagnitude;
        return sqrDist < TerrainManager.Instance.LOD_Distance * TerrainManager.Instance.LOD_Distance;
    }

    public enum TerrainTileDirtyStates : uint {
        TERRAIN = 1,
        TREES = 2,
        ROCKS = 4,
        CONTOURS = 8,
    }
}