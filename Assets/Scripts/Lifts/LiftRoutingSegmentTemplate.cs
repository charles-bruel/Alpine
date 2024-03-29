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

using UnityEngine;

public class LiftRoutingSegmentTemplate : LiftSegmentTemplate
{
    public enum RoutingSegmentType {
        STATION,
        MIDSTATION,
        TURN,
    }

    public APILiftRoutingSegment APILiftRoutingSegment;
    public LiftPathAccessDefinition[] DefaultUphillAccessPoints;
    public LiftPathAccessDefinition[] DefaultDownhillAccessPoints;

    private void Initialize() {
        if(APILiftRoutingSegment != null) return;
        
        APILiftRoutingSegment = LiftSegmentAPIDef.Fetch<APILiftRoutingSegment>();
        APILiftSegment = APILiftRoutingSegment;
    }

    public override IPoolable Clone() {
        Initialize();
        var temp = GameObject.Instantiate(this);
        temp.APILiftRoutingSegment = APILiftRoutingSegment;
        temp.APILiftSegment = APILiftSegment;
        return temp;
    }
}