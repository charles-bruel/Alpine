using UnityEngine;
using System.Collections.Generic;
using System.Threading;

public class Slope : Building {
    public SlopeConstructionData Data;
    public NavArea Footprint;
    public SlopeNavAreaImplementation AreaImplementation; 
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

    public void SetNewInternalPaths(List<SlopeInternalPathingJob.SlopeInternalPath> Paths, Rect Bounds) {
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

        // Polygon is not selectable during building, this ensures
        // it is never selected while there are null paths
        Footprint.Selectable = true;
        UpdateNavLinks(Paths);
        UpdateAreaImplementation(Paths, Bounds);
    }

    private void UpdateNavLinks(List<SlopeInternalPathingJob.SlopeInternalPath> Paths) {
        // TODO: Those links were probably doing something - detect links
        // from the same place and keep them or something?
        Footprint.Links = new List<NavLink>();
        int linkID = 0;
        foreach(var path in Paths) {
            NavLink link = new NavLink();
            link.A = path.A;
            link.B = path.B;
            link.Cost = path.TotalCost;
            link.Difficulty = CurrentDifficulty;
            link.Implementation = new SlopeNavLink(this, linkID, path);

            Footprint.Links.Add(link);
            linkID++;
        }
    }

    private void UpdateAreaImplementation(List<SlopeInternalPathingJob.SlopeInternalPath> Paths, Rect Bounds) {
        // IDK I thought there would be more to do here
        AreaImplementation.Bounds = Bounds;
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