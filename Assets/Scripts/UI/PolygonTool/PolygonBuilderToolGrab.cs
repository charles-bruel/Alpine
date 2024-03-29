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

public class PolygonBuilderToolGrab : GenericGrab {
    public int SlopePointGroupIndex;
    public int SlopePointIndex;
    public PolygonConstructionData Data;
    public AlpinePolygon Footprint;

    public override void OnDragBehavior(Vector2 newPos) {

        PolygonsController.PolygonSnappingResult? snapping = PolygonsController.Instance.CheckForSnapping(newPos, 10, 7, PolygonFlags.NAVIGABLE_MASK, Footprint);
        if(snapping != null) {
            newPos = snapping.Value.Pos;
            RectTransform.anchoredPosition = newPos;

            Data.SlopePoints[SlopePointIndex] = new PolygonConstructionData.SlopePoint(newPos, snapping.Value);
        } else {
            Data.SlopePoints[SlopePointIndex] = new PolygonConstructionData.SlopePoint(newPos);
        }
        PolygonsController.Instance.MarkPolygonsDirty();
    }
}