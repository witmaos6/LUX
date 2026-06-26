using UnityEngine;

public class ActivateOnEvent : MonoBehaviour
{
    public GameEvent activateEvent;

    private void Awake()
    {
        GameEventManager.Subscribe(activateEvent, Activate);
        gameObject.SetActive(false);
    }

    private void OnDestroy() => GameEventManager.Unsubscribe(activateEvent, Activate);

    private void Activate() => gameObject.SetActive(true);
}
