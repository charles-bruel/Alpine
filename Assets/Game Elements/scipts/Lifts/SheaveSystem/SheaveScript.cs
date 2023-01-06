using System.Collections.Generic;
using UnityEngine;
using System;


public class SheaveScript : MonoBehaviour
{
    [Header("Debug")]
    public bool UpdateToggle;

    [Header("Control")]
    public float StartAngle = 180;
    public float EndAngle = 10;

    [Header("Config Data")]
    public GameObject Wheel;
    public GameObject RotationParent;
    public Sheave ComboSheave;
    public Sheave SheaveStage1;
    public Sheave SheaveStage2;
    public Sheave SheaveStage3;
    public Sheave SheaveStage4;
    public Sheave SheaveStage5;
    public int NumWheels;
    public float WheelSpacing;
    public bool EnableCatwalk = true;
    public CatwalkScript Catwalk;
    public Vector2Int[] Stage2Layout;
    public Vector2Int[] Stage3Layout;
    public Vector2Int[] Stage4Layout;
    public Vector2Int[] Stage5Layout;
    public float TowerSpacing = 1;
    public float MaxSupportAngleCombo = 2;
    public float MaxDepressionAngleCombo = 10;

    [Header("Runtime Data")]
    public GameObject SheaveParent;
    public GameObject ComboParent;
    public GameObject Combo;
    public List<GameObject> Wheels;
    public List<Sheave> Stage1s;
    public List<Sheave> Stage2s;
    public List<Sheave> Stage3s;
    public List<Sheave> Stage4s;
    public List<Sheave> Stage5s;
    public float Length;
    public List<float> Offsets;
    public float Radius;
    public bool Uphill;
    public bool ComboEnable;
    public bool ComboInitialized;
    public bool LastToggle;
    public bool Initialized;

    void Start()
    {
    }

    public List<Transform> GetAllCablePoints(bool above)
    {
        List<Transform> toReturn = new List<Transform>();
        if (ComboEnable)
        {
            Transform t = Stage1s[0].transform.GetChild(0);
            for (int i = 0; i < t.childCount; i++)
            {
                toReturn.Add(t.GetChild(i));
            }
            return toReturn;
        }
        if (Wheels == null) return toReturn;
        foreach (GameObject go in Wheels)
        {
            toReturn.Add(go.transform.GetChild(above ? 1 : 2));
        }
        return toReturn;
    }

    public void Reset()
    {
        if (!Initialized)
        {
            SheaveParent = new GameObject();
            SheaveParent.transform.SetParent(RotationParent.transform, false);
            SheaveParent.name = "Sheave Parent";
            ComboParent = new GameObject();
            ComboParent.transform.SetParent(RotationParent.transform, false);
            ComboParent.name = "Combo Parent";
            Initialized = true;
        }

        float AngleChange = 180 - StartAngle + EndAngle;
        ComboEnable = ComboSheave != null && AngleChange < MaxDepressionAngleCombo && AngleChange > -MaxSupportAngleCombo;

        ResetSheaves();
    }

    private void ResetSheaves()
    {
        if (Wheels != null)
        {
            foreach (var obj in Wheels)
            {
                Destroy(obj);
            }
        }
        if (Stage1s != null)
        {
            foreach (var obj in Stage1s)
            {
                Destroy(obj.gameObject);
            }
        }
        if (Stage2s != null)
        {
            foreach (var obj in Stage2s)
            {
                Destroy(obj.gameObject);
            }
        }
        if (Stage3s != null)
        {
            foreach (var obj in Stage3s)
            {
                Destroy(obj.gameObject);
            }
        }
        if (Stage4s != null)
        {
            foreach (var obj in Stage4s)
            {
                Destroy(obj.gameObject);
            }
        }
        if (Stage5s != null)
        {
            foreach (var obj in Stage5s)
            {
                Destroy(obj.gameObject);
            }
        }

        Wheels = new List<GameObject>();
        Stage1s = new List<Sheave>();
        Stage2s = new List<Sheave>();
        Stage3s = new List<Sheave>();
        Stage4s = new List<Sheave>();
        Stage5s = new List<Sheave>();

        if (NumWheels % 2 == 1)
        {
            NumWheels++;
        }

        for (int i = 0; i < (ComboEnable ? 2 : NumWheels); i++)
        {
            GameObject temp = Instantiate(Wheel, SheaveParent.transform);
            if (ComboEnable)
            {
                temp.SetActive(false);
            }
            Wheels.Add(temp);
            
        }

        int id = 0;
        if (!ComboEnable)
        {
            for (int i = 0; i < NumWheels / 2; i++)
            {
                Stage1s.Add(Instantiate(SheaveStage1, SheaveParent.transform));
                Stage1s[i].ID = id++;
                Stage1s[i].ParentSheaveObject = this;
            }
            for (int i = 0; i < Stage2Layout.Length; i++)
            {
                Stage2s.Add(Instantiate(SheaveStage2, SheaveParent.transform));
                Stage2s[i].ID = id++;
                Stage2s[i].ParentSheaveObject = this;
            }
            for (int i = 0; i < Stage3Layout.Length; i++)
            {
                Stage3s.Add(Instantiate(SheaveStage3, SheaveParent.transform));
                Stage3s[i].ID = id++;
                Stage3s[i].ParentSheaveObject = this;
            }
            for (int i = 0; i < Stage4Layout.Length; i++)
            {
                Stage4s.Add(Instantiate(SheaveStage4, SheaveParent.transform));
                Stage4s[i].ID = id++;
                Stage4s[i].ParentSheaveObject = this;
            }
            for (int i = 0; i < Stage5Layout.Length; i++)
            {
                Stage5s.Add(Instantiate(SheaveStage5, SheaveParent.transform));
                Stage5s[i].ID = id++;
                Stage5s[i].ParentSheaveObject = this;
            }
        } 
        else
        {
            Stage1s.Add(Instantiate(ComboSheave, SheaveParent.transform));
            Stage1s[0].ID = id++;
            Stage1s[0].ParentSheaveObject = this;
        }
        for (int i = 0; i < Stage1s.Count; i++)
        {
            Wheels[i * 2 + 0].transform.parent = Stage1s[i].transform;
            Wheels[i * 2 + 1].transform.parent = Stage1s[i].transform;

            Wheels[i * 2 + 0].transform.localPosition = new Vector3(0, 0, -SheaveStage1.Length / 2f);
            Wheels[i * 2 + 1].transform.localPosition = new Vector3(0, 0, SheaveStage1.Length / 2f);
        }

        DoSheaveStage(Stage2Layout, Stage2s, CollectOpenSheaves());
        DoSheaveStage(Stage3Layout, Stage3s, CollectOpenSheaves());
        DoSheaveStage(Stage4Layout, Stage4s, CollectOpenSheaves());
        DoSheaveStage(Stage5Layout, Stage5s, CollectOpenSheaves());

        ManageMultipleTopLevels();

        UpdateSheavePositions();

        ApplyAngle();
    }

    private void ManageMultipleTopLevels()
    {
        List<float> Lengths = new List<float>();
        List<Sheave> topLevels = CollectOpenSheaves();
        foreach (Sheave sheave in topLevels)
        {
            Vector2 bounds = GenerateSheavePositions(sheave, 0);
            Lengths.Add(bounds.y - bounds.x);
        }
        Offsets = new List<float>();
        float runningOffset = 0;
        for (int i = 0; i < topLevels.Count; i++)
        {
            Offsets.Add(runningOffset);
            runningOffset += Lengths[i] + WheelSpacing * (1 + TowerSpacing);
        }
        if (topLevels.Count <= 1) return;
        for (int i = 1; i < topLevels.Count; i++)
        {
            topLevels[i].transform.localPosition = new Vector3(0, 0, Offsets[i]);
        }
    }

    private void UpdateSheavePositions()
    {
        float min = float.MaxValue;
        float max = float.MinValue;
        List<Sheave> topLevels = CollectOpenSheaves();
        for (int i = 0; i < topLevels.Count; i++)
        {
            Sheave sheave = topLevels[i];
            Vector2 bounds = GenerateSheavePositions(sheave, Offsets[i]);
            if (bounds.x < min)
            {
                min = bounds.x;
            }
            if (bounds.y > max)
            {
                max = bounds.y;
            }
        }
        foreach (Sheave sheave in CollectOpenSheaves())
        {
            NormalizeSheavePositions(sheave, -min);
        }
        Length = max - min;
    }

    private Vector2 GenerateSheavePositions(Sheave sheave, float position)
    {
        float lh = sheave.Length / 2;
        sheave.StartPos = position - lh;
        sheave.EndPos = position + lh;
        float min = sheave.StartPos;
        float max = sheave.EndPos;
        if (sheave.ChildAtA != null)
        {
            Vector2 temp = GenerateSheavePositions(sheave.ChildAtA, sheave.StartPos);
            if (temp.x < min)
            {
                min = temp.x;
            }
            if (temp.y > max)
            {
                max = temp.y;
            }
        }
        if (sheave.ChildAtB != null)
        {
            Vector2 temp = GenerateSheavePositions(sheave.ChildAtB, sheave.EndPos);
            if (temp.x < min)
            {
                min = temp.x;
            }
            if (temp.y > max)
            {
                max = temp.y;
            }
        }
        return new Vector2(min, max);
    }

    private void NormalizeSheavePositions(Sheave sheave, float offset)
    {
        sheave.StartPos += offset;
        sheave.EndPos += offset;
        if (sheave.ChildAtA != null)
        {
            NormalizeSheavePositions(sheave.ChildAtA, offset);
        }
        if (sheave.ChildAtB != null)
        {
            NormalizeSheavePositions(sheave.ChildAtB, offset);
        }
    }

    public List<Sheave> CollectOpenSheaves()
    {
        List<Sheave> toReturn = new List<Sheave>();
        foreach (Sheave sheave in Stage1s)
        {
            CheckSheaveOpen(sheave, toReturn);
        }
        return toReturn;
    }

    private void CheckSheaveOpen(Sheave sheave, List<Sheave> list)
    {
        if (list.Contains(sheave)) return;
        if (sheave.Open)
        {
            list.Add(sheave);
        }
        else
        {
            CheckSheaveOpen(sheave.Parent, list);
        }
    }

    private void DoSheaveStage(Vector2Int[] Layout, List<Sheave> Sheaves, List<Sheave> OpenSheaves)
    {
        for (int i = 0; i < Sheaves.Count; i++)
        {
            Vector2Int CurrentLayout = Layout[i];

            OpenSheaves[CurrentLayout.x].transform.parent = Sheaves[i].transform;
            OpenSheaves[CurrentLayout.y].transform.parent = Sheaves[i].transform;

            OpenSheaves[CurrentLayout.x].transform.localPosition = new Vector3(0, 0, -Sheaves[i].Length / 2f);
            OpenSheaves[CurrentLayout.y].transform.localPosition = new Vector3(0, 0, Sheaves[i].Length / 2f);

            OpenSheaves[CurrentLayout.x].Open = false;
            OpenSheaves[CurrentLayout.y].Open = false;

            OpenSheaves[CurrentLayout.x].Parent = Sheaves[i];
            OpenSheaves[CurrentLayout.y].Parent = Sheaves[i];

            Sheaves[i].ChildAtA = OpenSheaves[CurrentLayout.x];
            Sheaves[i].ChildAtB = OpenSheaves[CurrentLayout.y];
        }
    }

    void Update()
    {
        if (UpdateToggle != LastToggle)
        {
            LastToggle = UpdateToggle;
            Reset();
        }
    }

    private void ApplyAngle()
    {
        float a = StartAngle;
        float b = EndAngle;
        if (a < 0)
        {
            a += 360;
        }
        if (b < 0)
        {
            b += 360;
        }
        if (b > a)
        {
            a += 360;
        }
        float RequiredAngle = a - b;
        if (RequiredAngle > 180)
        {
            RequiredAngle = -360 + RequiredAngle;
        }

        float RequiredAngleDelta = 180 - Mathf.Abs(RequiredAngle);
        RequiredAngleDelta *= Mathf.Sign(RequiredAngle);

        float degreesPerUnit = RequiredAngleDelta / Length;
        Radius = (360 / degreesPerUnit) / (2 * Mathf.PI);

        foreach (Sheave sheave in CollectOpenSheaves())
        {
            ApplyRotation(sheave, degreesPerUnit);
            FixPosition(sheave);
        }

        RotationParent.transform.localEulerAngles = new Vector3(180 - a, 0, 0);

        Uphill = RequiredAngle > 0;

        if (EnableCatwalk) {
            Catwalk.Radius = Radius;
            Catwalk.RequiredAngle = RequiredAngleDelta;
            Catwalk.RotationOffset = 180 - a;
            Catwalk.Generate();
        }
    }

    private void ApplyRotation(Sheave sheave, float degreesPerUnit)
    {
        float targetRotation = ((sheave.StartPos + sheave.EndPos) * degreesPerUnit) / 2;
        sheave.TotalSheaveRotation = targetRotation;
        float parentRotation = 0;
        if (sheave.Parent != null)
        {
            parentRotation = sheave.Parent.TotalSheaveRotation;
        }
        sheave.transform.localEulerAngles = new Vector3(-targetRotation + parentRotation, 0, 0);
        if (sheave.ChildAtA != null)
        {
            ApplyRotation(sheave.ChildAtA, degreesPerUnit);
        }
        if (sheave.ChildAtB != null)
        {
            ApplyRotation(sheave.ChildAtB, degreesPerUnit);
        }
    }

    private void FixPosition(Sheave sheave)
    {
        float theta = (90 + sheave.TotalSheaveRotation) * Mathf.Deg2Rad;
        float dx = -Mathf.Cos(theta) * Radius;
        float dy = -Mathf.Sin(theta) * Radius;
        sheave.transform.localPosition = new Vector3(0, dy, dx);
    }

}