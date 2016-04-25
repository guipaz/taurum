using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class TickableVisual : MonoBehaviour
{
    public Vector2 position;
    public int speed;

    public delegate void TickHandler(TickableVisual visual);
    public event TickHandler EntityTick;

    public virtual void Tick()
    {
        if (EntityTick != null)
            EntityTick(this);
    }
}
