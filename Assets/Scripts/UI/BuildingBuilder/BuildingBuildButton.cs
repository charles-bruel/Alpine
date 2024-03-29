using UnityEngine;

public class BuildingBuildButton : MonoBehaviour {
    public SimpleBuildingTemplate Template;
    public Canvas WorldUICanvas;

    public void OnBuildingToolEnablle() {
        BuildingBuilderTool tool = new BuildingBuilderTool();
        tool.Template = Template;
        tool.WorldUICanvas = WorldUICanvas;
        InterfaceController.Instance.SelectedTool = tool;
    }
}