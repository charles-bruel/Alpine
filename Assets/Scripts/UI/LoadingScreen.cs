using System;
using Codice.CM.Common.Merge;
using TMPro;
using UnityEngine;

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
        Canvas.gameObject.SetActive(true);
    }

    void Update() {
        Timer -= Time.deltaTime;
        if(Timer < 0) {
            LoadingMessageArea.text = LoadingMessages[UnityEngine.Random.Range(0, LoadingMessages.Length)];
            Timer = 1;
        }

        if(LoadingTasks == 0) {
            Loaded = true;
            Canvas.gameObject.SetActive(false);
            gameObject.SetActive(false);
        }
    }

    public bool IsLoaded() {
        return Loaded;
    }
}