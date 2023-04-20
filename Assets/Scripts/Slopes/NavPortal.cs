using UnityEngine;

public class NavPortal : MonoBehaviour {
    public LineRenderer Renderer;
    public AlpinePolygon A;
    public AlpinePolygon B;
    public int A1index;
    public int A2index;
    public int B1index;
    public int B2index;
    public float A1Offset;
    public float A2Offset;
    public float B1Offset;
    public float B2Offset;
    public NavPortalDirectionality Directionality;

    public Vector2 A1 {
        get {
            Vector2 prev = A.Polygon.points[A1index];
            Vector2 next;
            if(A1index == A.Polygon.pointCount - 1) {
                next = A.Polygon.points[0];
            } else {
                next = A.Polygon.points[A1index + 1];
            }
            return Vector2.Lerp(prev, next, A1Offset);
        }
    }

    public Vector2 A2 {
        get {
            Vector2 prev = A.Polygon.points[A2index];
            Vector2 next;
            if(A2index == A.Polygon.pointCount - 1) {
                next = A.Polygon.points[0];
            } else {
                next = A.Polygon.points[A2index + 1];
            }
            return Vector2.Lerp(prev, next, A2Offset);
        }
    }

    public Vector2 B1 {
        get {
            Vector2 prev = B.Polygon.points[B1index];
            Vector2 next;
            if(B1index == B.Polygon.pointCount - 1) {
                next = B.Polygon.points[0];
            } else {
                next = B.Polygon.points[B1index + 1];
            }
            return Vector2.Lerp(prev, next, B1Offset);
        }
    }

    public Vector2 B2 {
        get {
            Vector2 prev = B.Polygon.points[B2index];
            Vector2 next;
            if(B2index == B.Polygon.pointCount - 1) {
                next = B.Polygon.points[0];
            } else {
                next = B.Polygon.points[B2index + 1];
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
        Renderer.SetPositions(new Vector3[] {start, end});
    }

    public enum NavPortalDirectionality {
        BIDIRECTIONAL,
        A_TO_B,
        B_TO_A,
    }
}