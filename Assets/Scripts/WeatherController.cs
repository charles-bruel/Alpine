using System;
using System.Collections.Generic;
using UnityEngine;

public class WeatherController : MonoBehaviour {

    [Header("Driven Values")]
    [Range(0, 1)]
    public float BaseSnowThreshold;
    [Range(0, 1)]
    public float BaseSnowDepth;
    [Range(0, 1)]
    public float RecentSnowThreshold;
    [Range(0, 1)]
    public float RecentSnowDepth;

    // [Header("Driver settings")]

    public void UpdateMaterial(Material material, SnowCatcherType type) {
        float depth, threshold;
        if(type == SnowCatcherType.Base) {
            depth = BaseSnowDepth;
            threshold = BaseSnowThreshold;
        } else {
            depth = RecentSnowDepth;
            threshold = RecentSnowThreshold;
        }
             
        if(material.HasFloat("_Depth")) {
            material.SetFloat("_Depth", depth);
        }

        if(material.HasFloat("_Threshold")) {
            material.SetFloat("_Threshold", threshold);
        }
    }

    public enum SnowCatcherType {
        Base,
        Recent,
    }
}