using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateController : MonoBehaviour
{
    public GameObject[] TwoDOnlyObjects;
    public GameObject[] ThreeDOnlyObjects;

    public void ToggleMode(bool mode) {
        for(int i = 0;i < TwoDOnlyObjects.Length;i ++) {
            TwoDOnlyObjects[i].SetActive(mode);
        }
        for(int i = 0;i < ThreeDOnlyObjects.Length;i ++) {
            ThreeDOnlyObjects[i].SetActive(!mode);
        }
    }

    void Start() {
        Instance = this;
    }

    public static StateController Instance;
}
