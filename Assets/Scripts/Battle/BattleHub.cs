using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro; 
public class BattleHub : MonoBehaviour
{
    [SerializeField] TMP_Text nameText; // Đổi Text thành TMP_Text
    [SerializeField] TMP_Text levelText; // Đổi Text thành TMP_Text
    [SerializeField] TMP_Text statusText; // Đổi Text thành TMP_Text
    [SerializeField] HPBar hpBar;
    [SerializeField] GameObject expBar;
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
        if (_monster != null)
        {
            _monster.OnStatusChange -= SetStatusText;
        }
        _monster = monster;
        nameText.text = monster.Base.Name;
        SetLevel();
        hpBar.SetHP((float)monster.HP / monster.MaxHP);
        SetExp();

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
            statusText.text = "";
        }
       else
        {
            statusText.text = _monster.Status.id.ToString().ToUpper();
            statusText.color = StatusColor[_monster.Status.id];
        }
    }
    public void SetLevel()
    {
        levelText.text = "Lv: " + _monster.Level;
    }
    public IEnumerator UpdateHP()
    {
        if (_monster.HpChange == true)
        {
            yield return hpBar.SetHPSmooth((float)_monster.HP / _monster.MaxHP);
            _monster.HpChange = false;

        }
            
    }
    public void SetExp()
    {
        if (expBar == null) return;
        float normalizeExp = GetNormalizeExp();
        expBar.transform.localScale = new Vector3(normalizeExp, 1, 1);

    }
    public IEnumerator SetExpSmooth(bool reset=false)
    {
        if (expBar == null) yield break;
        if (reset)
        {
            expBar.transform.localScale = new Vector3(0, 1, 1);
        }
        float normalizeExp = GetNormalizeExp();
        yield return expBar.transform.DOScaleX(normalizeExp, 1.5f).WaitForCompletion();

    }
    float GetNormalizeExp()
    {
        int currentExp =_monster.Base.GetExpForLevel(_monster.Level);
        int nextLevelExp = _monster.Base.GetExpForLevel(_monster.Level+1);
        float normalizeExp=( float)(_monster.EXP -currentExp)/(nextLevelExp-currentExp);
        return Mathf.Clamp01(normalizeExp);
    }
    public void UpdateStatBoosts()
    {
        if (statBoostHUD != null)
        {
            statBoostHUD.SetStatBoosts(_monster);
            
        }
    }
}
