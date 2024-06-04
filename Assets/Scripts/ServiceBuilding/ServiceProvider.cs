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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public abstract class ServiceProvider : BuildingFunctionality {

    public override void OnFinishConstruction() {
        VisitorController.Instance.Services.Add(this);
    }

    public override void OnDestroy() {
        VisitorController.Instance.Services.Remove(this);
    }

    public abstract Service[] Services();

    public override void OnVisitorArrival(Visitor visitor) {
        Service[] ourServices = Services();
        // Find the first service that the visitor needs
        Need[] needs = Enum.GetValues(typeof(Need)) as Need[];
        Service? serviceToProvide = null;
        // TODO: Refactor to pull out this factor (identical one is in CreateVisitorPlanJob.cs) to constants
        float needValue = 0.5f; // Only provide service if visitor actively wants that
        foreach (Need need in needs) {
            foreach (Service service in ourServices) {
                if (service.Need == need && visitor.Needs[need] < needValue) {
                    serviceToProvide = service;
                    needValue = visitor.Needs[need];
                    break;
                }
            }
        }
        if (serviceToProvide == null) {
            // The visitor just happens to be stopping by here; we have no service to give them
            return;
        }

        visitor.Needs[serviceToProvide.Value.Need] += serviceToProvide.Value.NeedValue;
        Debug.Log("Visitor " + visitor + " received service " + serviceToProvide.Value.Need + " at " + this);
    }
}
