using UnityEngine;
using System.Collections.Generic;

public class LiftBuilderTool : ITool {
    private bool done = false;

    public LiftConstructionData Data;
    public LiftBuilder Builder;
    public LiftBuilderToolGrab GrabTemplate;
    public List<LiftBuilderToolGrab> Grabs = new List<LiftBuilderToolGrab>();
    public Canvas Canvas;

    public bool Require2D() {
        return true;
    }

    public void Cancel() {
        Builder.Build();
        Builder.Finish();
        for(int i = 0;i < Grabs.Count;i ++) {
            GameObject.Destroy(Grabs[i]);
        }
    }

    public bool IsDone() {
        return done;
    }

    public void Start() {
        Builder = new LiftBuilder();
        Builder.Data = Data;
        Builder.Initialize();

        for(int i = 0;i < Data.RoutingSegments.Count;i ++) {
            LiftBuilderToolGrab grab = GameObject.Instantiate(GrabTemplate);
            grab.transform.SetParent(Canvas.transform, false);
            grab.RectTransform.anchoredPosition = Data.RoutingSegments[i].Position.ToHorizontal();
            grab.RoutingSegmentIndex = i;
            grab.Data = Data;
            Grabs.Add(grab);
        }
    }

    public void Update() {
        Builder.LightBuild();
    }
}