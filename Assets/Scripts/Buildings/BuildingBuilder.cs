using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.Assertions;

public class BuildingBuilder {
    public Vector2 Pos;
    public SimpleBuildingTemplate Template;
    private SimpleBuildingTemplate Instaniated;
    private SimpleBuilding Result;

    public void LightBuild() {

    }

    public void Initialize() {

    }

    public void Build() {
        Instaniated = GameObject.Instantiate(Template);
        Instaniated.transform.position = TerrainManager.Instance.Project(Pos);
        Result = Instaniated.gameObject.AddComponent<SimpleBuilding>();
        Result.Functionality = Instaniated.Functionality;
        Result.Functionality.Building = Result;
    }

    public void Cancel() {

    }

    public void Finish() {
        List<NavArea> navAreas = new List<NavArea>();

        List<AlpinePolygon> polygons = new List<AlpinePolygon>();
        for(int i = 0;i < Instaniated.Polygons.Length;i ++) {
            Transform parentTransform = Instaniated.Polygons[i].ParentElement;
            polygons.Add(ModAPIUtils.TransformPolygon(Instaniated.Polygons[i], parentTransform.eulerAngles.y, parentTransform.position));
        
            if((polygons[i].Flags & PolygonFlags.NAVIGABLE_MASK) != 0) {
                NavArea temp = new NavArea();

                temp.Guid                = polygons[i].Guid;
                temp.Level               = polygons[i].Level;
                temp.Polygon             = polygons[i].Polygon;
                temp.Filter              = polygons[i].Filter;
                temp.Renderer            = polygons[i].Renderer;
                temp.Color               = polygons[i].Color;
                temp.ArbitrarilyEditable = polygons[i].ArbitrarilyEditable;
                temp.Flags               = polygons[i].Flags;
                temp.Height              = polygons[i].Height;
                
                temp.Owner = Result;

                navAreas.Add(temp);
                polygons[i] = temp;
            }
            PolygonsController.Instance.RegisterPolygon(polygons[i]);
        }

        NavArea serviceArea = polygons[Template.ServiceNode.PolygonDefinitionID] as NavArea;
        Assert.IsNotNull(serviceArea);
        NavDestination serviceNode = new NavDestination {
            Pos = ModAPIUtils.TransformBuildingCoordinates(Instaniated.ServiceNode.Pos, Instaniated.transform.eulerAngles.y, Instaniated.transform.position.ToHorizontal()), 
            Area = serviceArea
        };

        serviceArea.Nodes.Add(serviceNode);

        Result.ServiceNode = serviceNode;
        Result.NavAreas = navAreas;

        Result.Functionality.OnFinishConstruction();
    }

    public void UpdatePos(Vector2 pos)
    {
        Pos = pos;
    }
}