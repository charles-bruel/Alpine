using System.Collections.Generic;
using EPPZ.Geometry.Model;
using UnityEngine;

public class PolygonsPreview {
    private AlpinePolygonSource[] PolygonSources;
    private AlpinePolygon[] Polygons;

    public void SetPolygons(AlpinePolygonSource[] polygons) {
        Destroy();
        Initialize(polygons);
    }

    public void Initialize(AlpinePolygonSource[] polygons) {
        PolygonSources = polygons;
        Polygons = new AlpinePolygon[PolygonSources.Length];
        for(int i = 0;i < Polygons.Length;i ++) {
            AlpinePolygon poly = new AlpinePolygon();
            poly.Guid                = System.Guid.NewGuid();
            poly.Level               = PolygonSources[i].Level;
            poly.ArbitrarilyEditable = PolygonSources[i].ArbitrarilyEditable;
            poly.Flags               = (PolygonFlags) 0;
            poly.Height              = PolygonSources[i].Height;
            poly.Selectable          = false;
            
            // We removed flags (so they don't actually affect the world), so
            // has to be set manually
            poly.Color               = PolygonsController.ColorFromFlags(PolygonSources[i].Flags);

            Vector2[] points = new Vector2[PolygonSources[i].Points.Length];
            PolygonSources[i].Points.CopyTo(points, 0);
            poly.Polygon = Polygon.PolygonWithPoints(points);

            PolygonsController.Instance.RegisterPolygon(poly, false);

            Polygons[i] = poly;
        }
    }

    public void Destroy() {
        if(Polygons == null) return;
        foreach(AlpinePolygon polygon in Polygons) {
            PolygonsController.Instance.DestroyPolygon(polygon);
        }
    }

    public void Update(float angle, Vector2 position) {
        for(int i = 0;i < Polygons.Length;i ++) {
            Vector2[] transformedPoints = new Vector2[PolygonSources[i].Points.Length];
            for(int j = 0;j < PolygonSources[i].Points.Length;j ++) {
                transformedPoints[j] = ModAPIUtils.TransformBuildingCoordinates(PolygonSources[i].Points[j], angle, position);
            }

            Polygons[i].Polygon = Polygon.PolygonWithPoints(transformedPoints);
        }

        PolygonsController.Instance.MarkPolygonsDirty();
    }
}