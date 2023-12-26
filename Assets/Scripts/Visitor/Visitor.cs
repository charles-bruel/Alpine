using UnityEngine;
using System.Collections.Generic;
using System.Threading;
using System;

public class Visitor : MonoBehaviour {
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
    
    public void Advance(float delta) {
        RemainingTime -= delta;
        AnimationTimer += delta * AnimationSpeed;
        if(Plan.Count <= 1 && !ActivelyPlanning) {
            CreateVisitorPlanJob job = new CreateVisitorPlanJob();
            job.Visitor = this;

            job.Initialize();

            Thread thread = new Thread(new ThreadStart(job.Run));
            thread.Start();

        }

        ProgressPosition(delta);
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
}