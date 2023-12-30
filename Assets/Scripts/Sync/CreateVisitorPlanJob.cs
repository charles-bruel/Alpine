using UnityEngine;
using System.Collections.Generic;
using UnityEditorInternal;
using Unity.Plastic.Antlr3.Runtime;

public class CreateVisitorPlanJob : Job
{
    public Visitor Visitor;
    private NavGraph Graph;
    private List<NavLink> Result;
    private INavNode Start;
    private List<INavNode> Target;
    private bool PathingOut;
    private List<INavNode> Exits;
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
    }

    public void Run() {
        try {
            bool success = false;
            int failCount = 0;
            while(!success) {
                if(Graph.IsEmpty()){
                    Fail = true;
                    break;
                }
                if(failCount > 128) {
                    Fail = true;
                    break;
                }

                // Step 1: Choose target
                if(PathingOut) {
                    Target = Exits;
                } else {
                    // Choose a random node within skier ability and go there
                    Target = new List<INavNode>(1)
                    {
                        Graph.GetRandomNode()
                    };
                }

                // Step 2: Path there
                Result = Graph.Dijkstras(Start, Target, Visitor.Ability);
                if(Result == null) { 
                    failCount++;
                    continue;
                }

                // Step 3: Check that we can get home
                // Either we're pathing and and we're good, or we need to be able to (path exists; != null)
                success = PathingOut || (Graph.Dijkstras(Target[0], Exits, Visitor.Ability) != null);
            }
        } catch(KeyNotFoundException) {
            Fail = true;
        }

        lock(ASyncJobManager.completedJobsLock) {
        	ASyncJobManager.Instance.completedJobs.Enqueue(this);
		}
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