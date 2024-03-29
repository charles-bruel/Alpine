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

public static class Extensions {
    public static Vector2 ToHorizontal(this Vector3 v) {
        return new Vector2(v.x, v.z);
    }

    public static Vector3 Inflate3rdDim(this Vector2 v, float y) {
        return new Vector3(v.x, y, v.y);
    }

    public static Vector3 DropW(this Vector4 v) {
        return new Vector3(v.x, v.y, v.z);
    }

    public static float NextFloat(this System.Random rand, float min, float max) {
        float val = (float) rand.NextDouble();
        val *= (max - min);
        val += min;
        return val;
    }

    public static float NextAngleRads(this System.Random rand) {
        return (float)(rand.NextDouble() * 2.0 * 3.141592653589793);
    }

    // UNSIGNED distance to the polygon. This simply iterates over each line and calculates 
    // its distance, and takes the min of that
    public static float DistanceToPoint(this EPPZ.Geometry.Model.Polygon polygon, Vector2 point) {
        float min = Mathf.Infinity;
        foreach(var edge in polygon.edges) {
            Vector2 p1 = edge.a;
            Vector2 p2 = edge.b;

            float dist = Utils.LineSegmentPoint(p1, p2, point);
            if(dist < min) {
                min = dist;
            }
        }

        return min;
    }
}