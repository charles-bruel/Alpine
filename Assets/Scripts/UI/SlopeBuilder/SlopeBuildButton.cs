using UnityEngine;

public class SlopeBuildButton : MonoBehaviour {
    public SlopeBuilderToolGrab GrabTemplate;
    public SlopeBuilderUI UI;
    public Canvas Canvas;

    public void OnSlopeToolEnable() {
        SlopeBuilderTool tool = new SlopeBuilderTool();
        tool.PolygonTool.GrabTemplate = GrabTemplate;
        tool.PolygonTool.Canvas = Canvas;
        InterfaceController.Instance.SelectedTool = tool;
        tool.UI = UI;
        UI.gameObject.SetActive(true);
    }
}