using System;
using UnityEngine;
using UnityEngine.Assertions;

public class SnowfrontPanelUI : MonoBehaviour {
    [NonSerialized]
    public Snowfront CurrentSnowfront;

    public void Inflate(Snowfront newSnowfront) {
        CurrentSnowfront = newSnowfront;
        gameObject.SetActive(true);
    }

    public void Hide() {
        CurrentSnowfront = null;
        gameObject.SetActive(false);
    }

    public void OnDeleteButtonPressed() {
        Assert.IsNotNull(CurrentSnowfront);
        CurrentSnowfront.Destroy();
    }

    public void OnEditButtonPressed() {
        throw new NotImplementedException();
    }
}