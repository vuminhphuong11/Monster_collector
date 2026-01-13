using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public enum InventoryUIState { ItemSelection, PartySelection, Busy }

public class InventoryUI : MonoBehaviour
{
    [SerializeField] GameObject itemList;
    [SerializeField] ItemSlotUI itemSlotUI;
    [SerializeField] Image itemIcon;
    [SerializeField] Image upArrow;
    [SerializeField] Image downArrow;
    [SerializeField] Text ItemDesText;
    [SerializeField] StorageUI partyScreen;

    Inventory inventory;
    int selectedItem = 0;
    const int itemsInViewpost = 5;
    List<ItemSlotUI> slotUIList;
    RectTransform itemListRect;
    InventoryUIState state;

    private void Awake()
    {
        inventory = Inventory.GetInventory();
        itemListRect = itemList.GetComponent<RectTransform>();
    }
    private void OnEnable()
    {
        state = InventoryUIState.ItemSelection;
        // Đảm bảo cập nhật lại danh sách item để tránh hiển thị sai
        if (inventory != null)
        {
            UpdateItemList();
        }
    }
    private void Start()
    {
        UpdateItemList();
    }
    void UpdateItemList()
    {
        foreach (Transform child in itemList.transform) Destroy(child.gameObject);
        slotUIList = new List<ItemSlotUI>();
        foreach (var itemSlot in inventory.Slots)
        {
            var slotUIObj = Instantiate(itemSlotUI, itemList.transform);
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

            // KHI BẤM Z: Mở Storage
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
            // Khi Storage mở, Inventory chuyển quyền điều khiển cho StorageUI vaf su dung item cho pokemon
            partyScreen.HandleUpdate();
        }
    }
    void UpdateItemSelection()
    {
        for (int i = 0; i < slotUIList.Count; i++)
        {
            Color colorToSet = (i == selectedItem) ? Color.blue : Color.black; 
            slotUIList[i].NameText.color = colorToSet;
            slotUIList[i].CountText.color = colorToSet;
        }
        var item = inventory.Slots[selectedItem];
        itemIcon.sprite = item.Item.Icon;
        ItemDesText.text = item.Item.Description;
        HandleScrolling();
    }
    void HandleScrolling()
    {
        // Nếu số lượng item ít hơn hoặc bằng số dòng hiển thị -> Không cần cuộn
        if (slotUIList.Count <= itemsInViewpost)
        {
            itemListRect.localPosition = new Vector2(itemListRect.localPosition.x, 0);
            upArrow.gameObject.SetActive(false);
            downArrow.gameObject.SetActive(false);
            return;
        }
        float scrollIndex = Mathf.Clamp(selectedItem - itemsInViewpost / 2, 0, slotUIList.Count - itemsInViewpost);
        float scrollingPos = scrollIndex * slotUIList[0].Height;
        itemListRect.localPosition = new Vector2(itemListRect.localPosition.x, scrollingPos);
        bool showUpArrow = scrollIndex > 0;
        upArrow.gameObject.SetActive(showUpArrow);
        bool showDownArrow = scrollIndex < (slotUIList.Count - itemsInViewpost);
        downArrow.gameObject.SetActive(showDownArrow);
    }
    void OpenMonsterStorageUI()
    {
        state = InventoryUIState.PartySelection;
        // 1. Bật Storage
        partyScreen.gameObject.SetActive(true);
        // 2. TẮT INVENTORY (Để không hiện đè nhau)
        this.gameObject.SetActive(false);
        // 3. Lấy đúng Party của Player
        var playerParty = FindObjectOfType<PlayerController>().GetComponent<MonsterParty>();
        // 4. Bật chế độ chọn bên Storage
        partyScreen.EnableSelectionMode(playerParty, OnMonsterSelected, CloseMonsterStorageUI);
    }
    void OnMonsterSelected(Monster selectedMonster)
    {
        // 1. Gọi hàm UseItem từ Inventory
        // Hàm này sẽ tự động gọi item.Use() và RemoveItem() nếu thành công
        // Xem file Inventory.cs của bạn để thấy logic này
        var usedItem = inventory.UseItem(selectedItem, selectedMonster);

        // 2. Kiểm tra kết quả
        if (usedItem != null)
        {
            // --- THÀNH CÔNG ---
            // Item đã được trừ bên trong hàm inventory.UseItem rồi

            // Cập nhật lại danh sách item hiển thị (vì số lượng đã giảm)
            UpdateItemList();

            // Gửi thông báo sang Storage
            partyScreen.SetMessageText($"Used {usedItem.Name} on {selectedMonster.Base.Name}!");

            Debug.Log($"Used item {usedItem.Name} successfully.");
        }
        else
        {
            // --- THẤT BẠI ---
            // Do đầy máu/PP hoặc không đúng bệnh
            partyScreen.SetMessageText("It won't have any effect!");

            Debug.Log("Item failed to use.");
        }
    }

    void CloseMonsterStorageUI()
    {
        state = InventoryUIState.ItemSelection;
        // 1. Tắt Storage
        partyScreen.gameObject.SetActive(false);
        // 2. Hiện lại Inventory
        this.gameObject.SetActive(true);
        // Khi Inventory hiện lên rồi, ta mới cập nhật số lượng item
        UpdateItemList();
    }
}