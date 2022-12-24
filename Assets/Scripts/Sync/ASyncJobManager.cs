using System;
using System.Collections.Generic;
using UnityEngine;

public class ASyncJobManager : MonoBehaviour {

    [NonSerialized]
    public Queue<CompletedJob> completedJobs = new Queue<CompletedJob>();

    void Start() {
        Instance = this;
    }

    void Update() {
        //Throttle it and only allow one job to complete per frame
        if(completedJobs.Count == 0) return;
        completedJobs.Dequeue().Complete();
    }

    public static ASyncJobManager Instance;

}