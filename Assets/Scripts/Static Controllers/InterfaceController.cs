using UnityEngine;

public class InterfaceController : MonoBehaviour {

    public static InterfaceController Instance;

    private ITool selectedTool;
    public ITool SelectedTool {
        set {
            if(selectedTool != null) selectedTool.Cancel();
            selectedTool = value;
            selectedTool.Start();
        }
        get {
            return selectedTool;
        }
    }

    public void UpdateTool() {
        if(selectedTool == null) return;
        if(selectedTool.Require2D() && StateController.Instance.Mode3D) {
            selectedTool.Cancel();
            selectedTool = null;
            return;
        }
        selectedTool.Update();
        if(selectedTool.IsDone()) {
            selectedTool = null;
        }
    }

    public void Initialize() {
        Instance = this;
    }
}