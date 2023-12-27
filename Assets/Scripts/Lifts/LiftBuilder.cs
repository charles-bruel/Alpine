using System;
using System.Collections;
using System.Collections.Generic;
using Codice.CM.Common.Tree;
using EPPZ.Geometry.Model;
using Mono.Cecil;
using PlasticGui.WorkspaceWindow.Items;
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

    private Tuple<LiftCablePoint[], List<LiftAccessPointIntermediate>> CreateCables() {
        // TODO: Populate this
        List<LiftAccessPointIntermediate> exchangePoints = new List<LiftAccessPointIntermediate>();

        LiftCableBuilder builder = new LiftCableBuilder();

        //  /----+----\
        // +     t    x
        //  \----x----/
        // Clockwise direction of motion
        // +'s are caugh on the uphill run
        // x's are caught on the downhill run
        // (t) midstation
        
        for(int i = 0;i < Data.SpanSegments.Count;i ++) {
            LiftRoutingSegmentTemplate routing = Data.SpanSegments[i].Start.PhysicalSegment;

            // Lift access point(s)
            LiftRoutingSegmentType type = i == 0 ? LiftRoutingSegmentType.FIRST : LiftRoutingSegmentType.MIDDLE;
            LiftPathAccessDefinition[] definitions = routing.APILiftRoutingSegment.GetPathAccess(type);
            foreach(LiftPathAccessDefinition definition in definitions) {
                if(definition.Side == LiftPathAccessDefinition.Direction.DOWNHILL) {
                    Assert.IsFalse(type == LiftRoutingSegmentType.FIRST, "First segment cannot have downhill access points");
                    continue;
                }
                LiftAccessPointIntermediate point = new LiftAccessPointIntermediate
                {
                    Pos = builder.Points.Count + definition.Pos,
                    ID = i,
                    Entry = definition.Entry,
                    Exit = definition.Exit
                };
                exchangePoints.Add(point);
            }

            // Cable
            List<LiftCablePoint> temp = routing.APILiftSegment.GetCablePointsUphill(routing, routing.UphillCablePoint);
            if(i != 0) builder.AddPointsWithSag(builder.LastPoint, temp[0], 1.0001f);
            builder.AddPointsWithoutSag(temp);

            // Span to the next routing segment
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

            LiftRoutingSegmentType type = i == Data.SpanSegments.Count - 1 ? LiftRoutingSegmentType.LAST : LiftRoutingSegmentType.MIDDLE;
            LiftPathAccessDefinition[] definitions = routing.APILiftRoutingSegment.GetPathAccess(type);
            foreach(LiftPathAccessDefinition definition in definitions) {
                if(definition.Side == LiftPathAccessDefinition.Direction.UPHILL) {
                    Assert.IsFalse(type == LiftRoutingSegmentType.LAST, "Last segment cannot have uphill access points");
                    continue;
                }
                LiftAccessPointIntermediate point = new LiftAccessPointIntermediate
                {
                    Pos = builder.Points.Count + definition.Pos,
                    // e.g. A lift with two midstations (CW direction)
                    // goal:
                    // /-1-2-\
                    // 0     3
                    // \-5-4-/
                    // i:
                    // 0 1 2 3
                    // 
                    // j is the ID
                    // j=n*2-i
                    // 5=3*2
                    ID = Data.SpanSegments.Count * 2 - (i + 1),
                    Entry = definition.Entry,
                    Exit = definition.Exit
                };
                exchangePoints.Add(point);
            }

            // Cable
            List<LiftCablePoint> temp;
            if(type == LiftRoutingSegmentType.LAST) {
                temp = routing.APILiftSegment.GetCablePointsUphill(routing, routing.UphillCablePoint);
            } else {
                temp = routing.APILiftSegment.GetCablePointsDownhill(routing, routing.DownhillCablePoint);
            }
            builder.AddPointsWithSag(builder.LastPoint, temp[0], 1.0001f);
            builder.AddPointsWithoutSag(temp);

            // Span to the next routing segment
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

        return new Tuple<LiftCablePoint[], List<LiftAccessPointIntermediate>>(builder.Points.ToArray(), exchangePoints);
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

        Tuple<LiftCablePoint[], List<LiftAccessPointIntermediate>> createCablesResult = CreateCables();
        Result.CablePoints = createCablesResult.Item1;

        Result.Footprint = GenerateFootprint();
        Tuple<List<INavNode>, List<INavNode>> nodes = RegisterPolygonsAndNav(createCablesResult.Item2);

        Result.CableJoins = CreateCableJoins(createCablesResult.Item2, nodes);

        BuildingsController.Instance.RegisterBuilding(Result);

        Result.Finish();
    }

    private List<LiftVehicleSystem.LiftAccessNode> CreateCableJoins(List<LiftAccessPointIntermediate> data, Tuple<List<INavNode>, List<INavNode>> nodes) {
        List<LiftVehicleSystem.LiftAccessNode> result = new List<LiftVehicleSystem.LiftAccessNode>(data.Count);

        foreach(LiftAccessPointIntermediate point in data) {
            LiftVehicleSystem.LiftAccessNode node = new LiftVehicleSystem.LiftAccessNode();
            node.Index = point.Pos;
            if(point.Entry) {
                node.Entry = nodes.Item1[point.ID];
            }
            if(point.Exit) {
                node.Exit = nodes.Item2[point.ID];
            }

            result.Add(node);
        }

        return result;
    }

    private Tuple<List<INavNode>, List<INavNode>> RegisterPolygonsAndNav(List<LiftAccessPointIntermediate> liftAccessPoints) {
        List<NavArea> navAreas = new List<NavArea>();
        
        List<INavNode> entries = new List<INavNode>();
        List<INavNode> exits = new List<INavNode>();

        List<INavNode> reverseEntries = new List<INavNode>();
        List<INavNode> reverseExits = new List<INavNode>();

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
                Assert.IsNotNull(exitArea);
                NavDestination exitNode = new NavDestination { 
                    Pos = ModAPIUtils.TransformBuildingCoordinates(stationTemplate.ExitNavNode.Pos, parentAngle, parentPos), 
                    Area = exitArea 
                };

                entryArea.Nodes.Add(entryNode);
                exitArea.Nodes.Add(exitNode);

                entries.Add(entryNode);
                exits.Add(exitNode);

            }
            // Furthermore, if it is a midstation segment, we add the reverse nodes
            if(Data.RoutingSegments[i].PhysicalSegment is LiftMidStationTemplate) {
                float parentAngle = Data.RoutingSegments[i].PhysicalSegment.transform.eulerAngles.y;
                Vector2 parentPos = Data.RoutingSegments[i].PhysicalSegment.transform.position.ToHorizontal();

                LiftMidStationTemplate stationTemplate = Data.RoutingSegments[i].PhysicalSegment as LiftMidStationTemplate;
                
                NavArea entryArea = polygons[stationTemplate.ExtraEntryNavNode.PolygonDefinitionID] as NavArea;
                Assert.IsNotNull(entryArea);
                NavDestination entryNode = new NavDestination {
                    Pos = ModAPIUtils.TransformBuildingCoordinates(stationTemplate.ExtraEntryNavNode.Pos, parentAngle, parentPos), 
                    Area = entryArea
                };

                NavArea exitArea = polygons[stationTemplate.ExtraExitNavNode.PolygonDefinitionID] as NavArea;
                Assert.IsNotNull(exitArea);
                NavDestination exitNode = new NavDestination { 
                    Pos = ModAPIUtils.TransformBuildingCoordinates(stationTemplate.ExtraExitNavNode.Pos, parentAngle, parentPos), 
                    Area = exitArea 
                };

                entryArea.Nodes.Add(entryNode);
                exitArea.Nodes.Add(exitNode);

                reverseEntries.Add(entryNode);
                reverseExits.Add(exitNode);
            }
        }

        // To create the final list, we need to reverse the reverse nodes and add them to the main list
        reverseEntries.Reverse();
        reverseExits.Reverse();
        entries.AddRange(reverseEntries);
        exits.AddRange(reverseExits);
        // TODO: This will *NOT* work with turn segments, as it is expected the indices of the list entries to
        // be synced to the IDs. To make turns work (or any other segment that doesn't add nodes), we need to
        // insert null dummy entries when not inserting nodes in the above loop
        Assert.AreEqual(entries.Count, exits.Count);
        Assert.AreEqual(entries.Count, Data.SpanSegments.Count * 2);

        // Now we determine which nodes have associated lift access data and therefore should get Lift links
        bool[] validEntries = new bool[entries.Count];
        bool[] validExits = new bool[exits.Count];
        for(int i = 0;i < liftAccessPoints.Count;i ++) {
            if(liftAccessPoints[i].Entry) {
                validEntries[liftAccessPoints[i].ID] = true;
            }
            if(liftAccessPoints[i].Exit) {
                validExits[liftAccessPoints[i].ID] = true;
            }
        }

        List<NavLink> liftLinks = new List<NavLink>();

        for(int i = 0; i < entries.Count;i ++) {
            if(!validEntries[i]) continue;
            for(int j = 0; j < exits.Count;j ++) {
                if(i == j) continue; // Don't link the same station
                if(!validExits[j]) continue;

                float dist = 0;
                int mindex = i; 
                int maxdex = j;
                if(i > j) {
                    mindex = j;
                    maxdex = i;
                }
                for(int k = mindex;k < maxdex;k ++) {
                    dist += (Data.RoutingSegments[k].Position - Data.RoutingSegments[k + 1].Position).magnitude;
                }
                
                NavLink link = new NavLink {
                    A = entries[i],
                    B = exits[j],
                    Cost = 100 * dist / Data.Template.MaxSpeed,
                    Difficulty = SlopeDifficulty.GREEN,
                    Implementation = new LiftNavLinkImplementation(),
                    Marker = "Lift explicit link: " + i + " to " + j,
                };

                entries[i].AddExplictNavLink(link);
                exits[j].AddExplictNavLink(link);

                liftLinks.Add(link);
            }
        }

        Result.NavAreas = navAreas;
        Result.NavLinks = liftLinks;

        return new Tuple<List<INavNode>, List<INavNode>>(entries, exits);
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

    public struct LiftAccessPointIntermediate {
        public int Pos;
        public int ID;
        public bool Entry;
        public bool Exit;
    }
}
