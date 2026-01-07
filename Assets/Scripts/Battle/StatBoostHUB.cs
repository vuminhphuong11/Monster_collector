using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq; // Cần thêm cái này để dùng .Max()

public class StatBoostHUD : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] GameObject statPanel;

    [SerializeField] Image attackBar;
    [SerializeField] Image defenseBar;
    [SerializeField] Image spAttackBar;
    [SerializeField] Image spDefenseBar;
    [SerializeField] Image speedBar;
    [SerializeField] Image accBar;

    [Header("Colors")]
    [SerializeField] Color buffColor = new Color(0.2f, 0.8f, 0.2f);
    [SerializeField] Color debuffColor = new Color(0.8f, 0.2f, 0.2f);
    [SerializeField] Color neutralColor = new Color(0.5f, 0.5f, 0.5f);

    // Thay vì chỉ nhận Dictionary boost, ta nhận cả object Monster để lấy chỉ số gốc
    public void SetStatBoosts(Monster monster)
    {
        statPanel.SetActive(true);

        // 1. Tìm chỉ số gốc cao nhất (trong 5 chỉ số chính) để làm thước đo chuẩn
        // Mục đích: Để thanh Atk của con rồng (Atk 200) sẽ dài gấp đôi con sâu (Atk 100)
        int maxBaseStat = GetMaxBaseStat(monster);

        // 2. Cập nhật các thanh chỉ số chính
        // Ta truyền vào: Chỉ số thực tế hiện tại, và Mốc chuẩn để so sánh
        UpdateStatBar(attackBar, monster.Attack, monster.StatBoosts[Stat.Attack], maxBaseStat);
        UpdateStatBar(defenseBar, monster.Defense, monster.StatBoosts[Stat.Defense], maxBaseStat);
        UpdateStatBar(spAttackBar, monster.SpAttack, monster.StatBoosts[Stat.SpAttack], maxBaseStat);
        UpdateStatBar(spDefenseBar, monster.SpDefense, monster.StatBoosts[Stat.SpDefense], maxBaseStat);
        UpdateStatBar(speedBar, monster.Speed, monster.StatBoosts[Stat.Speed], maxBaseStat);

        // 3. Xử lý riêng Accuracy (Vì nó ko có chỉ số gốc như Atk/Def)
        UpdateAccuracyBar(accBar, monster.StatBoosts[Stat.Accuracy]);

        StopAllCoroutines();
        StartCoroutine(HidePanel(2f));
    }

    // Tìm giá trị lớn nhất trong các chỉ số gốc (không tính boost)
    int GetMaxBaseStat(Monster monster)
    {
        // Lấy các giá trị thô từ Stats dictionary (đã tính level nhưng chưa tính boost)
        int maxVal = 0;
        if (monster.Stats[Stat.Attack] > maxVal) maxVal = monster.Stats[Stat.Attack];
        if (monster.Stats[Stat.Defense] > maxVal) maxVal = monster.Stats[Stat.Defense];
        if (monster.Stats[Stat.SpAttack] > maxVal) maxVal = monster.Stats[Stat.SpAttack];
        if (monster.Stats[Stat.SpDefense] > maxVal) maxVal = monster.Stats[Stat.SpDefense];
        if (monster.Stats[Stat.Speed] > maxVal) maxVal = monster.Stats[Stat.Speed];

        // Tránh chia cho 0
        return Mathf.Max(maxVal, 1);
    }

    void UpdateStatBar(Image bar, int currentVal, int boostLevel, int maxBaseStat)
    {


        float normalizedVal = (float)currentVal / (maxBaseStat * 2.5f);

        // Clamp lại để không bị tràn thanh nếu buff quá khủng
        normalizedVal = Mathf.Clamp(normalizedVal, 0.05f, 1f);

        bar.transform.localScale = new Vector3(normalizedVal, 1f, 1f);

        // Đổi màu dựa trên Boost
        if (boostLevel > 0) bar.color = buffColor;
        else if (boostLevel < 0) bar.color = debuffColor;
        else bar.color = neutralColor;
    }

    // Accuracy hoạt động theo cơ chế xác suất nên giữ nguyên logic cũ hoặc chỉnh nhẹ
    void UpdateAccuracyBar(Image bar, int boostLevel)
    {
        // Accuracy mặc định là 50% thanh (Scale 0.5)
        float normalizedVal = 0.5f;

        if (boostLevel > 0)
            normalizedVal += (float)boostLevel / 6f * 0.5f; // Tăng dần lên 1
        else
            normalizedVal -= (float)Mathf.Abs(boostLevel) / 6f * 0.35f; // Giảm xuống nhưng ko mất hẳn

        bar.transform.localScale = new Vector3(normalizedVal, 1f, 1f);

        if (boostLevel > 0) bar.color = buffColor;
        else if (boostLevel < 0) bar.color = debuffColor;
        else bar.color = neutralColor;
    }

    IEnumerator HidePanel(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        statPanel.SetActive(false);
    }
}