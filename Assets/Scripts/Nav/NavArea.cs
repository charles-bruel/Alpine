using UnityEngine;
using System.Collections.Generic;
using System;

public class NavArea : AlpinePolygon {
    public List<INavNode> Nodes;
    public List<NavLink> Links;
    public Building Owner;
    public bool Modified = false;
    public INavAreaImplementation Implementation;

    private bool SelectedLast = false;
    public NavArea() {
        Nodes = new List<INavNode>();
        Links = new List<NavLink>();
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

    // Assume we are a simple nav area, i.e. we don't care about slopes or anything else
    public void RecalculateSimpleLinks()
    {
        Links = new List<NavLink>();
        for(int i = 0;i < Nodes.Count;i ++) {
            for(int j = 0;j < Nodes.Count;j ++) {
                if(i == j) continue; // Don't path between ourselves
                if(j > i) continue; // Only looking at unique pair

                float dist = (Nodes[i].GetPosition() - Nodes[j].GetPosition()).magnitude;

                NavLink linkA = new NavLink {
                    A = Nodes[i],
                    B = Nodes[j],
                    Cost = dist * 0.5f,
                    Difficulty = SlopeDifficulty.GREEN,
                    Implementation = new BasicNavLinkImplementation(),
                };
                NavLink linkB = new NavLink {
                    B = Nodes[i],
                    A = Nodes[j],
                    Cost = dist * 0.5f,
                    Difficulty = SlopeDifficulty.GREEN,
                    Implementation = new BasicNavLinkImplementation(),
                };

                Links.Add(linkA);
                Links.Add(linkB);
            }
        }
        GlobalNavController.MarkGraphDirty();
    }
}