using System;
using System.Collections.Generic;
using System.Threading;
using EPPZ.Geometry.Model;
using UnityEngine;

public class TerrainModificationController : MonoBehaviour
{
    public List<AlpinePolygon> TerrainModificationEffects;

    //TODO: Check to make sure it's initialized
    public void Register(AlpinePolygon effect) {
        if((effect.Flags & PolygonFlags.FLATTEN) == 0) return;
        TerrainModificationEffects.Add(effect);
        bool flattenUp = (effect.Flags & PolygonFlags.FLATTEN_UP) != 0;
        bool flattenDown = (effect.Flags & PolygonFlags.FLATTEN_DOWN) != 0;
        Flatten(effect.Polygon, flattenUp, flattenDown, effect.Height);
    }

    private void Flatten(Polygon polygon, bool flattenUp, bool flattenDown, float height) {
        FlattenTerrainJob job = new FlattenTerrainJob();
        job.polygon = polygon;
        job.flattenUp = flattenUp;
        job.flattenDown = flattenDown;
        job.height = height;
        job.Initialize();
        Thread thread = new Thread(new ThreadStart(job.Run));
        thread.Start();
    }

    public void Initialize() {
        Instance = this;

        // AlpinePolygon effect = new AlpinePolygon();
        // effect.Height = 600;
        // effect.Polygon = Polygon.PolygonWithPoints(new Vector2[] {
        //     new Vector2(-200, -200),
        //     new Vector2(-200,  200),
        //     new Vector2( 200,  200),
        //     new Vector2( 200, -200)
        // });
        // effect.Flags = PolygonFlags.FLATTEN;

        // Register(effect);
    }

    public static TerrainModificationController Instance;
}
