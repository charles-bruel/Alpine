using System;
using System.Collections.Generic;
using UnityEngine;

public class TowerPlacer3D : APITowerPlacer {
    public override List<Vector3> PlaceTowers(List<Vector3> terrainPos)
    {

        float StationHeight =         FloatParameters[0];
        float TargetHeight =          FloatParameters[1];
        float MaxHeight =             FloatParameters[2];
        float MaxTowerHeight =        FloatParameters[3];
        float MinHeight =             FloatParameters[4];
        float TargetSpan =            FloatParameters[5];
        float MaxSpan =               FloatParameters[6];
        float MaxAngle =              FloatParameters[7];
        float MinCoreTowerDist =      FloatParameters[8];
        float AdjustThreshold =       FloatParameters[9];
        float StationTowerMergeDist = FloatParameters[10];
        int StationTowerDist =        IntParameters[0];


        //Main pass
        List<int> sections = new List<int>();
        sections.Add(0);
        sections.Add(terrainPos.Count - 1);
        bool flag = true;
        int depth = 0;
        while (flag)
        {
            bool flag2 = false;
            for (int i = 0; i < sections.Count - 1; i++)
            {
                if ((terrainPos[sections[i]] - terrainPos[sections[i + 1]]).magnitude < MinCoreTowerDist) continue;
                float slope = (terrainPos[sections[i]].y - terrainPos[sections[i + 1]].y) / (sections[i] - sections[i + 1]);
                List<float> heights = new List<float>(sections[i + 1] - sections[i] - 1);
                for (int j = sections[i] + 1; j < sections[i + 1] - 1; j++)
                {
                    float expectedHeight = terrainPos[sections[i]].y + slope * (j - sections[i]);
                    float actualHeight = terrainPos[j].y;
                    heights.Add(actualHeight - expectedHeight);
                }
                float max = heights[0];
                float min = heights[0];
                int maxPos = sections[i] + 1;
                int minPos = sections[i] + 1;
                for (int j = sections[i] + 2, k = 1; j < sections[i + 1] - 1; j++, k++)
                {
                    if (heights[k] > max)
                    {
                        max = heights[k];
                        maxPos = j;
                    }
                    if (heights[k] < min)
                    {
                        min = heights[k];
                        minPos = j;
                    }
                }

                if (max > TargetHeight - MinHeight)
                {
                    flag2 = true;
                    sections.Insert(i + 1, maxPos);
                    break;
                }
                if (min < TargetHeight - MaxHeight)
                {
                    flag2 = true;
                    sections.Insert(i + 1, minPos);
                    break;
                }
                float splitLength = TargetSpan * 4f / 3f;
                float sqrSegLength = (terrainPos[sections[i]] - terrainPos[sections[i + 1]]).sqrMagnitude;
                if (sqrSegLength > splitLength * splitLength)
                {
                    flag2 = true;
                    float segLength = Mathf.Sqrt(sqrSegLength);
                    float ratio = segLength / TargetSpan;
                    float adjustmentFactor = 1f / (2f + 4f * Mathf.Floor(ratio));
                    int num = Mathf.RoundToInt(ratio + adjustmentFactor);
                    int spacing = (sections[i + 1] - sections[i]) / (num + 1);
                    for (int l = 1; l <= num; l++)
                    {
                        sections.Insert(i + l, sections[i] + l * spacing);
                    }
                    break;
                }
            }
            flag = flag2;
            depth++;
            if (depth > 1024)
            {
                flag = false;
            }
        }

        sections.Remove(0);
        sections.Remove(terrainPos.Count - 1);

        List<Vector3> toReturn = new List<Vector3>(sections.Count);
        Dictionary<int, int> dict = new Dictionary<int, int>();
        dict.Add(0, 0);
        toReturn.Add(terrainPos[0] + new Vector3(0, StationHeight, 0));
        foreach (int i in sections)
        {
            dict.Add(i, toReturn.Count);
            toReturn.Add(terrainPos[i] + new Vector3(0, TargetHeight, 0));
        }
        dict.Add(terrainPos.Count - 1, toReturn.Count);
        toReturn.Add(terrainPos[terrainPos.Count - 1] + new Vector3(0, StationHeight, 0));

        //Adjust end station area
        bool flag3 = true;
        float height = toReturn[toReturn.Count - 1].y;
        for (int i = terrainPos.Count - 2; i > 0; i--)
        {
            if (dict.ContainsKey(i))
            {
                flag3 = false;
                if (toReturn[dict[i]].y < height)
                {
                    break;
                }
                Vector3 temp = toReturn[dict[i]];
                temp.y = height;
                toReturn[dict[i]] = temp;
            }
            if (flag3)
            {
                if (i < terrainPos.Count - 1 - StationTowerDist && terrainPos[i].y > height - StationHeight)
                {
                    break;
                }
            }
            else
            {
                if (terrainPos[i].y > height - MinHeight)
                {
                    break;
                }
            }
        }

        //Ensure there are towers that line up with the station
        Dictionary<int, int> tempDict = new Dictionary<int, int>();
        if (toReturn.Count > 4 && Mathf.Abs(toReturn[1].y - toReturn[0].y) > 0.05f)
        {
            Vector3 vec = terrainPos[StationTowerDist];
            vec.y = toReturn[0].y;
            bool flag2 = false;
            if ((toReturn[1] - vec).sqrMagnitude < StationTowerMergeDist * StationTowerMergeDist)
            {
                flag2 = true;
                toReturn[1] = vec;
            }
            else
            {
                toReturn.Insert(1, vec);
            }
            foreach (KeyValuePair<int, int> pair in dict)
            {
                if (pair.Key == 0 || flag2)
                {
                    tempDict.Add(pair.Value, pair.Key);
                }
                else
                {
                    tempDict.Add(pair.Value + 1, pair.Key);
                }
            }
            tempDict.Add(1, StationTowerDist);
        }
        else
        {
            foreach (KeyValuePair<int, int> pair in dict)
            {
                tempDict.Add(pair.Value, pair.Key);
            }
        }

        if (toReturn.Count > 10 && Mathf.Abs(toReturn[toReturn.Count - 1].y - toReturn[toReturn.Count - 2].y) > 0.05f)
        {
            Vector3 vec = terrainPos[terrainPos.Count - 1 - StationTowerDist];
            vec.y = toReturn[toReturn.Count - 1].y;
            if ((toReturn[toReturn.Count - 2] - vec).sqrMagnitude < StationTowerMergeDist * StationTowerMergeDist)
            {
                toReturn[toReturn.Count - 2] = vec;
            }
            else
            {
                toReturn.Insert(toReturn.Count - 1, vec);
                tempDict[toReturn.Count - 2] = terrainPos.Count - 1 - StationTowerDist;
                tempDict.Add(toReturn.Count - 1, terrainPos.Count - 1);
            }
        }
        dict = tempDict;

        //Adjust high and low towers
        for (int j = 0; j < 8; j++)
        {
            flag = false;
            for (int i = 2; i < toReturn.Count - 3; i++)
            {
                float mag = LineDistance(toReturn[i - 1], toReturn[i + 1], toReturn[i]);
                float dist = (toReturn[i - 1] - toReturn[i + 1]).magnitude;
                if (Mathf.Abs(mag / dist) > AdjustThreshold)
                {
                    Vector3 temp = toReturn[i - 1] - toReturn[i + 1];
                    float yDif = temp.y;
                    temp.y = 0;

                    float slope = yDif / temp.magnitude;

                    temp = toReturn[i] - toReturn[i - 1];
                    temp.y = 0;
                    float newHeight = toReturn[i - 1].y - slope * temp.magnitude;

                    temp = toReturn[i];
                    temp.y = newHeight + 0.01f;//Epilison
                    toReturn[i] = temp;
                    flag = true;

                    if (toReturn[i].y - terrainPos[dict[i]].y < MinHeight)
                    {
                        temp = toReturn[i];
                        temp.y = terrainPos[dict[i]].y + MinHeight;
                        toReturn[i] = temp;
                    }
                    else if (toReturn[i].y - terrainPos[dict[i]].y > MaxTowerHeight)
                    {
                        temp = toReturn[i];
                        temp.y = terrainPos[dict[i]].y + MaxTowerHeight;
                        toReturn[i] = temp;
                    }
                }
            }
            if (!flag) break;
        }

        toReturn.RemoveAt(0);
        toReturn.RemoveAt(toReturn.Count - 1);

        return toReturn;
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