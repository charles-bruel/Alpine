using UnityEngine;
using System;
using System.Collections.Generic;

public class SnowLevelBuffer {
    public static readonly int Size = 256;
    public List<float> Data = new List<float>(Size);
    public ComputeBuffer Buffer;

    public SnowLevelBuffer() {
        //Test data
        for(int i = 0;i < Size;i ++) {
            Data.Add(0);
        }

        //sizeof() is unsafe
        unsafe {
            Buffer = new ComputeBuffer(Size, sizeof(float));
        }

        SendBufferUpdate();
    }

    public void SendBufferUpdate() {
        Buffer.SetData(Data);
    }

    public void Dispose() {
        if(Buffer != null)
            Buffer.Dispose();
    }

    public void Affect(int baseIndex, float quantity, bool clamp = true) {
        for(int i = baseIndex;i < Size;i ++) {
            Data[i] += quantity;
            if(clamp) {
                if(Data[i] < 0) Data[i] = 0;
                if(Data[i] > 1) Data[i] = 1;
            }
        }
    }

}