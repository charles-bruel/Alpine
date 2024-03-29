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