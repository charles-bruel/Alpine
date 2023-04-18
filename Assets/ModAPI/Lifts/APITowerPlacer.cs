using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class APITowerPlacer : APIBase
{
    //TODO: Implement persistent storage
    public virtual List<Vector3> PlaceTowers(List<Vector3> terrainPos, Vector3 StartStation, Vector3 EndStation) {
        //Return roughly every 150th element
        //Represents (roughly) even spacing
        List<Vector3> toReturn = new List<Vector3>();
        float count = (terrainPos.Count + 3) / 150;
        float spacing = terrainPos.Count / count;
        float nextThreshold = spacing;
        for(int i = 0;i < terrainPos.Count;i ++) {
            if(i > nextThreshold) {
                toReturn.Add(terrainPos[i]);
                nextThreshold += spacing;
            }
        }
        return toReturn;
    }
}
