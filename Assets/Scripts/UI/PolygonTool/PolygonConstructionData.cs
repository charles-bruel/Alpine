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

public class PolygonConstructionData {
    public PolygonConstructionData() {
        SlopePoints = new List<SlopePoint>();
    }

    //TODO: Support polygons with holes, etc.
    public List<SlopePoint> SlopePoints;

    public Vector2[] GetPoints() {
        Vector2[] toReturn = new Vector2[SlopePoints.Count];
        for(int i = 0;i < toReturn.Length;i ++) {
            toReturn[i] = SlopePoints[i].Pos;
        }
        return toReturn;
    }

    public struct SlopePoint {
        public SlopePoint(Vector2 pos) {
            Pos = pos;
            Snapping = null;
        }
        public SlopePoint(Vector2 pos, PolygonsController.PolygonSnappingResult snapping) {
            Pos = pos;
            Snapping = snapping;
        }
        public Vector2 Pos;
        public PolygonsController.PolygonSnappingResult? Snapping;
    }
}