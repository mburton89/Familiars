using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AttackStyle { None, Target, Projectile, Area, AreaStatic }
// None   - doesn't target, can be global or always targets self, or something else
// Target - Targets a single tile/unit
// Projectile - Starts at an origin point until it reaches a valid target
// Area   - Targets a section of targets or all targets
// AreaStatic  - Target an immovable area of targets.
[CreateAssetMenu(fileName = "New Attack", menuName = "Familiar/Attacks")]
public class AttackBase : ScriptableObject
{
    [SerializeField] string name;

    [TextArea]
    [SerializeField] string description;

    [SerializeField] int power;
    [SerializeField] int magic;
    [SerializeField] int accuracy;
    [SerializeField] bool alwaysHits;
    [SerializeField] int range;
    [SerializeField] int uses;
    
    [SerializeField] Types type;

    [SerializeField] int targetOriginPosition = 4;
    [SerializeField] int sourceOriginPosition = 4;

    [SerializeField] AttackStyle attackStyle;
    [SerializeField] AttackCategory category;
    [SerializeField] AttackEffects effects;
    [SerializeField] List<SecondaryEffects> secondaries;
    [SerializeField] AttackTarget target;

    [SerializeField] bool relative;
    
    [SerializeField] PatternBase sources; // The places that the familiar can use the attack from
    [SerializeField] PatternBase targets; // The places that the familiar is able to target with the move
    [SerializeField] PatternBase targetingReticle; // the shape that the attack resolves in (AOE's and such)

    [SerializeField] PatternBase[] sourceArray;
    [SerializeField] PatternBase[] targetArray;
    [SerializeField] PatternBase[] targetingReticleArray;

    // For Projectile
    [SerializeField] int projectileOrigin = 1;
    [SerializeField] PatternBase eligibleOrigins;
    [SerializeField] int direction; // 0 - back, down, forward, up

    // For AOE
    [SerializeField] int upperX = 2;
    [SerializeField] int upperY = 2;
    [SerializeField] int lowerX = 0;
    [SerializeField] int lowerY = 0;

    public string Name
    {
        get { return name; }
    }
    public string Description
    {
        get { return description; }
    }
    public int Power
    {
        get { return power; }
    }

    public int Magic
    {
        get { return magic; }
    }

    public int Accuracy
    {
        get { return accuracy; }
    }

    public bool AlwaysHits
    {
        get { return alwaysHits; }
    }

    public int Range
    {
        get { return range; }
    }
    public int Uses
    {
        get { return uses; }
    }

    public Types Type
    {
        get { return type; }
    }

    public int TargetOriginPosition
    {
        get { return targetOriginPosition; }
    }

    public int SourceOriginPosition
    {
        get { return sourceOriginPosition; }
    }
    
    public int Direction
    {
        get { return direction; }
    }

    public AttackStyle AttackStyle
    {
        get { return attackStyle;  }
    }

    public AttackCategory Category
    {
        get { return category; }
    }

    public AttackEffects Effects
    {
        get { return effects; }
    }

    public List<SecondaryEffects> Secondaries
    {
        get { return secondaries; }
    }

    public AttackTarget Target
    {
        get { return target; }
    }



    #region Relative Targeting
    public bool Relative
    {
        get { return relative; }
    }
    #endregion

    //
    #region Field Patterns
    public PatternBase Sources
    {
        get { return sources; }
    }

    public PatternBase Targets
    {
        get { return targets; }
    }

    public PatternBase TargetingReticle
    {
        get { return targetingReticle;  }
    }

    public PatternBase[] SourceArray
    {
        get { return sourceArray; }
    }

    public PatternBase[] TargetArray
    {
        get { return targetArray; }
    }

    public PatternBase[] TargetingReticleArray
    {
        get { return targetingReticleArray; }
    }
    #endregion

    public int ProjectileOrigin
    {
        get { return projectileOrigin; }
    }

    public PatternBase EligibleOrigins
    {
        get { return eligibleOrigins;  }
    }


    #region Area Limits

    public int UpperX
    {
        get { return upperX; }
    }
    public int UpperY
    {
        get { return upperY; }
    }
    public int LowerX
    {
        get { return lowerX; }
    }
    public int LowerY
    {
        get { return lowerY; }
    }
    #endregion


}

[System.Serializable]
public class AttackEffects
{
    [SerializeField] List<StatBoost> boosts;
    [SerializeField] ConditionID status;
    [SerializeField] ConditionID volatileStatus;
    [SerializeField] ForcedMovement movement;

    public List<StatBoost> Boosts
    {
        get { return boosts; }
    }

    public ConditionID Status
    {
        get { return status; }
    }

    public ConditionID VolatileStatus
    {
        get { return volatileStatus; }
    }

    public ForcedMovement Movement
    {
        get { return movement; }
    }
}

[System.Serializable]
public class SecondaryEffects : AttackEffects
{
    [SerializeField] int chance;
    [SerializeField] AttackTarget target;

    public int Chance
    {
        get { return chance; }
    }

    public AttackTarget Target
    {
        get { return target; }
    }
}


[System.Serializable]
public class StatBoost
{
    public Stat stat;
    public int boost;
}

[System.Serializable]
public class ForcedMovement
{
    public int direction;
    public int squares;
}


public enum AttackCategory
{
    Attack, Status
}

public enum AttackTarget
{
    Enemy, Ally
}
