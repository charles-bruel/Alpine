using UnityEngine;

// Contains random needed UI references
public class UIsController : MonoBehaviour {
    public UITimeController UITimeController;

    public static UIsController Instance;

    public void Initialize() {
        Instance = this;
    }
}