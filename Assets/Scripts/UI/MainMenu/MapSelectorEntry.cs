using Codice.Client.BaseCommands;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MapSelectorEntry : MonoBehaviour {
    public TMP_Text MapName;
    public UnityEngine.UI.Image Thumbnail;
    public Button Button;
    public MapList MapList;
    public IMap Map;
    
    public void Inflate(MapList mapList, IMap map) {
        MapList = mapList;
        Map = map;
        MapName.text = map.GetName();
        Thumbnail.sprite = map.GetThumbnail();
    }

    public void OnClicked() {
        Assert.IsNotNull(Map);
        GameController.TargetSaveGame = null;
        TerrainManager.TargetMap = Map;
        SceneManager.LoadSceneAsync("Game");
    }
}