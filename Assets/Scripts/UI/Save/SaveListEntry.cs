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