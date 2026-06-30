using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TransformEntry
{
    public GameEvent requestEvent;
    public Transform value;
}

public class TransformProvider : MonoBehaviour
{
    [SerializeField] private TransformEntry[] entries;
    private readonly List<(GameEvent e, Action<Action<Transform>> cb)> _subs = new();

    private void OnEnable()
    {
        foreach (var entry in entries)
        {
            if (entry.requestEvent == null) continue;
            var captured = entry;
            Action<Action<Transform>> cb = callback => callback(captured.value);
            _subs.Add((entry.requestEvent, cb));
            GameEventManager.Subscribe<Action<Transform>>(entry.requestEvent, cb);
        }
    }

    private void OnDisable()
    {
        foreach (var (e, cb) in _subs)
            GameEventManager.Unsubscribe<Action<Transform>>(e, cb);
        _subs.Clear();
    }
}
