using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new capture item")]
public class CaptureItem : ItemBase
{
    // Item này không dùng logic Use(Monster) thông thường 
    // vì việc bắt quái cần xử lý animation phức tạp trong BattleSystem
    public override bool Use(Monster monster)
    {
        return true; // Trả về true để xác nhận item có thể chọn
    }
}