using UnityEngine;

public abstract class InteractionObject : MonoBehaviour
{
    [Header("Interaction Highlight")]
    [SerializeField] private Color arrowKeyOutlineColor = Color.white;
    [SerializeField] private Color interactionKeyOutlineColor = Color.yellow;
    [SerializeField, Range(1f, 4f)] private float outlineWidth = 4f;
    [SerializeField, Range(0f, 1f)] private float outlineAlphaThreshold = 0.05f;

    private SpriteOutlineHighlighter outlineHighlighter;

    public delegate void InteractionComplete();
    public InteractionComplete interactionComplete;

    public enum InteractionType
    { 
        ArrowKey,
        InteractionKey
    }

    public InteractionType interactionType; // for editor read
    bool isActivate = false;

    /// <summary>
    /// Applies the interaction-type color rule through the reusable outline component.
    /// </summary>
    public virtual void SetHighlighted(bool highlighted)
    {
        GetOutlineHighlighter().SetHighlighted(
            highlighted,
            GetOutlineColor(),
            outlineWidth,
            outlineAlphaThreshold);
    }

    protected virtual void OnDisable()
    {
        SetHighlighted(false);
    }

    private Color GetOutlineColor()
    {
        return interactionType switch
        {
            InteractionType.ArrowKey => arrowKeyOutlineColor,
            InteractionType.InteractionKey => interactionKeyOutlineColor,
            _ => interactionKeyOutlineColor
        };
    }

    private SpriteOutlineHighlighter GetOutlineHighlighter()
    {
        if (outlineHighlighter == null)
        {
            outlineHighlighter = GetComponent<SpriteOutlineHighlighter>();
            if (outlineHighlighter == null)
            {
                outlineHighlighter = gameObject.AddComponent<SpriteOutlineHighlighter>();
                // To do: 현재는 런타임 추가 방식 사용 추후에 변경할 수 있다.
            }
        }

        return outlineHighlighter;
    }
   
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
