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
        FrameID++;
    }

    public static ASyncJobManager Instance;
    public static int FrameID = 0;

}