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
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

// A seperate data structure that is used for pathfinding. This contains minimal
// information. It will be constructed from the nav information so pathfinding can
// occur on another thread without sync issues.
public class NavGraph {
    public NavGraph() {
        NodesToIdx = new Dictionary<INavNode, uint>();
        EdgesFromNode = new Dictionary<uint, List<Edge>>();
    }

    public static NavGraph CreateCompleteGraph() {
        List<NavArea> temp = new List<NavArea>();

        foreach(var poly in PolygonsController.Instance.PolygonObjects) {
            if(poly is NavArea) {
                temp.Add(poly as NavArea);
            }
        }

        if(temp.Count == 0) { return new NavGraph(); }
        
        NavGraph result = Build(temp[0]);
        for (int i = 1; i < temp.Count; i++) {
            result.Add(temp[i]);
        }

        return result;
    }

    // Call from main thread
    public static NavGraph Build(NavArea area) {
        NavGraph temp = new NavGraph();
        temp.Add(area);

        return temp;
    }

    // Call from main thread
    public void DrawDebug() {
        foreach(var edges in EdgesFromNode.Values) {
            foreach(var edge in edges) {
                if(edge.Ref.Implementation is SlopeNavLinkImplentation) continue;
                Vector3 pos1 = edge.Ref.A.GetPosition().Inflate3rdDim(1000);
                Vector3 pos2 = edge.Ref.B.GetPosition().Inflate3rdDim(1000);
                if(pos1 == pos2) continue;
                Utils.DebugDrawArrow(pos1, pos2 - pos1, Color.black, (pos2-pos1).magnitude * 0.1f);
            }
        }
    }

    // Call from main thread
    public void Add(NavArea area) {
        // We add all reachable nodes by recursively exploring, and store all NavLinks
        // that we run into.
        // Then we turn those NavLinks into Edges
        
        // Populate flattened lists of nodes and edges
        List<INavNode> nodes = new List<INavNode>();
        List<NavLink> links = new List<NavLink>();
        foreach(var node in area.Nodes) {
            if(node.IsDead()) continue;
            ExploreRecursive(nodes, links, node);
        }

        // Populate NodesToIdx and EdgesFromNode
        for(uint i = 0;i < nodes.Count;i ++) {
            if(!NodesToIdx.ContainsKey(nodes[(int) i])) {
                NodesToIdx[nodes[(int) i]] = i;
                EdgesFromNode[i] = new List<Edge>(); 
            }
        }

        // Add edges to EdgesFromNode
        foreach(var link in links) {
            uint startNode = NodesToIdx[link.A];
            foreach(var existingLink in EdgesFromNode[startNode]) {
                if(existingLink.Ref == link) {
                    // C# needs labelled continues
                    goto continue_outer_loop;
                }
            }

            // We have confirmed that this edge is not already in the graph
            Edge toAdd = new Edge {
                Target = NodesToIdx[link.B],
                Cost = link.Cost,
                Difficulty = link.Difficulty,
                Ref = link
            };

            EdgesFromNode[startNode].Add(toAdd);

            continue_outer_loop: continue;
        }
    }

    private void ExploreRecursive(List<INavNode> nodes, List<NavLink> links, INavNode current) {
        foreach(var link in current.GetLinks()) {
            if(link.IsDead()) continue;
            if(!links.Contains(link)) {
                links.Add(link);

                // We "explore" both sides. The side we are coming from has to have
                // the node already in the list, so it doesn't introduce infinite recursion
                if(!nodes.Contains(link.A) && !link.A.IsDead()) {
                    nodes.Add(link.A);
                    ExploreRecursive(nodes, links, link.A);
                }
                if(!nodes.Contains(link.B) && !link.B.IsDead()) {
                    nodes.Add(link.B);
                    ExploreRecursive(nodes, links, link.B);
                }
            }
        }
    }

    private Dictionary<INavNode, uint> NodesToIdx;
    private Dictionary<uint, List<Edge>> EdgesFromNode;

    public uint NodeID(INavNode navNode) {
        return NodesToIdx[navNode];
    }

    public List<INavNode> GetAllNodesInGraph() {
        return Enumerable.ToList(NodesToIdx.Keys);
    }

    public List<NavLink> GetAllLinksInGraph() {
        List<NavLink> result = new List<NavLink>();
        foreach(var edges in EdgesFromNode.Values) {
            foreach(var edge in edges) {
                result.Add(edge.Ref);
            }
        }
        return result;
    }

    public INavNode GetRandomNode() {
        var list = Enumerable.ToList(NodesToIdx.Keys);
        var rand = new System.Random();
        return list[rand.Next(0, list.Count)]; 
    }

    // Can be called from any thread
    public List<NavLink> Dijkstras(INavNode start, List<INavNode> end, SlopeDifficulty Ability) {
        List<uint> idTargets = new List<uint>(end.Count);
        foreach(var node in end) {
            if(NodesToIdx.ContainsKey(node)) {
                idTargets.Add(NodesToIdx[node]);
            }
            // TODO: Fail condition?
        }
        if(idTargets.Count == 0) return null;

        return Dijkstras(NodesToIdx[start], idTargets, Ability);
    }
    
    // Can be called from any thread
    public List<NavLink> Dijkstras(uint start, List<uint> end, SlopeDifficulty Ability) {
        Assert.AreNotEqual(end.Count, 0);
        return Dijkstras(start, (uint id, float cost) => end.Contains(id), Ability);
    }

    public List<NavLink> Dijkstras(uint start, Func<uint, float, bool> endCondition, SlopeDifficulty Ability) {
        PriorityQueue<uint, float> openSet = new PriorityQueue<uint, float>();
        Dictionary<uint, Tuple<uint, NavLink>> cameFrom = new Dictionary<uint, Tuple<uint, NavLink>>();
        Dictionary<uint, float> costs = new Dictionary<uint, float>();
        List<uint> Visited = new List<uint>();

        costs[start] = 0;
        openSet.Enqueue(start, 0);
        while(openSet.Count > 0) {
            uint current = openSet.Dequeue();
            if(Visited.Contains(current)) continue;

            if(endCondition(current, costs[current])) {
                return ReconstructPath(cameFrom, current);
            }

            List<Edge> edges = EdgesFromNode[current];
            foreach(var edge in edges) {
                if(edge.Difficulty > Ability) continue;
                if(Visited.Contains(edge.Target)) continue;

                float totalCost = costs[current] + edge.Cost;
                if(totalCost < costs.GetValueOrDefault(edge.Target, Mathf.Infinity)) {
                    cameFrom[edge.Target] = new Tuple<uint, NavLink>(current, edge.Ref);
                    costs[edge.Target] = totalCost;

                    openSet.Enqueue(edge.Target, totalCost);
                }
            }
            Visited.Add(current);
        }

        return null;
    }

    private List<NavLink> ReconstructPath(Dictionary<uint, Tuple<uint, NavLink>> cameFrom, uint current) {
        List<NavLink> totalPath = new List<NavLink>();
        while(cameFrom.ContainsKey(current)) {
            Tuple<uint, NavLink> cameFromEntry = cameFrom[current];
            current = cameFromEntry.Item1;
            totalPath.Add(cameFromEntry.Item2);
        }
        totalPath.Reverse();
        return totalPath;
    }

    public bool IsEmpty() {
        return NodesToIdx == null || NodesToIdx.Count == 0;
    }

    public struct Edge {
        public uint Target;
        public float Cost;
        public SlopeDifficulty Difficulty;
        public NavLink Ref;
    }
}