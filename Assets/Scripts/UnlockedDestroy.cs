using UnityEngine;

public class UnlockedDestroy : MonoBehaviour
{
    public GameEvent unlockEvent;

    public Vector3 soundPosition = Vector3.zero;
    public float soundStrength = 1f;
    public float soundRange = 5f;

    private void OnEnable() => GameEventManager.Subscribe(unlockEvent, Unlocked);
    private void OnDisable() => GameEventManager.Unsubscribe(unlockEvent, Unlocked);

    void Unlocked()
    {
        DevilDispatcher.Instance.NotifySuspicionSource(transform.position, soundRange, soundStrength);

        Destroy(gameObject, 0.1f);
    }
}
