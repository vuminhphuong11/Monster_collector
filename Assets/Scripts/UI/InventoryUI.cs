using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] GameObject itemList;
    [SerializeField] ItemSlotUI itemSlotUI;
    [SerializeField] Image itemIcon;
    [SerializeField] Text ItemDesText;

    Inventory inventory;
    int selectedItem = 0;
    const int itemsInViewpost = 4;
    List<ItemSlotUI> slotUIList;
    RectTransform itemListRect;
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
        int preSelection = selectedItem;
        if(Input.GetKeyDown(KeyCode.DownArrow))
        {
            ++selectedItem;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            --selectedItem;
        }
        selectedItem= Mathf.Clamp(selectedItem, 0,inventory.Slots.Count- 1);
        if (preSelection != selectedItem)
        {
            UpdateItemSelection();
        }
        if (Input.GetKeyUp(KeyCode.X))
        {
            onBack?.Invoke();
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
    }
}
