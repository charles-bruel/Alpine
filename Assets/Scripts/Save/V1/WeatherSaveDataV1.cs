using System;
using UnityEngine;

[System.Serializable]
public struct WeatherSaveDataV1 {
    public bool Storm;
    public float StormHeight;
    public float StormPower;
    public float Timer;
    public float Snowfall12Hr;
    public float Snowfall24Hr;
    public float Snowfall7D;
    public float[] SnowfallTracker;
    public int SnowfallTrackerIndex;
    public byte[] BaseSnow;
    public byte[] RecentSnow;

    public static WeatherSaveDataV1 FromWeather(WeatherController instance) {
        WeatherSaveDataV1 result = new WeatherSaveDataV1();
        result.Storm = instance.Storm;
        result.StormHeight = instance.StormHeight;
        result.StormPower = instance.StormPower;
        result.Timer = instance.StormTimer;
        result.Snowfall12Hr = instance.Snowfall12Hr;
        result.Snowfall24Hr = instance.Snowfall24Hr;
        result.Snowfall7D = instance.Snowfall7D;
        result.SnowfallTracker = instance.SnowfallTracker.Clone() as float[];
        result.SnowfallTrackerIndex = instance.SnowfallTrackerIndex;
        
        float[] BaseSnow = new float[SnowLevelBuffer.Size];
        float[] RecentSnow = new float[SnowLevelBuffer.Size];

        for(int i = 0;i < SnowLevelBuffer.Size;i ++) {
            BaseSnow[i] = instance.Base.Data[i];
        }

        for(int i = 0;i < SnowLevelBuffer.Size;i ++) {
            RecentSnow[i] = instance.Recent.Data[i];
        }

        result.BaseSnow = Utils.FloatArrayToByteArray(BaseSnow);
        result.RecentSnow = Utils.FloatArrayToByteArray(RecentSnow);

        return result;
    }

    public void Restore() {
        WeatherController.Instance.Storm = Storm;
        WeatherController.Instance.StormHeight = StormHeight;
        WeatherController.Instance.StormPower = StormPower;
        WeatherController.Instance.StormTimer = Timer;
        WeatherController.Instance.Snowfall12Hr = Snowfall12Hr;
        WeatherController.Instance.Snowfall24Hr = Snowfall24Hr;
        WeatherController.Instance.Snowfall7D = Snowfall7D;
        WeatherController.Instance.SnowfallTracker = SnowfallTracker;
        WeatherController.Instance.SnowfallTrackerIndex = SnowfallTrackerIndex;

        float[] BaseSnow = Utils.ByteArrayToFloatArray(this.BaseSnow);
        float[] RecentSnow = Utils.ByteArrayToFloatArray(this.RecentSnow);

        for(int i = 0;i < BaseSnow.Length;i ++) {
            WeatherController.Instance.Recent.Data[i] = BaseSnow[i];
        }

        for(int i = 0;i < RecentSnow.Length;i ++) {
            WeatherController.Instance.Recent.Data[i] = RecentSnow[i];
        }
    }
}