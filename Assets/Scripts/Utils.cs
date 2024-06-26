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
using System;
using UnityEngine.Assertions;

public static class Utils {
    // https://forum.unity.com/threads/debug-drawarrow.85980/
    public static void DebugDrawArrow(Vector3 pos, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
    {
        Debug.DrawRay(pos, direction, color);
       
        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0,180+arrowHeadAngle,0) * new Vector3(0,0,1);
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0,180-arrowHeadAngle,0) * new Vector3(0,0,1);
        Debug.DrawRay(pos + direction, right * arrowHeadLength, color);
        Debug.DrawRay(pos + direction, left * arrowHeadLength, color);
    }

    public static void RemoveTreesByPolygon(Polygon polygon) {
        RemoveTreesJob job = new RemoveTreesJob();
        job.Polygon = polygon;
        Thread thread = new Thread(new ThreadStart(job.Run));
        thread.Start();
    }

    public static void RemoveRocksByPolygon(Polygon polygon) {
        RemoveRocksJob job = new RemoveRocksJob();
        job.Polygon = polygon;
        Thread thread = new Thread(new ThreadStart(job.Run));
        thread.Start();
    }

    public static List<int> GetTreesToRemove(Polygon polygon, byte x, byte y) {
        List<int> toReturn = new List<int>();

        var enumerator = TerrainManager.Instance.TreesData.GetIndexEnumerator(x, y);
        while(enumerator.MoveNext()) {
            var element = TerrainManager.Instance.TreesData[enumerator.Current];
            if(polygon.ContainsPoint(element.pos.ToHorizontal())) {
                toReturn.Add(enumerator.Current);
            }
        }

        return toReturn;
    }

    public static List<int> GetRocksToRemove(Polygon polygon, byte x, byte y) {
        List<int> toReturn = new List<int>();

        var enumerator = TerrainManager.Instance.RocksData.GetIndexEnumerator(x, y);
        while(enumerator.MoveNext()) {
            var element = TerrainManager.Instance.RocksData[enumerator.Current];
            if(polygon.ContainsPoint(element.pos.ToHorizontal())) {
                toReturn.Add(enumerator.Current);
            }
        }

        return toReturn;
    }

    // Thank you wikiepdia
    // https://en.wikipedia.org/wiki/Line%E2%80%93line_intersection
    // This finds the intersection points between 2 2d lines defined by points A1 and A2, 
    // and points B1 and B2
    public static Vector2 LineLine(Vector2 A1, Vector2 A2, Vector2 B1, Vector2 B2) {
        // Redefine the variables
        // This should be optimized away; it's just here so it matches wikipedia
        float x1 = A1.x;
        float x2 = A2.x;
        float x3 = B1.x;
        float x4 = B2.x;
        float y1 = A1.y;
        float y2 = A2.y;
        float y3 = B1.y;
        float y4 = B2.y;


        float x_num = (x1 * y2 - y1 * x2) * (x3 - x4) - (x1 - x2) * (x3 * y4 - y3 * x4);
        float y_num = (x1 * y2 - y1 * x2) * (y3 - y4) - (y1 - y2) * (x3 * y4 - y3 * x4);
        float dom = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);
        
        return new Vector2(x_num / dom, y_num / dom);
    }

    // This creates a fillet between the AB and BC segments, with radius r and the fillet
    // itself will have filletPoints points. The output is an array of points, including A
    // and C, of length filletPoints + 2
    public static List<Vector2> Fillet(Vector2 A, Vector2 B, Vector2 C, float r, int filletPoints) {
        List<Vector2> toReturn = new List<Vector2>();
        toReturn.Add(A);

        Vector2 deltaA = A - B;
        Vector2 deltaC = C - B;

        float aAngle = Mathf.Atan2(deltaA.y, deltaA.x) * Mathf.Rad2Deg;
        float cAngle = Mathf.Atan2(deltaC.y, deltaC.x) * Mathf.Rad2Deg;
        float mAngle = Mathf.LerpAngle(aAngle, cAngle, 0.5f);
        float alpha = Mathf.DeltaAngle(aAngle, mAngle);
        bool flag = alpha < 0;
        float y = r / Mathf.Sin(alpha * Mathf.Deg2Rad);
        if(flag) y *= -1;
        Vector2 O = B + new Vector2(Mathf.Cos(mAngle * Mathf.Deg2Rad), Mathf.Sin(mAngle * Mathf.Deg2Rad)) * y;
        float startAngle = aAngle - 90;
        float endAngle = cAngle + 90;
        if(flag) {
            startAngle += 180;
            endAngle += 180;
        }
        for(int i = 0;i < filletPoints;i ++) {
            float t = (float) i / filletPoints;
            float angle = Mathf.LerpAngle(startAngle, endAngle, t) * Mathf.Deg2Rad;
            toReturn.Add(O + r * new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)));
        }

        toReturn.Add(C);

        return toReturn;
    }

    // Finds the distance between a point and a line segment
    // Based on https://stackoverflow.com/questions/849211/shortest-distance-between-a-point-and-a-line-segment
    public static float LineSegmentPoint(Vector2 v, Vector2 w, Vector2 p) {
        return Mathf.Sqrt(LineSegmentPointSqr(v, w, p));
    }

    public static float LineSegmentPointSqr(Vector2 v, Vector2 w, Vector2 p) {
        float l2 = (v - w).sqrMagnitude;
        if(l2 == 0) return (v - p).sqrMagnitude;
        float t = Mathf.Max(0, Mathf.Min(1, Vector2.Dot(p - v, w - v) / l2));
        Vector2 projection = v + t * (w - v);
        return (p - projection).sqrMagnitude;
    }

    // Converts a float array to a byte array, bit by bit
    public static byte[] FloatArrayToByteArray(float[] floats) {
        byte[] toReturn = new byte[floats.Length * sizeof(float)];
        Buffer.BlockCopy(floats, 0, toReturn, 0, toReturn.Length);
        return toReturn;
    }

    // Converts a byte array to a float array, bit by bit
    public static float[] ByteArrayToFloatArray(byte[] bytes) {
        Assert.IsTrue(bytes.Length % sizeof(float) == 0);
        float[] toReturn = new float[bytes.Length / sizeof(float)];
        Buffer.BlockCopy(bytes, 0, toReturn, 0, bytes.Length);
        return toReturn;
    }
}