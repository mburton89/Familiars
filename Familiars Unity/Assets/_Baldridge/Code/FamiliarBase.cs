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


public class TypeChart
{
    static float[][] chart =
    {
        // Attacker vv  Defender ->  NOR FIR AQA NAT EAR AIR SCK FRC ARC SWT SND LGT DRK ACT
        /*  Normal   */ new float[] { 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f,.5f, 1f, 1f, 1f, 1f,.5f},
        /*  Fire     */ new float[] { 2f,.5f,.5f, 2f,.5f, 2f, 1f, 1f, 1f, 2f, 1f, 1f, 1f, 1f},
        /*  Aqua     */ new float[] { 1f, 2f,.5f,.5f, 2f, 1f,.5f, 1f, 1f, 2f, 1f, 1f, 1f, 1f},
        /*  Nature   */ new float[] { 1f,.5f, 2f,.5f, 2f,.5f, 1f, 1f, 2f, 1f, 1f, 1f, 1f, 1f},
        /*  Earth    */ new float[] { 1f, 1f, 1f, 1f, 2f, 0f, 2f, 1f, 1f, 1f, 1f, 1f, 1f, 1f},
        /*  Air      */ new float[] { 1f, 2f, 1f, 2f,.5f,.5f, 1f, 2f, 1f, 1f, 1f, 1f, 1f, 1f},
        /*  Shock    */ new float[] { 1f, 1f, 2f, 1f, 1f, 1f,.5f, 1f, 1f, 1f, 1f, 1f, 1f, 1f},
        /*  Force    */ new float[] { 1f, 1f, 1f, 2f, 2f, 1f, 1f, 1f,.5f, 2f,.5f, 2f, 2f,.5f},
        /*  Arcane   */ new float[] { 1f, 1f, 1f, 1f, 1f, 1f, 1f, 2f,.5f,.5f, 2f,.5f, 2f,.5f},
        /*  Sweet    */ new float[] { 1f,.5f,.5f,.5f,.5f,.5f,.5f, 1f,.5f, 1f, 1f, 1f, 2f,.5f},
        /*  Sound    */ new float[] { 1f, 1f, 1f, 1f, 1f, 1f, 1f, 2f, 2f, 1f,.5f, 0f, 0f, 1f},
        /*  Light    */ new float[] { 1f, 1f, 1f, 1f, 1f, 1f, 1f,.5f, 2f, 1f,.5f,.5f, 2f, 2f},
        /*  Dark     */ new float[] {.5f, 1f, 1f, 1f, 1f, 1f, 1f, 2f, 2f, 2f,.5f,.5f,.5f, 1f},
        /*  Ancient  */ new float[] { 2f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f,.5f, 1f, 1f, 1f, 2f}
    };

    public static float GetEffectiveness(Types attackType, Types defenseType)
    {
        if (attackType == Types.None || defenseType == Types.None)
        {
            return 1;
        }

        int row = (int)attackType - 1;
        int col = (int)defenseType - 1;

        return chart[row][col];
    }
}

