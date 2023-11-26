using System.Collections.Generic;
using UnityEngine;

public interface INavNode {
    public List<NavLink> GetLinks();
    public void AddExplictNavLink(NavLink link);
    public void RemoveExplicitNavLink(NavLink link);
    public Vector2 GetPosition();
    public float GetHeight();
}