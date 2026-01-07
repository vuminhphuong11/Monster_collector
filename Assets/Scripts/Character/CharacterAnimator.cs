using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    //paarameter
    [SerializeField] List<Sprite> walkDownSprite;
    [SerializeField] List<Sprite> walkUpSprite;
    [SerializeField] List<Sprite> walkRightSprite;
    [SerializeField] List <Sprite> walkLeftSprite;


    public float MoveX {  get; set; }
    public float MoveY { get; set; }

    public bool IsMoving { get; set; }
    //state
    SpriteAnimator walkDownAni;
    SpriteAnimator walkUpAni;
    SpriteAnimator walkRightAni;
    SpriteAnimator walkLeftAni;

    SpriteAnimator currentAni;
    bool wasPreviouslyMoving;
    // refrences
    SpriteRenderer spriteRenderer;
    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        walkDownAni = new SpriteAnimator(walkDownSprite,spriteRenderer);
        walkUpAni =new SpriteAnimator(walkUpSprite,spriteRenderer);
        walkLeftAni =new SpriteAnimator(walkLeftSprite,spriteRenderer);
        walkRightAni=new SpriteAnimator(walkRightSprite,spriteRenderer);
        currentAni = walkDownAni;
    }
    private void Update()
    {
        var preAni = currentAni;
        // 1. Xác định Animation dựa trên hướng di chuyển
        if (MoveX == 1)
            currentAni = walkRightAni;
        else if (MoveX == -1)
            currentAni = walkLeftAni;
        else if (MoveY == 1)
            currentAni = walkUpAni;
        else if (MoveY == -1)
            currentAni = walkDownAni;
        if(preAni != currentAni||IsMoving!=wasPreviouslyMoving)
        {
            currentAni.Start();
        }
        if (IsMoving)
        {
            currentAni.HandleUpdate();
        }
        else
        {
            spriteRenderer.sprite=currentAni.Frames[0];
        }
        wasPreviouslyMoving=IsMoving;
    }
}
