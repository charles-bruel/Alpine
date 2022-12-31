using UnityEngine;

public struct TreePos : IGridable {
    public Vector3 pos;
    public float rot;
    public float scale;
    public uint type;

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