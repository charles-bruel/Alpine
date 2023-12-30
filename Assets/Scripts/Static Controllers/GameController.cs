using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour{
    public RenderingData RenderingData;
    public GameParameters GameParameters;
    public TerrainManager TerrainManager;
    public WeatherController WeatherController;
    public PolygonsController PolygonsController;
    public StateController StateController;
    public TerrainModificationController TerrainModificationController;
    public BuildingsController BuildingsController;
    public InterfaceController InterfaceController;
    public GlobalNavController GlobalNavController;
    public VisitorController VisitorController;
    public LoadingScreen LoadingScreen;

    public float TimeMultiplier = 1;

    public static string TargetSaveGame = null;

    void Start() {
        Instance = this;

        LoadingScreen.Initialize();

        RenderingData.Initialize();
        GameParameters.Initialize();
        TerrainManager.Initialize();
        WeatherController.Initialize();
        PolygonsController.Initialize();
        StateController.Initialize();
        BuildingsController.Initialize();
        InterfaceController.Initialize();
        GlobalNavController.Initialize();
        VisitorController.Initialize();
    }

    void Update() {
        float delta = Time.deltaTime * TimeMultiplier;
        WeatherController.Advance(delta);
        BuildingsController.Advance(delta);
        VisitorController.Advance(delta);

        InterfaceController.UpdateTool();
    }

    public void TerrainManagerDoneCallback() {
        TerrainModificationController.Initialize();
    }

    public static GameController Instance;
}
