using System.Threading;
using UnityEngine;

public class PlaceTreesJob : Job
{
    public float[] Data;
    public Bounds MapBounds;
    public Color[] DecoMap;
    public int DecoMapSize;
    public int TreeCount;

    public static readonly int FloatsPerTree = 4;

    public void Run() {
        // Simple spin lock
        if(LoadTerrainJob.ActiveJobs > 0) {
            Thread.Sleep(100);
        }

        // Data layout:
        // Float ID | Purpose
        //    0     | x pos
        //    1     | y pos
        //    2     | z pos
        //    3     | rotation
        Data = new float[TreeCount * FloatsPerTree];

        System.Random random = new System.Random();
		int i = 0;
		while (i < TreeCount)
		{
			Vector2 normalized = new Vector2((float)random.NextDouble(), (float)random.NextDouble());
			Vector2 size = MapBounds.size.ToHorizontal();
            Vector2 min = MapBounds.min.ToHorizontal();
			Vector2 position = new Vector2(normalized.x * size.x + min.x, normalized.y * size.y + min.y);
            normalized.x = 1 - normalized.x;
            normalized.y = 1 - normalized.y;
			int x = Mathf.FloorToInt(normalized.x * DecoMapSize);
			int y = Mathf.FloorToInt(normalized.y * DecoMapSize);
            int index = x * DecoMapSize + y;
			float g = DecoMap[index].g;
			if (random.NextDouble() <= (double)g)
			{
				// Scale = (float)(random.NextDouble() + 0.5),
				float RotationY = (float)(random.NextDouble() * 2.0 * 3.141592653589793);
                Data[i * FloatsPerTree + 0] = position.x;
                Data[i * FloatsPerTree + 1] = 0;//Will get populated later
                Data[i * FloatsPerTree + 2] = position.y;
                Data[i * FloatsPerTree + 3] = RotationY;

				i++;
			}
		}

        lock(ASyncJobManager.completedJobsLock) {
        	ASyncJobManager.Instance.completedJobs.Enqueue(this);
		}
    }

    public override void Complete() {
        TerrainManager.Instance.TreesData = Data;
        for(int i = 0;i < TerrainManager.Instance.Tiles.Count;i ++) {
            TerrainManager.Instance.Dirty.Enqueue(TerrainManager.Instance.Tiles[i]);
        }
    }
}