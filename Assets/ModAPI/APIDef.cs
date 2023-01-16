[System.Serializable]
public struct APIDef {
    public bool Enabled;
    public string AssemblyName;
    public string Namespace;
    public string ClassName;
    
    public bool HasContent() {
        return Enabled && AssemblyName != null && ClassName != null;
    }

    public T Fetch<T>() where T : APIBase {
        return (T) ReflectionHelper.GetInstance(typeof(T), this);
    }
}