using UnityEngine;

public interface IMap {
    public string GetName();
    public void Load(TerrainManager terrainManager);
    public string GetID();
    public Sprite GetThumbnail();
}