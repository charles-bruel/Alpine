using UnityEngine;
[System.Serializable]
public class AlpinePolygonSource {
    //Note: Polygons on level 0 do not recieve mouse events
    public Transform ParentElement;
    public uint Level;
    public Vector2[] Points;
    public bool ArbitrarilyEditable;
    public PolygonFlags Flags;
    public float Height;
}