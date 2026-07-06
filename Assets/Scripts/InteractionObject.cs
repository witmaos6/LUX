using UnityEngine;

public abstract class InteractionObject : MonoBehaviour
{
    public delegate void InteractionComplete();
    public InteractionComplete interactionComplete;

    public enum InteractionType
    { 
        ArrowKey,
        InteractionKey
    }

    public InteractionType interactionType; // for editor read
    bool isActivate = false;
   
    public void Interaction(GameObject tryObject)
    {
        if(!isActivate)
        {
            ActivateInteraction(tryObject);
            isActivate = true;
        }
        else
        {
            InputPressed();
        }
    }

    public abstract void ActivateInteraction(GameObject tryObject);

    public virtual void InputPressed() { }

    public void EndInteraction()
    {
        interactionComplete?.Invoke();
        interactionComplete = null;
        isActivate = false;
    }

    public virtual void CancelInteraction()
    {
        EndInteraction();
    }
}
