using UnityEngine;
using UnityEngine.UI;

public class LiftBuilderUI : MonoBehaviour {
    public LiftBuilderTool Tool;

    public void OnPreview() {
        Tool.Builder.Build();
    }

    public void OnFinish() {
        InterfaceController.Instance.Finish();
    }

    public void OnCancel() {
        InterfaceController.Instance.SelectedTool = null;
    }
}