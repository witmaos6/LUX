using UnityEngine;

public class ActivateOnEvent : MonoBehaviour
{
    public GameEvent activateEvent;

    private void Awake()
    {
        GameEventManager.Subscribe(activateEvent, Activate);

        bool alreadyFired = activateEvent != null && SaveManager.HasEventFired(activateEvent.name);
        gameObject.SetActive(alreadyFired);
    }

    private void OnDestroy() => GameEventManager.Unsubscribe(activateEvent, Activate);

    private void Activate()
    {
        if (activateEvent != null)
            SaveManager.MarkEventFired(activateEvent.name);

        gameObject.SetActive(true);
    }
}
