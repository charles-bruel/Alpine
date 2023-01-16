using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiftSegment : MonoBehaviour, IPoolable
{
    public Transform CableAimingPoint;
    public APIDef LiftSegmentAPIDef;
    public APILiftSegment APILiftSegment;

    private bool Initialized = false;

    private void Initialize() {
        if(Initialized) return;
        Initialized = true;

        APILiftSegment = LiftSegmentAPIDef.Fetch<APILiftSegment>();
    }

    public IPoolable Clone() {
        Initialize();
        var temp = GameObject.Instantiate(this);
        temp.APILiftSegment = APILiftSegment;
        temp.Initialized = true;
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
