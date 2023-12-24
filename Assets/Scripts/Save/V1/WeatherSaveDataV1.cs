using System;

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
        result.Timer = instance.Timer;
        result.Snowfall12Hr = instance.Snowfall12Hr;
        result.Snowfall24Hr = instance.Snowfall24Hr;
        result.Snowfall7D = instance.Snowfall7D;
        result.SnowfallTracker = instance.SnowfallTracker.Clone() as float[];
        result.SnowfallTrackerIndex = instance.SnowfallTrackerIndex;
        
        float[] BaseSnow = new float[instance.BaseSnow.length];
        float[] RecentSnow = new float[instance.RecentSnow.length];

        for(int i = 0;i < instance.BaseSnow.length;i ++) {
            BaseSnow[i] = instance.BaseSnow[i].value;
        }

        for(int i = 0;i < instance.RecentSnow.length;i ++) {
            RecentSnow[i] = instance.RecentSnow[i].value;
        }

        result.BaseSnow = Utils.FloatArrayToByteArray(BaseSnow);
        result.RecentSnow = Utils.FloatArrayToByteArray(RecentSnow);

        return result;
    }
}