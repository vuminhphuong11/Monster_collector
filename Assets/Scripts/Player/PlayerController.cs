using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 2f;
    public LayerMask solidObjectsLayer;
    public LayerMask grassLayer;

    public event Action OnEncountered;
    private bool isMoving;


    private Vector2 input;
    private Animator animator;
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    public void HandleUpdate()
    {
        if (!isMoving)
        {
            input.x=Input.GetAxisRaw("Horizontal");
            input.y=Input.GetAxisRaw("Vertical");
            if(input.x != 0) input.y = 0; // Prevent diagonal movement
            if (input != Vector2.zero)
            {
                animator.SetFloat("moveX", input.x);
                animator.SetFloat("moveY", input.y);
                var targetPos = transform.position;
                targetPos.x += input.x;
                targetPos.y += input.y;
                if (IsWalkable(targetPos))
                    StartCoroutine(Move(targetPos));
            }
        }
        animator.SetBool("isMoving", isMoving);
    }
    IEnumerator Move(Vector2 targetPos)
    {

        isMoving = true;
        while((targetPos - (Vector2)transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPos;
        isMoving = false;
        CheckForEncounters();
    }
    private bool IsWalkable(Vector2 targetPos)
    {
        if( Physics2D.OverlapCircle(targetPos, 0.05f, solidObjectsLayer) != null)
        {
            return false;
        }
        return true;
    }
    private void CheckForEncounters()
    {
        if(Physics2D.OverlapCircle(transform.position, 0.2f, grassLayer) != null)
        {
            if(UnityEngine.Random.Range(1,101) <= 10) // 10% chance
            {
                Debug.Log("A wild Pokémon appeared!");
                animator.SetBool("isMoving", false);
                OnEncountered();

            }
        }
    }
}
