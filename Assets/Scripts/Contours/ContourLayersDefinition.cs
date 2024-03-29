//>============================================================================<
//
//    Alpine, Ski Resort Tycoon Game
//    Copyright (C) 2024  Charles Bruel
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see <https://www.gnu.org/licenses/>.
//>============================================================================<

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