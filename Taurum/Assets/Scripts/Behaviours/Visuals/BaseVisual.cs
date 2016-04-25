using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class BaseVisual : MonoBehaviour
{
    public Vector2 position;

    public delegate void TickHandler(BaseVisual visual);
    public event TickHandler EntityTick;

    public virtual void Action()
    {
        
    }

    public virtual void Tick()
    {
        if (EntityTick != null)
            EntityTick(this);
    }

    public virtual int GetSpeed()
    {
        return 1;
    }

    public virtual int GetLevel()
    {
        return 1;
    }
}
