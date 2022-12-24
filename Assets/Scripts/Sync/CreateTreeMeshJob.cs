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

        float[] Data = TerrainManager.Instance.TreesData;

        //Copy
        for(int i = 0, t = 0;i < Data.Length;i += PlaceTreesJob.FloatsPerTree) {
            Vector3 pos = new Vector3(Data[i + 0], Data[i + 1], Data[i + 2]);
            if(Bounds.Contains(pos)) {
                //Copy a single tree
                for(int j = 0;j < numVerticesPerModel;j ++) {
                    Vector3 transformedPoint = new Vector3(OldVertices[j].y, OldVertices[j].z, OldVertices[j].x) * 0.25f;

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
        Mesh.Clear();
        Mesh.indexFormat = IndexFormat.UInt32;
        Mesh.vertices = Vertices;
        Mesh.uv = UVs;
        Mesh.triangles = Triangles;
        Mesh.RecalculateNormals();
    }
}