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
