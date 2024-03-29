//>============================================================================<
//
//    Alpine, Ski Resort Tycoon Game
//    Copyright (C) 2024  Charles Bruel
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see <https://www.gnu.org/licenses/>.
//>============================================================================<

using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public class LiftConstructionData {
    public LiftTemplate Template;
    public List<RoutingSegment> RoutingSegments;
    public List<SpanSegment> SpanSegments;

    public int SelectedVehicleIndex;
    [NonSerialized]
    public LiftVehicle PhysicalVehicle;

    [System.Serializable]
    public class RoutingSegment {
        public LiftRoutingSegmentTemplate.RoutingSegmentType RoutingSegmentType;
        public int TemplateIndex;
        public Vector3 Position;
        public bool HasVerticalPos;
        [NonSerialized]
        public LiftRoutingSegmentTemplate PhysicalSegment;
        [NonSerialized]
        public float Angle;

        public RoutingSegment Clone() {
            RoutingSegment result = new RoutingSegment();
            
            result.RoutingSegmentType = RoutingSegmentType;
            result.TemplateIndex = TemplateIndex;
            result.Position = Position;
            result.HasVerticalPos = HasVerticalPos;

            return result;
        }
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

        public SpanSegment Clone() {
            SpanSegment result = new SpanSegment();
            
            result.Start = Start;
            result.End = End;
            result.StartPos = StartPos;
            result.EndPos = EndPos;

            result.Towers = new List<TowerSegment>(Towers);
            for(int i = 0;i < Towers.Count;i ++) {
                result.Towers[i] = result.Towers[i].Clone();
            }

            return result;
        }
    }

    [System.Serializable]
    public class TowerSegment {
        public int TemplateIndex;
        public Vector3 Position;
        [NonSerialized]
        public LiftTowerTemplate PhysicalTower;
        [NonSerialized]
        public float Angle;

        public TowerSegment Clone() {
            TowerSegment result = new TowerSegment();

            result.TemplateIndex = TemplateIndex;
            result.Position = Position;

            return result;
        }
    }

    public LiftConstructionData Clone() {
        LiftConstructionData result = new LiftConstructionData();

        result.Template = Template;
        result.SelectedVehicleIndex = SelectedVehicleIndex;

        result.RoutingSegments = new List<RoutingSegment>(RoutingSegments);
        for(int i = 0;i < result.RoutingSegments.Count;i ++) {
            result.RoutingSegments[i] = result.RoutingSegments[i].Clone();
        }

        result.SpanSegments = new List<SpanSegment>(SpanSegments);
        for(int i = 0;i < result.SpanSegments.Count;i ++) {
            result.SpanSegments[i] = result.SpanSegments[i].Clone();
            // We need to update where the Start and End references point
            int startIdx = RoutingSegments.IndexOf(result.SpanSegments[i].Start);
            result.SpanSegments[i].Start = result.RoutingSegments[startIdx];
            int endIdx = RoutingSegments.IndexOf(result.SpanSegments[i].End);
            result.SpanSegments[i].End = result.RoutingSegments[endIdx];
        }

        return result;
    }
}