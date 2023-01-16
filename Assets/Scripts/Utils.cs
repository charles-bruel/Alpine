using UnityEngine;
using System.Collections.Generic;
using EPPZ.Geometry.Model;

public static class Utils {
    public static bool RemoveTreesByPolygon(Polygon polygon) {
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

        List<int> toRemove = new List<int>();

        for(byte x = (byte) minx;x <= maxx;x ++) {
            for(byte y = (byte) miny;y <= maxy;y ++) {
                var range = GetToRemove(polygon, x, y);
                toRemove.AddRange(range);
                if(range.Count > 0) {
                    TerrainTile tile = TerrainManager.Instance.Tiles[x + TerrainManager.Instance.NumTilesX * y];
                    tile.DirtyStates |= TerrainTile.TerrainTileDirtyStates.TREES;
                    TerrainManager.Instance.Dirty.Enqueue(tile);
                }
            }
        }

        if(toRemove.Count == 0) return false;

        foreach(int i in toRemove) {
            TerrainManager.Instance.TreesData.RemoveAt(i);
        }

        TerrainManager.Instance.TreeLODRenderersDirty = true;
        return true;
    }

    private static List<int> GetToRemove(Polygon polygon, byte x, byte y) {
        List<int> toReturn = new List<int>();

        var enumerator = TerrainManager.Instance.TreesData.GetIndexEnumerator(x, y);
        while(enumerator.MoveNext()) {
            var element = TerrainManager.Instance.TreesData[enumerator.Current];
            if(polygon.ContainsPoint(element.pos.ToHorizontal())) {
                toReturn.Add(enumerator.Current);
            }
        }

        return toReturn;
    }
}