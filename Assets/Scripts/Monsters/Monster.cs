using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
[System.Serializable]
public class Monster
{
    [SerializeField] MonsterBase _base;
    [SerializeField] int level;
    public event System.Action OnHPChanged;
    public Monster(MonsterBase pBase,int plevel)
    {
        _base=pBase;
        level = plevel;
        Init();
    }
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
    public int EXP { get; set; }
    public int HP { get; set; }
    public List<Move> Moves { get; set; }
    public Move CurrentMove { get; set; }
    public Dictionary<Stat, int> Stats{ get; private set; }
    public Dictionary<Stat, int> StatBoosts { get; private set; }

    public Condition Status {  get;private set; }

    public int StatusTime { get; set; }

    public Condition VolatileStatus { get; private set; }
    public int VolatileStatusTime { get; set; }

    public Queue<string> StatusChanges { get; private set; }
    public bool HpChange { get;  set; }
    public event System.Action OnStatusChange;

    
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
            if(Moves.Count >= MonsterBase.MaxNumOfMoves)
            {
                break;
            }
        }
        EXP =Base.GetExpForLevel(Level);
        CalculateStats();    
        HP = MaxHP;
        StatusChanges = new Queue<string>();
        ResetStatBoosts();
        Status = null;
        VolatileStatus = null;
        

    }
    public float GetTotalHealTimeLeft()
    {
        // Nếu máu đã đầy, trả về 0
        if (HP >= MaxHP) return 0;
        float timeToHeal1HP = 1f + (Level * 0.2f); // Công thức tốc độ hồi (phải khớp với HandlePassiveRegen)
        int missingHP = MaxHP - HP; 
        float timeForCurrentHP = Mathf.Max(0, timeToHeal1HP - regenTimer);
        if (missingHP > 1)
        {
            return timeForCurrentHP + ((missingHP - 1) * timeToHeal1HP);
        }
        else
        {
            // Nếu chỉ thiếu đúng 1 giọt
            return timeForCurrentHP;
        }
    }
    public float GetTimeUntilNextHeal()
    {
        // Nếu máu đầy hoặc chết (tùy logic bạn chọn), trả về 0
        if (HP >= MaxHP) return 0;

        float timeToHeal1HP = 1f + (Level * 0.2f); // Công thức phải khớp với hàm HandlePassiveRegen
        return Mathf.Max(0, timeToHeal1HP - regenTimer);
    }
    private float regenTimer = 0f;
    public void HandlePassiveRegen()
    {
        // 1. Chỉ dừng lại nếu Máu đã đầy
        if (HP >= MaxHP)
        {
            regenTimer = 0;
            return;
        }
        float timeToHeal1HP = 1f + (Level * 0.2f);
        regenTimer += Time.deltaTime;
        if (regenTimer >= timeToHeal1HP)
        {
            HP++;
            regenTimer = 0;
            // 2. GỌI SỰ KIỆN: Báo cho UI biết máu đã tăng
            OnHPChanged?.Invoke();
            if (HP == 1)
            {
                CureStatus();
                CureVolatileStatus();
                // Gọi thêm lần nữa để cập nhật trạng thái nếu cần
                OnHPChanged?.Invoke();
            }
        }
    }
    public void CycleMove(Move moveUsed)
    {
        // Kiểm tra xem chiêu vừa dùng có trong danh sách không
        if (Moves.Contains(moveUsed))
        {
            Moves.Remove(moveUsed); // Xóa khỏi vị trí hiện tại
            Moves.Add(moveUsed);    // Thêm vào cuối danh sách
        }
    }
    public Monster(MonsterSaveData saveData)
    {
        HP = saveData.hp;
        level = saveData.lv;
        EXP = saveData.exp;
        _base=MonsterDB.GetMonsterByName(saveData.name);
        Moves =saveData.moves.Select(s=>new Move(s)).ToList();
        CalculateStats();
        StatusChanges = new Queue<string>();
        ResetStatBoosts();
        Status = null;
        VolatileStatus = null;
    }
    public MonsterSaveData GetSaveData()
    {
        var saveData = new MonsterSaveData()
        {
            name=Base.Name,
            hp=HP,
            exp=EXP,
            lv=Level,
            moves=Moves.Select(m=>m.GetSaveData()).ToList(),
        };
        return saveData;
    }
    void CalculateStats()
    {
        Stats = new Dictionary<Stat, int>();

        // --- CẤU HÌNH SỨC MẠNH ---
        // 1.035f nghĩa là tăng 3.5% mỗi cấp (Lãi kép).
        // Level 13 ~ x1.5 lần Base
        // Level 27 ~ x2.5 lần Base (Mạnh gấp rưỡi Level 13)
        float atkgrowthRate = 1.1f;
        float defgrowthRate = 1.05f;

        Stats.Add(Stat.Attack, Mathf.FloorToInt(Base.Attack * Mathf.Pow(atkgrowthRate, Level)) + 5);
        Stats.Add(Stat.Defense, Mathf.FloorToInt(Base.Defense * Mathf.Pow(defgrowthRate, Level)) + 5);
        Stats.Add(Stat.SpAttack, Mathf.FloorToInt(Base.SpAttack * Mathf.Pow(atkgrowthRate, Level)) + 5);
        Stats.Add(Stat.SpDefense, Mathf.FloorToInt(Base.SpDefense * Mathf.Pow(defgrowthRate, Level)) + 5);
        Stats.Add(Stat.Speed, Mathf.FloorToInt(Base.Speed * Mathf.Pow(atkgrowthRate, Level)) + 5);

        // --- CÔNG THỨC MÁU (HP) ---

        float hpMultiplier = Mathf.Pow(1.03f, Level); // HP tăng 4% mỗi cấp
        MaxHP = Mathf.FloorToInt((Base.MaxHP * hpMultiplier) + (Level * 2) + 10);
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
            { Stat.Accuracy, 0 },
            {Stat.Evasion, 0 },
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

    public bool CheckForLevelUp()
    {
        if(EXP>Base.GetExpForLevel(level + 1))
        {
            level++;
            return true;

        }
        return false;
    }
    public LearnableMove GetLearnableMoveAtCurrLevel()
    {
        return Base.LearnableMoves.Where(x => x.Level == level).FirstOrDefault();
    }
    public void LearnMove(LearnableMove moveToLearn)
    {
        if (Moves.Count > MonsterBase.MaxNumOfMoves) return;
        Moves.Add(new Move(moveToLearn.Base));
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
        DecreaseHP(damage);
        
        return damageDetails;
    }
    public void DecreaseHP(int damage)
    {
        HP=Mathf.Clamp(HP-damage, 0, MaxHP);
        HpChange = true;
    }
    public void IncreaseHP(int amount)
    {
        HP = Mathf.Clamp(HP + amount, 0, MaxHP);
        HpChange = true;
    }
    // item hàm

    // Hàm hồi phục toàn bộ PP (như Elixir/Max Elixir)
    public void HealAllPP()
    {
        foreach (var move in Moves)
        {
            move.PP = move.Base.PP;
        }
    }
    //  PP cho tất cả các chiêu một lượng nhỏ
    public void RestorePP(int amount)
    {
        foreach (var move in Moves)
        {
            move.PP = Mathf.Clamp(move.PP + amount, 0, move.Base.PP);
        }
    }

    // Kiểm tra xem Monster có cần hồi PP không (để tránh dùng phí item)
    public bool NeedsPPHeal()
    {
        foreach (var move in Moves)
        {
            if (move.PP < move.Base.PP) return true;
        }
        return false;
    }

    public void SetStatus(ConditionID conditionID)
    {
        if (Status != null) return;
        Status = ConditionsDB.Conditions[conditionID];
        Status?.OnStart?.Invoke(this);
        StatusChanges.Enqueue($"{Base.Name} {Status.StartMessage}");
        OnStatusChange?.Invoke();
    }
    public void CureStatus()
    {
        Status = null;
        OnStatusChange?.Invoke();
    }
    public void SetVolatileStatus(ConditionID conditionID)
    {
        if (VolatileStatus != null) return;
        VolatileStatus = ConditionsDB.Conditions[conditionID];
        VolatileStatus?.OnStart?.Invoke(this);
        StatusChanges.Enqueue($"{Base.Name} {VolatileStatus.StartMessage}");
        
    }
    public void CureVolatileStatus()
    {
        VolatileStatus = null;
    
    }
    public Move GetRandomMove()
    {
        // neu enemy ma het chieu co trong pp thi no se dung dc cac chieu khac vo han, de phong nguoi choi danh ma con nay no het chieu va pp
        var moveWithPP = Moves.Where(x=>x.PP>0).ToList();
        if (moveWithPP.Count > 0)
        {
            int r = Random.Range(0, moveWithPP.Count);
            return moveWithPP[r];
        }
        else
        {
            int r =Random.Range(0, Moves.Count);
            return Moves[r];
        }
    }
    public bool OnBeforeMove()
    {
        bool canPerformMove = true;
        if (Status?.OnBeforeMove != null)
        {
            if (!Status.OnBeforeMove(this))
            {
                canPerformMove = false;
            }
        }
        if (VolatileStatus?.OnBeforeMove != null)
        {
            if (!VolatileStatus.OnBeforeMove(this))
            {
                canPerformMove = false;
            }
        }
        return canPerformMove;
    }
    public void OnAfterTurn()
    {
        if (Status != null) // Kiểm tra xem quái có đang bị dính hiệu ứng không
        {
            Status.OnAfterTurn?.Invoke(this);
        }
        if (VolatileStatus != null) // Kiểm tra xem quái có đang bị dính hiệu ứng không
        { 
            VolatileStatus.OnAfterTurn?.Invoke(this);
        }
    }
    public void OnBattleOver()
    {
        VolatileStatus = null;
        ResetStatBoosts();
    }   
}
public class DamageDetails
{
    public bool Fainted { get; set; }
    public float Critical { get; set; }
    public float TypeEffectiveness { get; set; }
}
[Serializable]
public class MonsterSaveData
{
    public string name;
    public int hp;
    public int lv;
    public int exp;
    public List<MoveSaveData> moves;
}
