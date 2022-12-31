using UnityEngine;

public struct RockPos : IGridable {
    public Vector3 pos;
    public Vector3 normal;
    public float scale;

    public byte GetGridX()
    {
        int x = Mathf.FloorToInt(pos.x / TerrainManager.Instance.TileSize);
        x += TerrainManager.Instance.NumTilesX / 2;
        return (byte) x;
    }

    public byte GetGridY()
    {
        int y = Mathf.FloorToInt(pos.z / TerrainManager.Instance.TileSize);
        y += TerrainManager.Instance.NumTilesY / 2;
        return (byte) y;
    }
}