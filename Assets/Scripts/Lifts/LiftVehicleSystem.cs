using System.Collections.Generic;
using UnityEngine;

public class LiftVehicleSystem {
    public Lift Parent;
    public LiftVehicle TemplateVehicle;
    public List<LiftVehicle> LiftVehicles = new List<LiftVehicle>();
    public float TotalLength { get; private set; }
    public float Speed = 10;

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

        CreateVehicles(TemplateVehicle, 50);
    }

    private void CreateVehicles(LiftVehicle template, float spacing) {
        int numVehicles = (int) (TotalLength / spacing);
        float actualSpacing = TotalLength / numVehicles;

        for(int i = 0;i < numVehicles;i ++) {
            LiftVehicle temp = GameObject.Instantiate(template);
            temp.transform.SetParent(Parent.transform);
            temp.Position = i * actualSpacing;
            LiftVehicles.Add(temp);
        }

        Advance(0);
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

    public Vector2 EvaluateAngles(float pos) {
        Vector4 prev = CablePoints[Parent.CablePoints.Length - 1];
        for(int i = 0;i < CablePoints.Length;i ++) {
            Vector4 current = CablePoints[i];
            if(current.w > pos) {
                Vector3 prevXYZ = new Vector3(prev.x, prev.y, prev.z);
                Vector3 currentXYZ = new Vector3(current.x, current.y, current.z);
                Vector3 delta = currentXYZ - prevXYZ;

                float dy = delta.y;
                delta.y = 0;
                float dh = delta.magnitude;
                float dx = delta.x;
                float dz = delta.z;
                return new Vector2(Mathf.Atan2(dz, dx) * Mathf.Rad2Deg, Mathf.Atan2(dy, dh) * Mathf.Rad2Deg); 
            }

            prev = current;
        }

        return default(Vector2);
    }

    public void Advance(float delta) {
        for(int i = 0;i < LiftVehicles.Count;i ++) {
            LiftVehicles[i].Position += delta * Speed;
            while(LiftVehicles[i].Position > TotalLength) LiftVehicles[i].Position -= TotalLength;

            MoveVehicle(LiftVehicles[i]);            
        }
    }

    private void MoveVehicle(LiftVehicle vehicle) {
        vehicle.transform.position = Evaluate(vehicle.Position);

        Vector2 vehicleAngles = EvaluateAngles(vehicle.Position);
        vehicle.transform.localEulerAngles = new Vector3(0, 90 - vehicleAngles.x, 0);
        vehicle.RotateTransform.localEulerAngles = new Vector3(-90 - vehicleAngles.y, 0, 0);
        vehicle.DerotateTransform.localEulerAngles = new Vector3(vehicleAngles.y, 0, 0);
    }
}