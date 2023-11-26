using UnityEngine;
using System.Collections.Generic;

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
}