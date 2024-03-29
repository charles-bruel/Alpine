using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class SnowfrontBuilderTool : ITool {
    public SlopeBuilderUI UI;
    public PolygonTool PolygonTool;
    public Snowfront Result;

    public SnowfrontBuilderTool() {
        PolygonTool = new PolygonTool();
    }

    public bool Require2D() {
        return true;
    }

    public void Cancel(bool confirm) {
        PolygonsController.Instance.PolygonObjects.Remove(PolygonTool.Builder.Result.Footprint);
        if(confirm && PolygonTool.Data.SlopePoints.Count > 2) {
            PolygonTool.Builder.Build();

            NavArea temp = new NavArea();

            temp.Guid                = Result.Footprint.Guid;
            temp.Level               = Result.Footprint.Level;
            temp.Polygon             = Result.Footprint.Polygon;
            temp.Filter              = Result.Footprint.Filter;
            temp.Renderer            = Result.Footprint.Renderer;
            temp.Color               = Result.Footprint.Color;
            temp.ArbitrarilyEditable = Result.Footprint.ArbitrarilyEditable;
            temp.Flags               = Result.Footprint.Flags;
            temp.Height              = Result.Footprint.Height;

            temp.Implementation = new SnowfrontNavAreaImplementation(Result);

            temp.Owner = Result;

            Result.Footprint = temp;

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
        GameObject gameObject = new GameObject("Snowfront");
        Result = gameObject.AddComponent<Snowfront>();

        PolygonTool.Start(Result, PolygonFlags.CLEARANCE | PolygonFlags.FLAT_NAVIGABLE);
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