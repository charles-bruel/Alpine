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

public static class ContoursUtils {

    public static ContourDefinition GetContours(ContourLayersDefinition layersDefinition, float[,] data, Bounds bounds) {
        // The process is as follows:
        // We rescale the target heights to [0 - 1]
        // We generate each contour line individually

        ContourDefinition toReturn = new ContourDefinition();
        toReturn.Layers = layersDefinition;
        toReturn.MajorPoints = new List<Vector2>[layersDefinition.Major.Length];
        toReturn.MinorPoints = new List<Vector2>[layersDefinition.Minor.Length];

        // Rescale the heights
        float[] major = new float[layersDefinition.Major.Length];
        float[] minor = new float[layersDefinition.Minor.Length];

        for(int i = 0;i < major.Length;i ++) {
            major[i] = layersDefinition.Major[i] / bounds.size.y + bounds.min.y;
        }

        for(int i = 0;i < minor.Length;i ++) {
            minor[i] = layersDefinition.Minor[i] / bounds.size.y + bounds.min.y;
        }

        // Generate the lines
        for(int i = 0;i < major.Length;i ++) {
            toReturn.MajorPoints[i] = GenerateContour(major[i], data, bounds);
        }

        for(int i = 0;i < minor.Length;i ++) {
            toReturn.MinorPoints[i] = GenerateContour(minor[i], data, bounds);
        }

        return toReturn;
    }

    public static List<Vector2> GenerateContour(float targetHeight, float[,] data, Bounds bounds) {
        List<Vector2> toReturn = new List<Vector2>();

        //Pixel jump
        // Each tile should have a grid of 32 points to make contours off of
        int j = data.GetLength(0) / 16;

        int xl = data.GetLength(1) - 1;
        int yl = data.GetLength(0) - 1;

        for(int x = 0;x < xl;x += j) {
            int nextX = x + j;
            if(nextX > xl) {
                nextX = x + 1;
            }
            for(int y = 0;y < yl;y += j) {
                int nextY = y + j;
                if(nextY > yl) {
                    nextY = y + 1;
                }
                Vector2 n = new Vector2((float) x / xl, (float) y / yl);
                Vector2 m = new Vector2((float) (x + j) / xl, (float) (y + j) / yl);

                n = n * bounds.size.ToHorizontal() + bounds.min.ToHorizontal();
                m = m * bounds.size.ToHorizontal() + bounds.min.ToHorizontal();

                float a = data[y+0, x];
                float b = data[y+0, nextX];
                float c = data[y+j, nextX];
                float d = data[y+j, x];
                
                toReturn.AddRange(RectPlaneIntersection(n, m, a, b, c, d, targetHeight));
            }
        }

        return toReturn;
    }

    // p_a = (n.x, a, n.y)        a-b       n-
    // p_b = (m.x, b, n.y)        | |       | |
    // p_c = (m.x, c, m.y)        d-c        -m
    // p_d = (n.x, d, m.y)
    private static List<Vector2> RectPlaneIntersection(Vector2 n, Vector2 m, float a, float b, float c, float d, float planeHeight) {
        List<Vector2> toReturn = new List<Vector2>();

        //If all points are on the same side of the plane, ignore
        if(a > planeHeight && b > planeHeight && c > planeHeight && d > planeHeight) {
            return toReturn;
        }

        if(a < planeHeight && b < planeHeight && c < planeHeight && d < planeHeight) {
            return toReturn;
        }

        //Average point
        float h = (a + b + c + d)/4;
        Vector2 i = (n + m) / 2;

        Vector3 p_a = new Vector3(n.x, a, n.y);
        Vector3 p_b = new Vector3(m.x, b, n.y);
        Vector3 p_c = new Vector3(m.x, c, m.y);
        Vector3 p_d = new Vector3(n.x, d, m.y);
        Vector3 mid = new Vector3(i.x, h, i.y);
        
        toReturn.AddRange(TrianglePlane(p_a, p_b, mid, planeHeight));
        toReturn.AddRange(TrianglePlane(p_b, p_c, mid, planeHeight));
        toReturn.AddRange(TrianglePlane(p_c, p_d, mid, planeHeight));
        toReturn.AddRange(TrianglePlane(p_d, p_a, mid, planeHeight));

        return toReturn;
    }

    private static List<Vector2> TrianglePlane(Vector3 a, Vector3 b, Vector3 c, float h) {
        List<Vector2> toReturn = new List<Vector2>();

        //If all points are on the same side of the plane, ignore
        if(a.y > h && b.y > h && c.y > h) {
            return toReturn;
        }

        if(a.y < h && b.y < h && c.y < h) {
            return toReturn;
        }

        // This algorithm works by finding the difference height above or below the plane, and lerping based on that
        toReturn.AddRange(LinePlane(a, b, h));
        toReturn.AddRange(LinePlane(b, c, h));
        toReturn.AddRange(LinePlane(c, a, h));

        return toReturn;
    }

    private static List<Vector2> LinePlane(Vector3 a, Vector3 b, float h) {
        List<Vector2> toReturn = new List<Vector2>();

        //If all points are on the same side of the plane, ignore
        if(a.y > h && b.y > h) {
            return toReturn;
        }

        if(a.y < h && b.y < h) {
            return toReturn;
        }

        float d_a = h - a.y;
        float d_t = b.y - a.y;
        float t = d_a / d_t;

        Vector2 result = new Vector2(Mathf.Lerp(a.x, b.x, t), Mathf.Lerp(a.z, b.z, t));
        toReturn.Add(result);

        return toReturn;
    }

}