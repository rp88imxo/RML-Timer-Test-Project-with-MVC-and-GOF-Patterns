using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RML.Core
{
public abstract class TimerAbstractFactory
{
    public abstract ITimer CreateTimer(float duration,
        Action onComplete,
        Action<float> onTick = null,
        bool looping = false,
        bool useRealTime = false);
}
}


