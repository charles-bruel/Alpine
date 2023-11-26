using UnityEngine;

public class BasicNavLinkImplementation : INavLinkImplementation
{
    public void OnDeselected()
    {
        
    }

    public void OnRemove()
    {
        
    }

    public void OnSelected()
    {
        
    }

    public void ProgressPosition(Visitor self, NavLink link, float delta, ref float progress, ref Vector3 pos, ref Vector3 angles)
    {
        // Straight line
        Vector3 pos1 = link.A.GetPosition3d();
        Vector3 pos2 = link.B.GetPosition3d();
        float dist = (pos1 - pos2).magnitude;
        pos = Vector3.Lerp(pos1, pos2, progress);
        progress += self.TraverseSpeed * delta / dist;
    }
}