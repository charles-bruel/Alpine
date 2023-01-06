using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

class SimpleSheaveControlScript : MonoBehaviour
{
    public SheaveScript Target;
    public Transform RotationPoint;
    public Transform Start;
    public Transform End;
    public float WheelRadius;
    public Transform Connector;
    public float TowerHeight;
    public float Depression = 1;
    public bool Debug;

    void Update()
    {
        if(Debug) PushUpdate();
    }

    public void PushUpdate()
    {
        Vector3 temp = transform.position - Start.transform.position;
        float yDif = temp.y;
        temp.y = 0;
        float xDif = temp.magnitude;
        float angle = Mathf.Rad2Deg * Mathf.Atan2(yDif, xDif);
        Target.StartAngle = angle + 180 - Depression;

        temp = End.transform.position - transform.position;
        yDif = temp.y;
        temp.y = 0;
        xDif = temp.magnitude;
        angle = Mathf.Rad2Deg * Mathf.Atan2(yDif, xDif);
        Target.EndAngle = angle - Depression;

        Target.Reset();

        float theta = (Target.StartAngle - 180) - Target.EndAngle;
        float alpha = theta * 0.5f;
        float beta = 90 + Target.EndAngle + alpha;

        alpha *= Mathf.Deg2Rad;
        beta *= Mathf.Deg2Rad;

        float length = Target.Radius / Mathf.Cos(alpha);
        if (Target.Uphill)
        {
            length += WheelRadius;
        }
        else
        {
            length -= WheelRadius;
        }
        Vector2 pos = new Vector2(length * Mathf.Cos(beta), length * Mathf.Sin(beta));

        Target.transform.localPosition = new Vector3(-pos.x, pos.y, 0);

        if(Connector != null)
        {
            Transform Point = Target.CollectOpenSheaves()[0].TowerAttachPoint;
            float delta = Point.position.y - Connector.position.y - TowerHeight;
            Connector.position = new Vector3(Connector.position.x, Connector.position.y + delta, Connector.position.z);
        }
    }
}

