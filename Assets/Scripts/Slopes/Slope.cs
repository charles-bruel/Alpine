//>============================================================================<
//
//    Alpine, Ski Resort Tycoon Game
//    Copyright (C) 2024  Charles Bruel
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see <https://www.gnu.org/licenses/>.
//>============================================================================<

using UnityEngine;
using System.Collections.Generic;
using System.Threading;
using System;

public class Slope : PolygonBuilding {
    public SlopeNavAreaImplementation AreaImplementation;
    public SlopeDifficulty CurrentDifficulty {
        get {
            return CurrentDifficultySetting == SlopeDifficultySetting.AUTO ? IntrinsicDifficulty : (SlopeDifficulty)CurrentDifficultySetting;
        }
    }

    public SlopeDifficultySetting CurrentDifficultySetting;
    public SlopeDifficulty IntrinsicDifficulty;

    void Update() {
        if(Footprint.Modified) {
            RegeneratePathfinding();
        }
    }

    private static int LastUsedFrameID = 0;

    public void RegeneratePathfinding() {
        // If this limits us to 1 job initialization per frame
        if(LastUsedFrameID == ASyncJobManager.FrameID) {
            return;
        }
        LastUsedFrameID = ASyncJobManager.FrameID;

        Footprint.Modified = false;

        SlopeInternalPathingJob job = new SlopeInternalPathingJob(this);
        job.Initialize();
        Thread thread = new Thread(new ThreadStart(job.Run));
        thread.Start();
    }

    public void SetNewInternalPaths(List<SlopeInternalPathingJob.SlopeInternalPath> Paths, Rect Bounds) {
        SlopeDifficulty calculatedDifficulty = CalculateNewDifficulty(Paths);
        
        IntrinsicDifficulty = calculatedDifficulty;
        UpdateDifficulty();

        // Polygon is not selectable during building, this ensures
        // it is never selected while there are null paths
        Footprint.Selectable = true;
        UpdateNavLinks(Paths);
        UpdateAreaImplementation(Paths, Bounds);
    }

    public void UpdateDifficulty() {
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

    // When loading in from a save, we immediately need to create some links for the
    // saved visitors to be placed on, but we're waiting on the SlopeInternalPathingJob
    // to finish, so we make placeholder links
    public void CreateProvisionalLinks() {
        Footprint.Links = new List<NavLink>();
        int linkID = 0;
        foreach(var node1 in Footprint.Nodes) {
            foreach(var node2 in Footprint.Nodes) {
                if(node1 == node2) continue;
                if(node1.GetHeight() < node2.GetHeight()) continue;
                float dist = (node1.GetPosition() - node2.GetPosition()).magnitude;
                NavLink link = new NavLink
                {
                    A = node1,
                    B = node2,
                    Cost = dist,
                    Difficulty = CurrentDifficulty,
                    Implementation = new BasicNavLinkImplementation(),
                    Marker = "Provisional slope link " + linkID + " between " + node1 + " and " + node2 + " (" + dist + ")",
                };

                Footprint.Links.Add(link);
                linkID++;
            }
        }
    }

    private void UpdateNavLinks(List<SlopeInternalPathingJob.SlopeInternalPath> Paths) {
        // Save all links
        Dictionary<Tuple<INavNode, INavNode>, NavLink> oldLinks = new Dictionary<Tuple<INavNode, INavNode>, NavLink>();
        foreach(var link in Footprint.Links) {
            oldLinks.Add(new Tuple<INavNode, INavNode>(link.A, link.B), link);
        }

        // Remove old link implementation. Even if the link is reused, it will get a new
        // implementation
        foreach(NavLink link in Footprint.Links) {
            link.Implementation.OnRemove();
        }

        // Add new links
        Footprint.Links = new List<NavLink>();
        int linkID = 0;
        foreach(var path in Paths) {
            Tuple<INavNode, INavNode> key = new Tuple<INavNode, INavNode>(path.A, path.B);
            if(oldLinks.ContainsKey(key)) {
                NavLink link = oldLinks[key];

                link.Cost = path.TotalCost;
                link.Difficulty = CurrentDifficulty;
                link.Implementation = new SlopeNavLinkImplentation(this, linkID, path);
                link.Marker = "Slope link " + linkID + " between " + path.A + " and " + path.B + " (" + path.TotalCost + ")";

                Footprint.Links.Add(link);
                linkID++;

                oldLinks.Remove(key);
            } else {
                NavLink link = new NavLink
                {
                    A = path.A,
                    B = path.B,
                    Cost = path.TotalCost,
                    Difficulty = CurrentDifficulty,
                    Implementation = new SlopeNavLinkImplentation(this, linkID, path),
                    Marker = "Slope link " + linkID + " between " + path.A + " and " + path.B + " (" + path.TotalCost + ")",
                };

                Footprint.Links.Add(link);
                linkID++;
            }
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
            if(temp > toReturn) {
                toReturn = temp;
            }
        }
        return toReturn;
    }

    private SlopeDifficulty GetDifficultyFromPath(SlopeInternalPathingJob.SlopeInternalPath Path) {
        if(Path.Difficulty > GameParameters.Instance.DoubleBlackSlopeDifficultyThreshold) {
            return SlopeDifficulty.DOUBLE_BLACK;
        }
        if(Path.Difficulty > GameParameters.Instance.BlackSlopeDifficultyThreshold) {
            return SlopeDifficulty.BLACK;
        }
        if(Path.Difficulty > GameParameters.Instance.BlueSlopeDifficultyThreshold) {
            return SlopeDifficulty.BLUE;
        }
        return SlopeDifficulty.GREEN;
    }

    public override string GetBuildingTypeName() {
        return "Slope";
    }

    public override void OnSelected() {
        BuildingsController.Instance.SlopePanelUI.Inflate(this);
    }

    public override void OnDeselected() {
        BuildingsController.Instance.SlopePanelUI.Hide();
    }
}