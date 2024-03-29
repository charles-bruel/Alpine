//>============================================================================<
//
//    Alpine, Ski Resort Tycoon Game
//    Copyright (C) 2024  Charles Bruel
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see <https://www.gnu.org/licenses/>.
//>============================================================================<

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
