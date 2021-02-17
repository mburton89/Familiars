using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AttackStyle { None, Target, Launch, Area, AreaStatic }
// None   - doesn't target, can be global or always targets self, or something else
// Target - Targets a single tile/unit
// Launch - Starts at an origin point until it reaches a valid target
// Area   - Targets a section of targets or all targets

[CreateAssetMenu(fileName = "New Attack", menuName = "Familiar/Attacks")]
public class AttackBase : ScriptableObject
{
    [SerializeField] string name;

    [TextArea]
    [SerializeField] string description;

    [SerializeField] int power;
    [SerializeField] int magic;
    [SerializeField] int accuracy;
    [SerializeField] int range;
    [SerializeField] int uses;
    [SerializeField] Types type;

    [SerializeField] int originPosition = 4;

    [SerializeField] AttackStyle attackStyle;
    
    [SerializeField] PatternBase sources; // The places that the familiar can use the attack from
    [SerializeField] PatternBase targets; // The places that the familiar is able to target with the move
    [SerializeField] PatternBase targetingReticle; // the shape that the attack resolves in (AOE's and such)

    // For Projectile
    [SerializeField] int projectileOrigin = 1;
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

    public int OriginPosition
    {
        get { return originPosition; }
    }

    public int Direction
    {
        get { return direction; }
    }

    public AttackStyle AttackStyle
    {
        get { return attackStyle;  }
    }

    //
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

    public int ProjectileOrigin
    {
        get { return projectileOrigin; }
    }

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


}