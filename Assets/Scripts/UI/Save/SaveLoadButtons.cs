using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveLoadButtons : MonoBehaviour {
    public SaveLoadScreen SaveLoadScreen;
    public void OnSaveClicked() {
        SaveLoadScreen.Inflate(true, false);
    }

    public void OnLoad() {
        SaveLoadScreen.Inflate(false, true);
    }

    public void OnReturnToMainMenu() {
        SceneManager.LoadScene("MainMenu");
    }
}