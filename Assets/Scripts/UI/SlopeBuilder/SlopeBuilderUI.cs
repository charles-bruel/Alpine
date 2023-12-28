using UnityEngine;
using UnityEngine.UI;

public class SlopeBuilderUI : MonoBehaviour {
    public void OnFinish() {
        InterfaceController.Instance.Finish();
    }

    public void OnCancel() {
        InterfaceController.Instance.SelectedTool = null;
    }
}