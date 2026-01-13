using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public enum InventoryUIState { ItemSelection, PartySelection ,Busy}
public class InventoryUI : MonoBehaviour
{
    [SerializeField] GameObject itemList;
    [SerializeField] ItemSlotUI itemSlotUI;
    [SerializeField] Image itemIcon;
    [SerializeField] Image upArrow;
    [SerializeField] Image  downArrow;
    [SerializeField] Text ItemDesText;
    [SerializeField] StorageUI partyScreen;

    Inventory inventory;
    int selectedItem = 0;
    const int itemsInViewpost = 4;
    List<ItemSlotUI> slotUIList;
    RectTransform itemListRect;
    InventoryUIState state;
    private void Awake()
    {
        inventory = Inventory.GetInventory();
        itemListRect = itemList.GetComponent<RectTransform>();
    }
    private void Start()
    {
        UpdateItemList();
    }
    void UpdateItemList()
    {
        // clear all exiting Item
        foreach (Transform child in itemList.transform)
        {
            Destroy(child.gameObject);
        }
        slotUIList = new List<ItemSlotUI>();
        foreach (var itemSlot in inventory.Slots)
        {
            var slotUIObj=Instantiate(itemSlotUI, itemList.transform);
            slotUIObj.SetData(itemSlot);
            slotUIList.Add(slotUIObj);
        }
        UpdateItemSelection();
    }
    public void HandleUpdate(Action onBack)
    {
        if (state == InventoryUIState.ItemSelection)
        {
            int preSelection = selectedItem;
            if (Input.GetKeyDown(KeyCode.DownArrow)) ++selectedItem;
            else if (Input.GetKeyDown(KeyCode.UpArrow)) --selectedItem;

            selectedItem = Mathf.Clamp(selectedItem, 0, inventory.Slots.Count - 1);

            if (preSelection != selectedItem) UpdateItemSelection();

            // KHI BẤM Z: Mở Storage để chọn Pokemon
            if (Input.GetKeyDown(KeyCode.Z))
            {
                OpenMonsterStorageUI();
            }
            else if (Input.GetKeyUp(KeyCode.X))
            {
                onBack?.Invoke();
            }
        }
        else if (state == InventoryUIState.PartySelection)
        {
            // --- SỬA LỖI TẠI ĐÂY ---
            // Gọi hàm HandleUpdate trên instance 'partyScreen' chứ không gọi class StorageUI
            partyScreen.HandleUpdate();
        }
    }
    void UpdateItemSelection()
    {
        for (int i = 0; i < slotUIList.Count; i++)
        {
            Color colorToSet = (i == selectedItem) ? GlobalSetting.i.HighlightedColor : Color.black;

            // Gán màu cho từng thành phần
            slotUIList[i].NameText.color = colorToSet;
            slotUIList[i].CountText.color = colorToSet;
        }
        var item = inventory.Slots[selectedItem];
        itemIcon.sprite=item.Item.Icon;
        ItemDesText.text=item.Item.Description;
        HandleScrolling();
    }
    void HandleScrolling()
    {
        float scrollingPos = Mathf.Clamp(selectedItem-itemsInViewpost,0,selectedItem )* slotUIList[0].Height;
        itemListRect.localPosition=new Vector2(itemListRect.localPosition.x, scrollingPos);
        bool showUpArrow = selectedItem>itemsInViewpost;
        upArrow.gameObject.SetActive(showUpArrow);
        bool showDownArrow = selectedItem+itemsInViewpost<slotUIList.Count;
        downArrow.gameObject.SetActive(showDownArrow);
    }
    void OpenMonsterStorageUI()
    {
        state = InventoryUIState.PartySelection;
        partyScreen.gameObject.SetActive(true);
        this.gameObject.SetActive(false);

        // Lấy dữ liệu Party của người chơi
        // (Lưu ý: InventoryUI nằm trên GameController hoặc Player, 
        // ở đây ta giả định lấy thông qua GameController hoặc tìm component)
        var playerParty = FindObjectOfType<PlayerController>().GetComponent<MonsterParty>();

        // Gọi hàm bật chế độ chọn mà ta vừa viết bên StorageUI
        partyScreen.EnableSelectionMode(playerParty, OnMonsterSelected, CloseMonsterStorageUI);
    }

    void OnMonsterSelected(Monster selectedMonster)
    {
        // 1. Lấy Item đang chọn
        var itemSlot = inventory.Slots[selectedItem];
        ItemBase item = itemSlot.Item;

        Debug.Log($"Used item {item.Name} on Pokemon {selectedMonster.Base.Name}");

        // 2. [TODO] Áp dụng logic Item tại đây 
        // Ví dụ: item.Use(selectedMonster);
        // Nếu dùng thành công thì trừ số lượng item...

        // 3. Sau khi dùng xong, đóng Storage và quay lại túi đồ
        CloseMonsterStorageUI();
    }

    void CloseMonsterStorageUI()
    {
        state = InventoryUIState.ItemSelection;

        // 1. Tắt Storage
        partyScreen.gameObject.SetActive(false);

        // 2. Hiện lại Inventory
        this.gameObject.SetActive(true);
    }
}
