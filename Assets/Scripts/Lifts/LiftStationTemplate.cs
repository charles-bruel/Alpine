using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiftStationTemplate : LiftRoutingSegmentTemplate
{
    public NavDestinationDefinition EntryNavNode;
    public NavDestinationDefinition ExitNavNode;

    protected new void OnDrawGizmos() {
        base.OnDrawGizmos();
        if(Polygons == null) return;
        if(EntryNavNode.PolygonDefinitionID < Polygons.Length) {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(EntryNavNode.Pos.Inflate3rdDim(Polygons[EntryNavNode.PolygonDefinitionID].Height), 1);
        }
        if(ExitNavNode.PolygonDefinitionID < Polygons.Length) {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(ExitNavNode.Pos.Inflate3rdDim(Polygons[ExitNavNode.PolygonDefinitionID].Height), 1);
        }

    }
}
