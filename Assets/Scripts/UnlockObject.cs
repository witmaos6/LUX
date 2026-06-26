using System.Collections;
using UnityEngine;
using static DropItem;

public class UnlockObject : InteractionObject
{
    public GameEvent unlockFailed;
    public GameEvent unlockComplete;
    public SpawnEvent spawnRaise;

    public GameObject uiPrefab;
    public Transform canvasTransform;

    private GameObject uiInstance;

    public ItemCode itemCode;
    public bool postUnlockDestroy = false;

    [SerializeField] private GameObject m_spawnPrefab;

    private void Awake()
    {
        interactionType = InteractionType.InteractionKey;

        if (canvasTransform == null)
        {
            canvasTransform = GameObject.Find("Canvas").transform;
        }
    }

    public override void ActivateInteraction(GameObject tryObject)
    {
        PlayerController playerController = tryObject.GetComponent<PlayerController>();
        if (playerController != null)
        {
            if (playerController.ExistItem(itemCode))
            {
                EndInteraction();

                if(unlockComplete != null)
                {
                    GameEventManager.Raise(unlockComplete);
                }
                
                if(spawnRaise != null)
                {
                    var data = new SpawnEventData
                    {
                        spawnPrefab = m_spawnPrefab,
                        spawnPosition = transform.position,
                        spawnRotation = transform.rotation,
                    };
                    GameEventManager.Raise<SpawnEventData>(spawnRaise, data);
                }

                if (postUnlockDestroy == true)
                {
                    Destroy(gameObject, 0.1f);
                }
            }
            else
            {
                uiInstance = Instantiate(uiPrefab, canvasTransform);

                if(unlockFailed != null)
                {
                    GameEventManager.Raise(unlockFailed);
                }
            }
        }
    }

    public override void InputPressed()
    {
        if(uiInstance != null)
        {
            Destroy(uiInstance);
        }
        EndInteraction();
    }
}
