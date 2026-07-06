using System.Collections;
using UnityEngine;

public class ViewImage : InteractionObject
{
    public GameObject uiPrefab;
    public Transform canvasTransform;

    private GameObject uiInstance;

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
        if(uiPrefab)
        {
            uiInstance = Instantiate(uiPrefab, canvasTransform);
        }
    }

    public override void InputPressed()
    {
        if (uiInstance != null)
        {
            Destroy(uiInstance);
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
