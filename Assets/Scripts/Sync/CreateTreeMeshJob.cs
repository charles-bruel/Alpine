using UnityEngine;
using UnityEngine.Rendering;

public class CreateTreeMeshJob : Job
{
    public Bounds Bounds;
    public Mesh MeshTarget;
    public Vector3[] Vertices;
    public Vector3[] Normals;
    public Vector2[] UVs;
    public int[] Triangles;
    public int NumTrees;
    public Vector3[] OldVertices;
    public Vector3[] OldNormals;
    public Vector2[] OldUVs;
    public int[] OldTriangles;

    public void Run() {
        //Create data structures
        int numVerticesPerModel = OldVertices.Length;
        int numTrianglesPerModel = OldTriangles.Length;
        Vertices = new Vector3[numVerticesPerModel * NumTrees];
        Normals = new Vector3[numVerticesPerModel * NumTrees];
        UVs = new Vector2[numVerticesPerModel * NumTrees];
        Triangles = new int[numTrianglesPerModel * NumTrees];

        TreePos[] Data = TerrainManager.Instance.TreesData;

        //Copy
        for(int i = 0, t = 0;i < Data.Length;i ++) {
            Vector3 pos = Data[i].pos;
            if(Bounds.Contains(pos)) {
                float sinTheta = Mathf.Sin(Data[i].rot);
                float cosTheta = Mathf.Cos(Data[i].rot);
                float scaleMul = Data[i].scale;

                //Copy a single tree
                for(int j = 0;j < numVerticesPerModel;j ++) {
                    Vector3 transformedVertex = new Vector3(
                        OldVertices[j].y * cosTheta - OldVertices[j].x * sinTheta, 
                        OldVertices[j].z, 
                        OldVertices[j].y * sinTheta + OldVertices[j].x * cosTheta
                    ) * scaleMul;
                    Vertices[t * numVerticesPerModel + j] = transformedVertex + pos;

                    Vector3 transformedNormal = new Vector3(
                        OldNormals[j].y * cosTheta - OldNormals[j].x * sinTheta, 
                        OldNormals[j].z, 
                        OldNormals[j].y * sinTheta + OldNormals[j].x * cosTheta
                    ); 
                    Normals[t * numVerticesPerModel + j] = transformedNormal;

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

        Mesh.AllocateWritableMeshData(1);

        MeshTarget.Clear();
        MeshTarget.SetVertices(Vertices);
        MeshTarget.SetNormals(Normals);
        MeshTarget.SetUVs(0, UVs);
        MeshTarget.SetTriangles(Triangles, 0, false);
        MeshTarget.bounds = Bounds;

        watch.Stop();
        Debug.Log($"Mesh Set Execution Time: {watch.ElapsedMilliseconds} ms");
    }
}