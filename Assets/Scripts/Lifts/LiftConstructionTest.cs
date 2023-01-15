using UnityEngine;

public class LiftConstructionTest : MonoBehaviour {
    public LiftConstructionData Data;
    public LiftBuilder Builder;

    void Start() {
        Builder = new LiftBuilder();
        Builder.Data = Data;
        Builder.Initialize();
    }

    void Update() {
        Builder.Build();
    }
}