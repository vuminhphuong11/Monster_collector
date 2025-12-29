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

    public bool IsSpecial()
    {
        if (type == MonsterType.Metal || type == MonsterType.Water ||
            type == MonsterType.Fire || type == MonsterType.Grass)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

}
