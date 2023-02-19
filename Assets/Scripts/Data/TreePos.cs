using UnityEngine;

public struct TreePos : IGridable {
    public Vector3 pos;
    public float rot;
    public float scale;
    public uint type;

    // We use a uint here because bool is not blittable. It cannot be a byte
    // because GLSL doesn't support sized types smaller than 32 bits
    public uint enabled;

    public uint padding;

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