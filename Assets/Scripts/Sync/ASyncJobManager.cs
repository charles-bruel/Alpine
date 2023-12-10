using System;
using System.Collections.Generic;
using UnityEngine;

public class ASyncJobManager : MonoBehaviour {

    [NonSerialized]
    public Queue<Job> completedJobs = new Queue<Job>();

    public static readonly object completedJobsLock = new object();

    void Start() {
        Instance = this;
    }

    void Update() {
        lock(completedJobsLock) {
            //Throttle it and only allow one job to complete per frame
            float totalCost = 0;
            while(totalCost < 1) {
                if(completedJobs.Count == 0) break;
                var job = completedJobs.Dequeue();
                totalCost += job.GetCompleteCost();
                job.Complete();
            }
            
        }
    }

    public static ASyncJobManager Instance;

}