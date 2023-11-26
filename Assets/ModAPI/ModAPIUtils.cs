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