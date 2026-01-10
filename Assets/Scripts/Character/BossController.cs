using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossController : MonoBehaviour, Interactable
{
    [SerializeField] Dialog dialog;
    [SerializeField] Dialog dialogAfterBattle;
    [SerializeField] GameObject exclamation;
    [SerializeField] GameObject fov;
    [SerializeField] Sprite sprite;
    [SerializeField] string name;
    bool battleLost=false;
    Character character;
    private void Awake()
    {
        character = GetComponent<Character>();
    }
    private void Start()
    {
        SetFovRotation(character.Animator.DefaultDirection);
    }
    public void Interact(Transform initiator)
    {
        character.LookTorwads(initiator.position);
        if (!battleLost)
        {
            StartCoroutine(DiaLogManager.Instance.ShowDiaLog(dialog, () =>
            {
                GameController.Instance.StartBossBattle(this);
            }));
        }
        else
        {
            StartCoroutine(DiaLogManager.Instance.ShowDiaLog(dialogAfterBattle));
        }
    }
    public IEnumerator TriggerBossBattle(PlayerController player)
    {
        //show exclamation
        exclamation.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        exclamation.SetActive(false);
        var diff =player.transform.position - transform.position;
        var moveVec = diff - diff.normalized;
        //walk towards the player
        moveVec= new Vector2(Mathf.Round(moveVec.x),Mathf.Round(moveVec.y));
        yield return character.Move(moveVec);

        //show dialog
        StartCoroutine(DiaLogManager.Instance.ShowDiaLog(dialog, () =>
        {
            GameController.Instance.StartBossBattle(this);
        }));
    }
    public IEnumerator ResumeBattleAfterCooldown(float cooldownTime = 10f)
    {
        // 1. Tắt vùng phát hiện người chơi (FOV) để không bị bắt trận lại ngay
        if (fov != null)
        {
            fov.SetActive(false);
        }
        yield return new WaitForSeconds(cooldownTime);
        // 3. Bật lại FOV (nếu Boss chưa bị đánh bại)
        if (!battleLost && fov != null)
        {
            fov.SetActive(true);
        }
    }
    public void BattleLost()
    {
        battleLost = true;
        fov.gameObject.SetActive(false);
    }
    public void SetFovRotation(FacingDirection dir)
    {
        float angle =0f;
        if (dir == FacingDirection.down)
        {
            angle = 0f;
        }

        else if (dir == FacingDirection.right)
        {
            angle = 90f;
        }
        else if (dir == FacingDirection.left)
        {
            angle = 270f;
        }
        else if(dir == FacingDirection.up)
        {
            angle = 180f;
        }
        fov.transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    public string Name
    {
        get => name;
    }
    public Sprite Sprite
    {
        get => sprite;
    }
}
