using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RML.Core;
using UnityEngine;

namespace RML.Game
{
public class TimerScreenData : BaseScreenData
{
    public int TimerId { get; }

    public override ScreenType ScreenType { get; set; } =
        ScreenType.TimerScreen;

    public TimerScreenData(int timerId) { TimerId = timerId; }
}

public class TimerWrapper
{
    private readonly int _timerId;
    private readonly Action<int> _onComplete;
    private readonly Action<int, float> _onTick;

    public ITimer Timer { get; private set; }
    public float LeftTime => Timer.LeftTime;
    public bool IsStarted => Timer.IsStarted;

    public float Duration
    {
        get => Timer.Duration;
        set => Timer.Duration = value;
    }

    public TimerWrapper(TimerAbstractFactory timerAbstractFactory,
        int timerId,
        float duration,
        Action<int> onComplete,
        Action<int, float> onTick = null,
        bool looping = false,
        bool useRealTime = false)
    {
        _timerId = timerId;
        _onComplete = onComplete;
        _onTick = onTick;
        Timer = timerAbstractFactory.CreateTimer(duration,
            ONComplete,
            ONTick,
            looping,
            useRealTime);
    }

    public void Start() { Timer.Start(); }

    public void Stop() { Timer.Stop(); }

    public void Reset() { Timer.Reset(); }

    private void ONTick(float timeLeft)
    {
        _onTick?.Invoke(_timerId, timeLeft);
    }

    private void ONComplete() { _onComplete?.Invoke(_timerId); }
}

public class TimerScreenController : IScreen, IInit
{
    private readonly ScreenController _screenController;
    private readonly TimerSaver _timerSaver;
    private readonly TimerAbstractFactory _timerAbstractFactory;
    private readonly TimerScreenViewPresenter _timerScreenViewPresenter;
    private readonly EventBus _eventBus;

    private readonly Dictionary<int, TimerWrapper> _timers;

    private int _currentTimerIndex;
    private ITimer _currentViewTimer;
    private float _currentTimerDuration;

    private TimersSaveData _cachedTimerSavedData;

    public IReadOnlyDictionary<int, TimerWrapper> CurrentTimers
        => _timers;

    public TimerScreenController(ScreenController screenController,
        TimerSaver timerSaver,
        TimerAbstractFactory timerAbstractFactory,
        TimerScreenViewPresenter timerScreenViewPresenter,
        EventBus eventBus)
    {
        _screenController = screenController;
        _timerSaver = timerSaver;
        _timerAbstractFactory = timerAbstractFactory;
        _timerScreenViewPresenter = timerScreenViewPresenter;
        _eventBus = eventBus;
        _timers = new Dictionary<int, TimerWrapper>();
    }

    public ScreenType ScreenType => ScreenType.TimerScreen;

    public void RegisterScreen()
    {
        ScreenController.RegisterScreen(this);
    }

    public void UnregisterScreen()
    {
        ScreenController.UnregisterScreen(this);
    }

    public void Init()
    {
        _eventBus.AddListener(Events.TimerButtonDeletedEvent,
            HandleTimerButtonDeleted);

        _timerScreenViewPresenter.Init(IncreaseTimeCallback,
            DecreaseTimeCallback,
            StartTimerCallback,
            BackButtonClicked);
        _timerScreenViewPresenter.Hide();

        RegisterScreen();
    }

    private void HandleTimerButtonDeleted(IPayload obj)
    {
        var p = (TimerButtonDeletedPayload)obj;

        if (_timers.TryGetValue(p.TimerId, out var timer))
        {
            timer.Stop();
            _timers.Remove(p.TimerId);
        }
    }


    #region VIEW_PRESENTERS_HANDLERS

    private void BackButtonClicked()
    {
        _screenController.BackToPreviousScreen().Forget();
    }

    private void IncreaseTimeCallback(float obj) { ChangeTime(obj); }

    private void DecreaseTimeCallback(float obj) { ChangeTime(-obj); }

    private void ChangeTime(float obj)
    {
        var timer = GetOrCreateTimer();

        _currentTimerDuration =
            Mathf.Clamp(_currentTimerDuration + obj, 0, 100000);

        timer.Duration = _currentTimerDuration;
        _currentViewTimer.Duration = _currentTimerDuration;

        RepaintView(_currentTimerDuration);
    }

    private void StartTimerCallback()
    {
        var timer = GetOrCreateTimer();
        
        timer.Start();

        if (_currentViewTimer.IsDone)
        {
            _currentViewTimer.Reset();
        }

        _currentViewTimer.Duration = _currentTimerDuration;
        _currentViewTimer.Start();

        _screenController.BackToPreviousScreen().Forget();
    }

    private TimerWrapper GetOrCreateTimer()
    {
        if (_timers.TryGetValue(_currentTimerIndex, out var timer))
        {
            if (timer.Timer.IsDone)
            {
                timer.Reset();
            }
        }
        else
        {
            timer = new TimerWrapper(_timerAbstractFactory,
                _currentTimerIndex,
                _currentTimerDuration,
                OnTimerCompleted);
            _timers.Add(_currentTimerIndex, timer);
        }

        return timer;
    }

    #endregion

    public void SaveCurrentData()
    {
        if (_cachedTimerSavedData == null)
        {
            return;
        }

        var data = _cachedTimerSavedData.TimerSaveData;

        foreach (var (key, value) in _timers)
        {
            data[key].LeftTime = value.LeftTime;
        }

        _timerSaver.Save(_cachedTimerSavedData);
    }

    public async UniTask OnShow<T>(T baseScreenData)
        where T : BaseScreenData
    {
        if (!(baseScreenData is TimerScreenData timerScreenData))
        {
            Debug.Log("Wrong Screen Data!");
            _screenController.BackToPreviousScreen().Forget();
            return;
        }

        _currentTimerIndex = timerScreenData.TimerId;

        _currentViewTimer?.Stop();

        if (!_timers.TryGetValue(_currentTimerIndex, out var timer))
        {
            LoadData();

            var data =
                _cachedTimerSavedData.TimerSaveData[_currentTimerIndex];

            _currentTimerDuration = data.LeftTime;

            timer = new TimerWrapper(_timerAbstractFactory,
                _currentTimerIndex,
                _currentTimerDuration,
                OnTimerCompleted);

            _timers.Add(_currentTimerIndex, timer);

            _currentViewTimer =
                _timerAbstractFactory.CreateTimer(_currentTimerDuration,
                    OnViewTimerCompleted,
                    OnViewTimerTick);

            ToggleButtons(true);
            RepaintView(_currentTimerDuration);
        }
        else
        {
            _currentTimerDuration = timer.LeftTime;

            var isTimerStarted = timer.IsStarted;

            if (isTimerStarted)
            {
                _currentViewTimer =
                    _timerAbstractFactory.CreateTimer(
                        _currentTimerDuration,
                        OnViewTimerCompleted,
                        OnViewTimerTick);

                _currentViewTimer.Start();
            }
            else
            {
                RepaintView(_currentTimerDuration);
            }

            ToggleButtons(!isTimerStarted);
        }


        await _timerScreenViewPresenter.ShowAsync();
    }

    private void OnViewTimerCompleted()
    {
        ToggleButtons(true);
        _currentTimerDuration = 0f;
        RepaintView(_currentTimerDuration);
    }

    private void OnViewTimerTick(float left) { RepaintView(left); }

    private void RepaintView(float left)
    {
        _timerScreenViewPresenter.Repaint(new TimerScreenViewModel
        {
            LeftTime = TimeSpan.FromSeconds(left)
        });
    }

    private void ToggleButtons(bool state)
    {
        _timerScreenViewPresenter.ToggleButtons(state);
    }

    private void OnTimerCompleted(int timerId)
    {
        _eventBus.PublishEvent(new TimerCompletedPayload(
            Events.TimerCompletedEvent,
            timerId));
        
        _timers.Remove(timerId);
    }

    private void LoadData()
    {
        _cachedTimerSavedData = _timerSaver.Load();
    }

    public async UniTask OnHide()
    {
        await _timerScreenViewPresenter.HideAsync();
    }
}
}