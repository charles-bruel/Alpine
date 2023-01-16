using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LiftConstructionData {
    public Lift Template;
    public List<RoutingSegment> RoutingSegments;
    public List<SpanSegment> SpanSegments;

    [System.Serializable]
    public class RoutingSegment {
        public LiftRoutingSegment.RoutingSegmentType RoutingSegmentType;
        public int TemplateIndex;
        public Vector3 Position;
        public bool HasVerticalPos;
    }

    // Represents the span between two routing segments,
    // and the configuration of towers and such
    [System.Serializable]
    public class SpanSegment {
        public List<TowerSegment> Towers;
        public RoutingSegment Start;
        public RoutingSegment End;
    }

    [System.Serializable]
    public class TowerSegment {
        public int TemplateIndex;
        public Vector3 Position;
    }
}