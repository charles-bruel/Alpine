using UnityEngine;

public class NavPortal {
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

    public void Inflate() {
        Vector3 start = new Vector3(A1.x, 10, A1.y);
        Vector3 end = new Vector3(A2.x, 10, A2.y);

        Renderer = gameObject.AddComponent<LineRenderer>();
        Renderer.startColor = Renderer.endColor = RenderingData.Instance.PortalColor;
        Renderer.startWidth = Renderer.endWidth = 5;
        Renderer.material = RenderingData.Instance.VertexColorMaterial;
        Renderer.SetPositions(new Vector3[] {start, end});
    }

    public enum NavPortalDirectionality {
        BIDIRECTIONAL,
        A_TO_B,
        B_TO_A,
    }
}