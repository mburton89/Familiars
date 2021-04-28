using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionsDB
{
    public static void Init()
    {
        foreach (var kvp in Conditions)
        {
            var conditionId = kvp.Key;
            var condition = kvp.Value;

            condition.Id = conditionId;
        }
    }

    public static Dictionary<ConditionID, Condition> Conditions { get; set; } = new Dictionary<ConditionID, Condition>()
    {
        {
            ConditionID.psn,
            new Condition()
            {
                Name = "Poison",
                StartMessage = "has been poisoned",
                OnAfterTurn = (Familiar familiar) =>
                {
                    familiar.UpdateHP(familiar.MaxHp / 8);
                    familiar.StatusChanges.Enqueue($"{familiar.Base.Name} was hurt due to poisoning.");
                }
            }
        },
        {
            ConditionID.brn,
            new Condition()
            {
                Name = "Burn",
                StartMessage = "has been burned",
                OnAfterTurn = (Familiar familiar) =>
                {
                    familiar.UpdateHP(familiar.MaxHp / 16);
                    familiar.StatusChanges.Enqueue($"{familiar.Base.Name} was hurt from its burn.");
                }
            }
            },
        {
            ConditionID.daz,
            new Condition()
            {
                Name = "Dazed",
                StartMessage = "has been dazed",
                OnStart = (Familiar familiar) =>
                {
                    // Be Dazed for a certain amount of turns (1-3)?
                    familiar.StatusTime = Random.Range(1, 4);
                    Debug.Log($"[ConditionsDB.cs/ConditionID.daz] Will be asleep for {familiar.StatusTime} turns");
                },
                OnBeforeSelection = (Familiar familiar) =>
                {
                    if (familiar.CanAct)
                        return true;
                    return false;
                },
                OnAfterAttack = (Familiar familiar) =>
                {
                    familiar.CanAct = false;
                },
                OnAfterTurn = (Familiar familiar) =>
                {
                    familiar.StatusTime--;

                    if (!familiar.CanAct)
                    {
                        familiar.CanAct = true;
                    }

                    if (familiar.StatusTime <= 0)
                    {
                        familiar.CureStatus();
                        familiar.StatusChanges.Enqueue($"{familiar.Base.Name} is no longer dazed!");
                    }
                }
            }
        },

        // Volatile Status Conditions

    };
}

public enum ConditionID
{
    none, psn, brn, imo, daz,
    confusion
}