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

    [Header("Driver settings")]
    public float TimeFactor;
    public float MaxStormTime;
    public float MaxCalmTime;
    public float MaxStormPower;
    public float RecentThresholdDecay;
    public float RecentDepthDecay;
    public float BaseThresholdDecay;
    public float BaseDepthDecay;
    public float RecentSnowPowerMultiplier;
    [Header("Current Storm Settings")]
    public bool Storm;
    public float StormHeight;
    public float StormPower;
    public float Timer;

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

    void Update() {
        float delta = Time.deltaTime * TimeFactor;
        Timer -= delta;

        if(Storm) {
            RecentSnowDepth += StormPower * delta * RecentSnowPowerMultiplier;
            BaseSnowDepth += StormPower * delta;

            if(RecentSnowThreshold > StormHeight) {
                RecentSnowThreshold += (StormHeight - RecentSnowThreshold) * delta * RecentSnowPowerMultiplier * StormPower;
            }

            if(BaseSnowThreshold > StormHeight) {
                BaseSnowThreshold += (StormHeight - BaseSnowThreshold) * delta * StormPower;
            }
            
            BaseSnowThreshold = Mathf.Min(BaseSnowThreshold, StormHeight);

            if(Timer < 0) {
                Storm = false;

                System.Random random = new System.Random();
                Timer = (float)(MaxCalmTime * random.NextDouble());
            }
        } else {
            RecentSnowDepth -= RecentDepthDecay * delta;
            RecentSnowThreshold += RecentThresholdDecay * delta;
            BaseSnowDepth -= BaseDepthDecay * delta;
            BaseSnowThreshold += BaseThresholdDecay * delta;

            if(Timer < 0) {
                Storm = true;

                System.Random random = new System.Random();
                Timer = (float)(MaxStormTime * random.NextDouble());
                StormPower = (float)(MaxStormPower * random.NextDouble());
                StormHeight = (float)(random.NextDouble());
            }
        }

        if(RecentSnowDepth < 0) RecentSnowDepth = 0;
        if(RecentSnowDepth > 1) RecentSnowDepth = 1;
        if(RecentSnowThreshold < 0) RecentSnowThreshold = 0;
        if(RecentSnowThreshold > 1) RecentSnowThreshold = 1;
        if(BaseSnowDepth < 0) BaseSnowDepth = 0;
        if(BaseSnowDepth > 1) BaseSnowDepth = 1;
        if(BaseSnowThreshold < 0) BaseSnowThreshold = 0;
        if(BaseSnowThreshold > 1) BaseSnowThreshold = 1;
    }

    public enum SnowCatcherType {
        Base,
        Recent,
    }
}