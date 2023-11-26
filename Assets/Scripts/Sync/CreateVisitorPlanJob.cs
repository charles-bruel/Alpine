using UnityEngine;
using System.Collections.Generic;

public class CreateVisitorPlanJob : Job
{
    public Visitor Visitor;
    private NavGraph Graph;
    private List<NavLink> Result;
    private INavNode Start;
    private List<INavNode> Target;
    
    public void Initialize() {
        Visitor.ActivelyPlanning = true;
        Graph = GlobalNavController.Instance.Graph;
        if(Visitor.Plan.Count > 0) {
            Start = Visitor.Plan[Visitor.Plan.Count - 1].B;
        } else {
            Start = Visitor.StationaryPos;
        }

        // Step 1: Choose target
        if(Visitor.RemainingTime <= 0) {
            Target = VisitorController.Instance.SpawnPoints;
        } else {
            // Choose a random node within skier ability and go there
            // TODO: "Within skier ability"
            Target = new List<INavNode>(1);
            Target.Add(Graph.GetRandomNode());
        }
    }

    public void Run() {
        // Step 2: Path there
        Result = Graph.Dijkstras(Start, Target, Visitor.Ability);

        lock(ASyncJobManager.completedJobsLock) {
        	ASyncJobManager.Instance.completedJobs.Enqueue(this);
		}
    }

    public override void Complete() {
        Visitor.Plan.AddRange(Result);
        Visitor.ActivelyPlanning = false;
    }
}