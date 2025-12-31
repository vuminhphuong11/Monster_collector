using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "Monster", menuName = "Monster/Create New Monster")]
public class MonsterBase : ScriptableObject
{
    [SerializeField] string _name;
    [TextArea]
    [SerializeField] string description;
    [SerializeField] Sprite leftSprite;
    [SerializeField] Sprite rightSprite;
    [SerializeField] MonsterType type1;
    [SerializeField] MonsterType type2;
    // base stats
    [SerializeField] int maxHP;
    [SerializeField] int attack;
    [SerializeField] int defense;
    [SerializeField] int spAttack;
    [SerializeField] int spDefense;
    [SerializeField] int speed;
    [SerializeField] List<LearnableMove> learnableMoves;

    public string Name
    {
        get { return _name; }
    }
    public string Description
    {
        get { return description; }
    }
    public Sprite FrontSprite
    {
        get { return leftSprite; }
    }
    public Sprite BackSprite
    {
        get { return rightSprite; }
    }
    public MonsterType Type1
    {
        get { return type1; }
    }
    public MonsterType Type2
    {
        get { return type2; }
    }
    public int MaxHP
    {
        get { return maxHP; }
    }
    public int Attack
    {
        get { return attack; }
    }
    public int Defense
    {
        get { return defense; }
    }
    public int SpAttack
    {
        get { return spAttack; }
    }
    public int SpDefense
    {
        get { return spDefense; }
    }   
    public int Speed
    {
        get { return speed; }
    }
    public List<LearnableMove> LearnableMoves
    {
        get { return learnableMoves; }
    }
}
[System.Serializable]
public class LearnableMove
{
    [SerializeField] MoveBase moveBase;
    [SerializeField] int level;
    public MoveBase Base
    {
        get { return moveBase; }
    }
    public int Level
    {
        get { return level; }
    }
}
public enum MonsterType
{
    None,
    Fire,
    Water,
    Grass,
    Metal,
    Earth
}
public class TypeChart
{
    static float[][] chart =
    {
        //                       NONE   FIRE    WATER   GRASS   METAL   EARTH
        /* NONE  */ new float[] {1.0f,  1.0f,   1.0f,   1.0f,   1.0f,   1.0f},
        /* FIRE  */ new float[] {1.0f,  0.8f,   0.8f,   1.5f,   1.0f,   1.0f},
        /* WATER */ new float[] {1.0f,  1.5f,   0.8f,   0.8f,   1.0f,   1.0f},
        /* GRASS */ new float[] {1.0f,  0.8f,   1.5f,   0.8f,   1.0f,   1.5f},
        /* METAL */ new float[] {1.0f,  1.0f,   1.0f,   1.0f,   0.8f,   1.5f},
        /* EARTH */ new float[] {1.0f,  1.0f,   1.5f,   0.8f,   0.8f,   1.0f}
    };
    public static float GetEffectiveness(MonsterType attackType, MonsterType defenseType)
    {
        if (attackType == MonsterType.None || defenseType == MonsterType.None)
            return 1f;
        int row = (int)attackType;
        int col = (int)defenseType;
        return chart[row][col];
    }
}
