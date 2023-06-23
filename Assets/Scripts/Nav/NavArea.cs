using UnityEngine;
using System.Collections.Generic;

public class NavArea : AlpinePolygon {
    public List<NavPortal> Portals;
    public List<NavLink> Links;
    public Building Owner;
    public bool Modified = false;
    public INavAreaImplementation Implementation;

    private bool SelectedLast = false;
    public NavArea() {
        Portals = new List<NavPortal>();
        Links = new List<NavLink>();
    }

    public void Advance(float delta) {
        if(Selected && !SelectedLast) Implementation.OnSelected();
        if(!Selected && SelectedLast) Implementation.OnDeselected();
        Implementation.OnAdvance(delta);
        if(Selected) Implementation.OnAdvanceSelected(delta);
        SelectedLast = Selected;
    }
}