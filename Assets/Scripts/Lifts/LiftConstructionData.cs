using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LiftConstructionData {
    public LiftTemplate Template;
    public List<RoutingSegment> RoutingSegments;
    public List<SpanSegment> SpanSegments;

    //TODO: Work out multiple vehicles
    public LiftVehicleTemplate SelectedVehicle;

    [System.Serializable]
    public class RoutingSegment {
        public LiftRoutingSegmentTemplate.RoutingSegmentType RoutingSegmentType;
        public int TemplateIndex;
        public Vector3 Position;
        public bool HasVerticalPos;
        public LiftRoutingSegmentTemplate PhysicalSegment;
        public float Angle;
    }

    // Represents the span between two routing segments,
    // and the configuration of towers and such
    [System.Serializable]
    public class SpanSegment {
        public List<TowerSegment> Towers;
        public RoutingSegment Start;
        public RoutingSegment End;
        public Vector2 StartPos;
        public Vector2 EndPos;
    }

    [System.Serializable]
    public class TowerSegment {
        public int TemplateIndex;
        public Vector3 Position;
        public LiftTowerTemplate PhysicalTower;
        public float Angle;
    }
}