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

using UnityEngine;
using System.Collections.Generic;
using GluonGui.Dialog;

public class PolygonTool {
    public PolygonConstructionData Data;
    public PolygonBuilder Builder;
    public PolygonBuilderToolGrab GrabTemplate;
    public List<PolygonBuilderToolGrab> Grabs = new List<PolygonBuilderToolGrab>();
    public Canvas Canvas;
    private HashSet<NavArea> EditedLinkedAreas = new HashSet<NavArea>();

    public void OnCancel() {
        for(int i = 0;i < Grabs.Count;i ++) {
            GameObject.Destroy(Grabs[i].gameObject);
        }
    }

    public void AddPoint(Vector2 pos, bool altMode = false) {
        // Universal setup
        PolygonBuilderToolGrab grab = GameObject.Instantiate(GrabTemplate);
        grab.transform.SetParent(Canvas.transform, false);
        grab.Data = Data;
        grab.Footprint = Builder.Result.Footprint;

        // If we are pressing control, we should insert into the sequence
        if(altMode && Data.SlopePoints.Count > 2) {
            // Identify between which points we should insert this one
            float minDisp = GetLengthChange(Data.SlopePoints[Data.SlopePoints.Count - 1].Pos, Data.SlopePoints[0].Pos, pos);
            grab.SlopePointIndex = 0;
            for(int i = 0;i < Data.SlopePoints.Count - 1;i ++) {
                float disp = GetLengthChange(Data.SlopePoints[i].Pos, Data.SlopePoints[i + 1].Pos, pos);
                if(disp < minDisp) {
                    minDisp = disp;
                    grab.SlopePointIndex = i + 1;
                }
            }

            Data.SlopePoints.Insert(grab.SlopePointIndex, new PolygonConstructionData.SlopePoint(pos));

            // We now need to go through all the grabs and update their indices
            for(int i = 0;i < Grabs.Count;i ++) {
                if(Grabs[i].SlopePointIndex >= grab.SlopePointIndex) {
                    Grabs[i].SlopePointIndex++;
                }
            }

            Grabs.Insert(grab.SlopePointIndex, grab);
        } else {
            Data.SlopePoints.Add(new PolygonConstructionData.SlopePoint(pos));
            grab.SlopePointIndex = Data.SlopePoints.Count - 1;

            Grabs.Add(grab);
        }
        
        // Universal finalization code
        grab.RectTransform.anchoredPosition = Data.SlopePoints[grab.SlopePointIndex].Pos;

        if(Data.SlopePoints.Count > 2) {
            if(!PolygonsController.Instance.PolygonObjects.Contains(Builder.Result.Footprint)) {
                PolygonsController.Instance.PolygonObjects.Add(Builder.Result.Footprint);
            }
            PolygonsController.Instance.MarkPolygonsDirty();
        }
    }

    // Returns (AP + BP ) - AB, i.e. how much length is added if a node 
    // inserted between a and b at p
    private float GetLengthChange(Vector2 a, Vector2 b, Vector2 p) {
        return (a - p).magnitude + (p - b).magnitude - (a - b).magnitude;
    }

    public void RemovePoint(Vector2 pos, bool altMode = false) {
        //Find the closest station to the position and remove it
        float sqrMinDist = Mathf.Infinity;
        int index = -1;

        for(int i = 0;i < Data.SlopePoints.Count;i ++) {
            Vector2 hpos = Data.SlopePoints[i].Pos;
            float sqrMagnitude = (hpos - pos).sqrMagnitude;
            if(sqrMagnitude < sqrMinDist) {
                sqrMinDist = sqrMagnitude;
                index = i;
            }
        }
        if(index == -1) return;

        Data.SlopePoints.RemoveAt(index);

        // Fix the grabs
        for(int i = 0;i < Grabs.Count;i ++) {
            if(Grabs[i].SlopePointIndex == index) {
                GameObject.Destroy(Grabs[i].gameObject);
                Grabs.RemoveAt(i);
                i--;
            } else if(Grabs[i].SlopePointIndex > index) {
                Grabs[i].SlopePointIndex--;
            }
        }

        if(Data.SlopePoints.Count <= 2) {
            PolygonsController.Instance.PolygonObjects.Remove(Builder.Result.Footprint);
            // TODO: More elegant system
            if(Builder.Result.Footprint.Filter != null && Builder.Result.Footprint.Filter.gameObject != null) {
                GameObject.Destroy(Builder.Result.Footprint.Filter.gameObject);
            }
        }
        PolygonsController.Instance.MarkPolygonsDirty();
    }

    public void Build() {
        Builder.Build();
    }

    public void Finish() {
        Builder.Finish();

        foreach(NavArea area in EditedLinkedAreas) {
            // We need to recalculate the portals on these nav areas, and see if there are any new portals
            // that we need to add to the polygon
            
            // To do this, we can create dummy PolygonConstructionData to find portals with
            PolygonConstructionData data = new PolygonConstructionData();
            data.SlopePoints = new List<PolygonConstructionData.SlopePoint>();
            foreach(var point in area.Polygon.points) {
                data.SlopePoints.Add(new PolygonConstructionData.SlopePoint(point));
            }

            PolygonBuilder.FindSnapping(0.1f, area, data);
            List<NavPortal> portals = PolygonBuilder.PlacePortals(area, data);

            // First filter pass - remove all portals that are not connected to the edited area
            for(int i = 0;i < portals.Count;i ++) {
                if(portals[i].A != area && portals[i].B != area) {
                    portals.RemoveAt(i);
                    i--;
                }
            }

            // Second filter pass - remove all portals that are already associated with the polygon.
            // We will assume for the sake of this operation that the portals are duplicate iff
            // the A1, A2, B1, B2 indices are the same
            for(int i = 0;i < portals.Count;i ++) {
                NavPortal portal = portals[i];
                bool duplicate = false;
                foreach(INavNode node in area.Nodes) {
                    if(node is NavPortal) {
                        NavPortal other = node as NavPortal;
                        // A = A, B = B
                        {
                            bool aMatchAligned = other.A1 == portal.A1 && other.A2 == portal.A2;
                            bool bMatchAligned = other.B1 == portal.B1 && other.B2 == portal.B2;
                            bool aMatchFlipped = other.A1 == portal.A2 && other.A2 == portal.A1;
                            bool bMatchFlipped = other.B1 == portal.B2 && other.B2 == portal.B1;

                            if((aMatchAligned || aMatchFlipped) && (bMatchAligned || bMatchFlipped)) {
                                duplicate = true;
                                break;
                            }
                        }

                        // A = B, B = A
                        {
                            bool abMatchAligned = other.A1 == portal.B1 && other.A2 == portal.B2;
                            bool baMatchAligned = other.B1 == portal.A1 && other.B2 == portal.A2;
                            bool abMatchFlipped = other.A1 == portal.B2 && other.A2 == portal.B1;
                            bool baMatchFlipped = other.B1 == portal.A2 && other.B2 == portal.A1;

                            if((abMatchAligned || abMatchFlipped) && (baMatchAligned || baMatchFlipped)) {
                                duplicate = true;
                                break;
                            }
                        }
                    }
                }

                if(duplicate) {
                    portals.RemoveAt(i);
                    i--;
                }
            }
            
            // For these new portals, we can now add them to the polygon
            Builder.Result.Inflate(portals);
        }
    }

    public void Start(PolygonBuilding polygonBuilding, PolygonFlags flags) {
        Builder = new PolygonBuilder();
        Builder.Initialize(polygonBuilding, flags);
        Data = Builder.Data;
    }

    public void PrepareForEditing(PolygonBuilding building) {
        NavArea footprint = building.Footprint;
        foreach(var point in footprint.Polygon.points) {
            AddPoint(point);
        }

        foreach(NavPortal navPortal in footprint.Nodes) {
            if(navPortal.A != footprint) EditedLinkedAreas.Add(navPortal.A);
            if(navPortal.B != footprint) EditedLinkedAreas.Add(navPortal.B);
        }

        building.Destroy();

        PolygonBuilder.FindSnapping(0.1f, Builder.Result.Footprint, Builder.Data);
    }
}