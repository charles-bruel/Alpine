using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SheaveTower : APILiftSegment
{
    public override void Build(GameObject self, Transform current, Transform next, Transform prev) {
        TowerAssemblyScript TowerAssembly = self.transform.GetChild(IntParameters[0]).GetComponent<TowerAssemblyScript>();
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

    public override List<Vector3> GetCablePointsDownhill(GameObject self, Transform downhillCablePoints) {
        TowerAssemblyScript TowerAssembly = self.transform.GetChild(IntParameters[0]).GetComponent<TowerAssemblyScript>();
        List<Transform> temp = TowerAssembly.SheaveScriptRight.GetAllCablePoints(TowerAssembly.Above);
        List<Vector3> toReturn = new List<Vector3>(temp.Count);
        for(int i = 0;i < temp.Count;i ++) {
            toReturn.Add(temp[i].position);
        }
        return toReturn;
    }

    public override List<Vector3> GetCablePointsUphill(GameObject self, Transform downhillCablePoints) {
        TowerAssemblyScript TowerAssembly = self.transform.GetChild(IntParameters[0]).GetComponent<TowerAssemblyScript>();
        List<Transform> temp = TowerAssembly.SheaveScriptLeft.GetAllCablePoints(TowerAssembly.Above);
        List<Vector3> toReturn = new List<Vector3>(temp.Count);
        for(int i = 0;i < temp.Count;i ++) {
            toReturn.Add(temp[i].position);
        }
        return toReturn;
    }
}
