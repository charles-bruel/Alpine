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
using EPPZ.Geometry.Model;
using UnityEngine;

[System.Serializable]
public class AlpinePolygon {
    public Guid Guid;
    //Note: Polygons on level 0 do not recieve mouse events
    public uint Level;
    public Polygon Polygon;
    // [NonSerialized]
    public MeshFilter Filter;
    // [NonSerialized]
    public MeshRenderer Renderer;
    public Color Color;
    public bool ArbitrarilyEditable;
    public PolygonFlags Flags;
    public IUISelectable Owner;
    public float Height;
    // Note: To be set by PolygonController ONLY
    public bool Selected;
    public bool Selectable = true;

    public virtual void OnSelected() {
        Owner.OnSelected();
    }

    public virtual void OnDeselected() {
        Owner.OnDeselected();
    }

    public virtual void OnDestroy() {}
}