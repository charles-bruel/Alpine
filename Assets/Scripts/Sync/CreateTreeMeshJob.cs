using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class CreateTreeMeshJob : Job
{
    public byte PosX;
    public byte PosY;
    public Bounds Bounds;
    public Mesh MeshTarget;
    public TreeTypeDescriptorForJob[] Descriptors;
    private NativeArray<Vector3> Vertices;
    private NativeArray<Vector3> LocalCoords;
    private NativeArray<Vector3> Normals;
    private NativeArray<Vector2> UVs;
    private NativeArray<int> Triangles;
    private Mesh.MeshDataArray OutputMeshData;
    private bool JobFailed = false;

    // Based on
    // https://github.com/Unity-Technologies/MeshApiExamples/blob/master/Assets/CreateMeshFromAllSceneMeshes/CreateMeshFromWholeScene.cs

    public void Initialize() {
        LoadingScreen.INSTANCE.LoadingTasks++;

        int[] numVerticesPerModel = new int[Descriptors.Length];
        int[] numTrianglesPerModel = new int[Descriptors.Length];
        int totalVertices = 0;
        int totalTriangles = 0;

        for(int i = 0;i < Descriptors.Length;i ++) {
            numVerticesPerModel[i]  = Descriptors[i].OldVertices .Length;
            numTrianglesPerModel[i] = Descriptors[i].OldTriangles.Length;

            totalVertices  += Descriptors[i].OldVertices .Length * Descriptors[i].NumTrees;
            totalTriangles += Descriptors[i].OldTriangles.Length * Descriptors[i].NumTrees;
        }

        OutputMeshData = Mesh.AllocateWritableMeshData(1);
        Mesh.MeshData outputMesh = OutputMeshData[0];
        outputMesh.SetIndexBufferParams(totalTriangles, IndexFormat.UInt32);
        outputMesh.SetVertexBufferParams(totalVertices,
            new VertexAttributeDescriptor(VertexAttribute.Position),
            new VertexAttributeDescriptor(VertexAttribute.Normal, stream:1),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, stream:2, dimension:2),
            new VertexAttributeDescriptor(VertexAttribute.Color, stream:3)
        );

        Vertices = outputMesh.GetVertexData<Vector3>(stream:0);
        LocalCoords = outputMesh.GetVertexData<Vector3>(stream:3);
        Normals = outputMesh.GetVertexData<Vector3>(stream:1);
        UVs = outputMesh.GetVertexData<Vector2>(stream:2);
        Triangles = outputMesh.GetIndexData<int>();

    }

    public void Run() {
        int totalVertices = 0;
        int totalTriangles = 0;
        for(uint i = 0;i < Descriptors.Length;i ++) {
            try{
                Copy(i, totalVertices, totalTriangles, Descriptors[i]);
            } catch (InvalidOperationException) {
                // This occurs when the backing tree collection is updated while we are generating a new mesh.
                // We simply swallow the error and abort the job. We do have to trigger the dirty status in case
                // the update is in another tile and this would not be marked dirty.

                // We need to clean up from the main thread, so we mark the job as failed and "complete" it
                JobFailed = true;
                lock(ASyncJobManager.completedJobsLock) {
        	        ASyncJobManager.Instance.completedJobs.Enqueue(this);
		        }

                //We remark the job as dirty
                TerrainTile tile = TerrainManager.Instance.Tiles[PosX + TerrainManager.Instance.NumTilesX * PosY];
                tile.DirtyStates |= TerrainTile.TerrainTileDirtyStates.TREES;
                TerrainManager.Instance.Dirty.Enqueue(tile);

                return;
            }

            totalVertices  += Descriptors[i].OldVertices .Length * Descriptors[i].NumTrees;
            totalTriangles += Descriptors[i].OldTriangles.Length * Descriptors[i].NumTrees;
        }

        lock(ASyncJobManager.completedJobsLock) {
        	ASyncJobManager.Instance.completedJobs.Enqueue(this);
		}
    }

    private void Copy(uint TypeToLookFor, int verticesBaseIndex, int trianglesBaseIndex, TreeTypeDescriptorForJob descriptor) {
        GridArray<TreePos> Data = TerrainManager.Instance.TreesData;

        int numVerticesPerModel  = descriptor.OldVertices .Length;
        int numTrianglesPerModel = descriptor.OldTriangles.Length;

        int t = 0;
        var enumerator = Data.GetEnumerator(PosX, PosY);
        while(enumerator.MoveNext()) {
            Vector3 pos = enumerator.Current.pos;
            if(enumerator.Current.type == TypeToLookFor) {
                float sinTheta = Mathf.Sin(enumerator.Current.rot);
                float cosTheta = Mathf.Cos(enumerator.Current.rot);
                float scaleMul = enumerator.Current.scale;

                //Copy a single tree
                for(int j = 0;j < numVerticesPerModel;j ++) {
                    Vector3 transformedVertex = new Vector3(
                        descriptor.OldVertices[j].y * cosTheta - descriptor.OldVertices[j].x * sinTheta, 
                        descriptor.OldVertices[j].z, 
                        descriptor.OldVertices[j].y * sinTheta + descriptor.OldVertices[j].x * cosTheta
                    );
                    Vector3 temp = transformedVertex;
                    temp.y *= descriptor.SnowMultiplier;
                    LocalCoords[t * numVerticesPerModel + j + verticesBaseIndex] = temp; 
                    Vertices[t * numVerticesPerModel + j + verticesBaseIndex] = transformedVertex * scaleMul + pos;

                    Vector3 transformedNormal = new Vector3(
                        descriptor.OldNormals[j].y * cosTheta - descriptor.OldNormals[j].x * sinTheta, 
                        descriptor.OldNormals[j].z, 
                        descriptor.OldNormals[j].y * sinTheta + descriptor.OldNormals[j].x * cosTheta
                    ); 
                    Normals[t * numVerticesPerModel + j + verticesBaseIndex] = transformedNormal;

                    UVs[t * numVerticesPerModel + j + verticesBaseIndex] = descriptor.OldUVs[j];
                }
                for(int j = 0;j < numTrianglesPerModel;j ++) {
                    Triangles[t * numTrianglesPerModel + j + trianglesBaseIndex] = descriptor.OldTriangles[j] + t * numVerticesPerModel + verticesBaseIndex;
                }

                t++;
            }

        }
    }

    public override void Complete() {
        // The job had to be aborted for some reason
        // The aborter cleaned up everything it could, be these resources need to be freed from the main thread
        if(JobFailed) {
            OutputMeshData.Dispose();
            return;
        }

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

    public class TreeTypeDescriptorForJob {
        public int NumTrees;
        public Vector3[] OldVertices;
        public Vector3[] OldNormals;
        public Vector2[] OldUVs;
        public int[] OldTriangles;
        public float SnowMultiplier;
    }
}