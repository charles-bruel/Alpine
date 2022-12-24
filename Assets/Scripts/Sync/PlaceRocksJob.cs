using System.Threading;
using UnityEngine;

public class PlaceRocksJob : Job
{
    public RockPos[] Data;
    public Bounds MapBounds;
    public Color[] DecoMap;
    public int DecoMapSize;
    public int RockCount;

    public void Run() {
        // Simple spin lock
        if(LoadTerrainJob.ActiveJobs > 0) {
            Thread.Sleep(100);
        }

        Data = new RockPos[RockCount];

        System.Random random = new System.Random();
		int i = 0;

        Vector2 size = MapBounds.size.ToHorizontal();
        Vector2 min = MapBounds.min.ToHorizontal();

		while (i < RockCount)
		{
			Vector2 normalized = new Vector2((float)random.NextDouble(), (float)random.NextDouble());
            Vector2 normalizedWorldPos = new Vector2(1 - normalized.x, 1 - normalized.y);
			Vector2 position = new Vector2(normalizedWorldPos.x * size.x + min.x, normalizedWorldPos.y * size.y + min.y);
			int x = Mathf.FloorToInt(normalized.x * DecoMapSize);
			int y = Mathf.FloorToInt(normalized.y * DecoMapSize);
            int index = x * DecoMapSize + y;
            if(index < 0 || index > DecoMap.Length) continue;
			float b = DecoMap[index].b - 0.1f;
			if (random.NextDouble() <= (double)b)
			{
                Data[i].pos.x = position.x;
                Data[i].pos.y = 0;//Will get populated later
                Data[i].pos.z = position.y;
                Data[i].normal = Vector3.zero;//Will get populated later
                Data[i].scale = (float)(random.NextDouble() * 0.5 + 0.75);
				i++;
			}
		}

        lock(ASyncJobManager.completedJobsLock) {
        	ASyncJobManager.Instance.completedJobs.Enqueue(this);
		}
    }

    public override void Complete() {
        TerrainManager.Instance.RocksData = Data;
        for(int i = 0;i < TerrainManager.Instance.Tiles.Count;i ++) {
            TerrainManager.Instance.Dirty.Enqueue(TerrainManager.Instance.Tiles[i]);
        }
    }
}