using System;
using System.Threading;
using UnityEngine;

public class TerrainTile : MonoBehaviour {
    
    public int posx;
    public int posy;
    public int id;

    [NonSerialized]
    public Terrain TerrainComponent;
    [NonSerialized]
    public MeshFilter TreesComponent;
    [NonSerialized]
    public Material TerrainMaterial;
    [NonSerialized]
    public Material TreeMaterial;
    [NonSerialized]
    public bool HasTerrain = false;

    void Start() {
        GameObject terrain = new GameObject("Terrain");
        terrain.transform.parent = transform;
        terrain.transform.localPosition = Vector3.zero;

        TerrainComponent = terrain.AddComponent<Terrain>();
        TerrainComponent.terrainData = new TerrainData();
        TerrainComponent.materialTemplate = TerrainMaterial;

        TerrainCollider collider = terrain.AddComponent<TerrainCollider>();
        collider.terrainData = TerrainComponent.terrainData;

        GameObject trees = new GameObject("Trees");
        trees.transform.parent = transform;
        trees.transform.position = Vector3.zero;

        MeshRenderer meshRenderer = trees.AddComponent<MeshRenderer>();
        meshRenderer.material = TreeMaterial;

        TreesComponent = trees.AddComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        TreesComponent.mesh = mesh;
    }

    public void LoadTerrain(Texture2D texture2D)
	{
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

		Thread thread = new Thread(new ThreadStart(job.Run));
		thread.Start();

        HasTerrain = true;
	}

    public void RecreateTreeMesh(Bounds bounds, Mesh template) {
        float[] Data = TerrainManager.Instance.TreesData;

        //First we need to do our raycasts
        //We also use this to count the number of trees we need to place
        int numTrees = 0;

        for(int i = 0;i < Data.Length;i += PlaceTreesJob.FloatsPerTree) {
            Vector3 pos = new Vector3(Data[i + 0], Data[i + 1], Data[i + 2]);
            if(bounds.Contains(pos)) {
                Data[i + 1] = TerrainManager.Instance.Project(pos.ToHorizontal()).y;
                numTrees ++;
            }
        }
        
        // Debug.Log(numTrees);

        CreateTreeMeshJob job = new CreateTreeMeshJob();
		job.Bounds = bounds;
        job.Mesh = TreesComponent.mesh;
        job.NumTrees = numTrees;
        job.OldVertices = template.vertices;
        job.OldUVs = template.uv;
        job.OldTriangles = template.triangles;

		Thread thread = new Thread(new ThreadStart(job.Run));
		thread.Start();
    }

    public LODLevel GetLODLevel() {
        float sqrDist = (transform.position - Camera.main.transform.position).sqrMagnitude;
        if(sqrDist < TerrainManager.LOD1_sqr) {
            return LODLevel.LOD1;
        }
        if(sqrDist < TerrainManager.LOD2_sqr) {
            return LODLevel.LOD2;
        }
        if(sqrDist < TerrainManager.LOD3_sqr) {
            return LODLevel.LOD3;
        }
        return LODLevel.LOD4;
    }
}