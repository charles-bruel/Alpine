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

using EPPZ.Geometry.Model;
using UnityEngine;
using System.Collections.Generic;

public class FlattenTerrainJob : Job {
    public Polygon polygon;
    public bool flattenUp;
    public bool flattenDown;
    public float height;
    public float falloff = 20;
    private Queue<TerrainModificationResult> ToComplete = new Queue<TerrainModificationResult>();
    private TerrainModificationResult[,] heights;

    public void Initialize() {
        Rect bounds = polygon.bounds;
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
        
        heights = new TerrainModificationResult[maxx + 1 - minx,maxy + 1 - miny];

        for(byte x = (byte) minx;x <= maxx;x ++) {
            for(byte y = (byte) miny;y <= maxy;y ++) {
                TerrainTile tile = TerrainManager.Instance.Tiles[x + TerrainManager.Instance.NumTilesX * y];
                TerrainData data = tile.TerrainComponent.terrainData;
                heights[x - minx,y - miny] = new TerrainModificationResult(data, data.GetHeights(0, 0, data.heightmapResolution, data.heightmapResolution));
            }
        }
    }

    public void Run() {
        Rect bounds = polygon.bounds;
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

        for(byte x = (byte) minx;x <= maxx;x ++) {
            for(byte y = (byte) miny;y <= maxy;y ++) {
                FlattenTile(polygon, flattenUp, flattenDown, height, x, y, minx, miny);
            }
        }

        lock(ASyncJobManager.completedJobsLock) {
        	ASyncJobManager.Instance.completedJobs.Enqueue(this);
		}
    }

    private void FlattenTile(Polygon polygon, bool flattenUp, bool flattenDown, float height, byte x, byte y, int minx, int miny) {
        TerrainTile tile = TerrainManager.Instance.Tiles[x + TerrainManager.Instance.NumTilesX * y];
        Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
        bounds.min = new Vector3(tile.PosX * TerrainManager.Instance.TileSize, 0, tile.PosY * TerrainManager.Instance.TileSize);
        bounds.max = new Vector3((tile.PosX + 1) * TerrainManager.Instance.TileSize, TerrainManager.Instance.TileHeight, (tile.PosY + 1) * TerrainManager.Instance.TileSize);

        float[,] localHeights = heights[x - minx, y - miny].heights;
        float[,] weights = new float[localHeights.GetLength(0), localHeights.GetLength(1)];

        for(int hy = 0;hy < localHeights.GetLength(0);hy ++) {
            for(int hx = 0;hx < localHeights.GetLength(1);hx ++) {
                Vector2 t = new Vector2((float) hx / localHeights.GetLength(1), (float) hy / localHeights.GetLength(0));
                Vector2 pos = new Vector2(
                    Mathf.Lerp(bounds.min.x, bounds.max.x, t.x),
                    Mathf.Lerp(bounds.min.z, bounds.max.z, t.y)
                );

                float multiplier = 0;

                if(polygon.ContainsPoint(pos)) {
                    multiplier = 1;
                } else {
                    float dist = polygon.DistanceToPoint(pos);
                    if(dist <= falloff) {
                        multiplier = 1 - (dist / falloff);
                    
                        // multiplier = 2 * multiplier * multiplier * multiplier + 3 * multiplier * multiplier;
                    }
                }

                float value = localHeights[hy, hx];
                //TODO: Optimize this?
                value *= TerrainManager.Instance.TileHeight;

                if(flattenUp) {
                    if(value < height) value = height;
                }
                if(flattenDown) {
                    if(value > height) value = height;
                }

                value /= TerrainManager.Instance.TileHeight;
                localHeights[hy, hx] = value;
                weights[hy, hx] = multiplier;
            }
        }

        ToComplete.Enqueue(new TerrainModificationResult(heights[x - minx, y - miny].data, localHeights, weights));
    }

    public override void Complete() {
        if(ToComplete.Count != 0) {
            TerrainModificationResult result = ToComplete.Dequeue();
            float[,] data = result.data.GetHeights(0, 0, result.data.heightmapResolution, result.data.heightmapResolution);
            for(int i = 0;i < data.GetLength(0);i ++) {
                for(int j = 0;j < data.GetLength(1);j ++) {
                    float t = result.weights[i, j];
                    data[i, j] = data[i, j] * (1 - t) + t * result.heights[i, j];
                }
            }
            result.data.SetHeights(0, 0, data);
            lock(ASyncJobManager.completedJobsLock) {
        	    ASyncJobManager.Instance.completedJobs.Enqueue(this);
		    }
        }
        
    }

    private struct TerrainModificationResult {
        public TerrainData data;
        public float[,] heights;
        public float[,] weights;

        public TerrainModificationResult(TerrainData data, float[,] heights) {
            this.heights = heights;
            this.data = data;
            this.weights = new float[0,0];
        }

        public TerrainModificationResult(TerrainData data, float[,] heights, float[,] weights) {
            this.heights = heights;
            this.data = data;
            this.weights = weights;
        }
    }
}