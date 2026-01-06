using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; 
public class BattleHub : MonoBehaviour
{
    [SerializeField] TMP_Text nameText; // Đổi Text thành TMP_Text
    [SerializeField] TMP_Text levelText; // Đổi Text thành TMP_Text
    [SerializeField] TMP_Text statusText; // Đổi Text thành TMP_Text
    [SerializeField] HPBar hpBar;
    [SerializeField] Color psnColor;
    [SerializeField] Color brnColor;
    [SerializeField] Color slpColor;
    [SerializeField] Color parColor;
    [SerializeField] Color frzColor;
    [SerializeField] StatBoostHUD statBoostHUD;
    Monster _monster;
    Dictionary<ConditionID, Color> StatusColor;
    public void SetData(Monster monster)
    {
        _monster = monster;
        nameText.text = monster.Base.Name;
        levelText.text = "Lv : " + monster.Level;
        hpBar.SetHP((float)monster.HP / monster.MaxHP);

        if (statBoostHUD != null)
            statBoostHUD.gameObject.SetActive(false);
        StatusColor = new Dictionary<ConditionID, Color>()
        {
            {ConditionID.psn,psnColor},
            {ConditionID.brn,brnColor},
            {ConditionID.par,parColor},
            {ConditionID.frz,frzColor},
            {ConditionID.slp,slpColor},
        };
        SetStatusText();
        _monster.OnStatusChange += SetStatusText;
    }
    public void SetStatusText()
    {
       if(_monster.Status == null)
        {
            statusText.text = "NaN";
        }
       else
        {
            statusText.text = _monster.Status.id.ToString().ToUpper();
            statusText.color = StatusColor[_monster.Status.id];
        }
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
