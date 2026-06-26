using System;
using System.Collections.Generic;
using UnityEngine;

public static class GameEventManager
{
    private static readonly Dictionary<ScriptableObject, Action> handlers = new();
    private static readonly Dictionary<ScriptableObject, Delegate> payloadHandlers = new();

    public static void Subscribe(ScriptableObject topic, Action handler)
    {
        if(!handlers.ContainsKey(topic))
            handlers[topic] = null;

        handlers[topic] += handler;
    }

    public static void Unsubscribe(ScriptableObject topic, Action handler)
    {
        if (handlers.ContainsKey(topic))
            handlers[topic] -= handler;
    }

    public static void Raise(ScriptableObject topic)
    {
        if(handlers.TryGetValue(topic, out var handler))
            handler?.Invoke();
    }

    public static void Subscribe<T>(ScriptableObject topic, Action<T> handler)
    {
        if (!payloadHandlers.ContainsKey(topic))
            payloadHandlers[topic] = null;

        payloadHandlers[topic] = Delegate.Combine(payloadHandlers[topic], handler);
    }

    public static void Unsubscribe<T>(ScriptableObject topic, Action<T> handler)
    {
        if (payloadHandlers.ContainsKey(topic))
            payloadHandlers[topic] = Delegate.Remove(payloadHandlers[topic], handler);
    }

    public static void Raise<T>(ScriptableObject topic, T payload)
    {
        if (payloadHandlers.TryGetValue(topic, out var handler))
            (handler as Action<T>)?.Invoke(payload);
    }

#if UNITY_EDITOR
    [UnityEditor.InitializeOnEnterPlayMode]
    static void OnEnterPlayMode()
    {
        handlers.Clear();
        payloadHandlers.Clear();
    }
#endif
}
