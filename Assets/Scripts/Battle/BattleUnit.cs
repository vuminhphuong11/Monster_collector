using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BattleUnit : MonoBehaviour
{
    
    [SerializeField] bool isPlayerUnit;

    Image image;
    Vector3 originalPosition;
    Color originalColor;
    Vector3 originalScale; // Khai báo thêm

    private void Awake()
    {
        image = GetComponent<Image>();
        originalPosition = image.transform.localPosition;
        originalColor = image.color;
        originalScale = image.transform.localScale;
    }
    public Monster Monster { get; set; }
    public void Setup(Monster monster )
    {
        Monster = monster;
        if (isPlayerUnit)
        {
            //player unit setup
            image.sprite = Monster.Base.BackSprite;
        }
        else
        {
            //enemy unit setup
            image.sprite = Monster.Base.FrontSprite;
        }
        // Reset về thông số gốc từ Awake để sẵn sàng cho trận đấu sau
        image.DOKill();
        image.color = originalColor;
        image.transform.localPosition = originalPosition;
        image.transform.localScale = originalScale;
        PlayEnterAnimation();
    }
    public void PlayEnterAnimation()
    {
        if (isPlayerUnit)
        {
            //player unit enter animation
            image.transform.localPosition = new Vector3(-500f, originalPosition.y);
        }
        else
        {
            //enemy unit enter animation
            image.transform.localPosition = new Vector3(500f, originalPosition.y);
        }
        image.transform.DOLocalMove(originalPosition, 1f);
    }
    public void PlayAttackAnimation()
    {
        var sequence = DOTween.Sequence();
        if (isPlayerUnit)
        {
            //player unit attack animation
            sequence.Append(image.transform.DOLocalMoveX(originalPosition.x + 50f, 0.15f));
        }
        else
        {
            //enemy unit attack animation
            sequence.Append(image.transform.DOLocalMoveX(originalPosition.x - 50f, 0.15f));
        }
        sequence.Append(image.transform.DOLocalMoveX(originalPosition.x, 0.15f));
    }
    public void PlayHitAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(image.DOColor(Color.gray, 0.1f));
        sequence.Append(image.DOColor(originalColor, 0.1f));
        sequence.Append(image.DOColor(Color.gray, 0.15f));
        sequence.Append(image.DOColor(originalColor, 0.2f));
        sequence.Append(image.DOColor(Color.gray, 0.15f));
        sequence.Append(image.DOColor(originalColor, 0.25f));
    }
    public void PlayFaintAnimation()
    {
        // Tạo sequence mới
        var sequence = DOTween.Sequence();

        // --- GIAI ĐOẠN 1: SHOCK (Chớp trắng & Nảy nhẹ) ---
        sequence.Append(image.DOColor(Color.white, 0.05f).SetLoops(4, LoopType.Yoyo));
        sequence.Join(image.transform.DOLocalMoveY(originalPosition.y + 20f, 0.15f).SetEase(Ease.OutQuad));

        // --- GIAI ĐOẠN 2: SUY YẾU (Đổi màu xám & Rung lắc) ---
        // Sử dụng màu xám để tạo cảm giác mất sức sống
        sequence.Append(image.DOColor(Color.gray, 0.2f));
        sequence.Join(image.transform.DOShakePosition(0.4f, strength: new Vector3(20, 0, 0), vibrato: 15));

        // --- GIAI ĐOẠN 3: ĐĂNG XUẤT (Chìm xuống & Thu dẹt) ---
        // MoveY xuống kết hợp ScaleY về 0 để tuyệt đối không lộ chân
        sequence.Append(image.transform.DOLocalMoveY(originalPosition.y - 120f, 0.5f).SetEase(Ease.InBack));
        sequence.Join(image.transform.DOScaleY(0f, 0.5f).SetEase(Ease.InBack));
        sequence.Join(image.DOFade(0f, 0.4f));

    }
}
