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
   
   public int Id { get; private set; }

   private Action<int> _clickCallback;

   public void ResetState()
   {
      Id = 0;
      _buttonText.text = String.Empty;
      _clickCallback = null;
      _button.onClick.RemoveListener(ClickCallback);
   }
   
   public void Repaint(MainScreenButtonTimerViewModel data)
   {
      Id = data.Id;
      _buttonText.text = data.Text;
      _clickCallback = data.ClickCallback;
      
      _button.onClick.AddListener(ClickCallback);
   }

   private void ClickCallback()
   {
      _clickCallback.Invoke(Id);
   }
}
}


