using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RML.Game
{
[RequireComponent(typeof(Button))]
public class HoldButtonHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private Button _button;

    [SerializeField]
    private float _defaultSpeed = 10f;

    [SerializeField]
    private float _maxSpeed = 100f;

    [SerializeField]
    private float _incrementValue = 1f;
    
    private Coroutine _coroutine;
    
    private float _currentSpeed; // How many calls per second 
    private float _totalSum;
    
    private PointerEventData _pointerEventData;

    private void Awake()
    {
        _currentSpeed = _defaultSpeed;
        _pointerEventData = new PointerEventData(EventSystem.current);
    }

    IEnumerator PressEnumerator()
    {
        while (true)
        {
            var delta = Time.deltaTime;

            _totalSum += _currentSpeed * delta;
            
            if (_totalSum > 1f)
            {
                var totalHits = Mathf.RoundToInt(_totalSum);
                
                while (totalHits-- > 0)
                {
                    _button.OnPointerClick(_pointerEventData);
                }
                
                _totalSum = 0f;
            }
            
            _currentSpeed =
                Mathf.Min(_currentSpeed + _incrementValue * delta,
                    _maxSpeed);
            
            yield return null;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!_button.interactable)
        {
            return;
        }

        _coroutine = StartCoroutine(PressEnumerator());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Stop();
    }

    public void OnPointerEnter(PointerEventData eventData) { }

    public void OnPointerExit(PointerEventData eventData)
    {
        Stop();
    }

    private void Stop()
    {
        if (_coroutine == null)
        {
           return;
        }
       
        StopCoroutine(_coroutine);
        _currentSpeed = _defaultSpeed;
        _totalSum = 0f;
    }
}
}


