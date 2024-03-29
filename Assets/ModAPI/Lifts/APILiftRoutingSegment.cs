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

using System.Linq;

public class APILiftRoutingSegment : APILiftSegment {

    // This function will return the length from the origin to the end of the station
    // This function will be called after Build()
    // Default behavior is to return FloatParameters[0], if it exists, otherwise 0
    public virtual float GetLength(ICustomScriptable parent) {
        if(FloatParameters.Length > 0) {
            return FloatParameters[0];
        }
        return 0;
    }
    
    public virtual LiftPathAccessDefinition[] GetPathAccess(LiftRoutingSegmentType type, LiftPathAccessDefinition[] defaultUphillAccessPoints, LiftPathAccessDefinition[] defaultDownhillAccessPoints) {
        switch(type) {
            case LiftRoutingSegmentType.FIRST:
            return defaultUphillAccessPoints;
            case LiftRoutingSegmentType.LAST:
            return defaultDownhillAccessPoints;
            case LiftRoutingSegmentType.MIDDLE:
            return defaultDownhillAccessPoints.Concat(defaultUphillAccessPoints).ToArray();
        }
        throw new System.Exception("Invalid LiftRoutingSegmentType");
    }
}