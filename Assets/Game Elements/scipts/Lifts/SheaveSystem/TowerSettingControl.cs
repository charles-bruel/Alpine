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

class TowerSettingControl : SettingControl
{
    public float AngleThreshold1 = 2.5f;
    public float AngleThreshold2 = 15f;
    public float AngleThreshold3 = 25f;
    public float TiltThreshold = 5f;
    public float HalfTiltThreshold = 20f;
    public int AllTiltThreshold = 6;
    public TowerAssemblyScript tower;
    private int BaseIndex;
    private bool Initialized = false;
    private void Initialize()
    {
        BaseIndex = tower.SheaveLayout;
        Initialized = true;
    }
    public override void Run()
    {
        if (!Initialized) Initialize();
        float avg = Mathf.Abs(tower.StartAngle + tower.EndAngle - 180) / 2;
        float overall = Mathf.Abs(tower.StartAngle - tower.EndAngle - 180);
        int index = BaseIndex;
        if (overall > AngleThreshold1) index++;
        if (overall > AngleThreshold2) index++;
        if (overall > AngleThreshold3) index++;
        tower.SheaveLayout = index;
        if (index >= AllTiltThreshold)
        {
            tower.TiltTowers = true;
            tower.HalfTilt = false;
        } 
        else
        {
            if (avg > TiltThreshold)
            {
                tower.TiltTowers = true;
                if (avg > HalfTiltThreshold)
                {
                    tower.HalfTilt = true;
                }
                else 
                {
                    tower.HalfTilt = false;
                }
            } 
            else
            {
                tower.TiltTowers = false;
                tower.HalfTilt = false;
            }
        }
    }
}