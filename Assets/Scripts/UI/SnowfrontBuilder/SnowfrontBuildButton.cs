using UnityEngine;

public class SnowfrontBuildButton : MonoBehaviour {
    public PolygonBuilderToolGrab GrabTemplate;
    public SlopeBuilderUI UI;
    public Canvas Canvas;

    public void OnSnowfrontToolEnable() {
        SnowfrontBuilderTool tool = new SnowfrontBuilderTool();
        tool.PolygonTool.GrabTemplate = GrabTemplate;
        tool.PolygonTool.Canvas = Canvas;
        InterfaceController.Instance.SelectedTool = tool;
        tool.UI = UI;
        UI.gameObject.SetActive(true);
    }
}