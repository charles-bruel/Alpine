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
    private int Version;

    public int Count => Backing.Count;

    public bool IsReadOnly => false;

    public GridArray(int capacity, byte GridWidth, byte GridHeight) {
        this.Backing = new HollowList<T>(capacity);
        this.GridWidth = GridWidth;
        this.GridHeight = GridHeight;
        this.IndicesReference = new HollowList<int>[GridWidth, GridHeight];

        for(byte x = 0;x < GridWidth;x ++) {
            for(byte y = 0;y < GridHeight;y ++) {
                IndicesReference[x, y] = new HollowList<int>();
            }
        }

        this.Version = 0;
    }

    public GridArray(byte GridWidth, byte GridHeight) : this(0, GridWidth, GridHeight) {}

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
        Version++;
    }

    public int GetCountInCell(byte x, byte y) {
        return IndicesReference[x, y].Count;
    }

    public void Clear() {
        Backing.Clear();
        for(byte x = 0;x < GridWidth;x ++) {
            for(byte y = 0;y < GridHeight;y ++) {
                IndicesReference[x, y].Clear();
            }
        }
        Version++;
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
        Version++;
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

    public bool RemoveAt(int i) {
        T value = Backing[i];
        IndicesReference[value.GetGridX(), value.GetGridY()].Remove(i);
        Backing.RemoveAt(i);
        return true;
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return Backing.GetEnumerator();
    }

    public IWriteableEnumerator<T> GetMutEnumerator() {
        return Backing.GetMutEnumerator();
    }

    public IWriteableEnumerator<T> GetEnumerator(byte x, byte y) {
        return new GridCellEnumerator(this, x, y);
    }

    public IEnumerator<int> GetIndexEnumerator(byte x, byte y) {
        return IndicesReference[x, y].GetEnumerator();
    }

    internal struct GridCellEnumerator : IWriteableEnumerator<T> {
        private GridArray<T> array;
        private int version;
        private IEnumerator<int> cellIndex;
        private byte x, y;

        public GridCellEnumerator(GridArray<T> array, byte x, byte y) {
            this.array = array;
            this.x = x;
            this.y = y;
            this.version = array.Version;
            cellIndex = array.IndicesReference[x, y].GetEnumerator();
        }

        public T Current {
            get {
                if(this.version != array.Version) {
                    throw new InvalidOperationException("Tried to use enumeration after collection change");
                }
                return array[cellIndex.Current];
            }
        }

        public T CurrentMut { 
            get {
                if(this.version != array.Version) {
                    throw new InvalidOperationException("Tried to use enumeration after collection change");
                }
                return array[cellIndex.Current];
            }
            set {
                if(this.version != array.Version) {
                    throw new InvalidOperationException("Tried to use enumeration after collection change");
                }
                array[cellIndex.Current] = value;
            }
        }

        object IEnumerator.Current {
            get {
                if(this.version != array.Version) {
                    throw new InvalidOperationException("Tried to use enumeration after collection change");
                }
                return array[cellIndex.Current];
            }
        }

        public void Dispose() {}

        public bool MoveNext()
        {
            if(this.version != array.Version) {
                throw new InvalidOperationException("Tried to use enumeration after collection change");
            }
            return cellIndex.MoveNext();
        }

        public void Reset()
        {
            cellIndex.Reset();
        }
    }
}