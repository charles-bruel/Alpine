using TMPro;
using UnityEngine;

public class BuildingPanelUI : MonoBehaviour {
    public Building CurrentBuilding;
    public TMP_Text Name;

    public void Inflate(Building newBuilding) {
        CurrentBuilding = newBuilding;
        gameObject.SetActive(true);
        Name.text = newBuilding.GetBuildingTypeName();
    }

    public void Hide() {
        CurrentBuilding = null;
        gameObject.SetActive(false);
    }

    public void OnDeleteButtonPressed() {
        CurrentBuilding.Destroy();
    }
}