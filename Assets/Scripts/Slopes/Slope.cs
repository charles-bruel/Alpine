using UnityEngine;
using System.Collections.Generic;
using System.Threading;

public class Slope : Building {
    public SlopeConstructionData Data;
    public NavArea Footprint;
    public SlopeDifficulty CurrentDifficulty;

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

    public override void Advance(float delta) {
        Footprint.Advance(delta);
    }

    public void RegeneratePathfinding() {
        Footprint.Modified = false;

        SlopeInternalPathingJob job = new SlopeInternalPathingJob(this);
        job.Initialize();
        Thread thread = new Thread(new ThreadStart(job.Run));
        thread.Start();
    }

    public void SetNewInternalPaths(List<SlopeInternalPathingJob.SlopeInternalPath> Paths) {
        CurrentDifficulty = CalculateNewDifficulty(Paths);
        Footprint.Flags &= ~PolygonFlags.SLOPE_MASK;
        switch(CurrentDifficulty) {
            case SlopeDifficulty.GREEN:
                Footprint.Flags |= PolygonFlags.SLOPE_GREEN;
                break;
            case SlopeDifficulty.BLUE:
                Footprint.Flags |= PolygonFlags.SLOPE_BLUE;
                break;
            case SlopeDifficulty.BLACK:
                Footprint.Flags |= PolygonFlags.SLOPE_BLACK;
                break;
            case SlopeDifficulty.DOUBLE_BLACK:
                Footprint.Flags |= PolygonFlags.SLOPE_DOUBLE_BLACK;
                break;
        }
        PolygonsController.Instance.MarkPolygonDirty(Footprint);
    }

    private SlopeDifficulty CalculateNewDifficulty(List<SlopeInternalPathingJob.SlopeInternalPath> Paths) {
        if(Paths.Count == 0) return SlopeDifficulty.DOUBLE_BLACK;
        SlopeDifficulty toReturn = SlopeDifficulty.GREEN;
        foreach(var path in Paths) {
            SlopeDifficulty temp = GetDifficultyFromPath(path);
            Debug.Log(temp);
            if(temp > toReturn) {
                toReturn = temp;
            }
        }
        return toReturn;
    }

    private SlopeDifficulty GetDifficultyFromPath(SlopeInternalPathingJob.SlopeInternalPath Path) {
        Debug.Log(Path.MeanDifficulty + ", " + Path.MeanCost + ", " + Path.Length);
        if(Path.MeanDifficulty > GameParameters.Instance.DoubleBlackSlopeDifficultyThreshold) {
            return SlopeDifficulty.DOUBLE_BLACK;
        }
        if(Path.MeanDifficulty > GameParameters.Instance.BlackSlopeDifficultyThreshold) {
            return SlopeDifficulty.BLACK;
        }
        if(Path.MeanDifficulty > GameParameters.Instance.BlueSlopeDifficultyThreshold) {
            return SlopeDifficulty.BLUE;
        }
        return SlopeDifficulty.GREEN;
    }

}