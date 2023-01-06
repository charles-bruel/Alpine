using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class CatwalkSegment : MonoBehaviour
{
    [Header("Config Data")]
    public GameObject StartFence;
    public GameObject EndFence;

    [Header("Runtime Data")]
    public bool IsStart;
    public bool IsEnd;

    // Update is called once per frame
    void Update()
    {
        if(StartFence.activeSelf != IsStart)
            StartFence.SetActive(IsStart);
        if (EndFence.activeSelf != IsEnd)
            EndFence.SetActive(IsEnd);
    }
}