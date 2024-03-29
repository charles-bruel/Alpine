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

using UnityEngine;

public class LiftNavLinkImplementation : INavLinkImplementation {
    public LiftVehicleSystem LiftVehicleSystem;

    public LiftNavLinkImplementation() {
        
    }
    
    public void OnDeselected() {
    }

    public void OnRemove() {
    }

    public void OnSelected() {
    }

    public void ProgressPosition(Visitor self, NavLink link, float delta, ref float progress, ref Vector3 pos, ref Vector3 angles, float animationTimer) {
        if(progress == 0) {
            // We just started, enter ourselves into the lift queue
            LiftVehicleSystem.EnterQueue(self, link.A);
            progress = 0.1f;
        }
        if(progress == 0.1f) {
            // Waiting for a vehicle; chill at entry node
            pos = link.A.GetPosition3d();
            return;
        }
        if(progress == 0.5f) {
            // Progress will be set to 0.5f by the LiftVehicleSystem when we enter the vehicle
            // The LiftVehicleSystem will also set the position and angles, so we do nothing
        }
    }

    public void OnSaveLoad(Visitor visitor, ref float progress) {
        // Progress changing to 0.1f marks that it's been added to the queue, which doesn't survive across restarts
        if(progress == 0.1f) progress = 0;
    }
}