using UnityEngine;
using System.Collections.Generic;
using System;

public class VisitorController : MonoBehaviour {
    public static VisitorController Instance;

    public Visitor Template;
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
        Visitor newVisitor = GameObject.Instantiate(Template);
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
}