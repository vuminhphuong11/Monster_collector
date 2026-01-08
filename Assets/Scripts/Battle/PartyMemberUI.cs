using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PartyMemberUI : MonoBehaviour
{
    [SerializeField] TMP_Text nameText; // Đổi Text thành TMP_Text
    [SerializeField] TMP_Text levelText; // Đổi Text thành TMP_Text
    [SerializeField] HPBar hpBar;
    [SerializeField] Color highlightedColor;
    [SerializeField] Image monsterImage;

    Monster _monster;

    public void SetData(Monster monster)
    {
        _monster = monster;
        nameText.text = monster.Base.Name;
        levelText.text = "Lv : " + monster.Level;
        hpBar.SetHP((float)monster.HP / monster.MaxHP);
        hpBar.SetHP((float)monster.HP / monster.MaxHP);
        monsterImage.sprite = monster.Base.LeftSprite;
        monsterImage.color = (monster.HP >= 0) ? new Color(1, 1, 1, 1f) : Color.gray;
    }
    public void SetSelected (bool selected)
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
