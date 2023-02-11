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
    [Header("Materials")]
    public Material LiftLineMaterial;

    public void Initialize() {
        Instance = this;
    }

    void Update() {
        Initialize();
    }

    public static RenderingData Instance;
}
