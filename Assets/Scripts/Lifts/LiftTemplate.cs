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

using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Lift", menuName = "Game Elements/Lifts/Lift", order = 1)]
public class LiftTemplate : ScriptableObject, IUIToolbarItemProvider
{
    [Header("UI Info")]
    public string Name;
    public Sprite Sprite;
    [Header("Objects")]
    public LiftStationTemplate[] AvaliableStations;
    public LiftTowerTemplate[] AvaliableTowers;
    public LiftMidStationTemplate[] AvaliableMidStations;
    public LiftTurnTemplate[] AvaliableTurns;
    public LiftVehicle[] AvaliableLiftVehicles;

    [Header("Operation settings")]
    [Tooltip("In m/s")]
    public float MinOperatingSpeed;
    [Tooltip("In m/s")]
    public float MaxSpeed;
    public uint WorkerCount;

    [Header("Pulse Settings")]
    public bool Pulsed;
    //TODO: Finish pulse settings

    [Header("Cable Settings")]
    public float CableThickness;
    [Tooltip("The x, y, and z value specify a normal offset. The w specifies cable thickness, as a ratio to the default thickness. You probably care about X (side to side) and Y (up and down) the most.")]
	public Vector4[] ExtraCables;
    public Material CableMaterial;

    [Header("Misc")]
    public LiftBidirectionality BidirectionalityAvaliablity;
    public APIDef TowerPlacementScript;
    
    public bool VehicleMixingAllowed;

    public Sprite GetSprite() {
        return Sprite;
    }

    public void OnToolEnabled(UIReferences uiReferences) {
        LiftBuilderTool tool = new();

        LiftConstructionData data = new()
        {
            Template = this,
            RoutingSegments = new List<LiftConstructionData.RoutingSegment>(),
            SpanSegments = new List<LiftConstructionData.SpanSegment>(),
        };

        tool.Data = data;
        tool.GrabTemplate = uiReferences.LiftBuilderGrabTemplate;
        tool.Canvas = uiReferences.WorldCanvas;
        InterfaceController.Instance.SelectedTool = tool;

        var UI = uiReferences.LiftBuilderUI;
        UI.Tool = tool;
        tool.UI = UI;
        UI.gameObject.SetActive(true);
    }
}
