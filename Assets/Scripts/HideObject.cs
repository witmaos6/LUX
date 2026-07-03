using UnityEngine;
using UnityEngine.Rendering.Universal;

public class HideObject : InteractionObject
{
    private SpriteRenderer objectRenderer;
    PlayerController controller;
    private Light2D hideLight;

    private void Awake()
    {
        interactionType = InteractionType.ArrowKey;

        hideLight = GetComponentInChildren<Light2D>(true);
        if (hideLight != null)
        {
            hideLight.enabled = false;
        }
    }
    public override void ActivateInteraction(GameObject tryObject)
    {
        objectRenderer = tryObject.GetComponent<SpriteRenderer>();
        if(objectRenderer != null)
        {
            objectRenderer.sortingOrder = 7;
        }
        controller = tryObject.GetComponent<PlayerController>();

        if (hideLight != null)
        {
            hideLight.enabled = true;
        }
        if (controller != null)
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

        if (hideLight != null)
        {
            hideLight.enabled = false;
        }

        if (controller != null)
        {
            controller.SetState(PlayerController.PlayerState.Normal);

            DevilDispatcher.Instance.NotifyPlayerHidden(false, controller.transform.position);
        }

        EndInteraction();
    }
}
