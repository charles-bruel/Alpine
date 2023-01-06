using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sheave : MonoBehaviour
{
    [Header("Config Data")]
    public float Length;
    public int Level;
    public Transform TowerAttachPoint;

    [Header("Runtime Data")]
    public bool Open = true;
    public Sheave Parent;
    public Sheave ChildAtA;
    public Sheave ChildAtB;
    public int ID;
    public float StartPos;
    public float EndPos;
    public float TotalSheaveRotation;
    public SheaveScript ParentSheaveObject;
}