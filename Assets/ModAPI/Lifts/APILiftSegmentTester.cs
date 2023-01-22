using UnityEngine;

public class APILiftSegmentTester : MonoBehaviour {
    public Transform prev;
    public Transform next;
    public Transform self;
    public APIDef APIDef;
    public APILiftSegment Segment;

    void Start() {
        Segment = APIDef.Fetch<APILiftSegment>();
    }

    void Update() {
        Segment.Build(gameObject, self, next, prev);
    }
}