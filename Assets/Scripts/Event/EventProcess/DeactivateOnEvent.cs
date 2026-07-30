using UnityEngine;

[DefaultExecutionOrder(100)]
public class DeactivateOnEvent : MonoBehaviour
{
    public GameEvent deactivateEvent;

    private void Awake()
    {
        GameEventManager.Subscribe(deactivateEvent, Activate);

        if (deactivateEvent != null && SaveManager.HasEventFired(deactivateEvent.name))
        {
            gameObject.SetActive(false);
        }
    }

    private void OnDestroy() => GameEventManager.Unsubscribe(deactivateEvent, Activate);

    private void Activate()
    {
        if (deactivateEvent != null)
            SaveManager.MarkEventFired(deactivateEvent.name);

        gameObject.SetActive(false);
    }
}
