using System;
using EPPZ.Geometry.Model;
using UnityEngine;

[System.Serializable]
public class AlpinePolygon {
    public Guid Guid;
    //Note: Polygons on level 0 do not recieve mouse events
    public uint Level;
    public Polygon Polygon;
    [NonSerialized]
    public MeshFilter Filter;
    [NonSerialized]
    public MeshRenderer Renderer;
    public Color Color;
    public bool ArbitrarilyEditable;
    public PolygonFlags Flags;
    public float Height;
    // Note: To be set by PolygonController ONLY
    public bool Selected;
    public bool Selectable = true;

    public virtual void OnSelected() {}

    public virtual void OnDeselected() {}

    public virtual void OnDestroy() {}
}