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