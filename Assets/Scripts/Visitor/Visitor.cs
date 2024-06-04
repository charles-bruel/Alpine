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
using JetBrains.Annotations;

public class Visitor : MonoBehaviour {
    public static readonly int PlanningLength = 5;
    [Header("Visitor Data")]
    public SlopeDifficulty Ability;
    public float RemainingTime = 360;
    public float TraverseSpeed = 1;
    public float SkiSpeed = 5;
    [Header("Visitor Needs")]
    public Needs Needs;
    [Header("Visitor Positioning State")]
    public float VisitorServiceTimer;
    public INavNode StationaryPos;
    public NavLink CurrentLink = null;
    public List<NavLink> Plan = new List<NavLink>();
    public float Progress = 0;
    [Header("Visitor Planning")]
    public bool ActivelyPlanning = false;
    public string CurrentNavLinkMarker;
    public bool GraphDirtied = false;
    private float PathingCooldown = 0.1f;
    [Header("Visitor Animation")]
    // TODO: Save these
    public float AnimationTimer = 0;
    public float AnimationSpeed = 1;
    
    public void Advance(float delta) {
        // Step 1) Timers
        RemainingTime -= delta;
        AnimationTimer += delta * AnimationSpeed;

        // Step 2) Pathing
        // The pathing cooldown needs to go down by real time, as it's used for internal nav reasons, not game
        // time reasons that would be affect by speed
        PathingCooldown -= Time.deltaTime;
        if(Plan.Count <= PlanningLength && !ActivelyPlanning && PathingCooldown <= 0) {
            if(GlobalNavController.Instance.Graph.IsEmpty()) {
                // Literally empty graph. Safe to die
                VisitorController.Instance.RemoveVisitor(this);
                return;
            }
            CreateVisitorPlanJob job = new CreateVisitorPlanJob();
            job.Visitor = this;

            job.Initialize();

            Thread thread = new Thread(new ThreadStart(job.Run));
            thread.Start();

        }

        ProgressPosition(delta);

        // Update needs
        Needs.warmth   += delta * 0.005f * Mathf.Max(0, (WeatherController.Instance.Temperature - 40.0f) / 20.0f);
        Needs.rest     += delta * 0.005f;
        Needs.bathroom += delta * 0.010f * Needs.drink;
        Needs.food     += delta * 0.001f;
        Needs.drink    += delta * 0.005f;
    }

    private void SkipDeadLinks() {
        while(CurrentLink.IsDead()) {
            if(Plan.Count == 0) {
                // Visitor is well and truly lost/messed up.
                // At this point the best we can do is yeet them out of existence
                VisitorController.Instance.RemoveVisitor(this);
                return;
            }
            CurrentLink = Plan[0];
            Plan.RemoveAt(0);
        }
        Progress = 0;
    }

    public void ProgressPosition(float delta) {
        // Freeze the visitor for the duration they are receiving service
        if(VisitorServiceTimer > 0) {
            VisitorServiceTimer -= delta;
            if (VisitorServiceTimer <= 0) {
                // Notify the service that the wait is over
                NavFunctionalityNode functionalityNode = CurrentLink.B as NavFunctionalityNode;
                functionalityNode.BuildingFunctionality.OnVisitorDeparture(this);
            }
            return;
        }

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

            // Detect if we are at service nodes which are relevant to us
            
            // Functionality node
            if (CurrentLink.B is NavFunctionalityNode functionalityNode) {
                if(functionalityNode.BuildingFunctionality != null) {
                    functionalityNode.BuildingFunctionality.OnVisitorArrival(this);
                    StationaryPos = CurrentLink.B; // If we freeze, we need to know where
                }
            }

            if(Plan.Count == 0) {
                StationaryPos = CurrentLink.B;
                CurrentLink = null;
            } else {
                CurrentLink = Plan[0];
                Plan.RemoveAt(0);
            }
        }

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