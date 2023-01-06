using System;
using System.Collections.Generic;
using UnityEngine;

public class TowerAssemblyScript : MonoBehaviour
{

    [Header("Sheave Config Data")]
    public SheaveScript SheaveScriptLeft;
    public SheaveScript SheaveScriptRight;
    public Transform SheavesParent;
    public int SheaveLayout;
    public float SheaveScale = 1;
    public bool ContinuousUpdate = false;
    public SheaveLayoutSet layouts;
    public bool Recenter = false;
    public bool EdgePoint = false;

    [Header("Tower Config Data")]
    public TowerScript Tower;
    public bool TiltTowers = true;
    public bool HalfTilt = false;
    public GameObject PaddingSource;
    public TowerScript DifferentFirstTower;

    [Header("Runtime Data")]
    public float StartAngle = 180;
    public float EndAngle = 10;
    public List<TowerScript> Towers;
    public float CurrentDroopAmount = 0;

    private SettingControl setting;
    private bool Initialized = false;

    private void Initialize()
    {
        setting = GetComponent<SettingControl>();
    }

    public void Reset()
    {
        if (!Initialized) Initialize();
        if (180 + EndAngle == StartAngle) StartAngle += 0.1f;
        if (setting != null) setting.Run();

        SheavesParent.transform.localScale = new Vector3(SheaveScale, SheaveScale, SheaveScale);
        FullSheaveLayoutDescriptor descriptor = layouts.Descriptors[SheaveLayout];
        SheaveScriptLeft.NumWheels = descriptor.NumWheels;
        SheaveScriptLeft.Stage2Layout = descriptor.Stage2Layout;
        SheaveScriptLeft.Stage3Layout = descriptor.Stage3Layout;
        SheaveScriptLeft.Stage4Layout = descriptor.Stage4Layout;
        SheaveScriptLeft.Stage5Layout = descriptor.Stage5Layout;
        SheaveScriptLeft.StartAngle = StartAngle;
        SheaveScriptLeft.EndAngle = EndAngle;
        SheaveScriptRight.NumWheels = descriptor.NumWheels;
        SheaveScriptRight.Stage2Layout = descriptor.Stage2Layout;
        SheaveScriptRight.Stage3Layout = descriptor.Stage3Layout;
        SheaveScriptRight.Stage4Layout = descriptor.Stage4Layout;
        SheaveScriptRight.Stage5Layout = descriptor.Stage5Layout;
        SheaveScriptRight.StartAngle = 180 - EndAngle;
        SheaveScriptRight.EndAngle = 180 - StartAngle;

        SheaveScriptLeft.Reset();
        SheaveScriptRight.Reset();

        SheaveScriptLeft.transform.localPosition = new Vector3(Tower.Width / (2 * SheaveScale), SheaveScriptLeft.Radius, 0);
        SheaveScriptRight.transform.localPosition = new Vector3(-Tower.Width / (2 * SheaveScale), SheaveScriptRight.Radius, 0);

        SheaveScriptRight.transform.localEulerAngles = new Vector3(0, 180, 0);

        List<Sheave> AttachPoints = SheaveScriptLeft.CollectOpenSheaves();
        InitializeTowers(AttachPoints.Count);
        

        if (Recenter)
        {
            List<Sheave> AttachPointsAlt = SheaveScriptRight.CollectOpenSheaves();
            Vector3 worldPosition;
            if (AttachPoints.Count % 2 == 1)
            {
                worldPosition = AttachPoints[AttachPoints.Count / 2].transform.position;
                worldPosition += AttachPointsAlt[AttachPoints.Count / 2].transform.position;
                worldPosition /= 2;
            }
            else 
            {
                worldPosition = AttachPoints[AttachPoints.Count / 2].transform.position + AttachPoints[AttachPoints.Count / 2 - 1].transform.position;
                worldPosition += AttachPointsAlt[AttachPoints.Count / 2].transform.position + AttachPointsAlt[AttachPoints.Count / 2 - 1].transform.position;
                worldPosition /= 4;
            }
            SheavesParent.position += (transform.position - worldPosition);
        }
        if (EdgePoint)
        {
            List<Sheave> AttachPointsAlt = SheaveScriptRight.CollectOpenSheaves();
            Vector3 centerPosition;
            if (AttachPoints.Count % 2 == 1)
            {
                centerPosition = AttachPoints[AttachPoints.Count / 2].transform.position;
                centerPosition += AttachPointsAlt[AttachPoints.Count / 2].transform.position;
                centerPosition /= 2;
            }
            else
            {
                centerPosition = AttachPoints[AttachPoints.Count / 2].transform.position + AttachPoints[AttachPoints.Count / 2 - 1].transform.position;
                centerPosition += AttachPointsAlt[AttachPoints.Count / 2].transform.position + AttachPointsAlt[AttachPoints.Count / 2 - 1].transform.position;
                centerPosition /= 4;
            }


            Vector3 localOrigin = SheaveScriptRight.transform.position;
            Vector3 vec = (centerPosition - localOrigin);

            float rad = vec.magnitude;
            float theta = 180 - StartAngle + EndAngle;
            theta += CurrentDroopAmount * 2;
            theta /= 2f;
            theta *= Mathf.Deg2Rad;
            float newLength = rad / Mathf.Cos(theta);


            vec = vec.normalized * newLength;

            Vector3 worldPosition = localOrigin + vec;

            SheavesParent.position += (transform.position - worldPosition);
        }

        ResetFinalize();
    }

    private void ResetFinalize()
    {
        List<Sheave> AttachPoints = SheaveScriptLeft.CollectOpenSheaves();
        List<Sheave> AttachPointsAlt = SheaveScriptRight.CollectOpenSheaves();
        for (int i = 0; i < AttachPoints.Count; i++)
        {
            Vector3 tempRelPos = ((AttachPoints[i].TowerAttachPoint.position + AttachPointsAlt[AttachPoints.Count - 1 - i].TowerAttachPoint.position) / 2) - transform.position;

            float theta = Mathf.Deg2Rad * (transform.rotation.eulerAngles.y);
            float x = tempRelPos.x;
            float y = tempRelPos.z;

            //Thanks, wikipedia! https://en.wikipedia.org/wiki/Rotation_matrix
            Vector3 relPos = new Vector3(x * Mathf.Cos(theta) - y * Mathf.Sin(theta), tempRelPos.y, x * Mathf.Sin(theta) + y * Mathf.Cos(theta));
            Towers[i].transform.localPosition = relPos / transform.localScale.x;//Assuming uniform scaling
            if (TiltTowers)
            {
                Towers[i].transform.localEulerAngles = new Vector3(0, 90, AttachPoints[i].transform.localEulerAngles.x + SheaveScriptLeft.RotationParent.transform.localEulerAngles.x * (HalfTilt ? 0.5f : 1));
            }
            else
            {
                Towers[i].transform.localEulerAngles = new Vector3(0, 90, 0);
            }
        }

        foreach (TowerScript tower in Towers)
        {
            tower.Reset();
        }

    }

    private void InitializeTowers(int numberTowers)
    {
        for (int i = 0; i < Towers.Count; i++)
        {
            Towers[i].gameObject.SetActive(i < numberTowers);
        }
        if (Towers == null)
        {
            Towers = new List<TowerScript>(numberTowers);
        }
        if (Towers.Count == numberTowers)
        {
            return;
        }
        if (numberTowers > Towers.Count)
        {
            for (int i = Towers.Count; i < numberTowers; i++)
            {
                TowerScript temp;
                if (i == 0 && DifferentFirstTower != null)
                {
                    temp = Instantiate(DifferentFirstTower, transform);
                } 
                else
                {
                    temp = Instantiate(Tower, transform);
                }
                temp.PaddingSource = PaddingSource;
                Towers.Add(temp);
            }
        }
    }
}