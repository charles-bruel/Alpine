using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiftBuilder
{
    public LiftConstructionData Data;
    
    private Pool[] StationPools;
    private Pool[] MidStationPools;
    private Pool[] TurnPools;
    private Pool[] TowerPools;

    public void Initialize() {
        InitializePools();
    }

    private void InitializePools() {
        StationPools = new Pool[Data.Template.AvaliableStations.Length];
        for(int i = 0;i < StationPools.Length;i ++) {
            StationPools[i] = new Pool();
            StationPools[i].Template = Data.Template.AvaliableStations[i].gameObject;
        }
        MidStationPools = new Pool[Data.Template.AvaliableMidStations.Length];
        for(int i = 0;i < MidStationPools.Length;i ++) {
            MidStationPools[i] = new Pool();
            MidStationPools[i].Template = Data.Template.AvaliableMidStations[i].gameObject;
        }
        TurnPools = new Pool[Data.Template.AvaliableTurns.Length];
        for(int i = 0;i < TurnPools.Length;i ++) {
            TurnPools[i] = new Pool();
            TurnPools[i].Template = Data.Template.AvaliableTurns[i].gameObject;
        }
        TowerPools = new Pool[Data.Template.AvaliableTowers.Length];
        for(int i = 0;i < TowerPools.Length;i ++) {
            TowerPools[i] = new Pool();
            TowerPools[i].Template = Data.Template.AvaliableTowers[i].gameObject;
        }
    }

    public void Build() {
        Reset();
        ConstructRoutingSegments();
        GenerateSpanSegments();
        PlaceTowers();
        ConstructSpanSegments();
    }

    private void PlaceTowers() {        
        for(int i = 0;i < Data.SpanSegments.Count;i ++) {
            LiftConstructionData.SpanSegment spanSegment = Data.SpanSegments[i];
            Vector2 start = spanSegment.Start.Position.ToHorizontal();
            Vector2 end = spanSegment.End.Position.ToHorizontal();

            //TODO: Replace with proper algorithm
            int numTowers = (int) ((start - end).magnitude / 100);

            for(int j = 1;j < numTowers - 1;j ++) {
                LiftConstructionData.TowerSegment tower = new LiftConstructionData.TowerSegment();

                //TOOD: Select tower type
                tower.TemplateIndex = 0;
                tower.Position = TerrainManager.Instance.Project(Vector2.Lerp(start, end, ((float)j/(numTowers-1))));

                spanSegment.Towers.Add(tower);
            }
        }            
    }

    private void ConstructSpanSegments() {
        for(int i = 0;i < Data.SpanSegments.Count;i ++) {
            LiftConstructionData.SpanSegment segment = Data.SpanSegments[i];

            Vector2 temp = (segment.Start.Position - segment.End.Position).ToHorizontal();
            float angle = Mathf.Atan2(temp.y, temp.x);
            angle = -angle * Mathf.Rad2Deg - 90;
            for(int j = 0;j < segment.Towers.Count;j ++) {
                Pool pool = TowerPools[segment.Towers[j].TemplateIndex];

                GameObject obj = pool.Instantiate();
                obj.transform.position = segment.Towers[j].Position;
                obj.transform.rotation =  Quaternion.Euler(0, angle, 0);
            }
        }
    }

    private void GenerateSpanSegments() {
        Data.SpanSegments.Clear();
        for(int i = 0;i < Data.RoutingSegments.Count - 1;i ++) {
            LiftConstructionData.SpanSegment spanSegment = new LiftConstructionData.SpanSegment();
            spanSegment.Start = Data.RoutingSegments[i];
            spanSegment.End = Data.RoutingSegments[i + 1];
            spanSegment.Towers = new List<LiftConstructionData.TowerSegment>();

            Data.SpanSegments.Add(spanSegment);
        }
    }

    private void ConstructRoutingSegments() {
        for(int i = 0;i < Data.RoutingSegments.Count;i ++) {
            LiftConstructionData.RoutingSegment segment = Data.RoutingSegments[i];
            if(!segment.HasVerticalPos) {
                segment.Position = TerrainManager.Instance.Project(segment.Position.ToHorizontal());
            }

            Pool[] pool = StationPools;
            switch(segment.RoutingSegmentType) {
                case LiftRoutingSegment.RoutingSegmentType.STATION:
                    pool = StationPools;
                    break;
                case LiftRoutingSegment.RoutingSegmentType.MIDSTATION:
                    pool = MidStationPools;
                    break;
                case LiftRoutingSegment.RoutingSegmentType.TURN:
                    pool = TurnPools;
                    break;
            }

            //Determine 2d angle
            float angle;
            if (i == 0) {
                //First station, so we point directly at the next routing segment
                Vector2 temp = (segment.Position - Data.RoutingSegments[i + 1].Position).ToHorizontal();
                angle = Mathf.Atan2(temp.y, temp.x);
            } else if(i == Data.RoutingSegments.Count - 1) {
                //Last station, so we point directly at the next routing segment
                Vector2 temp = (Data.RoutingSegments[i - 1].Position - segment.Position).ToHorizontal();
                angle = Mathf.Atan2(temp.y, temp.x);
            } else {
                Vector2 temp = (Data.RoutingSegments[i - 1].Position - Data.RoutingSegments[i + 1].Position).ToHorizontal();
                angle = Mathf.Atan2(temp.y, temp.x);
            }

            GameObject obj = pool[segment.TemplateIndex].Instantiate();
            obj.transform.position = segment.Position;
            obj.transform.rotation =  Quaternion.Euler(0, -angle * Mathf.Rad2Deg - 90, 0);
        }
    }

    private void Reset() {
        ResetPools(StationPools);
        ResetPools(MidStationPools);
        ResetPools(TurnPools);
        ResetPools(TowerPools);
    }

    private void ResetPools(Pool[] pools) {
        for(int i = 0;i < pools.Length;i ++) {
            pools[i].Reset();
        }
    }

    public void Finish() {

    }
}
