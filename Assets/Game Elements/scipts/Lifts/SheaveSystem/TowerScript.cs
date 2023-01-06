using System.Collections.Generic;
using UnityEngine;
using System;

public class TowerScript : MonoBehaviour
{
    public float Width;
    public GameObject PaddingSource;
    public List<GameObject> PaddingObjects;
    public Transform BaseItems;
    public Transform Target;
    public Transform WirePoint;

    void Update()
    {
        //Ugly and possibly slow but gets the job done.
        if (PaddingSource != null)
        {
            foreach (GameObject obj in PaddingObjects){
                if(obj.activeSelf != PaddingSource.activeSelf)
                    obj.SetActive(PaddingSource.activeSelf);
            }
        }
    }

    public void Reset()
    {
        if(BaseItems != null)
        {
            RaycastHit hitInfo = default(RaycastHit);
            Physics.Raycast(transform.position, Target.position - transform.position, out hitInfo, float.MaxValue, 768);
            BaseItems.position = hitInfo.point;
        }
    }
}