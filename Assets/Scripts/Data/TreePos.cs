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

public struct TreePos : IGridable {
    public Vector3 pos;
    public float rot;
    public float scale;
    public uint type;

    // We use a uint here because bool is not blittable. It cannot be a byte
    // because GLSL doesn't support sized types smaller than 32 bits
    public uint enabled;

    public uint padding;

    public byte GetGridX()
    {
        int x = Mathf.FloorToInt(pos.x / TerrainManager.Instance.TileSize);
        x += TerrainManager.Instance.NumTilesX / 2;
        return (byte) x;
    }

    public byte GetGridY()
    {
        int y = Mathf.FloorToInt(pos.z / TerrainManager.Instance.TileSize);
        y += TerrainManager.Instance.NumTilesY / 2;
        return (byte) y;
    }
}