// Contains data about the IDs use to link things within the save together

// Nav nodes and links can be recreated in any order, and we need to be able to assign links IDs reliably
// Each nav area gets a consistent ID, and dictionary containing save-specific small IDs of all associated nodes and links

// The node dictionary maps from (area, area) in the case of portals, and (-1, index) for service nodes. *This assumes service
// node indices are consistent across instances of the game*, which should be accurate since service nodes

// The links dictionary maps from (node, node), where the node IDs are the local ones defined in the save data.

using System;
using System.Collections.Generic;

[System.Serializable]
public struct NavAreaGraphSaveDataV1 {
    public int ID;
    public Dictionary<int, Tuple<int, int>> NodesToIds;
    public Dictionary<int, Tuple<int, int>> LinksToIds;

    public static NavAreaGraphSaveDataV1 FromNavArea(NavArea area, SavingContextV1 context) {
        NavAreaGraphSaveDataV1 result = new NavAreaGraphSaveDataV1();
        result.ID = area.ID;
        result.NodesToIds = new Dictionary<int, Tuple<int, int>>();
        result.LinksToIds = new Dictionary<int, Tuple<int, int>>();

        int serviceNodeId = 0;
        foreach(var node in area.Nodes) {
            if(node is NavPortal) {
                NavPortal portal = (NavPortal)node;
                result.NodesToIds.Add(context.nodeIds[node], new Tuple<int, int>(portal.A.ID, portal.B.ID));
            } else if(node is NavDestination) {
                NavDestination destination = (NavDestination)node;
                result.NodesToIds.Add(context.nodeIds[node], new Tuple<int, int>(-1, serviceNodeId++));
            } else {
                throw new Exception("Unknown node type");
            }
        }

        foreach(var link in area.Links) {
            result.LinksToIds.Add(context.linkIds[link], new Tuple<int, int>(context.nodeIds[link.A], context.nodeIds[link.B]));
        }

        return result;
    }
}