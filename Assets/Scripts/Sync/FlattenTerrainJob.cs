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

        for(int hy = 0;hy < localHeights.GetLength(0);hy ++) {
            for(int hx = 0;hx < localHeights.GetLength(1);hx ++) {
                Vector2 t = new Vector2((float) hx / localHeights.GetLength(1), (float) hy / localHeights.GetLength(0));
                Vector2 pos = new Vector2(
                    Mathf.Lerp(bounds.min.x, bounds.max.x, t.x),
                    Mathf.Lerp(bounds.min.z, bounds.max.z, t.y)
                );

                float multiplier = 1;

                if(!polygon.ContainsPoint(pos)) {
                    float dist = polygon.DistanceToPoint(pos);
                    if(dist > falloff) continue;
                    multiplier = 1 - (dist / falloff);
                    
                    multiplier = 2 * multiplier * multiplier * multiplier + 3 * multiplier * multiplier;
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
                localHeights[hy, hx] = Mathf.Lerp(localHeights[hy, hx], value, multiplier);
            }
        }

        ToComplete.Enqueue(new TerrainModificationResult(heights[x - minx, y - miny].data, localHeights));
    }

    public override void Complete() {
        if(ToComplete.Count != 0) {
            TerrainModificationResult result = ToComplete.Dequeue();
            result.data.SetHeights(0, 0, result.heights);
            lock(ASyncJobManager.completedJobsLock) {
        	    ASyncJobManager.Instance.completedJobs.Enqueue(this);
		    }
        }
        
    }

    private struct TerrainModificationResult {
        public TerrainData data;
        public float[,] heights;

        public TerrainModificationResult(TerrainData data, float[,] heights) {
            this.heights = heights;
            this.data = data;
        }
    }
}