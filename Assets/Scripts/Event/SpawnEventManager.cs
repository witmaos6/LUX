using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SpawnEntry
{
    public GameEvent onEvent;
    public GameObject prefab;
    public Transform spawnPoint;
    public Vector3 positionOffset;
    public Vector3 eulerRotation;
}

public class SpawnEventManager : MonoBehaviour
{
    [SerializeField] private SpawnEntry[] entries;
    private readonly List<(GameEvent e, Action cb)> _subs = new();

    private void OnEnable()
    {
        foreach (var entry in entries)
        {
            if (entry.onEvent == null) continue;
            var captured = entry;
            Action cb = () => Spawn(captured);
            _subs.Add((entry.onEvent, cb));
            GameEventManager.Subscribe(entry.onEvent, cb);
        }
    }

    private void OnDisable()
    {
        foreach (var (e, cb) in _subs)
            GameEventManager.Unsubscribe(e, cb);
        _subs.Clear();
    }

    private void Spawn(SpawnEntry entry)
    {
        if (entry.prefab == null) return;
        var pos = (entry.spawnPoint != null ? entry.spawnPoint.position : transform.position)
                  + entry.positionOffset;

        Instantiate(entry.prefab, pos, Quaternion.Euler(entry.eulerRotation));
    }
}
