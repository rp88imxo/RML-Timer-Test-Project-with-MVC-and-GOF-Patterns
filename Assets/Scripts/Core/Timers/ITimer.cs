using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RML.Core
{
public interface ITimer
{ 
    float Duration { get; }
    void Start();
    void Stop();
    void Pause();
    void Resume();
}
}


