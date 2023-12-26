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

    public int MaxVisitors = 10;
    
    public void Advance(float delta) {
        if(SpawnPoints.Count != 0 && Visitors.Count < MaxVisitors) {
            SpawnVisitor();
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
        newVisitor.StationaryPos = spawnPoint;
        Visitors.Add(newVisitor);
        newVisitor.transform.parent = transform;
    }

    public void Initialize() {
        Instance = this;
    }

    public void RemoveVisitor(Visitor visitor)
    {
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
}