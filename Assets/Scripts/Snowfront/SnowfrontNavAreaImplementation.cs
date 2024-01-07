using UnityEngine;

public class SnowfrontNavAreaImplementation : INavAreaImplementation {
    public Snowfront Owner;
    public NavArea Area;
        
    public SnowfrontNavAreaImplementation(Snowfront owner) {
        Owner = owner;
        Area = owner.Footprint;
    }

    public void OnAdvance(float delta) {

    }

    public void OnAdvanceSelected(float delta) {

    }

    public void OnDeselected() {
        
    }

    public void OnRemove() {
        
    }

    public void OnSelected() {

    }
}