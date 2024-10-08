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

using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.Assertions;
using UnityEngine.UI;
using System.Security.AccessControl;

public class BuildingBuilder {
    public Canvas WorldUICanvas;

    public Vector2 Pos;
    public float Rotation;
    public SimpleBuildingTemplate Template;
    private SimpleBuildingTemplate Instaniated;
    private SimpleBuilding Result;
    private Image MapDisplay;

    private PolygonsPreview PolygonsPreview;

    public void LightBuild() {
        // Place above any contour lines
        MapDisplay.rectTransform.localPosition = new Vector3(Pos.x, Pos.y, -(TerrainManager.Instance.TileHeight + 128));
        MapDisplay.rectTransform.localRotation = Quaternion.Euler(0, 0, -Rotation);

        PolygonsPreview.Update(Rotation, Pos);
    }

    public void Initialize() {
        // TODO: Better? (or more consistent?) solution
        if(WorldUICanvas == null) {
            var objs = GameObject.FindObjectsOfType<Canvas>(true);
            foreach(var obj in objs) {
                if(obj.transform.parent != null && obj.transform.parent.name == "WorldUI") {
                    WorldUICanvas = obj;
                    break;
                }
            }
        }

        GameObject gameObject = new GameObject();
        MapDisplay = gameObject.AddComponent<Image>();
        MapDisplay.sprite = Template.Icon2D;
        MapDisplay.rectTransform.SetParent(WorldUICanvas.transform);
        MapDisplay.rectTransform.localRotation = Quaternion.Euler(0, 0, 0);
        MapDisplay.rectTransform.localScale = new Vector3(Template.IconSize.x, Template.IconSize.y, 1) / 100;
        MapDisplay.raycastTarget = false;

        PolygonsPreview = new PolygonsPreview();
        PolygonsPreview.SetPolygons(Template.Polygons);
    }

    public void Build() {
        Instaniated = GameObject.Instantiate(Template);
        Instaniated.transform.position = TerrainManager.Instance.Project(Pos);
        Instaniated.transform.rotation = Quaternion.Euler(0, Rotation, 0);
        Result = Instaniated.gameObject.AddComponent<SimpleBuilding>();
        Result.Template = Template;
        Result.Functionality = Instaniated.Functionality;
        Result.Functionality.Building = Result;
    }

    public void Cancel() {
        GameObject.Destroy(MapDisplay.gameObject);

        PolygonsPreview.Destroy();
    }

    public void Finish() {
        List<NavArea> navAreas = new List<NavArea>();

        List<AlpinePolygon> polygons = new List<AlpinePolygon>();
        for(int i = 0;i < Instaniated.Polygons.Length;i ++) {
            Transform parentTransform = Instaniated.Polygons[i].ParentElement;
            polygons.Add(ModAPIUtils.TransformPolygon(Instaniated.Polygons[i], parentTransform.eulerAngles.y, parentTransform.position));
        
            polygons[i].Owner = Result;

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
                temp.Owner               = polygons[i].Owner;

                temp.Implementation = new BasicFlatNavigableNavAreaImplementation(Result);
                
                navAreas.Add(temp);
                polygons[i] = temp;
            }
            PolygonsController.Instance.RegisterPolygon(polygons[i]);
        }

        NavArea serviceArea = polygons[Template.FunctionalityNode.PolygonDefinitionID] as NavArea;
        Assert.IsNotNull(serviceArea);
        NavFunctionalityNode functionality = new NavFunctionalityNode {
            Pos = ModAPIUtils.TransformBuildingCoordinates(Instaniated.FunctionalityNode.Pos, Instaniated.transform.eulerAngles.y, Instaniated.transform.position.ToHorizontal()), 
            Area = serviceArea,
            BuildingFunctionality = Result.Functionality
        };

        serviceArea.Nodes.Add(functionality);

        Result.FunctionalityNode = functionality;
        Result.NavAreas = navAreas;
        Result.Polygons = polygons;

        Result.WorldUIIcon = MapDisplay.rectTransform;

        Result.Functionality.OnFinishConstruction();

        BuildingsController.Instance.RegisterBuilding(Result);

        PolygonsPreview.Destroy();
    }

    public void UpdatePos(Vector2 pos) {
        Pos = pos;
    }

    public static Building BuildFromSave(Vector3 pos, float rotation, string templateName, NavAreaGraphSaveDataV1[] navData, LoadingContextV1 loadingContext) {
        BuildingBuilder builder = new BuildingBuilder();
        builder.Template = BuildingsController.Instance.GetBuildingTemplate(templateName);
        builder.Initialize();
        builder.Pos = pos.ToHorizontal();
        builder.Rotation = rotation;
        builder.Build();
        builder.Instaniated.transform.position = pos;
        builder.Finish();

        Assert.AreEqual(navData.Length, builder.Result.NavAreas.Count);
        for(int i = 0; i < navData.Length;i ++) {
            builder.Result.NavAreas[i].ID = navData[i].ID;
            loadingContext.navAreas.Add(navData[i].ID, builder.Result.NavAreas[i]);
        }

        return builder.Result;
    }
}