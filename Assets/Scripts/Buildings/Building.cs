using UnityEngine;
using System;
using UnityEditor.Experimental.GraphView;

public abstract class Building : MonoBehaviour, IUISelectable {
    public virtual void Advance(float delta) {

    }

    public virtual void Initialize() {

    }

    public virtual void Destroy() {
        BuildingsController.Instance.UnregisterBuilding(this);
        Destroy(gameObject);
    }

    public abstract string GetBuildingTypeName();

    public abstract void OnSelected();

    public abstract void OnDeselected();
}