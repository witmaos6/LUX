using UnityEngine;

public class ActivateOnEvent : MonoBehaviour
{
    public GameEvent activateEvent;
    public bool savedEvent = true;

    private void Awake()
    {
        GameEventManager.Subscribe(activateEvent, Activate);

        if(savedEvent)
        {
            bool alreadyFired = activateEvent != null && SaveManager.HasEventFired(activateEvent.name);
            gameObject.SetActive(alreadyFired);
        }
        if(!savedEvent)
        {
            gameObject.SetActive(false);
        }
    }

    private void OnDestroy() => GameEventManager.Unsubscribe(activateEvent, Activate);

    private void Activate()
    {
        if (activateEvent != null && savedEvent)
            SaveManager.MarkEventFired(activateEvent.name);

        gameObject.SetActive(true);
    }
}
