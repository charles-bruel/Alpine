using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Codice.Client.BaseCommands;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class SlopePanelUI : MonoBehaviour {
    [Header("Edit settings")]
    public SlopeBuilderToolGrab GrabTemplate;
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
        tool.GrabTemplate = GrabTemplate;
        tool.Canvas = Canvas;
        InterfaceController.Instance.SelectedTool = tool;
        tool.UI = UI;
        UI.gameObject.SetActive(true);
        foreach(var point in CurrentSlope.Footprint.Polygon.points) {
            tool.AddPoint(point);
        }

        CurrentSlope.Destroy();
    }
}