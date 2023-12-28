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
        
        return flag;
    }
}