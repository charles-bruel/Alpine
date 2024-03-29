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

// Gives a pool of a type of game object
public class Pool<T> where T : IPoolable {
    public T Template;

    private List<T> Backing = new List<T>();
    private int Index = -1;

    public T this[int key] {
        get { 
            if(key < 0) {
                throw new IndexOutOfRangeException();
            }
            if(key > Index) {
                throw new IndexOutOfRangeException();
            }
            return Backing[key];
        }
        set {
            if(key < 0) {
                throw new IndexOutOfRangeException();
            }
            if(key > Index) {
                throw new IndexOutOfRangeException();
            }
            Backing[key] = value;
        }
    }

    public int Count {
        get => Index + 1;
    }

    public T Instantiate() {
        Index++;
        if(Index == Backing.Count) {
            T temp = (T) Template.Clone();
            Backing.Add(temp);
        } else {
            Backing[Index].Enable();
        }
        return Backing[Index];
    }

    public void Reset() {
        if(Backing.Count == 0) return;
        for(int i = 0;i <= Index;i ++) {
            Backing[i].Disable();
        }
        Index = -1;
    }

    public void Finish() {
        for(int i = Index + 1;i < Backing.Count;i ++) {
            Backing[i].Destroy();
        }
    }
}