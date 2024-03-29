//>============================================================================<
//
//    Alpine, Ski Resort Tycoon Game
//    Copyright (C) 2024  Charles Bruel
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see <https://www.gnu.org/licenses/>.
//>============================================================================<

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
    private bool AttemptedToLoadSave = false;

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
            if(GameController.TargetSaveGame != null && !AttemptedToLoadSave) {
                AttemptedToLoadSave = true;
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