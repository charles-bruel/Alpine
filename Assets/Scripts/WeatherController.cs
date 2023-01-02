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
    public float RecentDecay;
    public float BaseDecay;
    public float RecentSnowPowerMultiplier;
    public float MinStormHeight;
    public float MaxStormHeight;
    public float HeightVariability;
    [Header("Current Storm Settings")]
    public bool Storm;
    public float StormHeight;
    public float StormPower;
    public float Timer;
    [Header("Current Conditions")]
    public AnimationCurve BaseSnow;
    public AnimationCurve RecentSnow;

    void Start() {
        Recent = new SnowLevelBuffer();
        Base = new SnowLevelBuffer();

        
        Keyframe[] blank = new Keyframe[SnowLevelBuffer.Size];
        for(int i = 0;i < SnowLevelBuffer.Size;i ++) {
            blank[i] = new Keyframe(i / (float) SnowLevelBuffer.Size, 0);
        }
        BaseSnow = new AnimationCurve(blank);
        RecentSnow = new AnimationCurve(blank);
    }

    public void UpdateMaterial(Material material, SnowCatcherType type) {
        SnowLevelBuffer reference;
        if(type == SnowCatcherType.Base) {
            reference = Base;
        } else {
            reference = Recent;
        }

        //TODO: Reduce call frequency
        material.SetBuffer("snowCurve", reference.Buffer);
    }

    void Update() {
        float delta = Time.deltaTime * TimeFactor;
        Timer -= delta;

        if(Storm) {
            System.Random random = new System.Random();
            float HeightThisFrame = StormHeight + random.NextFloat(-HeightVariability, HeightVariability);
            
            if(HeightThisFrame < 0) HeightThisFrame = 0;
            if(HeightThisFrame > 1) HeightThisFrame = 1;

            Base.Affect((int) (HeightThisFrame * 256), StormPower * delta);
            Recent.Affect((int) (HeightThisFrame * 256), StormPower * delta * RecentSnowPowerMultiplier);

            if(Timer < 0) {
                Storm = false;

                Timer = (float)(MaxCalmTime * random.NextDouble());
            }
        } else {
            Base.Affect(0, -BaseDecay * delta);
            Recent.Affect(0, -RecentDecay * delta);

            if(Timer < 0) {
                Storm = true;

                System.Random random = new System.Random();
                Timer = (float)(MaxStormTime * random.NextDouble());
                StormPower = (float)(MaxStormPower * random.NextDouble());
                StormHeight = random.NextFloat(MinStormHeight, MaxStormHeight);
            }
        }

        Recent.SendBufferUpdate();
        Base.SendBufferUpdate();
        for(int i = 0;i < SnowLevelBuffer.Size;i ++) {
            RecentSnow.MoveKey(i, new Keyframe(i / (float) SnowLevelBuffer.Size, Recent.Data[i]));
            BaseSnow.MoveKey(i, new Keyframe(i / (float) SnowLevelBuffer.Size, Base.Data[i]));
        }

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