using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UITimeController : MonoBehaviour {
    public ToggleGroup TimeToggleGroup;
    public Toggle X0;
    public Toggle X1;
    public Toggle X10;
    public Toggle X100;

    [NonSerialized]
    public Dictionary<Toggle, float> ToggleToTimeMultiplier;

    private bool Initialized = false;

    private void Initialize() {
        if(Initialized) return;
        Initialized = true;
        ToggleToTimeMultiplier = new Dictionary<Toggle, float> {
            {X0, 0},
            {X1, 1},
            {X10, 10},
            {X100, 100}
        };
    }

    public void OnTimeToggleChanged() {
        Initialize();
        
        float newTimeMultiplier = ToggleToTimeMultiplier[TimeToggleGroup.ActiveToggles().First()];
        GameController.Instance.TimeMultiplier = newTimeMultiplier;
    }
}