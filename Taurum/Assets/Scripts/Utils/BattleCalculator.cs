using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public enum TValue
{
    Base,
    Current,
}

public enum TAttribute
{
    Strenght,
    Agility,
    Intelligence,
}

public enum TStat
{
    HP,
    MP,

    SPD,
    DOD,

    PAtq,
    RAtq,
    MAtq,
    
    PDef,
    RDef,
    MDef,
}

public enum TAttackType
{
    Physical,
    Ranged,
    Magical,
}

public static class BattleCalculator
{
    public static bool GetHit(Dictionary<TValue, TStat, int> attacker, Dictionary<TValue, TStat, int> defender)
    {
        int chance = 90 - (defender[TValue.Current, TStat.DOD] - attacker[TValue.Current, TStat.DOD]);
        if (chance <= 0)
            return false;
        if (chance >= 100)
            return true;
        return Global.random.Next(0, 100) < chance;
    }

    public static int GetDamage(Dictionary<TValue, TStat, int> attacker, Dictionary<TValue, TStat, int> defender, TAttackType type)
    {
        int att = 0;
        int def = 0;
        switch (type)
        {
            case TAttackType.Physical:
                att = attacker[TValue.Current, TStat.PAtq];
                def = defender[TValue.Current, TStat.PDef];
                break;
            case TAttackType.Ranged:
                att = attacker[TValue.Current, TStat.RAtq];
                def = defender[TValue.Current, TStat.RDef];
                break;
            case TAttackType.Magical:
                att = attacker[TValue.Current, TStat.MAtq];
                def = defender[TValue.Current, TStat.MDef];
                break;
        }

        int damage = att - def;
        return damage > 0 ? damage : 0;
    }
}
