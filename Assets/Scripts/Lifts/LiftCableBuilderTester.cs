using UnityEngine;
using System.Collections.Generic;
using System;

public class LiftCableBuilderTester : MonoBehaviour {

    public Material CableMaterial;

    void Start() {
        LiftCableBuilder builder = new LiftCableBuilder();

        builder.AddPointsWithSag(GenerateTestPoints(32), 1.01f);

        builder.CreateGameObject(transform, CableMaterial);
        builder.StartMesh(1);
        builder.BuildMesh(0, new Vector3(), 0.1f);
        builder.FinalizeMesh();
    }

    private List<Vector3> GenerateTestPoints(int v)
    {
        List<Vector3> toReturn = new List<Vector3>(v);
        for(int i = 0;i < v;i ++) {
            float angleRads = 2 * MathF.PI * ((float)i/v);
            toReturn.Add(new Vector3(Mathf.Sin(angleRads) * 64, 0, Mathf.Cos(angleRads) * 64));
        }
        return toReturn;
    }
}