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
        RestoreSpawnedObjects();

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

    private string GetSaveId(SpawnEntry entry, int index) => $"{entry.onEvent.name}_{index}";

    private void RestoreSpawnedObjects()
    {
        for (int i = 0; i < entries.Length; i++)
        {
            var entry = entries[i];
            if (entry.onEvent == null || entry.prefab == null) continue;

            string id = GetSaveId(entry, i);
            if (SaveManager.IsDestroyed(id)) continue;

            var record = SaveManager.FindSpawnedRecord(id);
            if (record == null) continue;

            var instance = Instantiate(entry.prefab, record.position, Quaternion.Euler(entry.eulerRotation));
            AttachSaveableId(instance, id);
        }
    }

    private void Spawn(SpawnEntry entry)
    {
        if (entry.prefab == null) return;
        var pos = (entry.spawnPoint != null ? entry.spawnPoint.position : transform.position)
                  + entry.positionOffset;

        var instance = Instantiate(entry.prefab, pos, Quaternion.Euler(entry.eulerRotation));

        int index = Array.IndexOf(entries, entry);
        string id = GetSaveId(entry, index);
        AttachSaveableId(instance, id);

        SaveManager.MarkSpawned(id, entry.onEvent.name, index, pos);
    }

    private void AttachSaveableId(GameObject instance, string id)
    {
        var saveable = instance.GetComponent<SaveableId>();
        if (saveable == null)
            saveable = instance.AddComponent<SaveableId>();
        saveable.SetId(id);
    }
}
