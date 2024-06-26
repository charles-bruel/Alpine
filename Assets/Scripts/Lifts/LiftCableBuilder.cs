//>============================================================================<
//
//    Alpine, Ski Resort Tycoon Game
//    Copyright (C) 2024  Charles Bruel
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see <https://www.gnu.org/licenses/>.
//>============================================================================<

using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class LiftCableBuilder {

    public List<LiftCablePoint> Points = new List<LiftCablePoint>();

    private Mesh.MeshDataArray OutputMeshData;
    private NativeArray<Vector3> Vertices;
    private NativeArray<Vector3> Normals;
    private NativeArray<Vector2> UVs;
    private NativeArray<int> Triangles;
    private Mesh Target;

    public static readonly int numPoints = 8;
    public static readonly int numTrianglesPerPoint = numPoints * 2;
    public static readonly int numVerticesPerPoint = numPoints;
    public static readonly int numVerticesPerTriangle = 3;

    public LiftCablePoint LastPoint {
        get => Points[Points.Count - 1];
    }

    public Vector3 LastPos {
        get => Points[Points.Count - 1].pos;
    }

    public void AddPointsWithoutSag(List<LiftCablePoint> points) {
        if(Points.Count > 0 && (LastPos - points[0].pos).sqrMagnitude < float.Epsilon) {
            // I believe removing the last element of main array is faster than removing
            // the first element of the added array because last element removes should be faster
            Points.RemoveAt(Points.Count - 1);
        }
        Points.AddRange(points);
    }

    public void AddPointsWithSag(List<LiftCablePoint> points, float lengthMultiplier) {
        for(int i = 0;i < points.Count - 1;i ++) {
            //TODO: Intelligent num points
            List<Vector3> catenaryResult = PointsCatenary(points[i].pos, points[i + 1].pos, lengthMultiplier, 32);

            if(i != points.Count - 2) catenaryResult.RemoveAt(catenaryResult.Count - 1);

            List<LiftCablePoint> injectedResult = new List<LiftCablePoint>(catenaryResult.Count);
            for(int j = 0;j < catenaryResult.Count;j ++) {
                float t = (float) j / (catenaryResult.Count - 1);
                injectedResult.Add(new LiftCablePoint(catenaryResult[j], Mathf.Lerp(points[i].speed, points[i + 1].speed, t)));
            }

            AddPointsWithoutSag(injectedResult);
        }
    }

    public void AddPointsWithSag(LiftCablePoint a, LiftCablePoint b, float lengthMultiplier) {
        List<LiftCablePoint> temp = new List<LiftCablePoint>(2);
        temp.Add(a);
        temp.Add(b);
        AddPointsWithSag(temp, lengthMultiplier);
    }

    private List<Vector3> PointsCatenary(Vector3 a, Vector3 b, float lengthMultiplier, int numPoints) {
        Vector3 d = b - a;
        float L = d.magnitude * lengthMultiplier;
        float dy = d.y;
        d.y = 0;
        float dx = d.magnitude;

        List<Vector2> points = PointsCatenary2D(new Vector2(0, 0), new Vector2(dx, dy), L, numPoints);
        List<Vector3> toReturn = new List<Vector3>(points.Count);

        for(int i = 0;i < points.Count;i ++) {
            // We use lerp to place it horizontally
            Vector3 temp = Vector3.Lerp(a, b, points[i].x / dx);
            // Then we set the y
            temp.y = points[i].y + a.y;
            toReturn.Add(temp);
        }

        return toReturn;
    }

    private List<Vector2> PointsCatenary2D(Vector2 a, Vector2 b, float L, int numPoints) {
        Catenary catenary = Catenary.FromCoordinates(a, b, L);

        List<Vector2> toReturn = new List<Vector2>();
        toReturn.Add(a);

        for(int i = 1;i <= numPoints;i ++) {
            float x = (float) i / (numPoints + 1);
            x = Mathf.Lerp(a.x, b.x, x);
            float y = catenary.Evaluate(x);
            toReturn.Add(new Vector2(x, y));
        }

        toReturn.Add(b);

        return toReturn;
    }

    public void CreateGameObject(Transform parent, Material cableMaterial) {
        GameObject obj = new GameObject("Cable");
        obj.transform.parent = parent;
        obj.transform.position = Vector3.zero;

        MeshRenderer meshRenderer = obj.AddComponent<MeshRenderer>();
        meshRenderer.material = cableMaterial;

        MeshFilter filter = obj.AddComponent<MeshFilter>();
        Target = new Mesh();
        filter.mesh = Target;
        Target.indexFormat = IndexFormat.UInt32;
        Target.MarkDynamic();
    }

    public void StartMesh(int numCables) {
        OutputMeshData = Mesh.AllocateWritableMeshData(1);
        Mesh.MeshData outputMesh = OutputMeshData[0];
        outputMesh.SetIndexBufferParams(numTrianglesPerPoint * Points.Count * numCables * numVerticesPerTriangle, IndexFormat.UInt32);
        outputMesh.SetVertexBufferParams(numVerticesPerPoint * Points.Count * numCables,
            new VertexAttributeDescriptor(VertexAttribute.Position),
            new VertexAttributeDescriptor(VertexAttribute.Normal, stream:1),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, stream:2, dimension:2)
        );

        Vertices = outputMesh.GetVertexData<Vector3>(stream:0);
        Normals = outputMesh.GetVertexData<Vector3>(stream:1);
        UVs = outputMesh.GetVertexData<Vector2>(stream:2);
        Triangles = outputMesh.GetIndexData<int>();
    }

    public void BuildMesh(int cableIndex, Vector3 offset, float thickness) {
        for(int i = 0;i < Points.Count;i ++) {
            BuildRing(cableIndex, i, offset, thickness);
        }
        LinkRings(cableIndex);
    }

    private void LinkRings(int cableIndex) {
        int baseIndex = cableIndex * numTrianglesPerPoint * Points.Count * numVerticesPerTriangle;
        int triangle = 0;
        for(int i = 0;i < Points.Count;i ++) {
            int ipp = i + 1;
            if(ipp >= Points.Count) {
                ipp = 0;
            }

            int baseRingIndex = i * numVerticesPerPoint;
            int baseRingIndexpp = ipp * numVerticesPerPoint;
            for(int j = 0;j < numPoints;j ++) {
                // Ring i  Ring i+1
                // A--------------C
                // |\------------\|
                // B--------------D

                int jpp = j + 1;
                if(jpp >= numPoints) {
                    jpp = 0;
                }

                int a = baseRingIndex + j;
                int b = baseRingIndex + jpp;
                int c = baseRingIndexpp + j;
                int d = baseRingIndexpp + jpp;

                Triangles[baseIndex + triangle * numVerticesPerTriangle + 0] = a;
                Triangles[baseIndex + triangle * numVerticesPerTriangle + 1] = d;
                Triangles[baseIndex + triangle * numVerticesPerTriangle + 2] = c;
                triangle++;
                Triangles[baseIndex + triangle * numVerticesPerTriangle + 0] = a;
                Triangles[baseIndex + triangle * numVerticesPerTriangle + 1] = b;
                Triangles[baseIndex + triangle * numVerticesPerTriangle + 2] = d;
                triangle++;
            }
        }
    }

    private void BuildRing(int cableIndex, int id, Vector3 offset, float thickness) {
        Vector3 mainPoint = Points[id].pos;
        int prevId = id - 1;
        if(prevId < 0) prevId = Points.Count - 1;
        Vector3 prevPoint = Points[prevId].pos;
        int nextId = id + 1;
        if(nextId > Points.Count - 1) {
            nextId = 0;
        }
        Vector3 nextPoint = Points[nextId].pos;

        Vector3 direction = (nextPoint - prevPoint);
        Vector3 rightAxis = Vector3.Cross(direction, Vector3.up).normalized;

        Quaternion aboutDirection = Quaternion.AngleAxis(90, direction);

        Vector3 upAxis = aboutDirection * rightAxis;

        //TODO: Offset

        for(int i = 0;i < numPoints;i ++) {
            float angleRads = 2 * Mathf.PI * ((float) i / numPoints);
            float cos = Mathf.Cos(angleRads);
            float sin = Mathf.Sin(angleRads);
            Vector3 pos = mainPoint + (cos * rightAxis + sin * upAxis) * thickness;
            Vector3 normal = -sin * rightAxis + cos * upAxis;
            int index = cableIndex * numVerticesPerPoint * Points.Count + id * numVerticesPerPoint + i;
            Vertices[index] = pos;
            UVs[index] = new Vector2(0, 0);
            Normals[index] = normal;
        }
    }

    public void FinalizeMesh() {
        SubMeshDescriptor subMesh = new SubMeshDescriptor(0, Triangles.Length, MeshTopology.Triangles);
        subMesh.firstVertex = 0;
        subMesh.vertexCount = Vertices.Length;

        Mesh.MeshData outputMesh = OutputMeshData[0];

        outputMesh.subMeshCount = 1;
        outputMesh.SetSubMesh(0, subMesh, MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontNotifyMeshUsers);
        Mesh.ApplyAndDisposeWritableMeshData(OutputMeshData, new[]{ Target }, 
            MeshUpdateFlags.DontValidateIndices |
            MeshUpdateFlags.DontNotifyMeshUsers
        );

        Target.RecalculateBounds();
    }
}