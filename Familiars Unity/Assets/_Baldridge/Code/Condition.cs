using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Condition : MonoBehaviour
{
    public ConditionID Id { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public string StartMessage { get; set; }

    public Action<Familiar> OnStart { get; set; }

    public Action<Familiar> OnBeforeTurn { get; set; }

    public Func<Familiar, bool> OnBeforeSelection { get; set; }

    public Func<Familiar, bool> OnBeforeAttack { get; set; }

    public Action<Familiar> OnAfterAttack { get; set; }

    public Action<Familiar> OnAfterTurn { get; set; }
}
