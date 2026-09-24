using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Adds an alpha-based outline to SpriteRenderers on this object or its children.
/// It can be used independently of InteractionObject.
/// </summary>
[DisallowMultipleComponent]
public sealed class SpriteOutlineHighlighter : MonoBehaviour
{
    private const string OutlineMaterialResourcePath = "InteractionOutline";

    private static readonly int OutlineColorId = Shader.PropertyToID("_OutlineColor");
    private static readonly int OutlineWidthId = Shader.PropertyToID("_OutlineWidth");
    private static readonly int AlphaThresholdId = Shader.PropertyToID("_AlphaThreshold");
    private static readonly int OutlinePixelStepId = Shader.PropertyToID("_OutlinePixelStep");

    [Header("Outline Style")]
    [SerializeField] private Color outlineColor = Color.yellow;
    [SerializeField, Range(1f, 4f)] private float outlineWidth = 2f;
    [SerializeField, Range(0f, 1f)] private float alphaThreshold = 0.05f;

    [Header("Targets")]
    [Tooltip("Leave empty to outline every SpriteRenderer on this object and its children.")]
    [SerializeField] private SpriteRenderer[] targetRenderers;

    [Header("Sorting")]
    [Tooltip("When enabled, the outline uses Order In Layer below instead of the source SpriteRenderer's order.")]
    [SerializeField] private bool useCustomOrderInLayer;
    [SerializeField] private int orderInLayer;

    private readonly List<OutlineRendererPair> outlineRendererPairs = new();
    private MaterialPropertyBlock outlineProperties;
    private Material outlineMaterial;
    private bool isHighlighted;
    private Color activeOutlineColor;
    private float activeOutlineWidth;
    private float activeAlphaThreshold;

    private sealed class OutlineRendererPair
    {
        public SpriteRenderer source;
        public SpriteRenderer outline;
    }

    /// <summary>Uses the style configured in this component's Inspector.</summary>
    public void SetHighlighted(bool highlighted)
    {
        SetHighlighted(highlighted, outlineColor, outlineWidth, alphaThreshold);
    }

    /// <summary>Shows or hides the outline with a caller-provided style.</summary>
    public void SetHighlighted(
        bool highlighted,
        Color color,
        float width,
        float threshold)
    {
        activeOutlineColor = color;
        activeOutlineWidth = Mathf.Clamp(width, 1f, 4f);
        activeAlphaThreshold = Mathf.Clamp01(threshold);
        isHighlighted = highlighted;

        if (highlighted)
        {
            EnsureOutlineRenderers();
        }

        UpdateOutlineRenderers();
    }

    /// <summary>Rescans the target hierarchy so newly added SpriteRenderers can be outlined.</summary>
    public void RefreshTargets()
    {
        EnsureOutlineRenderers();
        UpdateOutlineRenderers();
    }

    /// <summary>Sets an independent Order in Layer while retaining the source Sorting Layer.</summary>
    public void SetOrderInLayer(int order)
    {
        useCustomOrderInLayer = true;
        orderInLayer = order;

        if (isHighlighted)
        {
            UpdateOutlineRenderers();
        }
    }

    /// <summary>Returns outline ordering to the source SpriteRenderer's Order in Layer.</summary>
    public void FollowSourceOrderInLayer()
    {
        useCustomOrderInLayer = false;

        if (isHighlighted)
        {
            UpdateOutlineRenderers();
        }
    }

    private void Awake()
    {
        activeOutlineColor = outlineColor;
        activeOutlineWidth = outlineWidth;
        activeAlphaThreshold = alphaThreshold;
    }

    private void LateUpdate()
    {
        if (isHighlighted)
        {
            UpdateOutlineRenderers();
        }
    }

    private void OnDisable()
    {
        SetOutlineRenderersEnabled(false);
    }

    private void OnEnable()
    {
        if (isHighlighted)
        {
            UpdateOutlineRenderers();
        }
    }

    private void EnsureOutlineRenderers()
    {
        outlineMaterial ??= Resources.Load<Material>(OutlineMaterialResourcePath);
        if (outlineMaterial == null)
        {
            Debug.LogError(
                $"Outline material was not found at Resources/{OutlineMaterialResourcePath}.mat.",
                this);
            return;
        }

        RemoveInvalidPairs();

        foreach (SpriteRenderer sourceRenderer in GetSourceRenderers())
        {
            if (sourceRenderer == null || IsRegisteredRenderer(sourceRenderer))
            {
                continue;
            }

            GameObject outlineObject = new GameObject($"{sourceRenderer.gameObject.name} (Outline)");
            outlineObject.layer = sourceRenderer.gameObject.layer;
            outlineObject.transform.SetParent(sourceRenderer.transform, false);

            SpriteRenderer outlineRenderer = outlineObject.AddComponent<SpriteRenderer>();
            outlineRenderer.sharedMaterial = outlineMaterial;
            outlineRenderer.enabled = false;

            outlineRendererPairs.Add(new OutlineRendererPair
            {
                source = sourceRenderer,
                outline = outlineRenderer
            });
        }

        outlineProperties ??= new MaterialPropertyBlock();
    }

    private IEnumerable<SpriteRenderer> GetSourceRenderers()
    {
        if (targetRenderers != null && targetRenderers.Length > 0)
        {
            return targetRenderers;
        }

        return GetComponentsInChildren<SpriteRenderer>(true);
    }

    private bool IsRegisteredRenderer(SpriteRenderer renderer)
    {
        foreach (OutlineRendererPair pair in outlineRendererPairs)
        {
            if (pair.source == renderer || pair.outline == renderer)
            {
                return true;
            }
        }

        return false;
    }

    private void UpdateOutlineRenderers()
    {
        if (outlineProperties == null)
        {
            return;
        }

        for (int i = outlineRendererPairs.Count - 1; i >= 0; i--)
        {
            OutlineRendererPair pair = outlineRendererPairs[i];
            if (pair.source == null || pair.outline == null)
            {
                outlineRendererPairs.RemoveAt(i);
                continue;
            }

            CopyRendererState(pair.source, pair.outline);
            ApplyOutlineProperties(pair.source, pair.outline);
            pair.outline.enabled = isHighlighted && pair.source.enabled;
        }
    }

    private void ApplyOutlineProperties(SpriteRenderer source, SpriteRenderer outline)
    {
        Texture2D spriteTexture = source.sprite != null ? source.sprite.texture : null;
        Color color = activeOutlineColor;
        color.a *= source.color.a;

        outlineProperties.Clear();
        outlineProperties.SetColor(OutlineColorId, color);
        outlineProperties.SetFloat(OutlineWidthId, activeOutlineWidth);
        outlineProperties.SetFloat(AlphaThresholdId, activeAlphaThreshold);
        outlineProperties.SetVector(
            OutlinePixelStepId,
            spriteTexture == null
                ? Vector4.zero
                : new Vector4(
                    1f / spriteTexture.width,
                    1f / spriteTexture.height,
                    spriteTexture.width,
                    spriteTexture.height));

        outline.SetPropertyBlock(outlineProperties);
    }

    private void SetOutlineRenderersEnabled(bool enabled)
    {
        foreach (OutlineRendererPair pair in outlineRendererPairs)
        {
            if (pair.outline != null)
            {
                pair.outline.enabled = enabled;
            }
        }
    }

    private void CopyRendererState(SpriteRenderer source, SpriteRenderer outline)
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
        outline.sortingOrder = useCustomOrderInLayer
            ? orderInLayer
            : source.sortingOrder;
        outline.renderingLayerMask = source.renderingLayerMask;
    }

    private void RemoveInvalidPairs()
    {
        outlineRendererPairs.RemoveAll(pair => pair.source == null || pair.outline == null);
    }
}
