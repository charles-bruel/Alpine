using UnityEngine;

public class SlopeNavLink : INavLinkImplementation {
    public Slope Owner;
    public NavArea Parent;
    public SlopeNavAreaImplementation ParentImplementation;
    public int LinkID;
    public LineRenderer PathRenderer;

    public SlopeInternalPathingJob.SlopeInternalPath RawData;

    public NavLink Link { 
        get { 
            return Parent.Links[LinkID];
        }
    }
    
    public SlopeNavLink(Slope owner, int linkID, SlopeInternalPathingJob.SlopeInternalPath rawData) {
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

        PathRenderer.widthMultiplier = 5f;
        PathRenderer.material = RenderingData.Instance.VertexColorMaterial;

        // TODO: Make color based on difficulty or something
        PathRenderer.startColor = Color.red;
        PathRenderer.endColor = Color.red;

        // TODO: Make a smoothed version and use that
        var y = RawData.Points;
        Vector3[] linePositions = new Vector3[(y.Count - 1) / 10 + 2];
        int j = 0;
        for(int i = 0;i < y.Count;i ++) {
            if (i % 10 == 0 || i == y.Count - 1) {
                Vector2 current = ParentImplementation.Bounds.min + new Vector2(y[i].x, y[i].y) * SlopeInternalPathingJob.GridCellSize;
                linePositions[j] = TerrainManager.Instance.Project(current) 
                // Shifts the line up so doesn't clip the terrain
                    + Vector3.up * 5;
                j++;
            }
        }
        PathRenderer.positionCount = linePositions.Length;
        PathRenderer.SetPositions(linePositions);
    }
}