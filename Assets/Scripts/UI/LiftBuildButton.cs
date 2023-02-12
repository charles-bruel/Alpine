using UnityEngine;

public class LiftBuildButton : MonoBehaviour {
    public LiftConstructionData data;
    public LiftBuilderToolGrab GrabTemplate;
    public Canvas Canvas;

    public void OnLiftToolEnable() {
        LiftBuilderTool tool = new LiftBuilderTool();
        tool.Data = data;
        tool.GrabTemplate = GrabTemplate;
        tool.Canvas = Canvas;
        InterfaceController.Instance.SelectedTool = tool;
    }
}