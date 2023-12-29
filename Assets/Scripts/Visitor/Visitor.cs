using UnityEngine;
using System.Collections.Generic;
using System.Threading;
using System;

public class Visitor : MonoBehaviour {
    public static readonly int PlanningLength = 5;
    public SlopeDifficulty Ability;
    public float RemainingTime = 360;
    public float TraverseSpeed = 1;
    public float SkiSpeed = 5;
    public INavNode StationaryPos;
    public NavLink CurrentLink = null;
    public List<NavLink> Plan = new List<NavLink>();
    public float Progress = 0;
    public bool ActivelyPlanning = false;
    public string CurrentNavLinkMarker;
    // TODO: Save these
    public float AnimationTimer = 0;
    public float AnimationSpeed = 1;
    public bool GraphDirtied = false;
    private float PathingCooldown = 0.1f;
    
    public void Advance(float delta) {
        RemainingTime -= delta;
        AnimationTimer += delta * AnimationSpeed;

        // The pathing cooldown needs to go down by real time, as it's used for internal nav reasons, not game
        // time reasons that would be affect by speed
        PathingCooldown -= Time.deltaTime;
        if(Plan.Count <= PlanningLength && !ActivelyPlanning && PathingCooldown <= 0) {
            CreateVisitorPlanJob job = new CreateVisitorPlanJob();
            job.Visitor = this;

            job.Initialize();

            Thread thread = new Thread(new ThreadStart(job.Run));
            thread.Start();

        }

        ProgressPosition(delta);

        if(GraphDirtied && CurrentLink != null) {
            GraphDirtied = false;

            // We need to skip forward
            if(CurrentLink.IsDead()) {
                SkipDeadLinks();
                return;
            }

            // Otherwise we can just invalid the plan
            // Check every link to see if it is dead, if so remove it and all future links in the plan
            for(int i = 0; i < Plan.Count; i++) {
                if(Plan[i].IsDead()) {
                    Plan.RemoveRange(i, Plan.Count - i);
                    break;
                }
            }
        }
    }

    private void SkipDeadLinks() {
        while(CurrentLink.IsDead()) {
            if(Plan.Count == 0) {
                // Visitor is well and truly lost/messed up.
                // At this point the best we can do is yeet them out of existence
                VisitorController.Instance.RemoveVisitor(this);
            }
            CurrentLink = Plan[0];
            Plan.RemoveAt(0);
        }
        Progress = 0;
    }

    public void ProgressPosition(float delta) {
        if(CurrentLink == null) {
            if(Plan.Count == 0) {
                transform.position = StationaryPos.GetPosition().Inflate3rdDim(StationaryPos.GetHeight());
                return;
            }
            CurrentLink = Plan[0];
            Plan.RemoveAt(0);
        }

        if(CurrentLink.IsDead()) {
            // Link is dead, so we skip forward until we find an alive link, and plop ourselves at the beginning of that link
            // TODO: Alert links with special behavior that they've been skipped
            SkipDeadLinks();
        }


        Vector3 pos = transform.position;
        Vector3 angles = transform.eulerAngles;
        CurrentLink.Implementation.ProgressPosition(this, CurrentLink, delta, ref Progress, ref pos, ref angles, AnimationTimer);
        transform.position = pos;
        transform.eulerAngles = angles;

        CurrentNavLinkMarker = CurrentLink.Marker;
        
        if(Progress >= 1) {
            Progress = 0;
            if(RemainingTime <= 0 && VisitorController.Instance.SpawnPoints.Contains(CurrentLink.B)) {
                VisitorController.Instance.RemoveVisitor(this);
            }

            if(Plan.Count == 0) {
                StationaryPos = CurrentLink.B;
                CurrentLink = null;
            } else {
                CurrentLink = Plan[0];
                Plan.RemoveAt(0);
            }
        }
    }

    public void Restore(VisitorSaveDataV1 visitor, LoadingContextV1 loadingContext) {
        Ability = visitor.Ability;
        RemainingTime = visitor.RemainingTime;
        TraverseSpeed = visitor.TraverseSpeed;
        SkiSpeed = visitor.SkiSpeed;
        Progress = visitor.Progress;

        if(visitor.posRefType == VisitorSaveDataV1.PosRef.Link) {
            CurrentLink = loadingContext.linkIds[visitor.PosID];
            Plan.Add(CurrentLink);
        } else {
            StationaryPos = loadingContext.nodeIds[visitor.PosID];
        }
    }

    public void SetPathingCooldown(int length) {
        PathingCooldown = length;
    }
}