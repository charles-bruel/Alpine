using System;
using System.Collections.Generic;
using UnityEngine;

public class WeatherController : MonoBehaviour {

    [Header("Driven Values")]
    SnowLevelBuffer Recent;
    SnowLevelBuffer Base;
    [Header("Driver settings")]
    public float TimeFactor;
    public float MinStormTime;
    public float MinCalmTime;
    public float MaxStormTime;
    public float MaxCalmTime;
    public float MinStormPower;
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
    [Header("Visuals")]
    public ParticleSystem SnowParticles;

    //TODO: Make these properties or something
    [Header("Snowfall Data")]
    public float Snowfall12Hr;
    public float Snowfall24Hr;
    public float Snowfall7D;

    
    //Sampling every 12 hours for 7 days
    private static readonly int SnowfallTrackerSize = 14;
    private float[] SnowfallTracker;
    private int SnowfallTrackerIndex;

    //TODO: Unified time control
    private float SnowfallTrackerTimer;
    private float CurrentSnowfallTracker;

    void Start() {
        Recent = new SnowLevelBuffer();
        Base = new SnowLevelBuffer();

        
        Keyframe[] blank = new Keyframe[SnowLevelBuffer.Size];
        for(int i = 0;i < SnowLevelBuffer.Size;i ++) {
            blank[i] = new Keyframe(i / (float) SnowLevelBuffer.Size, 0);
        }
        BaseSnow = new AnimationCurve(blank);
        RecentSnow = new AnimationCurve(blank);

        SnowfallTracker = new float[SnowfallTrackerSize];
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

            CurrentSnowfallTracker += StormPower * delta;

            if(Timer < 0) {
                Storm = false;

                Timer = random.NextFloat(MinCalmTime, MaxCalmTime);

                if(SnowParticles != null) {
                    SnowParticles.Stop();
                }
            }
        } else {
            Base.Affect(0, -BaseDecay * delta);
            Recent.Affect(0, -RecentDecay * delta);

            if(Timer < 0) {
                Storm = true;

                System.Random random = new System.Random();
                Timer = random.NextFloat(MinStormTime, MaxStormTime);
                StormPower = random.NextFloat(MinStormPower, MaxStormPower);
                StormHeight = random.NextFloat(MinStormHeight, MaxStormHeight);

                if(SnowParticles != null) {
                    SnowParticles.Play();
                    SnowParticles.transform.position = new Vector3(0, StormHeight * TerrainManager.Instance.TileHeight, 0);
                }
            }
        }

        Recent.SendBufferUpdate();
        Base.SendBufferUpdate();
        for(int i = 0;i < SnowLevelBuffer.Size;i ++) {
            RecentSnow.MoveKey(i, new Keyframe(i / (float) SnowLevelBuffer.Size, Recent.Data[i]));
            BaseSnow.MoveKey(i, new Keyframe(i / (float) SnowLevelBuffer.Size, Base.Data[i]));
        }

        SnowfallTrackerTimer += delta;
        //12 Hours passed
        if(SnowfallTrackerTimer > 0.5f) {
            SnowfallTrackerTimer -= 0.5f;

            SnowfallTrackerIndex++;
            SnowfallTrackerIndex %= SnowfallTrackerSize;

            SnowfallTracker[SnowfallTrackerIndex] = CurrentSnowfallTracker;
            CurrentSnowfallTracker = 0;

            Snowfall12Hr = SnowfallTracker[SnowfallTrackerIndex];

            int prevIndex = SnowfallTrackerIndex == 0 ? SnowfallTrackerSize - 1 : SnowfallTrackerIndex - 1;
            Snowfall24Hr = Snowfall12Hr + SnowfallTracker[prevIndex];

            Snowfall7D = 0;
            foreach(float val in SnowfallTracker) {
                Snowfall7D += val;
            }
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