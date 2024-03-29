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
