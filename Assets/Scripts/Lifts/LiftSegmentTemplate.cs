using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiftSegmentTemplate : MonoBehaviour, IPoolable
{
    public float Gauge;
    public Transform CableAimingPoint;
    public Transform UphillCablePoint;
    public Transform DownhillCablePoint;
    public APIDef LiftSegmentAPIDef;
    public APILiftSegment APILiftSegment;

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
}
