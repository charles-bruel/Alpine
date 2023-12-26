using UnityEngine;
using System.Collections.Generic;
using System;

public class NavArea : AlpinePolygon {
    public List<INavNode> Nodes;
    // All links from this area to other overlapping nav areas
    public List<NavLink> Links;
    public List<NavArea> OverlappingNavAreas;
    public Building Owner;
    public bool Modified = false;
    public INavAreaImplementation Implementation;
    public int ID;
    public static int IDStartIndex = 0;

    private bool SelectedLast = false;
    public NavArea() {
        Nodes = new List<INavNode>();
        Links = new List<NavLink>();
        OverlappingNavAreas = new List<NavArea>();
        ID = IDStartIndex++;
    }

    public void Advance(float delta) {
        if(Selected && !SelectedLast){
            Implementation.OnSelected();
            foreach(var link in Links) {
                link.Implementation.OnSelected();
            }
        }
        if(!Selected && SelectedLast){
            Implementation.OnDeselected();
            foreach(var link in Links) {
                link.Implementation.OnDeselected();
            }
        }
        Implementation.OnAdvance(delta);
        if(Selected) Implementation.OnAdvanceSelected(delta);
        SelectedLast = Selected;
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
}