using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// Thêm kế thừa ISavable
public class MonsterStorage : MonoBehaviour, ISavable
{
    public static MonsterStorage Instance { get; private set; }
    public const int MONSTERS_PER_BOX = 6;

    [Header("Debug / Starting Data")]
    [SerializeField] List<Monster> startingMonsters;

    List<Monster> storedMonsters = new List<Monster>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Chỉ load startingMonsters nếu kho đang rỗng (lần đầu chơi)
        // Nếu load game, RestoreState sẽ chạy sau và ghi đè cái này
        if (storedMonsters.Count == 0)
        {
            foreach (var monster in startingMonsters)
            {
                monster.Init();
                AddMonster(monster);
            }
        }
    }

    public void AddMonster(Monster monster)
    {
        monster.HP = monster.MaxHP;
        monster.CureStatus();
        monster.CureVolatileStatus();
        for (int i = 0; i < storedMonsters.Count; i++)
        {
            if (storedMonsters[i] == null)
            {
                storedMonsters[i] = monster;
                return;
            }
        }
        storedMonsters.Add(monster);
    }

    public List<Monster> GetMonstersInBox(int boxIndex)
    {
        return storedMonsters.Skip(boxIndex * MONSTERS_PER_BOX).Take(MONSTERS_PER_BOX).ToList();
    }

    public int GetTotalBoxes()
    {
        if (storedMonsters.Count == 0) return 1;
        // Tính dựa trên Index cuối cùng có quái vật
        return Mathf.Max(1, Mathf.CeilToInt((float)storedMonsters.Count / MONSTERS_PER_BOX));
    }
    public void SwapMonster(int boxIndex, int slotIndex, int partyIndex, MonsterParty party)
    {
        int globalBoxIndex = boxIndex * MONSTERS_PER_BOX + slotIndex;

        // Đảm bảo danh sách đủ lớn để chứa Index này (tránh lỗi Index Out Of Range)
        while (storedMonsters.Count <= globalBoxIndex)
        {
            storedMonsters.Add(null);
        }

        Monster boxMonster = storedMonsters[globalBoxIndex];
        Monster partyMonster = (partyIndex < party.Monsters.Count) ? party.Monsters[partyIndex] : null;

        if (boxMonster != null && partyMonster != null)
        {
            storedMonsters[globalBoxIndex] = partyMonster;
            party.Monsters[partyIndex] = boxMonster;
        }
        else if (boxMonster != null && partyMonster == null)
        {
            party.Monsters.Add(boxMonster);
            storedMonsters[globalBoxIndex] = null; // Giữ ô trống tại Index này
        }
        else if (boxMonster == null && partyMonster != null)
        {
            storedMonsters[globalBoxIndex] = partyMonster; // Đặt đúng vào ô đã chọn
            party.Monsters.RemoveAt(partyIndex);
        }
    }
    private void Update()
    {
        foreach (var monster in storedMonsters)
        {
            if (monster != null) // QUAN TRỌNG: Phải kiểm tra null
            {
                monster.HandlePassiveRegen();
            }
        }
    }


    public object CaptureState()
    {
        // Lưu toàn bộ danh sách quái trong kho
        List<MonsterSaveData> saveData = new List<MonsterSaveData>();
        foreach (var monster in storedMonsters)
        {
            saveData.Add(monster.GetSaveData());
        }
        return saveData;
    }

    public void RestoreState(object state)
    {
        var saveData = state as List<MonsterSaveData>;
        if (saveData != null)
        {
            storedMonsters.Clear(); // Xóa dữ liệu cũ/mặc định
            foreach (var data in saveData)
            {
                storedMonsters.Add(new Monster(data)); // Tái tạo quái từ file save
            }
        }
    }
}