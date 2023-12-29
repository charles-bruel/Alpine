using System.Xml.Schema;
using Mono.Cecil;
using UnityEngine;

public class SlopeNavAreaImplementation : INavAreaImplementation {
    public Slope Owner;
    public NavArea Area;
    public Rect Bounds;
    
    public SlopeNavAreaImplementation(Slope owner, Rect bounds) {
        Owner = owner;
        Area = owner.Footprint;
        Bounds = bounds;
    }

    public void OnAdvance(float delta) {

    }

    public void OnAdvanceSelected(float delta) {

    }

    public void OnDeselected() {
        BuildingsController.Instance.SlopePanelUI.Hide();
    }

    public void OnRemove() {
        
    }

    public void OnSelected() {
        BuildingsController.Instance.SlopePanelUI.Inflate(Owner);
    }
}