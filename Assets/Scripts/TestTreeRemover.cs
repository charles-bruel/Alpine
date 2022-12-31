using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestTreeRemover : MonoBehaviour {

    private readonly float DIST = 20;

    void Update() {
        if(Input.GetKey("p")) {
            Vector2Int tilePos = TerrainManager.Instance.GetTilePos(transform.position);
            int minx = tilePos.x - 1;
            int maxx = tilePos.x + 1;
            int miny = tilePos.y - 1;
            int maxy = tilePos.y + 1;
            if(minx < 0) minx = 0;
            if(maxx > TerrainManager.Instance.NumTilesX - 1) maxx = TerrainManager.Instance.NumTilesX - 1;
            if(miny < 0) miny = 0;
            if(maxy > TerrainManager.Instance.NumTilesY - 1) maxy = TerrainManager.Instance.NumTilesY - 1;

            List<int> toRemove = new List<int>();

            for(byte x = (byte) minx;x <= maxx;x ++) {
                for(byte y = (byte) miny;y <= maxy;y ++) {
                    var range = GetToRemove(x, y);
                    toRemove.AddRange(range);
                    if(range.Count > 0) {
                        TerrainTile tile = TerrainManager.Instance.Tiles[x + TerrainManager.Instance.NumTilesX * y];
                        tile.DirtyStates |= TerrainTile.TerrainTileDirtyStates.TREES;
                        TerrainManager.Instance.Dirty.Enqueue(tile);
                    }
                }
            }

            if(toRemove.Count == 0) return;

            foreach(int i in toRemove) {
                TerrainManager.Instance.TreesData.RemoveAt(i);
            }

            TerrainManager.Instance.TreeLODRenderersDirty = true;
        }
    }

    private List<int> GetToRemove(byte x, byte y) {
        List<int> toReturn = new List<int>();

        var enumerator = TerrainManager.Instance.TreesData.GetIndexEnumerator(x, y);
        while(enumerator.MoveNext()) {
            var element = TerrainManager.Instance.TreesData[enumerator.Current];
            if((element.pos - transform.position).sqrMagnitude < DIST * DIST) {
                toReturn.Add(enumerator.Current);
            }
        }

        return toReturn;
    }
}
