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

public class LiftBuildButton : MonoBehaviour {
    public LiftConstructionData data;
    public LiftBuilderToolGrab GrabTemplate;
    public LiftBuilderUI UI;
    public Canvas Canvas;

    public void OnLiftToolEnable() {
        LiftBuilderTool tool = new LiftBuilderTool();
        tool.Data = data.Clone();
        tool.GrabTemplate = GrabTemplate;
        tool.Canvas = Canvas;
        InterfaceController.Instance.SelectedTool = tool;
        UI.Tool = tool;
        tool.UI = UI;
        UI.gameObject.SetActive(true);
    }
}