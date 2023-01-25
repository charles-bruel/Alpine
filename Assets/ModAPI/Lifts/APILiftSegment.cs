using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class APILiftSegment : APIBase
{
    // This is called first and allows the segment to change it's appearance based on the enviorment
    // current, next and prev are cable points. For lift routing segments (stations and turns), the
    // passed cables points are for *other* routing segments, not towers. If you want to make something
    // based on tower position, you will need to place a tower to do so.
    public virtual void Build(GameObject self, Transform current, Transform next, Transform prev) {
        
    }

    // Gets the cable points (in world space) going uphill on the lift.
    // This function will be called after Build()
    // Default behavior is to return the children of uphillCablePoints is there are any, otherwise
    // returning uphillCablePoints
    public virtual List<Vector3> GetCablePointsUphill(GameObject self, Transform uphillCablePoints) {
        List<Vector3> toReturn = new List<Vector3>(uphillCablePoints.childCount);
        if(uphillCablePoints.childCount == 0) {
            toReturn.Add(uphillCablePoints.position);
        } else {
            for(int i = 0;i < uphillCablePoints.childCount;i ++) {
                toReturn.Add(uphillCablePoints.GetChild(i).position);
            }
        }
        return toReturn;
    }

    // Gets the cable points (in world space) going downhill on the lift.
    // This function will be called after Build()
    // Default behavior is to return the children of downhillCablePoints is there are any, otherwise
    // returning downhillCablePoints
    public virtual List<Vector3> GetCablePointsDownhill(GameObject self, Transform downhillCablePoints) {
        List<Vector3> toReturn = new List<Vector3>(downhillCablePoints.childCount);
        if(downhillCablePoints.childCount == 0) {
            toReturn.Add(downhillCablePoints.position);
        } else {
            for(int i = 0;i < downhillCablePoints.childCount;i ++) {
                toReturn.Add(downhillCablePoints.GetChild(i).position);
            }
        }
        return toReturn;
    }

    // Gets the cable points (in world space) going uphill on the lift.
    // This will be called last
    public virtual void Finish() {
        
    }
}
