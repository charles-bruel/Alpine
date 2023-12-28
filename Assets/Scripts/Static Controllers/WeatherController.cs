using System;
using System.Collections.Generic;
using UnityEngine;

//TODO: Expanded weather mechanics
public class WeatherController : MonoBehaviour {

    [Header("Driven Values")]
    public SnowLevelBuffer Recent;
    public SnowLevelBuffer Base;
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
    public float ConstantMeltingRate;
    [Tooltip("Per degree F above 32F")]
    public float AirTemperatureMeltingRateMultiplier;
    public float CloudyMeltingRateMultiplier;
    public float CloudyProportional;
    public float CloudCheckPeriodMean;
    public float CloudCheckPeriodVariance;
    public float TemperatureMeanSunny;
    public float TemperatureMeanCloudy;
    public float TemperatureVariability;
    public float TemperatureRestoringPower;
    public float WindPowMean;
    public float WindVariability;
    public float WindRestoringPower;
    public float RecentSnowPowerMultiplier;
    public float MinStormHeight;
    public float MaxStormHeight;
    public float HeightVariability;
    public float RainThreshold;
    public float RainDestructionMultiplier;
    [Header("Current Storm Settings")]
    public bool Storm;
    public float StormHeight;
    public float StormPower;
    public float StormTimer;
    [Header("Current Cloud Settings")]
    public bool Cloudy;
    public float CloudyTimer;
    [Header("Current Other Settings")]
    public float Temperature;
    public float Wind;
    [Header("Current Conditions")]
    // NOTE - these are literally *just* for visualizing in the inspector
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
    public float[] SnowfallTracker;
    public int SnowfallTrackerIndex;

    //TODO: Unified time control
    private float SnowfallTrackerTimer;
    private float CurrentSnowfallTracker;

    private bool Initialized = false;

    public float particlesPer1ksq = 10000;

    public static WeatherController Instance;

    public void Initialize() {
        Instance = this;
        
        Recent = new SnowLevelBuffer();
        Base = new SnowLevelBuffer();
        
        Keyframe[] blank = new Keyframe[SnowLevelBuffer.Size];
        for(int i = 0;i < SnowLevelBuffer.Size;i ++) {
            blank[i] = new Keyframe(i / (float) SnowLevelBuffer.Size, 0);
        }
        BaseSnow = new AnimationCurve(blank);
        RecentSnow = new AnimationCurve(blank);

        SnowfallTracker = new float[SnowfallTrackerSize];

        {
            float width = TerrainManager.Instance.NumTilesX * TerrainManager.Instance.TileSize;
            float height = TerrainManager.Instance.NumTilesY * TerrainManager.Instance.TileSize;
            var temp = SnowParticles.shape;
            temp.scale = new Vector3(
                width,
                height,
                TerrainManager.Instance.TileHeight + 400
            );
            width *= 0.001f;// Convert to km
            height *= 0.001f;
            int particleCount = (int) (particlesPer1ksq * width * height);
            var temp2 = SnowParticles.main;
            temp2.maxParticles = particleCount;
            var temp3 = SnowParticles.emission;
            temp3.rateOverTime = particleCount;
        }

        Initialized = true;

        Temperature = TemperatureMeanSunny;
        Wind = Mathf.Exp(WindPowMean);
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

    private float GetMeltingRateMultipler(float temperature) {
        // Melting rate comes from two sources - the sun and air temperature
        // The sun will be constant compared to the air temperature, whereas
        // the air temperature rate is proportional to the difference between
        // the air temperature and the snow (i.e. 32F).

        // Sorry for imperial units
        float meltingRate = ConstantMeltingRate;
        if(Cloudy) {
            meltingRate *= CloudyMeltingRateMultiplier;
        }

        if(temperature > 32) {
            meltingRate += (temperature - 32) * AirTemperatureMeltingRateMultiplier;
        }

        return meltingRate;
    }

    private float GetNextTemperature(float temperature, float delta) {
        float distFromMean = (Cloudy ? TemperatureMeanCloudy : TemperatureMeanSunny) - temperature;
        float deltaAdjust = distFromMean * TemperatureRestoringPower;
        float randomAdjust = UnityEngine.Random.Range(-TemperatureVariability, TemperatureVariability);

        return temperature + delta * (deltaAdjust * deltaAdjust * Mathf.Sign(deltaAdjust) + randomAdjust);
    }

    // Wind is exp(int_wind)
    private float GetNextWind(float wind, float delta) {
        float prev_internal_value = Mathf.Log(wind) * 5;

        float distFromMean = WindPowMean - prev_internal_value;
        float deltaAdjust = distFromMean * WindRestoringPower;
        float randomAdjust = UnityEngine.Random.Range(-WindVariability, WindVariability);

        float new_wind_internal_value = prev_internal_value + delta * (deltaAdjust * deltaAdjust * Mathf.Sign(deltaAdjust) + randomAdjust);
        return Mathf.Exp(new_wind_internal_value * 0.2f);
    }

    public void Advance(float delta) {
        if(!Initialized) return;
        delta *= TimeFactor;

        StormTimer -= delta;

        Temperature = GetNextTemperature(Temperature, delta);
        Wind = GetNextWind(Wind, delta);

        if(CloudyTimer < 0) {
            System.Random random = new System.Random();
            CloudyTimer = random.NextFloat(CloudCheckPeriodMean - CloudCheckPeriodVariance, CloudCheckPeriodMean + CloudCheckPeriodVariance);
            Cloudy = random.NextFloat(0, 1) < CloudyProportional;
        } else {
            CloudyTimer -= delta;
        }

        if(Storm) {
            System.Random random = new System.Random();
            if(Temperature > RainThreshold) {
                StormHeight = 0;

                // Rain :(
                // TODO: Rain particle effects
                Base.Affect(0, -StormPower * delta * RainDestructionMultiplier);
                Recent.Affect(0, -StormPower * delta * RecentSnowPowerMultiplier);
            } else {
                float HeightThisFrame = StormHeight + random.NextFloat(-HeightVariability, HeightVariability);

                if(HeightThisFrame < 0) HeightThisFrame = 0;
                if(HeightThisFrame > 1) HeightThisFrame = 1;

                Base.Affect((int) (HeightThisFrame * 256), StormPower * delta);
                Recent.Affect((int) (HeightThisFrame * 256), StormPower * delta * RecentSnowPowerMultiplier);

                CurrentSnowfallTracker += StormPower * delta;
            }

            if(StormTimer < 0) {
                Storm = false;

                StormTimer = random.NextFloat(MinCalmTime, MaxCalmTime);

                if(SnowParticles != null) {
                    SnowParticles.Stop();
                }
            }
        } else {
            float meltingRate = GetMeltingRateMultipler(Temperature);
            Base.Affect(0, -BaseDecay * meltingRate * delta);
            Recent.Affect(0, -RecentDecay * meltingRate * delta);

            if(StormTimer < 0) {
                Storm = true;

                System.Random random = new System.Random();
                StormTimer = random.NextFloat(MinStormTime, MaxStormTime);
                StormPower = random.NextFloat(MinStormPower, MaxStormPower);
                StormHeight = random.NextFloat(MinStormHeight, MaxStormHeight);

                if(SnowParticles != null) {
                    SnowParticles.Play();
                    SnowParticles.transform.position = new Vector3(0, Mathf.Max(StormHeight * TerrainManager.Instance.TileHeight, 0), 0);
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

            Snowfall12Hr = SnowfallTracker[SnowfallTrackerIndex] * Units.METERS_TO_INCHES;

            int prevIndex = SnowfallTrackerIndex == 0 ? SnowfallTrackerSize - 1 : SnowfallTrackerIndex - 1;
            Snowfall24Hr = Snowfall12Hr + SnowfallTracker[prevIndex] * Units.METERS_TO_INCHES;

            Snowfall7D = 0;
            foreach(float val in SnowfallTracker) {
                Snowfall7D += val;
            }
            Snowfall7D *= Units.METERS_TO_INCHES;
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