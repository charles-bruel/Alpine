using UnityEngine;

public class LiftConstructionTest : MonoBehaviour {
    public LiftConstructionData Data;
    public LiftBuilder Builder;

    void Start() {
        Builder = new LiftBuilder();
        Builder.Data = Data;
        Builder.Initialize();
    }

    void OnDisable() {
        Builder.Finish();
    }

    void Update() {
        Builder.LightBuild();
        Builder.Build();
    }
}