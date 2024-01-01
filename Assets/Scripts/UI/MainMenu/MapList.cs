using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

public class MapList : MonoBehaviour {
    public MapSelectorEntry MapSelectorEntryPrefab;
    public GameObject MapListParent;
    public RectTransform LayoutGroupTransform;
    public ScrollRect ScrollRect;
    public NewGameScreen NewGameScreen;

    public List<GameObject> MapListEntries = new List<GameObject>();

    public void Inflate() {
        List<IMap> mapList = TerrainManager.GetAllMaps();
        foreach(IMap map in mapList) {
            MapSelectorEntry entry = Instantiate(MapSelectorEntryPrefab, MapListParent.transform);
            entry.Inflate(this, map);
            MapListEntries.Add(entry.gameObject);
        }
    }

    public void Reset() {
        foreach(GameObject entry in MapListEntries) {
            Destroy(entry);
        }
        MapListEntries.Clear();
    }
}