using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(BoxCollider))]
public class InterfaceController : MonoBehaviour, IPointerClickHandler {

    public static InterfaceController Instance;
    public BoxCollider Collider;

    private ITool selectedTool;
    public ITool SelectedTool {
        set {
            if(selectedTool != null) selectedTool.Cancel(false);
            selectedTool = value;
            if(selectedTool != null) selectedTool.Start();
        }
        get {
            return selectedTool;
        }
    }

    public void Finish() {
        selectedTool.Cancel(true);
        selectedTool = null;
    }

    public void UpdateTool() {
        if(selectedTool == null) return;
        // if(selectedTool.Require2D() && StateController.Instance.Mode2D) {
        //     selectedTool.Cancel();
        //     selectedTool = null;
        //     return;
        // }
        selectedTool.Update();
        if(selectedTool.IsDone()) {
            selectedTool = null;
        }
    }

    public void Initialize() {
        Instance = this;

        Collider = GetComponent<BoxCollider>();
        float width = TerrainManager.Instance.TileSize * TerrainManager.Instance.NumTilesX;
        float height = TerrainManager.Instance.TileSize * TerrainManager.Instance.NumTilesY;
        Collider.size = new Vector3(width, height, 1);
    }

    public void OnPointerClick(PointerEventData eventData) {
        if(selectedTool == null) return;
        selectedTool.OnPointerClick(eventData);
    }
}