using System;
using System.Collections.Generic;
using EPPZ.Geometry.Model;
using UnityEngine;

public class RemoveTreesJob : Job {
    public Polygon Polygon;
    private List<int> ToRemove;
    private List<TerrainTile> ToMarkDirty;
    public void Run() {
        Rect bounds = Polygon.bounds;
        Vector2Int minPos = TerrainManager.Instance.GetTilePos(bounds.min);
        Vector2Int maxPos = TerrainManager.Instance.GetTilePos(bounds.max);
        int minx = minPos.x - 1;
        int maxx = maxPos.x + 1;
        int miny = minPos.y - 1;
        int maxy = maxPos.y + 1;
        if(minx < 0) minx = 0;
        if(maxx > TerrainManager.Instance.NumTilesX - 1) maxx = TerrainManager.Instance.NumTilesX - 1;
        if(miny < 0) miny = 0;
        if(maxy > TerrainManager.Instance.NumTilesY - 1) maxy = TerrainManager.Instance.NumTilesY - 1;

        ToRemove = new List<int>();
        ToMarkDirty = new List<TerrainTile>();

        // Big hack - the list of trees may update while we are determining what to remove
        // In that case, try again!
        for(byte x = (byte) minx;x <= maxx;x ++) {
            for(byte y = (byte) miny;y <= maxy;y ++) {
                while(true) {
                    try {
                        var range = Utils.GetTreesToRemove(Polygon, x, y);
                        ToRemove.AddRange(range);
                        if(range.Count > 0) {
                            TerrainTile tile = TerrainManager.Instance.Tiles[x + TerrainManager.Instance.NumTilesX * y];
                            ToMarkDirty.Add(tile);
                        }
                    } catch(InvalidOperationException) {
                        continue;
                    }
                    break;
                }
            }
        }

        if(ToRemove.Count == 0) return;
        
        lock(ASyncJobManager.completedJobsLock) {
        	ASyncJobManager.Instance.completedJobs.Enqueue(this);
		}
    }

    public override void Complete() {
        foreach(int i in ToRemove) {
            TerrainManager.Instance.TreesData.RemoveAt(i);
        }
        foreach(TerrainTile tile in ToMarkDirty) {
            tile.DirtyStates |= TerrainTile.TerrainTileDirtyStates.TREES;
            TerrainManager.Instance.Dirty.Enqueue(tile);
        }
        TerrainManager.Instance.TreeLODRenderersDirty = true;
    }
}
