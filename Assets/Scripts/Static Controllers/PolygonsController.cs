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

    public static PolygonsController Instance;

    private bool PolygonsDirty = false;
    private bool Initialized = false;

    public void MarkPolygonsDirty() {
        PolygonsDirty = true;
    }

    public void RegisterPolygon(AlpinePolygon polygon) {
        PolygonObjects.Add(polygon);
        polygon.Color = ColorFromFlags(polygon.Flags);

        ApplyPolygonEffects(polygon);

        MarkPolygonsDirty();
    }

    public static Color ColorFromFlags(PolygonFlags flags) {
        if((flags & PolygonFlags.FLAT_NAVIGABLE) != 0) {
            return RenderingData.Instance.SnowfrontColor;
        }
        if((flags & PolygonFlags.GROUND_CLEARANCE) != 0) {
            return RenderingData.Instance.ClearedColor;
        }
        if((flags & PolygonFlags.AERIAL_CLEARANCE) != 0) {
            return RenderingData.Instance.DevelopedColor;
        }

        return RenderingData.Instance.UndevelopedBackgroundColor;
    }

    private void ApplyPolygonEffects(AlpinePolygon polygon) {
        if((polygon.Flags & PolygonFlags.AERIAL_CLEARANCE) != 0) {
            Utils.RemoveTreesByPolygon(polygon.Polygon);
        }
        if((polygon.Flags & PolygonFlags.GROUND_CLEARANCE) != 0) {
            Utils.RemoveRocksByPolygon(polygon.Polygon);
        }
        if((polygon.Flags & PolygonFlags.FLATTEN) != 0) {
            TerrainModificationController.Instance.Register(polygon);
        }
    }

    public void DeregisterPolygon(Guid guid) {
        //TODO: Also remove polygon render objects
        for(int i = 0;i < PolygonObjects.Count;i ++) {
            if(PolygonObjects[i].Guid == guid) {
                PolygonObjects.RemoveAt(i);
                i--;
            }
        }

        Remesh();
    }

    void Update() {
        if(!Initialized) return;

        if(PolygonsDirty) {
            PolygonsDirty = false;
            Remesh();
        }
    }

    public void Initialize() {
        Instance = this;

        Collider = GetComponent<BoxCollider>();
        float width = TerrainManager.Instance.TileSize * TerrainManager.Instance.NumTilesX;
        float height = TerrainManager.Instance.TileSize * TerrainManager.Instance.NumTilesY;
        Collider.size = new Vector3(width, height, 1);

        float x1 = -width  / 2;
        float x2 =  width  / 2;
        float y1 = -height / 2;
        float y2 =  height / 2;

        AlpinePolygon backgroundPolygon = new AlpinePolygon();
        backgroundPolygon.Guid = Guid.NewGuid();
        backgroundPolygon.Level = 0;
        backgroundPolygon.Polygon = Polygon.PolygonWithPoints(new Vector2[] {
            new Vector2(x1, y1),
            new Vector2(x1, y2),
            new Vector2(x2, y2),
            new Vector2(x2, y1)
        });
        backgroundPolygon.Color = RenderingData.Instance.UndevelopedBackgroundColor;

        RegisterPolygon(backgroundPolygon);

        Initialized = true;
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

    public PolygonSnappingResult? CheckForSnapping(Vector2 Pos, float MaxDistPoint, float MaxDistEdge, PolygonFlags Mask) {
        PolygonSnappingResult? Result = null;
        float MaxDistSqr = MaxDistPoint * MaxDistPoint;
        float MinDistSqr = Mathf.Infinity;
        
        for(int i = 0;i < PolygonObjects.Count;i ++) {
            AlpinePolygon poly = PolygonObjects[i];
            if((poly.Flags & Mask) == 0) continue;
            // TODO: Expand bounds
            // if(!poly.Polygon.bounds.Contains(Pos)){
            //     continue;
            // }
            //TODO: Support subpolygons
            for(int j = 0;j < poly.Polygon.points.Length;j ++) {
                Vector2 pos = poly.Polygon.points[j];
                float distSqr = (pos - Pos).sqrMagnitude;
                if(distSqr < MaxDistSqr) {
                    if(distSqr < MinDistSqr) {
                        MinDistSqr = distSqr;

                        PolygonSnappingResult newResult = new PolygonSnappingResult();
                        newResult.Pos = pos;
                        newResult.Offset = 0;
                        newResult.Target = poly;
                        newResult.PointID = j;

                        Result = newResult;
                    }
                }
            }
        }
        // Snapping to point is always more desirable
        if(Result != null) {
            return Result;
        }

        MaxDistSqr = MaxDistEdge * MaxDistEdge;
        MinDistSqr = Mathf.Infinity;

        for(int i = 0;i < PolygonObjects.Count;i ++) {
            AlpinePolygon poly = PolygonObjects[i];
            if((poly.Flags & Mask) == 0) continue;
            // TODO: Expand bounds
            // if(!poly.Polygon.bounds.Contains(Pos)){
            //     continue;
            // }
            //TODO: Support subpolygons
            for(int j = 0;j < poly.Polygon.points.Length;j ++) {
                Vector2 p1 = poly.Polygon.points[j];
                Vector2 p2;
                if(j == poly.Polygon.points.Length - 1) {
                    p2 = poly.Polygon.points[0];
                } else {
                    p2 = poly.Polygon.points[j + 1];
                }
                
                // Extracted the LineSegmentPointSqr method to get at internal variables
                float l2 = (p1 - p2).sqrMagnitude;
                float t = Mathf.Max(0, Mathf.Min(1, Vector2.Dot(Pos - p1, p2 - p1) / l2));
                Vector2 projection = p1 + t * (p2 - p1);
                float distSqr = (Pos - projection).sqrMagnitude;
                
                if(distSqr < MaxDistSqr) {
                    if(distSqr < MinDistSqr) {
                        MinDistSqr = distSqr;

                        PolygonSnappingResult newResult = new PolygonSnappingResult();
                        newResult.Pos = projection;
                        newResult.Offset = t;
                        newResult.Target = poly;
                        newResult.PointID = j;

                        Result = newResult;
                    }
                }
            }
        }

        return Result;
    }

    public struct PolygonSnappingResult {
        public Vector2 Pos;
        public AlpinePolygon Target;
        public int PointID;
        // An offset of 0 means it's on the given point, an offset
        // of 1 means it's at the next point
        // This allows it to return a point on the edge or on a
        // vertex
        public float Offset;
    }
}
