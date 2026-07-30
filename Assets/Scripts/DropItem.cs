using System.Numerics;
using UnityEngine;

public class DropItem : InteractionObject
{
    public enum ItemCode
    { 
        Flashlight = 0,
        BoltCutter = 1,
        Password = 2,
        Stick = 3, // 기존에 사용하던 Stick은 DoorOpener로 기획 변경, Stick은 현재 사용하지 않는다.
        DoorOpener = 4,
        USB = 5,
        None = 6,
    }

    public GameObject uiPrefab;
    private GameObject uiInstance;
    public ItemCode itemCode;
    public bool objectDestroy = true;
    public bool inputObjectDestroy = true;
    public GameEvent gameEvent;
    public Transform canvasTransform;


    private void Awake()
    {
        interactionType = InteractionType.InteractionKey;

        if ((objectDestroy || inputObjectDestroy) && SaveManager.HasItem(itemCode))
        {
            Destroy(gameObject);
            return;
        }

        if (canvasTransform == null)
        {
            canvasTransform = GameObject.Find("Canvas").transform;
        }
    }

    public override void ActivateInteraction(GameObject tryObject)
    {
        PlayerController controller = tryObject.GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.AddItem(itemCode);

            if(gameEvent != null)
            {
                GameEventManager.Raise(gameEvent);
            }

            if (uiPrefab != null)
            {
                uiInstance = Instantiate(uiPrefab, canvasTransform);
            }

            if (objectDestroy)
            {
                Destroy(gameObject, 0.1f);
                EndInteraction();
            }
        }
    }

    public override void InputPressed()
    { 
        if(uiInstance != null)
        {
            Destroy(uiInstance);
        }
        if(inputObjectDestroy)
        {
            Destroy(gameObject, 0.1f);
        }
        EndInteraction();
    }

    public override void CancelInteraction()
    {
        if (uiInstance != null)
            Destroy(uiInstance);

        base.CancelInteraction();
    }
}
