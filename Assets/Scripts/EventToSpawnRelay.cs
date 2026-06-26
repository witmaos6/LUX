using UnityEngine;

public class EventToSpawnRelay : MonoBehaviour
{
    public GameEvent receiveEvent;
    public SpawnEvent startEvent;

    public GameObject spawnGameObject;

    public bool destoryAfterEventCall = true;

    private void OnEnable()
    {
        GameEventManager.Subscribe(receiveEvent, ActivateEvent);
    }

    void ActivateEvent()
    {
        var data = new SpawnEventData
        {
            spawnPrefab = spawnGameObject,
            spawnPosition = transform.position,
            spawnRotation = transform.rotation,
        };

        GameEventManager.Raise<SpawnEventData>(startEvent, data);

        if(destoryAfterEventCall)
        {
            Destroy(gameObject, 0.1f);
        }
    }

    private void OnDisable()
    {
        GameEventManager.Unsubscribe(receiveEvent, ActivateEvent);
    }
}
