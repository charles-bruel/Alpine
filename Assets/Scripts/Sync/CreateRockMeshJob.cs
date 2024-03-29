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

using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class CreateRockMeshJob : Job
{
    public Bounds Bounds;
    public byte PosX;
    public byte PosY;
    public Mesh MeshTarget;
    private NativeArray<Vector3> Vertices;
    private NativeArray<Vector3> Normals;
    private NativeArray<Vector2> UVs;
    private NativeArray<int> Triangles;
    private Mesh.MeshDataArray OutputMeshData;
    public int NumRocks;
    public Vector3[] OldVertices;
    public Vector3[] OldNormals;
    public Vector2[] OldUVs;
    public int[] OldTriangles;

    public void Initialize() {
        LoadingScreen.INSTANCE.LoadingTasks++;

        int numVerticesPerModel = OldVertices.Length;
        int numTrianglesPerModel = OldTriangles.Length;

        OutputMeshData = Mesh.AllocateWritableMeshData(1);
        Mesh.MeshData outputMesh = OutputMeshData[0];
        outputMesh.SetIndexBufferParams(numTrianglesPerModel * NumRocks, IndexFormat.UInt32);
        outputMesh.SetVertexBufferParams(numVerticesPerModel * NumRocks,
            new VertexAttributeDescriptor(VertexAttribute.Position),
            new VertexAttributeDescriptor(VertexAttribute.Normal, stream:1),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, stream:2, dimension:2)
        );

        Vertices = outputMesh.GetVertexData<Vector3>(stream:0);
        Normals = outputMesh.GetVertexData<Vector3>(stream:1);
        UVs = outputMesh.GetVertexData<Vector2>(stream:2);
        Triangles = outputMesh.GetIndexData<int>();
    }

    public void Run() {
        //Create data structures
        int numVerticesPerModel = OldVertices.Length;
        int numTrianglesPerModel = OldTriangles.Length;

        GridArray<RockPos> Data = TerrainManager.Instance.RocksData;

        int t = 0;
        var enumerator = Data.GetEnumerator(PosX, PosY);
        while(enumerator.MoveNext()) {
            Quaternion rotation = Quaternion.FromToRotation(Vector3.up, enumerator.Current.normal);

            float scaleMul = enumerator.Current.scale;

            //Copy a single rock
            for(int j = 0;j < numVerticesPerModel;j ++) {
                //This turns "hashes" a vector3 into an int
                Vector3 temp = OldVertices[j] * 1000;
                int vertex_id = (int) temp.x + (int) temp.y + (int) temp.z;
                if(vertex_id < 0) vertex_id *= -1;
                
                Vector4 transformedVertex = new Vector4(OldVertices[j].y, OldVertices[j].z, OldVertices[j].x, 1);
                Vector3 temp2 = transformedVertex;
                temp2.y *= TerrainManager.Instance.RockSnowMultiplier;
                transformedVertex = (rotation * transformedVertex) * scaleMul;
                Vertices[t * numVerticesPerModel + j] = transformedVertex.DropW() + enumerator.Current.pos;
                Vertices[t * numVerticesPerModel + j] += BumpValues.Values[(t + vertex_id) % BumpValues.Values.Length] * scaleMul * 0.5f;
                
                Vector4 transformedNormal = new Vector4(OldNormals[j].y, OldNormals[j].z, OldNormals[j].x, 1); 
                transformedNormal = rotation * transformedNormal;
                Normals[t * numVerticesPerModel + j] = transformedNormal.DropW();

                UVs[t * numVerticesPerModel + j] = OldUVs[j];
            }

            for(int j = 0;j < numTrianglesPerModel;j ++) {
                Triangles[t * numTrianglesPerModel + j] = OldTriangles[j] + t * numVerticesPerModel;
            }

            t++;
        }

        lock(ASyncJobManager.completedJobsLock) {
        	ASyncJobManager.Instance.completedJobs.Enqueue(this);
		}
    }

    public override void Complete() {
        SubMeshDescriptor subMesh = new SubMeshDescriptor(0, Triangles.Length, MeshTopology.Triangles);
        subMesh.firstVertex = 0;
        subMesh.vertexCount = Vertices.Length;

        Mesh.MeshData outputMesh = OutputMeshData[0];

        outputMesh.subMeshCount = 1;
        outputMesh.SetSubMesh(0, subMesh, MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontNotifyMeshUsers);
        Mesh.ApplyAndDisposeWritableMeshData(OutputMeshData, new[]{ MeshTarget }, 
            MeshUpdateFlags.DontRecalculateBounds |
            MeshUpdateFlags.DontValidateIndices   |
            MeshUpdateFlags.DontNotifyMeshUsers
        );
        MeshTarget.bounds = Bounds;

        LoadingScreen.INSTANCE.LoadingTasks--;
    }
}