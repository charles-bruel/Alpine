using System.Threading;
using UnityEngine;

public class PlaceTreesJob : Job
{
    public TreePos[] Data;
    public Bounds MapBounds;
    public Color[] DecoMap;
    public int DecoMapSize;
    public int TreeCount;

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
        Data = new TreePos[TreeCount];

        System.Random random = new System.Random();
		int i = 0;

        Vector2 size = MapBounds.size.ToHorizontal();
        Vector2 min = MapBounds.min.ToHorizontal();

		while (i < TreeCount)
		{
			Vector2 normalized = new Vector2((float)random.NextDouble(), (float)random.NextDouble());
            Vector2 normalizedWorldPos = new Vector2(1 - normalized.x, 1 - normalized.y);
			Vector2 position = new Vector2(normalizedWorldPos.x * size.x + min.x, normalizedWorldPos.y * size.y + min.y);
			int x = Mathf.FloorToInt(normalized.x * DecoMapSize);
			int y = Mathf.FloorToInt(normalized.y * DecoMapSize);
            int index = x * DecoMapSize + y;
            if(index < 0 || index > DecoMap.Length) continue;
			float g = DecoMap[index].g - 0.1f;
			if (random.NextDouble() <= (double)g)
			{
                Data[i].pos.x = position.x;
                Data[i].pos.y = 0;//Will get populated later
                Data[i].pos.z = position.y;
                Data[i].rot = (float)(random.NextDouble() * 2.0 * 3.141592653589793);
                Data[i].scale = (float)(random.NextDouble() * 0.5 + 0.75);
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