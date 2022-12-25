using UnityEngine;
using System.Collections;

//This is a pretty dumb class and relies on something else to drive it
public class TreeLODRenderer : MonoBehaviour
{
    public Mesh instanceMesh;
    public Material instanceMaterial;
    public int subMeshIndex = 0;
    public Params Parameters;

    private ComputeBuffer dataBuffer;
    private ComputeBuffer paramBuffer;
    private ComputeBuffer argsBuffer;
    private uint[] args = new uint[5] { 0, 0, 0, 0, 0 };

    void Start()
    {
        UpdateBuffers(new TreePos[0]);
    }

    void Update() {
        Graphics.DrawMeshInstancedIndirect(instanceMesh, subMeshIndex, instanceMaterial, new Bounds(Vector3.zero, new Vector3(100.0f, 100.0f, 100.0f)), argsBuffer);
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
            dataBuffer = new ComputeBuffer(data.Length, sizeof(TreePos));
            dataBuffer.SetData(data);
            instanceMaterial.SetBuffer("dataBuffer", dataBuffer);
            paramBuffer.SetData(new Params[] { Parameters });
            instanceMaterial.SetBuffer("paramBuffer", paramBuffer);

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

        if (argsBuffer != null)
            argsBuffer.Release();
        argsBuffer = null;

        if (paramBuffer != null)
            paramBuffer.Release();
        paramBuffer = null;
    }

    void OnEnable() {
        unsafe {
            argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
            paramBuffer = new ComputeBuffer(1, sizeof(Params));
        }
    }

    [System.Serializable]
    public struct Params {
        public float SnowMultiplier;
    }
}