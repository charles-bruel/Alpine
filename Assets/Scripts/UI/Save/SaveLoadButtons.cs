using UnityEngine;

public class SaveLoadButtons : MonoBehaviour {
    public void OnSaveClicked() {
        SaveManager.QueueSaveJob(SaveManager.GetSave(), "save");
    }

    public void OnTestLoad() {
        SaveManager.LoadSave("save");
    }

}