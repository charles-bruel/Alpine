using System;
using System.Collections.Generic;
using UnityEngine;

public class WeatherController : MonoBehaviour {

    [Header("Driven Values")]
    SnowLevelBuffer Recent;
    SnowLevelBuffer Base;
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

    void Start() {
        Recent = new SnowLevelBuffer();
        Base = new SnowLevelBuffer();
    }

    public void UpdateMaterial(Material material, SnowCatcherType type) {
        SnowLevelBuffer reference;
        if(type == SnowCatcherType.Base) {
            reference = Base;
        } else {
            reference = Recent;
        }

        reference.SendBufferUpdate();

        //TODO: Reduce call frequency
        material.SetBuffer("snowCurve", reference.Buffer);
    }

    void Update() {
        float delta = Time.deltaTime * TimeFactor;
        Timer -= delta;

        if(Storm) {
            // RecentSnowDepth += StormPower * delta * RecentSnowPowerMultiplier;
            // BaseSnowDepth += StormPower * delta;

            // if(RecentSnowThreshold > StormHeight) {
            //     RecentSnowThreshold += (StormHeight - RecentSnowThreshold) * delta * RecentSnowPowerMultiplier * StormPower;
            // }

            // if(BaseSnowThreshold > StormHeight) {
            //     BaseSnowThreshold += (StormHeight - BaseSnowThreshold) * delta * StormPower;
            // }
            
            // BaseSnowThreshold = Mathf.Min(BaseSnowThreshold, StormHeight);

            if(Timer < 0) {
                Storm = false;

                System.Random random = new System.Random();
                Timer = (float)(MaxCalmTime * random.NextDouble());
            }
        } else {
            // RecentSnowDepth -= RecentDepthDecay * delta;
            // RecentSnowThreshold += RecentThresholdDecay * delta;
            // BaseSnowDepth -= BaseDepthDecay * delta;
            // BaseSnowThreshold += BaseThresholdDecay * delta;

            if(Timer < 0) {
                Storm = true;

                System.Random random = new System.Random();
                Timer = (float)(MaxStormTime * random.NextDouble());
                StormPower = (float)(MaxStormPower * random.NextDouble());
                StormHeight = (float)(random.NextDouble());
            }
        }

        Recent.SendBufferUpdate();
        Base.SendBufferUpdate();
    }

    void OnDestroy() {
        Recent.Dispose();
        Base.Dispose();
    }

    public enum SnowCatcherType {
        Base,
        Recent,
    }
}