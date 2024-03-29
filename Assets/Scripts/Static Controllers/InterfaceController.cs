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
using UnityEngine.EventSystems;

[RequireComponent(typeof(BoxCollider))]
public class InterfaceController : MonoBehaviour, IPointerClickHandler {

    public static InterfaceController Instance;
    public BoxCollider Collider;

    private ITool selectedTool;
    public ITool SelectedTool {
        set {
            if(selectedTool != null) selectedTool.Cancel(false);
            selectedTool = value;
            if(selectedTool != null) selectedTool.Start();
        }
        get {
            return selectedTool;
        }
    }

    public void Finish() {
        selectedTool.Cancel(true);
        selectedTool = null;
    }

    public void UpdateTool() {
        if(selectedTool == null) return;
        // if(selectedTool.Require2D() && StateController.Instance.Mode2D) {
        //     selectedTool.Cancel();
        //     selectedTool = null;
        //     return;
        // }
        selectedTool.Update();
        if(selectedTool.IsDone()) {
            selectedTool = null;
        }
    }

    public void Initialize() {
        Instance = this;

        Collider = GetComponent<BoxCollider>();
        float width = TerrainManager.Instance.TileSize * TerrainManager.Instance.NumTilesX;
        float height = TerrainManager.Instance.TileSize * TerrainManager.Instance.NumTilesY;
        Collider.size = new Vector3(width, height, 1);
    }

    public void OnPointerClick(PointerEventData eventData) {
        if(selectedTool == null) return;
        selectedTool.OnPointerClick(eventData);
    }
}