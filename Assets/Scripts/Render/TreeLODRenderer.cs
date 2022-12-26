using UnityEngine;
using System.Collections;
using System;

//This is a pretty dumb class and relies on something else to drive it
public class TreeLODRenderer : MonoBehaviour
{
    public Mesh instanceMesh;
    public Material InstanceMaterial;
    public int subMeshIndex = 0;
    public Params Parameters;
    public ComputeShader CullingShader;
    public uint TargetType;

    private ComputeBuffer dataBuffer;
    private ComputeBuffer dataBufferCulled;
    private ComputeBuffer paramBuffer;
    private ComputeBuffer argsBuffer;
    private ComputeBuffer cullShaderArgsBuffer;
    private uint[] args = new uint[5] { 0, 0, 0, 0, 0 };

    [NonSerialized]
    public Bounds Bounds;

    void Start()
    {
        UpdateBuffers(new TreePos[0]);
    }

    void Update() {
        Draw(Camera.main);
    }

    public void Draw(Camera camera) {
        //Render
        dataBufferCulled.SetCounterValue(0U);
        cullShaderArgsBuffer.SetData(new uint[] { (uint) dataBuffer.count, TargetType });
        int kernelIndex = CullingShader.FindKernel("CullShader");
        CullingShader.SetBuffer(kernelIndex, "input", dataBuffer);
        CullingShader.SetBuffer(kernelIndex, "args", cullShaderArgsBuffer);
        CullingShader.SetBuffer(kernelIndex, "output", dataBufferCulled);
        CullingShader.SetMatrix("camera", camera.projectionMatrix * camera.worldToCameraMatrix);
        CullingShader.Dispatch(kernelIndex, dataBuffer.count / 64 + 1, 1, 1);
        ComputeBuffer.CopyCount(dataBufferCulled, argsBuffer, 4);
        InstanceMaterial.SetBuffer("dataBuffer", dataBufferCulled);
        InstanceMaterial.SetBuffer("paramBuffer", paramBuffer);
        Graphics.DrawMeshInstancedIndirect(instanceMesh, subMeshIndex, InstanceMaterial, Bounds, argsBuffer);
    }

    public void UpdateBuffers(TreePos[] data) {
        if(data.Length == 0) {
            this.enabled = false;
            return;
        } else {
            this.enabled = true;
        }

        //Unsafe to allow use of sizeof()
        //That's safer than using a magic number
        unsafe {
            // Ensure submesh index is in range
            if (instanceMesh != null)
                subMeshIndex = Mathf.Clamp(subMeshIndex, 0, instanceMesh.subMeshCount - 1);

            // Positions
            if (dataBuffer != null)
                dataBuffer.Release();
            if (dataBufferCulled != null)
                dataBufferCulled.Release();
            dataBuffer = new ComputeBuffer(data.Length, sizeof(TreePos));
            dataBuffer.SetData(data);
            dataBufferCulled = new ComputeBuffer(data.Length, sizeof(TreePos), ComputeBufferType.Append);
            paramBuffer.SetData(new Params[] { Parameters });

            // Indirect args
            if (instanceMesh != null)
            {
                args[0] = (uint)instanceMesh.GetIndexCount(subMeshIndex);
                args[1] = (uint)data.Length;
                args[2] = (uint)instanceMesh.GetIndexStart(subMeshIndex);
                args[3] = (uint)instanceMesh.GetBaseVertex(subMeshIndex);
            }
            else
            {
                args[0] = args[1] = args[2] = args[3] = 0;
            }

            argsBuffer.SetData(args);
        }
    }

    void OnDisable() {
        if (dataBuffer != null)
            dataBuffer.Release();
        dataBuffer = null;

        if (dataBufferCulled != null)
            dataBufferCulled.Release();
        dataBufferCulled = null;

        if (argsBuffer != null)
            argsBuffer.Release();
        argsBuffer = null;

        if (cullShaderArgsBuffer != null)
            cullShaderArgsBuffer.Release();
        cullShaderArgsBuffer = null;

        if (paramBuffer != null)
            paramBuffer.Release();
        paramBuffer = null;
    }

    void OnEnable() {
        unsafe {
            argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
            cullShaderArgsBuffer = new ComputeBuffer(2, sizeof(uint), ComputeBufferType.IndirectArguments);
            paramBuffer = new ComputeBuffer(1, sizeof(Params));
        }
    }

    [System.Serializable]
    public struct Params {
        public float SnowMultiplier;
    }
}