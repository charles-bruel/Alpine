using System.Collections.Generic;
using UnityEngine;
using System;
using EPPZ.Geometry.Model;

public class SlopeInternalPathingJob : Job {
    public static readonly float GridCellSize = 4;
    public static readonly float HeuristicConstant = 0;
    public static readonly float CenteringWeight = 16f;
    public static readonly float CenteringFalloff = 0.95f;
    public static readonly float CrossSlopeCost = 0.8f;

    private Slope slope;
    private Rect trueBounds;
    private Point[,] points;
    private Vector2Int[] portals;
    private List<SlopeInternalPath> Result;

    public SlopeInternalPathingJob(Slope slope) {
        this.slope = slope;
    }

    public void Initialize() {
        Polygon expanded = slope.Footprint.Polygon.OffsetPolygon(GridCellSize);

        // First we determine the grid size and offsets
        Rect bounds = expanded.bounds;

        int width  = Mathf.CeilToInt(bounds.width  / GridCellSize);
        int height = Mathf.CeilToInt(bounds.height / GridCellSize);
        // We expand the bounds by 4 cells so that we don't
        // run into the borders
        width  += 4;
        height += 4;

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

    }
    
    public void Run() {

        int width = points.GetLength(0);
        int height = points.GetLength(1);
        // First we need to finish initialization
        // We need to calculate the deltas and distance costs
        CalculateDeltas(width, height);
        CalculateDistanceCosts(width, height);

        // Finally, we must prepare the portals
        portals = new Vector2Int[slope.Footprint.Nodes.Count];
        for (int i = 0; i < slope.Footprint.Nodes.Count; i++)
        {
            INavNode node = slope.Footprint.Nodes[i];
            // We will take the center of the portal
            Vector2 actualPosition = node.GetPosition();
            Vector2 unRoundPosition = actualPosition - trueBounds.min;
            unRoundPosition /= GridCellSize;
            portals[i] = new Vector2Int(
                Mathf.RoundToInt(unRoundPosition.x),
                Mathf.RoundToInt(unRoundPosition.y)
            );
        }

        Result = new List<SlopeInternalPath>(portals.Length * (portals.Length - 1));

        // Initialization of the array is complete
        // We can now begin pathing
        for (int a = 0; a < portals.Length; a++)
        {
            for (int b = 0; b < portals.Length; b++)
            {
                if (a == b) continue;
                // Basic height filtering:
                // If the end is higher than the start, we invalidate it. This does disallow
                // bidirectional pathes to be made with the slope tool, which is intended as
                // those should be made with the nav area tool
                if(slope.Footprint.Nodes[b].GetHeight() > slope.Footprint.Nodes[a].GetHeight()) continue;

                SlopeInternalPath result = GetPath(portals[a], portals[b]);
                result.A = slope.Footprint.Nodes[a];
                result.B = slope.Footprint.Nodes[b];

                // TODO: Find a better filtering method
                // if(result.MeanCost < Mathf.Pow(10, 13))
                // I've disabled filtering for now aside from the height difference filter
                Result.Add(result);
            }
        }

        lock (ASyncJobManager.completedJobsLock)
        {
            ASyncJobManager.Instance.completedJobs.Enqueue(this);
        }
    }

    private void CalculateDeltas(int width, int height) {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (!points[x, y].within) continue;

                float deltaX;
                if (x == 0)
                {
                    deltaX = points[x, y].height - points[x + 1, y].height;
                }
                else if (x == width - 1)
                {
                    deltaX = points[x - 1, y].height - points[x, y].height;
                }
                else
                {
                    deltaX = (points[x - 1, y].height - points[x + 1, y].height) / 2;
                }
                points[x, y].dx = deltaX;

                float deltaY;
                if (y == 0)
                {
                    deltaY = points[x, y].height - points[x, y + 1].height;
                }
                else if (y == height - 1)
                {
                    deltaY = points[x, y - 1].height - points[x, y].height;
                }
                else
                {
                    deltaY = (points[x, y - 1].height - points[x, y + 1].height) / 2;
                }
                points[x, y].dy = deltaY;

                points[x, y].costDistance = -1;
            }
        }
    }

    private void CalculateDistanceCosts(int width, int height) {
        // First we calculate the raw distances to the edges
        int[,] rawDistances = new int[width, height];

        // Populate the array
        for(int x = 0;x < width;x ++) {
            for(int y = 0;y < height;y ++) {
                rawDistances[x,y] = Int32.MaxValue;
            }
        }

        int maxDist = 0;
        int cycle = 0;
        for (; cycle < 16384; cycle++) {
            bool flag = false;
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    if (points[x, y].within) {
                        List<Vector2Int> temp = GetNeighboringCells(x, y);
                        int minValue = rawDistances[x, y];
                        bool flag2 = false;
                        foreach (var c in temp) {
                            Point p = points[c.x, c.y];
                            if (!p.within && minValue > 0) {
                                // Edge point
                                minValue = 0;
                                flag2 = true;
                            }
                            int v = rawDistances[c.x, c.y] + 1;
                            if (p.within && rawDistances[c.x, c.y] != Int32.MaxValue && minValue > v) {
                                minValue = v;
                                flag2 = true;
                            }
                        }
                        if (flag2) {
                            flag = true;
                            rawDistances[x, y] = minValue;
                            if(maxDist < minValue) {
                                maxDist = minValue;
                            }
                        }
                    }
                }
            }
            if (!flag) break;
        }

        // Now we make it so that the max value occurs across the enter center
        cycle = 0;
        for (; cycle < 16384; cycle++) {
            bool flag = false;
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    if (points[x, y].within) {
                        // We check if the cell has any neighbors which are exactly 1 greater
                        // If not, increase it by one (up to the maxDist)

                        int currentDist = rawDistances[x, y];
                        // It should never be greater than the maxDist but just in case
                        if(currentDist >= maxDist) continue;

                        List<Vector2Int> temp = GetNeighboringCells(x, y);
                        bool flag2 = true;
                        foreach (var c in temp) {
                            if(rawDistances[c.x, c.y] == currentDist + 1) {
                                flag2 = false;
                            }
                        }
                        if(flag2) {
                            rawDistances[x, y] ++;
                            flag = true;
                        }
                    }
                }
            }
            if (!flag) break;
        }

        // Finally, we take the distances and turn them into costs
        float b = 1/CenteringFalloff;
        float o = Mathf.Log(CenteringWeight, b);
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                if (points[x, y].within) {
                    int inverseDistance = maxDist - rawDistances[x, y];
                    // Now on the interval [1,maxDist+1]
                    inverseDistance += 1;
                    float value = CenteringWeight - Mathf.Pow(b, -(inverseDistance - o));

                    points[x, y].costDistance = value;
                }
            }
        }
    }

    private SlopeInternalPath GetPath(Vector2Int start, Vector2Int end) {
        Tuple<List<Vector2Int>, float, float> tempResult = AStar(start, end);
        SlopeInternalPath result = new SlopeInternalPath();

        result.Points = tempResult.Item1;
        result.TotalCost = tempResult.Item2 * GridCellSize;
        result.TotalDifficulty = tempResult.Item3  * GridCellSize;
        result.Length = result.Points.Count * GridCellSize;

        result.MeanCost = result.TotalCost / result.Length;
        result.MeanDifficulty = result.TotalDifficulty / result.Length;

        return result;
    }

    // A*
    // Algorithm taken from
    // https://en.wikipedia.org/wiki/A*_search_algorithm

    // There may be an efficiency issue with how priority queue works
    // The same point may be checked multiple times
    private Tuple<List<Vector2Int>, float, float> AStar(Vector2Int start, Vector2Int end) {
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

        // For node n, dScore[n] is the difficulty of the cheapest path from start to n currently known.
        Dictionary<Vector2Int, float> dScore = new Dictionary<Vector2Int, float>();
        dScore[start] = 0;

        // For node n, fScore[n] := gScore[n] + h(n). fScore[n] represents our current best guess as to
        // how cheap a path could be from start to finish if it goes through n.
        Dictionary<Vector2Int, float> fScore = new Dictionary<Vector2Int, float>();
        fScore[start] = AStar_H(start, end);

        while(openSet.Count != 0) {
            Vector2Int current = openSet.Dequeue();
            if(current == end) {
                return new Tuple<List<Vector2Int>, float, float>(
                    AStar_ReconstructPath(cameFrom, current), 
                    gScore[current],
                    dScore[current]
                );
            }

            List<Tuple<Vector2Int, float, float>> neighbors = GetValidNeighboringCellsCostAndDifficulty(current.x, current.y);
            foreach(var neighbor in neighbors) {
                // tentative_gScore is the distance from start to the neighbor through current
                // raised to a high power to make small changes have a large impact. this drives
                // the path towards cheap areas even at the cost of length
                // float tentative_gScore = gScore[current] + Mathf.Pow(neighbor.Item2, 4);
                float tentative_gScore = gScore[current] + neighbor.Item2;
                if(tentative_gScore < gScore.GetValueOrDefault(neighbor.Item1, Mathf.Infinity)) {
                    // This path to neighbor is better than any previous one. Record it!
                    cameFrom[neighbor.Item1] = current;
                    gScore[neighbor.Item1] = tentative_gScore;
                    dScore[neighbor.Item1] = dScore[current] + neighbor.Item3;
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
        slope.SetNewInternalPaths(Result, trueBounds);
        GlobalNavController.MarkGraphDirty();
    }

    private List<Tuple<Vector2Int, float, float>> GetValidNeighboringCellsCostAndDifficulty(int x, int y) {
        List<Tuple<Vector2Int, float, float>> toReturn = new List<Tuple<Vector2Int, float, float>>();
        Point point = points[x, y];

        if(x != points.GetLength(0) - 1) {
            Point newPoint = points[x + 1, y];
            if(newPoint.within) {
                toReturn.Add(new Tuple<Vector2Int, float, float>(
                    new Vector2Int(x + 1, y), 
                    point.GetCost(Direction.PX),
                    point.GetDifficulty(Direction.PX)
                ));
            }
        }
        if(y != points.GetLength(1) - 1) {
            Point newPoint = points[x, y + 1];
            if(newPoint.within) {
                toReturn.Add(new Tuple<Vector2Int, float, float>(
                    new Vector2Int(x, y + 1), 
                    point.GetCost(Direction.PY),
                    point.GetDifficulty(Direction.PY)
                ));
            }
        }
        if(x != 0) {
            Point newPoint = points[x - 1, y];
            if(newPoint.within) {
                toReturn.Add(new Tuple<Vector2Int, float, float>(
                    new Vector2Int(x - 1, y), 
                    point.GetCost(Direction.MX),
                    point.GetDifficulty(Direction.MX)
                ));
            }
        }
        if(y != 0) {
            Point newPoint = points[x, y - 1];
            if(newPoint.within) {
                toReturn.Add(new Tuple<Vector2Int, float, float>(
                    new Vector2Int(x, y - 1), 
                    point.GetCost(Direction.MY),
                    point.GetDifficulty(Direction.MY)
                ));
            }
        }

        return toReturn;
    }

    private List<Vector2Int> GetNeighboringCells(int x, int y) {
        List<Vector2Int> toReturn = new List<Vector2Int>();
        Point point = points[x, y];

        if(x != points.GetLength(0) - 1) {
            Vector2Int newPoint = new Vector2Int(x + 1, y);
            toReturn.Add(newPoint);
        }
        if(y != points.GetLength(1) - 1) {
            Vector2Int newPoint = new Vector2Int(x, y + 1);
            toReturn.Add(newPoint);
        }
        if(x != 0) {
            Vector2Int newPoint = new Vector2Int(x - 1, y);
            toReturn.Add(newPoint);
        }
        if(y != 0) {
            Vector2Int newPoint = new Vector2Int(x, y - 1);
            toReturn.Add(newPoint);
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

            return costSlope + costDistance;
        }

        public float GetDifficulty(Direction direction) {
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

            return costSlope * costSlope;
        }
    }

    private enum Direction {
        PX, PY, MX, MY
    }

    public struct SlopeInternalPath {
        public INavNode A;
        public INavNode B;
        public List<Vector2Int> Points;
        public float TotalCost;
        public float TotalDifficulty;
        public float MeanCost;
        public float MeanDifficulty;
        public float Length;
    }
}