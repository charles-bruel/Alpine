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
    }

    void Update() {
        Builder.LightBuild();
        Builder.Build();
        var watch = new System.Diagnostics.Stopwatch();
        watch.Start();
        Builder.Finish();
        gameObject.SetActive(false);
        watch.Stop();
        Debug.Log($"Finish Time: {watch.ElapsedMilliseconds} ms");
    }
}