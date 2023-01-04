using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LayersController : MonoBehaviour
{
    public Camera MainCamera;
    public Camera TopCamera;

    public void OnContoursClicked(Toggle change) {
        List<TerrainTile> tiles = TerrainManager.Instance.Tiles;
        for(int i = 0;i < tiles.Count;i ++) {
            tiles[i].ContoursComponent.gameObject.SetActive(change.isOn);
        }
    }

    public void OnModeChangeClicked(Toggle change) {
        MainCamera.gameObject.SetActive(!change.isOn);
        TopCamera.gameObject.SetActive(change.isOn);
    }
}
