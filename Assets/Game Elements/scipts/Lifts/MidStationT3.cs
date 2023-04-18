using System.Collections.Generic;
using UnityEngine;

public class MidStationT3 : APITurnSegment {
    public override void Build(ICustomScriptable parent, Transform current, Transform next, Transform prev) {
        float pivotOffset = FloatParameters[0];
        float stationLen = FloatParameters[1];

        Transform primarySegment = parent.GetGameObject().transform.GetChild(IntParameters[0]);
        Transform secondarySegment = parent.GetGameObject().transform.GetChild(IntParameters[1]);
        Transform turnGapCover = parent.GetGameObject().transform.GetChild(IntParameters[2]);
        Transform turnGapCoverA = turnGapCover.GetChild(0);
        Transform turnGapCoverB = turnGapCover.GetChild(1);

        Vector2 primaryVector = (current.position - prev.position).ToHorizontal();
        float primaryAngle = Mathf.Atan2(primaryVector.y, primaryVector.x) * Mathf.Rad2Deg;
        Vector2 secondaryVector = (next.position - current.position).ToHorizontal();
        float secondaryAngle = Mathf.Atan2(secondaryVector.y, secondaryVector.x) * Mathf.Rad2Deg;

        float averageAngle = Mathf.LerpAngle(primaryAngle, secondaryAngle, 0.5f);

        float alpha;
        {
            float a = 180 - primaryAngle + secondaryAngle;
            float b = 180 - secondaryAngle + primaryAngle;
            if(a < b) {
                alpha = a;
            } else {
                alpha = b;
            }
            if(alpha < 0) {
                alpha *= -1;
            }
        }
        
        float beta = 180 - alpha;
        float pivotDist = pivotOffset / Mathf.Cos(beta * 0.5f * Mathf.Deg2Rad);
        turnGapCoverA.localPosition = new Vector3(pivotDist, 0, 0);
        turnGapCoverB.localPosition = new Vector3(-pivotDist, 0, 0);

        float primaryWorldAngle = 90 - primaryAngle;
        float secondaryWorldAngle = 270 - secondaryAngle;

        float pivotWorldAngle = Mathf.LerpAngle(primaryWorldAngle, secondaryWorldAngle, 0.5f) - 90;
        turnGapCover.eulerAngles = new Vector3(0, pivotWorldAngle, 0);

        primarySegment.eulerAngles = new Vector3(0, primaryWorldAngle, 0);
        secondarySegment.eulerAngles = new Vector3(0, secondaryWorldAngle, 0);

        primaryWorldAngle *= Mathf.Deg2Rad;
        secondaryWorldAngle *= Mathf.Deg2Rad;

        float stationOffset = Mathf.Tan(beta * 0.5f * Mathf.Deg2Rad) * pivotOffset;
        stationOffset -= stationLen;

        parent.PersistentData()["lastlength"] = -stationOffset + stationLen;

        primarySegment.position = parent.GetGameObject().transform.position + new Vector3(
            stationOffset * Mathf.Sin(primaryWorldAngle),
            primarySegment.localPosition.y,
            stationOffset * Mathf.Cos(primaryWorldAngle)
        );
        secondarySegment.position = parent.GetGameObject().transform.position + new Vector3(
            stationOffset * Mathf.Sin(secondaryWorldAngle), 
            secondarySegment.localPosition.y, 
            stationOffset * Mathf.Cos(secondaryWorldAngle)
        );
    }

    public override float GetLength(ICustomScriptable parent)
    {
        return (float)parent.PersistentData()["lastlength"];
    }

    public override List<LiftCablePoint> GetCablePointsUphill(ICustomScriptable parent, Transform uphillCablePoints) {
        return GetCablePointsUniversal(parent.GetGameObject(), true);
    }

    public override List<LiftCablePoint> GetCablePointsDownhill(ICustomScriptable parent, Transform downhillCablePoints) {
        return GetCablePointsUniversal(parent.GetGameObject(), false);
    }

    private List<LiftCablePoint> GetCablePointsUniversal(GameObject self, bool uphill) {
        Transform primarySegment = self.transform.GetChild(IntParameters[0]);
        Transform secondarySegment = self.transform.GetChild(IntParameters[1]);

        Transform primarySegmentCPTransform = primarySegment.GetChild(IntParameters[uphill ? 3 : 4]);
        Transform secondarySegmentCPTransform = secondarySegment.GetChild(IntParameters[uphill ? 3 : 4]);

        //Grab the innermost points for start and end
        Vector3 primarySegmentCP = primarySegmentCPTransform.GetChild(1).position;
        Vector3 secondarySegmentCP = secondarySegmentCPTransform.GetChild(1).position;

        int extraPoints = primarySegmentCPTransform.childCount + secondarySegmentCPTransform.childCount + 2;
        
        float Arad = primarySegment.eulerAngles.y * Mathf.Deg2Rad;
        Vector2 Aray = new Vector2(Mathf.Sin(Arad), Mathf.Cos(Arad));
        float Brad = secondarySegment.eulerAngles.y * Mathf.Deg2Rad;
        Vector2 Bray = new Vector2(Mathf.Sin(Brad), Mathf.Cos(Brad));

        Vector2 A1 = primarySegmentCP.ToHorizontal();
        Vector2 A2 = A1 + Aray;
        Vector2 B1 = secondarySegmentCP.ToHorizontal();
        Vector2 B2 = B1 + Bray;

        Vector2 intersection = Utils.LineLine(A1, A2, B1, B2);
        
        List<Vector2> filletResult = Utils.Fillet(A1, intersection, B1, 3, 32);

        List<LiftCablePoint> toReturn = new List<LiftCablePoint>(filletResult.Count - 2 + extraPoints);

        toReturn.Add(new LiftCablePoint(primarySegmentCPTransform.position, primarySegmentCPTransform.localScale.x));

        for(int i = 0;i < primarySegmentCPTransform.childCount;i ++) {
            Transform temp = primarySegmentCPTransform.GetChild(i);
            toReturn.Add(new LiftCablePoint(temp.position, temp.localScale.x));
        }

        for(int i = 0;i < filletResult.Count;i ++) {
            Vector3 pos = new Vector3(filletResult[i].x, primarySegmentCP.y, filletResult[i].y);
            toReturn.Add(new LiftCablePoint(pos, toReturn[toReturn.Count - 1].speed));
        }

        for(int i = secondarySegmentCPTransform.childCount - 1;i >= 0;i --) {
            Transform temp = secondarySegmentCPTransform.GetChild(i);
            toReturn.Add(new LiftCablePoint(temp.position, temp.localScale.x));
        }

        toReturn.Add(new LiftCablePoint(secondarySegmentCPTransform.position, secondarySegmentCPTransform.localScale.x));

        if(!uphill) toReturn.Reverse();

        return toReturn;
    }
}