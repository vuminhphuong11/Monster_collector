using System.Collections.Generic;
using UnityEngine;

public class ItemDB
{
    static Dictionary<string, ItemBase> items;

    public static void Init()
    {
        items = new Dictionary<string, ItemBase>();

        // Tải tất cả ItemBase từ thư mục Resources/Items (hoặc bất kỳ đâu trong Resources)
        var itemList = Resources.LoadAll<ItemBase>("");
        foreach (var item in itemList)
        {
            if (items.ContainsKey(item.Name))
            {
                Debug.LogError($"Có hai vật phẩm trùng tên: {item.Name}");
                continue;
            }
            items[item.Name] = item;
        }
    }

    public static ItemBase GetItemByName(string name)
    {
        if (!items.ContainsKey(name))
        {
            Debug.LogError($"Không tìm thấy vật phẩm '{name}' trong ItemDB. Hãy chắc chắn item nằm trong thư mục Resources.");
            return null;
        }
        return items[name];
    }
}