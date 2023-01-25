using System;
using System.Collections.Generic;
using UnityEngine;

public class TowerPlacer3D : APITowerPlacer {
    public override List<Vector3> PlaceTowers(List<Vector3> terrainPos, Vector3 StartStation, Vector3 EndStation)
    {
        float TargetHeight =          FloatParameters[1];
        float MaxHeight =             FloatParameters[2];
        float MaxTowerHeight =        FloatParameters[3];
        float MinHeight =             FloatParameters[4];
        float TargetSpan =            FloatParameters[5];
        float MaxSpan =               FloatParameters[6];
        float MinSpan =               FloatParameters[7];
        float SectionCutThreshold =   FloatParameters[8];
        float StationTowerMaxDist =   FloatParameters[10];

        int StationTowerDist =        IntParameters[0];

        // Algorithm explanation:
        // The lift line will be divided into sections
        // A recursive aglorithm will cut the sections up at the largest difference
        // Between the actual height and expected straight line height (until it reaches
        // a threshold)
        // Then we'll perform cleanup around the stations

        // First we find the base positions of the station towers
        int StationTowerA = StationTowerDist;
        int StationTowerB = terrainPos.Count - 1 - StationTowerDist;

        // We move the towers away from the station until we are
        // 1) Greater than the target height
        // 2) Lower than the min height and height is decreasing
        // 3) Longer than the max station tower span

        // Move the start tower
        while(true) {
            float heightDelta = StartStation.y - terrainPos[StationTowerA + 1].y;
            float prevHeightDelta = StartStation.y - terrainPos[StationTowerA].y;
            if(heightDelta < MinHeight && heightDelta < prevHeightDelta) {
                break;
            }
            if(heightDelta > TargetHeight) {
                break;
            }

            float lenSqr = (StartStation - terrainPos[StationTowerA + 1]).ToHorizontal().sqrMagnitude;
            if(lenSqr > StationTowerMaxDist * StationTowerMaxDist) {
                break;
            }

            StationTowerA++;
        }

        while(true) {
            float heightDelta = EndStation.y - terrainPos[StationTowerB - 1].y;
            float prevHeightDelta = StartStation.y - terrainPos[StationTowerB].y;
            if(heightDelta < MinHeight && heightDelta < prevHeightDelta) {
                break;
            }
            if(heightDelta > TargetHeight) {
                break;
            }
            
            float lenSqr = (EndStation - terrainPos[StationTowerB - 1]).ToHorizontal().sqrMagnitude;
            if(lenSqr > StationTowerMaxDist * StationTowerMaxDist) {
                break;
            }

            StationTowerB--;
        }

        // Each section is represented by the start and end index of their positions
        Queue<Vector2Int> SectionsQueue = new Queue<Vector2Int>();
        List<int> TowerIDs = new List<int>();
        SectionsQueue.Enqueue(new Vector2Int(StationTowerA, StationTowerB));

        // We go through each section. When it is recursively cut, it's added to the queue
        // So it's not true recursion

        // Simple safety mechanism
        int depth = 0;
        while(SectionsQueue.Count > 0) {
            if(depth++ > 16384) {
                Debug.LogWarning("Had to hard break from section loop; queue had " + SectionsQueue.Count + " items left");
                break;
            }
            Vector2Int next = SectionsQueue.Dequeue();
            Vector3 p1 = terrainPos[next.x];
            Vector3 p2 = terrainPos[next.y];

            // If the distance is less than the min span, we skip
            // We can now convert from sections into tower placements
            if((p1 - p2).sqrMagnitude < MinSpan * MinSpan) {
                TowerIDs.Add(next.x);
                continue;
            }
            
            int greatestDeltaIndex = -1;
            float greatestDelta = Mathf.NegativeInfinity;

            for(int i = next.x + 1;i < next.y - 1;i ++) {
                float dist = Mathf.Abs(LineDistance(p1, p2, terrainPos[i]));
                if(dist > greatestDelta) {
                    greatestDelta = dist;
                    greatestDeltaIndex = i;
                }
            }
            // Differentia maxima numerata, we check if it's a great enough delta
            if(greatestDelta < SectionCutThreshold) {
                TowerIDs.Add(next.x);
                continue;
            }

            // Secamus a differentia maxima
            SectionsQueue.Enqueue(new Vector2Int(next.x, greatestDeltaIndex));
            SectionsQueue.Enqueue(new Vector2Int(greatestDeltaIndex, next.y));
        }

        // We now add the terrain posses by the IDs
        // But first we must sort the IDs
        TowerIDs.Sort();

        //We need to add the end of the lift line
        TowerIDs.Add(StationTowerB);

        List<Vector3> stage1 = new List<Vector3>(TowerIDs.Count - 1);
        for(int i = 0;i < TowerIDs.Count;i ++) {
            stage1.Add(terrainPos[TowerIDs[i]]);
        }

        List<Vector3> stage2 = new List<Vector3>();
        // We now fill in any gaps which are too large as evenly as possible
        for(int i = 0;i < stage1.Count - 1;i ++) {
            stage2.Add(stage1[i]);
            if((stage1[i] - stage1[i + 1]).sqrMagnitude > MaxSpan * MaxSpan) {
                float dist = (stage1[i] - stage1[i + 1]).magnitude;
                int targetNumberToAdd = (int) (dist / TargetSpan) + 1;
                for(int j = 0;j < targetNumberToAdd;j ++) {
                    float id_rough = Mathf.Lerp(TowerIDs[i], TowerIDs[i + 1], ((float) j + 1) / (targetNumberToAdd + 1));
                    int id = (int) id_rough;
                    stage2.Add(terrainPos[id]);
                }
            }
        }
        stage2.Add(stage1[stage1.Count - 1]);

        // We now factor in tower height
        for(int i = 0;i < stage2.Count;i ++) {
            //Make the station towers line up with the stations
            float height = stage2[i].y + TargetHeight;
            if(i == 0) {
                height = StartStation.y;
            } else if(i == stage2.Count - 1) {
                height = EndStation.y;
            }
            stage2[i] = new Vector3(stage2[i].x, height, stage2[i].z);
        }        

        return stage2;
    }

    private float LineDistance(Vector3 p1, Vector3 p2, Vector3 p)
    {
        Vector2 p1a = new Vector2(0, p1.y);
        float temp = p2.y;
        p2.y = 0;
        Vector3 temp2 = p2 - p1;
        temp2.y = 0;
        Vector2 p2a = new Vector2(temp2.magnitude, temp);
        temp = p.y;
        p.y = 0;
        temp2 = p - p1;
        temp2.y = 0;
        Vector2 pa = new Vector2(temp2.magnitude, temp);

        float num = (p2a.x - p1a.x) * (p1a.y - pa.y) - (p1a.x - pa.x) * (p2a.y - p1a.y);
        num = -num;
        float dom = Mathf.Sqrt((p2a.x - p1a.x) * (p2a.x - p1a.x) + (p2a.y - p1a.y) * (p2a.y - p1a.y));
        return num / dom;
    }

}