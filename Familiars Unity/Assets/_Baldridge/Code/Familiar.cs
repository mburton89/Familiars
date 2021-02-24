using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Familiar
{
    [SerializeField] FamiliarBase _base;
    [SerializeField] int level;

    public FamiliarBase Base
    {
        get { return _base; }
    }

    public int Level
    {
        get { return level; }
    } 

    public int HP { get; set; }

    public List<Attack> Attacks { get; set; }

    public void Init()
    {
        HP = MaxHp;

        // Generate Attacks
        Attacks = new List<Attack>();
        foreach (var attack in Base.LearnableAttacks)
        {
            if (attack.Level <= Level)
            {
                Attacks.Add(new Attack(attack.Base));
            }

            if (Attacks.Count >= 4)
                break;
        }
    }

    /*public Familiar(FamiliarBase fBase, int fLevel)
    {
        //Base = fBase;
        //Level = fLevel;
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

    } */

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
        get { return Mathf.FloorToInt((Base.MaxHp * Level) / 100f) + 30; }
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

    public DamageDetails TakeDamage(Attack attack, Familiar attacker)
    {
        //float modifier = Random.Range(0.8f, 1f);
        //float a = (2 * attacker.Level + 10) / 250f;
        //float d = a * attack.Base.Power * ((float)attacker.Attack / Defense) + 2;
        //int damage = Mathf.FloorToInt(d * modifier);

        //Critical Hit?
        float crit = 1f;
        if (Random.value * 100f <= 6.25)
        {
            crit = 2f;
        }

        float type = TypeChart.GetEffectiveness(attack.Base.Type, this.Base.Type[0]) * TypeChart.GetEffectiveness(attack.Base.Type, this.Base.Type[1]);
        float mod = Random.Range(0.8f, 1f);
        float d = ((attack.Base.Power + attacker.Attack) - Defense) + (((attack.Base.Magic + attacker.SpAttack) - SpDefense) * type);

        var damageDetails = new DamageDetails()
        {
            TypeEffectiveness = type,
            Critical = crit,
            Fainted = false
        };
        

        Debug.Log("Target HP: " + HP + ", Incoming Physical Damage: " + ((attack.Base.Power + attacker.Attack) - Defense)
                + ", Incoming Special Damage: " + ((attack.Base.Magic + attacker.SpAttack) - SpDefense) + ".");
        int damage = Mathf.FloorToInt(d * mod);
        HP -= damage;
        if (HP <= 0)
        {
            HP = 0;
            damageDetails.Fainted = true;
            //return true;
        }

        return damageDetails;

    }

    public Attack GetRandomAttack()
    {
        int _r = Random.Range(0, Attacks.Count);
        return Attacks[_r];
    }
}


public class DamageDetails
{
    public bool Fainted { get; set; }

    public float Critical { get; set; }

    public float TypeEffectiveness { get; set; }
}