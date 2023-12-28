using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeatherStatusController : MonoBehaviour
{
    public Image WeatherIcon;
    public TMP_Text Temp;
    public TMP_Text Wind;
    public TMP_Text Snow12Hr;
    public TMP_Text Snow24Hr;
    public TMP_Text Snow7D;

    public Sprite[] WeatherSprites;

    //TODO: Make update only when needed
    void Update()
    {
        Snow12Hr.text = Mathf.RoundToInt(WeatherController.Instance.Snowfall12Hr) + "\"";
        Snow24Hr.text = Mathf.RoundToInt(WeatherController.Instance.Snowfall24Hr) + "\"";
        Snow7D.text = Mathf.RoundToInt(WeatherController.Instance.Snowfall7D) + "\"";

        Wind.text = Mathf.RoundToInt(WeatherController.Instance.Wind) + "MPH";
        Temp.text = Mathf.RoundToInt(WeatherController.Instance.Temperature) + "°F";

        if(TerrainManager.Instance.WeatherController.Storm) {
            if(WeatherController.Instance.Temperature > WeatherController.Instance.RainThreshold) {
                WeatherIcon.sprite = WeatherSprites[2];
            } else {
                WeatherIcon.sprite = WeatherSprites[0];
            }
        } else {
            if(WeatherController.Instance.Cloudy) {
                WeatherIcon.sprite = WeatherSprites[1];
            } else {
                WeatherIcon.sprite = WeatherSprites[3];
            }
        }
    }
}
