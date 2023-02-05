using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour{
    public ColorsData ColorsData;
    public TerrainManager TerrainManager;
    public WeatherController WeatherController;
    public PolygonsController PolygonsController;
    public StateController StateController;
    public TerrainModificationController TerrainModificationController;

    void Start() {
        Instance = this;

        ColorsData.Initialize();
        TerrainManager.Initialize();
        WeatherController.Initialize();
        PolygonsController.Initialize();
        StateController.Initialize();
    }

    public void TerrainManagerDoneCallback() {
        TerrainModificationController.Initialize();
    }

    public static GameController Instance;
}
