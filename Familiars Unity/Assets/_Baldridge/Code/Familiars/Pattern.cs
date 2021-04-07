using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Pattern
{
    public PatternBase Base { get; set; }
    public bool linked;

    public bool[] Active { get; set; }

    //public bool Linked
    //{
    //    get { return linked; }
    //}

}
