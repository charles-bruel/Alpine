using System;
using UnityEngine;

public class MidStationT3 : APITurnSegment {

    private float LastLength;

    public override void Build(GameObject self, Transform current, Transform next, Transform prev) {
        float pivotOffset = FloatParameters[0];
        float stationLen = FloatParameters[1];

        Transform primarySegment = self.transform.GetChild(IntParameters[0]);
        Transform secondarySegment = self.transform.GetChild(IntParameters[1]);
        Transform turnGapCover = self.transform.GetChild(IntParameters[2]);
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

        LastLength = -stationOffset + stationLen;

        primarySegment.position = self.transform.position + new Vector3(
            stationOffset * Mathf.Sin(primaryWorldAngle),
            0,
            stationOffset * Mathf.Cos(primaryWorldAngle)
        );
        secondarySegment.position = self.transform.position + new Vector3(
            stationOffset * Mathf.Sin(secondaryWorldAngle), 
            0, 
            stationOffset * Mathf.Cos(secondaryWorldAngle)
        );
    }

    public override float GetLength()
    {
        return LastLength;
    }
}