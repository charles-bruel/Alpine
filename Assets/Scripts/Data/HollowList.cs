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

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is a list where items are moved upon removing an item, increasing the speed of removal to O(N).
/// This is also useful when having changing indices is undesirable. As long as an item itself is not removed,
/// an index referencing is garunteed to remain valid.
/// </summary>

//TODO: Add bounds checking
public class HollowList<T> : ICollection<T>
{
    private List<T> Backing;
    private SortedSet<int> Holes;
    private int TrueCount;
    private int Version;
    private int HolesVersion;

    public int Count => TrueCount;

    public bool IsReadOnly => false;

    public HollowList(int capacity) {
        this.Backing = new List<T>(capacity);
        this.Holes = new SortedSet<int>();
        this.TrueCount = 0;
        this.Version = 0;
    }

    public HollowList() : this(0) {}

    public T this[int index]
    {
        get => Backing[index];
        set => Backing[index] = value;
    }

    public int AddGetIndex(T item) {
        TrueCount++;
        Version++;

        if(Holes.Count > 0) {
            int index = Holes.Min;
            Holes.Remove(Holes.Min);
            Backing[index] = item;
            return index;
        }

        Backing.Add(item);
        return TrueCount - 1;
    }

    public void Add(T item) {
        AddGetIndex(item);
    }

    public void Clear() {
        Version++;
        Backing.Clear();
        Holes.Clear();
        TrueCount = 0;
    }

    public bool Contains(T item) {
        foreach(T x in this) {
            if(EqualityComparer<T>.Default.Equals(x, item)) {
                return true;
            }
        }
        return false;
    }

    public void CopyTo(T[] array, int arrayIndex) {
        throw new NotImplementedException();
    }

    public IEnumerator<T> GetEnumerator() {
        return new Enumerator(this);
    }

    public IWriteableEnumerator<T> GetMutEnumerator() {
        return new Enumerator(this);
    }

    public bool Remove(T item) {
        return RemoveGetIndex(item) != -1;
    }

    public int RemoveGetIndex(T item) {
        for(int i = 0;i < Backing.Count;i ++) {
            if(EqualityComparer<T>.Default.Equals(Backing[i], item)) {
                if(!Holes.Contains(i)) {
                    HolesVersion++;
                    Holes.Add(i);
                    TrueCount--;
                    return i;
                }
            }
        }
        return -1;
    }

    public bool RemoveAt(int index) {
        HolesVersion++;
        Holes.Add(index);
        TrueCount--;
        return true;
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return new Enumerator(this);
    }

    internal struct Enumerator : IWriteableEnumerator<T>
    {
        private IEnumerator<int> holes;
        private int version;
        private int holesVersion;
        private HollowList<T> list;
        private int currentIndex;
        private bool holesDone;

        internal Enumerator(HollowList<T> list) {
            this.holes = list.Holes.GetEnumerator();
            this.holes.MoveNext();
            this.list = list;
            this.version = list.Version;
            this.holesVersion = list.HolesVersion;
            currentIndex = -1;
            holesDone = list.Holes.Count == 0;
        }

        public T Current {
            get {
                if(this.version != list.Version) {
                    throw new InvalidOperationException("Tried to use enumeration after collection change");
                }
                return list[currentIndex];
            }
        }

        public T CurrentMut {
            get {
                if(this.version != list.Version) {
                    throw new InvalidOperationException("Tried to use enumeration after collection change");
                }
                return list[currentIndex];
            }
            set {
                if(this.version != list.Version) {
                    throw new InvalidOperationException("Tried to use enumeration after collection change");
                }
                list[currentIndex] = value;
            }
        }

        object IEnumerator.Current {
            get {
                if(this.version != list.Version) {
                    throw new InvalidOperationException("Tried to use enumeration after collection change");
                }
                return list[currentIndex];
            }
        }

        public void Dispose() {}

        public bool MoveNext() {
            if(this.version != list.Version) {
                throw new InvalidOperationException("Tried to use enumeration after collection change");
            }
            if(this.holesVersion != list.HolesVersion) {
                this.holesVersion = list.HolesVersion;
                // We can recover from this, and it's important to do so
                // It's fine to add a new holes; there is nothing we can
                // do about the previously returned values and the we can
                // skip any new values; *however*, we must rest the holes
                // enumerator
                holes = list.Holes.GetEnumerator();
                // We must also skip ahead so that holes.Current is in the
                // future and reset holesDone
                holesDone = list.Holes.Count == 0;
                try {
                    while(holes.Current < currentIndex) {
                        holesDone = !holes.MoveNext();
                    }
                } catch(InvalidOperationException) {
                    return MoveNext();
                }
            }
            currentIndex++;
            while(!holesDone && holes.Current == currentIndex) {
                currentIndex ++;
                holesDone = !holes.MoveNext();
            }
            return currentIndex < list.Backing.Count;
        }

        public void Reset() {
            this.currentIndex = -1;
            this.holes = list.Holes.GetEnumerator();
            this.holes.MoveNext();
        }
    }
}