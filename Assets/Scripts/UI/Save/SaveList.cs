using System.Collections.Generic;
using Mono.Cecil;
using PlasticGui;
using UnityEditor.TextCore.Text;
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
        List<string> saves = SaveManager.GetSaves();

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

        foreach(string saveName in saves) {
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