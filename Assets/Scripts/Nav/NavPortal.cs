using System.Collections.Generic;
using UnityEngine;

public class NavPortal : INavNode {
    public LineRenderer Renderer;
    public NavArea A;
    public NavArea B;
    public int A1Index;
    public int A2Index;
    public int B1Index;
    public int B2Index;
    public float A1Offset;
    public float A2Offset;
    public float B1Offset;
    public float B2Offset;
    // The height at the center of the portal
    public float Height;
    public NavPortalDirectionality Directionality;
    public GameObject gameObject;
    public List<NavLink> ExplicitNavLinks = new List<NavLink>();

    public Vector2 A1 {
        get {
            Vector2 prev = A.Polygon.points[A1Index];
            Vector2 next;
            if(A1Index == A.Polygon.pointCount - 1) {
                next = A.Polygon.points[0];
            } else {
                next = A.Polygon.points[A1Index + 1];
            }
            return Vector2.Lerp(prev, next, A1Offset);
        }
    }

    public Vector2 A2 {
        get {
            Vector2 prev = A.Polygon.points[A2Index];
            Vector2 next;
            if(A2Index == A.Polygon.pointCount - 1) {
                next = A.Polygon.points[0];
            } else {
                next = A.Polygon.points[A2Index + 1];
            }
            return Vector2.Lerp(prev, next, A2Offset);
        }
    }

    public Vector2 B1 {
        get {
            Vector2 prev = B.Polygon.points[B1Index];
            Vector2 next;
            if(B1Index == B.Polygon.pointCount - 1) {
                next = B.Polygon.points[0];
            } else {
                next = B.Polygon.points[B1Index + 1];
            }
            return Vector2.Lerp(prev, next, B1Offset);
        }
    }

    public Vector2 B2 {
        get {
            Vector2 prev = B.Polygon.points[B2Index];
            Vector2 next;
            if(B2Index == B.Polygon.pointCount - 1) {
                next = B.Polygon.points[0];
            } else {
                next = B.Polygon.points[B2Index + 1];
            }
            return Vector2.Lerp(prev, next, B2Offset);
        }
    }

    public void AddExplictNavLink(NavLink link)
    {
        ExplicitNavLinks.Add(link);
        GlobalNavController.MarkGraphDirty();
    }

    public void RemoveExplicitNavLink(NavLink link)
    {
        ExplicitNavLinks.Remove(link);
        GlobalNavController.MarkGraphDirty();
    }

    public float GetHeight()
    {
        return Height;
    }

    public List<NavLink> GetLinks()
    {
        List<NavLink> toReturn = new List<NavLink>();
        foreach(var link in A.Links) {
            if(link.A == this || link.B == this) {
                toReturn.Add(link);
            }
        }
        foreach(var link in B.Links) {
            if(link.A == this || link.B == this) {
                toReturn.Add(link);
            }
        }
        toReturn.AddRange(ExplicitNavLinks);
        return toReturn;
    }

    public Vector2 GetPosition() {
        return (A1 + A2) / 2;
    }

    public void Inflate() {
        Vector3 start = new Vector3(A1.x, 10, A1.y);
        Vector3 end = new Vector3(A2.x, 10, A2.y);

        Renderer = gameObject.AddComponent<LineRenderer>();
        Renderer.startColor = Renderer.endColor = RenderingData.Instance.PortalColor;
        Renderer.startWidth = Renderer.endWidth = 5;
        Renderer.material = RenderingData.Instance.VertexColorMaterial;
        Renderer.SetPositions(new Vector3[] {start, end});
    }

    public List<NavLink> GetExplicitLinksForSerialization() {
        List<NavLink> toReturn = new List<NavLink>();

        foreach(var link in ExplicitNavLinks) {
            if(link.A == this) {
                toReturn.Add(link);
            }
        }

        return toReturn;
    }

    private bool Dead = false;

    public void Destroy() {
        A.Nodes.Remove(this);
        B.Nodes.Remove(this);

        A.Modified = true;
        B.Modified = true;

        GameObject.Destroy(gameObject);

        Dead = true;
    }

    public bool IsDead() {
        return Dead;
    }

    public enum NavPortalDirectionality {
        BIDIRECTIONAL,
        A_TO_B,
        B_TO_A,
    }
}