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

using System.Threading;
using UnityEngine;

public class PlaceTreesJob : Job
{
    public GridArray<TreePos> Data;
    public Bounds MapBounds;
    public Color[] DecoMap;
    public int DecoMapSize;
    public int TreeCount;
    public float MinHeight;
    public float MaxHeight;

    public void Run() {
        // Simple spin lock
        if(LoadTerrainJob.ActiveJobs > 0) {
            Thread.Sleep(100);
        }

        Data = new GridArray<TreePos>(TreeCount, (byte) TerrainManager.Instance.NumTilesX, (byte) TerrainManager.Instance.NumTilesY);

        System.Random random = new System.Random();
		int i = 0;

        MapBounds.Expand(-10f);

        Vector2 size = MapBounds.size.ToHorizontal();
        Vector2 min = MapBounds.min.ToHorizontal();

		while (i < TreeCount)
		{
			Vector2 normalized = new Vector2((float)random.NextDouble(), (float)random.NextDouble());
			Vector2 position = new Vector2(normalized.x * size.x + min.x, normalized.y * size.y + min.y);
			int x = Mathf.FloorToInt(normalized.x * DecoMapSize);
			int y = Mathf.FloorToInt(normalized.y * DecoMapSize);
            int index = x + y * DecoMapSize;
            if(index < 0 || index > DecoMap.Length) continue;
            //We check again place a tree there, then choose
			float v = DecoMap[index].g + DecoMap[index].r;
			if (random.NextDouble() <= (double)v)
			{
                float treeChoose = (float) random.NextDouble() * v;
                TreePos value = new TreePos();
                if(DecoMap[index].r < treeChoose) {
                    value.type = 0;
                } else {
                    value.type = 1;
                }
                value.pos.x = position.x;
                value.pos.y = 0;//Will get populated later
                value.pos.z = position.y;
                value.rot = (float)(random.NextDouble() * 2.0 * 3.141592653589793);

                value.scale = random.NextFloat(MinHeight, MaxHeight);
                // value.enabled = 1;
                Data.Add(value);
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
            TerrainManager.Instance.Tiles[i].DirtyStates |= TerrainTile.TerrainTileDirtyStates.TREES;
            TerrainManager.Instance.Dirty.Enqueue(TerrainManager.Instance.Tiles[i]);
        }

        LoadingScreen.INSTANCE.LoadingTasks--;
    }

    public void Initialize() {
        LoadingScreen.INSTANCE.LoadingTasks++;
    }
}