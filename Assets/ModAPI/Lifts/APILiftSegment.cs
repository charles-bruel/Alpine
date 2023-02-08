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
    public virtual List<LiftCablePoint> GetCablePointsUphill(GameObject self, Transform uphillCablePoints) {
        List<LiftCablePoint> toReturn = new List<LiftCablePoint>(uphillCablePoints.childCount);
        if(uphillCablePoints.childCount == 0) {
            toReturn.Add(new LiftCablePoint(uphillCablePoints.position, uphillCablePoints.localScale.x));
        } else {
            for(int i = 0;i < uphillCablePoints.childCount;i ++) {
                Transform temp = uphillCablePoints.GetChild(i);
                toReturn.Add(new LiftCablePoint(temp.position, temp.localScale.x));
            }
        }
        return toReturn;
    }

    // Gets the cable points (in world space) going downhill on the lift.
    // This function will be called after Build()
    // Default behavior is to return the children of downhillCablePoints is there are any, otherwise
    // returning downhillCablePoints
    public virtual List<LiftCablePoint> GetCablePointsDownhill(GameObject self, Transform downhillCablePoints) {
        List<LiftCablePoint> toReturn = new List<LiftCablePoint>(downhillCablePoints.childCount);
        if(downhillCablePoints.childCount == 0) {
            toReturn.Add(new LiftCablePoint(downhillCablePoints.position, downhillCablePoints.localScale.x));
        } else {
            for(int i = 0;i < downhillCablePoints.childCount;i ++) {
                Transform temp = downhillCablePoints.GetChild(i);
                toReturn.Add(new LiftCablePoint(temp.position, temp.localScale.x));
            }
        }
        return toReturn;
    }

    // This will be called last
    public virtual void Finish() {
        
    }
}
