public class ContourLayersDefinition {
    public float[] Major;
    public float[] Minor;

    public ContourLayersDefinition(float[] major, float[] minor)
    {
        Major = major;
        Minor = minor;
    }

    public static ContourLayersDefinition tester = new ContourLayersDefinition(new float[] { 25, 50, 75, 100, 125, 150, 175, 200, 225 }, new float[] {});
}