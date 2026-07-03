using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class SpawnedRecord
{
    public string id;
    public string sourceEventId;
    public int entryIndex;
    public Vector3 position;
}

[Serializable]
public class CheckpointData
{
    public bool hasRespawnPoint;
    public Vector3 respawnPosition;
    public string respawnWallpaperId;
    public List<DropItem.ItemCode> inventory = new List<DropItem.ItemCode>();
    public List<string> destroyedObjectIds = new List<string>();
    public List<SpawnedRecord> spawnedObjects = new List<SpawnedRecord>();
}

public static class SaveManager
{
    private const string SaveFileName = "checkpoint.json";
    private static string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

    private static CheckpointData data = new CheckpointData();
    private static readonly HashSet<string> destroyedLookup = new HashSet<string>();
    private static bool isQuitting = false;

    public static bool HasSave { get; private set; }
    public static bool IsQuitting => isQuitting;

    public static Vector3 RespawnPosition => data.respawnPosition;
    public static string RespawnWallpaperId => data.respawnWallpaperId;
    public static IReadOnlyList<DropItem.ItemCode> Inventory => data.inventory;
    public static IReadOnlyList<SpawnedRecord> SpawnedObjects => data.spawnedObjects;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Load()
    {
        isQuitting = false;
        Application.quitting += () => isQuitting = true;

        if (File.Exists(SavePath))
        {
            try
            {
                data = JsonUtility.FromJson<CheckpointData>(File.ReadAllText(SavePath)) ?? new CheckpointData();
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to load save file: {e.Message}");
                data = new CheckpointData();
            }
        }
        else
        {
            data = new CheckpointData();
        }

        destroyedLookup.Clear();
        foreach (var id in data.destroyedObjectIds)
            destroyedLookup.Add(id);

        HasSave = data.hasRespawnPoint;
    }

    public static void ResetSave()
    {
        data = new CheckpointData();
        destroyedLookup.Clear();
        HasSave = false;
        Flush();
    }

    public static void SetRespawnPoint(Vector3 position, string wallpaperId)
    {
        data.hasRespawnPoint = true;
        data.respawnPosition = position;
        if (!string.IsNullOrEmpty(wallpaperId))
            data.respawnWallpaperId = wallpaperId;

        HasSave = true;
        Flush();
    }

    public static void SetInventory(IEnumerable<DropItem.ItemCode> items)
    {
        data.inventory = new List<DropItem.ItemCode>(items);
        Flush();
    }

    public static bool HasItem(DropItem.ItemCode item)
    {
        return data.inventory.Contains(item);
    }

    public static bool IsDestroyed(string id)
    {
        return !string.IsNullOrEmpty(id) && destroyedLookup.Contains(id);
    }

    public static void MarkDestroyed(string id)
    {
        if (string.IsNullOrEmpty(id)) return;
        if (!destroyedLookup.Add(id)) return;

        data.destroyedObjectIds.Add(id);
        data.spawnedObjects.RemoveAll(r => r.id == id);
        Flush();
    }

    public static void MarkSpawned(string id, string sourceEventId, int entryIndex, Vector3 position)
    {
        if (string.IsNullOrEmpty(id)) return;

        data.spawnedObjects.RemoveAll(r => r.id == id);
        data.spawnedObjects.Add(new SpawnedRecord
        {
            id = id,
            sourceEventId = sourceEventId,
            entryIndex = entryIndex,
            position = position
        });
        Flush();
    }

    public static SpawnedRecord FindSpawnedRecord(string id)
    {
        return data.spawnedObjects.Find(r => r.id == id);
    }

    private static void Flush()
    {
        try
        {
            File.WriteAllText(SavePath, JsonUtility.ToJson(data, true));
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Failed to write save file: {e.Message}");
        }
    }
}
