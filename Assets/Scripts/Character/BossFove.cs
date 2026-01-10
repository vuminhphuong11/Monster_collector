using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossFove : MonoBehaviour,IPlayerTriggerable
{
    public void OnPlayerTriggered(PlayerController player)
    {
        GameController.Instance.OnEnterBossesView(GetComponentInParent<BossController>());
    }
}
