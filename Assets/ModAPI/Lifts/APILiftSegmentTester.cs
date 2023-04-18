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
    public ICustomScriptable parent;

    void Start() {
        Segment = APIDef.Fetch<APILiftSegment>();
    }

    void Update() {
        Segment.Build(parent, self, next, prev);
        List<LiftCablePoint> temp = Segment.GetCablePointsDownhill(parent, cablePointDownhill);
        for(int i = 0;i < temp.Count - 1;i ++) {
            Debug.DrawLine(temp[i].pos, temp[i + 1].pos, Color.red, 1);
        }
        temp = Segment.GetCablePointsUphill(parent, cablePointUphill);
        for(int i = 0;i < temp.Count - 1;i ++) {
            Debug.DrawLine(temp[i].pos, temp[i + 1].pos, Color.red, 1);
        }
    }
}