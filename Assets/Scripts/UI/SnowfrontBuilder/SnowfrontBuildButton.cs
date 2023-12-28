using UnityEngine;

public class SnowfrontBuildButton : MonoBehaviour {
    public SlopeBuilderToolGrab GrabTemplate;
    public SlopeBuilderUI UI;
    public Canvas Canvas;

    public void OnSnowfrontToolEnable() {
        SnowfrontBuilderTool tool = new SnowfrontBuilderTool();
        tool.GrabTemplate = GrabTemplate;
        tool.Canvas = Canvas;
        InterfaceController.Instance.SelectedTool = tool;
        tool.UI = UI;
        UI.gameObject.SetActive(true);
    }
}