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
    Monster _monster;

    public void SetData(Monster monster)
    {
        _monster = monster;
        nameText.text = monster.Base.Name;
        levelText.text = "Lv : " + monster.Level;
        hpBar.SetHP((float)monster.HP / monster.MaxHP);
    }
}
