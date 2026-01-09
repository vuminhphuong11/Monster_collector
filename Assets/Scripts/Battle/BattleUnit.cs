using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BattleUnit : MonoBehaviour
{
    
    [SerializeField] bool isPlayerUnit;
    [SerializeField] BattleHub hub;
    public bool IsPlayerUnit { get { return isPlayerUnit; } }
    public BattleHub Hub { get { return hub; } }

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
            image.sprite = Monster.Base.RightPrite;
        }
        else
        {
            //enemy unit setup
            image.sprite = Monster.Base.LeftSprite;
        }
        hub.gameObject.SetActive(true);
        hub.SetData(Monster);
        // Reset về thông số gốc từ Awake để sẵn sàng cho trận đấu sau
        image.DOKill();
        image.color = originalColor;
        image.transform.localPosition = originalPosition;
        image.transform.localScale = originalScale;
        PlayEnterAnimation();
    }
    public void Clear()
    {
        hub.gameObject.SetActive(false);
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
        var sequence = DOTween.Sequence();
        sequence.Append(image.DOColor(Color.white, 0.05f).SetLoops(4, LoopType.Yoyo));
        sequence.Join(image.transform.DOLocalMoveY(originalPosition.y + 20f, 0.15f).SetEase(Ease.OutQuad));
        sequence.Append(image.DOColor(Color.gray, 0.2f));
        sequence.Join(image.transform.DOShakePosition(0.4f, strength: new Vector3(20, 0, 0), vibrato: 15));
        sequence.Append(image.transform.DOLocalMoveY(originalPosition.y - 120f, 0.5f).SetEase(Ease.InBack));
        sequence.Join(image.transform.DOScaleY(0f, 0.5f).SetEase(Ease.InBack));
        sequence.Join(image.DOFade(0f, 0.4f));

    }
    public void PlayExitAnimation()
    {
        var sequence = DOTween.Sequence();

        sequence.Append(image.transform.DOLocalMoveX(originalPosition.x + 30f, 0.1f).SetEase(Ease.OutQuad));
        sequence.Append(image.transform.DOLocalMoveX(originalPosition.x - 500f, 0.4f).SetEase(Ease.InSine));
        sequence.Join(image.DOFade(0f, 0.4f));
        sequence.Join(image.transform.DOScale(0.8f, 0.4f));
    }
    // Thêm tham số targetPosition (vị trí cuốn sách)
    public IEnumerator PlayCaptureAnimation(Vector3 targetPosition)
    {
        var sequence = DOTween.Sequence();

        // 1. Vừa mờ dần
        sequence.Append(image.DOFade(0, 0.7f));
        // 2. Vừa thu nhỏ
        sequence.Join(transform.DOScale(new Vector3(0.1f, 0.1f, 1f), 0.7f));
        // 3. QUAN TRỌNG: Vừa di chuyển về phía cuốn sách (tạo hiệu ứng bị hút)
        sequence.Join(transform.DOMove(targetPosition, 0.7f));

        yield return sequence.WaitForCompletion();
    }
    public IEnumerator PlayBreakOutAnimation(Vector3 bookPosition)
    {
        var sequence = DOTween.Sequence();
        // Đặt vị trí bắt đầu ngay tại cuốn sách
        transform.position = bookPosition;
        // Đảm bảo trạng thái bắt đầu: Nhỏ và Trong suốt
        transform.localScale = new Vector3(0.1f, 0.1f, 1f);
        var color = image.color;
        image.color = new Color(color.r, color.g, color.b, 0);
        // 1. Hiện hình lại (Fade In)
        sequence.Append(image.DOFade(1f, 0.5f));
        // 2. Phóng to về kích thước gốc
        sequence.Join(transform.DOScale(originalScale, 0.5f).SetEase(Ease.OutBack)); 
        // 3. QUAN TRỌNG: Di chuyển về vị trí gốc dùng DOLocalMove
        // Vì originalPosition là tọa độ Local
        sequence.Join(transform.DOLocalMove(originalPosition, 0.5f));

        yield return sequence.WaitForCompletion();
    }
}
