using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Pattern", menuName = "Familiar/New Pattern")]
public class PatternBase : ScriptableObject
{
    [SerializeField] bool[] active = new bool[9];

    public bool[] Active
    {
        get { return active; }
    }
}
