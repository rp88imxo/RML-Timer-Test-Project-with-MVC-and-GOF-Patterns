using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace RML.Core
{
public enum ScreenType
{
    MainScreen,
    TimerScreen
}

public class RequestedScreenData
{
    public BaseScreenData ScreenData { get; set; }
    public IScreen Screen { get; set; }
    public SwitchMode SwitchMode { get; set; }
}

public enum SwitchMode
{
    Replacing,
    Additive
}

public class SwitchScreenData
{
    public IScreen Screen { get; set; }
    public SwitchMode SwitchMode { get; set; }
}

[UsedImplicitly]
public class ScreenController
{
    public static event Action<ScreenType> ScreenChanged;

    private static readonly Dictionary<ScreenType, IScreen> Screens;
    private readonly Stack<SwitchScreenData> _screensStack;

    private readonly Dictionary<ScreenType, Queue<RequestedScreenData>>
        _screenQueueDictionary;

    public ScreenType CurrentScreenType { get; private set; }

    static ScreenController()
    {
        Screens = new Dictionary<ScreenType, IScreen>();
    }

    public ScreenController()
    {
        _screensStack = new Stack<SwitchScreenData>();

        int screenQueueCapacity =
            EnumUtils.GetNamesCount<ScreenType>();
        _screenQueueDictionary =
            new Dictionary<ScreenType, Queue<RequestedScreenData>>(
                screenQueueCapacity);
    }

    private void OnScreenChangedSelf(ScreenType newScreenType)
    {
        CurrentScreenType = newScreenType;
        CheckRequestedScreens();
    }

    public bool HasAnyRequestedScreensAfterScreen(ScreenType screenType)
    {
        if (_screenQueueDictionary.TryGetValue(screenType, out var queue))
        {
            return queue.Count > 0;
        }

        return false;
    }

    public static void RegisterScreen(IScreen screen)
    {
        if (Screens.ContainsKey(screen.ScreenType))
        {
            Debug.LogWarning(
                $"Screen with type {screen.ScreenType} already added to the Screens!");
            return;
        }

        Screens.Add(screen.ScreenType, screen);
    }

    public static void UnregisterScreen(IScreen screen)
    {
        if (!Screens.ContainsKey(screen.ScreenType))
        {
            Debug.LogWarning(
                $"Screen with type {screen.ScreenType} is missing in screens!");
            return;
        }

        Screens.Remove(screen.ScreenType);
    }

    public void Reset()
    {
        _screensStack.Clear();
    }

    public void BackToPreviousScreen()
    {
        if (_screensStack.Count <= 1)
        {
            Debug.LogWarning("This screen is a root, return to main screen!");
            SwitchToScreen(ScreenType.MainScreen);
            return;
        }

        var currentSwitchScreenData = _screensStack.Pop();
        var currentScreenToHide = currentSwitchScreenData.Screen;

        currentScreenToHide.OnHide();

        var previousScreenData = _screensStack.Peek();
        var previousScreen = previousScreenData.Screen;
        switch (currentSwitchScreenData.SwitchMode)
        {
        case SwitchMode.Replacing:
            previousScreen.OnShow<BaseScreenData>(null);
            break;
        case SwitchMode.Additive: break;
        default: throw new ArgumentOutOfRangeException();
        }

        ScreenChanged?.Invoke(previousScreen.ScreenType);

        Debug.Log(
            $"Screen switched from {currentScreenToHide.ScreenType} to {previousScreen.ScreenType}");
        OnScreenChangedSelf(previousScreen.ScreenType);
    }

    public void SwitchToScreen(ScreenType screenTypeToSwitch,
        SwitchMode switchMode = SwitchMode.Replacing,
        bool resetScreenStack = false)
    {
        SwitchToScreenWithScreenData(
            new EmptyScreenData(screenTypeToSwitch),
            switchMode,
            resetScreenStack);
    }

    public void SwitchToScreenWithScreenData<T>(T baseScreenData,
        SwitchMode switchMode = SwitchMode.Replacing,
        bool resetScreenStack = false)
        where T : BaseScreenData
    {
        IScreen currentScreen = null;
        SwitchScreenData currentSwitchScreenData = null;
        if (_screensStack.Count > 0)
        {
            currentSwitchScreenData = _screensStack.Peek();
            currentScreen = currentSwitchScreenData.Screen;
            if (resetScreenStack)
            {
                _screensStack.Clear();
            }
        }

        var screenType = baseScreenData.ScreenType;
        if (!Screens.TryGetValue(screenType, out var newScreen))
        {
            Debug.LogWarning(
                $"Can't find a screen with type {baseScreenData.ScreenType}");
            return;
        }

        if (currentSwitchScreenData != null)
        {
            switch (switchMode)
            {
            case SwitchMode.Replacing:
                currentScreen.OnHide();
                break;
            case SwitchMode.Additive: break;
            default: throw new ArgumentOutOfRangeException();
            }
        }

        var newSwitchScreenData = new SwitchScreenData
        {
            Screen = newScreen,
            SwitchMode = switchMode
        };

        _screensStack.Push(newSwitchScreenData);

        newScreen.OnShow(baseScreenData);

        ScreenChanged?.Invoke(newScreen.ScreenType);
        string switchedFrom =
            currentScreen?.ScreenType.ToString() ?? "<root>";
        Debug.Log(
            $"Screen switched from {switchedFrom} to {newScreen.ScreenType}");
        
        OnScreenChangedSelf(newScreen.ScreenType);
    }

    /// <summary>
    /// Request a screen which will be added to the queue
    /// </summary>
    public void RequestScreen(ScreenType screenTypeToSwitch,
        ScreenType addAfterScreen,
        SwitchMode switchMode = SwitchMode.Replacing)
    {
        RequestScreenWithData(new EmptyScreenData(screenTypeToSwitch),
            addAfterScreen,
            switchMode);
    }

    [PublicAPI]
    public void RequestScreenWithData<T>(T baseScreenData,
        ScreenType addAfterScreen,
        SwitchMode switchMode = SwitchMode.Replacing)
        where T : BaseScreenData
    {
        string screenTypeString = baseScreenData.ScreenType.ToString();

        var screenType = baseScreenData.ScreenType;
        if (!Screens.TryGetValue(screenType, out var newScreen))
        {
            Debug.LogWarning(
                $"Can't find a screen with type {screenTypeString}");
            return;
        }

        var requestedScreenData = new RequestedScreenData
        {
            ScreenData = baseScreenData,
            Screen = newScreen,
            SwitchMode = switchMode
        };

        if (_screenQueueDictionary.TryGetValue(addAfterScreen,
                out var queue))
        {
            queue.Enqueue(requestedScreenData);
            Debug.Log($"Screen {screenTypeString} added to queue!");
        }
        else
        {
            queue = new Queue<RequestedScreenData>();
            queue.Enqueue(requestedScreenData);

            _screenQueueDictionary.Add(addAfterScreen, queue);
        }

        CheckRequestedScreens();
    }

    /// <summary>
    /// Checking and popping a screen if we have any screens in requested screens of specified type
    /// </summary>
    private void CheckRequestedScreens()
    {
        if (_screenQueueDictionary.TryGetValue(CurrentScreenType,
                out var queue))
        {
            if (queue.Count == 0) return;

            var requestedScreenData = queue.Dequeue();
            SwitchToScreenWithScreenData(requestedScreenData
                    .ScreenData,
                requestedScreenData.SwitchMode);
        }
    }
}
}