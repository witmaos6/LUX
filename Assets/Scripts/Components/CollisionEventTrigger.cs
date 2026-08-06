using System.Collections;
using UnityEngine;

public class CollisionEventTrigger : MonoBehaviour
{
    [Min(0f)]
    public float startDelay = 0;
    public GameEvent beginEvent;
    public GameEvent blockEvent;

    [Header("Trigger Conditions")]
    [Tooltip("Only colliders with this tag can activate the trigger. Leave empty to accept every tag.")]
    [SerializeField] private string requiredTag = "Player";
    [Tooltip("When enabled, the event can only be raised once while this component exists.")]
    [SerializeField] private bool triggerOnce = true;

    private bool activateBlock = false;
    private bool isWaiting;
    private bool hasTriggered;
    private Coroutine beginEventCoroutine;

    private void OnEnable()
    {
        if (blockEvent)
        {
            GameEventManager.Subscribe(blockEvent, ActivateBlock);
        }
    }

    private void OnDisable()
    {
        if (blockEvent)
        {
            GameEventManager.Unsubscribe(blockEvent, ActivateBlock);
        }

        if (beginEventCoroutine != null)
        {
            StopCoroutine(beginEventCoroutine);
            beginEventCoroutine = null;
        }

        isWaiting = false;
    }

    private void ActivateBlock()
    {
        activateBlock = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!CanTrigger(collision))
        {
            return;
        }

        isWaiting = true;
        beginEventCoroutine = StartCoroutine(BeginEvent());
    }

    private bool CanTrigger(Collider2D collision)
    {
        if (!beginEvent || activateBlock || isWaiting || (triggerOnce && hasTriggered))
        {
            return false;
        }

        return string.IsNullOrEmpty(requiredTag) || collision.CompareTag(requiredTag);
    }

    private IEnumerator BeginEvent()
    {
        yield return new WaitForSeconds(Mathf.Max(0f, startDelay));

        if (!activateBlock)
        {
            GameEventManager.Raise(beginEvent);
            hasTriggered = true;
        }

        isWaiting = false;
        beginEventCoroutine = null;
    }
}
