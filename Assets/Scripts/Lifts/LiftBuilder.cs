using System;
using System.Collections;
using System.Collections.Generic;
using EPPZ.Geometry.Model;
using Mono.Cecil;
using UnityEngine;
using UnityEngine.Assertions;

public class LiftBuilder
{
    public LiftConstructionData Data;
    
    private Pool<LiftStationTemplate>[] StationPools;
    private Pool<LiftMidStationTemplate>[] MidStationPools;
    private Pool<LiftTurnTemplate>[] TurnPools;
    private Pool<LiftTowerTemplate>[] TowerPools;

    private Lift Result;

    private APITowerPlacer APITowerPlacer;

    private Transform Parent;

    public void Initialize() {
        InitializePools();
        APITowerPlacer = Data.Template.TowerPlacementScript.Fetch<APITowerPlacer>();

        GameObject gameObject = new GameObject("Lift");
        Result = gameObject.AddComponent<Lift>();
        Result.Template = Data.Template;
        Result.Data = Data;
        Result.CreateSubObjects();
        Parent = gameObject.transform;
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

    public void LightBuild() {
        UpdateLine();
    }

    private void UpdateLine() {
        Vector3[] positions = new Vector3[Data.RoutingSegments.Count];

        for(int i = 0;i < Data.RoutingSegments.Count;i ++) {
            positions[i] = Data.RoutingSegments[i].Position;
            positions[i].y = 50;
        }

        Result.Line.positionCount = positions.Length;

        Result.Line.SetPositions(positions);
    }

    public static void BuildFromSave(LiftConstructionData data, NavAreaGraphSaveDataV1[] navData, LoadingContextV1 loadingContext) {
        LiftBuilder builder = new LiftBuilder();
        builder.Data = data;
        builder.Data.PhysicalVehicle = builder.Data.Template.AvaliableLiftVehicles[builder.Data.SelectedVehicleIndex];
        builder.Initialize();
        builder.ConstructRoutingSegments();
        builder.BuildRoutingSegments();
        builder.ConstructSpanSegments();
        builder.BuildTowers();
        builder.Finish();

        Assert.AreEqual(navData.Length, builder.Result.NavAreas.Count);
        for(int i = 0; i < navData.Length;i ++) {
            builder.Result.NavAreas[i].ID = navData[i].ID;
            loadingContext.navAreas.Add(navData[i].ID, builder.Result.NavAreas[i]);
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
    }

    private LiftCablePoint[] CreateCables()
    {
        LiftCableBuilder builder = new LiftCableBuilder();

        for(int i = 0;i < Data.SpanSegments.Count;i ++) {
            LiftRoutingSegmentTemplate routing = Data.SpanSegments[i].Start.PhysicalSegment;
            List<LiftCablePoint> temp = routing.APILiftSegment.GetCablePointsUphill(routing, routing.UphillCablePoint);
            if(i != 0) builder.AddPointsWithSag(builder.LastPoint, temp[0], 1.0001f);
            builder.AddPointsWithoutSag(temp);

            for(int j = 0;j < Data.SpanSegments[i].Towers.Count;j ++) {
                LiftTowerTemplate tower = Data.SpanSegments[i].Towers[j].PhysicalTower;
                List<LiftCablePoint> temp2 = tower.APILiftSegment.GetCablePointsUphill(tower, tower.UphillCablePoint);
                builder.AddPointsWithSag(builder.LastPoint, temp2[0], 1.0001f);
                builder.AddPointsWithoutSag(temp2);
            }

            if(i == Data.SpanSegments.Count - 1) {
                routing = Data.SpanSegments[i].End.PhysicalSegment;
                List<LiftCablePoint> temp3 = routing.APILiftSegment.GetCablePointsDownhill(routing, routing.DownhillCablePoint);
                builder.AddPointsWithSag(builder.LastPoint, temp3[0], 1.0001f);
                builder.AddPointsWithoutSag(temp3);
            }
        }

        for(int i = Data.SpanSegments.Count - 1;i >= 0;i --) {
            LiftRoutingSegmentTemplate routing = Data.SpanSegments[i].End.PhysicalSegment;
            List<LiftCablePoint> temp;
            if(i == Data.SpanSegments.Count - 1) {
                temp = routing.APILiftSegment.GetCablePointsUphill(routing, routing.UphillCablePoint);
            } else {
                temp = routing.APILiftSegment.GetCablePointsDownhill(routing, routing.DownhillCablePoint);
            }
            builder.AddPointsWithSag(builder.LastPoint, temp[0], 1.0001f);
            builder.AddPointsWithoutSag(temp);

            for(int j = Data.SpanSegments[i].Towers.Count - 1;j >= 0;j --) {
                LiftTowerTemplate tower = Data.SpanSegments[i].Towers[j].PhysicalTower;
                List<LiftCablePoint> temp2 = tower.APILiftSegment.GetCablePointsDownhill(tower, tower.DownhillCablePoint);
                builder.AddPointsWithSag(builder.LastPoint, temp2[0], 1.0001f);
                builder.AddPointsWithoutSag(temp2);
            }

            if(i == 0) {
                routing = Data.SpanSegments[i].Start.PhysicalSegment;
                List<LiftCablePoint> temp3 = routing.APILiftSegment.GetCablePointsDownhill(routing, routing.DownhillCablePoint);
                builder.AddPointsWithSag(builder.LastPoint, temp3[0], 1.0001f);
                builder.AddPointsWithoutSag(temp3);
            }
        }

        builder.AddPointsWithSag(builder.LastPoint, builder.Points[0], 1.0001f);

        builder.CreateGameObject(Parent, Data.Template.CableMaterial);
        builder.StartMesh(1);
        builder.BuildMesh(0, new Vector3(), Data.Template.CableThickness);
        builder.FinalizeMesh();

        return builder.Points.ToArray();
    }

    private void FinishAll() {
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
                routingSegment.PhysicalSegment,
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
                    towerSegment.PhysicalTower,
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

                tower.transform.parent = Parent;
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

            float startLength = spanSegment.Start.PhysicalSegment.APILiftRoutingSegment.GetLength(spanSegment.Start.PhysicalSegment);
            float endLength = spanSegment.End.PhysicalSegment.APILiftRoutingSegment.GetLength(spanSegment.End.PhysicalSegment);

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

            obj.transform.parent = Parent;
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

        Data.PhysicalVehicle = Data.Template.AvaliableLiftVehicles[Data.SelectedVehicleIndex];
    }

    private void ResetPools<T>(Pool<T>[] pools) where T : IPoolable {
        for(int i = 0;i < pools.Length;i ++) {
            pools[i].Reset();
        }
    }

    public void Finish() {
        FinishAll();
        Result.CablePoints = CreateCables();
        Result.Footprint = GenerateFootprint();
        RegisterPolygonsAndNav();
        
        BuildingsController.Instance.RegisterBuilding(Result);

        Result.Finish();
    }

    private void RegisterPolygonsAndNav() {
        List<NavArea> navAreas = new List<NavArea>();
        
        List<INavNode> entries = new List<INavNode>();
        List<INavNode> exits = new List<INavNode>();

        for(int i = 0;i < Data.RoutingSegments.Count;i ++) {
            List<AlpinePolygon> polygons = Data.RoutingSegments[i].PhysicalSegment.APILiftSegment.GetPolygons(
                Data.RoutingSegments[i].PhysicalSegment, 
                Data.RoutingSegments[i].PhysicalSegment.Polygons
            );

            // Nav polygons
            for(int j = 0;j < polygons.Count;j ++) {
                // We need to create nav information if it's marked as navigable
                if((polygons[j].Flags & PolygonFlags.NAVIGABLE_MASK) != 0) {
                    NavArea temp = new NavArea();

                    temp.Guid                = polygons[j].Guid;
                    temp.Level               = polygons[j].Level;
                    temp.Polygon             = polygons[j].Polygon;
                    temp.Filter              = polygons[j].Filter;
                    temp.Renderer            = polygons[j].Renderer;
                    temp.Color               = polygons[j].Color;
                    temp.ArbitrarilyEditable = polygons[j].ArbitrarilyEditable;
                    temp.Flags               = polygons[j].Flags;
                    temp.Height              = polygons[j].Height;
                    
                    temp.Owner = Result;

                    navAreas.Add(temp);
                    polygons[j] = temp;
                }

                PolygonsController.Instance.RegisterPolygon(polygons[j]);
            }

            // Entry and exit nodes
            if(Data.RoutingSegments[i].PhysicalSegment is LiftStationTemplate) {
                float parentAngle = Data.RoutingSegments[i].PhysicalSegment.transform.eulerAngles.y;
                Vector2 parentPos = Data.RoutingSegments[i].PhysicalSegment.transform.position.ToHorizontal();

                LiftStationTemplate stationTemplate = Data.RoutingSegments[i].PhysicalSegment as LiftStationTemplate;
                
                NavArea entryArea = polygons[stationTemplate.EntryNavNode.PolygonDefinitionID] as NavArea;
                Assert.IsNotNull(entryArea);
                NavDestination entryNode = new NavDestination {
                    Pos = ModAPIUtils.TransformBuildingCoordinates(stationTemplate.EntryNavNode.Pos, parentAngle, parentPos), 
                    Area = entryArea
                };

                NavArea exitArea = polygons[stationTemplate.ExitNavNode.PolygonDefinitionID] as NavArea;
                Assert.IsNotNull(entryArea);
                NavDestination exitNode = new NavDestination { 
                    Pos = ModAPIUtils.TransformBuildingCoordinates(stationTemplate.ExitNavNode.Pos, parentAngle, parentPos), 
                    Area = exitArea 
                };

                entryArea.Nodes.Add(entryNode);
                exitArea.Nodes.Add(exitNode);

                entries.Add(entryNode);
                exits.Add(exitNode);
            }
        }

        List<NavLink> liftLinks = new List<NavLink>();

        // TODO: Downloading
        for(int i = 0; i < entries.Count;i ++) {
            for(int j = 0; j < exits.Count;j ++) {
                if(i == j) continue; // Don't link the same station
                if(i > j) continue; // Don't go downhill
                
                NavLink link = new NavLink {
                    A = entries[i],
                    B = exits[j],
                    Cost = 1,
                    Difficulty = SlopeDifficulty.GREEN,
                    Implementation = new LiftNavLink(),
                    Marker = "Lift explicit link",
                };

                entries[i].AddExplictNavLink(link);
                exits[j].AddExplictNavLink(link);

                liftLinks.Add(link);
            }
        }

        Result.NavAreas = navAreas;
        Result.NavLinks = liftLinks;
    }

    private AlpinePolygon GenerateFootprint() {
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

        AlpinePolygon poly = new AlpinePolygon();
        poly.Guid = Guid.NewGuid();
        poly.Level = 4;
        poly.Polygon = Polygon.PolygonWithPoints(points);

        poly.Flags = PolygonFlags.AERIAL_CLEARANCE;

        PolygonsController.Instance.RegisterPolygon(poly);

        return poly;
    }

    public void Cancel() {
        GameObject.Destroy(Result.gameObject);
    }
}
