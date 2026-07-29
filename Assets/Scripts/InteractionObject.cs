using System.Collections.Generic;
using UnityEngine;

public abstract class InteractionObject : MonoBehaviour
{
    private const string OutlineMaterialResourcePath = "InteractionOutline";

    private static readonly int OutlineColorId = Shader.PropertyToID("_OutlineColor");
    private static readonly int OutlineWidthId = Shader.PropertyToID("_OutlineWidth");
    private static readonly int AlphaThresholdId = Shader.PropertyToID("_AlphaThreshold");
    private static readonly int OutlinePixelStepId = Shader.PropertyToID("_OutlinePixelStep");

    [Header("Interaction Highlight")]
    [SerializeField] private Color outlineColor = new Color(1f, 0.8f, 0.15f, 1f);
    [SerializeField, Range(1f, 4f)] private float outlineWidth = 4f;
    [SerializeField, Range(0f, 1f)] private float outlineAlphaThreshold = 0.05f;

    private readonly List<OutlineRendererPair> outlineRendererPairs = new();
    private MaterialPropertyBlock outlineProperties;
    private bool isHighlighted;

    public delegate void InteractionComplete();
    public InteractionComplete interactionComplete;

    public enum InteractionType
    { 
        ArrowKey,
        InteractionKey
    }

    public InteractionType interactionType; // for editor read
    bool isActivate = false;

    private sealed class OutlineRendererPair
    {
        public SpriteRenderer source;
        public SpriteRenderer outline;
    }

    /// <summary>
    /// Enables or disables an outline for every SpriteRenderer below this interaction object.
    /// UI Image components are intentionally not included.
    /// </summary>
    public virtual void SetHighlighted(bool highlighted)
    {
        if (highlighted && outlineRendererPairs.Count == 0)
        {
            CreateOutlineRenderers();
        }

        isHighlighted = highlighted;
        UpdateOutlineRenderers();
    }

    private void LateUpdate()
    {
        if (isHighlighted)
        {
            UpdateOutlineRenderers();
        }
    }

    protected virtual void OnDisable()
    {
        SetHighlighted(false);
    }

    private void CreateOutlineRenderers()
    {
        Material outlineMaterial = Resources.Load<Material>(OutlineMaterialResourcePath);
        if (outlineMaterial == null)
        {
            Debug.LogError(
                $"Outline material was not found at Resources/{OutlineMaterialResourcePath}.mat.",
                this);
            return;
        }

        SpriteRenderer[] sourceRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        foreach (SpriteRenderer sourceRenderer in sourceRenderers)
        {
            GameObject outlineObject = new GameObject($"{sourceRenderer.gameObject.name} (Interaction Outline)");
            outlineObject.layer = sourceRenderer.gameObject.layer;
            outlineObject.transform.SetParent(sourceRenderer.transform, false);

            SpriteRenderer outlineRenderer = outlineObject.AddComponent<SpriteRenderer>();
            outlineRenderer.sharedMaterial = outlineMaterial;

            outlineRendererPairs.Add(new OutlineRendererPair
            {
                source = sourceRenderer,
                outline = outlineRenderer
            });
        }

        outlineProperties = new MaterialPropertyBlock();
    }

    private void UpdateOutlineRenderers()
    {
        if (outlineProperties == null)
        {
            return;
        }

        outlineProperties.Clear();
        outlineProperties.SetColor(OutlineColorId, outlineColor);
        outlineProperties.SetFloat(OutlineWidthId, outlineWidth);
        outlineProperties.SetFloat(AlphaThresholdId, outlineAlphaThreshold);

        for (int i = outlineRendererPairs.Count - 1; i >= 0; i--)
        {
            OutlineRendererPair pair = outlineRendererPairs[i];
            if (pair.source == null || pair.outline == null)
            {
                outlineRendererPairs.RemoveAt(i);
                continue;
            }

            CopyRendererState(pair.source, pair.outline);

            Texture2D spriteTexture = pair.source.sprite != null
                ? pair.source.sprite.texture
                : null;

            if (spriteTexture != null)
            {
                outlineProperties.SetVector(
                    OutlinePixelStepId,
                    new Vector4(
                        1f / spriteTexture.width,
                        1f / spriteTexture.height,
                        spriteTexture.width,
                        spriteTexture.height));
            }
            else
            {
                outlineProperties.SetVector(OutlinePixelStepId, Vector4.zero);
            }

            pair.outline.SetPropertyBlock(outlineProperties);
            pair.outline.enabled = isHighlighted && pair.source.enabled;
        }
    }

    private static void CopyRendererState(SpriteRenderer source, SpriteRenderer outline)
    {
        outline.sprite = source.sprite;
        outline.flipX = source.flipX;
        outline.flipY = source.flipY;
        outline.drawMode = source.drawMode;
        outline.size = source.size;
        outline.tileMode = source.tileMode;
        outline.maskInteraction = source.maskInteraction;
        outline.spriteSortPoint = source.spriteSortPoint;
        outline.sortingLayerID = source.sortingLayerID;
        outline.sortingOrder = source.sortingOrder;
        outline.renderingLayerMask = source.renderingLayerMask;
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
