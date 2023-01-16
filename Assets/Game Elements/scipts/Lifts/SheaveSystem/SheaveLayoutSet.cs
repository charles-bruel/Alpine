using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Sheave Layout Set", menuName = "Game Elements/Default/Sheave Layout Set", order = 1)]
public class SheaveLayoutSet : ScriptableObject
{
    public FullSheaveLayoutDescriptor[] Descriptors;
}