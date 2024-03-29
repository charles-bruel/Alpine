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
            PolygonTool.Build();

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

            PolygonTool.Finish();
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
            PolygonTool.AddPoint(pos, Input.GetKey(KeyCode.LeftControl));
        } else if(eventData.button == PointerEventData.InputButton.Right) {
            PolygonTool.RemovePoint(pos, Input.GetKey(KeyCode.LeftControl));
        }
    }
}