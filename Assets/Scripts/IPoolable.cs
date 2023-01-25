//This allows objects to have custom clone behavior inside a pool
public interface IPoolable {
    public abstract void Enable();
    public abstract void Disable();
    public abstract void Destroy();
    public abstract IPoolable Clone();
}