using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCController : MonoBehaviour,Interactable
{
    [SerializeField] Dialog dialog;
    [SerializeField] List<Vector2> movementPattenr;
    [SerializeField] float timeBetWeenPattern;
    NPCState state;
    float idleTimer=0f;
    int currentMovePattern=0;// tao bien nay de nho index cuar movement pattenr;
    Character character;

    private void Awake()
    {
        character = GetComponent<Character>();
    }
    public void Interact(Transform initiator)
    {
        //Debug.Log("Interacting with npc");
        if (state == NPCState.Idle)
        {
            state= NPCState.Dialog;
            character.LookTorwads(initiator.position);
            StartCoroutine(DiaLogManager.Instance.ShowDiaLog(dialog, () =>
            {
                idleTimer = 0f;
                state = NPCState.Idle;
            }));
        }
   
    }
    private void Update()
    {
        
        if (state == NPCState.Idle)
        {
            idleTimer += Time.deltaTime;
            if (idleTimer > timeBetWeenPattern)
            {
                idleTimer = 0f;
                if(movementPattenr.Count > 0)
                {
                    StartCoroutine(Walk());
                }
            }
        }
        character.HandleUpdate();
    }
    IEnumerator Walk()
    {
        state = NPCState.Walking;
        var oldPos =transform.position;
        yield return character.Move(movementPattenr[currentMovePattern]);
        if (transform.position != oldPos)
        {
            currentMovePattern = (currentMovePattern + 1) % (movementPattenr.Count);
        }
        state = NPCState.Idle;
    }
}
public enum NPCState { Idle,Walking,Dialog}
