
using System;
using System.Collections.Generic;

namespace RML.Core
{

public static class Events
{
    public static string TimerCompletedEvent = "TIMER_COMPLETED_EVENT";
    public static string TimerButtonDeletedEvent = "TIMER_BUTTON_DELETED_EVENT";
}

public interface IPayload
{
    string EventName { get; }
}

public readonly struct EmptyPayload : IPayload
{
    public string EventName { get; }
    public EmptyPayload(string eventName)
    {
        this.EventName = eventName;
    }
}

public readonly struct TimerCompletedPayload : IPayload
{
    public string EventName { get; }

    public int TimerId { get; }
    
    public TimerCompletedPayload(string eventName, int timerId)
    {
        EventName = eventName;
        TimerId = timerId;
    }
}

public readonly struct TimerButtonDeletedPayload : IPayload
{
    public TimerButtonDeletedPayload(string eventName, int timerId)
    {
        EventName = eventName;
        TimerId = timerId;
    }
    
    public string EventName { get; }
    public int TimerId { get; }
}

public class EventBus
{
    private readonly Dictionary<string, List<Action<IPayload>>> _events = new Dictionary<string,List<Action<IPayload>>>();

    public void AddListener(string eventName, Action<IPayload> listenerMethod)
    {
        if (!_events.ContainsKey(eventName))
        {
            _events.Add(eventName, new List<Action<IPayload>>());
        }

        _events[eventName].Add(listenerMethod);
    }

    public void RemoveListener(string eventName, Action<IPayload> listenerMethod)
    {
        if (_events.ContainsKey(eventName))
        {
            _events[eventName].Remove(listenerMethod);
        }
    }

    public void PublishEvent<T>(T payload) where T: IPayload
    {
        var eventName = payload.EventName;
        if (_events.ContainsKey(eventName))
        {
            _events[eventName].ForEach(method => method.Invoke(payload));
        }
    }
}
}
