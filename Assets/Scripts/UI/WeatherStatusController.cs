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
        Snow12Hr.text = Mathf.RoundToInt(TerrainManager.Instance.WeatherController.Snowfall12Hr) + "\"";
        Snow24Hr.text = Mathf.RoundToInt(TerrainManager.Instance.WeatherController.Snowfall24Hr) + "\"";
        Snow7D.text = Mathf.RoundToInt(TerrainManager.Instance.WeatherController.Snowfall7D) + "\"";

        if(TerrainManager.Instance.WeatherController.Storm) {
            WeatherIcon.sprite = WeatherSprites[0];
        } else {
            WeatherIcon.sprite = WeatherSprites[1];
        }
    }
}
