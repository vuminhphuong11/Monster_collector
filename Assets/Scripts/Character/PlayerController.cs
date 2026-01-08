using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    


    public event Action OnEncountered;



    private Vector2 input;

    private Character character;
    private void Awake()
    {

        character = GetComponent<Character>();
    }
    public void HandleUpdate()
    {
        if (!character.IsMoving)
        {
            input.x=Input.GetAxisRaw("Horizontal");
            input.y=Input.GetAxisRaw("Vertical");
            if(input.x != 0) input.y = 0; // Prevent diagonal movement
            if (input != Vector2.zero)
            {
                StartCoroutine(character.Move(input,CheckForEncounters));
            }
        }
        character.HandleUpdate();

        if (Input.GetKeyDown(KeyCode.Z))
        {
            Interact();
        }
    }
    void Interact()
    {
        var facingDir = new Vector3(character.Animator.MoveX,character.Animator.MoveY);
        var interactPos = transform.position + facingDir;
        //Debug.DrawLine(transform.position, interactPos,Color.black,0.5f);
        var collider =Physics2D.OverlapCircle(interactPos,0.4f,GameLayer.i.InteractableLayer);
        if (collider != null)
        {
            collider.GetComponent<Interactable>()?.Interact(transform);
        }
    }


    private void CheckForEncounters()
    {
        if(Physics2D.OverlapCircle(transform.position, 0.2f, GameLayer.i.GrassLayer) != null)
        {
            if(UnityEngine.Random.Range(1,101) <= 10) // 10% chance
            {
                Debug.Log("A wild Pokémon appeared!");
                character.Animator.IsMoving = false;
                OnEncountered();

            }
        }
    }
}
