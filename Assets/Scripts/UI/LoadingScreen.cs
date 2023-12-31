using System;
using TMPro;
using UnityEngine;

// TODO: Rename this. It is not obvious that LoadingScreen contains actual save load logic
public class LoadingScreen : MonoBehaviour {
    public static LoadingScreen INSTANCE;
    
    public String[] LoadingMessages;
    public TMP_Text LoadingMessageArea;
    public Canvas Canvas;

    public int LoadingTasks = 0;
    public float Timer = 0;

    private bool Loaded = false;

    public void Initialize() {
        INSTANCE = this;
        // Canvas.gameObject.SetActive(true);
    }

    void Update() {
        Timer -= Time.deltaTime;
        if(Timer < 0) {
            LoadingMessageArea.text = LoadingMessages[UnityEngine.Random.Range(0, LoadingMessages.Length)];
            Timer = 1;
        }

        if(LoadingTasks == 0) {
            if(GameController.TargetSaveGame != null) {
                SaveManager.LoadSave(GameController.TargetSaveGame);
                GameController.TargetSaveGame = null;
            } else {
                Loaded = true;
                Canvas.gameObject.SetActive(false);
                gameObject.SetActive(false);
            }
        }
    }

    public bool IsLoaded() {
        return Loaded;
    }
}