using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct SaveV1 {
    public BuildingSaveDataV1[] buildings;
    public SlopeSaveDataV1[] slopes;
    public LiftSaveDataV1[] lifts;
    public SnowfrontSaveDataV1[] snowfronts;
    public VisitorSaveDataV1[] visitors;
    public WeatherSaveDataV1 weather;

    public static SaveV1 CreateSaveData() {
        SavingContextV1 context = new SavingContextV1();
        context.Populate();

        SaveV1 result = new SaveV1();

        result.visitors = new VisitorSaveDataV1[VisitorController.Instance.Visitors.Count];
        for(int i = 0;i < VisitorController.Instance.Visitors.Count;i ++) {
            result.visitors[i] = VisitorSaveDataV1.FromVisitor(VisitorController.Instance.Visitors[i], context);
        }

        List<BuildingSaveDataV1> buildings = new List<BuildingSaveDataV1>();
        List<SlopeSaveDataV1> slopes = new List<SlopeSaveDataV1>();
        List<LiftSaveDataV1> lifts = new List<LiftSaveDataV1>();
        List<SnowfrontSaveDataV1> snowfronts = new List<SnowfrontSaveDataV1>();

        foreach(Building building in BuildingsController.Instance.Buildings) {
            if(building is SimpleBuilding simpleBuilding) {
                buildings.Add(BuildingSaveDataV1.FromSimpleBuilding(simpleBuilding, context));
            }
            else if(building is Slope slope) {
                slopes.Add(SlopeSaveDataV1.FromSlope(slope, context));
            }
            else if(building is Lift lift) {
                lifts.Add(LiftSaveDataV1.FromLift(lift, context));
            } else if(building is Snowfront snowfront) {
                snowfronts.Add(SnowfrontSaveDataV1.FromSnowfront(snowfront, context));
            } else {
                throw new NotImplementedException();
            }
        }

        result.buildings = buildings.ToArray();
        result.slopes = slopes.ToArray();
        result.lifts = lifts.ToArray();
        result.snowfronts = snowfronts.ToArray();

        result.weather = WeatherSaveDataV1.FromWeather(WeatherController.Instance);

        return result;
    }

    // Assumes a blank but generated map
    public void Restore() {
        LoadingContextV1 loadingContext = new LoadingContextV1();
        
        List<Lift> recreatedLifts = new List<Lift>();
        foreach(LiftSaveDataV1 lift in lifts) {
            recreatedLifts.Add(LiftBuilder.BuildFromSave(lift.ToConstructionData(), lift.NavAreaGraphs, loadingContext));
        }

        foreach(BuildingSaveDataV1 building in buildings) {
            BuildingBuilder.BuildFromSave(building.Position, building.Rotation, building.Template, building.NavAreaGraphs, loadingContext);
        }

        // Slopes and snowfronts have to go last, because they can place portals, but they will only regenerate properly if the other buildings are already there
        foreach(SlopeSaveDataV1 slope in slopes) {
            SlopeBuilder.BuildFromSave(slope.ToConstructionData(), slope.NavAreaGraphs, slope.CurrentDifficulty, slope.IntrinsicDifficulty, loadingContext);
        }

        foreach(SnowfrontSaveDataV1 snowfront in snowfronts) {
            SnowfrontBuilder.BuildFromSave(snowfront.ToConstructionData(), snowfront.NavAreaGraphs, loadingContext);
        }

        weather.Restore();
        // Everything needs to be loaded before we can restore the mappings
        // Get all list of all nav save data to use
        List<NavAreaGraphSaveDataV1> navSaveData = new List<NavAreaGraphSaveDataV1>();
        foreach(LiftSaveDataV1 lift in lifts) {
            foreach(NavAreaGraphSaveDataV1 navAreaGraph in lift.NavAreaGraphs) {
                navSaveData.Add(navAreaGraph);
            }
        }
        foreach(BuildingSaveDataV1 building in buildings) {
            foreach(NavAreaGraphSaveDataV1 navAreaGraph in building.NavAreaGraphs) {
                navSaveData.Add(navAreaGraph);
            }
        }
        foreach(SlopeSaveDataV1 slope in slopes) {
            navSaveData.Add(slope.NavAreaGraphs);
        }
        foreach(SnowfrontSaveDataV1 snowfront in snowfronts) {
            navSaveData.Add(snowfront.NavAreaGraphs);
        }

        loadingContext.LoadNavData(navSaveData);
        GlobalNavController.Instance.TriggerGraphRebuild();

        // Now we can restore the visitors
        VisitorController.Instance.RestoreVisitors(visitors, loadingContext);

        for(int i = 0;i < lifts.Length;i ++) {
            lifts[i].LiftVehicleSystem.RestoreTo(recreatedLifts[i].VehicleSystem);
        }
    }
}