using System;
using System.Collections;
using System.Collections.Generic;
using EPPZ.Geometry.Model;
using UnityEngine;

public class LiftBuilder
{
    public LiftConstructionData Data;
    
    private Pool<LiftStation>[] StationPools;
    private Pool<LiftMidStation>[] MidStationPools;
    private Pool<LiftTurn>[] TurnPools;
    private Pool<LiftTower>[] TowerPools;

    public void Initialize() {
        InitializePools();
    }

    private void InitializePools() {
        StationPools = new Pool<LiftStation>[Data.Template.AvaliableStations.Length];
        for(int i = 0;i < StationPools.Length;i ++) {
            StationPools[i] = new Pool<LiftStation>();
            StationPools[i].Template = Data.Template.AvaliableStations[i];
        }
        MidStationPools = new Pool<LiftMidStation>[Data.Template.AvaliableMidStations.Length];
        for(int i = 0;i < MidStationPools.Length;i ++) {
            MidStationPools[i] = new Pool<LiftMidStation>();
            MidStationPools[i].Template = Data.Template.AvaliableMidStations[i];
        }
        TurnPools = new Pool<LiftTurn>[Data.Template.AvaliableTurns.Length];
        for(int i = 0;i < TurnPools.Length;i ++) {
            TurnPools[i] = new Pool<LiftTurn>();
            TurnPools[i].Template = Data.Template.AvaliableTurns[i];
        }
        TowerPools = new Pool<LiftTower>[Data.Template.AvaliableTowers.Length];
        for(int i = 0;i < TowerPools.Length;i ++) {
            TowerPools[i] = new Pool<LiftTower>();
            TowerPools[i].Template = Data.Template.AvaliableTowers[i];
        }
    }

    public void Build() {
        Reset();
        ConstructRoutingSegments();
        GenerateSpanSegments();
        PlaceTowers();
        ConstructSpanSegments();
        BuildAllSegments();
    }

    private void BuildAllSegments() {
        for(int i = 0;i < Data.RoutingSegments.Count;i ++) {
            LiftConstructionData.RoutingSegment routingSegment = Data.RoutingSegments[i];
            Transform prev = null, next = null;
            
            if(i != 0) {
                prev = Data.RoutingSegments[i - 1].PhysicalSegment.CableAimingPoint;
            }

            if(i != Data.RoutingSegments.Count - 1) {
                next = Data.RoutingSegments[i + 1].PhysicalSegment.CableAimingPoint;
            }

            routingSegment.PhysicalSegment.APILiftSegment.Build(
                routingSegment.PhysicalSegment.gameObject,
                prev,
                routingSegment.PhysicalSegment.CableAimingPoint,
                next
            );
        }

        for(int i = 0;i < Data.SpanSegments.Count;i ++) {
            for(int j = 0;j < Data.SpanSegments[i].Towers.Count;j ++) {
                LiftConstructionData.TowerSegment towerSegment = Data.SpanSegments[i].Towers[j];

                Transform prev, next;
                if(j == 0) {
                    prev = Data.SpanSegments[i].Start.PhysicalSegment.CableAimingPoint;
                } else {
                    prev = Data.SpanSegments[i].Towers[j - 1].PhysicalTower.CableAimingPoint;
                }

                if(j == Data.SpanSegments[i].Towers.Count - 1) {
                    next = Data.SpanSegments[i].End.PhysicalSegment.CableAimingPoint;
                } else {
                    next = Data.SpanSegments[i].Towers[j + 1].PhysicalTower.CableAimingPoint;
                }

                towerSegment.PhysicalTower.APILiftSegment.Build(
                    towerSegment.PhysicalTower.gameObject,
                    prev,
                    towerSegment.PhysicalTower.CableAimingPoint,
                    next
                );
            }
        }
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
                Pool<LiftTower> pool = TowerPools[segment.Towers[j].TemplateIndex];

                LiftTower tower = pool.Instantiate();
                tower.transform.position = segment.Towers[j].Position;
                tower.transform.rotation =  Quaternion.Euler(0, angle, 0);
                segment.Towers[j].PhysicalTower = tower;
                segment.Towers[j].Angle = angle;
            }
        }
    }

    private void GenerateSpanSegments() {
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

            LiftRoutingSegment routingSegment = null;
            switch(segment.RoutingSegmentType) {
                case LiftRoutingSegment.RoutingSegmentType.STATION:
                    routingSegment = StationPools[segment.TemplateIndex].Instantiate();
                    break;
                case LiftRoutingSegment.RoutingSegmentType.MIDSTATION:
                    routingSegment = MidStationPools[segment.TemplateIndex].Instantiate();
                    break;
                case LiftRoutingSegment.RoutingSegmentType.TURN:
                    routingSegment = TurnPools[segment.TemplateIndex].Instantiate();
                    break;
            }
            GameObject obj = routingSegment.gameObject;

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

            angle = -angle * Mathf.Rad2Deg - 90;

            obj.transform.position = segment.Position;
            obj.transform.rotation =  Quaternion.Euler(0, angle, 0);

            segment.PhysicalSegment = routingSegment;
            segment.Angle = angle;
        }
    }

    private void Reset() {
        ResetConstructionData();

        ResetPools(StationPools);
        ResetPools(MidStationPools);
        ResetPools(TurnPools);
        ResetPools(TowerPools);
    }

    private void ResetConstructionData() {
        Data.SpanSegments.Clear();
        for(int i = 0;i < Data.RoutingSegments.Count;i ++) {
            Data.RoutingSegments[i].PhysicalSegment = null;
        }
    }

    private void ResetPools<T>(Pool<T>[] pools) where T : IPoolable {
        for(int i = 0;i < pools.Length;i ++) {
            pools[i].Reset();
        }
    }

    public void Finish() {
        var temp = GenerateFootprint();
        RemoveTrees(temp);
    }

    private PolygonsController.AlpinePolygon GenerateFootprint() {
        float gaugeExpansion = 3;

        List<Vector2> pointsLeft = new List<Vector2>();
        List<Vector2> pointsRight = new List<Vector2>();
        for(int i = 0;i < Data.RoutingSegments.Count;i ++) {
            {
                Vector2 pos = Data.RoutingSegments[i].Position.ToHorizontal();
                float angle = -Data.RoutingSegments[i].Angle * Mathf.Deg2Rad;
                Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                pointsLeft.Add(pos + dir * (Data.RoutingSegments[i].PhysicalSegment.Gauge + gaugeExpansion));
                pointsRight.Add(pos - dir * (Data.RoutingSegments[i].PhysicalSegment.Gauge + gaugeExpansion));
            }

            if(i < Data.SpanSegments.Count) {
                for(int j = 0;j < Data.SpanSegments[i].Towers.Count;j ++) {
                    Vector2 pos = Data.SpanSegments[i].Towers[j].Position.ToHorizontal();
                    float angle = -Data.SpanSegments[i].Towers[j].Angle * Mathf.Deg2Rad;
                    Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                    pointsLeft.Add(pos + dir * (Data.SpanSegments[i].Towers[j].PhysicalTower.Gauge + gaugeExpansion));
                    pointsRight.Add(pos - dir * (Data.SpanSegments[i].Towers[j].PhysicalTower.Gauge + gaugeExpansion));
                }
            }
        }

        pointsRight.Reverse();
        pointsLeft.AddRange(pointsRight);
        Vector2[] points = pointsLeft.ToArray();

        PolygonsController.AlpinePolygon poly = new PolygonsController.AlpinePolygon();
        poly.Guid = Guid.NewGuid();
        poly.Level = 4;
        poly.Polygon = Polygon.PolygonWithPoints(points);
        poly.Color = new Color(1, 0.75f, 0.25f);

        PolygonsController.Instance.RegisterPolygon(poly);

        return poly;
    }

    private void RemoveTrees(PolygonsController.AlpinePolygon temp) {
        Utils.RemoveTreesByPolygon(temp.Polygon);
    }
}
