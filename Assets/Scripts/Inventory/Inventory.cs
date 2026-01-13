using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] List<ItemSlot> slots;
    public List<ItemSlot> Slots => slots;

    public static Inventory GetInventory()
    {
        return FindObjectOfType<PlayerController>().GetComponent<Inventory>();

    }
    public ItemBase UseItem(int itemIndex,Monster selectedMonster)
    {
        var item=slots[itemIndex].Item;
        bool itemUsed=item.Use(selectedMonster);
        if (itemUsed)
        {
            RemoveItem(item);
            return item;
        }
        return null;
    }
    public void RemoveItem(ItemBase item)
    {
        // Nếu là vật phẩm bắt quái (vĩnh viễn) thì không xóa
        if (item is CaptureItem) return;

        var itemSlot = slots.First(slot => slot.Item == item);
        itemSlot.Count--;
        if (itemSlot.Count == 0)
        {
            slots.Remove(itemSlot);
        }
    }
    // Thêm vào file Inventory.cs
    public void AddItem(ItemBase item, int count = 1)
    {
        var itemSlot = slots.FirstOrDefault(slot => slot.Item == item);
        if (itemSlot != null)
        {
            itemSlot.Count += count;
        }
        else
        {
            // Sử dụng Constructor thay vì kiểu khởi tạo dấu ngoặc nhọn { }
            slots.Add(new ItemSlot(item, count));
        }
    }
}
[Serializable]
public class ItemSlot
{
    [SerializeField] ItemBase item;
    [SerializeField] int count;
    public ItemBase Item=>item;
    public int Count
    {
        get => count;
        set => count = value;
    }
    public ItemSlot(ItemBase item, int count)
    {
        this.item = item;
        this.count = count;
    }
}
