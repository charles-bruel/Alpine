using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class RenderingData : MonoBehaviour
{
    [Header("2D view colors")]
    public Color UndevelopedBackgroundColor;

    [Tooltip("This color which have been aerially cleared, i.e. trees have been removed")]
    public Color DevelopedColor;

    [Tooltip("This color covers areas which been completely cleared, i.e. trees and rocks have been removed")]
    public Color ClearedColor;

    [Tooltip("This color covers areas which skiers navigate around without downhill skiing")]
    public Color SnowfrontColor;

    [Tooltip("This shows the region that will become a slope while building")]
    public Color SlopeDraftColor;

    [Tooltip("This color shows a portal between multiple navigable regions")]
    public Color PortalColor;

    [Header("Slope Colors")]
    public Color GreenSlopeColor;
    public Color BlueSlopeColor;
    public Color BlackSlopeColor;
    public Color DoubleBlackSlopeColor;

    [Header("Materials")]
    public Material VertexColorMaterial;

    public void Initialize() {
        Instance = this;
    }

    void Update() {
        Initialize();
    }

    public static RenderingData Instance;
}
