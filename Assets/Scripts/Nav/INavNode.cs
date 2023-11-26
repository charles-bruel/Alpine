using System.Collections.Generic;
using UnityEngine;

public interface INavNode {
    public List<NavLink> GetLinks();
    public Vector2 GetPosition();
    public float GetHeight();
}