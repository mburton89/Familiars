using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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
