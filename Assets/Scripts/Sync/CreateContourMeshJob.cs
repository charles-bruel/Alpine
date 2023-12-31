using Unity.Collections;
using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.Rendering;

public class RecreateContourMeshJob : Job
{
    public Bounds Bounds;
    public TerrainTile Tile;
    public byte PosX;
    public byte PosY;
    public Mesh MeshTarget;
    public ContourLayersDefinition LayersDefinition;
    private NativeArray<Vector3> Vertices;
    private NativeArray<int> Points;
    private Mesh.MeshDataArray OutputMeshData;
    private ContourDefinition Contours;

    public void Initialize() {
        LoadingScreen.INSTANCE.LoadingTasks++;

        int numPoints = 0;
        Contours = ContoursUtils.GetContours(LayersDefinition, Tile.HeightData, Bounds);

        for(int i = 0;i < Contours.MajorPoints.Length;i ++) {
            numPoints += Contours.MajorPoints[i].Count;
        }

        for(int i = 0;i < Contours.MinorPoints.Length;i ++) {
            numPoints += Contours.MinorPoints[i].Count;
        }

        if(numPoints == 0) return;

        OutputMeshData = Mesh.AllocateWritableMeshData(1);
        Mesh.MeshData outputMesh = OutputMeshData[0];
        outputMesh.SetIndexBufferParams(numPoints, IndexFormat.UInt32);
        outputMesh.SetVertexBufferParams(numPoints,
            new VertexAttributeDescriptor(VertexAttribute.Position)
        );

        Vertices = outputMesh.GetVertexData<Vector3>(stream:0);
        Points = outputMesh.GetIndexData<int>();
    }

    public void Run() {
        if(Vertices.Length == 0) return;

        int index = 0;
        for(int l = 0;l < Contours.Layers.Major.Length;l ++) {
			var array = Contours.MajorPoints[l];
			for(int i = 0;i < array.Count - 1;i += 2) {
                Vector3 a = new Vector3(array[i].x, Contours.Layers.Major[l] + 0.1f, array[i].y);
                Vector3 b = new Vector3(array[i+1].x, Contours.Layers.Major[l] + 0.1f, array[i+1].y);

                Vertices[index + 0] = a;
                Vertices[index + 1] = b;

                Points[index + 0] = index + 0;
                Points[index + 1] = index + 1;

                index += 2;
			}
		}
        for(int l = 0;l < Contours.Layers.Minor.Length;l ++) {
			var array = Contours.MinorPoints[l];
			for(int i = 0;i < array.Count - 1;i += 2) {
                Vector3 a = new Vector3(array[i].x, Contours.Layers.Minor[l] + 0.1f, array[i].y);
                Vector3 b = new Vector3(array[i+1].x, Contours.Layers.Minor[l] + 0.1f, array[i+1].y);

                Vertices[index + 0] = a;
                Vertices[index + 1] = b;

                Points[index + 0] = index + 0;
                Points[index + 1] = index + 1;

                index += 2;
			}
		}

        lock(ASyncJobManager.completedJobsLock) {
        	ASyncJobManager.Instance.completedJobs.Enqueue(this);
		}
    }

    public override void Complete()
    {
        SubMeshDescriptor subMesh = new SubMeshDescriptor(0, Points.Length, MeshTopology.Lines);
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