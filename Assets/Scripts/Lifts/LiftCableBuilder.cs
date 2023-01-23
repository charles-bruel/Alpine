using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class LiftCableBuilder {

    public List<Vector3> Points;

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
        Vector3 mainPoint = Points[id];
        int prevId = id - 1;
        if(prevId < 0) prevId = Points.Count - 1;
        Vector3 prevPoint = Points[prevId];
        int nextId = id + 1;
        if(nextId > Points.Count - 1) {
            nextId = 0;
        }
        Vector3 nextPoint = Points[nextId];

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