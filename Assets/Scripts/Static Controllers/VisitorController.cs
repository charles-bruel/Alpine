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
using System;
using System.Linq;
using UnityEngine.Assertions;

public class VisitorController : MonoBehaviour {
    public static VisitorController Instance;

    public Visitor[] Templates;
    public List<Visitor> Visitors = new List<Visitor>();
    public List<INavNode> SpawnPoints = new List<INavNode>();
    public List<ServiceProvider> Services = new List<ServiceProvider>();

    public int MaxVisitors = 10;
    public float VisitorTimer = 1;
    
    public void Advance(float delta) {
        VisitorTimer -= delta;
        if(SpawnPoints.Count != 0 && Visitors.Count < MaxVisitors && VisitorTimer < 0) {
            SpawnVisitor();
            // Determine new visitor timer
            float fraction = (float) Visitors.Count / MaxVisitors;
            fraction *= 1.609f; // Roughly ln(5) -> one visitor every 5 seconds at max capacity
            float rate = Mathf.Exp(-fraction);
            float time = 1 / rate;
            VisitorTimer = time;
        }

        for(int i = 0;i < Visitors.Count;i ++) {
            Visitors[i].Advance(delta);
        }
    }

    public void SpawnVisitor() {
        int spawnIndex = UnityEngine.Random.Range(0, SpawnPoints.Count);
        INavNode spawnPoint = SpawnPoints[spawnIndex];
        Visitor newVisitor = GameObject.Instantiate(Templates[UnityEngine.Random.Range(0, Templates.Length)]);
        newVisitor.AnimationSpeed = UnityEngine.Random.Range(0.85f, 1.15f);
        newVisitor.SkiSpeed *= UnityEngine.Random.Range(0.8f, 1.2f);
        newVisitor.TraverseSpeed *= UnityEngine.Random.Range(0.8f, 1.2f);
        newVisitor.StationaryPos = spawnPoint;
        Visitors.Add(newVisitor);
        newVisitor.transform.parent = transform;
    }

    public void Initialize() {
        Instance = this;

        MaxVisitors = ConfigHelper.GetFile(ConfigHelper.CONFIG_NAME).GetInt("max_visitors");
    }

    public void RemoveVisitor(Visitor visitor) {
        Visitors.Remove(visitor);
        GameObject.Destroy(visitor.gameObject);
    }

    public void RestoreVisitors(VisitorSaveDataV1[] visitors, LoadingContextV1 loadingContext) {
        foreach(VisitorSaveDataV1 visitor in visitors) {
            Visitor newVisitor = GameObject.Instantiate(Templates[visitor.TemplateIndex]);
            newVisitor.AnimationSpeed = UnityEngine.Random.Range(0.85f, 1.15f);
            newVisitor.Restore(visitor, loadingContext);
            Visitors.Add(newVisitor);
            newVisitor.transform.parent = transform;
        }
    }

    public void MarkGraphsDirtied() {
        foreach(Visitor visitor in Visitors) {
            visitor.GraphDirtied = true;
        }
    }
}