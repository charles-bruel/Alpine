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
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class Collections
{

    #region HollowList

    [Test]
    public void HollowListEmpty()
    {
        HollowList<int> hollowList = new HollowList<int>();

        Assert.IsFalse(hollowList.Contains(1));
        Assert.AreEqual(0, hollowList.Count);
    }

    [Test]
    public void HollowListAdd()
    {
        HollowList<int> hollowList = new HollowList<int>();

        hollowList.Add(1);

        Assert.IsTrue(hollowList.Contains(1));
        Assert.IsFalse(hollowList.Contains(2));
        Assert.AreEqual(1, hollowList.Count);
    }

    [Test]
    public void HollowListAddRemove()
    {
        HollowList<int> hollowList = new HollowList<int>();

        hollowList.Add(1);
        hollowList.Remove(1);

        Assert.IsFalse(hollowList.Contains(1));
        Assert.AreEqual(0, hollowList.Count);
    }

    [Test]
    public void HollowListEnumerator()
    {
        HollowList<int> hollowList = new HollowList<int>();

        hollowList.Add(1);
        
        var enumerator = hollowList.GetEnumerator();
        enumerator.MoveNext();

        Assert.AreEqual(1, enumerator.Current);

        Assert.IsFalse(enumerator.MoveNext());
    }

    [Test]
    public void HollowListAddEnumerator()
    {
        HollowList<int> hollowList = new HollowList<int>();

        hollowList.Add(1);
        hollowList.Add(2);
        hollowList.Add(3);

        var enumerator = hollowList.GetEnumerator();
        enumerator.MoveNext();
        Assert.AreEqual(1, enumerator.Current);
        enumerator.MoveNext();
        Assert.AreEqual(2, enumerator.Current);
        enumerator.MoveNext();
        Assert.AreEqual(3, enumerator.Current);
    }

    [Test]
    public void HollowListAddRemoveEnumerator()
    {
        HollowList<int> hollowList = new HollowList<int>();

        hollowList.Add(1);
        hollowList.Add(2);
        hollowList.Add(3);
        hollowList.Add(4);
        hollowList.Add(5);

        hollowList.Remove(3);

        var enumerator = hollowList.GetEnumerator();
        enumerator.MoveNext();
        Assert.AreEqual(1, enumerator.Current);
        enumerator.MoveNext();
        Assert.AreEqual(2, enumerator.Current);
        enumerator.MoveNext();
        Assert.AreEqual(4, enumerator.Current);
        enumerator.MoveNext();
        Assert.AreEqual(5, enumerator.Current);
    }

    [Test]
    public void HollowListFill()
    {
        HollowList<int> hollowList = new HollowList<int>();

        hollowList.Add(1);
        hollowList.Add(2);
        hollowList.Add(3);

        hollowList.Remove(2);

        hollowList.Add(4);
        hollowList.Add(5);

        var enumerator = hollowList.GetEnumerator();
        enumerator.MoveNext();
        Assert.AreEqual(1, enumerator.Current);
        enumerator.MoveNext();
        Assert.AreEqual(4, enumerator.Current);
        enumerator.MoveNext();
        Assert.AreEqual(3, enumerator.Current);
        enumerator.MoveNext();
        Assert.AreEqual(5, enumerator.Current);
    }

    #endregion

    #region GridArray

    [Test]
    public void GridArrayEmpty()
    {
        GridArray<GriddableDummy> GridArray = new GridArray<GriddableDummy>(1, 1);

        Assert.IsFalse(GridArray.Contains(new GriddableDummy(1)));
        Assert.AreEqual(0, GridArray.Count);
    }

    [Test]
    public void GridArrayAdd()
    {
        GridArray<GriddableDummy> GridArray = new GridArray<GriddableDummy>(1, 1);

        GridArray.Add(new GriddableDummy(1));

        Assert.IsTrue(GridArray.Contains(new GriddableDummy(1)));
        Assert.IsFalse(GridArray.Contains(new GriddableDummy(2)));
        Assert.AreEqual(1, GridArray.Count);
    }

    [Test]
    public void GridArrayAddRemove()
    {
        GridArray<GriddableDummy> GridArray = new GridArray<GriddableDummy>(1, 1);

        GridArray.Add(new GriddableDummy(1));
        GridArray.Remove(new GriddableDummy(1));

        Assert.IsFalse(GridArray.Contains(new GriddableDummy(1)));
        Assert.AreEqual(0, GridArray.Count);
    }

    [Test]
    public void GridArrayEnumerator()
    {
        GridArray<GriddableDummy> GridArray = new GridArray<GriddableDummy>(1, 1);

        GridArray.Add(new GriddableDummy(1));
        
        var enumerator = GridArray.GetEnumerator();
        enumerator.MoveNext();

        Assert.AreEqual(new GriddableDummy(1), enumerator.Current);

        Assert.IsFalse(enumerator.MoveNext());
    }

    [Test]
    public void GridArrayAddEnumerator()
    {
        GridArray<GriddableDummy> GridArray = new GridArray<GriddableDummy>(1, 1);

        GridArray.Add(new GriddableDummy(1));
        GridArray.Add(new GriddableDummy(2));
        GridArray.Add(new GriddableDummy(3));

        var enumerator = GridArray.GetEnumerator();
        enumerator.MoveNext();
        Assert.AreEqual(new GriddableDummy(1), enumerator.Current);
        enumerator.MoveNext();
        Assert.AreEqual(new GriddableDummy(2), enumerator.Current);
        enumerator.MoveNext();
        Assert.AreEqual(new GriddableDummy(3), enumerator.Current);
    }

    [Test]
    public void GridArrayAddRemoveEnumerator()
    {
        GridArray<GriddableDummy> GridArray = new GridArray<GriddableDummy>(1, 1);

        GridArray.Add(new GriddableDummy(1));
        GridArray.Add(new GriddableDummy(2));
        GridArray.Add(new GriddableDummy(3));
        GridArray.Add(new GriddableDummy(4));
        GridArray.Add(new GriddableDummy(5));

        GridArray.Remove(new GriddableDummy(3));

        var enumerator = GridArray.GetEnumerator();
        enumerator.MoveNext();
        Assert.AreEqual(new GriddableDummy(1), enumerator.Current);
        enumerator.MoveNext();
        Assert.AreEqual(new GriddableDummy(2), enumerator.Current);
        enumerator.MoveNext();
        Assert.AreEqual(new GriddableDummy(4), enumerator.Current);
        enumerator.MoveNext();
        Assert.AreEqual(new GriddableDummy(5), enumerator.Current);
    }

    [Test]
    public void GridArrayFill()
    {
        GridArray<GriddableDummy> GridArray = new GridArray<GriddableDummy>(1, 1);

        GridArray.Add(new GriddableDummy(1));
        GridArray.Add(new GriddableDummy(2));
        GridArray.Add(new GriddableDummy(3));

        GridArray.Remove(new GriddableDummy(2));

        GridArray.Add(new GriddableDummy(4));
        GridArray.Add(new GriddableDummy(5));

        var enumerator = GridArray.GetEnumerator();
        enumerator.MoveNext();
        Assert.AreEqual(new GriddableDummy(1), enumerator.Current);
        enumerator.MoveNext();
        Assert.AreEqual(new GriddableDummy(4), enumerator.Current);
        enumerator.MoveNext();
        Assert.AreEqual(new GriddableDummy(3), enumerator.Current);
        enumerator.MoveNext();
        Assert.AreEqual(new GriddableDummy(5), enumerator.Current);
    }

    [Test]
    public void GridArrayGriddedEnum()
    {
        GridArray<GriddableDummy> GridArray = new GridArray<GriddableDummy>(2, 2);

        GridArray.Add(new GriddableDummy(0, 0, 1));
        GridArray.Add(new GriddableDummy(0, 1, 2));
        GridArray.Add(new GriddableDummy(1, 0, 3));
        GridArray.Add(new GriddableDummy(1, 1, 4));

        var enumerator = GridArray.GetEnumerator(0, 1);
        enumerator.MoveNext();
        Assert.AreEqual(new GriddableDummy(0, 1, 2), enumerator.Current);
        Assert.IsFalse(enumerator.MoveNext());
    }

    #endregion

    internal class GriddableDummy : IGridable
    {
        public byte x, y;
        public int value;

        public GriddableDummy(byte x, byte y, int value) {
            this.x = x;
            this.y = y;
            this.value = value;
        }

        public GriddableDummy(int value) {
            this.x = 0;
            this.y = 0;
            this.value = value;
        }

        public byte GetGridX() {
            return x;
        }

        public byte GetGridY() {
            return y;
        }

        public override bool Equals(object obj)
        {
            return obj is GriddableDummy dummy &&
                   value == dummy.value;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(value);
        }
    }
}
