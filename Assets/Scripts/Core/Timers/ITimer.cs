using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RML.Core
{
public interface ITimer
{
    event Action OnCompleteCallback;
    event Action<float> OnTickCallback;
    bool IsStarted { get; }
    bool IsDone { get; }
    float LeftTime { get; }
    float Duration { get; set; }
    void Start();
    void Stop();
    void Pause();
    void Resume();
    void Reset();
}
}