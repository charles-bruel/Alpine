using UnityEngine;

public class NewGameScreen : MonoBehaviour {
    public MapList MapList;
    public void Inflate() {
        gameObject.SetActive(true);
        MapList.Inflate();
    }

    public void Hide() {
        gameObject.SetActive(false);
        MapList.Reset();
    }
}