using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour {
    public SaveLoadScreen SaveLoadScreen;

    void Start() {
        StartupConfig.Initialize();
    }

    public void OnNewGame() {
        GameController.TargetSaveGame = null;
        SceneManager.LoadSceneAsync("Game");
    }

    public void OnLoadGame() {
        SaveLoadScreen.Inflate(false, true);
    }

    public void OnQuit() {
        Application.Quit();
    }
}
