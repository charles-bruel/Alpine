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
using UnityEngine;

public class GlobalNavController : MonoBehaviour
{
    public static GlobalNavController Instance;

    private bool GraphDirty = true;

    public NavGraph Graph = null;

    public void Initialize()
    {
        Instance = this;
    }

    public static void MarkGraphDirty() {
        if (Instance != null) Instance.GraphDirty = true;
    }

    void Update() {
        // TODO: Check for actual graph equality
        if(GraphDirty) {
            GraphDirty = false;

            TriggerGraphRebuild();
        }

        if(Graph != null) {
            Graph.DrawDebug();
        }
    }

    public bool TriggerGraphRebuild() {
        bool flag = false;

        foreach(var poly in PolygonsController.Instance.PolygonObjects) {
            if(poly is NavArea) {
                flag = true;
                break;
            }
        }

        if(flag == true){
            Graph = NavGraph.CreateCompleteGraph();
        }

        VisitorController.Instance.MarkGraphsDirtied();
        
        return flag;
    }
}