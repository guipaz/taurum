using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

[Serializable]
public class PlayerData
{
    public string name { get; set; }

    public int level { get; private set; }
    public int exp { get; private set; }
    public int points { get; private set; }

    public Dictionary<TValue, TAttribute, int> attributes { get; private set; }
    public Dictionary<TValue, TStat, int> stats { get; private set; }

    public PlayerData()
    {
        attributes = new Dictionary<TValue, TAttribute, int>();
        stats = new Dictionary<TValue, TStat, int>();

        name = "Unknown";
        level = 1;
        exp = 0;
    }

    public void AddAttribute(TAttribute att, int value)
    {
        attributes[TValue.Base, att] += value;
        Calculate();
    }

    public void Calculate()
    {
        // calculates level
        int expNeeded = (int)((level + level * 0.75f) * 100);
        while (exp >= expNeeded && exp > 0)
        {
            level++;
            points += 3;
            exp -= expNeeded;
        }

        CalculateAttributes();
        CalculateStats();
    }
    
    void CalculateAttributes()
    {
        // sets current values
        attributes[TValue.Current, TAttribute.Strenght] = attributes[TValue.Base, TAttribute.Strenght];
        attributes[TValue.Current, TAttribute.Agility] = attributes[TValue.Base, TAttribute.Agility];
        attributes[TValue.Current, TAttribute.Intelligence] = attributes[TValue.Base, TAttribute.Intelligence];

        // adds bonuses and stuff
        //TODO

        // adds debuffs and stuff
        //TODO
    }

    private void CalculateStats()
    {
        // calculates the bases
        stats[TValue.Base, TStat.HP] = attributes[TValue.Base, TAttribute.Strenght] * 3 + level;
        stats[TValue.Base, TStat.MP] = attributes[TValue.Base, TAttribute.Intelligence] * 4 + level;
        stats[TValue.Base, TStat.SPD] = attributes[TValue.Base, TAttribute.Agility] * 3 + level;
        stats[TValue.Base, TStat.DOD] = attributes[TValue.Base, TAttribute.Agility] * 2 + level;
        stats[TValue.Base, TStat.PAtq] = attributes[TValue.Base, TAttribute.Strenght] * 2;
        stats[TValue.Base, TStat.RAtq] = attributes[TValue.Base, TAttribute.Agility] * 2;
        stats[TValue.Base, TStat.MAtq] = attributes[TValue.Base, TAttribute.Intelligence] * 3;

        // sets current values
        stats[TValue.Current, TStat.HP] = stats[TValue.Base, TStat.HP];
        stats[TValue.Current, TStat.MP] = stats[TValue.Base, TStat.MP];
        stats[TValue.Current, TStat.SPD] = stats[TValue.Base, TStat.SPD];
        stats[TValue.Current, TStat.DOD] = stats[TValue.Base, TStat.DOD];
        stats[TValue.Current, TStat.PAtq] = stats[TValue.Base, TStat.PAtq];
        stats[TValue.Current, TStat.RAtq] = stats[TValue.Base, TStat.RAtq];
        stats[TValue.Current, TStat.MAtq] = stats[TValue.Base, TStat.MAtq];

        // adds bonuses and stuff
        //TODO

        //stats[TValue.Calculated, TStat.PDef] = stats[TValue.Base, TStat.HP];
        //stats[TValue.Calculated, TStat.RDef] = stats[TValue.Base, TStat.HP];
        //stats[TValue.Calculated, TStat.MDef] = stats[TValue.Base, TStat.HP];

        // adds debuffs and stuff (HP and MP reduction too)
        //TODO
    }

    public int Current(TAttribute attr)
    {
        return attributes[TValue.Current, attr];
    }

    public int Current(TStat stat)
    {
        return stats[TValue.Current, stat];
    }
}
