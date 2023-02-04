using System;
using System.Collections;
using System.Collections.Generic;
using EPPZ.Geometry.Model;
using UnityEngine;

public class LiftBuilder
{
    public LiftConstructionData Data;
    
    private Pool<LiftStationTemplate>[] StationPools;
    private Pool<LiftMidStationTemplate>[] MidStationPools;
    private Pool<LiftTurnTemplate>[] TurnPools;
    private Pool<LiftTowerTemplate>[] TowerPools;

    private APITowerPlacer APITowerPlacer;

    private Transform parent;

    public void Initialize() {
        InitializePools();
        APITowerPlacer = Data.Template.TowerPlacementScript.Fetch<APITowerPlacer>();

        GameObject gameObject = new GameObject("Lift");
        parent = gameObject.transform;
    }

    private void InitializePools() {
        StationPools = new Pool<LiftStationTemplate>[Data.Template.AvaliableStations.Length];
        for(int i = 0;i < StationPools.Length;i ++) {
            StationPools[i] = new Pool<LiftStationTemplate>();
            StationPools[i].Template = Data.Template.AvaliableStations[i];
        }
        MidStationPools = new Pool<LiftMidStationTemplate>[Data.Template.AvaliableMidStations.Length];
        for(int i = 0;i < MidStationPools.Length;i ++) {
            MidStationPools[i] = new Pool<LiftMidStationTemplate>();
            MidStationPools[i].Template = Data.Template.AvaliableMidStations[i];
        }
        TurnPools = new Pool<LiftTurnTemplate>[Data.Template.AvaliableTurns.Length];
        for(int i = 0;i < TurnPools.Length;i ++) {
            TurnPools[i] = new Pool<LiftTurnTemplate>();
            TurnPools[i].Template = Data.Template.AvaliableTurns[i];
        }
        TowerPools = new Pool<LiftTowerTemplate>[Data.Template.AvaliableTowers.Length];
        for(int i = 0;i < TowerPools.Length;i ++) {
            TowerPools[i] = new Pool<LiftTowerTemplate>();
            TowerPools[i].Template = Data.Template.AvaliableTowers[i];
        }
    }

    public void Build() {
        Reset();
        ConstructRoutingSegments();
        BuildRoutingSegments();
        GenerateSpanSegments();
        PlaceTowers();
        ConstructSpanSegments();
        BuildTowers();
        FinishAll();
    }

    private void CreateCables()
    {
        LiftCableBuilder builder = new LiftCableBuilder();

        for(int i = 0;i < Data.SpanSegments.Count;i ++) {
            LiftRoutingSegmentTemplate routing = Data.SpanSegments[i].Start.PhysicalSegment;
            List<Vector3> temp = routing.APILiftSegment.GetCablePointsUphill(routing.gameObject, routing.UphillCablePoint);
            if(i != 0) builder.AddPointsWithSag(builder.LastPoint, temp[0], 1.0001f);
            builder.AddPointsWithoutSag(temp);

            for(int j = 0;j < Data.SpanSegments[i].Towers.Count;j ++) {
                LiftTowerTemplate tower = Data.SpanSegments[i].Towers[j].PhysicalTower;
                List<Vector3> temp2 = tower.APILiftSegment.GetCablePointsUphill(tower.gameObject, tower.UphillCablePoint);
                builder.AddPointsWithSag(builder.LastPoint, temp2[0], 1.0001f);
                builder.AddPointsWithoutSag(temp2);
            }

            if(i == Data.SpanSegments.Count - 1) {
                routing = Data.SpanSegments[i].End.PhysicalSegment;
                List<Vector3> temp3 = routing.APILiftSegment.GetCablePointsDownhill(routing.gameObject, routing.DownhillCablePoint);
                builder.AddPointsWithSag(builder.LastPoint, temp3[0], 1.0001f);
                builder.AddPointsWithoutSag(temp3);
            }
        }

        for(int i = Data.SpanSegments.Count - 1;i >= 0;i --) {
            LiftRoutingSegmentTemplate routing = Data.SpanSegments[i].End.PhysicalSegment;
            List<Vector3> temp;
            if(i == Data.SpanSegments.Count - 1) {
                temp = routing.APILiftSegment.GetCablePointsUphill(routing.gameObject, routing.UphillCablePoint);
            } else {
                temp = routing.APILiftSegment.GetCablePointsDownhill(routing.gameObject, routing.DownhillCablePoint);
            }
            builder.AddPointsWithSag(builder.LastPoint, temp[0], 1.0001f);
            builder.AddPointsWithoutSag(temp);

            for(int j = Data.SpanSegments[i].Towers.Count - 1;j >= 0;j --) {
                LiftTowerTemplate tower = Data.SpanSegments[i].Towers[j].PhysicalTower;
                List<Vector3> temp2 = tower.APILiftSegment.GetCablePointsDownhill(tower.gameObject, tower.DownhillCablePoint);
                builder.AddPointsWithSag(builder.LastPoint, temp2[0], 1.0001f);
                builder.AddPointsWithoutSag(temp2);
            }

            if(i == 0) {
                routing = Data.SpanSegments[i].Start.PhysicalSegment;
                List<Vector3> temp3 = routing.APILiftSegment.GetCablePointsDownhill(routing.gameObject, routing.DownhillCablePoint);
                builder.AddPointsWithSag(builder.LastPoint, temp3[0], 1.0001f);
                builder.AddPointsWithoutSag(temp3);
            }
        }

        builder.AddPointsWithSag(builder.LastPoint, builder.Points[0], 1.0001f);

        builder.CreateGameObject(parent, Data.Template.CableMaterial);
        builder.StartMesh(1);
        builder.BuildMesh(0, new Vector3(), Data.Template.CableThickness);
        builder.FinalizeMesh();
    }

    private void FinishAll()
    {
        for(int i = 0;i < Data.RoutingSegments.Count;i ++) {
            LiftConstructionData.RoutingSegment routingSegment = Data.RoutingSegments[i];
            routingSegment.PhysicalSegment.APILiftSegment.Finish();
        }
    }

    private void BuildRoutingSegments() {
        for(int i = 0;i < Data.RoutingSegments.Count;i ++) {
            LiftConstructionData.RoutingSegment routingSegment = Data.RoutingSegments[i];
            Transform prev = null, next = null;
            
            //TODO: Work out these (towers cant be targets because of order)

            if(i != 0) {
                prev = Data.RoutingSegments[i - 1].PhysicalSegment.CableAimingPoint;
            }

            if(i != Data.RoutingSegments.Count - 1) {
                next = Data.RoutingSegments[i + 1].PhysicalSegment.CableAimingPoint;
            }

            routingSegment.PhysicalSegment.APILiftSegment.Build(
                routingSegment.PhysicalSegment.gameObject,
                routingSegment.PhysicalSegment.CableAimingPoint,
                next,
                prev
            );
        }
    }

    private void BuildTowers() {
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
                    towerSegment.PhysicalTower.CableAimingPoint,
                    next,
                    prev
                );
            }
        }
    }

    private void PlaceTowers() {        
        for(int i = 0;i < Data.SpanSegments.Count;i ++) {
            LiftConstructionData.SpanSegment spanSegment = Data.SpanSegments[i];
            Vector2 start = spanSegment.StartPos;
            Vector2 end = spanSegment.EndPos;

            int num = (int)(start - end).magnitude;
            List<Vector3> terrainPosses = new List<Vector3>(num);
            for(int j = 0;j < num;j ++) {
                Vector2 pos2d = Vector2.Lerp(start, end, ((float)j+1) / (num + 1));
                Vector3 pos3d = TerrainManager.Instance.Project(pos2d);
                terrainPosses.Add(pos3d);
            }

            List<Vector3> towerPosses = APITowerPlacer.PlaceTowers(
                terrainPosses, 
                spanSegment.Start.PhysicalSegment.CableAimingPoint.position,
                spanSegment.End.PhysicalSegment.CableAimingPoint.position
            );

            for(int j = 0;j < towerPosses.Count;j ++) {
                LiftConstructionData.TowerSegment tower = new LiftConstructionData.TowerSegment();

                //TOOD: Select tower type
                tower.TemplateIndex = 0;
                tower.Position = towerPosses[j];

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
                Pool<LiftTowerTemplate> pool = TowerPools[segment.Towers[j].TemplateIndex];

                LiftTowerTemplate tower = pool.Instantiate();

                tower.transform.parent = parent;
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

            Vector2 directionVector = (spanSegment.End.Position - spanSegment.Start.Position).ToHorizontal().normalized;

            float startLength = spanSegment.Start.PhysicalSegment.APILiftRoutingSegment.GetLength();
            float endLength = spanSegment.End.PhysicalSegment.APILiftRoutingSegment.GetLength();

            spanSegment.StartPos = spanSegment.Start.Position.ToHorizontal() + directionVector * startLength;
            spanSegment.EndPos = spanSegment.End.Position.ToHorizontal() - directionVector * endLength;

            Data.SpanSegments.Add(spanSegment);
        }
    }

    private void ConstructRoutingSegments() {
        for(int i = 0;i < Data.RoutingSegments.Count;i ++) {
            LiftConstructionData.RoutingSegment segment = Data.RoutingSegments[i];
            if(!segment.HasVerticalPos) {
                segment.Position = TerrainManager.Instance.Project(segment.Position.ToHorizontal());
            }

            LiftRoutingSegmentTemplate routingSegment = null;
            switch(segment.RoutingSegmentType) {
                case LiftRoutingSegmentTemplate.RoutingSegmentType.STATION:
                    routingSegment = StationPools[segment.TemplateIndex].Instantiate();
                    break;
                case LiftRoutingSegmentTemplate.RoutingSegmentType.MIDSTATION:
                    routingSegment = MidStationPools[segment.TemplateIndex].Instantiate();
                    break;
                case LiftRoutingSegmentTemplate.RoutingSegmentType.TURN:
                    routingSegment = TurnPools[segment.TemplateIndex].Instantiate();
                    break;
            }
            GameObject obj = routingSegment.gameObject;

            //Determine 2d angle
            float angle;
            if (i == 0) {
                //First station, so we point directly at the next routing segment
                Vector2 temp = (segment.Position - Data.RoutingSegments[i + 1].Position).ToHorizontal();
                angle = Mathf.Atan2(temp.y, temp.x) + Mathf.PI;
            } else if(i == Data.RoutingSegments.Count - 1) {
                //Last station, so we point directly at the next routing segment
                Vector2 temp = (Data.RoutingSegments[i - 1].Position - segment.Position).ToHorizontal();
                angle = Mathf.Atan2(temp.y, temp.x);
            } else {
                Vector2 temp = (Data.RoutingSegments[i - 1].Position - Data.RoutingSegments[i + 1].Position).ToHorizontal();
                angle = Mathf.Atan2(temp.y, temp.x);
            }

            angle = -angle * Mathf.Rad2Deg - 90;

            obj.transform.parent = parent;
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
        CreateCables();
        var temp = GenerateFootprint();
        RemoveTrees(temp);
    }

    private PolygonsController.AlpinePolygon GenerateFootprint() {
        float gaugeExpansion = 8;

        List<Vector2> pointsLeft = new List<Vector2>();
        List<Vector2> pointsRight = new List<Vector2>();
        for(int i = 0;i < Data.RoutingSegments.Count;i ++) {
            {
                Vector2 pos = Data.RoutingSegments[i].Position.ToHorizontal();
                float angle = -Data.RoutingSegments[i].Angle * Mathf.Deg2Rad;
                if(i == 0) angle += Mathf.PI;
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
