using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class SlopePanelUI : MonoBehaviour {
    public ToggleGroup DifficultyToggleGroup;
    public Toggle Green;
    public Toggle Blue;
    public Toggle Black;
    public Toggle DoubleBlack;
    
    [NonSerialized]
    public Slope CurrentSlope;
    [NonSerialized]
    public Dictionary<Toggle, SlopeDifficulty> ToggleToDifficulty;

    private bool Initialized = false;

    private void Initialize() {
        if(Initialized) return;
        Initialized = true;
        ToggleToDifficulty = new Dictionary<Toggle, SlopeDifficulty> {
            {Green, SlopeDifficulty.GREEN},
            {Blue, SlopeDifficulty.BLUE},
            {Black, SlopeDifficulty.BLACK},
            {DoubleBlack, SlopeDifficulty.DOUBLE_BLACK}
        };
    }

    public void Inflate(Slope newSlope) {
        Initialize();

        switch(newSlope.CurrentDifficulty) {
            case SlopeDifficulty.GREEN:
                Blue.isOn = false;
                Black.isOn = false;
                DoubleBlack.isOn = false;
                Green.isOn = true;
                break;
            case SlopeDifficulty.BLUE:
                Black.isOn = false;
                DoubleBlack.isOn = false;
                Blue.isOn = true;
                break;
            case SlopeDifficulty.BLACK:
                DoubleBlack.isOn = false;
                Black.isOn = true;
                break;
            case SlopeDifficulty.DOUBLE_BLACK:
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
        SlopeDifficulty difficulty = ToggleToDifficulty[selected];
        if(difficulty == CurrentSlope.CurrentDifficulty) return;
        CurrentSlope.UpdateDifficulty(difficulty);
    }

    public void OnDeleteButtonPressed() {
        Assert.IsNotNull(CurrentSlope);
        CurrentSlope.Destroy();
    }

    public void OnEditButtonPressed() {
        Assert.IsNotNull(CurrentSlope);
        throw new NotImplementedException();
    }
}