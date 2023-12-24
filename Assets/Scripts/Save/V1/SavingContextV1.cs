using System;
using System.Collections.Generic;

// DO NOT SERIALIZE THIS
public class SavingContextV1 {
    public Dictionary<INavNode, int> nodeIds;
    public Dictionary<NavLink, int> linkIds;

    public void Populate() {
        NavGraph basis = NavGraph.CreateCompleteGraph();
        nodeIds = new Dictionary<INavNode, int>();
        linkIds = new Dictionary<NavLink, int>();

        foreach(INavNode node in basis.GetAllNodesInGraph()) {
            nodeIds.Add(node, nodeIds.Count);
        }
        foreach(NavLink link in basis.GetAllLinksInGraph()) {
            linkIds.Add(link, linkIds.Count);
        }
    }
}