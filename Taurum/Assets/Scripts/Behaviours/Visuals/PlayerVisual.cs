using UnityEngine;
using System.Collections;
using System;

public class PlayerVisual : BaseVisual
{
    public PlayerData data { get; set; }

    public delegate void PlayerAttackedHandler(PlayerVisual visual);
    public event PlayerAttackedHandler PlayerAttacked;

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
        if (PlayerAttacked != null)
            PlayerAttacked(this);
    }
}
