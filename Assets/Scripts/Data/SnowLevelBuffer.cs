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
            // Data.Add(i / (float) Size);
            if(i < 64 || i > 128) {
                Data.Add(0);
            } else {
                Data.Add(1);
            }
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

}