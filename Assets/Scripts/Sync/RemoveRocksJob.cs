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

using System;
using System.Collections.Generic;
using EPPZ.Geometry.Model;
using UnityEngine;

public class RemoveRocksJob : Job {
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

        for(byte x = (byte) minx;x <= maxx;x ++) {
            for(byte y = (byte) miny;y <= maxy;y ++) {
                while(true) {
                    try {
                        var range = Utils.GetRocksToRemove(Polygon, x, y);
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
            TerrainManager.Instance.RocksData.RemoveAt(i);
        }
        foreach(TerrainTile tile in ToMarkDirty) {
            tile.DirtyStates |= TerrainTile.TerrainTileDirtyStates.ROCKS;
            TerrainManager.Instance.Dirty.Enqueue(tile);
        }
    }
}
