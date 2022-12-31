using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestTreeBrush : MonoBehaviour {

    private readonly float DIST = 20;
    private readonly int TREESPERSECOND = 100;

    void Update() {
        if(Input.GetKey("1")) {
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
        if(Input.GetKey("2")) {
            List<TreePos> toAdd = GetToAdd((int) (TREESPERSECOND * Time.deltaTime) + 1);
            List<TerrainTile> dirty = new List<TerrainTile>();
            
            foreach(TreePos pos in toAdd) {
                byte x = pos.GetGridX();
                byte y = pos.GetGridY();
                TerrainTile tile = TerrainManager.Instance.Tiles[x + TerrainManager.Instance.NumTilesX * y];
                if(!dirty.Contains(tile)) dirty.Add(tile);

                TerrainManager.Instance.TreesData.Add(pos);
            }

            foreach(TerrainTile tile in dirty) {
                tile.DirtyStates |= TerrainTile.TerrainTileDirtyStates.TREES;
                TerrainManager.Instance.Dirty.Enqueue(tile);
            }

            TerrainManager.Instance.TreeLODRenderersDirty = true;
        }
    }

    private List<TreePos> GetToAdd(int quant) {
        System.Random random = new System.Random();

        List<TreePos> toReturn = new List<TreePos>(quant);
        for(int i = 0;i < quant;i ++) {
            float dist = (float)(random.NextDouble() * DIST);
            float theta = (float)(random.NextDouble() * 2.0 * 3.141592653589793);

            TreePos value = new TreePos();

            value.type = 0;
            value.pos.x = Mathf.Sin(theta) * dist + transform.position.x;
            value.pos.y = 0;//Will get populated later
            value.pos.z = Mathf.Cos(theta) * dist + transform.position.z;
            value.rot = (float)(random.NextDouble() * 2.0 * 3.141592653589793);
            value.scale = (float)(random.NextDouble() * 0.5 + 0.75);

            value.pos = TerrainManager.Instance.Project(value.pos.ToHorizontal());
            if(TerrainManager.Instance.TerrainBounds.Contains(value.pos)) {
                toReturn.Add(value);
            }
        }
        return toReturn;
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
