public interface ITool {
    public abstract void Update();
    public abstract void Start();
    public abstract void Cancel();
    public abstract bool IsDone();
    public abstract bool Require2D();
}