using UnityEngine;

public class DeactivateOnEvent : MonoBehaviour
{
    public GameEvent deactivateEvent;

    private void Awake()
    {
        GameEventManager.Subscribe(deactivateEvent, Activate);
    }

    private void OnDestroy() => GameEventManager.Unsubscribe(deactivateEvent, Activate);

    private void Activate()
    {
        if (deactivateEvent != null)
            SaveManager.MarkEventFired(deactivateEvent.name);

        gameObject.SetActive(false);
    }
}
