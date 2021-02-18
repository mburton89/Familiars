using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class Attack
{ 
    public AttackBase Base { get; set; }

    public int Uses { get; set; }

    public Attack(AttackBase fBase)
    {
        Base = fBase;
        Uses = fBase.Uses;
    }
}
