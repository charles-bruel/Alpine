using UnityEngine;

public class BasicNavLinkImplementation : INavLinkImplementation {
    public void OnDeselected()
    {
        
    }

    public void OnRemove()
    {
        
    }

    public void OnSelected()
    {
        
    }

    public void ProgressPosition(Visitor self, NavLink link, float delta, ref float progress, ref Vector3 pos, ref Vector3 angles, float animationTimer) {
        // Straight line
        Vector2 pos1 = link.A.GetPosition();
        Vector2 pos2 = link.B.GetPosition();
        float dist = (pos1 - pos2).magnitude;
        Vector2 pos2d = Vector2.Lerp(pos1, pos2, progress);
        pos = TerrainManager.Instance.Project(pos2d) + Vector3.up;
        progress += self.TraverseSpeed * delta / dist;
    }
}