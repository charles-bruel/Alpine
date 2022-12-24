using System;
using UnityEngine;

public class LoadTerrain : CompletedJob {
    public Color[] InputData;
    public float[,] OutputData;
    public int Width;
    public TerrainData TerrainData;

    public void Run() {
        for (int i = 0; i < InputData.Length; i++)
		{
			Color color = InputData[i];
			int num = (int)(color.b * 255f) << 16 | (int)(color.g * 255f) << 8 | (int)(color.r * 255f);
			OutputData[i / Width, i % Width] = (float)num / 16777215f;
		}
        OutputData = Smooth(OutputData, Width, 2);
        ASyncJobManager.Instance.completedJobs.Enqueue(this);
    }

    public override void Complete() {
        try {
			TerrainData.SetHeights(0, 0, OutputData);
		} catch (Exception ex) {
			Debug.Log(ex.Message);
			Debug.Log(ex.StackTrace);
			Debug.Log("Error setting size. Make sure your heightmap is square and 2^n or 2^n+1 in size.");
		}
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