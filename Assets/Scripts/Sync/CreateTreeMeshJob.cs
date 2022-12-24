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

    public void Run() {
        //Create data structures
        int numVerticesPerModel1 = OldVertices1.Length;
        int numTrianglesPerModel1 = OldTriangles1.Length;
        int numVerticesPerModel2 = OldVertices2.Length;
        int numTrianglesPerModel2 = OldTriangles2.Length;
        Vertices = new Vector3[numVerticesPerModel1 * NumTrees1 + numVerticesPerModel2 * NumTrees2];
        Normals = new Vector3[numVerticesPerModel1 * NumTrees1 + numVerticesPerModel2 * NumTrees2];
        UVs = new Vector2[numVerticesPerModel1 * NumTrees1 + numVerticesPerModel2 * NumTrees2];
        Triangles = new int[numTrianglesPerModel1 * NumTrees1 + numTrianglesPerModel2 * NumTrees2];

        //Copy
        Copy(
            1, 0, 0,
            OldVertices1, OldNormals1, OldUVs1, OldTriangles1,
            numVerticesPerModel1, numTrianglesPerModel1
        );

        Copy(
            2, numVerticesPerModel1 * NumTrees1, numTrianglesPerModel1 * NumTrees1,
            OldVertices2, OldNormals2, OldUVs2, OldTriangles2,
            numVerticesPerModel2, numTrianglesPerModel2
        );

        lock(ASyncJobManager.completedJobsLock) {
        	ASyncJobManager.Instance.completedJobs.Enqueue(this);
		}
    }

    private void Copy(
        byte TypeToLookFor, int verticesBaseIndex, int trianglesBaseIndex,
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
                    ) * scaleMul;
                    Vertices[t * numVerticesPerModel + j + verticesBaseIndex] = transformedVertex + pos;

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