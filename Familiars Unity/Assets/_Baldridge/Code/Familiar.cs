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

    public bool TakeDamage(int damage)
    {
        HP -= damage;
        if (HP <= 0)
        {
            HP = 0;
            return true;
        }
        return false;
    }

    public bool TakeDamage(Attack attack, Familiar attacker)
    {
        float modifier = Random.Range(0.8f, 1f);
        float a = (2 * attacker.Level + 10) / 250f;
        float d = a * attack.Base.Power * ((float)attacker.Attack / Defense) + 2;
        int damage = Mathf.FloorToInt(d * modifier);

        HP -= damage;
        if (HP <= 0)
        {
            HP = 0;
            return true;
        }

        return false;

    }

    public Attack GetRandomAttack()
    {
        int _r = Random.Range(0, Attacks.Count);
        return Attacks[_r];
    }
}
