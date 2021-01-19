﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Familiar", menuName = "Familiar/Create new Familiar")]
public class FamiliarBase : ScriptableObject
{
    [SerializeField] string name;

    [TextArea]
    [SerializeField] string description;

    [SerializeField] Sprite familiarSprite;

    [SerializeField] Type[] types = new Type[2];

    //Base Stats
    [SerializeField] int maxHp;
    [SerializeField] int attack;
    [SerializeField] int defense;
    [SerializeField] int spAttack;
    [SerializeField] int spDefense;
    [SerializeField] int speed;

    [SerializeField] List<LearnableAttack> learnableAttacks;


    public string Name
    {
        get { return name; }
    }

    public string Description
    {
        get { return description; }
    }

    public Sprite FamiliarSprite
    {
        get { return familiarSprite; }
    }

    public Type[] Types
    {
        get { return types; }
    }

    public int MaxHp
    {
        get { return maxHp; }
    }
    public int Attack
    {
        get { return attack; }
    }
    public int Defense
    {
        get { return defense; }
    }
    public int SpAttack
    {
        get { return SpAttack; }
    }
    public int SpDefense
    {
        get { return SpDefense; }
    }
    public int Speed
    {
        get { return speed; }
    }

    public List<LearnableAttack> LearnableAttacks
    {
        get { return learnableAttacks;  }
    }

}

[System.Serializable]
public class LearnableAttack
{
    [SerializeField] AttackBase attackBase;
    [SerializeField] int level;

    public AttackBase Base
    {
        get { return attackBase; }
    }

    public int Level
    {
        get { return level; }
    }
}



public enum Type
{
    None,
    Normal,
    Fire,
    Aqua,
    Plant,
    Earth,
    Air,
    Shock,
    Force,
    Arcane,
    Sweet,
    Sound,
    Bug,
    Light,
    Dark,
    Ancient
}

