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