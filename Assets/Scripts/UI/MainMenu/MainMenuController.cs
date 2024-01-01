using UnityEngine;

public class MainMenuController : MonoBehaviour {
    public SaveLoadScreen SaveLoadScreen;
    public NewGameScreen NewGameScreen;

    void Start() {
        StartupConfig.Initialize();
    }

    public void OnNewGame() {
        NewGameScreen.Inflate();
    }

    public void OnLoadGame() {
        SaveLoadScreen.Inflate(false, true);
    }

    public void OnQuit() {
        Application.Quit();
    }
}
