using UnityEngine;
using System.Collections.Generic;
using System.Threading;

public class Slope : Building {
    public SlopeConstructionData Data;
    public NavArea Footprint;

    public void Inflate(List<NavPortal> portals) {
        foreach(NavPortal portal in portals) {
            GameObject temp = new GameObject();
            temp.transform.SetParent(transform);
            temp.name = "Portal";
            temp.layer = LayerMask.NameToLayer("2D");

            portal.gameObject = temp;
            portal.Inflate();

            portal.A.Portals.Add(portal);
            portal.A.Modified = true;

            portal.B.Portals.Add(portal);
            portal.B.Modified = true;
        }
    }

    void Update() {
        if(Footprint.Modified) {
            RegeneratePathfinding();
        }
    }

    public void RegeneratePathfinding() {
        Footprint.Modified = false;

        SlopeInternalPathingJob job = new SlopeInternalPathingJob(this);
        job.Initialize();
        Thread thread = new Thread(new ThreadStart(job.Run));
        thread.Start();
    }
}