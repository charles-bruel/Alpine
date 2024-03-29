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

[System.Serializable]
public struct LiftSaveDataV1 {
    public string TemplateName;
    public List<RoutingSegmentV1> RoutingSegments;
    public List<SpanSegmentV1> SpanSegments;
    public int SelectedVehicleIndex;
    public NavAreaGraphSaveDataV1[] NavAreaGraphs;
    public LiftVehicleSystemSaveDataV1 LiftVehicleSystem;

    public static LiftSaveDataV1 FromLift(Lift lift, SavingContextV1 context) {
        LiftSaveDataV1 result = FromConstructionData(lift.Data);
        result.NavAreaGraphs = new NavAreaGraphSaveDataV1[lift.NavAreas.Count];

        for(int i = 0;i < lift.NavAreas.Count;i ++) {
            result.NavAreaGraphs[i] = NavAreaGraphSaveDataV1.FromNavArea(lift.NavAreas[i], context);
        }

        result.LiftVehicleSystem = LiftVehicleSystemSaveDataV1.FromLiftVehicleSystem(lift.VehicleSystem);

        return result;
    }

    // Note - still need to grab the nav area graph data
    public static LiftSaveDataV1 FromConstructionData(LiftConstructionData data) {
        // It's pretty much a 1:1 translation
        // Assumes data is good
        LiftSaveDataV1 result = new LiftSaveDataV1();
        result.TemplateName = data.Template.name;
        result.SelectedVehicleIndex = data.SelectedVehicleIndex;

        result.RoutingSegments = new List<RoutingSegmentV1>();
        foreach(var segment in data.RoutingSegments) {
            RoutingSegmentV1 routingSegment = new RoutingSegmentV1
            {
                RoutingSegmentType = segment.RoutingSegmentType,
                TemplateIndex = segment.TemplateIndex,
                Position = segment.Position
            };
            result.RoutingSegments.Add(routingSegment);
        }

        result.SpanSegments = new List<SpanSegmentV1>();
        foreach(var segment in data.SpanSegments) {
            int StartIndex = data.RoutingSegments.IndexOf(segment.Start);
            int EndIndex = data.RoutingSegments.IndexOf(segment.End);
            SpanSegmentV1 spanSegment = new SpanSegmentV1
            {
                StartIndex = StartIndex,
                EndIndex = EndIndex,
                StartPos = segment.StartPos,
                EndPos = segment.EndPos,
                Towers = new List<TowerSegmentV1>()
            };
            foreach(var tower in segment.Towers) {
                TowerSegmentV1 towerSegment = new TowerSegmentV1
                {
                    TemplateIndex = tower.TemplateIndex,
                    Position = tower.Position
                };
                spanSegment.Towers.Add(towerSegment);
            }
            result.SpanSegments.Add(spanSegment);
        }

        return result;
    }

    // Note - still need to use the nav area graph data
    public LiftConstructionData ToConstructionData() {
        LiftConstructionData result = new LiftConstructionData();
        result.Template = BuildingsController.Instance.GetLiftTemplate(TemplateName);
        result.SelectedVehicleIndex = SelectedVehicleIndex;

        result.RoutingSegments = new List<LiftConstructionData.RoutingSegment>();
        foreach(var segment in RoutingSegments) {
            LiftConstructionData.RoutingSegment routingSegment = new LiftConstructionData.RoutingSegment
            {
                RoutingSegmentType = segment.RoutingSegmentType,
                TemplateIndex = segment.TemplateIndex,
                Position = segment.Position,
                HasVerticalPos = true,
            };
            result.RoutingSegments.Add(routingSegment);
        }

        result.SpanSegments = new List<LiftConstructionData.SpanSegment>();
        foreach(var segment in SpanSegments) {
            LiftConstructionData.RoutingSegment Start = result.RoutingSegments[segment.StartIndex];
            LiftConstructionData.RoutingSegment End = result.RoutingSegments[segment.EndIndex];
            LiftConstructionData.SpanSegment spanSegment = new LiftConstructionData.SpanSegment
            {
                Start = Start,
                End = End,
                StartPos = segment.StartPos,
                EndPos = segment.EndPos,
                Towers = new List<LiftConstructionData.TowerSegment>()
            };
            foreach(var tower in segment.Towers) {
                LiftConstructionData.TowerSegment towerSegment = new LiftConstructionData.TowerSegment
                {
                    TemplateIndex = tower.TemplateIndex,
                    Position = tower.Position
                };
                spanSegment.Towers.Add(towerSegment);
            }
            result.SpanSegments.Add(spanSegment);
        }

        return result;
    }

    [System.Serializable]
    public class RoutingSegmentV1 {
        public LiftRoutingSegmentTemplate.RoutingSegmentType RoutingSegmentType;
        public int TemplateIndex;
        public Vector3POD Position;

        public RoutingSegmentV1 Clone() {
            RoutingSegmentV1 result = new RoutingSegmentV1();
            
            result.RoutingSegmentType = RoutingSegmentType;
            result.TemplateIndex = TemplateIndex;
            result.Position = Position;

            return result;
        }
    }

    // Represents the span between two routing segments,
    // and the configuration of towers and such
    [System.Serializable]
    public class SpanSegmentV1 {
        public List<TowerSegmentV1> Towers;
        public int StartIndex;
        public int EndIndex;
        public Vector2POD StartPos;
        public Vector2POD EndPos;

        public SpanSegmentV1 Clone() {
            SpanSegmentV1 result = new SpanSegmentV1();
            
            result.StartIndex = StartIndex;
            result.EndIndex = EndIndex;
            result.StartPos = StartPos;
            result.EndPos = EndPos;

            result.Towers = new List<TowerSegmentV1>(Towers);
            for(int i = 0;i < Towers.Count;i ++) {
                result.Towers[i] = result.Towers[i].Clone();
            }

            return result;
        }
    }

    [System.Serializable]
    public class TowerSegmentV1 {
        public int TemplateIndex;
        public Vector3POD Position;

        public TowerSegmentV1 Clone() {
            TowerSegmentV1 result = new TowerSegmentV1();

            result.TemplateIndex = TemplateIndex;
            result.Position = Position;

            return result;
        }
    }
}