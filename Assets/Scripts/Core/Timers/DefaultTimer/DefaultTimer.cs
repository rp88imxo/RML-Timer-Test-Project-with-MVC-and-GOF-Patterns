using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using JetBrains.Annotations;
using Object = UnityEngine.Object;

namespace RML.Core
{
public class DefaultTimer : ITimer
{
    #region PUBLIC_PROPERTIES_API

    public event  Action OnCompleteCallback;
    public event  Action<float> OnTickCallback;
    
    public float LeftTime => GetLeftTimeFromStart();
    
    public float Duration { get; set; }

    public bool Looping { get; set; }

    public bool IsCompleted { get; private set; }

    public bool UseRealTime { get; private set; }

    public bool IsPaused => _timeElapsedBeforePause.HasValue;

    public bool IsCancelled => _timeElapsedBeforeCancel.HasValue;

    public bool IsDone => IsCompleted || IsCancelled;

    public bool IsStarted { get; private set; }

    #endregion

    #region PUBLIC_STATIC_API

    public static DefaultTimer CreateTimer(float duration,
        Action onComplete,
        Action<float> onTick = null,
        bool looping = false,
        bool useRealTime = false)
    {
        if (_timerController == null)
        {
            var managerInScene =
                Object.FindObjectOfType<DefaultTimerController>();
            if (managerInScene != null)
            {
                _timerController = managerInScene;
            }
            else
            {
                var managerObject =
                    new GameObject {name = "TimerManager"};
                _timerController = managerObject
                    .AddComponent<DefaultTimerController>();
            }
        }

        var defaultTimer = new DefaultTimer(duration,
            onComplete,
            onTick,
            looping,
            useRealTime);

        _timerController.AddTimer(defaultTimer);

        return defaultTimer;
    }

    #endregion

    #region PUBLIC_METHODS_API

    public void Start()
    {
        IsStarted = true;
        
        _startTime = GetTotalTimePassed();
        _lastUpdateTime = _startTime;
    }

    public void Stop()
    {
        if (IsDone) return;

        _timeElapsedBeforeCancel = GetPassedTimeFromStart();
        _timeElapsedBeforePause = null;
    }

    public void Pause()
    {
        if (IsPaused || IsDone || !IsStarted) return;

        _timeElapsedBeforePause = GetPassedTimeFromStart();
    }

    public void Resume()
    {
        if (!IsPaused || IsDone || !IsStarted) return;

        _timeElapsedBeforePause = null;
    }

    public void Reset()
    {
        IsStarted = false;
        IsCompleted = false;
        _timeElapsedBeforeCancel = null;

        _startTime = GetTotalTimePassed();
        _lastUpdateTime = _startTime;

        _timerController.AddTimer(this);
    }

    public float GetPassedTimeFromStart()
    {
        if (IsCompleted || GetTotalTimePassed() >= GetEndTime())
            return Duration;

        return _timeElapsedBeforeCancel
            ?? _timeElapsedBeforePause
            ?? GetTotalTimePassed() - _startTime;
    }

    public float GetLeftTimeFromStart()
    {
        if (!IsStarted)
        {
            return Duration;
        }
        
        return Duration - GetPassedTimeFromStart();
    }

    #endregion

    private static DefaultTimerController _timerController;

    #region INTERNAL_PRIVATE

    private float _startTime;
    private float _lastUpdateTime;

    private float? _timeElapsedBeforeCancel;
    private float? _timeElapsedBeforePause;

    #endregion

    private DefaultTimer(float duration,
        Action onCompleteCallback,
        Action<float> onTickCallback,
        bool looping,
        bool useRealTime)
    {
        Duration = duration;
        OnCompleteCallback = onCompleteCallback;
        OnTickCallback = onTickCallback;

        Looping = looping;
        UseRealTime = useRealTime;
    }


    private float GetTotalTimePassed()
    {
        return UseRealTime ? Time.realtimeSinceStartup : Time.time;
    }

    private float GetEndTime() { return _startTime + Duration; }

    private float GetDeltaTime()
    {
        return GetTotalTimePassed() - _lastUpdateTime;
    }
    

    public void Update()
    {
        if (IsDone || !IsStarted) return;

        if (IsPaused)
        {
            _startTime += GetDeltaTime();
            _lastUpdateTime = GetTotalTimePassed();
            return;
        }

        _lastUpdateTime = GetTotalTimePassed();

        OnTickCallback?.Invoke(GetLeftTimeFromStart());

        if (GetTotalTimePassed() >= GetEndTime())
        {
            OnCompleteCallback?.Invoke();

            if (Looping)
            {
                _startTime = GetTotalTimePassed();
            }
            else
            {
                IsStarted = false;
                IsCompleted = true;
            }
        }
    }
}
}