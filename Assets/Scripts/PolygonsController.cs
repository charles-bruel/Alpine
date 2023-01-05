using System;
using System.Collections.Generic;
using ClipperLib;
using EPPZ.Geometry.AddOns;
using EPPZ.Geometry.Model;
using UnityEngine;
using UnityEngine.Rendering;

using Path = System.Collections.Generic.List<ClipperLib.IntPoint>;

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


        poly = new AlpinePolygon();
        poly.guid = new Guid();
        poly.Level = 1;
        poly.Polygon = Polygon.PolygonWithPoints(new Vector2[] {
            new Vector2(-33, -45),
            new Vector2(-23,  32),
            new Vector2( 75,  23),
            new Vector2( 65, -76)
        });
        poly.Color = Color.green;

        RegisterPolygon(poly);

        poly = new AlpinePolygon();
        poly.guid = new Guid();
        poly.Level = 1;
        poly.Polygon = Polygon.PolygonWithPoints(new Vector2[] {
            new Vector2(-1200, -1100),
            new Vector2(-1200, -1000),
            new Vector2(-1000, -1200),
            new Vector2(-1100, -1200)
        });
        poly.Color = Color.blue;

        RegisterPolygon(poly);
    }

    private void Remesh() {
        for(int i = 0;i < PolygonObjects.Count;i ++) {
            AlpinePolygon poly = PolygonObjects[i];
            Polygon meshPoly = poly.Polygon;

            if(poly.Renderer == null) {
                poly.Renderer = CreateMeshRenderer(poly);
            }

            List<Path> others = new List<Path>();
            for(int j = 0;j < PolygonObjects.Count;j ++) {
                if(PolygonObjects[j].Level > poly.Level) {
                    others.Add(PolygonObjects[j].Polygon.ClipperPath(Polygon.clipperScale));
                }
            }

            //If there are meshes that would subtract from here, we need to deal with them
            if(others.Count > 0) {
                Clipper clipper = new Clipper();
                Path path = poly.Polygon.ClipperPath(Polygon.clipperScale);
                clipper.AddPath(path, PolyType.ptSubject, true);
                clipper.AddPaths(others, PolyType.ptClip, true);
                List<Path> sol = new List<Path>();
                clipper.Execute(ClipType.ctDifference, sol, PolyFillType.pftEvenOdd, PolyFillType.pftEvenOdd);

                if(sol.Count == 0) {
                    Debug.LogWarning("No object returned from clip!");
                    poly.Renderer.gameObject.SetActive(false);
                    return;
                }

                meshPoly = Polygon.PolygonWithPoints(ClipperAddOns.PointsFromClipperPath(sol[0], Polygon.clipperScale));
                //Capture any extra polygon pieces
                for(int j = 1;j < sol.Count;j ++) {
                    meshPoly.AddPolygon(Polygon.PolygonWithPoints(ClipperAddOns.PointsFromClipperPath(sol[j], Polygon.clipperScale)));
                }
            }

            poly.Renderer.mesh = meshPoly.Mesh(poly.Color, Triangulator);

            //AlpinePolygon is a struct, so we need to write our changes
            PolygonObjects[i] = poly;
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
