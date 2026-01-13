using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum GameState
{
    FreeRoam,
    Battle,
    Dialog,
    Cutscene,
    Menu,
    Storage,
    Bag

}
public class GameController : MonoBehaviour
{

    [SerializeField] PlayerController playerController;
    [SerializeField] StorageUI storageUI;
    [SerializeField] InventoryUI inventoryUI;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera worldCamera;
    GameState state;
    public static GameController Instance {  get; private set; }
    MenuController menuController;
    private void Awake()
    {
        Instance = this;
        menuController = GetComponent<MenuController>();
        // add this two line when the game done
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
        MonsterDB.Init();
        MoveDB.Init();
        ConditionsDB.Init();
        ItemDB.Init();
    }
    public SceneDetail CurrentScene {  get; set; }
    public  SceneDetail PreScene { get; set; }

    private void Start()
    {
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
        menuController.OnBack += () =>
        {
            state = GameState.FreeRoam;
        };
        menuController.onMenuSelected += OnMenuSelected;
    }
    public void PauseGame(bool pause)
    {
        if (pause)
        {
            state = GameState.Cutscene; // Chuyển sang Cutscene để chặn HandleUpdate của Player
        }
        else
        {
            state = GameState.FreeRoam;
        }
    }

    void EndBattle(bool won)
    {
        if (boss != null)
        {
            if (won)
            {
                // TRƯỜNG HỢP 1: Thắng Boss
                boss.BattleLost(); // Boss nhận thua và biến mất/ngừng tương tác
            }
            else
            {
                // TRƯỜNG HỢP 2: Bỏ chạy hoặc Thua (nhưng chưa Game Over)
                // Gọi hàm cooldown để Boss "nhắm mắt" trong 2 giây, cho phép người chơi chạy
                StartCoroutine(boss.ResumeBattleAfterCooldown(10f));
            }

            // Reset biến boss hiện tại về null vì trận đấu đã kết thúc
            boss = null;
        }
        state = GameState.FreeRoam;
        battleSystem.gameObject.SetActive(false);
        worldCamera.gameObject.SetActive(true);
    }

    public void StartBattle()
    {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);
        var playerParty = playerController.GetComponent<MonsterParty>();
        var wildMonster = CurrentScene.GetComponent<MapArea>().GetRandomWildMonster();
        var wildMonstercopy = new Monster(wildMonster.Base, wildMonster.Level);

        battleSystem.StartBattle(playerParty,wildMonstercopy);

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
    public void OnEnterBossesView(BossController boss)
    {
        state = GameState.Cutscene;
        StartCoroutine(boss.TriggerBossBattle(playerController));
    }

    public void Update()
    {
        if (state == GameState.FreeRoam)
        {
            // Xử lý logic khi ở trạng thái FreeRoam
            playerController.HandleUpdate();
            if(Input.GetKeyDown(KeyCode.Return))
            {
                menuController.OpenMenu();
                state=GameState.Menu;
            }
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
        else if(state== GameState.Menu)
        {
            menuController.HandleUpdate();
        }
        else if (state == GameState.Storage)
        {
            storageUI.HandleUpdate();
        }
        else if(state== GameState.Bag)
        {
            Action onBack = () =>
            {
                inventoryUI.gameObject.SetActive(false);
                state = GameState.FreeRoam;
            };
            inventoryUI.HandleUpdate(onBack);
        } 
    }
    public void SetCurrentScene(SceneDetail currScene)
    {
        PreScene=CurrentScene;
        CurrentScene = currScene;
    }
    void OnMenuSelected(int selectedItem)
    {
        if (selectedItem == 0)
        {
            storageUI.gameObject.SetActive(true);
            state = GameState.Storage; // Chuyển state để chặn Player di chuyển
            var playerParty = playerController.GetComponent<MonsterParty>();
            storageUI.Init(playerParty);
            return;

        }
        else if(selectedItem == 1)
        {
            //bag
            inventoryUI.gameObject.SetActive(true);
            state=GameState.Bag;
            return;
        }
        else if(selectedItem == 2)
        {
            //Save
            SavingSystem.i.Save("saveSlot1");
        }
        else if(selectedItem == 3)
        {
            //Load
            SavingSystem.i.Load("saveSlot1");
        }
        state= GameState.FreeRoam;

    }
    public void CloseStorage()
    {
        // Tắt UI
        storageUI.gameObject.SetActive(false);

        // Trả lại quyền điều khiển cho nhân vật
        state = GameState.FreeRoam;
    }
}
