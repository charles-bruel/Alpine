using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class LiftVehicleSystem {
    public Lift Parent;
    public LiftVehicle TemplateVehicle;
    public List<LiftVehicle> LiftVehicles = new List<LiftVehicle>();
    public float TotalLength { get; private set; }
    public float Speed = 10;

    public LiftVehicleSystemCablePoint[] CablePoints;

    public List<LiftAccessNode> LiftAccessNodes;

    public void Initialize(List<LiftAccessNode> cableJoins) {
        LiftAccessNodes = cableJoins;

        CablePoints = new LiftVehicleSystemCablePoint[Parent.CablePoints.Length];

        // We compute the distances
        float pos = 0;
        LiftCablePoint prev = Parent.CablePoints[Parent.CablePoints.Length - 1];
        for(int i = 0;i < CablePoints.Length;i ++) {
            Vector3 point = Parent.CablePoints[i].pos;
            float speed = Parent.CablePoints[i].speed;
            //Since speed changes linearly, a simple average speed will suffice
            float avgSpeed = ((speed + prev.speed) * 0.5f);
            //A slower point means the effective distance is longer
            pos += (point - prev.pos).magnitude / avgSpeed;
            CablePoints[i] = new LiftVehicleSystemCablePoint(point, speed, pos);
            prev = Parent.CablePoints[i];
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
    
    public CableEvaluationResult Evaluate(float pos) {
        LiftVehicleSystemCablePoint prev = CablePoints[Parent.CablePoints.Length - 1];
        for(int i = 0;i < CablePoints.Length;i ++) {
            LiftVehicleSystemCablePoint current = CablePoints[i];
            if(current.cablePosition > pos) {
                float betweenT = Mathf.InverseLerp(prev.cablePosition, current.cablePosition, pos);

                if(prev.speed != current.speed) {
                    // We need to find a way to interpolate the position so that it
                    // smoothly accelerates between the points. As per the comment
                    // in Initialize(), since speed has a linear change, the average
                    // speed will be simply the average of the start and end speeds.

                    // Let d_t be the true distance between the points and d_a be the
                    // adjusted distance between the points (taking into account a speed
                    // that isn't 1).
                    float d_t = (current.worldPosition - prev.worldPosition).magnitude;
                    float d_a = current.cablePosition - prev.cablePosition;
                    // The function normally exists on a normalized range [0, 1] to lerp
                    // between the points. We will operate on the range [0, t_f], where
                    // 1 : t_f = d_t : d_a, to account for the lower speed
                    float t_f = d_a / d_t;

                    // Let the initial velocity be v_i and the final velocity be v_f
                    float v_i = prev.speed;
                    float v_f = current.speed;
                    // We want to find a function f(x) which fulfills the following requirements:
                    // f(0)    = 0
                    // f(t_f)  = 1
                    // f'(0)   = v_i
                    // f'(t_f) = v_f
                    // Those requirements ensure that f(x) interpolates the range correctly and
                    // the starts and finishes at the desired points (we will plug it into a lerp).
                    // Additionally, it ensures that the velocity will be continious. More specifically:
                    // f'(x) = (1 - x/t_f) * v_i + (x / t_f) * v_f on the range [0, t_f]
                    // That is a standard lerp just adjusted to end at t_f not 1
                    // Expanding we get v_i - (x / t_f) * v_i  + (x / t_f) * v_f
                    // Simplifying we get f'(x) = v_i + (x / t_f) * (v_f - v_i)

                    // Taking the antiderivative, we get 
                    // f(x) = (0.5 * (v_f - v_i) / t_f) * x^2 + x * v_i + C
                    // Since one of our requirements is f(0) = 0, and without C
                    // the function will always evaluate as 0, C = 0
                    // Graphing this function in desmos can verify the math
                    float x = t_f * betweenT;
                    float fxv = (0.5f * (v_f - v_i) / t_f) * x * x + x * v_i;
                    betweenT = fxv;

                    // Real world applications of calculus!
                }

                Vector3 prevXYZ = prev.worldPosition;
                Vector3 currentXYZ = current.worldPosition;
                Vector3 delta = currentXYZ - prevXYZ;

                Vector3 worldPosition = Vector3.Lerp(prevXYZ, currentXYZ, betweenT);

                float dy = delta.y;
                delta.y = 0;
                float dh = delta.magnitude;
                float dx = delta.x;
                float dz = delta.z;

                float horizontalAngle = Mathf.Atan2(dz, dx) * Mathf.Rad2Deg;
                float verticalAngle = Mathf.Atan2(dy, dh) * Mathf.Rad2Deg;

                return new CableEvaluationResult(worldPosition, horizontalAngle, verticalAngle);
            }

            prev = current;
        }

        return default(CableEvaluationResult);
    }

    public struct CableEvaluationResult {
        public Vector3 position;
        public float horizontalAngle;
        public float verticalAngle;

        public CableEvaluationResult(Vector3 pos, float horizontalAngle, float verticalAngle)
        {
            this.position = pos;
            this.horizontalAngle = horizontalAngle;
            this.verticalAngle = verticalAngle;
        }
    }

    public void Advance(float delta) {
        for(int i = 0;i < LiftVehicles.Count;i ++) {
            AdvanceVehicle(i, delta * Speed);
            while(LiftVehicles[i].Position > TotalLength) LiftVehicles[i].Position -= TotalLength;

            MoveVehicle(LiftVehicles[i], delta);            
        }
    }

    private void AdvanceVehicle(int vehicleIndex, float quantity) {
        int initial = FindID(LiftVehicles[vehicleIndex].Position);
        LiftVehicles[vehicleIndex].Position += quantity;
        int final = FindID(LiftVehicles[vehicleIndex].Position);
        if(initial == final) return;

        // We've looped around
        if (initial > final) {
            for(int i = initial; i < CablePoints.Length;i ++) {
                TryCablePoint(vehicleIndex, i);
            }
            for(int i = 0; i < final;i ++) {
                TryCablePoint(vehicleIndex, i);
            }
        }

        for(int i = initial; i < final;i ++) {
            TryCablePoint(vehicleIndex, i);
        }
    }

    private void TryCablePoint(int vehicleIndex, int i) {
        foreach (LiftAccessNode node in LiftAccessNodes) {
            if (node.Index == i) {
                // Exit first so people can board in the spaces left
                if (node.Exit != null) {
                    OnExit(vehicleIndex, node.Exit);
                }
                if (node.Entry != null) {
                    OnEnter(vehicleIndex, node.Entry);
                }
            }
        }
    }

    private void OnExit(int vehicleIndex, INavNode exit) {
        Debug.Log("OnExit");
    }
    
    private void OnEnter(int vehicleIndex, INavNode entry) {
        Debug.Log("OnEnter");
    }

    private int FindID(float position) {
        for(int i = 0;i < CablePoints.Length;i ++) {
            LiftVehicleSystemCablePoint current = CablePoints[i];
            if(current.cablePosition > position) {
                return i - 1;
            }
        }
        return -1;
    }

    private void MoveVehicle(LiftVehicle vehicle, float delta) {
        CableEvaluationResult result = Evaluate(vehicle.Position);

        if(delta != 0) {
            float velocity = ((result.position - vehicle.transform.position) / delta).magnitude;
            vehicle.Acceleration = (velocity - vehicle.Velocity) / delta;
            vehicle.Velocity = velocity;

            vehicle.UpdateSwing(delta);
        }

        vehicle.transform.position = result.position;

        vehicle.transform.localEulerAngles =         new Vector3(0, 90 - result.horizontalAngle, 0);
        vehicle.RotateTransform.localEulerAngles =   new Vector3(-90 - result.verticalAngle, 0, 0);
        vehicle.DerotateTransform.localEulerAngles = new Vector3(result.verticalAngle + vehicle.Theta * Mathf.Rad2Deg, 0, 0);
    }

    public struct LiftVehicleSystemCablePoint {
        public Vector3 worldPosition;
        public float speed;
        public float cablePosition;

        public LiftVehicleSystemCablePoint(Vector3 worldPosition, float speed, float cablePosition)
        {
            this.worldPosition = worldPosition;
            this.speed = speed;
            this.cablePosition = cablePosition;
        }
    }

    public struct LiftAccessNode {
        public int Index;
        
        // Allowed to be null
        public INavNode Entry;
        // Allowed to be null
        public INavNode Exit;
    }
}