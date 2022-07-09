using System;
using System.Collections;
using System.Collections.Generic;
using RML.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RML.Game
{

public class MainScreenButtonTimerViewModel
{
   public int Id { get; set; }
   public string Text { get; set; }
   public Action<int> ClickCallback { get; set; }
}

public class MainScreenButtonTimer : MonoBehaviour, IPoolingResettable
{
   [SerializeField]
   private Button _button;

   [SerializeField]
   private TMP_Text _buttonText;

   [SerializeField]
   private RectTransform _root;

   [SerializeField]
   private OutOfScreenAnimationHandler _outOfScreenAnimationHandler;
   
   public int Id { get; private set; }

   private Action<int> _clickCallback;

   public RectTransform GetRoot() => _root;
   
   public void ResetState()
   {
      Id = 0;
      _buttonText.text = String.Empty;
      _clickCallback = null;
      _button.onClick.RemoveListener(ClickCallback);
   }
   
   public void Repaint(MainScreenButtonTimerViewModel data, float animationSpeedMultiplier)
   {
      Id = data.Id;
      _buttonText.text = data.Text;
      _clickCallback = data.ClickCallback;
      
      _button.onClick.AddListener(ClickCallback);
      
      _outOfScreenAnimationHandler.StopAnimate();
      _outOfScreenAnimationHandler.StartAnimate(animationSpeedMultiplier);
   }

   private void ClickCallback()
   {
      _clickCallback.Invoke(Id);
   }
}
}


