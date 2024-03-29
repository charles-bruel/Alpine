using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Assertions;

public class LoadingContextV1 {
    public Dictionary<int, NavArea> navAreas = new Dictionary<int, NavArea>();
    public Dictionary<int, INavNode> nodeIds = new Dictionary<int, INavNode>();
    public Dictionary<int, NavLink> linkIds = new Dictionary<int, NavLink>();

    public void LoadNavData(List<NavAreaGraphSaveDataV1> navSaveData) {
        PolygonsController.Instance.RecalculateOverlappingNavAreas();

        // Recalculate simple links for all traversible nav areas
        foreach(NavAreaGraphSaveDataV1 navData in navSaveData) {
            NavArea area = navAreas[navData.ID];
            // Don't do this to slopes, etc.
            // Slopes get provisional links in the next foreach loop
            if((area.Flags & PolygonFlags.FLAT_NAVIGABLE) == 0) continue;
            area.RecalculateSimpleLinks();
        }
        foreach(Building building in BuildingsController.Instance.Buildings) {
            if(building is Slope slope) {
                slope.CreateProvisionalLinks();
            }
        }

        // Determine node mapping
        foreach(NavAreaGraphSaveDataV1 navData in navSaveData) {
            foreach(var nodeData in navData.NodesToIds) {
                NavArea area = navAreas[navData.ID];
                if(nodeData.Value.Item1 == -1) {
                    // Find the service node
                    int serviceNodeId = 0;
                    NavDestination serviceNode = null;
                    foreach(INavNode node in area.Nodes) {
                        if(node is NavDestination destination) {
                            if(serviceNodeId == nodeData.Value.Item2) {
                                serviceNode = destination;
                                break;
                            }
                            serviceNodeId++;
                        }
                    }
                    Assert.IsNotNull(serviceNode);

                    if(nodeIds.ContainsKey(nodeData.Key)) {
                        Assert.AreEqual(nodeIds[nodeData.Key], serviceNode);
                    } else {
                        nodeIds.Add(nodeData.Key, serviceNode);
                    }
                } else {
                    // Find the portal
                    NavPortal portal = null;
                    foreach(INavNode node in area.Nodes) {
                        if (node is NavPortal candidate)
                        {
                            // Portal directionality doesn't matter
                            if ((candidate.A.ID == nodeData.Value.Item1 && candidate.B.ID == nodeData.Value.Item2) ||
                               (candidate.A.ID == nodeData.Value.Item2 && candidate.B.ID == nodeData.Value.Item1))
                            {
                                portal = candidate;
                                break;
                            }
                        }
                    }
                    Assert.IsNotNull(portal);
                    
                    if(nodeIds.ContainsKey(nodeData.Key)) {
                        Assert.AreEqual(nodeIds[nodeData.Key], portal);
                    } else {
                        nodeIds.Add(nodeData.Key, portal);
                    }
                }
            }
        }
        
        // Determine link mapping
        foreach(NavAreaGraphSaveDataV1 navData in navSaveData) {
            // Since explicit links can be in no nav areas, we go from a node
            foreach(var linkData in navData.LinksToIds) {
                NavLink link = null;
                foreach(NavLink candidate in nodeIds[linkData.Value.Item1].GetLinks()) {
                    if(candidate.A == nodeIds[linkData.Value.Item1] && candidate.B == nodeIds[linkData.Value.Item2]) {
                        link = candidate;
                        break;
                    }
                }
                // Skip extra check if we already have it
                if(link != null) goto foundLink;
                foreach(NavLink candidate in nodeIds[linkData.Value.Item2].GetLinks()) {
                    if(candidate.A == nodeIds[linkData.Value.Item1] && candidate.B == nodeIds[linkData.Value.Item2]) {
                        link = candidate;
                        break;
                    }
                }
                foundLink:

                if(link == null) {
                    Debug.LogWarning("Link not found for " + linkData.Value.Item1 + " " + linkData.Value.Item2);
                    continue;
                }

                // Assert.IsNotNull(link);

                if(linkIds.ContainsKey(linkData.Key)) {
                    Assert.AreEqual(linkIds[linkData.Key], link);
                } else {
                    linkIds.Add(linkData.Key, link);
                }
            }
        }
    }
}