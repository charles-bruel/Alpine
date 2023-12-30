using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveListEntry : MonoBehaviour {
    public string SaveName;
    public TMP_Text SaveNameField;
    public GameObject SaveButton;
    public GameObject LoadButton;
    public SaveLoadScreen SaveLoadScreen;

    public void Inflate(string saveName, bool save, bool load) {
        SaveName = saveName;
        SaveNameField.text = saveName;
        SaveButton.SetActive(save);
        LoadButton.SetActive(load);
    }

    public void OnSaveButtonClicked() {
        SaveManager.QueueSaveJob(SaveManager.GetSave(), SaveName, SaveLoadScreen);
    }

    public void OnLoadButtonClicked() {
        GameController.TargetSaveGame = SaveName;
        SceneManager.LoadSceneAsync("Game");
    }
}