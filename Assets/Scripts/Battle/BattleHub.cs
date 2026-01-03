using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; 
public class BattleHub : MonoBehaviour
{
    [SerializeField] TMP_Text nameText; // Đổi Text thành TMP_Text
    [SerializeField] TMP_Text levelText; // Đổi Text thành TMP_Text
    [SerializeField] HPBar hpBar;
    
    [SerializeField] StatBoostHUD statBoostHUD;
    Monster _monster;

    public void SetData(Monster monster)
    {
        _monster = monster;
        nameText.text = monster.Base.Name;
        levelText.text = "Lv : " + monster.Level;
        hpBar.SetHP((float)monster.HP / monster.MaxHP);

        if (statBoostHUD != null)
            statBoostHUD.gameObject.SetActive(false);
        
    }
    public IEnumerator UpdateHP()
    {
        if (_monster.HpChange == true)
        {
            yield return hpBar.SetHPSmooth((float)_monster.HP / _monster.MaxHP);
            _monster.HpChange = false;

        }
            
    }
    public void UpdateStatBoosts()
    {
        if (statBoostHUD != null)
        {
            statBoostHUD.SetStatBoosts(_monster.StatBoosts);
        }
    }
}
