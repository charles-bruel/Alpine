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

public class BasicNavLinkImplementation : INavLinkImplementation {
    public void OnDeselected()
    {
        
    }

    public void OnRemove()
    {
        
    }

    public void OnSelected()
    {
        
    }

    public void ProgressPosition(Visitor self, NavLink link, float delta, ref float progress, ref Vector3 pos, ref Vector3 angles, float animationTimer) {
        // Straight line
        Vector2 pos1 = link.A.GetPosition();
        Vector2 pos2 = link.B.GetPosition();
        float dist = (pos1 - pos2).magnitude;
        Vector2 pos2d = Vector2.Lerp(pos1, pos2, progress);
        pos = TerrainManager.Instance.Project(pos2d) + Vector3.up;
        progress += self.TraverseSpeed * delta / dist;
    }
}