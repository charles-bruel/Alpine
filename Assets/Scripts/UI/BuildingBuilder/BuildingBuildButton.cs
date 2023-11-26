using UnityEngine;

public class BuildingBuildButton : MonoBehaviour {
    public SimpleBuildingTemplate Template;

    public void OnBuildingToolEnablle() {
        BuildingBuilderTool tool = new BuildingBuilderTool();
        tool.Template = Template;
        InterfaceController.Instance.SelectedTool = tool;
    }
}