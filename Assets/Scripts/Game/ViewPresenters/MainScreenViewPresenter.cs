using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using RML.Core;
using UnityEngine;
using UnityEngine.UI;

namespace RML.Game
{

public class MainScreenViewPresenter : View
{
   [SerializeField]
   private ListPresenter<MainScreenButtonTimerViewModel, MainScreenButtonTimer>
      _buttonsPanelPresenter;

   [SerializeField]
   private Button _addTimerButton;

   [SerializeField]
   private Button _removeTimerButton;

   private Action _addTimerClickCallback;
   private Action _removeTimerClickCallback;
   
   public void Init(Action addTimerClickCallback, Action removeTimerClickCallback)
   {
      if (addTimerClickCallback == null || removeTimerClickCallback == null)
      {
         throw new ArgumentNullException(
            "Add or Remove callbacks can't be null!");
      }
      
      _addTimerClickCallback = addTimerClickCallback;
      _removeTimerClickCallback = removeTimerClickCallback;
      
      _addTimerButton.onClick.AddListener(AddTimerClickCallback);
      _removeTimerButton.onClick.AddListener(RemoveTimerClickCallback);
   }

   public void HandleTimerCompleted(int timerId)
   {
     var timer = _buttonsPanelPresenter.Views.FirstOrDefault(x
         => x.Value.Id == timerId).Value;

     if (timer == null)
     {
        throw new ArgumentNullException();
     }

     timer.transform.DOPunchScale(Vector3.up, 0.1f)
        .OnComplete(() => timer.transform.localScale = Vector3.one);
   }

   private void RemoveTimerClickCallback()
   {
      _removeTimerClickCallback.Invoke();
   }

   private void AddTimerClickCallback()
   {
      _addTimerClickCallback.Invoke();
   }

   public void Repaint(IEnumerable<MainScreenButtonTimerViewModel> timerViewModels)
   {
      _buttonsPanelPresenter.Repaint(timerViewModels, OnButtonCreated);
   }

   private void OnButtonCreated(MainScreenButtonTimerViewModel viewModel,
      MainScreenButtonTimer buttonTimer)
   {
      var total = _buttonsPanelPresenter.Views.Count;
      var speedMultiplier = 1.0f / ((total + 1.0f) * 0.9f);
      buttonTimer.Repaint(viewModel, speedMultiplier);
   }
}

}

