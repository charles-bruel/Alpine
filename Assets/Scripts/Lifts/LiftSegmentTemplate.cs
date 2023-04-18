using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiftSegmentTemplate : MonoBehaviour, IPoolable, ICustomScriptable
{
    public float Gauge;
    public Transform CableAimingPoint;
    public Transform UphillCablePoint;
    public Transform DownhillCablePoint;
    public AlpinePolygonSource[] Polygons;
    public APIDef LiftSegmentAPIDef;
    public APILiftSegment APILiftSegment;
    public Dictionary<string, object> CustomScriptPersistentData = new Dictionary<string, object>();

    private void Initialize() {
        if(APILiftSegment != null) return;

        APILiftSegment = LiftSegmentAPIDef.Fetch<APILiftSegment>();
    }

    public virtual IPoolable Clone() {
        Initialize();
        var temp = GameObject.Instantiate(this);
        temp.APILiftSegment = APILiftSegment;
        return temp;
    }

    public void Destroy() {
        GameObject.Destroy(this);
    }

    public void Disable() {
        gameObject.SetActive(false);
    }

    public void Enable() {
        gameObject.SetActive(true);
    }

    void OnDrawGizmos() {
        if(Polygons == null) return;
        for(int i = 0;i < Polygons.Length;i ++) { 
            AlpinePolygonSource poly = Polygons[i];
            Gizmos.color = PolygonsController.ColorFromFlags(poly.Flags);
            for(int j = 1;j < poly.Points.Length;j ++) {
                Gizmos.DrawLine(
                    new Vector3(poly.Points[j - 1].x, poly.Height, poly.Points[j - 1].y),
                    new Vector3(poly.Points[j].x, poly.Height, poly.Points[j].y)
                );
            }
            int final = poly.Points.Length - 1;
            Gizmos.DrawLine(
                new Vector3(poly.Points[final].x, poly.Height, poly.Points[final].y),
                new Vector3(poly.Points[0].x, poly.Height, poly.Points[0].y)
            );
        }
    }

    public Dictionary<string, object> PersistentData()
    {
        return CustomScriptPersistentData;
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }
}
