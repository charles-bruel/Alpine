public abstract class Job {
    
    // Some functions require the main thread to run properly
    // This function will be called from the main thread
    public abstract void Complete();

    public virtual float GetCompleteCost() {
        return 1f;
    }

}