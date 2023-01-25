using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Lift", menuName = "Game Elements/Lifts/Lift", order = 1)]
public class LiftTemplate : ScriptableObject
{
    [Header("Objects")]
    public LiftStationTemplate[] AvaliableStations;
    public LiftTowerTemplate[] AvaliableTowers;
    public LiftMidStationTemplate[] AvaliableMidStations;
    public LiftTurnTemplate[] AvaliableTurns;
    public LiftVehicleTemplate[] AvaliableLiftVehicles;

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

}
