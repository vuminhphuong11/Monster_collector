using DG.Tweening;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum BattleState { START, ACTIONSELECTION, MOVESELECTION, RUNINGTURN, BUSY, PARTYSCREEN, ABOUTTOUSE, MOVETOFORGET, BATTLEOVER, BAG }
public enum BattleAction { Move, SwitchMonster, UseItem, Run, Capture }

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] Image playerImage;
    [SerializeField] Image bossImage;
    [SerializeField] Text vesusText;
    [SerializeField] GameObject monsterBookSprite;
    [SerializeField] MoveSelectionUI moveSelectionUI;
    [SerializeField] InventoryUI inventoryUI;
    [SerializeField] BossRewardUI rewardUI;

    Inventory inventory;
    bool playerSwitchedAfterFaint = false;
    bool enemySwitchedAfterFaint = false;
    bool hasEngaged = false; // Biến đánh dấu đã giao tranh hay chưa

    public event Action<bool> OnBattleOver;

    BattleState state;
    BattleState? preState;
    int currentAction;
    int currentMove;
    int currentMember; // dungf cho party screen
    MonsterParty playerParty;
    MonsterParty bossParty;
    Monster WildMonster;
    bool isBossBattle = false;
    PlayerController player;
    BossController boss;
    MoveBase moveToLearn;

    public void StartBattle(MonsterParty playerParty, Monster WildMonster)
    {
        isBossBattle = false;
        this.playerParty = playerParty;
        this.WildMonster = WildMonster;
        player = playerParty.GetComponent<PlayerController>();
        StartCoroutine(SetupBattle());
    }

    public void StartBossBattle(MonsterParty playerParty, MonsterParty bossParty)
    {
        this.playerParty = playerParty;
        this.bossParty = bossParty;
        isBossBattle = true;
        player = playerParty.GetComponent<PlayerController>();
        boss = bossParty.GetComponent<BossController>();
        StartCoroutine(SetupBattle());
    }

    public IEnumerator SetupBattle()
    {
        hasEngaged = false;
        playerUnit.Clear();
        enemyUnit.Clear();
        inventory = player.GetComponent<Inventory>();
        var firstHealthyMonster = playerParty.GetHealthyMonster();

        if (firstHealthyMonster == null)
        {
            // Nếu không có ai còn sống
            dialogBox.EnableDialogText(true);
            dialogBox.EnableActionSelector(false);
            dialogBox.EnableMoveSelector(false);

            yield return dialogBox.TypeDialog("You have no healthy Pokemon!");
            yield return new WaitForSeconds(1f);
            yield return dialogBox.TypeDialog("You blacked out!");
            yield return new WaitForSeconds(1f);

            // Kết thúc trận đấu (Thua)
            BattleOver(false);
            yield break; // Dừng hàm tại đây, không chạy setup bên dưới nữa
        }
        if (!isBossBattle)
        {
            // wild monster
            playerUnit.Setup(playerParty.GetHealthyMonster());
            enemyUnit.Setup(WildMonster);
            dialogBox.SetMoveNames(playerUnit.Monster.Moves);
            yield return dialogBox.TypeDialog("A wild " + enemyUnit.Monster.Base.Name + " appeared!");
        }
        else
        {
            // boss battle
            playerUnit.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(false);

            playerImage.gameObject.SetActive(true);
            bossImage.gameObject.SetActive(true);
            vesusText.gameObject.SetActive(true);

            playerImage.sprite = player.Sprite;
            bossImage.sprite = boss.Sprite;
            yield return dialogBox.TypeDialog($"{boss.Name} want to battle!");
            yield return new WaitForSeconds(1f);

            vesusText.gameObject.SetActive(false);
            bossImage.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(true);
            var enemyMonser = bossParty.GetHealthyMonster();
            enemyUnit.Setup(enemyMonser);
            yield return dialogBox.TypeDialog($"{boss.Name} send out {enemyMonser.Base.Name}!");

            playerImage.gameObject.SetActive(false);
            playerUnit.gameObject.SetActive(true);
            var playerMonster = playerParty.GetHealthyMonster();
            playerUnit.Setup(playerMonster);
            yield return dialogBox.TypeDialog($"Go {playerMonster.Base.Name}!");
            dialogBox.SetMoveNames(playerUnit.Monster.Moves);
        }

        partyScreen.Init();
        ActionSelection();
    }

    void BattleOver(bool won)
    {
        state = BattleState.BATTLEOVER;
        playerParty.Monsters.ForEach(p => p.OnBattleOver());

        if (won && isBossBattle)
        {
            StartCoroutine(ShowBossRewards());
        }
        else
        {
            OnBattleOver(won);
        }
    }

    IEnumerator ShowBossRewards()
    {
        yield return dialogBox.TypeDialog("You defeated the Boss!");
        rewardUI.ShowRewards();

        bool rewardFinished = false;
        rewardUI.OnFinished += () => rewardFinished = true;

        while (!rewardFinished)
        {
            rewardUI.HandleUpdate();
            yield return null;
        }

        OnBattleOver(true);
    }

    void ActionSelection()
    {
        state = BattleState.ACTIONSELECTION;
        dialogBox.SetDialog("Choose an action:");
        dialogBox.EnableActionSelector(true);
        dialogBox.SetRunEnabled(!hasEngaged);
        dialogBox.UpdateActionSelection(currentAction);
    }

    void OpenPartyScreen()
    {
        preState = state;
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

    IEnumerator ChooseMoveToForget(Monster monster, MoveBase newMov)
    {
        state = BattleState.BUSY;
        yield return dialogBox.TypeDialog($"Choose a Move you wana forget!");
        moveSelectionUI.gameObject.SetActive(true);
        moveSelectionUI.SetMoveData(monster.Moves.Select(x => x.Base).ToList(), newMov);
        moveToLearn = newMov;
        state = BattleState.MOVETOFORGET;
    }

    IEnumerator RunTurns(BattleAction playerAction)
    {
        state = BattleState.RUNINGTURN;
        if (playerAction == BattleAction.Move)
        {
            hasEngaged = true;
            playerUnit.Monster.CurrentMove = playerUnit.Monster.Moves[currentMove];
            enemyUnit.Monster.CurrentMove = enemyUnit.Monster.GetRandomMove();

            bool playerGoesFirst = true;
            if (playerSwitchedAfterFaint)
            {
                playerGoesFirst = true;
                playerSwitchedAfterFaint = false;
            }
            else if (enemySwitchedAfterFaint)
            {
                playerGoesFirst = false;
                enemySwitchedAfterFaint = false;
            }
            else
            {
                playerGoesFirst = playerUnit.Monster.Speed >= enemyUnit.Monster.Speed;
            }

            var firstUnit = (playerGoesFirst) ? playerUnit : enemyUnit;
            var secondUnit = (playerGoesFirst) ? enemyUnit : playerUnit;
            var secondMonster = secondUnit.Monster;

            // First turn
            yield return RunMove(firstUnit, secondUnit, firstUnit.Monster.CurrentMove);
            yield return RunAfterTurn(firstUnit);
            if (state == BattleState.BATTLEOVER) yield break;

            if (secondMonster.HP > 0)
            {
                // Second Turn
                yield return RunMove(secondUnit, firstUnit, secondUnit.Monster.CurrentMove);
                yield return RunAfterTurn(secondUnit);
                if (state == BattleState.BATTLEOVER) yield break;
            }
        }
        else
        {
            if (playerAction == BattleAction.SwitchMonster)
            {
                var selectedMonster = playerParty.Monsters[currentMember];
                state = BattleState.BUSY;
                yield return SwitchMonster(selectedMonster);
            }
            else if (playerAction == BattleAction.Capture)
            {
                dialogBox.EnableActionSelector(false);
                yield return OpenMonsterbook();
            }
            else if (playerAction == BattleAction.UseItem)
            {
                // Item effect handled in UseItemInBattle
            }
            else if (playerAction == BattleAction.Run)
            {
                yield return StartCoroutine(TryToEscape());
            }

            if (state != BattleState.BATTLEOVER)
            {
                var enemyMove = enemyUnit.Monster.GetRandomMove();
                yield return RunMove(enemyUnit, playerUnit, enemyMove);
                yield return RunAfterTurn(enemyUnit);
            }
        }

        if (state != BattleState.BATTLEOVER)
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
            yield return sourceUnit.Hub.UpdateHP();
            if (sourceUnit.Monster.HP <= 0)
            {
                yield return HandleMonsterFainted(sourceUnit);
            }
            yield break;
        }
        yield return ShowStatusChanges(sourceUnit.Monster);
        move.PP--;

        yield return dialogBox.TypeDialog(sourceUnit.Monster.Base.Name + " used " + move.Base.Name + "!");
        if (CheckIfMoveHits(move, sourceUnit.Monster, targetUnit.Monster))
        {
            sourceUnit.PlayAttackAnimation();
            yield return new WaitForSeconds(1f);
            targetUnit.PlayHitAnimation();

            if (move.Base.Category == MoveCategory.Status)
            {
                yield return RunMoveEffects(move.Base.Effects, sourceUnit, targetUnit, move.Base.Target);
            }
            else
            {
                var damageDetails = targetUnit.Monster.TakeDamage(move, sourceUnit.Monster);
                yield return targetUnit.Hub.UpdateHP();
                yield return ShowDamageDetails(damageDetails);
            }

            // Xử lý Secondary Effects
            if (move.Base.Secondaries != null && move.Base.Secondaries.Count > 0 && targetUnit.Monster.HP > 0)
            {
                foreach (var secondary in move.Base.Secondaries)
                {
                    var rnd = UnityEngine.Random.Range(1, 101);
                    if (rnd <= secondary.Chance)
                    {
                        yield return RunMoveEffects(secondary, sourceUnit, targetUnit, secondary.Target);
                    }
                }
            }

            if (targetUnit.Monster.HP <= 0)
            {
                yield return HandleMonsterFainted(targetUnit);
            }
        }
        else
        {
            yield return dialogBox.TypeDialog(sourceUnit.Monster.Base.Name + "'s Attack missed!");
        }

        bool hasMovesLeft = sourceUnit.Monster.CycleMove(move);

        // Nếu là lượt của người chơi VÀ không còn chiêu nào dùng được
        if (sourceUnit.IsPlayerUnit && !hasMovesLeft)
        {
            yield return dialogBox.TypeDialog($"{sourceUnit.Monster.Base.Name} has no moves left!");
            yield return new WaitForSeconds(1f);
            yield return dialogBox.TypeDialog("You have no moves left to fight!");
            yield return new WaitForSeconds(1f);

            // Gọi hàm kết thúc trận đấu với kết quả THUA
            BattleOver(false);
            yield break;
        }

        // Cập nhật lại UI danh sách chiêu (đã được đảo bởi CycleMove)
        if (sourceUnit.IsPlayerUnit)
        {
            dialogBox.SetMoveNames(sourceUnit.Monster.Moves);
        }
    }

    // === ĐÃ SỬA: Giảm delay và xử lý hiển thị ===
    IEnumerator RunMoveEffects(MoveEffect effects, BattleUnit sourceUnit, BattleUnit targetUnit, MoveTarget moveTarget)
    {
        if (effects.Boosts != null && effects.Boosts.Count > 0)
        {
            if (moveTarget == MoveTarget.Self)
            {
                sourceUnit.Monster.ApplyBoosts(effects.Boosts);
                sourceUnit.Hub.UpdateStatBoosts();
            }
            else
            {
                targetUnit.Monster.ApplyBoosts(effects.Boosts);
                targetUnit.Hub.UpdateStatBoosts();
            }

            // Hiện text ngay sau khi áp dụng
            yield return ShowStatusChanges(sourceUnit.Monster);
            yield return ShowStatusChanges(targetUnit.Monster);
        }

        // Status
        if (effects.Status != ConditionID.none)
        {
            targetUnit.Monster.SetStatus(effects.Status);
            // Hiện text thông báo bị dính status (nếu hàm SetStatus có enqueue)
            yield return ShowStatusChanges(targetUnit.Monster);
        }

        // VolatileStatus
        if (effects.VolatileStatus != ConditionID.none)
        {
            targetUnit.Monster.SetVolatileStatus(effects.VolatileStatus);
            yield return ShowStatusChanges(targetUnit.Monster);
        }

        // === THAY ĐỔI QUAN TRỌNG: Giảm thời gian chờ từ 2s xuống 0.5s ===
        if (effects.Boosts != null && effects.Boosts.Count > 0)
        {
            yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator RunAfterTurn(BattleUnit sourceUnit)
    {
        if (state == BattleState.BATTLEOVER) yield break;
        yield return new WaitUntil(() => state == BattleState.RUNINGTURN);
        sourceUnit.Monster.OnAfterTurn();
        yield return ShowStatusChanges(sourceUnit.Monster);
        yield return sourceUnit.Hub.UpdateHP();
        if (sourceUnit.Monster.HP <= 0)
        {
            yield return HandleMonsterFainted(sourceUnit);
            yield return new WaitUntil(() => state == BattleState.RUNINGTURN || state == BattleState.BATTLEOVER);
        }
    }

    bool CheckIfMoveHits(Move move, Monster source, Monster target)
    {
        if (move.Base.AlwaysHits)
            return true;

        float moveAcruracy = move.Base.Accuracy;
        int accuracy = source.StatBoosts[Stat.Accuracy];
        int evasion = target.StatBoosts[Stat.Evasion];
        float[] boostValues = { 1f, 4f / 3f, 5f / 3f, 2f, 7f / 3f, 8f / 3f, 3f };

        if (accuracy > 0)
            moveAcruracy *= boostValues[accuracy];
        else
            moveAcruracy /= boostValues[-accuracy];

        if (evasion > 0)
            moveAcruracy /= boostValues[evasion];
        else
            moveAcruracy *= boostValues[-evasion];

        return (UnityEngine.Random.Range(1, 101) <= moveAcruracy);
    }

    IEnumerator ShowStatusChanges(Monster monster)
    {
        while (monster.StatusChanges.Count > 0)
        {
            var message = monster.StatusChanges.Dequeue();
            yield return dialogBox.TypeDialog(message);
        }
    }

    IEnumerator HandleMonsterFainted(BattleUnit faintedUnit)
    {
        yield return dialogBox.TypeDialog(faintedUnit.Monster.Base.Name + " fainted!");
        faintedUnit.PlayFaintAnimation();
        yield return new WaitForSeconds(2f);

        if (!faintedUnit.IsPlayerUnit)
        {
            // gain exp
            int expYield = faintedUnit.Monster.Base.ExpYield;
            int enemyLevel = faintedUnit.Monster.Level;
            float bossBonus = (isBossBattle) ? 1.5f : 1;
            int expGain = Mathf.FloorToInt((expYield * enemyLevel * bossBonus) / 7);
            playerUnit.Monster.EXP += expGain;
            yield return dialogBox.TypeDialog($"{playerUnit.Monster.Base.Name} gained {expGain} exp");
            yield return playerUnit.Hub.SetExpSmooth();

            // check lvup
            while (playerUnit.Monster.CheckForLevelUp())
            {
                playerUnit.Hub.SetLevel();
                yield return dialogBox.TypeDialog($"{playerUnit.Monster.Base.Name} level up! Current level {playerUnit.Monster.Level}");

                var newMov = playerUnit.Monster.GetLearnableMoveAtCurrLevel();
                if (newMov != null)
                {
                    if (playerUnit.Monster.Moves.Count < MonsterBase.MaxNumOfMoves)
                    {
                        playerUnit.Monster.LearnMove(newMov);
                        yield return dialogBox.TypeDialog($"{playerUnit.Monster.Base.Name} learned {newMov.Base.Name} !");
                        dialogBox.SetMoveNames(playerUnit.Monster.Moves);
                    }
                    else
                    {
                        yield return dialogBox.TypeDialog($"{playerUnit.Monster.Base.Name} trying to learn {newMov.Base.Name} !");
                        yield return dialogBox.TypeDialog($"But it can not learn more than 9 moves !");
                        yield return ChooseMoveToForget(playerUnit.Monster, newMov.Base);
                        yield return new WaitUntil(() => state != BattleState.MOVETOFORGET);
                        yield return new WaitForSeconds(1f);
                    }
                }
                yield return playerUnit.Hub.SetExpSmooth(true);
            }
            yield return new WaitForSeconds(1f);
        }
        CheckForBattleOver(faintedUnit);
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
            if (!isBossBattle)
            {
                BattleOver(true);
            }
            else
            {
                var nextMonster = bossParty.GetHealthyMonster();
                if (nextMonster != null)
                {
                    StartCoroutine(SendNextBossMonster(nextMonster));
                }
                else
                {
                    BattleOver(true);
                }
            }
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
            HandlePartySelection();
        }
        else if (state == BattleState.BAG)
        {
            inventoryUI.HandleUpdate(() => {
                inventoryUI.gameObject.SetActive(false);
                ActionSelection();
            });
        }
        else if (state == BattleState.MOVETOFORGET)
        {
            Action<int> onMoveSelected = (moveIndex) =>
            {
                moveSelectionUI.gameObject.SetActive(false);
                if (moveIndex == MonsterBase.MaxNumOfMoves)
                {
                    StartCoroutine(dialogBox.TypeDialog($"{playerUnit.Monster.Base.Name} did not learn {moveToLearn.Name}"));
                }
                else
                {
                    var selectedMove = playerUnit.Monster.Moves[moveIndex].Base;
                    StartCoroutine(dialogBox.TypeDialog($"{playerUnit.Monster.Base.Name} forgot {selectedMove.Name} and learned {moveToLearn.Name}"));
                    playerUnit.Monster.Moves[moveIndex] = new Move(moveToLearn);
                }
                moveToLearn = null;
                state = BattleState.RUNINGTURN;
            };
            moveSelectionUI.HandleMoveSelection(onMoveSelected);
        }
    }

    void HandleActionSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
            currentAction++;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            currentAction--;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            currentAction += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            currentAction -= 2;

        currentAction = Mathf.Clamp(currentAction, 0, 3);
        dialogBox.UpdateActionSelection(currentAction);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (currentAction == 0) // Fight
                MoveSelection();
            else if (currentAction == 1) // Bag
                OpenInventory();
            else if (currentAction == 2) // Monster
                OpenPartyScreen();
            else if (currentAction == 3) // Run
                StartCoroutine(RunTurns(BattleAction.Run));
        }
    }

    void OpenInventory()
    {
        state = BattleState.BAG;
        inventoryUI.gameObject.SetActive(true);
        inventoryUI.OpenInventory(OnItemSelectedFromBag, () => {
            inventoryUI.gameObject.SetActive(false);
            ActionSelection();
        });
    }

    void OnItemSelectedFromBag(ItemBase item)
    {
        inventoryUI.gameObject.SetActive(false);
        if (item is CaptureItem)
        {
            StartCoroutine(RunTurns(BattleAction.Capture));
        }
        else
        {
            StartCoroutine(UseItemInBattle(item));
        }
    }

    IEnumerator UseItemInBattle(ItemBase item)
    {
        state = BattleState.BUSY;
        if (inventory == null) inventory = Inventory.GetInventory();

        bool canUse = item.Use(playerUnit.Monster);
        if (canUse)
        {
            inventory.RemoveItem(item);
            yield return dialogBox.TypeDialog($"Used {item.Name}!");
            yield return playerUnit.Hub.UpdateHP();
            StartCoroutine(RunTurns(BattleAction.UseItem));
        }
        else
        {
            yield return dialogBox.TypeDialog("It won't have any effect!");
            ActionSelection();
        }
    }

    void HandleMoveSelection()
    {
        int visibleMoveCount = Mathf.Min(playerUnit.Monster.Moves.Count, 4);

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentMove + 2 < visibleMoveCount) currentMove += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentMove >= 2) currentMove -= 2;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (currentMove % 2 == 0 && currentMove + 1 < visibleMoveCount) currentMove++;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (currentMove % 2 != 0) currentMove--;
        }

        currentMove = Mathf.Clamp(currentMove, 0, visibleMoveCount - 1);
        var selectedMove = playerUnit.Monster.Moves[currentMove];
        dialogBox.UpdateMoveSelection(currentMove, selectedMove);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (selectedMove.PP == 0) return;
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(RunTurns(BattleAction.Move));
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
        if (Input.GetKeyDown(KeyCode.RightArrow)) ++currentMember;
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) --currentMember;
        currentMember = Mathf.Clamp(currentMember, 0, playerParty.Monsters.Count - 1);
        partyScreen.UpdateMemberSelection(currentMember);

        if (Input.GetKeyDown(KeyCode.Z))
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
            if (preState == BattleState.ACTIONSELECTION)
            {
                StartCoroutine(RunTurns(BattleAction.SwitchMonster));
            }
            else
            {
                state = BattleState.BUSY;
                playerSwitchedAfterFaint = true;
                StartCoroutine(SwitchMonster(selectedMember));
            }
            preState = null;
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            if (playerUnit.Monster.HP > 0)
            {
                partyScreen.gameObject.SetActive(false);
                dialogBox.EnableDialogText(true);
                ActionSelection();
            }
            preState = null;
        }
    }

    IEnumerator SwitchMonster(Monster newMonster)
    {
        if (playerUnit.Monster.HP > 0)
        {
            yield return dialogBox.TypeDialog("Come back " + playerUnit.Monster.Base.Name + "!");
            playerUnit.PlayExitAnimation();
            yield return new WaitForSeconds(2f);
        }
        playerUnit.Setup(newMonster);
        dialogBox.SetMoveNames(newMonster.Moves);
        yield return dialogBox.TypeDialog("Go " + newMonster.Base.Name + "!");
        state = BattleState.RUNINGTURN;
    }

    IEnumerator SendNextBossMonster(Monster nextMonster)
    {
        state = BattleState.BUSY;
        enemyUnit.Setup(nextMonster);
        enemySwitchedAfterFaint = true;
        yield return dialogBox.TypeDialog($"{boss.Name} send out {nextMonster.Base.Name}");
        state = BattleState.RUNINGTURN;
    }

    IEnumerator OpenMonsterbook()
    {
        state = BattleState.BUSY;
        if (isBossBattle)
        {
            yield return dialogBox.TypeDialog($"you can't steel the enemy's monster!");
            state = BattleState.RUNINGTURN;
            yield break;
        }
        yield return dialogBox.TypeDialog($"{player.Name} used MonsterBook to catch Monster!");

        var monsterBookObj = Instantiate(monsterBookSprite, playerUnit.transform.position - new Vector3(5f, 0, 0), Quaternion.identity);
        var monsterBook = monsterBookObj.GetComponent<SpriteRenderer>();
        Vector3 catchPosition = enemyUnit.transform.position + new Vector3(-5.5f, -0.5f, 0);

        yield return monsterBook.transform.DOJump(catchPosition, 2f, 1, 1f).WaitForCompletion();
        yield return monsterBook.transform.DOMoveY(catchPosition.y + 2.5f, 0.5f).WaitForCompletion();
        yield return enemyUnit.PlayCaptureAnimation(monsterBook.transform.position);

        int shakeCount = TryToCatchMonster(enemyUnit.Monster);
        for (int i = 0; i < Mathf.Min(shakeCount, 3); i++)
        {
            yield return new WaitForSeconds(0.5f);
            yield return monsterBook.transform.DOPunchRotation(new Vector3(0, 0, 30f), 0.5f).WaitForCompletion();
        }

        if (shakeCount == 4)
        {
            yield return dialogBox.TypeDialog($"{enemyUnit.Monster.Base.Name} was caught!");
            yield return monsterBook.transform.DOMoveY(catchPosition.y - 1.5f, 0.5f).WaitForCompletion();
            yield return monsterBook.DOFade(0, 1.5f).WaitForCompletion();

            bool addedToParty = playerParty.AddMonster(enemyUnit.Monster);
            if (addedToParty)
            {
                yield return dialogBox.TypeDialog($"{enemyUnit.Monster.Base.Name} has been added to your party!");
            }
            else
            {
                yield return dialogBox.TypeDialog("The party is full!");
                yield return dialogBox.TypeDialog($"{enemyUnit.Monster.Base.Name} was transferred to the PC Box!");
            }

            Destroy(monsterBook.gameObject);
            BattleOver(true);
        }
        else
        {
            yield return new WaitForSeconds(1f);
            monsterBook.DOFade(0, 0.3f);
            yield return enemyUnit.PlayBreakOutAnimation(monsterBook.transform.position);
            if (shakeCount < 2)
            {
                yield return dialogBox.TypeDialog($"{enemyUnit.Monster.Base.Name} broke free!");
            }
            else
            {
                yield return dialogBox.TypeDialog($"Almost Caught it!");
            }
            Destroy(monsterBook.gameObject);
            state = BattleState.RUNINGTURN;
        }
    }

    int TryToCatchMonster(Monster monster)
    {
        float a = (3 * monster.MaxHP - 2 * monster.HP) * (float)monster.Base.CatchRate / (3 * monster.MaxHP);
        float hpPercent = (float)monster.HP / monster.MaxHP;
        if (hpPercent >= 0.5f)
        {
            a = a * 0.2f;
        }

        // Bảo vệ: Tránh lỗi a <= 0 gây Crash game
        if (a <= 0) a = 1;

        if (a >= 225) return 4; // Bắt dính luôn nếu tỉ lệ quá cao

        // Tính toán độ rung lắc (giữ nguyên logic gốc)
        float b = 1048560 / Mathf.Sqrt(Mathf.Sqrt(16711680 / a));
        int shakeCount = 0;
        while (shakeCount < 4)
        {
            if (UnityEngine.Random.Range(0, 65535) >= b) break;
            shakeCount++;
        }
        return shakeCount;
    }

    IEnumerator TryToEscape()
    {
        state = BattleState.BUSY;
        if (hasEngaged)
        {
            yield return dialogBox.TypeDialog("You cannot run once the battle has started!");
            state = BattleState.RUNINGTURN;
            yield break;
        }
        yield return dialogBox.TypeDialog("Ran away safely!");
        BattleOver(false);
    }
}