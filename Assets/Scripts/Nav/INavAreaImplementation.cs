public interface INavAreaImplementation {
    public abstract void OnAdvance(float delta);
    public abstract void OnAdvanceSelected(float delta);
    public abstract void OnSelected();
    public abstract void OnDeselected();
    public abstract void OnRemove();
}