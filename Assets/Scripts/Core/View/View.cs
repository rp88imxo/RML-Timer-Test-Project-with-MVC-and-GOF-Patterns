using System;
using JetBrains.Annotations;
using UnityEngine;
using NaughtyAttributes;
using UnityEngine.UI;

namespace RML.Core
{
public abstract class
    View : MonoBehaviour
{
    [Foldout("View")]
    [SerializeField]
    private GameObject _rootObject;

    [Foldout("View")]
    [SerializeField]
    protected Button _closeButton;

    private GameObject _root
    {
        get
        {
            if (_rootObject == null) _rootObject = gameObject;

            return _rootObject;
        }
    }

    public event Action Shown = () => { };
    public event Action Hidden = () => { };

    public bool IsShown => _root.activeInHierarchy;

    protected virtual void Awake()
    {
        if (_closeButton) _closeButton.onClick.AddListener(Hide);
    }

    public void OnCloseButtonClicked(Action callback)
    {
        if (_closeButton)
            _closeButton.onClick.AddListener(callback.Invoke);
    }

    [PublicAPI]
    public virtual void Show() { SetActiveState(true); }

    [PublicAPI]
    public virtual void Hide() { SetActiveState(false); }

    [PublicAPI]
    public virtual void Toggle(bool state) { SetActiveState(state); }

    private void SetActiveState(bool state, bool throwEvent = true)
    {
        _root.SetActive(state);

        if (!throwEvent) return;

        if (state)
            Shown?.Invoke();
        else
            Hidden?.Invoke();
    }

    #if UNITY_EDITOR
    [Button("Show")]
    [UsedImplicitly]
    public void EDITOR_SHOW() { SetActiveState(true, false); }

    [Button("Hide")]
    [UsedImplicitly]
    public void EDITOR_HIDE() { SetActiveState(false, false); }

    #endif
}
}