using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Inventory : MonoBehaviour, ISavable
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
    public object CaptureState()
    {
        // Chuyển đổi danh sách slots thành dữ liệu lưu trữ (chỉ chứa tên item và số lượng)
        var saveData = slots.Select(i => i.GetSaveData()).ToList();
        return saveData;
    }

    public void RestoreState(object state)
    {
        // Nhận lại dữ liệu và tái tạo danh sách slots
        var saveData = (List<ItemSlotSaveData>)state;
        slots = saveData.Select(i => new ItemSlot(i)).ToList();
    }
}
[Serializable]
public class ItemSlot
{
    [SerializeField] ItemBase item;
    [SerializeField] int count;
    public ItemBase Item => item;
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

    // 2. Thêm Constructor để tạo ItemSlot từ dữ liệu đã lưu
    public ItemSlot(ItemSlotSaveData saveData)
    {
        // Tìm lại Item thực sự từ Database thông qua tên đã lưu
        item = ItemDB.GetItemByName(saveData.name);
        count = saveData.count;
    }

    // 3. Hàm tạo dữ liệu để lưu (không lưu trực tiếp ScriptableObject)
    public ItemSlotSaveData GetSaveData()
    {
        return new ItemSlotSaveData()
        {
            name = item.Name, // Lưu tên của ItemBase
            count = count
        };
    }
}
[Serializable]
public class ItemSlotSaveData
{
    public string name;
    public int count;
}
