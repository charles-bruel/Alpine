using System;
using System.Collections.Generic;
using EPPZ.Geometry.Model;
using UnityEngine;

public class TerrainModificationController : MonoBehaviour
{
    public List<AlpinePolygon> TerrainModificationEffects;

    //TODO: Check to make sure it's initialized
    public void Register(AlpinePolygon effect) {
        if((effect.Flags & PolygonFlags.FLATTEN) == 0) return;
        TerrainModificationEffects.Add(effect);
        bool flattenUp = (effect.Flags & PolygonFlags.FLATTEN_UP) != 0;
        bool flattenDown = (effect.Flags & PolygonFlags.FLATTEN_DOWN) != 0;
        Debug.Log(flattenUp);
        Debug.Log(flattenDown);
        Flatten(effect.Polygon, flattenUp, flattenDown, effect.Height);
    }

    private void Flatten(Polygon polygon, bool flattenUp, bool flattenDown, float height) {
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
                FlattenTile(polygon, flattenUp, flattenDown, height, x, y);
            }
        }
    }

    private void FlattenTile(Polygon polygon, bool flattenUp, bool flattenDown, float height, byte x, byte y) {
        TerrainTile tile = TerrainManager.Instance.Tiles[x + TerrainManager.Instance.NumTilesX * y];
        Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
        bounds.min = new Vector3(tile.PosX * TerrainManager.Instance.TileSize, 0, tile.PosY * TerrainManager.Instance.TileSize);
        bounds.max = new Vector3((tile.PosX + 1) * TerrainManager.Instance.TileSize, TerrainManager.Instance.TileHeight, (tile.PosY + 1) * TerrainManager.Instance.TileSize);

        TerrainData data = tile.TerrainComponent.terrainData;
        float[,] heights = data.GetHeights(0, 0, data.heightmapResolution, data.heightmapResolution);

        for(int hy = 0;hy < heights.GetLength(0);hy ++) {
            for(int hx = 0;hx < heights.GetLength(1);hx ++) {
                Vector2 t = new Vector2((float) hx / heights.GetLength(1), (float) hy / heights.GetLength(0));
                Vector2 pos = new Vector2(
                    Mathf.Lerp(bounds.min.x, bounds.max.x, t.x),
                    Mathf.Lerp(bounds.min.z, bounds.max.z, t.y)
                );

                if(!polygon.ContainsPoint(pos)) continue;

                float value = heights[hy, hx];
                //TODO: Optimize this?
                value *= TerrainManager.Instance.TileHeight;

                if(flattenUp) {
                    if(value < height) value = height;
                }
                if(flattenDown) {
                    if(value > height) value = height;
                }

                value /= TerrainManager.Instance.TileHeight;
                heights[hy, hx] = value;
            }
        }

        data.SetHeights(0, 0, heights);
    }

    public void Initialize() {
        // Instance = this;

        // AlpinePolygon effect = new AlpinePolygon();
        // effect.Height = 600;
        // effect.Polygon = Polygon.PolygonWithPoints(new Vector2[] {
        //     new Vector2(-200, -200),
        //     new Vector2(-200,  200),
        //     new Vector2( 200,  200),
        //     new Vector2( 200, -200)
        // });
        // effect.Flags = PolygonFlags.FLATTEN;

        // Register(effect);
    }

    public static TerrainModificationController Instance;
}
