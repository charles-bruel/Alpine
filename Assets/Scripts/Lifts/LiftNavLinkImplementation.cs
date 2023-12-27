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
}