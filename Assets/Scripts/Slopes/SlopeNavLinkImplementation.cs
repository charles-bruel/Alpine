using System.IO;
using EPPZ.Cloud.Scenes.Helpers;
using UnityEditor;
using UnityEngine;

public class SlopeNavLinkImplentation : INavLinkImplementation {
    public Slope Owner;
    public NavArea Parent;
    public SlopeNavAreaImplementation ParentImplementation;
    public int LinkID;
    public LineRenderer PathRenderer;

    public SlopeInternalPathingJob.SlopeInternalPath RawData;

    public static int LineRendererPointDensity = 2;

    public NavLink Link { 
        get { 
            return Parent.Links[LinkID];
        }
    }
    
    public SlopeNavLinkImplentation(Slope owner, int linkID, SlopeInternalPathingJob.SlopeInternalPath rawData) {
        Owner = owner;
        Parent = owner.Footprint;
        LinkID = linkID;
        ParentImplementation = (owner.Footprint.Implementation as SlopeNavAreaImplementation);
        RawData = rawData;
    }

    public void OnDeselected() {
        Initialize();
        PathRenderer.gameObject.SetActive(false);
    }

    public void OnSelected() {
        Initialize();
        PathRenderer.gameObject.SetActive(true);
    }

    private bool Initialized = false;
    public void Initialize() {
        if(Initialized) return;
        Initialized = true;

        GameObject gameObject = new GameObject("Link Render Line");
        gameObject.transform.SetParent(Owner.transform, true);
        gameObject.layer = LayerMask.NameToLayer("2D");
        PathRenderer = gameObject.AddComponent<LineRenderer>();

        PathRenderer.widthMultiplier = 2.5f;
        PathRenderer.material = RenderingData.Instance.VertexColorMaterial;

        // TODO: Make color based on difficulty or something
        PathRenderer.startColor = Color.blue;
        PathRenderer.endColor = Color.blue;

        // TODO: Make a smoothed version and use that
        var y = RawData.Points;
        Vector3[] linePositions = new Vector3[(y.Count - 1) / LineRendererPointDensity + 2];
        int j = 0;
        for(int i = 0;i < y.Count;i ++) {
            if (i % LineRendererPointDensity == 0 || i == y.Count - 1) {
                Vector2 current = ParentImplementation.Bounds.min + new Vector2(y[i].x, y[i].y) * SlopeInternalPathingJob.GridCellSize;
                linePositions[j] = TerrainManager.Instance.Project(current) 
                // Shifts the line up so doesn't clip the terrain
                    + Vector3.up * 5;
                j++;
            }
        }
        PathRenderer.positionCount = j;
        PathRenderer.SetPositions(linePositions);
    }

    public void OnRemove() {
        if(PathRenderer != null)
            GameObject.Destroy(PathRenderer.gameObject);
    }

    private static readonly float[] dist = new float[] {1, 2, 3, 4, 3, 2, 1};
    private static readonly int center = dist.Length / 2;

    private Vector2 Lookup(int i) {
        if(i < 0) return RawData.Points[0];
        if(i >= RawData.Points.Count) return RawData.Points[RawData.Points.Count - 1];
        return RawData.Points[i];
    }

    private float LookupWidth(int i) {
        if(i < 0) return RawData.Widths[0];
        if(i >= RawData.Widths.Count) return RawData.Widths[RawData.Widths.Count - 1];
        return RawData.Widths[i];
    }

    private Vector2 EvalLattice(int i) {
        Vector2 sum = Vector2.zero;
        float weightSum = 0;
        for(int j = 0;j < dist.Length;j ++) {
            sum += Lookup(i + j - center) * dist[j];
            weightSum += dist[j];
        }
        return sum / weightSum;
    }

    // TODO: Shift to visitor template, randomize?
    private readonly float[] sines_frequencies = new float[] {0.25f, 0.5f, 2, 10};
    private readonly float[] sines_amplitudes = new float[] {0.6f, 0.19f, 0.19f, 0.02f};

    private float EvaluateSines(float animationTimer) {
        float sum = 0;
        for(int i = 0;i < sines_frequencies.Length;i ++) {
            sum += Mathf.Sin(animationTimer * sines_frequencies[i]) * sines_amplitudes[i];
        }
        return sum;
    }

    public void ProgressPosition(Visitor self, NavLink link, float delta, ref float progress, ref Vector3 pos, ref Vector3 angles, float animationTimer) {

        // Position
        float totalLength = RawData.Points.Count + center * 2;
        float overallProgress = progress * totalLength;
        int i = (int) overallProgress;
        float mod = overallProgress - i;
        i -= center;
        Vector2 eval0 = EvalLattice(i);
        Vector2 eval1 = EvalLattice(i + 1);
        Vector2 pos2d = eval0 * (1 - mod) + eval1 * mod;

        // Normal
        Vector2 normal = (eval1 - eval0).normalized;
        normal = new Vector2(-normal.y, normal.x);

        float width0 = LookupWidth(i);
        float width1 = LookupWidth(i + 1);
        float width = width0 * (1 - mod) + width1 * mod;

        Vector2 normalOffset = normal * EvaluateSines(animationTimer) * width * 0.75f;

        // Combine
        pos2d = ParentImplementation.Bounds.min + pos2d * SlopeInternalPathingJob.GridCellSize;
        pos2d += normalOffset;

        pos = TerrainManager.Instance.Project(pos2d) + Vector3.up;
        progress += self.SkiSpeed * delta / totalLength;
    }
}