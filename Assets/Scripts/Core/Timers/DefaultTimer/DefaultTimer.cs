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

    public float Duration { get; private set; }

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
        Action<float> onUpdate = null,
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
            onUpdate,
            looping,
            useRealTime);

        _timerController.AddTimer(defaultTimer);

        return defaultTimer;
    }

    #endregion

    #region PUBLIC_METHODS_API

    public void Start() { IsStarted = true; }

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
        return Duration - GetPassedTimeFromStart();
    }

    #endregion

    private static DefaultTimerController _timerController;

    #region INTERNAL_PRIVATE

    private readonly Action _onCompleteCallback;
    private readonly Action<float> _onUpdateCallback;
    private float _startTime;
    private float _lastUpdateTime;

    private float? _timeElapsedBeforeCancel;
    private float? _timeElapsedBeforePause;

    #endregion

    private DefaultTimer(float duration,
        Action onCompleteCallback,
        Action<float> onUpdateCallback,
        bool looping,
        bool useRealTime)
    {
        Duration = duration;
        _onCompleteCallback = onCompleteCallback;
        _onUpdateCallback = onUpdateCallback;

        Looping = looping;
        UseRealTime = useRealTime;

        _startTime = GetTotalTimePassed();
        _lastUpdateTime = _startTime;
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

        _onUpdateCallback?.Invoke(GetPassedTimeFromStart());

        if (GetTotalTimePassed() >= GetEndTime())
        {
            _onCompleteCallback.Invoke();

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