using UnityEngine;

public class LiftBuildButton : MonoBehaviour {
    public LiftConstructionData data;
    public LiftBuilderToolGrab GrabTemplate;
    public LiftBuilderUI UI;
    public Canvas Canvas;

    public void OnLiftToolEnable() {
        LiftBuilderTool tool = new LiftBuilderTool();
        tool.Data = data.Clone();
        tool.GrabTemplate = GrabTemplate;
        tool.Canvas = Canvas;
        InterfaceController.Instance.SelectedTool = tool;
        UI.Tool = tool;
        tool.UI = UI;
        UI.gameObject.SetActive(true);
    }
}