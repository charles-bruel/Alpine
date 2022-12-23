using System;
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
		for (int i = 0; i < pixels.Length; i++)
		{
			Color color = pixels[i];
            if(i < 255) Debug.Log(color.b * 255f);
			int num = (int)(color.b * 255f) << 16 | (int)(color.g * 255f) << 8 | (int)(color.r * 255f);
			array[i / width, i % width] = (float)num / 16777215f;
		}
		try
		{
			TerrainComponent.terrainData.SetHeights(0, 0, Smooth(array, width, 2));
		}
		catch (Exception ex)
		{
			Debug.Log(ex.Message);
			Debug.Log(ex.StackTrace);
			Debug.Log("Error setting size. Make sure your heightmap is square and 2^n or 2^n+1 in size.");
		}
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

    public static float[,] Smooth(float[,] data, int size, int blurSize)
	{
		float[,] array = new float[size, size];
		for (int i = 0; i < size; i++)
		{
			for (int j = 0; j < size; j++)
			{
				if (i < blurSize || j < blurSize || i >= size - blurSize || j >= size - blurSize)
				{
					array[j, i] = data[j, i];
				}
				else
				{
					float num = 0f;
					for (int k = -blurSize; k <= blurSize; k++)
					{
						for (int l = -blurSize; l <= blurSize; l++)
						{
							num += data[j + k, i + l];
						}
					}
					array[j, i] = num / (float)((blurSize * 2 + 1) * (blurSize * 2 + 1));
				}
			}
		}
		return array;
	}
}