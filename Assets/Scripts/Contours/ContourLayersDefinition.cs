using System.Collections.Generic;

[System.Serializable]
public struct ContourLayersDefinition {
    public float MajorSpacing;
    public float MinorSpacing;
    public float[] Major;
    public float[] Minor;

    public ContourLayersDefinition Convert(float maxHeight) {
        List<float> majorTemp = new List<float>();
        List<float> minorTemp = new List<float>();

        for(float i = 0;i <= maxHeight; i += MajorSpacing) {
            majorTemp.Add(i);
        }

        Major = majorTemp.ToArray();

        if(MinorSpacing != 0) {
            for(float i = 0;i <= maxHeight; i += MinorSpacing) {
                if(!majorTemp.Contains(i)) minorTemp.Add(i);
            }
        }

        Minor = minorTemp.ToArray();

        return this;
    }
}