using UnityEngine;

public interface INavLinkImplementation {
    public abstract void OnSelected();
    public abstract void OnDeselected();
    public abstract void OnRemove();
    public abstract void ProgressPosition(Visitor self, NavLink link, float delta, ref float progress, ref Vector3 pos, ref Vector3 angles, float animationTimer);
    public virtual void OnSaveLoad(Visitor visitor, ref float progress) {}
}