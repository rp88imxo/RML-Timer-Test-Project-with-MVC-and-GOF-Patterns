using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace RML.Core
{
public class DefaultTimerController : MonoBehaviour
{
    private List<DefaultTimer> _timers = new List<DefaultTimer>();

    private List<DefaultTimer>
        _newTimers = new List<DefaultTimer>();

    public void AddTimer(DefaultTimer defaultTimer)
    {
        _newTimers.Add(defaultTimer);
    }

    public void StopAllTimers()
    {
        foreach (var timer in _timers)
            timer.Stop();

        _timers = new List<DefaultTimer>();
        _newTimers = new List<DefaultTimer>();
    }

    public void PauseAllTimers()
    {
        foreach (var timer in _timers) 
            timer.Pause();
    }

    public void ResumeAllTimers()
    {
        foreach (var timer in _timers)
            timer.Resume();
    }

    private void Update()
    {
        UpdateAllTimers();
    }

    private void UpdateAllTimers()
    {
        if (_newTimers.Count > 0)
        {
            _timers.AddRange(_newTimers);
            _newTimers.Clear();
        }

        foreach (var timer in _timers)
            timer.Update();

        _timers.RemoveAll(t => t.IsDone);
    }
}
}