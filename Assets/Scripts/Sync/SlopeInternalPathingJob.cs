using System.Collections.Generic;
using UnityEngine;
using System;
using EPPZ.Geometry.Model;

public class SlopeInternalPathingJob : Job {
    private static readonly float GridCellSize = 8;
    private static readonly float HeuristicConstant = 1;

    private Slope slope;
    private Rect trueBounds;
    private Point[,] points;
    private Vector2Int[] portals;
    private List<Tuple<List<Vector2Int>, float>> Result;

    public SlopeInternalPathingJob(Slope slope) {
        this.slope = slope;
    }

    public void Initialize() {
        Debug.Log("Pathfind initialize stage 1 begin");

        Polygon expanded = slope.Footprint.Polygon.OffsetPolygon(GridCellSize);

        // First we determine the grid size and offsets
        Rect bounds = expanded.bounds;

        int width  = Mathf.CeilToInt(bounds.width  / GridCellSize);
        int height = Mathf.CeilToInt(bounds.height / GridCellSize);
        // We expand the bounds by 2 cells so that we don't
        // run into the borders
        width  += 2;
        height += 2;

        float scaledWidth  = width  * GridCellSize;
        float scaledHeight = height * GridCellSize;

        trueBounds = new Rect(
            bounds.center - new Vector2(scaledWidth / 2, scaledHeight / 2),
            new Vector2(scaledWidth, scaledHeight)
        );

        // Now we can initialize the array
        points = new Point[width, height];


        // The last initialization step is to fill the array with heights
        for(int x = 0;x < width;x ++) {
            for(int y = 0;y < height;y ++) {
                Vector2 positionOffset = new Vector2(x * GridCellSize, y * GridCellSize);
                Vector2 actualPosition = trueBounds.min + positionOffset;
                points[x, y].within = expanded.ContainsPoint(actualPosition);
                if(points[x, y].within) {
                    points[x, y].height = TerrainManager.Instance.Project(actualPosition).y;
                }
            }
        }

        Debug.Log("Pathfind initialize stage 1 end");
    }
    
    public void Run() {
        Debug.Log("Pathfind initialize stage 2 begin");

        int width  = points.GetLength(0);
        int height = points.GetLength(1);
        // First we need to finish initialization
        // We need to calculate the deltas and distance costs
        for(int x = 0;x < width;x ++) {
            for(int y = 0;y < height;y ++) {
                if(!points[x, y].within) continue;

                float deltaX;
                if(x == 0) {
                    deltaX = points[x, y].height - points[x + 1, y].height;
                } else if (x == width - 1) {
                    deltaX = points[x - 1, y].height - points[x, y].height;
                } else {
                    deltaX = (points[x - 1, y].height - points[x + 1, y].height) / 2;
                }
                points[x, y].dx = deltaX;

                float deltaY;
                if(y == 0) {
                    deltaY = points[x, y].height - points[x, y + 1].height;
                } else if (y == height - 1) {
                    deltaY = points[x, y - 1].height - points[x, y].height;
                } else {
                    deltaY = (points[x, y - 1].height - points[x, y + 1].height) / 2;
                }
                points[x, y].dy = deltaY;

                points[x, y].costDistance = -1;
            }
        }
        Debug.Log("Pathfind initialize stage 2.1");

        for(int cycle = 0;cycle < 16384;cycle ++) {
            bool flag = false;
            for(int x = 0;x < width;x ++) {
                for(int y = 0;y < height;y ++) {
                    if(!points[x, y].within) continue;
                    List<Point> temp = GetValidNeighboringCells(x, y);
                    float minValue = Mathf.Infinity;
                    foreach(var p in temp) {
                        if(!p.within && minValue > 2) {
                            // Edge point
                            minValue = 2;
                        }
                        if(p.costDistance != -1 && minValue > p.costDistance) {
                            minValue = p.costDistance;
                        }
                    }
                    if(minValue != Mathf.Infinity){
                        flag = true;
                        points[x, y].costDistance = minValue / 2;
                    }
                }
            }
            if(!flag) break;
        }
        Debug.Log("Pathfind initialize stage 2.2");

        // Finally, we must prepare the portals
        portals = new Vector2Int[slope.Footprint.Portals.Count];
        for(int i = 0;i < slope.Footprint.Portals.Count;i ++) {
            NavPortal portal = slope.Footprint.Portals[i];
            // We will take the center of the portal
            Vector2 actualPosition = (portal.A1 + portal.A2) / 2;
            Vector2 unRoundPosition = actualPosition - trueBounds.min;
            unRoundPosition /= GridCellSize;
            portals[i] = new Vector2Int(
                Mathf.RoundToInt(unRoundPosition.x), 
                Mathf.RoundToInt(unRoundPosition.y)
            );
        }
        Debug.Log("Pathfind initialize stage 2 end");

        Result = new List<Tuple<List<Vector2Int>, float>>(portals.Length * (portals.Length - 1));
        Debug.Log("Pathfind begin");

        // Initialization of the array is complete
        // We can now begin pathing
        for(int a = 0;a < portals.Length;a ++) {
            for(int b = 0;b < portals.Length;b ++) {
                if(a == b) continue;
                Tuple<List<Vector2Int>, float> result = AStar(portals[a], portals[b]);
                Result.Add(result);
            }
        }

        Debug.Log("Pathfind complete");

        lock(ASyncJobManager.completedJobsLock) {
        	ASyncJobManager.Instance.completedJobs.Enqueue(this);
		}
    }

    // A*
    // Algorithm taken from
    // https://en.wikipedia.org/wiki/A*_search_algorithm

    // There may be an efficiency issue with how priority queue works
    // The same point may be checked multiple times
    private Tuple<List<Vector2Int>, float> AStar(Vector2Int start, Vector2Int end) {
        // The set of discovered nodes that may need to be (re-)expanded.
        // Initially, only the start node is known.
        PriorityQueue<Vector2Int, float> openSet = new PriorityQueue<Vector2Int, float>();
        openSet.Enqueue(start, 0);

        // For node n, cameFrom[n] is the node immediately preceding it on the cheapest path from the start
        // to n currently known.
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        
        // For node n, gScore[n] is the cost of the cheapest path from start to n currently known.
        Dictionary<Vector2Int, float> gScore = new Dictionary<Vector2Int, float>();
        gScore[start] = 0;

        // For node n, fScore[n] := gScore[n] + h(n). fScore[n] represents our current best guess as to
        // how cheap a path could be from start to finish if it goes through n.
        Dictionary<Vector2Int, float> fScore = new Dictionary<Vector2Int, float>();
        fScore[start] = AStar_H(start, end);

        while(openSet.Count != 0) {
            Vector2Int current = openSet.Dequeue();
            if(current == end) {
                return new Tuple<List<Vector2Int>, float>(AStar_ReconstructPath(cameFrom, current), gScore[current]);
            }

            List<Tuple<Vector2Int, float>> neighbors = GetValidNeighboringCellsAndCost(current.x, current.y);
            foreach(var neighbor in neighbors) {
                // tentative_gScore is the distance from start to the neighbor through current
                float tentative_gScore = gScore[current] + neighbor.Item2;
                if(tentative_gScore < gScore.GetValueOrDefault(neighbor.Item1, Mathf.Infinity)) {
                    // This path to neighbor is better than any previous one. Record it!
                    cameFrom[neighbor.Item1] = current;
                    gScore[neighbor.Item1] = tentative_gScore;
                    fScore[neighbor.Item1] = tentative_gScore + AStar_H(neighbor.Item1, end);

                    openSet.Enqueue(neighbor.Item1, fScore[neighbor.Item1]);
                }
            }
        }

        // Open set is empty but goal was never reached
        throw new System.NotImplementedException();
    }

    private List<Vector2Int> AStar_ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current) {
        List<Vector2Int> totalPath = new List<Vector2Int>();
        totalPath.Add(current);
        while(cameFrom.ContainsKey(current)) {
            current = cameFrom[current];
            // This should be prepended, but instead we add it at
            // the end and reverse the list before returning
            totalPath.Add(current);
        }
        totalPath.Reverse();
        return totalPath;
    }

    private float AStar_H(Vector2Int point, Vector2Int end) {
        return (point - end).magnitude * HeuristicConstant;
    }

    public override void Complete() {
        foreach(var x in Result) {
            var y = x.Item1;
            Debug.Log(x.Item2);
            Vector2 prev = new Vector2();
            for(int i = 0;i < y.Count;i ++) {
                Vector2 current = trueBounds.min + new Vector2(y[i].x, y[i].y) * GridCellSize;
                if(i != 0) {
                    Debug.DrawLine(new Vector3(prev.x, 100, prev.y), new Vector3(current.x, 100, current.y), Color.black, 1000);
                }
                prev = current;
            }
        }
        // throw new System.NotImplementedException();
    }

    private List<Tuple<Vector2Int, float>> GetValidNeighboringCellsAndCost(int x, int y) {
        List<Tuple<Vector2Int, float>> toReturn = new List<Tuple<Vector2Int, float>>();
        Point point = points[x, y];

        if(x != points.GetLength(0) - 1) {
            Point newPoint = points[x + 1, y];
            if(newPoint.within) {
                toReturn.Add(new Tuple<Vector2Int, float>(new Vector2Int(x + 1, y), point.GetCost(Direction.PX)));
            }
        }
        if(y != points.GetLength(1) - 1) {
            Point newPoint = points[x, y + 1];
            if(newPoint.within) {
                toReturn.Add(new Tuple<Vector2Int, float>(new Vector2Int(x, y + 1), point.GetCost(Direction.PY)));
            }
        }
        if(x != 0) {
            Point newPoint = points[x - 1, y];
            if(newPoint.within) {
                toReturn.Add(new Tuple<Vector2Int, float>(new Vector2Int(x - 1, y), point.GetCost(Direction.MX)));
            }
        }
        if(y != 0) {
            Point newPoint = points[x, y - 1];
            if(newPoint.within) {
                toReturn.Add(new Tuple<Vector2Int, float>(new Vector2Int(x, y - 1), point.GetCost(Direction.MY)));
            }
        }

        return toReturn;
    }

    private List<Point> GetValidNeighboringCells(int x, int y) {
        List<Point> toReturn = new List<Point>();
        Point point = points[x, y];

        if(x != points.GetLength(0) - 1) {
            Point newPoint = points[x + 1, y];
            if(newPoint.within) {
                toReturn.Add(newPoint);
            }
        }
        if(y != points.GetLength(1) - 1) {
            Point newPoint = points[x, y + 1];
            if(newPoint.within) {
                toReturn.Add(newPoint);
            }
        }
        if(x != 0) {
            Point newPoint = points[x - 1, y];
            if(newPoint.within) {
                toReturn.Add(newPoint);
            }
        }
        if(y != 0) {
            Point newPoint = points[x, y - 1];
            if(newPoint.within) {
                toReturn.Add(newPoint);
            }
        }

        return toReturn;
    }

    private struct Point {
        public bool within;
        public float height;
        public float dx, dy;
        public float costDistance;

        public float GetCost(Direction direction) {
            float delta;
            if(direction == Direction.PX) {
                delta = dx;
            } else if(direction == Direction.PY) {
                delta = dy;
            } else if(direction == Direction.MX) {
                delta = -dx;
            } else {
                delta = -dy;
            }
            delta /= GridCellSize;
            // delta is now the slope in the direction the path is going

            // The primary cost is the slope downward. A steeper slope results
            // in a higher cost. The cost follows tan(θ)
            float costSlope = -delta;

            // Traverse detection. Below roughly -5° and 2°, the cost increases
            // to avoid flat areas
            if(delta > -0.087f && delta < 0.034) {
                costSlope = 0.25f;
            } else if(delta > 0) {
                // We are going upwards not part of a traverse, which is
                // completely different behavior
                // The cost will be dramatic
                costSlope = delta * 100;
            }

            return costSlope + costDistance + 1;
        }

        public float GetDifficulty(Direction direction) {
            throw new System.NotImplementedException();
        }
    }

    private enum Direction {
        PX, PY, MX, MY
    }
}