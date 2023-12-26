using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiftMidStationTemplate : LiftStationTemplate {
    public NavDestinationDefinition ExtraEntryNavNode;
    public NavDestinationDefinition ExtraExitNavNode;

    protected new void OnDrawGizmos() {
        base.OnDrawGizmos();
        if(ExtraEntryNavNode.PolygonDefinitionID < Polygons.Length) {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(ExtraEntryNavNode.Pos.Inflate3rdDim(Polygons[ExtraEntryNavNode.PolygonDefinitionID].Height), 1);
        }
        if(ExtraExitNavNode.PolygonDefinitionID < Polygons.Length) {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(ExtraExitNavNode.Pos.Inflate3rdDim(Polygons[ExtraExitNavNode.PolygonDefinitionID].Height), 1);
        }
    }
}
