using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum BattleState { START, PLAYERACTION, PLAYERMOVE,  ENEMYMOVE, BUSY}
public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleHub playerHub;
    [SerializeField] BattleHub enemyHub;
    [SerializeField] BattleDialogBox dialogBox;

    public event Action<bool> OnBattleOver;
    BattleState state;
    int currentAction;
    int currentMove;
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
        playerHub.SetData(playerUnit.Monster);
        enemyHub.SetData(enemyUnit.Monster);
        dialogBox.SetMoveNames(playerUnit.Monster.Moves);

        yield return dialogBox.TypeDialog("A wild " + enemyUnit.Monster.Base.Name + " appeared!");

        
        PlayerAction();
    }

    void PlayerAction()
    {
        state = BattleState.PLAYERACTION;
        StartCoroutine( dialogBox.TypeDialog("Choose an action:"));
        dialogBox.EnableActionSelector(true);
    }

    void PlayerMove()
    {
        state = BattleState.PLAYERMOVE;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
    }

    IEnumerator PerformPlayerMove()
    {
        state = BattleState.BUSY;
        var move = playerUnit.Monster.Moves[currentMove];

        move.PP--;
        yield return dialogBox.TypeDialog(playerUnit.Monster.Base.Name + " used " + move.Base.Name + "!");
        playerUnit.PlayAttackAnimation();

        yield return new WaitForSeconds(1f);
        enemyUnit.PlayHitAnimation();
        var damageDetails =enemyUnit.Monster.TakeDamage(move, playerUnit.Monster);
        yield return enemyHub.UpdateHP();
        yield return ShowDamageDetails(damageDetails);
        if (damageDetails.Fainted)
        {
            yield return dialogBox.TypeDialog(enemyUnit.Monster.Base.Name + " fainted!");

           
            enemyUnit.PlayFaintAnimation();

            yield return new WaitForSeconds(2f);
            OnBattleOver(true);
        }
        else
        {
            StartCoroutine(EnemyMove());
        }
    }

    IEnumerator EnemyMove()
    {
        state = BattleState.ENEMYMOVE;
        var move = enemyUnit.Monster.GetRandomMove();
        move.PP--;
        yield return dialogBox.TypeDialog(enemyUnit.Monster.Base.Name + " used " + move.Base.Name + "!");
        enemyUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(1f);
        playerUnit.PlayHitAnimation();  

        var damageDetails = playerUnit.Monster.TakeDamage(move, enemyUnit.Monster);
        yield return playerHub.UpdateHP();
        yield return ShowDamageDetails(damageDetails);
        if (damageDetails.Fainted)
        {
            yield return dialogBox.TypeDialog(playerUnit.Monster.Base.Name + " fainted!");

            
            playerUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);

            var nextMonster=playerParty.GetHealthyMonster();
            if (nextMonster != null)
            {
                playerUnit.Setup(nextMonster);
                playerHub.SetData(nextMonster);
                dialogBox.SetMoveNames(playerUnit.Monster.Moves);
                yield return dialogBox.TypeDialog("Go " +nextMonster.Base.Name + "!");
                PlayerAction();
            }
            else
                OnBattleOver(false);
        }
        else
        {
            PlayerAction();
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
        if (state == BattleState.PLAYERACTION)
        {
            HandleActionSelection();
        }
        else if (state == BattleState.PLAYERMOVE)
        {
            HandleMoveSelection();
        }

    }

    void HandleActionSelection()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentAction < 1)
                ++currentAction;
            Debug.Log("Đã nhấn xuống, vị trí hiện tại: " + currentAction); // Thêm dòng này
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentAction > 0)
                --currentAction;
            Debug.Log("Đã nhấn len, vị trí hiện tại: " + currentAction); // Thêm dòng này
        }
        dialogBox.UpdateActionSelection(currentAction);
        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (currentAction == 0)
            {
                //Fight
                PlayerMove();
            }
            else if (currentAction == 1)
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
            StartCoroutine( PerformPlayerMove());
        }
    }
}

