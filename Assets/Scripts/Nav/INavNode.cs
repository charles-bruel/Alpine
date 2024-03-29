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

using System;
using System.Collections.Generic;
using UnityEngine;

public interface INavNode {
    public List<NavLink> GetLinks();
    public List<NavLink> GetExplicitLinksForSerialization();
    public void AddExplictNavLink(NavLink link);
    public void RemoveExplicitNavLink(NavLink link);
    public Vector2 GetPosition();
    public Vector3 GetPosition3d() {
        return GetPosition().Inflate3rdDim(GetHeight());
    }
    public float GetHeight();
    public void Destroy();
    public bool IsDead();
}