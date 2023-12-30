using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour {
    public void OnNewGame() {
        GameController.TargetSaveGame = null;
        SceneManager.LoadSceneAsync("Game");
    }

    public void OnLoadGame() {
        GameController.TargetSaveGame = "Test";
        SceneManager.LoadSceneAsync("Game");
    }

    public void OnQuit() {
        Application.Quit();
    }
}
