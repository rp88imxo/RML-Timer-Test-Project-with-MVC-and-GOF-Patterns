using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RML.Core;
using UnityEngine;

namespace RML.Game
{

// IOC, should be replaced with some di container in real project like Zenject or StrangeIOC
public class GameInitializer : MonoBehaviour
{
    [SerializeField]
    private TimerScreenViewPresenter _timerScreenViewPresenter;

    [SerializeField]
    private MainScreenViewPresenter _mainScreenViewPresenter;

    private TimerScreenController _timerScreenController;
    private ScreenController _screenController;
    private MainScreenController _mainScreenController;
    private TimerSaver _timerSaver;

    private EventBus _eventBus;
    private TimerAbstractFactory _timerAbstractFactory;
    
    private void Awake()
    {
        _timerSaver = new TimerSaver();
        _screenController = new ScreenController();
        _timerAbstractFactory = new DefaultTimerFactory();
        _eventBus = new EventBus();

        _mainScreenController = new MainScreenController(
            _mainScreenViewPresenter,
            _timerSaver,
            _screenController,
            _eventBus);
        
        _timerScreenController = new TimerScreenController(
            _screenController,
            _timerSaver,
            _timerAbstractFactory,
            _timerScreenViewPresenter,
            _eventBus);
    }

    private void Start()
    {
        _mainScreenController.Init();
        _timerScreenController.Init();

        _screenController.SwitchToScreen(ScreenType.MainScreen).Forget();
    }

    private void OnApplicationQuit()
    {
        _timerScreenController.SaveCurrentData();
    }
}
}


