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

    // ... (Các hàm AddMonster, SwapMonster, GetMonstersInBox giữ nguyên) ...
    public void AddMonster(Monster monster)
    {
        monster.HP = monster.MaxHP;
        monster.CureStatus();
        monster.CureVolatileStatus();
        storedMonsters.Add(monster);
    }

    public List<Monster> GetMonstersInBox(int boxIndex)
    {
        return storedMonsters.Skip(boxIndex * MONSTERS_PER_BOX).Take(MONSTERS_PER_BOX).ToList();
    }

    public int GetTotalBoxes()
    {
        return Mathf.Max(1, Mathf.CeilToInt((float)storedMonsters.Count / MONSTERS_PER_BOX) + 1);
    }

    public void SwapMonster(int boxIndex, int slotIndex, int partyIndex, MonsterParty party)
    {
        // ... (Logic Swap bạn đã có, giữ nguyên) ...
        int globalBoxIndex = boxIndex * MONSTERS_PER_BOX + slotIndex;
        Monster boxMonster = (globalBoxIndex < storedMonsters.Count) ? storedMonsters[globalBoxIndex] : null;
        Monster partyMonster = (partyIndex < party.Monsters.Count) ? party.Monsters[partyIndex] : null;

        if (boxMonster != null && partyMonster != null)
        {
            storedMonsters[globalBoxIndex] = partyMonster;
            party.Monsters[partyIndex] = boxMonster;
        }
        else if (boxMonster != null && partyMonster == null)
        {
            party.Monsters.Add(boxMonster);
            storedMonsters.RemoveAt(globalBoxIndex);
        }
        else if (boxMonster == null && partyMonster != null)
        {
            AddMonster(partyMonster);
            party.Monsters.RemoveAt(partyIndex);
        }
    }

    // --- PHẦN SAVE/LOAD QUAN TRỌNG ---

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