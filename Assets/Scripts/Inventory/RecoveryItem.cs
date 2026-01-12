using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Items/Create new recoveryItem")]
public class RecoveryItem : ItemBase
{
    [Header("HP")]
    [SerializeField] int hpAmount;
    [SerializeField] bool restoreMaxHp;
    [Header("pp")]
    [SerializeField] bool restoreMaxPP;
    [SerializeField] int ppAmount;
    [Header("StatusCondition")]
    [SerializeField] ConditionID status;
    [SerializeField] bool recoverAllStatus;
}
