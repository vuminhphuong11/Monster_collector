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
    // item them mau cho monster
    public override bool Use(Monster monster)
    {
        bool isUsed = false;

        // 1. XỬ LÝ HỒI MÁU (HP)
        if (restoreMaxHp || hpAmount > 0)
        {
            if (monster.HP < monster.MaxHP)
            {
                if (restoreMaxHp)
                    monster.IncreaseHP(monster.MaxHP);
                else
                    monster.IncreaseHP(hpAmount);

                isUsed = true;
            }
        }

        // 2. XỬ LÝ TRẠNG THÁI (STATUS & VOLATILE STATUS)
        // Nếu item chữa tất cả (Full Heal) hoặc chữa đúng bệnh đang mắc
        if (recoverAllStatus || status != ConditionID.none)
        {
            // --- Logic cũ: Chữa Status chính (Psn, Brn, Slp...) ---
            if (monster.Status != null)
            {
                if (recoverAllStatus || monster.Status.id == status)
                {
                    monster.CureStatus();
                    isUsed = true;
                }
            }

            // --- THÊM LOGIC MỚI: Chữa Volatile Status (Confusion) ---
            if (monster.VolatileStatus != null)
            {
                if (recoverAllStatus || monster.VolatileStatus.id == status)
                {
                    monster.CureVolatileStatus();
                    isUsed = true;
                }
            }
        }

        // 3. XỬ LÝ HỒI PP
        if (restoreMaxPP || ppAmount > 0)
        {
            if (monster.NeedsPPHeal())
            {
                if (restoreMaxPP)
                    monster.HealAllPP();
                else
                    monster.RestorePP(ppAmount);

                isUsed = true;
            }
        }

        return isUsed;
    }

}
