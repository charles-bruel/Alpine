using UnityEngine;

public class SaveLoadButtons : MonoBehaviour {
    public SaveLoadScreen SaveLoadScreen;
    public void OnSaveClicked() {
        SaveLoadScreen.Inflate(true, false);
    }

    public void OnTestLoad() {
        SaveLoadScreen.Inflate(false, true);
    }

}