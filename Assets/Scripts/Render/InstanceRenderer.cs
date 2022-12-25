using UnityEngine;
using System.Collections;

// Based on the unity example in
// https://docs.unity3d.com/ScriptReference/Graphics.DrawMeshInstancedIndirect.html

// Not designed to be used by itself, it's supposed to be a base or reference
public class InstanceRenderer : MonoBehaviour
{
    public int instanceCount = 100000;
    public Mesh instanceMesh;
    public Material instanceMaterial;
    public int subMeshIndex = 0;

    private int cachedInstanceCount = -1;
    private int cachedSubMeshIndex = -1;
    private ComputeBuffer dataBuffer;
    private ComputeBuffer argsBuffer;
    private uint[] args = new uint[5] { 0, 0, 0, 0, 0 };

    void Start()
    {
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        UpdateBuffers();
    }

    void Update()
    {
        // Update starting position buffer
        if (cachedInstanceCount != instanceCount || cachedSubMeshIndex != subMeshIndex)
            UpdateBuffers();

        // Render
        Graphics.DrawMeshInstancedIndirect(instanceMesh, subMeshIndex, instanceMaterial, new Bounds(Vector3.zero, new Vector3(100.0f, 100.0f, 100.0f)), argsBuffer);
    }

    void UpdateBuffers()
    {
        unsafe {
            // Ensure submesh index is in range
            if (instanceMesh != null)
                subMeshIndex = Mathf.Clamp(subMeshIndex, 0, instanceMesh.subMeshCount - 1);

            // Positions
            if (dataBuffer != null)
                dataBuffer.Release();
            dataBuffer = new ComputeBuffer(instanceCount, 24);
            TreePos[] positions = new TreePos[instanceCount];
            for (int i = 0; i < instanceCount; i++)
            {
                float angle = Random.Range(0.0f, Mathf.PI * 2.0f);
                float rotation = Random.Range(0.0f, Mathf.PI * 2.0f);
                float distance = Random.Range(20.0f, 100.0f);
                float height = Random.Range(-2.0f, 2.0f);
                float size = Random.Range(0.05f, 0.25f);
                Vector3 pos = new Vector3(Mathf.Sin(angle) * distance, height, Mathf.Cos(angle) * distance);
                positions[i].pos = pos;
                positions[i].scale = size;
                positions[i].rot = rotation;
            }
            dataBuffer.SetData(positions);
            instanceMaterial.SetBuffer("positionBuffer", dataBuffer);

            // Indirect args
            if (instanceMesh != null)
            {
                args[0] = (uint)instanceMesh.GetIndexCount(subMeshIndex);
                args[1] = (uint)instanceCount;
                args[2] = (uint)instanceMesh.GetIndexStart(subMeshIndex);
                args[3] = (uint)instanceMesh.GetBaseVertex(subMeshIndex);
            }
            else
            {
                args[0] = args[1] = args[2] = args[3] = 0;
            }
            argsBuffer.SetData(args);

            cachedInstanceCount = instanceCount;
            cachedSubMeshIndex = subMeshIndex;
        }
    }

    void OnDisable()
    {
        if (dataBuffer != null)
            dataBuffer.Release();
        dataBuffer = null;

        if (argsBuffer != null)
            argsBuffer.Release();
        argsBuffer = null;
    }
}