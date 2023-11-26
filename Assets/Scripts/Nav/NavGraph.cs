using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

// A seperate data structure that is used for pathfinding. This contains minimal
// information. It will be constructed from the nav information so pathfinding can
// occur on another thread without sync issues.
public class NavGraph {

    // Call from main thread
    public static NavGraph Build(NavArea area) {
        NavGraph temp = new NavGraph();
        temp.NodesToIdx = new Dictionary<INavNode, uint>();
        temp.EdgesFromNode = new Dictionary<uint, List<Edge>>();
        temp.Add(area);

        return temp;
    }

    // Call from main thread
    public void DrawDebug() {
        foreach(var edges in EdgesFromNode.Values) {
            foreach(var edge in edges) {
                Vector3 pos1 = edge.Ref.A.GetPosition().Inflate3rdDim(1000);
                Vector3 pos2 = edge.Ref.B.GetPosition().Inflate3rdDim(1000);
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
            if(!links.Contains(link)) {
                links.Add(link);

                // We "explore" both sides. The side we are coming from has to have
                // the node already in the list, so it doesn't introduce infinite recursion
                if(!nodes.Contains(link.A)) {
                    nodes.Add(link.A);
                    ExploreRecursive(nodes, links, link.A);
                }
                if(!nodes.Contains(link.B)) {
                    nodes.Add(link.B);
                    ExploreRecursive(nodes, links, link.B);
                }
            }
        }
    }

    private Dictionary<INavNode, uint> NodesToIdx;
    private Dictionary<uint, List<Edge>> EdgesFromNode;

    public INavNode GetRandomNode() {
        return Enumerable.ToList(NodesToIdx.Keys)[UnityEngine.Random.Range(0, NodesToIdx.Count)]; 
    }

    // Can be called from any thread
    public List<NavLink> Dijkstras(INavNode start, List<INavNode> end, SlopeDifficulty Ability) {
        List<uint> idTargets = new List<uint>(end.Count);
        foreach(var node in end) {
            idTargets.Add(NodesToIdx[node]);
        }
        return Dijkstras(NodesToIdx[start], idTargets, Ability);
    }
    
    // Can be called from any thread
    public List<NavLink> Dijkstras(uint start, List<uint> end, SlopeDifficulty Ability) {
        Assert.AreNotEqual(end.Count, 0);
        PriorityQueue<uint, float> openSet = new PriorityQueue<uint, float>();
        Dictionary<uint, Tuple<uint, NavLink>> cameFrom = new Dictionary<uint, Tuple<uint, NavLink>>();
        Dictionary<uint, float> costs = new Dictionary<uint, float>();
        List<uint> Visited = new List<uint>();

        costs[start] = 0;
        openSet.Enqueue(start, 0);
        while(openSet.Count > 0) {
            uint current = openSet.Dequeue();
            if(Visited.Contains(current)) continue;

            if(end.Contains(current)) {
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

        Debug.Log("Couldn't find path from " + start + " to " + end);

        throw new System.NotImplementedException();
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

    public struct Edge {
        public uint Target;
        public float Cost;
        public SlopeDifficulty Difficulty;
        public NavLink Ref;
    }
}