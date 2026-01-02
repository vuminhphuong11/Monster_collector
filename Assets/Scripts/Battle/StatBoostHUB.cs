using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatBoostHUD : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] GameObject statPanel; // Panel chứa toàn bộ các thanh (để bật/tắt)

    // 5 thanh chỉ số
    [SerializeField] Image attackBar;
    [SerializeField] Image defenseBar;
    [SerializeField] Image spAttackBar;
    [SerializeField] Image spDefenseBar;
    [SerializeField] Image speedBar;

    [Header("Colors")]
    [SerializeField] Color buffColor = new Color(0.2f, 0.8f, 0.2f); // Màu xanh (Tăng)
    [SerializeField] Color debuffColor = new Color(0.8f, 0.2f, 0.2f); // Màu đỏ (Giảm)
    [SerializeField] Color neutralColor = Color.gray; // Màu xám (Bình thường)

    public void SetStatBoosts(Dictionary<Stat, int> boosts)
    {
        // 1. Hiện bảng lên
        statPanel.SetActive(true);

        // 2. Cập nhật từng thanh
        UpdateSingleStat(attackBar, boosts[Stat.Attack]);
        UpdateSingleStat(defenseBar, boosts[Stat.Defense]);
        UpdateSingleStat(spAttackBar, boosts[Stat.SpAttack]);
        UpdateSingleStat(spDefenseBar, boosts[Stat.SpDefense]);
        UpdateSingleStat(speedBar, boosts[Stat.Speed]);

        // 3. Đếm ngược 2 giây rồi ẩn
        StopAllCoroutines();
        StartCoroutine(HidePanel(2f));
    }

    void UpdateSingleStat(Image bar, int boostLevel)
    {
        // Quy đổi boost (-6 đến +6) thành tỷ lệ (0 đến 1)
        // -6 = 0 (Rỗng), 0 = 0.5 (Một nửa), +6 = 1 (Đầy)
        float normalizedVal = (boostLevel + 6) / 12f;

        // Cập nhật độ dài thanh
        bar.transform.localScale = new Vector3(normalizedVal, 1f, 1f);

        // Đổi màu
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