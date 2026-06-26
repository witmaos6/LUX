using UnityEngine;

public class HideObject : InteractionObject
{
    private SpriteRenderer objectRenderer;
    PlayerController controller;
    private void Awake()
    {
        interactionType = InteractionType.ArrowKey;
    }
    public override void ActivateInteraction(GameObject tryObject)
    {
        objectRenderer = tryObject.GetComponent<SpriteRenderer>();
        if(objectRenderer != null)
        {
            objectRenderer.sortingOrder = 7;
        }
        controller = tryObject.GetComponent<PlayerController>();
        if(controller != null)
        {
            controller.SetState(PlayerController.PlayerState.Hide);

            DevilDispatcher.Instance.NotifyPlayerHidden(true, controller.transform.position);
        }
    }

    public override void InputPressed()
    {
        if (objectRenderer != null)
        {
            objectRenderer.sortingOrder = 10;
        }

        if (controller != null)
        {
            controller.SetState(PlayerController.PlayerState.Normal);

            DevilDispatcher.Instance.NotifyPlayerHidden(false, controller.transform.position);
        }

        EndInteraction();
    }
}
