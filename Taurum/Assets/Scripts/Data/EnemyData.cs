using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class EnemyData
{
    public string name { get; set; }

    public int level { get; private set; }
    
    public Dictionary<TValue, TStat, int> stats { get; private set; }
    
    public EnemyData()
    {
        stats = new Dictionary<TValue, TStat, int>();
        
        name = "Creature";
        level = 1;
    }

    public void Calculate()
    {
        // sets the current base
        stats[TValue.Current, TStat.HP] = stats[TValue.Base, TStat.HP];
        stats[TValue.Current, TStat.MP] = stats[TValue.Base, TStat.MP];
        stats[TValue.Current, TStat.SPD] = stats[TValue.Base, TStat.SPD];
        stats[TValue.Current, TStat.DOD] = stats[TValue.Base, TStat.DOD];
        stats[TValue.Current, TStat.PAtq] = stats[TValue.Base, TStat.PAtq];
        stats[TValue.Current, TStat.RAtq] = stats[TValue.Base, TStat.RAtq];
        stats[TValue.Current, TStat.MAtq] = stats[TValue.Base, TStat.MAtq];
        stats[TValue.Current, TStat.PDef] = stats[TValue.Base, TStat.PDef];
        stats[TValue.Current, TStat.RDef] = stats[TValue.Base, TStat.RDef];
        stats[TValue.Current, TStat.MDef] = stats[TValue.Base, TStat.MDef];

        // adds bonuses and stuff
        //TODO

        // adds debuffs and stuff (including HP and MP reduction)
        //TODO
    }

    public int Current(TStat stat)
    {
        return stats[TValue.Current, stat];
    }
}
