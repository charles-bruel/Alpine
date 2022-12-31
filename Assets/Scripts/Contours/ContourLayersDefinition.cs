[System.Serializable]
public struct ContourLayersDefinition {
    public float[] Major;
    public float[] Minor;

    public ContourLayersDefinition(float[] major, float[] minor)
    {
        Major = major;
        Minor = minor;
    }
}