using UnityEngine;
using System;
using System.Collections.Generic;

// Gives a pool of a type of game object
public class Pool {
    public GameObject Template;

    private List<GameObject> Backing = new List<GameObject>();
    private int Index = -1;

    public GameObject this[int key] {
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

    public GameObject Instantiate() {
        Index++;
        if(Index == Backing.Count) {
            GameObject temp = GameObject.Instantiate(Template);
            Backing.Add(temp);
        } else {
            Backing[Index].SetActive(true);
        }
        return Backing[Index];
    }

    public void Reset() {
        if(Backing.Count == 0) return;
        for(int i = 0;i <= Index;i ++) {
            Backing[i].SetActive(false);
        }
        Index = -1;
    }

    public void Finish() {
        for(int i = Index + 1;i < Backing.Count;i ++) {
            Backing[i].SetActive(false);
        }
    }
}