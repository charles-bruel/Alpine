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

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaveList : MonoBehaviour {
    public SaveListEntry SaveListEntryPrefab;
    public NewSaveListEntry NewSaveListEntryPrefab;
    public GameObject SaveListParent;
    public RectTransform LayoutGroupTransform;
    public ScrollRect ScrollRect;
    public SaveLoadScreen SaveLoadScreen;

    public List<GameObject> SaveListEntries = new List<GameObject>();

    public void Inflate(bool save, bool load) {
        List<Tuple<DateTime, string>> saves = SaveManager.GetSaves();

        int length = saves.Count;
        if(save) {
            length++;
        }

        LayoutGroupTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 80 * length);

        if(save) {
            NewSaveListEntry entry = Instantiate(NewSaveListEntryPrefab, SaveListParent.transform);
            entry.SaveLoadScreen = SaveLoadScreen;
            SaveListEntries.Add(entry.gameObject);
        }

        foreach(var value in saves) {
            string saveName = value.Item2;
            SaveListEntry entry = Instantiate(SaveListEntryPrefab, SaveListParent.transform);
            entry.SaveLoadScreen = SaveLoadScreen;
            entry.Inflate(saveName, save, load);
            SaveListEntries.Add(entry.gameObject);
        }

        ScrollRect.normalizedPosition = new Vector2(0, 1);
    }

    public void Reset() {
        foreach(GameObject entry in SaveListEntries) {
            Destroy(entry);
        }
        SaveListEntries.Clear();
    }
}