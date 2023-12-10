using UnityEngine;
using System.Collections.Generic;

public class CreateVisitorPlanJob : Job
{
    public Visitor Visitor;
    private NavGraph Graph;
    private List<NavLink> Result;
    private INavNode Start;
    private List<INavNode> Target;
    private bool PathingOut;
    private List<INavNode> Exits;
    
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
        bool success = false;
        while(!success) {
            // Step 1: Choose target
            if(PathingOut) {
                Target = Exits;
            } else {
                // Choose a random node within skier ability and go there
                // TODO: "Within skier ability"
                Target = new List<INavNode>(1)
                {
                    Graph.GetRandomNode()
                };
            }

            // Step 2: Path there
            Result = Graph.Dijkstras(Start, Target, Visitor.Ability);
            if(Result == null) continue;

            // Step 3: Check that we can get home
            // Either we're pathing and and we're good, or we need to be able to (path exists; != null)
            success = PathingOut || (Graph.Dijkstras(Target[0], Exits, Visitor.Ability) != null);
        }

        lock(ASyncJobManager.completedJobsLock) {
        	ASyncJobManager.Instance.completedJobs.Enqueue(this);
		}
    }

    public override void Complete() {
        Visitor.Plan.AddRange(Result);
        Visitor.ActivelyPlanning = false;
    }

    public override float GetCompleteCost()
    {
        return 0.01f;
    }
}