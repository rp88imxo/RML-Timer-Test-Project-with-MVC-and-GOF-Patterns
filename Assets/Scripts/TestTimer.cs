using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;

namespace RML
{
    public class TestTimer : MonoBehaviour
    {
        [SerializeField] private TMP_Text _timerText;

        private MultiThreadedTimer _multiThreadedTimer;

        private void Awake()
        {
            _multiThreadedTimer = new MultiThreadedTimer(SynchronizationContext.Current, 5000, 50, TickCallback,
                ONCompletedCallback);


            _multiThreadedTimer.Start();
        }

        private void ONCompletedCallback()
        {
            _multiThreadedTimer.Dispose();
        }

        private void TickCallback()
        {
            var lt = _multiThreadedTimer.GetLeftTime;
            _timerText.text = lt.ToString("mm\\:ss\\:ff");
            transform.Translate(2.5f, 0f, 0, Space.Self);
            Debug.Log($"Left time {lt:mm\\:ss}");
        }
    }
}