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