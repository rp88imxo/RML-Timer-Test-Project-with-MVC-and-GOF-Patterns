using System;
using System.Collections;
using System.Collections.Generic;
using RML.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace RML
{
public class TestTimer2 : MonoBehaviour
{
    private DefaultTimer _defaultTimer;

    [SerializeField]
    private TMP_Text _tex;

    [SerializeField]
    private Button _buttonPause;

    [SerializeField]
    private Button _buttonResume;

    private void Awake()
    {
        _defaultTimer = DefaultTimer.CreateTimer(5f, ONComplete, ONUpdate);
        _defaultTimer.Start();
        _buttonPause.onClick.AddListener(PauseCallback);
        _buttonResume.onClick.AddListener(ResumeCallback);
    }

    private void ResumeCallback()
    {
        if (_defaultTimer.IsPaused)
        {
            _defaultTimer.Resume();
        }
    }

    private void PauseCallback()
    {
        if (!_defaultTimer.IsPaused)
        {
            _defaultTimer.Pause();
        }
    }

    private void ONUpdate(float obj)
    {
        var timeSpan = TimeSpan.FromSeconds(obj);
        _tex.text = timeSpan.ToString("mm\\:ss\\:ff");
        Debug.Log($"ELAPSED TIME: {obj}");
    }

    private void ONComplete() { Debug.Log("Timer is completed"); }
}
}