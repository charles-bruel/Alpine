using UnityEngine;

public abstract class BuildingFunctionality : MonoBehaviour {
    public SimpleBuilding Building;

    public abstract void OnFinishConstruction();
    public abstract void OnDestroy();
}