using UnityEngine;

public class LiftBuilderTool : ITool {
    private bool done = false;

    public bool Require2D() {
        return true;
    }

    public void Cancel() {
        Debug.Log("cancel");
    }

    public bool IsDone() {
        return done;
    }

    public void Start() {
        Debug.Log("start");
    }

    public void Update() {
        Debug.Log("update");
    }
}