using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour{
    public TerrainManager TerrainManager;
    public WeatherController WeatherController;
    public PolygonsController PolygonsController;
    public StateController StateController;

    void Start() {
        Instance = this;

        TerrainManager.Initialize();
        WeatherController.Initialize();
        PolygonsController.Initialize();
        StateController.Initialize();
    }

    public void TerrainManagerDoneCallback() {
        //TODO: Call this
    }

    public static GameController Instance;
}
