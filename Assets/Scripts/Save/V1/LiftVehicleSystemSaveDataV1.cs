using System.Collections.Generic;
using UnityEngine.Assertions;

public class LiftVehicleSystemSaveDataV1 {
    public List<LiftVehicleSaveDataV1> Vehicles;

    public static LiftVehicleSystemSaveDataV1 FromLiftVehicleSystem(LiftVehicleSystem liftVehicleSystem) {
        LiftVehicleSystemSaveDataV1 result = new LiftVehicleSystemSaveDataV1();
        result.Vehicles = new List<LiftVehicleSaveDataV1>();

        foreach(var vehicle in liftVehicleSystem.LiftVehicles) {
            LiftVehicleSaveDataV1 vehicleSaveData = new LiftVehicleSaveDataV1();
            vehicleSaveData.Visitors = new int[vehicle.Visitors.Length];
            for(int i = 0;i < vehicle.Visitors.Length;i ++) {
                vehicleSaveData.Visitors[i] = VisitorController.Instance.Visitors.IndexOf(vehicle.Visitors[i]);
            }
            vehicleSaveData.Position = vehicle.Position;
            result.Vehicles.Add(vehicleSaveData);
        }

        return result;
    }

    public void RestoreTo(LiftVehicleSystem Target) {
        Assert.AreEqual(Vehicles.Count, Target.LiftVehicles.Count);

        for(int i = 0;i < Vehicles.Count;i ++) {
            LiftVehicleSaveDataV1 vehicleSaveData = Vehicles[i];
            LiftVehicle vehicle = Target.LiftVehicles[i];
            vehicle.Position = vehicleSaveData.Position;
            vehicle.Visitors = new Visitor[vehicleSaveData.Visitors.Length];
            for(int j = 0;j < vehicleSaveData.Visitors.Length;j ++) {
                if(vehicleSaveData.Visitors[j] == -1) continue;
                vehicle.Visitors[j] = VisitorController.Instance.Visitors[vehicleSaveData.Visitors[j]];
            }
        }
    }

    public struct LiftVehicleSaveDataV1 {
        public int[] Visitors;
        public float Position;
    }
}