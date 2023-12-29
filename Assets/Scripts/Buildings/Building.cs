using UnityEngine;
using System;

public class Building : MonoBehaviour {
    public virtual void Advance(float delta) {

    }

    public virtual void Initialize() {

    }

    public virtual void Destroy() {
        BuildingsController.Instance.UnregisterBuilding(this);
        Destroy(gameObject);
    }
}