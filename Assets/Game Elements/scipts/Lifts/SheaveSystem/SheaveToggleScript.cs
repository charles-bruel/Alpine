using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SheaveToggleScript : MonoBehaviour
{
    public Sheave target;
    public Transform Lower;
    public Transform Higher;
    public int Threshold;

    private bool PastStatus;

    void Update()
    {
        bool status = target.Parent != null && target.Parent.Level >= Threshold;
        if(PastStatus = status)
        {
            return;
        }
        PastStatus = status;
        if (status)
        {
            Lower.gameObject.SetActive(false);
            Higher.gameObject.SetActive(true);
        } 
        else
        {
            Lower.gameObject.SetActive(true);
            Higher.gameObject.SetActive(false);
        }
    }
}
