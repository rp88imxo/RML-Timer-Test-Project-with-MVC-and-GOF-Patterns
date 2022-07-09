using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RML.Core
{

public struct TimerScreenViewModel
{
    public TimeSpan LeftTime { get; set; }
}

public class TimerScreenViewPresenter : View
{
    [SerializeField]
    private Button _decreaseTimeButton;

    [SerializeField]
    private Button _startTimerButton;
    
    [SerializeField]
    private Button _increaseTimeButton;

    [SerializeField]
    private Button _backButton;
    
    [SerializeField]
    private TMP_Text _timerText;

    [SerializeField]
    private Transform _timerPanelRoot;
    
    private Action<float> _increaseTimeCallback;
    private Action<float> _decreaseTimeCallback;
    private Action _startTimerCallback;

    private float _changeTimeValue = 0.1f; //in seconds
    
    public void Init(
        Action<float> increaseTimeCallback,
        Action<float> decreaseTimeCallback,
        Action startTimerCallback, Action backButtonClicked)
    {
        _increaseTimeCallback = increaseTimeCallback;
        _decreaseTimeCallback = decreaseTimeCallback;
        _startTimerCallback = startTimerCallback;
        
        _decreaseTimeButton.onClick.AddListener(HandleDecreaseClicked);
        _startTimerButton.onClick.AddListener(HandleStartClicked);
        _increaseTimeButton.onClick.AddListener(HandleIncreaseClicked);
        _backButton.onClick.AddListener(backButtonClicked.Invoke);
    }

    private void HandleIncreaseClicked()
    {
        _increaseTimeCallback.Invoke(_changeTimeValue);
    }

    private void HandleStartClicked()
    {
        _startTimerCallback.Invoke();
    }

    private void HandleDecreaseClicked()
    {
        _decreaseTimeCallback.Invoke(_changeTimeValue);
    }

    public void Repaint(TimerScreenViewModel timerScreenViewModel)
    {
        _timerText.text =
            timerScreenViewModel.LeftTime.ToString("mm\\:ss\\:ff");
    }

    public void ToggleButtons(bool state)
    {
        _decreaseTimeButton.interactable = state;
        _startTimerButton.interactable = state;
        _increaseTimeButton.interactable = state;
    }

    public async UniTask HideAsync()
    {
        ToggleButtons(false);
        _backButton.interactable = false;
        
        await _timerPanelRoot.DOPunchScale(Vector3.down, 0.5f);
        
        _backButton.interactable = true;
        ToggleButtons(true);
        
        Hide();
    }
    
    public UniTask ShowAsync()
    {
        Show();
        return UniTask.CompletedTask;
    }
}

}
