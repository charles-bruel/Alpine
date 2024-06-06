//>============================================================================<
//
//    Alpine, Ski Resort Tycoon Game
//    Copyright (C) 2024  Charles Bruel
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see <https://www.gnu.org/licenses/>.
//>============================================================================<

using UnityEngine;
using System.Collections;
using System;

//This is a pretty dumb class and relies on something else to drive it
public class OverlayRenderer : MonoBehaviour {
    public Camera OverlayCamera;
    public Mesh instanceMesh;
    public Material InstanceMaterial;
    public int subMeshIndex = 0;
    public ComputeShader CullingShader;
    public uint TargetType;

    private ComputeBuffer dataBuffer;
    private ComputeBuffer dataBufferCulled;
    private ComputeBuffer argsBuffer;
    private ComputeBuffer cullShaderArgsBuffer;
    private uint[] args = new uint[5] { 0, 0, 0, 0, 0 };

    [NonSerialized]
    public Bounds Bounds;

    void Start() {
        UpdateBuffers(new Vector3[0]);
    }

    void Update() {
        if(StateController.Instance.Mode2D) Draw(OverlayCamera);
    }

    public void Draw(Camera camera) {
        // For some reason, this breaks for two frames when loading from the main menu
        if(dataBufferCulled == null) {
            return;
        }

        InstanceMaterial.SetMatrix("_Camera", camera.projectionMatrix * camera.worldToCameraMatrix);

        //Render
        dataBufferCulled.SetCounterValue(0U);
        cullShaderArgsBuffer.SetData(new uint[] { (uint) dataBuffer.count, 0 });
        int kernelIndex = CullingShader.FindKernel("CullShader");
        CullingShader.SetBuffer(kernelIndex, "input", dataBuffer);
        CullingShader.SetBuffer(kernelIndex, "args", cullShaderArgsBuffer);
        CullingShader.SetBuffer(kernelIndex, "output", dataBufferCulled);
        CullingShader.SetMatrix("camera", camera.projectionMatrix * camera.worldToCameraMatrix);
        CullingShader.Dispatch(kernelIndex, dataBuffer.count / 64 + 1, 1, 1);
        ComputeBuffer.CopyCount(dataBufferCulled, argsBuffer, 4);
        InstanceMaterial.SetBuffer("dataBuffer", dataBufferCulled);
        Graphics.DrawMeshInstancedIndirect(instanceMesh, subMeshIndex, InstanceMaterial, Bounds, argsBuffer, layer: 9, camera: camera);
    }

    public void UpdateBuffers(Vector3[] data) {
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
            dataBuffer = new ComputeBuffer(data.Length, sizeof(Vector3));
            dataBuffer.SetData(data);
            dataBufferCulled = new ComputeBuffer(data.Length, sizeof(Vector3), ComputeBufferType.Append);

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
    }

    void OnEnable() {
        unsafe {
            argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
            cullShaderArgsBuffer = new ComputeBuffer(2, sizeof(uint), ComputeBufferType.IndirectArguments);
        }
    }
}