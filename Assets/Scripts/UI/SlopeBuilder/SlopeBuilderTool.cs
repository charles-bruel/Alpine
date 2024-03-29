using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEditor;

public class SlopeBuilderTool : ITool {
    public SlopeBuilderUI UI;
    public PolygonTool PolygonTool;
    public Slope Result;

    public SlopeBuilderTool() {
        PolygonTool = new PolygonTool();
    }

    public bool Require2D() {
        return true;
    }

    public void Cancel(bool confirm) {
        PolygonsController.Instance.PolygonObjects.Remove(PolygonTool.Builder.Result.Footprint);
        if(confirm && PolygonTool.Data.SlopePoints.Count > 2) {
            PolygonTool.Builder.Build();
            PolygonTool.Builder.Finish();
        } else {
            PolygonTool.Builder.Cancel();
        }
        
        PolygonTool.OnCancel();

        UI.gameObject.SetActive(false);
    }

    public bool IsDone() {
        return false;
    }

    public void Start() {
        GameObject gameObject = new GameObject("Slope");
        Result = gameObject.AddComponent<Slope>();
        
        PolygonTool.Start(Result, PolygonFlags.CLEARANCE | PolygonFlags.SLOPE_NAVIGABLE);

        Result.AreaImplementation = new SlopeNavAreaImplementation(Result, default(Rect));
        Result.Footprint.Implementation = Result.AreaImplementation;
    }

    public void Update() {
        PolygonTool.Builder.LightBuild();
    }

    public void OnPointerClick(PointerEventData eventData) {
        Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition).ToHorizontal();
        if(eventData.button == PointerEventData.InputButton.Left) {
            PolygonTool.AddPoint(pos);
        } else if(eventData.button == PointerEventData.InputButton.Right) {
            PolygonTool.RemovePoint(pos);
        }
    }
}