using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;


public class PlayerController : MonoBehaviour,ISavable
{
    [SerializeField] Sprite sprite;
    [SerializeField] string name;
    public string Name
    {
        get => name;
    }
    public Sprite Sprite
    {
        get => sprite;
    }
    public Character Character =>character;

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
                StartCoroutine(character.Move(input,OnMoveOver));
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

    private void OnMoveOver()
    {
        var colliders= Physics2D.OverlapCircleAll(transform.position, 0.2f, GameLayer.i.TriggerableLayer);
        foreach(var collider in colliders)
        {
            var triggerable = collider.GetComponent<IPlayerTriggerable>();
            if(triggerable != null)
            {
                
                triggerable.OnPlayerTriggered(this);
                break;
            }
        }

    }

    public object CaptureState()
    {
        var saveData = new PlayerSaveData()
        {
            position = new float[] { transform.position.x, transform.position.y },
            monsters = GetComponent<MonsterParty>().Monsters.Select(p=>p.GetSaveData()).ToList()
        };
        float[] position= new float[] {transform.position.x,transform.position.y};
        return saveData;
    }

    public void RestoreState(object state)
    {
        var saveData= (PlayerSaveData)state;
        var pos=saveData.position;
        transform.position = new Vector3(pos[0], pos[1]);
        GetComponent<MonsterParty>().Monsters= saveData.monsters.Select(s=>new Monster(s)).ToList();
    }
}
[Serializable]
public class PlayerSaveData
{
    public float[] position;
    public List<MonsterSaveData> monsters;
}
