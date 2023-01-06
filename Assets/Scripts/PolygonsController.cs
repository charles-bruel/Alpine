using System;
using System.Collections.Generic;
using ClipperLib;
using EPPZ.Geometry.AddOns;
using EPPZ.Geometry.Model;
using UnityEngine;
using UnityEngine.EventSystems;

using Path = System.Collections.Generic.List<ClipperLib.IntPoint>;

[RequireComponent(typeof(BoxCollider))]
public class PolygonsController : MonoBehaviour, IPointerClickHandler
{
    public TriangulatorType Triangulator = TriangulatorType.Dwyer;
    public Material Material;
    public Material SelectedMaterial;
    public List<AlpinePolygon> PolygonObjects;
    public Guid SelectedPolygon = Guid.Empty;
    public PolygonEditor PolygonEditor;
    //To recieve raycasts
    public BoxCollider Collider;

    private bool PolygonsDirty = false;

    public void MarkPolygonsDirty() {
        PolygonsDirty = true;
    }

    public void RegisterPolygon(AlpinePolygon polygon) {
        PolygonObjects.Add(polygon);

        Remesh();
    }

    public void DeregisterPolygon(Guid guid) {
        for(int i = 0;i < PolygonObjects.Count;i ++) {
            if(PolygonObjects[i].Guid == guid) {
                PolygonObjects.RemoveAt(i);
                i--;
            }
        }

        Remesh();
    }

    void Update() {
        if(PolygonsDirty) {
            PolygonsDirty = false;
            Remesh();
        }
    }

    void Start() {
        Collider = GetComponent<BoxCollider>();
        Collider.size = new Vector3(
            TerrainManager.Instance.TileSize * TerrainManager.Instance.NumTilesX,
            TerrainManager.Instance.TileSize * TerrainManager.Instance.NumTilesY,
            1
        );

        AlpinePolygon poly = new AlpinePolygon();
        poly.Guid = Guid.NewGuid();
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
        poly.Guid = Guid.NewGuid();
        poly.Level = 1;
        poly.Polygon = Polygon.PolygonWithPoints(new Vector2[] {
            new Vector2(-33, -45),
            new Vector2(-23,  32),
            new Vector2( 75,  23),
            new Vector2( 65, -76)
        });
        poly.Color = Color.green;
        poly.ArbitrarilyEditable = true;

        RegisterPolygon(poly);

        poly = new AlpinePolygon();
        poly.Guid = Guid.NewGuid();
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

            if(poly.Filter == null) {
                var temp = CreateMeshRenderer(poly);
                poly.Renderer = temp.Item1;
                poly.Filter = temp.Item2;
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
                    poly.Filter.gameObject.SetActive(false);
                    return;
                }

                meshPoly = Polygon.PolygonWithPoints(ClipperAddOns.PointsFromClipperPath(sol[0], Polygon.clipperScale));
                //Capture any extra polygon pieces
                for(int j = 1;j < sol.Count;j ++) {
                    meshPoly.AddPolygon(Polygon.PolygonWithPoints(ClipperAddOns.PointsFromClipperPath(sol[j], Polygon.clipperScale)));
                }
            }

            poly.Filter.mesh = meshPoly.Mesh(poly.Filter.mesh, poly.Color, Triangulator, poly.Guid.ToString());
        }
    }

    private (MeshRenderer, MeshFilter) CreateMeshRenderer(AlpinePolygon poly) {
        GameObject meshObject = new GameObject(poly.Guid.ToString());
        meshObject.transform.parent = transform;
        meshObject.transform.position = Vector3.zero;
        meshObject.transform.localEulerAngles = new Vector3();
        meshObject.layer = gameObject.layer;

        MeshRenderer meshRenderer = meshObject.AddComponent<MeshRenderer>();
        meshRenderer.material = Material;

        MeshFilter meshFilter = meshObject.AddComponent<MeshFilter>();

        return (meshRenderer, meshFilter);

    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition).ToHorizontal();

        uint max = 0;
        //Find the greatest level
        //We want to select the "top" polygon with higher priority
        //Slightly inefficient but it's fine the cold path

        //We deselect the old polygon here too
        for(int i = 0;i < PolygonObjects.Count;i ++) {
            if(PolygonObjects[i].Level > max) {
                max = PolygonObjects[i].Level;
            }
            if(SelectedPolygon == PolygonObjects[i].Guid) {
                PolygonObjects[i].Renderer.material = Material;
            }
        }

        //This does higher level to lowest (0)
        for(uint l = max;l > 0;l --) {
            for(int i = 0;i < PolygonObjects.Count;i ++) {
                if(PolygonObjects[i].Level == l) {
                    //Correct level, logic goes here
                    if(PolygonObjects[i].Polygon.ContainsPoint(pos)) {
                        SelectedPolygon = PolygonObjects[i].Guid;
                        PolygonObjects[i].Renderer.material = SelectedMaterial;

                        //We now break completely out of the loop to avoid selecting two polygons
                        //TODO: Refactor to not use goto?
                        goto Selected;
                    }
                }
            }
        }
        Selected:

        if(PolygonEditor != null)
            PolygonEditor.Reinflate();
    }

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
    }
}
