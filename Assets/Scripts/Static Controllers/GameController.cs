//>============================================================================<
//
//    Alpine, Ski Resort Tycoon Game
//    Copyright (C) 2024  Charles Bruel
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see <https://www.gnu.org/licenses/>.
//>============================================================================<

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {
    [Header("Controllers")]
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
    public UIsController UIsController;

    [Header("Other")]
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
        UIsController.Initialize();
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
