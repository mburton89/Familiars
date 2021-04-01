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

    public Familiar (FamiliarBase fBase, int level)
    {
        _base = fBase;
        this.level = level;
    }

    public int HP { get; set; }

    public int RandomID { get; set; }

    public List<Attack> Attacks { get; set; }
    public Dictionary<Stat, int> Stats { get; private set; }
    public Dictionary<Stat, int> StatBoosts { get; private set; }

    public void Init()
    {
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


        CalculateStats();
        HP = MaxHp;
        RandomID = Random.Range(100000, 999999);

        StatBoosts = new Dictionary<Stat, int>()
        {
            {Stat.Attack, 0},
            {Stat.Defense, 0},
            {Stat.SpAttack, 0},
            {Stat.SpDefense, 0},
            {Stat.Speed, 0},
            {Stat.Movement, 2}
        };

    }

    void CalculateStats()
    {
        Stats = new Dictionary<Stat, int>();
        Stats.Add(Stat.Attack, Mathf.FloorToInt((Base.Attack * Level) / 100f) + 5);
        Stats.Add(Stat.Defense, Mathf.FloorToInt((Base.Defense * Level) / 100f) + 5);
        Stats.Add(Stat.SpAttack, Mathf.FloorToInt((Base.SpAttack * Level) / 100f) + 5);
        Stats.Add(Stat.SpDefense, Mathf.FloorToInt((Base.SpDefense * Level) / 100f) + 5);
        Stats.Add(Stat.Speed, Mathf.FloorToInt((Base.Speed * Level) / 100f) + 5);
        Stats.Add(Stat.Movement, Base.Movement);

        MaxHp = Mathf.FloorToInt((Base.MaxHp * Level) / 100f) + 30;
    }

    int GetStat(Stat stat)
    {
        int statVal = Stats[stat];

        // Apply stat boosts;
        int boost = StatBoosts[stat];

        if (stat != Stat.Movement)
        {
            var boostValues = new float[] { 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f };

            if (boost >= 0)
            {
                statVal = Mathf.FloorToInt(statVal * boostValues[boost]);
            }
            else
            {
                statVal = Mathf.FloorToInt(statVal / boostValues[-boost]);
            }
        }
        else
        {
            statVal = boost; 
        }

        return statVal;
    }

    public void ApplyBoosts(List<StatBoost> statBoosts)
    {
        foreach (var statBoost in statBoosts)
        {
            var stat = statBoost.stat;
            var boost = statBoost.boost;

            StatBoosts[stat] = Mathf.Clamp(StatBoosts[stat] + boost, -6, 6);
            Debug.Log($"{Base.Name} - {RandomID}'s {stat} has been boosted to {StatBoosts[stat]}");
        }
    }

    public int Attack
    {
        get { return GetStat(Stat.Attack); }
    }

    public int Defense
    {
        get { return GetStat(Stat.Defense); }
    }

    public int SpAttack
    {
        get { return GetStat(Stat.SpAttack); }
    }

    public int SpDefense
    {
        get { return GetStat(Stat.SpDefense); }
    }

    public int Speed
    {
        get { return GetStat(Stat.Speed); }
    }

    public int Movement
    {
        get { return GetStat(Stat.Movement); }
    }


    public int MaxHp { get; private set; }

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

        Debug.Log("[Familiar.cs/TakeDamage()] Damage: " + d);

        var damageDetails = new DamageDetails()
        {
            TypeEffectiveness = type,
            Critical = crit,
            Fainted = false
        };
        
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