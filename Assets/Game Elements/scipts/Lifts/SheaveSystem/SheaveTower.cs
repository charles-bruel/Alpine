using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SheaveTower : APILiftSegment
{
    private TowerAssemblyScript TowerAssembly;
    private float WheelSize;

    private bool Above;
    private Vector3 TowerOGPos;
    private float DroopAmount;

    public override void Build(GameObject self, Transform current, Transform next, Transform prev) {
        TowerAssembly = self.transform.GetChild(IntParameters[0]).GetComponent<TowerAssemblyScript>();
        WheelSize = FloatParameters[0];
        TowerOGPos = TowerAssembly.transform.localPosition;
        DroopAmount = FloatParameters[1];

        Vector3 dif = prev.position - current.position;
        float yDif = dif.y;
        dif.y = 0;
        float xDif = dif.magnitude;
        float endAngle = Mathf.Atan(yDif / xDif) * Mathf.Rad2Deg;
        
        dif = current.position - next.position;
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

        Above = RequiredAngle < 0;

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
}
