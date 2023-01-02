using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleButtonController : MonoBehaviour
{
    public Color SelectedColor;
    public Color RegularColor;

    public Button Button;
    public bool ButtonSelected = false;

    public Image BackgroundPanel;

    void OnStart() {
        Button.onClick.AddListener(() => OnClicked());
    }

    void OnClicked() {
        ButtonSelected = !ButtonSelected;
        BackgroundPanel.color = ButtonSelected ? RegularColor : SelectedColor;
    }
}
