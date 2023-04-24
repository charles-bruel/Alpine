using UnityEngine;

public class GameParameters : MonoBehaviour {
    public float BlueSlopeDifficultyThreshold;
    public float BlackSlopeDifficultyThreshold;
    public float DoubleBlackSlopeDifficultyThreshold;

    public void Initialize() {
        Instance = this;
    }

    void Update() {
        Initialize();
    }

    public static GameParameters Instance;

}