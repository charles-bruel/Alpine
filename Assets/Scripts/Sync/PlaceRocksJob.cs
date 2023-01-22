using System.Threading;
using UnityEngine;

public class PlaceRocksJob : Job
{
    public GridArray<RockPos> Data;
    public Bounds MapBounds;
    public Color[] DecoMap;
    public int DecoMapSize;
    public int RockCount;
    public float MinSize;
    public float MaxSize;

    public void Run() {
        // Simple spin lock
        if(LoadTerrainJob.ActiveJobs > 0) {
            Thread.Sleep(100);
        }

        Data = new GridArray<RockPos>(RockCount, (byte) TerrainManager.Instance.NumTilesX, (byte) TerrainManager.Instance.NumTilesY);

        System.Random random = new System.Random();
		int i = 0;

        Vector2 size = MapBounds.size.ToHorizontal();
        Vector2 min = MapBounds.min.ToHorizontal();

		while (i < RockCount)
		{
			Vector2 normalized = new Vector2((float)random.NextDouble(), (float)random.NextDouble());
			Vector2 position = new Vector2(normalized.x * size.x + min.x, normalized.y * size.y + min.y);
			int x = Mathf.FloorToInt(normalized.x * DecoMapSize);
			int y = Mathf.FloorToInt(normalized.y * DecoMapSize);
            int index = x + y * DecoMapSize;
            if(index < 0 || index > DecoMap.Length) continue;
			float b = DecoMap[index].b - 0.1f;
			if (random.NextDouble() <= (double)b)
			{
                RockPos value = new RockPos();
                value.pos.x = position.x;
                value.pos.y = 0;//Will get populated later
                value.pos.z = position.y;
                value.normal = Vector3.zero;//Will get populated later
                value.scale = random.NextFloat(MinSize, MaxSize);
                Data.Add(value);
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
            TerrainManager.Instance.Tiles[i].DirtyStates |= TerrainTile.TerrainTileDirtyStates.ROCKS;
            TerrainManager.Instance.Dirty.Enqueue(TerrainManager.Instance.Tiles[i]);
        }
    }
}