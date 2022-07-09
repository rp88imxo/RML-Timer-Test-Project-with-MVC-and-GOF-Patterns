using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using RML.Core;
using UnityEngine;

namespace RML.Game
{

public class MainScreenController : IScreen, IInit
{
    // TODO: MOVE TO DATA CONFIG
    private readonly int _defaultCount = 3;
    private readonly float _defaultDuration = 5;
    private readonly int _minCount = 3;
    private readonly int _maxCount = 5;
    // ---------------------------------------------

    private readonly ScreenController _screenController;
    private readonly MainScreenViewPresenter _mainScreenViewPresenter;
    private readonly TimerSaver _timerSaver;
    private readonly EventBus _eventBus;

    private TimersSaveData _timersSaveData;

    private List<MainScreenButtonTimerViewModel>
        _mainScreenButtonTimerViewModels;

  

    public MainScreenController(MainScreenViewPresenter mainScreenViewPresenter, TimerSaver timerSaver,
        ScreenController screenController, EventBus eventBus)
    {
        _mainScreenViewPresenter = mainScreenViewPresenter;
        _timerSaver = timerSaver;
        _screenController = screenController;
        _eventBus = eventBus;
        _mainScreenButtonTimerViewModels =
            new List<MainScreenButtonTimerViewModel>();
    }
    
    public ScreenType ScreenType => ScreenType.MainScreen;

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
        _eventBus.AddListener(Events.TimerCompletedEvent, HandleTimerCompleted);
        
        _mainScreenViewPresenter.Init(HandleAddTimerClick, HandleRemoveTimerClick );
        _mainScreenViewPresenter.Hide();
        RegisterScreen();
    }

    private void HandleTimerCompleted(IPayload obj)
    {
        var payload = (TimerCompletedPayload)obj;
        _mainScreenViewPresenter.HandleTimerCompleted(payload.TimerId);
    }

    private void HandleRemoveTimerClick()
    {
        var count = _mainScreenButtonTimerViewModels.Count;
        
        if (CanBeRemoved(count))
        {
            var lastIndex = _mainScreenButtonTimerViewModels.Count - 1;
            _mainScreenButtonTimerViewModels.RemoveAt(lastIndex);
            
            _timersSaveData.TimerSaveData.Remove(lastIndex);
            
            _timerSaver.Save(_timersSaveData);

            _eventBus.PublishEvent(
                new TimerButtonDeletedPayload(
                    Events.TimerButtonDeletedEvent,
                    lastIndex));
            
            RaiseModelStateChanged();
        }
    }

    private void HandleAddTimerClick()
    {
        var count = _mainScreenButtonTimerViewModels.Count;
        
        if (CanBeAdded(count))
        {
            var newId = _mainScreenButtonTimerViewModels.Count;
            
            _mainScreenButtonTimerViewModels.Add(new MainScreenButtonTimerViewModel
            {
                Id = newId,
                Text = $"Button {newId + 1}",
                ClickCallback = ButtonClickCallback
            });
            
            _timersSaveData.TimerSaveData.Add(newId, new TimerSaveData
            {
                LeftTime = _defaultDuration
            });
            
            _timerSaver.Save(_timersSaveData);
            
            RaiseModelStateChanged();
        }
    }

    private bool CanBeRemoved(int currentCount)
    {
        return currentCount > _minCount;
    }
    
    private bool CanBeAdded(int currentCount)
    {
        return currentCount < _maxCount;
    }

    public UniTask OnShow<T>(T baseScreenData) where T : BaseScreenData
    {
        LoadData();

        RaiseModelStateChanged();
        _mainScreenViewPresenter.Show();
        return UniTask.CompletedTask;
    }

    private void RaiseModelStateChanged()
    {
        _mainScreenButtonTimerViewModels =
            _timersSaveData.TimerSaveData
                .Select(x => new MainScreenButtonTimerViewModel
                {
                    Id = x.Key,
                    Text = $"Button {x.Key + 1}",
                    ClickCallback = ButtonClickCallback
                }).ToList();
        
        _mainScreenViewPresenter.Repaint(_mainScreenButtonTimerViewModels);
    }

    private void ButtonClickCallback(int timerId)
    {
        // Switch to timer screen controoller
        
        _screenController.SwitchToScreenWithScreenData(
            new TimerScreenData(timerId)).Forget();
    }

    public UniTask OnHide()
    {
        _mainScreenViewPresenter.Hide();
        return UniTask.CompletedTask;
    }

    private void LoadData()
    {
        _timersSaveData = _timerSaver.Load();
        if (_timersSaveData == null)
        {
            _timersSaveData = new TimersSaveData
            {
                TimerSaveData = CreateTimersData(_defaultCount,
                    _defaultDuration)
            };

            _timerSaver.Save(_timersSaveData);
        }
    }
    
    private Dictionary<int, TimerSaveData> CreateTimersData(int count, float duration)
    {
        var l = Enumerable
            .Range(0, count)
            .ToDictionary(x => x,
                y => new TimerSaveData {LeftTime = duration});

        return l;
    }
}
}

