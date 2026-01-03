using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Move", menuName = "Monster/Create New Move")]
public class MoveBase : ScriptableObject
{
    [SerializeField] string _name;
    [TextArea] 
    [SerializeField] string description;
    [SerializeField] MonsterType type;
    [SerializeField] int power;
    [SerializeField] int accuracy;
    [SerializeField] int pp;
    [SerializeField] MoveCategory category;
    [SerializeField] MoveEffect effects;
    [SerializeField] MoveTarget target;
    public string Name
    {
        get { return _name; }
    }
    public string Description
    {
        get { return description; }
    }
    public MonsterType Type
    {
        get { return type; }
    }
    public int Power
    {
        get { return power; }
    }
    public int Accuracy
    {
        get { return accuracy; }
    }
    public int PP
    {
        get { return pp; }
    }

    public MoveCategory Category
    {
        get { return category; }
    }

    public MoveEffect Effects
    { 
        get { return effects; } 
    }
    public MoveTarget Target
    {
        get { return target; }
    }


}
[System.Serializable]
public class MoveEffect 
{
    [SerializeField] List<StatBoost> boosts;
    [SerializeField] ConditionID status;

    public List<StatBoost>Boosts
    {
        get { return boosts; }
    }
    public ConditionID Status
    {
        get { return status; }
    }
}
[System.Serializable]
public class StatBoost
{
    public Stat stat;
    public int boost;
}
public enum MoveCategory
{
    Physical,
    Special,
    Status
}
public enum MoveTarget 
{     
    Foe,
    Self
}