using System;
using System.Threading;
using UnityEngine;

public class LoadTerrainJob : Job {
    public Color[] InputData;
    public float[,] OutputData;
    public int Width;
    public TerrainData TerrainData;
	public TerrainTile Reference;

	public static int ActiveJobs = 0;

    public void Run() {
		Interlocked.Increment(ref ActiveJobs);

        for (int i = 0; i < InputData.Length; i++)
		{
			Color color = InputData[i];
			int b = (int)(color.b * 255f);
			int g = (int)(color.g * 255f);
			if(g == 0) b++;
			int r = (int)(color.r * 255f);
			if(r == 0) g++;
			int num = b << 16 | g << 8 | r;
			int x = Width - 1 - (i / Width);
			int y = Width - 1 - (i % Width);
			OutputData[y, x] = (float)num / 16777215f;
		}

		Reference.HeightData = OutputData;

		Reference.DirtyStates |= TerrainTile.TerrainTileDirtyStates.CONTOURS;
		
		lock(ASyncJobManager.completedJobsLock) {
        	ASyncJobManager.Instance.completedJobs.Enqueue(this);
		}
    }

    public override void Complete() {
        try {
			TerrainData.SetHeights(0, 0, OutputData);
		} catch (Exception ex) {
			Debug.Log(ex.Message);
			Debug.Log(ex.StackTrace);
			Debug.Log("Error setting size. Make sure your heightmap is square and 2^n or 2^n+1 in size.");
		}

		Interlocked.Decrement(ref ActiveJobs);
    }
}