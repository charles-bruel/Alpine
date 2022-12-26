using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class CreateTreeMeshJob : Job
{
    public Bounds Bounds;
    public Mesh MeshTarget;
    public TreeTypeDescriptorForJob[] Descriptors;
    private NativeArray<Vector3> Vertices;
    private NativeArray<Vector3> LocalCoords;
    private NativeArray<Vector3> Normals;
    private NativeArray<Vector2> UVs;
    private NativeArray<int> Triangles;
    private Mesh.MeshDataArray OutputMeshData;

    // Based on
    // https://github.com/Unity-Technologies/MeshApiExamples/blob/master/Assets/CreateMeshFromAllSceneMeshes/CreateMeshFromWholeScene.cs

    public void Initialize() {
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
            Copy(i, totalVertices, totalTriangles, Descriptors[i]);

            totalVertices  += Descriptors[i].OldVertices .Length * Descriptors[i].NumTrees;
            totalTriangles += Descriptors[i].OldTriangles.Length * Descriptors[i].NumTrees;
        }

        lock(ASyncJobManager.completedJobsLock) {
        	ASyncJobManager.Instance.completedJobs.Enqueue(this);
		}
    }

    private void Copy(uint TypeToLookFor, int verticesBaseIndex, int trianglesBaseIndex, TreeTypeDescriptorForJob descriptor) {
        TreePos[] Data = TerrainManager.Instance.TreesData;

        int numVerticesPerModel  = descriptor.OldVertices .Length;
        int numTrianglesPerModel = descriptor.OldTriangles.Length;

        for(int i = 0, t = 0;i < Data.Length;i ++) {
            Vector3 pos = Data[i].pos;
            if(Data[i].type == TypeToLookFor && Bounds.Contains(pos)) {
                float sinTheta = Mathf.Sin(Data[i].rot);
                float cosTheta = Mathf.Cos(Data[i].rot);
                float scaleMul = Data[i].scale;

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

    public override void Complete()
    {
        // MeshTarget.Optimize();

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