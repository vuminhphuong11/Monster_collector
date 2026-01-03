using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionsDB 
{
    
    public static Dictionary<ConditionID, Condition> Conditions { get; set; } = new Dictionary<ConditionID, Condition>()
    {
        {
            ConditionID.psn, new Condition()
            {
                Name ="Poison",
                StartMessage="has been Poisoned",
                OnAfterTurn = (Monster monster) =>
                {
                    monster.UpdateHp(monster.MaxHP/8);
                    monster.StatusChanges.Enqueue($"{monster.Base.Name} hurt itself due to Poison");
                }
            }

        },
        {
            ConditionID.brn, new Condition()
            {
                Name ="Burn",
                StartMessage="has been Burn",
                OnAfterTurn = (Monster monster) =>
                {
                    monster.UpdateHp(monster.MaxHP/16);
                    monster.StatusChanges.Enqueue($"{monster.Base.Name} hurt by Burn!");
                }
            }

        },
        {
            ConditionID.par, new Condition()
            {
                Name ="Paralyzed",
                StartMessage="has been Paralyzed",
                OnBeforeMove = (Monster monster) =>
                {
                    if(Random.Range(1,6)==1)
                    {
                        monster.StatusChanges.Enqueue($"{monster.Base.Name}'s is Paralyzed and can't move!");
                        return false;
                    }
                    return true;
                }
            }

        },
        {
            ConditionID.frz, new Condition()
            {
                Name ="Freeze",
                StartMessage="has been Freezed",
                OnBeforeMove = (Monster monster) =>
                {
                    if(Random.Range(1,4)==1)
                    {
                        monster.CureStatus();
                        monster.StatusChanges.Enqueue($"{monster.Base.Name}'s is not Freezed anymore and can move!");
                        return true;
                    }
                    return false;
                }
            }

        },
        {
            ConditionID.slp, new Condition()
            {
                Name ="Sleep",
                StartMessage="has fallen Asleep!",
                OnStart = (Monster monster) =>
                {
                    // sleep for 1-3 turn
                    monster.StatusTime= Random.Range(1,4);
                    Debug.Log($"Will be asleep for {monster.StatusTime} moves!");

                },
                OnBeforeMove = (Monster monster) =>
                {
                    if (monster.StatusTime <= 0)
                    {
                        monster.CureStatus();
                        monster.StatusChanges.Enqueue($"{monster.Base.Name} woke up ");
                        return true;
                    }
                    monster.StatusTime--;
                    monster.StatusChanges.Enqueue($"{monster.Base.Name} is sleeping ");
                    return false;
                }
            }

        },
    };
}
public enum ConditionID
{
    none,psn,brn, slp, par,frz
}
