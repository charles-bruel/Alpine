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
using EPPZ.Geometry.Model;
using System.Threading;

public static class ModAPIUtils {
    public static Vector2 TransformBuildingCoordinates(Vector2 input, float parentAngle, Vector2 parentPos) {
        float theta = -parentAngle * Mathf.Deg2Rad;
        float sin = Mathf.Sin(theta);
        float cos = Mathf.Cos(theta);
        float x = input.x;
        float y = input.y;
        Vector2 point = new Vector2(x * cos - y * sin, x * sin + y * cos);

        point += parentPos;

        return point;
    }

    public static AlpinePolygon TransformPolygon(AlpinePolygonSource polygon, float parentAngle, Vector3 parentPos) {
        AlpinePolygon toReturn = new AlpinePolygon();
        toReturn.Guid                = System.Guid.NewGuid();
        toReturn.Level               = polygon.Level;
        toReturn.ArbitrarilyEditable = polygon.ArbitrarilyEditable;
        toReturn.Flags               = polygon.Flags;
        toReturn.Height              = polygon.Height + parentPos.y;

        Vector2[] transformedPoints = new Vector2[polygon.Points.Length];
        for(int i = 0;i < polygon.Points.Length;i ++) {
            // First we rotate the polygon
            float theta = -parentAngle * Mathf.Deg2Rad;
            float sin = Mathf.Sin(theta);
            float cos = Mathf.Cos(theta);
            float x = polygon.Points[i].x;
            float y = polygon.Points[i].y;
            Vector2 point = new Vector2(x * cos - y * sin, x * sin + y * cos);

            point.x += parentPos.x;
            point.y += parentPos.z;

            transformedPoints[i] = point;
        }
        toReturn.Polygon = Polygon.PolygonWithPoints(transformedPoints);

        return toReturn;
    }
}