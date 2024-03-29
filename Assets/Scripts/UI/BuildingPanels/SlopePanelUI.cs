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
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class SlopePanelUI : MonoBehaviour {
    [Header("Edit settings")]
    public PolygonBuilderToolGrab GrabTemplate;
    public SlopeBuilderUI UI;
    public Canvas Canvas;
    [Header("UI settings")]
    public ToggleGroup DifficultyToggleGroup;
    public Toggle Auto;
    public Toggle Green;
    public Toggle Blue;
    public Toggle Black;
    public Toggle DoubleBlack;
    
    [NonSerialized]
    public Slope CurrentSlope;
    [NonSerialized]
    public Dictionary<Toggle, SlopeDifficultySetting> ToggleToDifficulty;

    private bool Initialized = false;

    private void Initialize() {
        if(Initialized) return;
        Initialized = true;
        ToggleToDifficulty = new Dictionary<Toggle, SlopeDifficultySetting> {
            {Auto, SlopeDifficultySetting.AUTO},
            {Green, SlopeDifficultySetting.GREEN},
            {Blue, SlopeDifficultySetting.BLUE},
            {Black, SlopeDifficultySetting.BLACK},
            {DoubleBlack, SlopeDifficultySetting.DOUBLE_BLACK}
        };
    }

    public void Inflate(Slope newSlope) {
        Initialize();

        switch(newSlope.CurrentDifficultySetting) {
            case SlopeDifficultySetting.AUTO:
                Green.isOn = false;
                Blue.isOn = false;
                Black.isOn = false;
                DoubleBlack.isOn = false;
                Auto.isOn = true;
                break;
            case SlopeDifficultySetting.GREEN:
                Blue.isOn = false;
                Black.isOn = false;
                DoubleBlack.isOn = false;
                Green.isOn = true;
                break;
            case SlopeDifficultySetting.BLUE:
                Black.isOn = false;
                DoubleBlack.isOn = false;
                Blue.isOn = true;
                break;
            case SlopeDifficultySetting.BLACK:
                DoubleBlack.isOn = false;
                Black.isOn = true;
                break;
            case SlopeDifficultySetting.DOUBLE_BLACK:
                DoubleBlack.isOn = true;
                break;
        }

        CurrentSlope = newSlope;
        gameObject.SetActive(true);
    }

    public void Hide() {
        CurrentSlope = null;
        gameObject.SetActive(false);
    }

    public void OnDifficultyToggleChanged() {
        if(CurrentSlope == null) return;

        Toggle selected = DifficultyToggleGroup.ActiveToggles().FirstOrDefault();
        SlopeDifficultySetting difficulty = ToggleToDifficulty[selected];

        if(difficulty == CurrentSlope.CurrentDifficultySetting) return;

        CurrentSlope.CurrentDifficultySetting = difficulty;
        CurrentSlope.UpdateDifficulty();
    }

    public void OnDeleteButtonPressed() {
        Assert.IsNotNull(CurrentSlope);
        CurrentSlope.Destroy();
    }

    public void OnEditButtonPressed() {
        Assert.IsNotNull(CurrentSlope);

        SlopeBuilderTool tool = new SlopeBuilderTool();
        tool.PolygonTool.GrabTemplate = GrabTemplate;
        tool.PolygonTool.Canvas = Canvas;
        InterfaceController.Instance.SelectedTool = tool;
        tool.UI = UI;
        UI.gameObject.SetActive(true);

        tool.PolygonTool.PrepareForEditing(CurrentSlope);
    }
}