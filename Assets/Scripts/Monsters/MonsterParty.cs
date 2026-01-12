using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MonsterParty : MonoBehaviour
{
    [SerializeField] List<Monster> monsters;

    public List<Monster> Monsters
    {
        get { return monsters; }
        set { monsters = value; }
    }

    private void Start()
    {
        foreach (var monster in monsters)
        {
            monster.Init();
        }
    }

    public Monster GetHealthyMonster()
    {
        return monsters.Where(x => x.HP > 0).FirstOrDefault();
    }

    // --- SỬA ĐỔI CHÍNH TẠI ĐÂY ---
    public bool AddMonster(Monster newMonster)
    {
        // Bạn đang để giới hạn là 3, hãy sửa thành 6 nếu muốn đúng chuẩn Pokemon
        if (monsters.Count < 3)
        {
            monsters.Add(newMonster);
            Debug.Log("Added to Party.");
            return true; // Đã thêm vào Party
        }
        else
        {
            // Nếu đầy, tự động gửi vào MonsterStorage
            MonsterStorage.Instance.AddMonster(newMonster);
            Debug.Log("Party full! Transferred to PC Box.");
            return false; // Đã chuyển vào Box
        }
    }
}