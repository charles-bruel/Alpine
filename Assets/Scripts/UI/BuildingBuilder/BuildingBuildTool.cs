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

using UnityEngine.EventSystems;
using UnityEngine;
using Codice.Client.BaseCommands.BranchExplorer.Layout;

public class BuildingBuilderTool : ITool {
    private bool done = false;
    
    public Canvas WorldUICanvas;
    public SimpleBuildingTemplate Template;
    public BuildingBuilder Builder;

    public void Start()
    {
        Builder = new BuildingBuilder();
        Builder.Template = Template;
        Builder.WorldUICanvas = WorldUICanvas;
        Builder.Initialize();
    }

    public void Cancel(bool confirm)
    {
        if(confirm) {
            Builder.Build();
            Builder.Finish();
        } else {
            Builder.Cancel();
        }
    }

    public bool IsDone()
    {
        return done;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(eventData.button == PointerEventData.InputButton.Right) {
            InterfaceController.Instance.SelectedTool = null;
        }
        if (eventData.button == PointerEventData.InputButton.Left) {
            InterfaceController.Instance.Finish();
        }
    }

    public bool Require2D()
    {
        return true;
    }

    public void Update()
    {
        Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition).ToHorizontal();
        Builder.UpdatePos(pos);
        Builder.LightBuild();

        int dir = 0;

        if(Input.GetKey(KeyCode.M)) {
            dir = 1;
        }
        if(Input.GetKey(KeyCode.N)) {
            dir = -1;
        }

        if(Input.GetKey(KeyCode.LeftShift)) {
            Builder.Rotation += dir * Time.deltaTime * 5;
        } else {
            Builder.Rotation += dir * Time.deltaTime * 90;
        }
    }
}