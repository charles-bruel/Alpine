using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIButtons : MonoBehaviour {
    public void OnContoursClicked(Toggle change) {
        List<TerrainTile> tiles = TerrainManager.Instance.Tiles;
        for(int i = 0;i < tiles.Count;i ++) {
            tiles[i].ContoursComponent.gameObject.SetActive(change.isOn);
        }
    }

    public void OnModeChangeClicked(Toggle change) {
        StateController.Instance.ToggleMode(change.isOn);
    }

    public void OnLiftToolEnable() {
        InterfaceController.Instance.SelectedTool = new LiftBuilderTool();
    }
}
