using UnityEngine;
using System.Collections;
using System;

public class EnemyVisual : BaseVisual
{
    public EnemyData data { get; set; }
    
    public delegate void EnemyAttackedHandler(EnemyVisual visual);
    public delegate void EnemyKilledHandler(EnemyVisual visual);

    public event EnemyAttackedHandler EnemyAttacked;
    public event EnemyKilledHandler EnemyKilled;

    public override int GetSpeed()
    {
        return data.stats[TValue.Current, TStat.SPD];
    }

    public override int GetLevel()
    {
        return data.level;
    }

    public override void Action()
    {
        if (EnemyAttacked != null)
            EnemyAttacked(this);
    }

    public void ReceiveDamage(int damage)
    {
        if (data.Current(TStat.HP) - damage <= 0)
        {
            //TODO better calculation, increasing according to the HP loss
            bool dead = Global.random.Next(0, 50) < 50;
            if (dead)
            {
                if (EnemyKilled != null)
                    EnemyKilled(this);
                return;
            }
        }

        data.stats[TValue.Current, TStat.HP] -= damage;
    }
}
