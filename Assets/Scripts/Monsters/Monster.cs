using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
[System.Serializable]
public class Monster
{
    [SerializeField] MonsterBase _base;
    [SerializeField] int level;
    public MonsterBase Base { 
        get {
            return _base;
        } 
    }
    public int Level { 
        get {
            return level;
        } 
    }
    public int HP { get; set; }
    public List<Move> Moves { get; set; }
    public Dictionary<Stat, int> Stats{ get; private set; }
    public Dictionary<Stat, int> StatBoosts { get; private set; }

    public Condition Status {  get;private set; }

    public int StatusTime { get; set; }

    public Queue<string> StatusChanges { get; private set; }= new Queue<string>();
    public bool HpChange { get;  set; }

    
    public void Init()
    {

        //generate moves
        Moves = new List<Move>();
        foreach (var move in Base.LearnableMoves)
        {
            if (move.Level <= Level)
            {
                Moves.Add(new Move(move.Base));
            }
            if(Moves.Count >= 4)
            {
                break;
            }
        }
        CaculateStats();    
        HP = MaxHP;
        ResetStatBoosts();


    }
    void CaculateStats()
    {
        Stats = new Dictionary<Stat, int>();
        Stats.Add(Stat.Attack, Mathf.FloorToInt((Base.Attack * Level) / 100f) + 5);
        Stats.Add(Stat.Defense, Mathf.FloorToInt((Base.Defense * Level) / 100f) + 5);
        Stats.Add(Stat.SpAttack, Mathf.FloorToInt((Base.SpAttack * Level) / 100f) + 5);
        Stats.Add(Stat.SpDefense, Mathf.FloorToInt((Base.SpDefense * Level) / 100f) + 5);
        Stats.Add(Stat.Speed, Mathf.FloorToInt((Base.Speed * Level) / 100f) + 5);

        MaxHP = Mathf.FloorToInt((Base.MaxHP * Level) / 100f) + 10 + Level;
    }

    void ResetStatBoosts()
    {
        StatBoosts = new Dictionary<Stat, int>()
        {
            { Stat.Attack, 0 },
            { Stat.Defense, 0 },
            { Stat.SpAttack, 0 },
            { Stat.SpDefense, 0 },
            { Stat.Speed, 0 },
        };
    }

    int GetStat(Stat stat) 
    {  
        int statVal =Stats[stat];
        
        int boost = StatBoosts[stat];
        float[] boostValues = { 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f };
        if (boost>=0)
        {
            statVal=Mathf.FloorToInt(statVal*(boostValues[boost]));
        }
        else
        {
            statVal=Mathf.FloorToInt(statVal/(boostValues[-boost]));
        }
        return statVal;
    }
    public void ApplyBoosts(List<StatBoost> statBoosts)
    {
        foreach (var statBoost in statBoosts)
        {
            var stat = statBoost.stat;
            var boost = statBoost.boost;
            StatBoosts[stat] = Mathf.Clamp(StatBoosts[stat] + boost, -6, 6);
            if (boost > 0)
            {
                StatusChanges.Enqueue($"{Base.Name}'s {stat} rose!");
            }
            else
            {
                StatusChanges.Enqueue($"{Base.Name}'s {stat} fell!");
            }

            Debug.Log($"{stat} has been boosted by {boost}. Current boost: {StatBoosts[stat]}");
        }
    }
    public int Attack
    {
        get { return GetStat(Stat.Attack); }
    }
    public int Defense
    {
        get { return GetStat(Stat.Defense); }
    }
    public int SpAttack
    {
        get { return GetStat(Stat.SpAttack); }
    }
    public int SpDefense
    {
        get { return GetStat(Stat.SpDefense); }
    }   
    public int Speed
    {
        get { return GetStat(Stat.Speed); }
    }
    public int MaxHP
    {
        get; private set;
    }
    public DamageDetails TakeDamage(Move move, Monster attacker)
    {
        float critical = 1f;
        if (Random.value*100f <= 8f)
            critical =1.5f;

        float type = TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type1) * TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type2);
        
        var damageDetails  = new DamageDetails()
        {
            Critical = critical,
            TypeEffectiveness = type,
            Fainted = false
        };

        float attack=(move.Base.Category==MoveCategory.Special)? attacker.SpAttack : attacker.Attack;
        float defense=(move.Base.Category == MoveCategory.Special) ? SpDefense : Defense;

        float modifier = Random.Range(0.9f, 1f)*type*critical;
        float a = (2 * attacker.Level + 10) / 250f;
        float d = a * move.Base.Power * ((float)attack / defense) +2;
        int damage = Mathf.FloorToInt(d * modifier);
        UpdateHp(damage);
        
        return damageDetails;
    }
    public void UpdateHp(int damage)
    {
        HP=Mathf.Clamp(HP-damage, 0, MaxHP);
        HpChange = true;
    }

    public void SetStatus(ConditionID conditionID)
    {
        Status = ConditionsDB.Conditions[conditionID];
        Status?.OnStart?.Invoke(this);
        StatusChanges.Enqueue($"{Base.Name} {Status.StartMessage}");
    }
    public void CureStatus()
    {
        Status = null;
    }
    public Move GetRandomMove()
    {
        int r = Random.Range(0, Moves.Count);
        return Moves[r];
    }
    public bool OnBeforeMove()
    {
        if (Status?.OnBeforeMove != null)
        {
            return Status.OnBeforeMove(this);
        }
        return true;
    }
    public void OnAfterTurn()
    {
        if (Status != null) // Kiểm tra xem quái có đang bị dính hiệu ứng không
        {
            Status.OnAfterTurn?.Invoke(this);
        }
    }
    public void OnBattleOver()
    {
        ResetStatBoosts();
    }   
}
public class DamageDetails
{
    public bool Fainted { get; set; }
    public float Critical { get; set; }
    public float TypeEffectiveness { get; set; }
}

