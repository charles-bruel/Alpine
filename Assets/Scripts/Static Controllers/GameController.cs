using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour{
    public RenderingData ColorsData;
    public TerrainManager TerrainManager;
    public WeatherController WeatherController;
    public PolygonsController PolygonsController;
    public StateController StateController;
    public TerrainModificationController TerrainModificationController;
    public BuildingsController BuildingsController;

    //TODO: Seperate time controller?
    public float TimeMultiplier = 1;

    void Start() {
        Instance = this;

        ColorsData.Initialize();
        TerrainManager.Initialize();
        WeatherController.Initialize();
        PolygonsController.Initialize();
        StateController.Initialize();
        BuildingsController.Initialize();
    }

    void Update() {
        float delta = Time.deltaTime * TimeMultiplier;
        WeatherController.Advance(delta);
        BuildingsController.Advance(delta);
    }

    public void TerrainManagerDoneCallback() {
        TerrainModificationController.Initialize();
    }

    public static GameController Instance;
}
