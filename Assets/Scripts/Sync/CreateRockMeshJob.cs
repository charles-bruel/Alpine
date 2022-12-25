using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class CreateRockMeshJob : Job
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
    public int NumRocks;
    public Vector3[] OldVertices;
    public Vector3[] OldNormals;
    public Vector2[] OldUVs;
    public int[] OldTriangles;

    public void Initialize() {
        int numVerticesPerModel = OldVertices.Length;
        int numTrianglesPerModel = OldTriangles.Length;

        OutputMeshData = Mesh.AllocateWritableMeshData(1);
        Mesh.MeshData outputMesh = OutputMeshData[0];
        outputMesh.SetIndexBufferParams(numTrianglesPerModel * NumRocks, IndexFormat.UInt32);
        outputMesh.SetVertexBufferParams(numVerticesPerModel * NumRocks,
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
        int numVerticesPerModel = OldVertices.Length;
        int numTrianglesPerModel = OldTriangles.Length;

        RockPos[] Data = TerrainManager.Instance.RocksData;

        //Copy
        for(int i = 0, t = 0;i < Data.Length;i ++) {
            Vector3 pos = Data[i].pos;
            if(Bounds.Contains(pos)) {
                Quaternion rotation = Quaternion.FromToRotation(Vector3.up, Data[i].normal);

                float scaleMul = Data[i].scale;

                //Copy a single rock
                for(int j = 0;j < numVerticesPerModel;j ++) {
                    //This turns "hashes" a vector3 into an int
                    Vector3 temp = OldVertices[j] * 1000;
                    int vertex_id = (int) temp.x + (int) temp.y + (int) temp.z;
                    if(vertex_id < 0) vertex_id *= -1;

                    Vector4 transformedVertex = new Vector4(OldVertices[j].y, OldVertices[j].z, OldVertices[j].x, 1);
                    Vector3 temp2 = transformedVertex;
                    temp2.y *= TerrainManager.Instance.RockSnowMultiplier;
                    LocalCoords[t * numVerticesPerModel + j] = temp2;
                    transformedVertex = (rotation * transformedVertex) * scaleMul;
                    Vertices[t * numVerticesPerModel + j] = transformedVertex.DropW() + pos;
                    Vertices[t * numVerticesPerModel + j] += BumpValues.Values[(i + vertex_id) % BumpValues.Values.Length] * 0.5f;

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

        }

        lock(ASyncJobManager.completedJobsLock) {
        	ASyncJobManager.Instance.completedJobs.Enqueue(this);
		}
    }

    public override void Complete()
    {
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