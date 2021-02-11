using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Familiar", menuName = "Familiar/Create new Familiar")]
public class FamiliarBase : ScriptableObject
{
    [SerializeField] string name;

    [TextArea]
    [SerializeField] string description;

    [SerializeField] Sprite familiarSprite;

    [SerializeField] Types[] type = new Types[2];

    //Base Stats
    [SerializeField] int maxHp;
    [SerializeField] int attack;
    [SerializeField] int defense;
    [SerializeField] int spAttack;
    [SerializeField] int spDefense;
    [SerializeField] int speed;
    [SerializeField] int movement;

    [SerializeField] AudioClip attackSound;
    [SerializeField] AudioClip injuredSound;

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

    public Types[] Type
    {
        get { return type; }
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
        get { return spAttack; }
    }
    public int SpDefense
    {
        get { return spDefense; }
    }
    public int Speed
    {
        get { return speed; }
    }

    public int Movement
    {
        get { return movement; }
    }

    public AudioClip AttackSound
    {
        get { return attackSound; }
    }

    public AudioClip InjuredSound
    {
        get { return injuredSound; }
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



public enum Types
{
    None,
    Normal,
    Fire,
    Aqua,
    Nature,
    Earth,
    Air,
    Shock,
    Force,
    Arcane,
    Sweet,
    Sound,
    Light,
    Dark,
    Ancient
}

