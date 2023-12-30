using UnityEngine;

public class SaveLoadScreen : MonoBehaviour {
    public SaveList SaveList;

    public void Inflate(bool save, bool load) {
        gameObject.SetActive(true);
        SaveList.Inflate(save, load);
    }

    public void Hide() {
        gameObject.SetActive(false);
    }
}