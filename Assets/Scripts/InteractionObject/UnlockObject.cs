using UnityEngine;
using static DropItem;

public class UnlockObject : InteractionObject
{
    public GameEvent unlockFailed;
    public GameEvent unlockComplete;
    public GameEvent spawnRaise;

    public GameObject uiPrefab;
    public Transform canvasTransform;

    private GameObject uiInstance;

    public AudioClip sound;

    public ItemCode itemCode;
    public bool postUnlockDestroy = false;

    private void Awake()
    {
        interactionType = InteractionType.InteractionKey;

        if (canvasTransform == null)
            canvasTransform = GameObject.Find("Canvas").transform;
    }

    public override void ActivateInteraction(GameObject tryObject)
    {
        PlayerController playerController = tryObject.GetComponent<PlayerController>();
        if (playerController == null) return;

        if (playerController.ExistItem(itemCode))
        {
            if (unlockComplete != null)
                GameEventManager.Raise(unlockComplete);

            if (sound != null)
            {
                AudioSource.PlayClipAtPoint(sound, transform.position);
            }

            if (spawnRaise != null)
                GameEventManager.Raise(spawnRaise);

            if (postUnlockDestroy)
                Destroy(gameObject, 0.1f);

            EndInteraction();
        }
        else
        {
            if (unlockFailed != null)
                GameEventManager.Raise(unlockFailed);

            if (uiPrefab)
            {
                uiInstance = Instantiate(uiPrefab, canvasTransform);
            }
            else
            {
                EndInteraction();
            }
        }
    }

    public override void InputPressed()
    {
        if (uiInstance != null)
            Destroy(uiInstance);

        EndInteraction();
    }

    public override void CancelInteraction()
    {
        if (uiInstance != null)
            Destroy(uiInstance);

        base.CancelInteraction();
    }
}
