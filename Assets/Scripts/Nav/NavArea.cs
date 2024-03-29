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

using System.Collections.Generic;
using System;
using UnityEngine;
using System.Runtime.Serialization.Json;

public class NavArea : AlpinePolygon {
    public List<INavNode> Nodes;
    // All links from this area to other overlapping nav areas
    public List<NavLink> Links;
    public List<NavArea> OverlappingNavAreas;
    public bool Modified = false;
    public INavAreaImplementation Implementation;
    public int ID;
    public static int IDStartIndex = 0;

    public NavArea() {
        Nodes = new List<INavNode>();
        Links = new List<NavLink>();
        OverlappingNavAreas = new List<NavArea>();
        ID = IDStartIndex++;
    }

    public void Advance(float delta) {
        if(Implementation == null) Debug.Log(ID);
        Implementation.OnAdvance(delta);
        if(Selected) Implementation.OnAdvanceSelected(delta);
    }

    public List<INavNode> GetAllNavNodes() {
        List<INavNode> toReturn = new List<INavNode>();
        toReturn.AddRange(Nodes);
        foreach(var area in OverlappingNavAreas) {
            toReturn.AddRange(area.Nodes);
        }
        return toReturn;
    }

    // Assume we are a simple nav area, i.e. we don't care about slopes or anything else
    public void RecalculateSimpleLinks() {
        // Create a dictionary of old links
        Dictionary<Tuple<INavNode, INavNode>, NavLink> oldLinks = new Dictionary<Tuple<INavNode, INavNode>, NavLink>();
        foreach(var link in Links) {
            // I've seen an issue once with a error adding a duplicate key, so here's a check
            if(oldLinks.ContainsKey(new Tuple<INavNode, INavNode>(link.A, link.B))) {
                Debug.LogError("Duplicate key in RecalculateSimpleLinks");
                continue;
            }
            
            oldLinks.Add(new Tuple<INavNode, INavNode>(link.A, link.B), link);
        }

        // Get all new links
        List<INavNode> allNavNodes = GetAllNavNodes();
        Links = new List<NavLink>();
        for(int i = 0;i < Nodes.Count;i ++) {
            for(int j = 0;j < allNavNodes.Count;j ++) {
                if(i == j) continue; // Don't path between ourselves

                float dist = (Nodes[i].GetPosition() - allNavNodes[j].GetPosition()).magnitude;
                if(dist == 0) continue;

                // Check if we already had the link
                Tuple<INavNode, INavNode> key = new Tuple<INavNode, INavNode>(Nodes[i], allNavNodes[j]);
                if(oldLinks.ContainsKey(key)) {
                    Links.Add(oldLinks[key]);
                    oldLinks.Remove(key);
                    continue;
                }

                NavLink link = new NavLink {
                    A = Nodes[i],
                    B = allNavNodes[j],
                    Cost = dist * 0.5f,
                    Difficulty = SlopeDifficulty.GREEN,
                    Implementation = new BasicNavLinkImplementation(),
                    Marker = "Inter nav area link in area " + ID + " between " + i + " and " + j,
                };

                Links.Add(link);
            }
        }
        GlobalNavController.MarkGraphDirty();

        foreach(var link in oldLinks.Values) {
            link.Implementation.OnRemove();
        }
    }

    public override void OnSelected() {
        base.OnSelected();
        Implementation.OnSelected();
        foreach(var link in Links) {
            link.Implementation.OnSelected();
        }
    }

    public override void OnDeselected() {
        base.OnDeselected();
        Implementation.OnDeselected();
        foreach(var link in Links) {
            link.Implementation.OnDeselected();
        }
    }

    public override void OnDestroy() {
        base.OnDestroy();
        var nodesCopy = new List<INavNode>(Nodes);
        foreach(var node in nodesCopy) {
            node.Destroy();
        }

        var linksCopy = new List<NavLink>(Links);
        foreach(var link in linksCopy) {
            link.Destroy();
        }

        Dead = true;
    }

    private bool Dead = false;
    public bool IsDead() {
        return Dead;
    }
}