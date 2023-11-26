using System.Collections;
using System.Collections.Generic;
using EPPZ.Geometry.Model;
using UnityEngine;

public class APILiftSegment : APIBase
{
    // This is called first and allows the segment to change it's appearance based on the enviorment
    // current, next and prev are cable points. For lift routing segments (stations and turns), the
    // passed cables points are for *other* routing segments, not towers. If you want to make something
    // based on tower position, you will need to place a tower to do so.
    public virtual void Build(ICustomScriptable parent, Transform current, Transform next, Transform prev) {
        
    }

    // Gets the cable points (in world space) going uphill on the lift.
    // This function will be called after Build()
    // Default behavior is to return the children of uphillCablePoints is there are any, otherwise
    // returning uphillCablePoints
    public virtual List<LiftCablePoint> GetCablePointsUphill(ICustomScriptable parent, Transform uphillCablePoints) {
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
    public virtual List<LiftCablePoint> GetCablePointsDownhill(ICustomScriptable parent, Transform downhillCablePoints) {
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

    // This allows lifts elements to place their own effect polygons
    public virtual List<AlpinePolygon> GetPolygons(ICustomScriptable parent, AlpinePolygonSource[] providedPolygons) {
        // Provided polygons are in local space, so we must manually transform them
        // into world space
        List<AlpinePolygon> result = new List<AlpinePolygon>(providedPolygons.Length);

        for(int i = 0;i < providedPolygons.Length;i ++) {
            Transform parentTransform = providedPolygons[i].ParentElement;
            result.Add(ModAPIUtils.TransformPolygon(providedPolygons[i], parentTransform.eulerAngles.y, parentTransform.position));
        }

        return result;
    }
}
