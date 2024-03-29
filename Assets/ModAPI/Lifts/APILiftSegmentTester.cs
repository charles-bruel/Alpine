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
using System.Collections.Generic;

public class APILiftSegmentTester : MonoBehaviour {
    public Transform prev;
    public Transform next;
    public Transform self;
    public Transform cablePointUphill;
    public Transform cablePointDownhill;
    public APIDef APIDef;
    public APILiftSegment Segment;
    public ICustomScriptable parent;

    void Start() {
        Segment = APIDef.Fetch<APILiftSegment>();
    }

    void Update() {
        Segment.Build(parent, self, next, prev);
        List<LiftCablePoint> temp = Segment.GetCablePointsDownhill(parent, cablePointDownhill);
        for(int i = 0;i < temp.Count - 1;i ++) {
            Debug.DrawLine(temp[i].pos, temp[i + 1].pos, Color.red, 1);
        }
        temp = Segment.GetCablePointsUphill(parent, cablePointUphill);
        for(int i = 0;i < temp.Count - 1;i ++) {
            Debug.DrawLine(temp[i].pos, temp[i + 1].pos, Color.red, 1);
        }
    }
}