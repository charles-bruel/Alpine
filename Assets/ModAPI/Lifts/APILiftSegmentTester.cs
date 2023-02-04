using UnityEngine;
using System.Collections.Generic;

public class APILiftSegmentTester : MonoBehaviour {
    public Transform prev;
    public Transform next;
    public Transform self;
    public Transform cablePointUphill;
    public Transform cablePointDownhill;
    public APIDef APIDef;
    public APILiftSegment Segment;

    void Start() {
        Segment = APIDef.Fetch<APILiftSegment>();
    }

    void Update() {
        Segment.Build(gameObject, self, next, prev);
        List<Vector3> temp = Segment.GetCablePointsDownhill(gameObject, cablePointDownhill);
        for(int i = 0;i < temp.Count - 1;i ++) {
            Debug.DrawLine(temp[i], temp[i + 1], Color.red, 1);
        }
        temp = Segment.GetCablePointsUphill(gameObject, cablePointUphill);
        for(int i = 0;i < temp.Count - 1;i ++) {
            Debug.DrawLine(temp[i], temp[i + 1], Color.red, 1);
        }
    }
}