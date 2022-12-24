using UnityEngine;
using UnityEngine.Rendering;

public class CreateRockMeshJob : Job
{
    public Bounds Bounds;
    public Mesh MeshTarget;
    public Vector3[] Vertices;
    public Vector3[] Normals;
    public Vector2[] UVs;
    public int[] Triangles;
    public int NumRocks;
    public Vector3[] OldVertices;
    public Vector3[] OldNormals;
    public Vector2[] OldUVs;
    public int[] OldTriangles;

    public void Run() {
        //Create data structures
        int numVerticesPerModel = OldVertices.Length;
        int numTrianglesPerModel = OldTriangles.Length;
        Vertices = new Vector3[numVerticesPerModel * NumRocks];
        Normals = new Vector3[numVerticesPerModel * NumRocks];
        UVs = new Vector2[numVerticesPerModel * NumRocks];
        Triangles = new int[numTrianglesPerModel * NumRocks];

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

                    Vector4 transformedVertex = new Vector4(OldVertices[j].y, OldVertices[j].z, OldVertices[j].x, 1) * scaleMul;
                    transformedVertex = rotation * transformedVertex;
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
        var watch = new System.Diagnostics.Stopwatch();  
        watch.Start();

        MeshTarget.Clear();
        MeshTarget.SetVertices(Vertices);
        MeshTarget.SetNormals(Normals);
        MeshTarget.SetUVs(0, UVs);
        MeshTarget.SetTriangles(Triangles, 0, false);
        MeshTarget.bounds = Bounds;
        MeshTarget.Optimize();

        watch.Stop();
        // Debug.Log($"Mesh Set Execution Time: {watch.ElapsedMilliseconds} ms");
    }
}