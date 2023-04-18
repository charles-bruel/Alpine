using UnityEngine;

public class SlopeBuildButton : MonoBehaviour {
    public SlopeBuilderToolGrab GrabTemplate;
    public SlopeBuilderUI UI;
    public Canvas Canvas;

    public void OnSlopeToolEnable() {
        SlopeBuilderTool tool = new SlopeBuilderTool();
        tool.GrabTemplate = GrabTemplate;
        tool.Canvas = Canvas;
        InterfaceController.Instance.SelectedTool = tool;
        UI.Tool = tool;
        tool.UI = UI;
        UI.gameObject.SetActive(true);
    }
}