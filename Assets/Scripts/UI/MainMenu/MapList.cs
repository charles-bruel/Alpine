//>============================================================================<
//
//    Alpine, Ski Resort Tycoon Game
//    Copyright (C) 2024  Charles Bruel
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see <https://www.gnu.org/licenses/>.
//>============================================================================<

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