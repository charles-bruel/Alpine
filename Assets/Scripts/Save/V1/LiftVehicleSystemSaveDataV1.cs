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