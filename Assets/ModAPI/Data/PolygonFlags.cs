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

[System.Flags]
public enum PolygonFlags : uint {
    AERIAL_CLEARANCE = 1,
    GROUND_CLEARANCE = 2,
    CLEARANCE = AERIAL_CLEARANCE | GROUND_CLEARANCE,
    FLAT_NAVIGABLE = 4,
    FLATTEN_DOWN = 8,
    FLATTEN_UP = 16,
    FLATTEN = FLATTEN_DOWN | FLATTEN_UP,
    SLOPE_NAVIGABLE = 32,
    NAVIGABLE_MASK = FLAT_NAVIGABLE | SLOPE_NAVIGABLE,
    // All slopes are of the form 0001 xy00 0000
    // Where xy determines the difficulty
    SLOPE_GREEN = 256,
    SLOPE_BLUE = 320,
    SLOPE_BLACK = 384,
    SLOPE_DOUBLE_BLACK = 448,
    SLOPE_MASK = 448,
}