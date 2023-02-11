using System;
using System.Collections.Generic;
using ClipperLib;
using EPPZ.Geometry.AddOns;
using EPPZ.Geometry.Model;
using UnityEngine;
using UnityEngine.EventSystems;

[System.Serializable]
public class AlpinePolygon {
    public Guid Guid;
    //Note: Polygons on level 0 do not recieve mouse events
    public uint Level;
    public Polygon Polygon;
    public MeshFilter Filter;
    public MeshRenderer Renderer;
    public Color Color;
    public bool ArbitrarilyEditable;
    public PolygonFlags Flags;
    public float Height;
}