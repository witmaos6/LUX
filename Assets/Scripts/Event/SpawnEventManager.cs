using UnityEngine;

public class SpawnEventManager : MonoBehaviour
{
    public SpawnEvent[] spawnEvents;

    private void OnEnable()
    {
        foreach (var e in spawnEvents)
            if (e != null)
                GameEventManager.Subscribe<SpawnEventData>(e, OnSpawn);
    }

    private void OnDisable()
    {
        foreach (var e in spawnEvents)
            if (e != null)
                GameEventManager.Unsubscribe<SpawnEventData>(e, OnSpawn);
    }

    void OnSpawn(SpawnEventData data)
    {
        if (data.spawnPrefab != null)
            Instantiate(data.spawnPrefab, data.spawnPosition, data.spawnRotation);
    }
}
