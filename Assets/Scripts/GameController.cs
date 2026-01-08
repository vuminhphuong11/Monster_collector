using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum GameState
{
    FreeRoam,
    Battle,
    Dialog,
    Cutscene
    
}
public class GameController : MonoBehaviour
{

    [SerializeField] PlayerController playerController;

    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera worldCamera;
    GameState state;
    public static GameController Instance {  get; private set; }
    private void Awake()
    {
        Instance = this;
        ConditionsDB.Init();
    }

    private void Start()
    {
        playerController.OnEncountered += StartBattle;
        battleSystem.OnBattleOver += EndBattle;
        playerController.OnEnterBossView += (Collider2D bossCollider) =>
        {
            var boss = bossCollider.GetComponentInParent<BossController>();
            if (boss != null)
            {
                state=GameState.Cutscene;
                StartCoroutine(boss.TriggerBossBattle(playerController));
            }
        };

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
        if (boss != null && won == true)
        {
            boss.BattleLost();
            boss = null;
        }
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
    BossController boss;
    public void StartBossBattle(BossController boss)
    {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);
        this.boss= boss;
        var playerParty = playerController.GetComponent<MonsterParty>();
        var bossParty =boss.GetComponent<MonsterParty>();
        battleSystem.StartBossBattle(playerParty, bossParty);

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
