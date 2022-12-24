using UnityEngine;
using UnityEngine.Rendering;

public class CreateTreeMeshJob : Job
{
    public Bounds Bounds;
    public Mesh Mesh;
    public Vector3[] Vertices;
    public Vector2[] UVs;
    public int[] Triangles;
    public int NumTrees;
    public Vector3[] OldVertices;
    public Vector2[] OldUVs;
    public int[] OldTriangles;

    public void Run() {
        //Create data structures
        int numVerticesPerModel = OldVertices.Length;
        int numTrianglesPerModel = OldTriangles.Length;
        Vertices = new Vector3[numVerticesPerModel * NumTrees];
        UVs = new Vector2[numVerticesPerModel * NumTrees];
        Triangles = new int[numTrianglesPerModel * NumTrees];

        TreePos[] Data = TerrainManager.Instance.TreesData;

        //Copy
        for(int i = 0, t = 0;i < Data.Length;i ++) {
            Vector3 pos = Data[i].pos;
            if(Bounds.Contains(pos)) {
                float sinTheta = Mathf.Sin(Data[i].rot);
                float cosTheta = Mathf.Cos(Data[i].rot);

                //Copy a single tree
                for(int j = 0;j < numVerticesPerModel;j ++) {
                    Vector3 transformedPoint = new Vector3(
                        OldVertices[j].y * cosTheta - OldVertices[j].x * sinTheta, 
                        OldVertices[j].z, 
                        OldVertices[j].y * sinTheta + OldVertices[j].x * cosTheta
                    ) * 0.25f;

                    Vertices[t * numVerticesPerModel + j] = transformedPoint + pos;
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

        Mesh.Clear();
        Mesh.indexFormat = IndexFormat.UInt32;
        Mesh.vertices = Vertices;
        Mesh.uv = UVs;
        Mesh.triangles = Triangles;

        watch.Stop();
        // Debug.Log($"Mesh Set Execution Time: {watch.ElapsedMilliseconds} ms");

        watch = new System.Diagnostics.Stopwatch();  
        watch.Start();


        Mesh.RecalculateNormals();

        watch.Stop();
        // Debug.Log($"Mesh Normals Execution Time: {watch.ElapsedMilliseconds} ms");
    }
}