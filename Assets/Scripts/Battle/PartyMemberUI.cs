using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PartyMemberUI : MonoBehaviour
{
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text levelText;
    [SerializeField] HPBar hpBar;
    [SerializeField] Color highlightedColor;
    [SerializeField] Image monsterImage;
    // 1. THÊM MỚI: Biến tham chiếu tới thanh EXP
    [SerializeField] GameObject expBar;
    Monster _monster;
    public void SetNameColor(Color color)
    {
        nameText.color = color;
    }
    public void SetData(Monster monster)
    {
        // 1. HỦY ĐĂNG KÝ con quái cũ (nếu có) để tránh lỗi
        if (_monster != null)
        {
            _monster.OnHPChanged -= UpdateHP;
        }
        _monster = monster;
        // Cập nhật thông tin tĩnh
        nameText.text = monster.Base.Name;
        levelText.text = "Lv : " + monster.Level;
        monsterImage.sprite = monster.Base.LeftSprite;
        monsterImage.color = (monster.HP > 0) ? new Color(1, 1, 1, 1f) : Color.gray;
        SetExp();
        UpdateHP();
        _monster.OnHPChanged += UpdateHP;
    }
    void UpdateHP()
    {
        if (_monster != null)
        {
            hpBar.SetHP((float)_monster.HP / _monster.MaxHP);

            // Cập nhật lại màu sắc nếu vừa hồi sinh từ 0 lên 1
            if (_monster.HP > 0 && monsterImage.color == Color.gray)
            {
                monsterImage.color = new Color(1, 1, 1, 1f);
            }
        }
    }
    private void OnDisable()
    {
        if (_monster != null)
        {
            _monster.OnHPChanged -= UpdateHP;
        }
    }
    public void SetExp()
    {
        if (expBar == null) return;
        float normalizeExp = GetNormalizeExp();
        expBar.transform.localScale = new Vector3(normalizeExp, 1, 1);
    }
    // 4. THÊM MỚI: Hàm tính toán tỷ lệ % EXP hiện tại
    float GetNormalizeExp()
    {
        int currentLevelExp = _monster.Base.GetExpForLevel(_monster.Level);
        int nextLevelExp = _monster.Base.GetExpForLevel(_monster.Level + 1);

        // Tính toán EXP hiện có trong level hiện tại / Tổng EXP cần để lên level tiếp theo
        float normalizeExp = (float)(_monster.EXP - currentLevelExp) / (nextLevelExp - currentLevelExp);

        return Mathf.Clamp01(normalizeExp);
    }

    public void SetSelected(bool selected)
    {
        if (selected)
        {
            nameText.color = highlightedColor;
            levelText.color = highlightedColor;
        }
        else
        {
            nameText.color = Color.black;
            levelText.color = Color.black;
        }
    }
}