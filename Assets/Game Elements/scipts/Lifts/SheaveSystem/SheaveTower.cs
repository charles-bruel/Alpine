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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SheaveTower : APILiftSegment
{
    public override void Build(ICustomScriptable parent, Transform current, Transform next, Transform prev) {
        TowerAssemblyScript TowerAssembly = parent.GetGameObject().transform.GetChild(IntParameters[0]).GetComponent<TowerAssemblyScript>();
        float WheelSize = FloatParameters[0];
        Vector3 TowerOGPos = TowerAssembly.transform.localPosition;
        float DroopAmount = FloatParameters[1];

        Vector3 dif = next.position - current.position;
        float yDif = dif.y;
        dif.y = 0;
        float xDif = dif.magnitude;
        float endAngle = Mathf.Atan(yDif / xDif) * Mathf.Rad2Deg;
        
        dif = current.position - prev.position;
        yDif = dif.y;
        dif.y = 0;
        xDif = dif.magnitude;
        float startAngle = Mathf.Atan(yDif / xDif) * Mathf.Rad2Deg + 180;

        endAngle -= DroopAmount;
        startAngle += DroopAmount;

        TowerAssembly.EndAngle = endAngle;
        TowerAssembly.StartAngle = startAngle;
        TowerAssembly.CurrentDroopAmount = DroopAmount;

        if (startAngle < 0)
        {
            startAngle += 360;
        }
        if (endAngle < 0)
        {
            endAngle += 360;
        }
        if (endAngle > startAngle)
        {
            startAngle += 360;
        }
        float RequiredAngle = startAngle - endAngle;
        if (RequiredAngle > 180)
        {
            RequiredAngle = -360 + RequiredAngle;
        }

        bool Above = RequiredAngle < 0;
        TowerAssembly.Above = Above;

        if (Above)
        {
            TowerAssembly.transform.localPosition = TowerOGPos;
        }
        else
        {
            Vector3 temp = TowerOGPos;
            temp.y += WheelSize;
            TowerAssembly.transform.localPosition = temp;
        }
        TowerAssembly.Reset();
    }

    public override List<LiftCablePoint> GetCablePointsDownhill(ICustomScriptable parent, Transform downhillCablePoints) {
        TowerAssemblyScript TowerAssembly = parent.GetGameObject().transform.GetChild(IntParameters[0]).GetComponent<TowerAssemblyScript>();
        List<Transform> temp = TowerAssembly.SheaveScriptRight.GetAllCablePoints(TowerAssembly.Above);
        List<LiftCablePoint> toReturn = new List<LiftCablePoint>(temp.Count);
        for(int i = 0;i < temp.Count;i ++) {
            toReturn.Add(new LiftCablePoint(temp[i].position, 1));
        }
        return toReturn;
    }

    public override List<LiftCablePoint> GetCablePointsUphill(ICustomScriptable parent, Transform downhillCablePoints) {
        TowerAssemblyScript TowerAssembly = parent.GetGameObject().transform.GetChild(IntParameters[0]).GetComponent<TowerAssemblyScript>();
        List<Transform> temp = TowerAssembly.SheaveScriptLeft.GetAllCablePoints(TowerAssembly.Above);
        List<LiftCablePoint> toReturn = new List<LiftCablePoint>(temp.Count);
        for(int i = 0;i < temp.Count;i ++) {
            toReturn.Add(new LiftCablePoint(temp[i].position, 1));
        }
        return toReturn;
    }
}
