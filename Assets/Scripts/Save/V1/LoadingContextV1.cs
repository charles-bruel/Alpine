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
                    // Find the functionality node
                    int functionalityNodeId = 0;
                    NavDestination functionalityNode = null;
                    foreach(INavNode node in area.Nodes) {
                        if(node is NavDestination destination) {
                            if(functionalityNodeId == nodeData.Value.Item2) {
                                functionalityNode = destination;
                                break;
                            }
                            functionalityNodeId++;
                        }
                    }
                    Assert.IsNotNull(functionalityNode);

                    if(nodeIds.ContainsKey(nodeData.Key)) {
                        Assert.AreEqual(nodeIds[nodeData.Key], functionalityNode);
                    } else {
                        nodeIds.Add(nodeData.Key, functionalityNode);
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