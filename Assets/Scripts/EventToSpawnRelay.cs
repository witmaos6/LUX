using UnityEngine;

public class EventToSpawnRelay : MonoBehaviour
{
    public GameEvent receiveEvent;
    public GameEvent raiseEvent;
    public bool destroyAfterEventCall = true;

    private void OnEnable() => GameEventManager.Subscribe(receiveEvent, Relay);
    private void OnDisable() => GameEventManager.Unsubscribe(receiveEvent, Relay);

    private void Relay()
    {
        if (raiseEvent != null)
            GameEventManager.Raise(raiseEvent);

        if (destroyAfterEventCall)
            Destroy(gameObject, 0.1f);
    }
}
