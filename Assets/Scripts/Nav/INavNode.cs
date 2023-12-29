using System;
using System.Collections.Generic;
using UnityEngine;

public interface INavNode {
    public List<NavLink> GetLinks();
    public List<NavLink> GetExplicitLinksForSerialization();
    public void AddExplictNavLink(NavLink link);
    public void RemoveExplicitNavLink(NavLink link);
    public Vector2 GetPosition();
    public Vector3 GetPosition3d() {
        return GetPosition().Inflate3rdDim(GetHeight());
    }
    public float GetHeight();
    public void Destroy();
    public bool IsDead();
}