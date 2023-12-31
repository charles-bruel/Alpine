using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateController : MonoBehaviour {
    public bool Mode2D { get; private set; }
    public bool Mode3D { get { return !Mode2D; } }
    public GameObject[] TwoDOnlyObjects;
    public GameObject[] ThreeDOnlyObjects;
    public Camera TwoDCamera;
    public Camera ThreeDCamera;

    public void ToggleMode(bool mode) {
        for(int i = 0;i < TwoDOnlyObjects.Length;i ++) {
            TwoDOnlyObjects[i].SetActive(mode);
        }
        for(int i = 0;i < ThreeDOnlyObjects.Length;i ++) {
            ThreeDOnlyObjects[i].SetActive(!mode);
        }
        Mode2D = mode;
    }

    public void Initialize() {
        Instance = this;
    }

    public static StateController Instance;
}
