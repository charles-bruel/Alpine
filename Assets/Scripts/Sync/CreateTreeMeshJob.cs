using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class CreateTreeMeshJob : Job
{
    public Bounds Bounds;
    public Mesh MeshTarget;
    private NativeArray<Vector3> Vertices;
    private NativeArray<Vector3> LocalCoords;
    private NativeArray<Vector3> Normals;
    //Array is Vec3 so that stride matches
    //TODO: Find away to fix this
    private NativeArray<Vector3> UVs;
    private NativeArray<int> Triangles;
    private Mesh.MeshDataArray OutputMeshData;

    //TODO: Make number of template meshes variable
    public int NumTrees1;
    public int NumTrees2;
    public Vector3[] OldVertices1;
    public Vector3[] OldNormals1;
    public Vector2[] OldUVs1;
    public int[] OldTriangles1;
    public Vector3[] OldVertices2;
    public Vector3[] OldNormals2;
    public Vector2[] OldUVs2;
    public int[] OldTriangles2;

    // Based on
    // https://github.com/Unity-Technologies/MeshApiExamples/blob/master/Assets/CreateMeshFromAllSceneMeshes/CreateMeshFromWholeScene.cs

    public void Initialize() {
        int numVerticesPerModel1 = OldVertices1.Length;
        int numTrianglesPerModel1 = OldTriangles1.Length;
        int numVerticesPerModel2 = OldVertices2.Length;
        int numTrianglesPerModel2 = OldTriangles2.Length;

        OutputMeshData = Mesh.AllocateWritableMeshData(1);
        Mesh.MeshData outputMesh = OutputMeshData[0];
        outputMesh.SetIndexBufferParams(numTrianglesPerModel1 * NumTrees1 + numTrianglesPerModel2 * NumTrees2, IndexFormat.UInt32);
        outputMesh.SetVertexBufferParams(numVerticesPerModel1 * NumTrees1 + numVerticesPerModel2 * NumTrees2,
            new VertexAttributeDescriptor(VertexAttribute.Position),
            new VertexAttributeDescriptor(VertexAttribute.Normal, stream:1),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, stream:2),
            new VertexAttributeDescriptor(VertexAttribute.Color, stream:3)
        );

        Vertices = outputMesh.GetVertexData<Vector3>(stream:0);
        LocalCoords = outputMesh.GetVertexData<Vector3>(stream:3);
        Normals = outputMesh.GetVertexData<Vector3>(stream:1);
        UVs = outputMesh.GetVertexData<Vector3>(stream:2);
        Triangles = outputMesh.GetIndexData<int>();

    }

    public void Run() {
        //Create data structures
        int numVerticesPerModel1 = OldVertices1.Length;
        int numTrianglesPerModel1 = OldTriangles1.Length;
        int numVerticesPerModel2 = OldVertices2.Length;
        int numTrianglesPerModel2 = OldTriangles2.Length;

        //Copy
        Copy(
            1, 0, 0, TerrainManager.Instance.Tree1SnowMultiplier,
            OldVertices1, OldNormals1, OldUVs1, OldTriangles1,
            numVerticesPerModel1, numTrianglesPerModel1
        );

        Copy(
            2, numVerticesPerModel1 * NumTrees1, numTrianglesPerModel1 * NumTrees1, TerrainManager.Instance.Tree2SnowMultiplier,
            OldVertices2, OldNormals2, OldUVs2, OldTriangles2,
            numVerticesPerModel2, numTrianglesPerModel2
        );

        lock(ASyncJobManager.completedJobsLock) {
        	ASyncJobManager.Instance.completedJobs.Enqueue(this);
		}
    }

    private void Copy(
        byte TypeToLookFor, int verticesBaseIndex, int trianglesBaseIndex, float Stickiness,
        Vector3[] OldVertices, Vector3[] OldNormals, Vector2[] OldUVs, int[] OldTriangles,
        int numVerticesPerModel, int numTrianglesPerModel
    ) {

        TreePos[] Data = TerrainManager.Instance.TreesData;

        for(int i = 0, t = 0;i < Data.Length;i ++) {
            Vector3 pos = Data[i].pos;
            if(Data[i].type == TypeToLookFor && Bounds.Contains(pos)) {
                float sinTheta = Mathf.Sin(Data[i].rot);
                float cosTheta = Mathf.Cos(Data[i].rot);
                float scaleMul = Data[i].scale;

                //Copy a single tree
                for(int j = 0;j < numVerticesPerModel;j ++) {
                    Vector3 transformedVertex = new Vector3(
                        OldVertices[j].y * cosTheta - OldVertices[j].x * sinTheta, 
                        OldVertices[j].z, 
                        OldVertices[j].y * sinTheta + OldVertices[j].x * cosTheta
                    );
                    Vector3 temp = transformedVertex;
                    temp.y *= Stickiness;
                    LocalCoords[t * numVerticesPerModel + j + verticesBaseIndex] = temp; 
                    Vertices[t * numVerticesPerModel + j + verticesBaseIndex] = transformedVertex * scaleMul + pos;

                    Vector3 transformedNormal = new Vector3(
                        OldNormals[j].y * cosTheta - OldNormals[j].x * sinTheta, 
                        OldNormals[j].z, 
                        OldNormals[j].y * sinTheta + OldNormals[j].x * cosTheta
                    ); 
                    Normals[t * numVerticesPerModel + j + verticesBaseIndex] = transformedNormal;

                    UVs[t * numVerticesPerModel + j + verticesBaseIndex] = OldUVs[j];
                }
                for(int j = 0;j < numTrianglesPerModel;j ++) {
                    Triangles[t * numTrianglesPerModel + j + trianglesBaseIndex] = OldTriangles[j] + t * numVerticesPerModel + verticesBaseIndex;
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
}