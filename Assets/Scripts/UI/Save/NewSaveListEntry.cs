using TMPro;
using UnityEngine;

public class NewSaveListEntry : MonoBehaviour {
    public TMP_InputField InputField;
    public SaveLoadScreen SaveLoadScreen;

    public void OnSaveButtonClicked() {
        SaveManager.QueueSaveJob(SaveManager.GetSave(), InputField.text, SaveLoadScreen);
    }
}