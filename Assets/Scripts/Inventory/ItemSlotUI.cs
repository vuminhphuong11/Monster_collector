using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlotUI : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text countText;

    RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public Text NameText => nameText;
    public Text CountText => countText;

    // --- SỬA ĐOẠN NÀY ---
    public float Height
    {
        get
        {
            // Nếu rectTransform bị null (do Awake chưa chạy kịp), ta lấy nó ngay lập tức
            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
            }
            return rectTransform.rect.height;
        }
    }
    // --------------------

    public void SetData(ItemSlot itemSlot)
    {
        nameText.text = itemSlot.Item.Name;
        countText.text = $"x {itemSlot.Count}";
    }
}