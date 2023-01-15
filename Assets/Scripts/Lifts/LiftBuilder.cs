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
        ConstructSpanSegments();
        FillSpanSegments();
    }

    private void FillSpanSegments()
    {
        //TODO
    }

    private void ConstructSpanSegments()
    {
        //TODO
    }

    private void ConstructRoutingSegments()
    {
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

            GameObject obj = pool[segment.TemplateIndex].Instantiate();
            obj.transform.position = segment.Position;
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
