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
using System.Collections.Generic;
using ClipperLib;
using Codice.Client.BaseCommands.Download;
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
    public Material BaseLayerMaterial;
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

    public void RegisterPolygon(AlpinePolygon polygon, bool AutoColor = true) {
        PolygonObjects.Add(polygon);
         if(AutoColor) polygon.Color = ColorFromFlags(polygon.Flags);

        ApplyPolygonEffects(polygon);

        MarkPolygonsDirty();
    }

    public void MarkPolygonDirty(AlpinePolygon polygon) {
        polygon.Color = ColorFromFlags(polygon.Flags);
        RemeshPolygon(polygon);
    }

    public static Color ColorFromFlags(PolygonFlags flags) {
        if((flags & PolygonFlags.STRUCTURE) != 0) {
            return RenderingData.Instance.StructureColor;
        }
        PolygonFlags slopeIsolated = flags & PolygonFlags.SLOPE_MASK;
        if(slopeIsolated == PolygonFlags.SLOPE_GREEN) {
            return RenderingData.Instance.GreenSlopeColor;
        }
        if(slopeIsolated == PolygonFlags.SLOPE_BLUE) {
            return RenderingData.Instance.BlueSlopeColor;
        }
        if(slopeIsolated == PolygonFlags.SLOPE_BLACK) {
            return RenderingData.Instance.BlackSlopeColor;
        }
        if(slopeIsolated == PolygonFlags.SLOPE_DOUBLE_BLACK) {
            return RenderingData.Instance.DoubleBlackSlopeColor;
        }
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

    public void DestroyPolygon(AlpinePolygon polygon) {
        if(polygon.Selected) {
            SelectedPolygon = new Guid();
            polygon.Selected = false;
            polygon.OnDeselected();
        }
        if (polygon.Filter != null) {
            GameObject.Destroy(polygon.Filter.gameObject);
        }
        polygon.OnDestroy();
        UnregisterPolygon(polygon.Guid);
    }

    public void UnregisterPolygon(Guid guid) {
        //TODO: Also remove polygon render objects
        for(int i = 0;i < PolygonObjects.Count;i ++) {
            if(PolygonObjects[i].Guid == guid) {
                PolygonObjects.RemoveAt(i);
                i--;
            }
        }

        MarkPolygonsDirty();
    }

    void Update() {
        if(!Initialized) return;

        if(PolygonsDirty) {
            PolygonsDirty = false;
            Remesh();
            RecalculateOverlappingNavAreas();
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
            new Vector2(x2, y1),
            new Vector2(x2, y2),
            new Vector2(x1, y2)
        });
        backgroundPolygon.Color = RenderingData.Instance.UndevelopedBackgroundColor;

        RegisterPolygon(backgroundPolygon);

        Initialized = true;
    }

    private void Remesh() {
        for(int i = 0;i < PolygonObjects.Count;i ++) {
            AlpinePolygon poly = PolygonObjects[i];
            RemeshPolygon(poly);
        }
    }

    public void RecalculateOverlappingNavAreas() {
        for(int i = 0;i < PolygonObjects.Count;i ++ ) {
            NavArea area1 = PolygonObjects[i] as NavArea;
            if(area1 == null) continue;
            if((area1.Flags & PolygonFlags.FLAT_NAVIGABLE) == 0) continue;

            area1.Modified = true;
            area1.OverlappingNavAreas.Clear();

            for(int j = 0;j < PolygonObjects.Count;j ++ ) {
                if(i == j) continue;
                if(j > i) continue;

                NavArea area2 = PolygonObjects[j] as NavArea;
                if(area2 == null) continue;
                if((area2.Flags & PolygonFlags.FLAT_NAVIGABLE) == 0) continue;
                
                // Check intersection
                var path1 = area1.Polygon.ClipperPath(1);
                var path2 = area2.Polygon.ClipperPath(1);
                Clipper clipper = new Clipper();
                clipper.AddPath(path1, PolyType.ptSubject, true);
                clipper.AddPath(path2, PolyType.ptClip, true);
                List<List<IntPoint>> intersectionPaths = new List<List<IntPoint>>();
                clipper.Execute(ClipType.ctIntersection, intersectionPaths);
                bool intersect = intersectionPaths.Count > 0;
                if(!intersect) continue;

                // There is intersection, so we need to add the polygons to the set
                // Since overlapping is transitive, recursion is not nessecary
                for(int k = 0; k < area1.OverlappingNavAreas.Count;k ++) {
                    var area = area1.OverlappingNavAreas[k];
                    if(area.OverlappingNavAreas.Contains(area2)) continue;
                    area.OverlappingNavAreas.Add(area2);
                }
                for(int k = 0; k < area2.OverlappingNavAreas.Count;k ++) {
                    var area = area2.OverlappingNavAreas[k];
                    if(area.OverlappingNavAreas.Contains(area1)) continue;
                    area.OverlappingNavAreas.Add(area1);
                }
                area1.OverlappingNavAreas.Add(area2);
                area2.OverlappingNavAreas.Add(area1);
            }
        }
    }
    private void RemeshPolygon(AlpinePolygon poly) {
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
            List<Path> sol = new List<Path>();
            sol.Add(poly.Polygon.ClipperPath(Polygon.clipperScale));

            for(int i = 0;i < others.Count;i ++) {
                Clipper clipper = new Clipper();
                clipper.AddPaths(sol, PolyType.ptSubject, true);
                clipper.AddPath(others[i], PolyType.ptClip, true);
                sol.Clear();
                clipper.Execute(ClipType.ctDifference, sol, PolyFillType.pftNonZero, PolyFillType.pftNonZero);
                if(sol.Count == 0) {
                    // Completley covered; no mesh for this object
                    // TODO: Delete mesh?
                    return;
                }
            }

            meshPoly = Polygon.PolygonWithPoints(ClipperAddOns.PointsFromClipperPath(sol[0], Polygon.clipperScale));
            //Capture any extra polygon pieces
            for(int j = 1;j < sol.Count;j ++) {
                meshPoly.AddPolygon(Polygon.PolygonWithPoints(ClipperAddOns.PointsFromClipperPath(sol[j], Polygon.clipperScale)));
            }
        }

        poly.Filter.mesh = meshPoly.Mesh(poly.Filter.mesh, poly.Color, Triangulator, poly.Guid.ToString());
        
    }

    private (MeshRenderer, MeshFilter) CreateMeshRenderer(AlpinePolygon poly) {
        GameObject meshObject = new GameObject(poly.Guid.ToString());
        meshObject.transform.parent = transform;
        meshObject.transform.position = Vector3.zero;
        meshObject.transform.localEulerAngles = new Vector3();
        meshObject.layer = gameObject.layer;

        MeshRenderer meshRenderer = meshObject.AddComponent<MeshRenderer>();
        meshRenderer.material = poly.Level != 0 ? Material : BaseLayerMaterial;

        MeshFilter meshFilter = meshObject.AddComponent<MeshFilter>();

        return (meshRenderer, meshFilter);

    }

    public void OnPointerClick(PointerEventData eventData) {
        // Only allow left clicks though
        if(eventData.button != PointerEventData.InputButton.Left) return;

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
                // DESELECTION LOGIC
                PolygonObjects[i].Renderer.material = Material;
                PolygonObjects[i].Selected = false;
                PolygonObjects[i].OnDeselected();
            }
        }
        
        // DESELECTION LOGIC
        // If nothing is selected, ensure the old polygon is fully deselected
        SelectedPolygon = new Guid();

        //This does higher level to lowest (0)
        for(uint l = max;l > 0;l --) {
            for(int i = 0;i < PolygonObjects.Count;i ++) {
                if(PolygonObjects[i].Level == l) {
                    //Correct level, logic goes here
                    if(PolygonObjects[i].Selectable && PolygonObjects[i].Polygon.ContainsPoint(pos)) {
                        // SELECTION LOGIC
                        SelectedPolygon = PolygonObjects[i].Guid;
                        PolygonObjects[i].Renderer.material = SelectedMaterial;
                        PolygonObjects[i].Selected = true;
                        PolygonObjects[i].OnSelected();

                        //We now break completely out of the loop to avoid selecting two polygons
                        goto Selected;
                    }
                }
            }
        }
        Selected:

        if(PolygonEditor != null)
            PolygonEditor.Reinflate();
    }

    public PolygonSnappingResult? CheckForSnapping(Vector2 Pos, float MaxDistPoint, float MaxDistEdge, PolygonFlags Mask, AlpinePolygon exclude) {
        PolygonSnappingResult? Result = null;
        float MaxDistSqr = MaxDistPoint * MaxDistPoint;
        float MinDistSqr = Mathf.Infinity;
        
        for(int i = 0;i < PolygonObjects.Count;i ++) {
            AlpinePolygon poly = PolygonObjects[i];
            if((poly.Flags & Mask) == 0) continue;
            if(poly == exclude) continue;
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
            if(poly == exclude) continue;
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
