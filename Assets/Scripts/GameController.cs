using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum GameState
{
    FreeRoam,
    Battle,
    Dialog
}
public class GameController : MonoBehaviour
{

    [SerializeField] PlayerController playerController;

    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera worldCamera;
    GameState state;
    private void Awake()
    {
        ConditionsDB.Init();
    }

    private void Start()
    {
        playerController.OnEncountered += StartBattle;
        battleSystem.OnBattleOver += EndBattle;

        DiaLogManager.Instance.OnShowDialog += () =>
        {
            state = GameState.Dialog;
        };
        DiaLogManager.Instance.OnCloseDialog += () =>
        {
            if (state == GameState.Dialog)
            {
                state = GameState.FreeRoam;
            }
                
        };
    }

    void EndBattle(bool won)
    {
        state = GameState.FreeRoam;
        battleSystem.gameObject.SetActive(false);
        worldCamera.gameObject.SetActive(true);
    }

    void StartBattle()
    {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);

        var playerParty = playerController.GetComponent<MonsterParty>();
        var wildMonster = FindObjectOfType<MapArea>().GetComponent<MapArea>().GetRandomWildMonster();
        battleSystem.StartBattle(playerParty,wildMonster);

    }

    public void Update()
    {
        if (state == GameState.FreeRoam)
        {
            // Xử lý logic khi ở trạng thái FreeRoam
            playerController.HandleUpdate();
        }
        else if (state == GameState.Battle)
        {
            // Xử lý logic khi ở trạng thái Battle
            battleSystem.HandleUpdate();
        }
        else if (state == GameState.Dialog)
        {
            // su ly logic khi ma dang giao tiep vs npc
            DiaLogManager.Instance.HandleUpdate();
        }
    }
}
