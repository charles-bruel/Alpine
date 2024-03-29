using System;
using UnityEngine;
using UnityEngine.Assertions;

public class SnowfrontPanelUI : MonoBehaviour {
    [Header("Edit settings")]
    public PolygonBuilderToolGrab GrabTemplate;
    public SlopeBuilderUI UI;
    public Canvas Canvas;

    [NonSerialized]
    public Snowfront CurrentSnowfront;

    public void Inflate(Snowfront newSnowfront) {
        CurrentSnowfront = newSnowfront;
        gameObject.SetActive(true);
    }

    public void Hide() {
        CurrentSnowfront = null;
        gameObject.SetActive(false);
    }

    public void OnDeleteButtonPressed() {
        Assert.IsNotNull(CurrentSnowfront);
        CurrentSnowfront.Destroy();
    }

    public void OnEditButtonPressed() {
        Assert.IsNotNull(CurrentSnowfront);

        SnowfrontBuilderTool tool = new SnowfrontBuilderTool();
        tool.PolygonTool.GrabTemplate = GrabTemplate;
        tool.PolygonTool.Canvas = Canvas;
        InterfaceController.Instance.SelectedTool = tool;
        tool.UI = UI;
        UI.gameObject.SetActive(true);

        tool.PolygonTool.PrepareForEditing(CurrentSnowfront);
    }
}