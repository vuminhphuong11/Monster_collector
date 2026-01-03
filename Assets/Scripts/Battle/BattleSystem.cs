using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;
public enum BattleState { START, ACTIONSELECTION, MOVESELECTION,  PERFORMMOVE, BUSY,PARTYSCREEN,BATTLEOVER}
public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] PartyScreen partyScreen;

    public event Action<bool> OnBattleOver;
    BattleState state;
    int currentAction;
    int currentMove;
    int currentMember;// dungf cho party screen
    MonsterParty playerParty;
    Monster WildMonster;

    public  void StartBattle(MonsterParty playerParty , Monster WildMonster)
    {
        this.playerParty = playerParty;
        this.WildMonster = WildMonster;
        StartCoroutine( SetupBattle());
    }

    public IEnumerator SetupBattle()
    {
        playerUnit.Setup(playerParty.GetHealthyMonster());
        enemyUnit.Setup(WildMonster);


        partyScreen.Init();
        dialogBox.SetMoveNames(playerUnit.Monster.Moves);

        yield return dialogBox.TypeDialog("A wild " + enemyUnit.Monster.Base.Name + " appeared!");

        
        ChooseFirstTurn();
    }

    void ChooseFirstTurn()
    {
        if (playerUnit.Monster.Speed >= enemyUnit.Monster.Speed)
        {
            //Player goes first
            ActionSelection();
        }
        else
        {
            //Enemy goes first
            StartCoroutine( EnemyMove());
        }
    }

    void BattleOver(bool won)
    {
        state = BattleState.BATTLEOVER;
        playerParty.Monsters.ForEach(p => p.OnBattleOver());
        OnBattleOver(won);

    }
    void ActionSelection()
    {
        state = BattleState.ACTIONSELECTION;
        dialogBox.SetDialog("Choose an action:");
        dialogBox.EnableActionSelector(true);
    }

    void OpenPartyScreen()
    {
        state = BattleState.PARTYSCREEN;
        partyScreen.SetPartyData(playerParty.Monsters);
        partyScreen.gameObject.SetActive(true);
    }
    void MoveSelection()
    {
        currentMove = 0;
        state = BattleState.MOVESELECTION;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
    }

    IEnumerator PlayerMove()
    {
        state = BattleState.PERFORMMOVE;
        var move = playerUnit.Monster.Moves[currentMove];
        yield return RunMove(playerUnit, enemyUnit, move);

        //if the battle stat was not changed by RunMove then go to next step
        if (state == BattleState.PERFORMMOVE)
        {
            StartCoroutine(EnemyMove());
        }
        
       
    }

    IEnumerator EnemyMove()
    {
        state = BattleState.PERFORMMOVE;
        var move = enemyUnit.Monster.GetRandomMove();
        yield return RunMove(enemyUnit, playerUnit, move);

        //if the battle stat was not changed by RunMove then go to next step
        if (state == BattleState.PERFORMMOVE)
        {
            ActionSelection();
        }
    }

    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
    {

        bool canRunMove = sourceUnit.Monster.OnBeforeMove();
        if (!canRunMove) 
        { 
            yield return ShowStatusChanges(sourceUnit.Monster);
            yield break;
        }
        yield return ShowStatusChanges(sourceUnit.Monster);
        move.PP--;
        yield return dialogBox.TypeDialog(sourceUnit.Monster.Base.Name + " used " + move.Base.Name + "!");
        sourceUnit.PlayAttackAnimation();

        yield return new WaitForSeconds(1f);
        targetUnit.PlayHitAnimation();

        if (move.Base.Category == MoveCategory.Status)
        {
            // --- SỬA DÒNG NÀY: Truyền cả sourceUnit và targetUnit vào ---
            yield return RunMoveEffects(move, sourceUnit, targetUnit);
            // ------------------------------------------------------------
        }
        else
        {
            var damageDetails = targetUnit.Monster.TakeDamage(move, sourceUnit.Monster);
            yield return targetUnit.Hub.UpdateHP();
            yield return ShowDamageDetails(damageDetails);
        }

        // ... (phần code xử lý ngất giữ nguyên) ...
        if (targetUnit.Monster.HP <= 0)
        {
            yield return dialogBox.TypeDialog(targetUnit.Monster.Base.Name + " fainted!");
            targetUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);
            CheckForBattleOver(targetUnit);
        }
        sourceUnit.Monster.OnAfterTurn();
        yield return ShowStatusChanges(sourceUnit.Monster);
        yield return sourceUnit.Hub.UpdateHP();
        if (sourceUnit.Monster.HP <= 0)
        {
            yield return dialogBox.TypeDialog(sourceUnit.Monster.Base.Name + " fainted!");
            sourceUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);
            CheckForBattleOver(sourceUnit);
        }
    }
    // --- SỬA THAM SỐ ĐẦU VÀO: Monster -> BattleUnit ---
    // Trong file BattleSystem.cs

    IEnumerator RunMoveEffects(Move move, BattleUnit sourceUnit, BattleUnit targetUnit)
    {
        var effects = move.Base.Effects;

        // --- SỬA DÒNG NÀY ---
        // Thêm điều kiện: && effects.Boosts.Count > 0
        // Để đảm bảo chỉ hiện bảng khi thực sự có chỉ số thay đổi
        if (effects.Boosts != null && effects.Boosts.Count > 0)
        {
            if (move.Base.Target == MoveTarget.Self)
            {
                sourceUnit.Monster.ApplyBoosts(effects.Boosts);
                sourceUnit.Hub.UpdateStatBoosts();
            }
            else
            {
                targetUnit.Monster.ApplyBoosts(effects.Boosts);
                targetUnit.Hub.UpdateStatBoosts();
            }

            // Hiện thông báo Text
            yield return ShowStatusChanges(sourceUnit.Monster);
            yield return ShowStatusChanges(targetUnit.Monster);
        }

        // Xử lý hiệu ứng trạng thái (Độc, Đóng băng...)
        if (effects.Status != ConditionID.none)
        {
            targetUnit.Monster.SetStatus(effects.Status);
            // Không gọi ShowStatusChanges ở đây -> Bảng và Text sẽ không hiện ngay lúc này
        }

        // Chỉ delay nêú có hiện bảng chỉ số
        if (effects.Boosts != null && effects.Boosts.Count > 0)
        {
            yield return new WaitForSeconds(2f);
        }
    }


    IEnumerator ShowStatusChanges(Monster monster)
    {
        while (monster.StatusChanges.Count > 0)
        {
            var message = monster.StatusChanges.Dequeue();
            yield return dialogBox.TypeDialog(message);
        }
    }
    void CheckForBattleOver(BattleUnit faintedUnit)
    {
        if (faintedUnit.IsPlayerUnit)
        {
            var nextMonster = playerParty.GetHealthyMonster();
            if (nextMonster != null)
            {
                OpenPartyScreen();
            }
            else
                BattleOver(false);
        }
        else
        {
            BattleOver(true);
        }
    }

    IEnumerator ShowDamageDetails(DamageDetails damageDetails)
    {
        if (damageDetails.Critical > 1f)
            yield return dialogBox.TypeDialog("A critical hit!");
        if (damageDetails.TypeEffectiveness > 1f)
            yield return dialogBox.TypeDialog("It's super effective!");
        else if (damageDetails.TypeEffectiveness < 1f)
            yield return dialogBox.TypeDialog("It's not very effective...");
    }

    public void HandleUpdate()
    {
        if (state == BattleState.ACTIONSELECTION)
        {
            HandleActionSelection();
        }
        else if (state == BattleState.MOVESELECTION)
        {
            HandleMoveSelection();
        }
        else if (state == BattleState.PARTYSCREEN)
        {
            // Handle party screen input (not implemented in this snippet)
            HandlePartySelection();
        }

    }

    void HandleActionSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            currentAction++;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            currentAction--;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentAction += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentAction -= 2;
        }
        currentAction = Mathf.Clamp(currentAction, 0,3);

        dialogBox.UpdateActionSelection(currentAction);
        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (currentAction == 0)
            {
                //Fight
                MoveSelection();
            }
            else if (currentAction == 1)
            {
                //Bag
              
            }
            else if (currentAction == 2)
            {
                //Monster
                OpenPartyScreen();
               
            }
            else if (currentAction == 3)
            {
                //Run

            }
        }
    }

    void HandleMoveSelection()
    {
        int moveCount = playerUnit.Monster.Moves.Count;
        // Di chuyển XUỐNG (Cộng 2)
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentMove + 2 < moveCount)
                currentMove += 2;
        }
        // Di chuyển LÊN (Trừ 2)
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentMove >= 2)
                currentMove -= 2;
        }       // Di chuyển SANG PHẢI (Cộng 1)
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (currentMove % 2 == 0 && currentMove + 1 < moveCount)
                currentMove++;
        }// Di chuyển SANG TRÁI (Trừ 1)
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (currentMove % 2 != 0)
                currentMove--;
        }
        dialogBox.UpdateMoveSelection(currentMove, playerUnit.Monster.Moves[currentMove]);
        if(Input.GetKeyDown(KeyCode.Z))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine( PlayerMove());
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            ActionSelection();
        }
    }

    void HandlePartySelection()
    {
        // Not implemented in this snippet
        if(Input.GetKeyDown(KeyCode.RightArrow))
            ++currentMember;
        else if(Input.GetKeyDown(KeyCode.LeftArrow))
            --currentMember;
        else if(Input.GetKeyDown(KeyCode.DownArrow))
            currentMember +=2;
        else if(Input.GetKeyDown(KeyCode.UpArrow))
            currentMember -=2;
        currentMember =Mathf.Clamp(currentMember,0, playerParty.Monsters.Count -1);
        partyScreen.UpdateMemberSelection(currentMember);
        if(Input.GetKeyDown(KeyCode.Z))
        {
            var selectedMember = playerParty.Monsters[currentMember];
            if (selectedMember.HP <= 0)
            {
                partyScreen.SetMessageText("You can't send out a fainted monster!");
                return;
            }
            if (selectedMember == playerUnit.Monster)
            {
                partyScreen.SetMessageText("This monster is already in battle!");
                return;
            }
            partyScreen.gameObject.SetActive(false);
            state = BattleState.BUSY;
            StartCoroutine(SwitchMonster(selectedMember));

        }
        else if(Input.GetKeyDown(KeyCode.X))
        {
            partyScreen.gameObject.SetActive(false);
            dialogBox.EnableDialogText(true);
            ActionSelection();
        }
    }
    IEnumerator SwitchMonster(Monster newMonster)
    {
        bool currentMonsterFainted = true;
        if (playerUnit.Monster.HP > 0)
        {
            currentMonsterFainted = false;
            yield return dialogBox.TypeDialog("Come back " + playerUnit.Monster.Base.Name + "!");
            playerUnit.PlayExitAnimation();
            yield return new WaitForSeconds(2f);
        }
        playerUnit.Setup(newMonster);
        dialogBox.SetMoveNames(newMonster.Moves);
        yield return dialogBox.TypeDialog("Go " + newMonster.Base.Name + "!");
        if (currentMonsterFainted)
            ChooseFirstTurn();
        else
        {
            StartCoroutine(EnemyMove());
        }
    }

}


