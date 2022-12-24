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
    public GameObject TreesComponent;
    [NonSerialized]
    public Material Material;

    void Start() {
        GameObject terrain = new GameObject("Terrain");
        terrain.transform.parent = transform;
        terrain.transform.localPosition = Vector3.zero;

        TerrainComponent = terrain.AddComponent<Terrain>();
        TerrainComponent.terrainData = new TerrainData();
        TerrainComponent.materialTemplate = Material;

        TerrainCollider collider = terrain.AddComponent<TerrainCollider>();
        collider.terrainData = TerrainComponent.terrainData;
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
		
		LoadTerrain job = new LoadTerrain();
		job.InputData = pixels;
		job.OutputData = array;
		job.Width = width;
		job.TerrainData = TerrainComponent.terrainData;

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