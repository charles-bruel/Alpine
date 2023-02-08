using System.Collections.Generic;
using UnityEngine;

public class LiftVehicleSystem {
    public Lift Parent;
    public LiftVehicleTemplate TemplateVehicle;
    public float TotalLength { get; private set; }

    public Vector4[] CablePoints;

    public void Initialize() {
        CablePoints = new Vector4[Parent.CablePoints.Length];

        // We compute the distances
        float pos = 0;
        Vector3 prev = Parent.CablePoints[Parent.CablePoints.Length - 1].pos;
        for(int i = 0;i < CablePoints.Length;i ++) {
            Vector3 point = Parent.CablePoints[i].pos;
            //A slower point means the effective distance is longer
            pos += (point - prev).magnitude / Parent.CablePoints[i].speed;
            CablePoints[i] = new Vector4(point.x, point.y, point.z, pos);
            prev = point;
        }

        TotalLength = pos;

        CreateVehicles(TemplateVehicle.gameObject, 50);
    }

    private void CreateVehicles(GameObject g, float spacing) {
        int numVehicles = (int) (TotalLength / spacing);
        float actualSpacing = TotalLength / numVehicles;

        for(int i = 0;i < numVehicles;i ++) {
            GameObject temp = GameObject.Instantiate(g);
            temp.transform.SetParent(Parent.transform);
            temp.transform.position = Evaluate(i * actualSpacing);
        }
    }
    
    public Vector3 Evaluate(float pos) {
        Vector4 prev = CablePoints[Parent.CablePoints.Length - 1];
        for(int i = 0;i < CablePoints.Length;i ++) {
            Vector4 current = CablePoints[i];
            if(current.w > pos) {
                float betweenT = Mathf.InverseLerp(prev.w, current.w, pos);
                Vector3 prevXYZ = new Vector3(prev.x, prev.y, prev.z);
                Vector3 currentXYZ = new Vector3(current.x, current.y, current.z);

                Vector3 result = Vector3.Lerp(prevXYZ, currentXYZ, betweenT);

                return result;
            }

            prev = current;
        }

        return default(Vector3);
    }

    public void Update(float delta) {

    }

    private void MoveVehicle(LiftVehicle vehicle, float delta) {

    }
}