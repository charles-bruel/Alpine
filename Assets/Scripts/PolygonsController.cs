using System;
using System.Collections.Generic;
using EPPZ.Geometry.AddOns;
using EPPZ.Geometry.Model;
using UnityEngine;
using UnityEngine.Rendering;

public class PolygonsController : MonoBehaviour
{
    public TriangulatorType Triangulator = TriangulatorType.Dwyer;
    public Material Material;
    public List<AlpinePolygon> PolygonObjects;

    public void RegisterPolygon(AlpinePolygon polygon) {
        PolygonObjects.Add(polygon);

        Remesh();
    }

    public void ReregisterPolygon(Guid guid) {
        for(int i = 0;i < PolygonObjects.Count;i ++) {
            if(PolygonObjects[i].guid == guid) {
                PolygonObjects.RemoveAt(i);
                i--;
            }
        }

        Remesh();
    }

    void Start() {
        AlpinePolygon poly = new AlpinePolygon();
        poly.guid = new Guid();
        poly.Level = 0;
        poly.Polygon = Polygon.PolygonWithPoints(new Vector2[] {
            new Vector2(-1200, -1200),
            new Vector2(-1200,  1200),
            new Vector2( 1200,  1200),
            new Vector2( 1200, -1200)
        });
        poly.Color = Color.yellow;

        RegisterPolygon(poly);
    }

    private void Remesh() {
        for(int i = 0;i < PolygonObjects.Count;i ++) {
            AlpinePolygon poly = PolygonObjects[i];

            if(poly.Renderer == null) {
                poly.Renderer = CreateMeshRenderer(poly);
            }

            poly.Renderer.mesh = poly.Polygon.Mesh(poly.Color, Triangulator);
        }
    }

    private MeshFilter CreateMeshRenderer(AlpinePolygon poly) {
        GameObject meshObject = new GameObject(poly.guid.ToString());
        meshObject.transform.parent = transform;
        meshObject.transform.position = Vector3.zero;
        meshObject.transform.localEulerAngles = new Vector3();
        meshObject.layer = gameObject.layer;

        MeshRenderer meshRenderer = meshObject.AddComponent<MeshRenderer>();
        meshRenderer.material = Material;

        MeshFilter meshFilter = meshObject.AddComponent<MeshFilter>();
        return meshFilter;

    }

    [System.Serializable]
    public struct AlpinePolygon {
        public Guid guid;
        public uint Level;
        public Polygon Polygon;
        public MeshFilter Renderer;
        public Color Color;
    }
}
