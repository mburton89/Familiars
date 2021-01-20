using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Attack", menuName = "Familiar/Attacks")]
public class AttackBase : ScriptableObject
{
    [SerializeField] string name;

    [TextArea]
    [SerializeField] string description;

    [SerializeField] int power;
    [SerializeField] int accuracy;
    [SerializeField] int uses;
    [SerializeField] Type type;

    [SerializeField] PatternBase sources; // The places that the familiar can use the attack from
    [SerializeField] PatternBase targets; // The places that the familiar is able to target with the move
    [SerializeField] PatternBase targetingReticle; // the shape that the attack resolves in (AOE's and such)

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

    public int Accuracy
    {
        get { return accuracy; }
    }

    public int Uses
    {
        get { return uses; }
    }

    public Type Type
    {
        get { return type; }
    }

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



}