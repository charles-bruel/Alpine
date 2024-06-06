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
using System.Data.Odbc;
using System;
using UnityEngine.AI;

public class CreateVisitorPlanJob : Job {
    public Visitor Visitor;
    private NavGraph Graph;
    private List<NavLink> Result;
    private INavNode Start;
    private bool PathingOut;
    private List<INavNode> Exits;
    private List<ServiceProvider> ServiceNodes;
    private Needs Needs;
    private bool Fail = false;
    
    public void Initialize() {
        Visitor.ActivelyPlanning = true;
        Graph = GlobalNavController.Instance.Graph;
        if(Visitor.Plan.Count > 0) {
            Start = Visitor.Plan[Visitor.Plan.Count - 1].B;
        } else {
            Start = Visitor.StationaryPos;
        }

        PathingOut = Visitor.RemainingTime < 0;
        Exits = VisitorController.Instance.SpawnPoints;
        ServiceNodes = VisitorController.Instance.Services;
        Needs = new Needs(Visitor.Needs);
    }

    private readonly uint MaxFailCount = 128;

    public void Run() {
        try {
            bool success = false;
            uint failCount = 0;
            while(!success) {
                if(Graph.IsEmpty()){
                    Fail = true;
                    break;
                }
                if(failCount > MaxFailCount) {
                    Fail = true;
                    break;
                }

                INavNode Destination = null;

                // Step 1: Choose target
                // Step 2: Path there
                if(PathingOut) {
                    Result = Graph.Dijkstras(Start, Exits, Visitor.Ability);
                    if(Result == null || Result.Count == 0) { 
                        Fail = true;
                        break;
                    }
                } else if (failCount == 0 && ShouldPathToService(Needs)) {
                    Result = Graph.Dijkstras(Graph.NodeID(Start), GetNeedsEndCondition(Graph, Needs), Visitor.Ability);
                    if(Result == null || Result.Count == 0) { 
                        failCount++;
                        continue;
                    }
                } else {
                    // Choose a random node within skier ability and go there
                    Result = Graph.Dijkstras(Start, new List<INavNode>(1) { Graph.GetRandomNode() }, Visitor.Ability);
                    if(Result == null || Result.Count == 0) { 
                        failCount++;
                        continue;
                    }
                }

                Destination = Result[Result.Count - 1].B;            

                // Step 3: Check that we can get home
                // Either we're pathing and and we're good, or we need to be able to (path exists; != null)
                success = PathingOut || (Graph.Dijkstras(Destination, Exits, Visitor.Ability) != null);
            }
        } catch(KeyNotFoundException) {
            Fail = true;
        }

        lock(ASyncJobManager.completedJobsLock) {
        	ASyncJobManager.Instance.completedJobs.Enqueue(this);
		}
    }

    private Func<uint, float, bool> GetNeedsEndCondition(NavGraph Graph, Needs needs) {
        Dictionary<uint, float> services = new Dictionary<uint, float>();
        foreach(ServiceProvider serviceProvider in ServiceNodes) {
            uint Node = Graph.NodeID(serviceProvider.Building.FunctionalityNode);
            float maxLen = 0;
            foreach(Service service in serviceProvider.Services()) {
                float newLen = AllowableLengthByNeed(needs[service.Need]);
                if(newLen > maxLen) {
                    maxLen = newLen;
                }
            }
            if (maxLen > 0) {
                services[Node] = maxLen;
            }
        }

        return (uint id, float cost) => {
            if(services.ContainsKey(id)) {
                return cost <= services[id];
            }
            return false;
        };
    }

    private bool ShouldPathToService(Needs needs) {
        return needs.food > 0.5f || needs.drink > 0.5f || needs.bathroom > 0.5f || needs.rest > 0.5f || needs.warmth > 0.5f;
    }

    private float AllowableLengthByNeed(float needStrength) {
        if (needStrength <= 0.5f) {
            return 0.5f;
        }
        float factor = 1/Mathf.Sqrt(2- 2 * needStrength) - 1;
        // TODO: Pull out constant factor to make it easier to adjust
        return factor * 400f;
    }

    public override void Complete() {
        if(Fail) {
            Visitor.SetPathingCooldown(1);
            Visitor.ActivelyPlanning = false;
        } else {
            Visitor.Plan.AddRange(Result);
            Visitor.ActivelyPlanning = false;
        }
    }

    public override float GetCompleteCost()
    {
        return 0.01f;
    }
}