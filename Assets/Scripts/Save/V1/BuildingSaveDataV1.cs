using UnityEngine;

[System.Serializable]
public struct BuildingSaveDataV1 {
    public string Template;
    public Vector3POD Position;
    public float Rotation;
    public NavAreaGraphSaveDataV1[] NavAreaGraphs;

    public static BuildingSaveDataV1 FromSimpleBuilding(SimpleBuilding building, SavingContextV1 context) {
        BuildingSaveDataV1 result = new BuildingSaveDataV1();
        result.Template = building.Template.name;
        result.Position = building.transform.position;
        result.Rotation = building.transform.eulerAngles.y;
        result.NavAreaGraphs = new NavAreaGraphSaveDataV1[building.NavAreas.Count];

        for(int i = 0;i < building.NavAreas.Count;i ++) {
            result.NavAreaGraphs[i] = NavAreaGraphSaveDataV1.FromNavArea(building.NavAreas[i], context);
        }

        return result;
    }
}