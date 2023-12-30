using UnityEngine;
using System;

public abstract class Building : MonoBehaviour {
    public virtual void Advance(float delta) {

    }

    public virtual void Initialize() {

    }

    public virtual void Destroy() {
        BuildingsController.Instance.UnregisterBuilding(this);
        Destroy(gameObject);
    }

    public abstract string GetBuildingTypeName();
}