using UnityEngine;

[System.Serializable]
public class APIDef {
    public bool Enabled = true;
    public string AssemblyName;
    public string Namespace;
    public string ClassName;
    public BasicAPIParams Params;
    
    public bool HasContent() {
        return Enabled && AssemblyName != null && ClassName != null;
    }
}