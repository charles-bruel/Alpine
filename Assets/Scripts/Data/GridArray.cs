using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// This is a list where each element is placed into a grid cell for easier spatial lookup.
/// </summary>

//TODO: Add bounds checking
public class GridArray<T> : ICollection<T> where T : IGridable {

    private HollowList<T> Backing;
    public byte GridWidth { get; private set; }
    public byte GridHeight { get; private set; }
    private HollowList<int>[,] IndicesReference;

    public int Count => Backing.Count;

    public bool IsReadOnly => false;

    public GridArray(byte GridWidth, byte GridHeight) {
        this.Backing = new HollowList<T>();
        this.GridWidth = GridWidth;
        this.GridHeight = GridHeight;
        this.IndicesReference = new HollowList<int>[GridWidth, GridHeight];

        for(byte x = 0;x < GridWidth;x ++) {
            for(byte y = 0;y < GridHeight;y ++) {
                IndicesReference[x, y] = new HollowList<int>();
            }
        }
    }

    public T this[int index]
    {
        get => Backing[index];
        set => Backing[index] = value;
    }

    public void Add(T item) {
        int idx = Backing.AddGetIndex(item);
        byte x = item.GetGridX();
        byte y = item.GetGridY();
        IndicesReference[x, y].Add(idx);
    }

    public void Clear() {
        Backing.Clear();
        for(byte x = 0;x < GridWidth;x ++) {
            for(byte y = 0;y < GridHeight;y ++) {
                IndicesReference[x, y].Clear();
            }
        }
    }

    public bool Contains(T item) {
        byte x = item.GetGridX();
        byte y = item.GetGridY();
        foreach(int i in IndicesReference[x, y]) {
            if(EqualityComparer<T>.Default.Equals(Backing[i], item)) {
                return true;
            }
        }
        return false;
    }

    public void CopyTo(T[] array, int arrayIndex) {
        throw new NotImplementedException();
    }

    public IEnumerator<T> GetEnumerator() {
        //We don't have to make our own enumerator to keep track of changes
        //because any change to this will also affect the backing
        return Backing.GetEnumerator();
    }

    public bool Remove(T item) {
        byte x = item.GetGridX();
        byte y = item.GetGridY();
        foreach(int i in IndicesReference[x, y]) {
            if(EqualityComparer<T>.Default.Equals(Backing[i], item)) {
                Backing.RemoveAt(i);
                IndicesReference[x, y].Remove(i);
                return true;
            }
        }
        return false;
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return Backing.GetEnumerator();
    }
}