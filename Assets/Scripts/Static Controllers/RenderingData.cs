using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderingData : MonoBehaviour
{
    [Header("2D view colors")]
    public Color UndevelopedBackgroundColor;
    [Tooltip("This color covers developed areas that cannot be skied and do not have buildings, i.e. lift lines.")]
    public Color DevelopedColor;
    [Header("Materials")]
    public Material LiftLineMaterial;

    public void Initialize() {
        Instance = this;
    }

    public static RenderingData Instance;
}
