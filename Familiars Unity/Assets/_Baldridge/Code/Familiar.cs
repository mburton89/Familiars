using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Familiar
{
    public FamiliarBase Base { get; set; }
    public int Level { get; set; } 

    public int HP { get; set; }

    public List<Attack> Attacks { get; set; }

    public Familiar(FamiliarBase fBase, int fLevel)
    {
        Base = fBase;
        Level = fLevel;
        HP = MaxHp;

        // Generate Attacks
        Attacks = new List<Attack>();
        foreach (var attack in Base.LearnableAttacks)
        {
            if(attack.Level <= Level)
            {
                Attacks.Add(new Attack(attack.Base));
            }

            if (Attacks.Count >= 4)
                break;
        }

    }

    public int Attack
    {
        get { return Mathf.FloorToInt((Base.Attack * Level) / 100f) + 5; }
    }

    public int Defense
    {
        get { return Mathf.FloorToInt((Base.Defense * Level) / 100f) + 5; }
    }

    public int SpAttack
    {
        get { return Mathf.FloorToInt((Base.SpAttack * Level) / 100f) + 5; }
    }

    public int SpDefense
    {
        get { return Mathf.FloorToInt((Base.SpDefense * Level) / 100f) + 5; }
    }

    public int Speed
    {
        get { return Mathf.FloorToInt((Base.Speed * Level) / 100f) + 5; }
    }

    public int MaxHp
    {
        get { return Mathf.FloorToInt((Base.MaxHp * Level) / 100f) + 10; }
    }

}
