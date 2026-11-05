using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using static Devil;

public sealed class DevilManifestSequenceEffect : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] private Sprite[] manifestSprites;
    [SerializeField] private Sprite maskSprite;
    [SerializeField] private Material manifestMaterial;

    [Header("Spawn")]
    [SerializeField, Min(0f)] private float spawnRadius = 1f;
    [SerializeField, Min(0f)] private float minSpawnInterval = 0.2f;
    [SerializeField, Min(0f)] private float maxSpawnInterval = 0.6f;

    [Header("Timing")]
    [SerializeField, Min(0f)] private float revealDuration = 0.4f;
    [SerializeField, Min(0f)] private float visibleDuration = 0.2f;
    [SerializeField, Min(0f)] private float hideDuration = 0.4f;

    [Header("Direction")]
    [SerializeField] private float rotationOffsetDegrees;

    [Header("Sorting")]
    [SerializeField] private string sortingLayerName = "Default";
    [SerializeField] private int sortingOrder = 15;

    private readonly List<GameObject> activeEffectRoots = new List<GameObject>();

    private Coroutine spawnCoroutine;
    private int activeEffectCount;

    public bool IsPlaying => spawnCoroutine != null || activeEffectCount > 0;

    private void OnEnable()
    {
        Devil devil = transform.parent.gameObject.GetComponent<Devil>();
        if (devil != null)
        {
            devil.changeStateDelegate += ChangeState;
        }
    }

    void ChangeState(DevilState prevState, DevilState newState)
    {
        if(prevState == DevilState.Patrol && newState != DevilState.Patrol)
        {
            StartManifest();
        }
        else if (prevState != DevilState.Patrol && newState == DevilState.Patrol)
        {
            StopManifest();
        }
    }

    public void StartManifest()
    {
        if (IsPlaying)
            return;

        if (!HasValidConfiguration())
            return;

        spawnCoroutine = StartCoroutine(SpawnRoutine());
    }

    public void StopManifest()
    {
        if (spawnCoroutine == null)
            return;

        StopCoroutine(spawnCoroutine);
        spawnCoroutine = null;
    }

    private void OnDisable()
    {
        Devil devil = transform.parent.gameObject.GetComponent<Devil>();
        if (devil != null)
        {
            devil.changeStateDelegate -= ChangeState;
        }

        StopManifest();
        StopAllCoroutines();

        foreach (GameObject effectRoot in activeEffectRoots)
        {
            if (effectRoot != null)
                Destroy(effectRoot);
        }

        activeEffectRoots.Clear();
        activeEffectCount = 0;
    }

    private IEnumerator SpawnRoutine()
    {
        SpawnEffect();

        while (true)
        {
            float delay = Random.Range(minSpawnInterval, maxSpawnInterval);
            yield return new WaitForSeconds(delay);

            SpawnEffect();
        }
    }

    private void SpawnEffect()
    {
        Sprite sprite = GetRandomManifestSprite();
        if (sprite == null)
            return;

        Vector2 localPosition = Random.insideUnitCircle * spawnRadius;
        Vector2 direction = localPosition.sqrMagnitude > Mathf.Epsilon
            ? localPosition.normalized
            : Vector2.up;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        GameObject effectRoot = new GameObject("DevilManifestSprite");
        effectRoot.transform.SetParent(transform, false);
        effectRoot.transform.localPosition = localPosition;
        effectRoot.transform.localRotation = Quaternion.Euler(
            0f,
            0f,
            angle + rotationOffsetDegrees);

        SortingGroup sortingGroup = effectRoot.AddComponent<SortingGroup>();
        sortingGroup.sortingLayerName = sortingLayerName;
        sortingGroup.sortingOrder = sortingOrder;

        SpriteRenderer spriteRenderer = CreateSpriteRenderer(effectRoot.transform, sprite);
        Transform maskTransform = CreateMask(effectRoot.transform);

        activeEffectRoots.Add(effectRoot);
        activeEffectCount++;

        StartCoroutine(PlayEffectRoutine(
            effectRoot,
            maskTransform,
            sprite.bounds,
            maskSprite.bounds));
    }

    private SpriteRenderer CreateSpriteRenderer(Transform parent, Sprite sprite)
    {
        GameObject spriteObject = new GameObject("Sprite");
        spriteObject.transform.SetParent(parent, false);

        SpriteRenderer spriteRenderer = spriteObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        spriteRenderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;

        if (manifestMaterial != null)
            spriteRenderer.sharedMaterial = manifestMaterial;

        return spriteRenderer;
    }

    private Transform CreateMask(Transform parent)
    {
        GameObject maskObject = new GameObject("Mask");
        maskObject.transform.SetParent(parent, false);

        SpriteMask spriteMask = maskObject.AddComponent<SpriteMask>();
        spriteMask.sprite = maskSprite;

        return maskObject.transform;
    }

    private IEnumerator PlayEffectRoutine(
        GameObject effectRoot,
        Transform maskTransform,
        Bounds spriteBounds,
        Bounds maskBounds)
    {
        float fullWidth = spriteBounds.size.x;

        SetMaskWidth(maskTransform, spriteBounds, maskBounds, 0f, false);
        yield return AnimateMaskWidth(
            maskTransform,
            spriteBounds,
            maskBounds,
            0f,
            fullWidth,
            revealDuration,
            false);

        if (visibleDuration > 0f)
            yield return new WaitForSeconds(visibleDuration);

        yield return AnimateMaskWidth(
            maskTransform,
            spriteBounds,
            maskBounds,
            fullWidth,
            0f,
            hideDuration,
            true);

        activeEffectRoots.Remove(effectRoot);
        activeEffectCount--;
        Destroy(effectRoot);
    }

    private IEnumerator AnimateMaskWidth(
        Transform maskTransform,
        Bounds spriteBounds,
        Bounds maskBounds,
        float startWidth,
        float endWidth,
        float duration,
        bool anchorRight)
    {
        if (duration <= 0f)
        {
            SetMaskWidth(maskTransform, spriteBounds, maskBounds, endWidth, anchorRight);
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(elapsed / duration);
            float width = Mathf.Lerp(startWidth, endWidth, progress);

            SetMaskWidth(maskTransform, spriteBounds, maskBounds, width, anchorRight);
            yield return null;
        }

        SetMaskWidth(maskTransform, spriteBounds, maskBounds, endWidth, anchorRight);
    }

    private static void SetMaskWidth(
        Transform maskTransform,
        Bounds spriteBounds,
        Bounds maskBounds,
        float width,
        bool anchorRight)
    {
        float maskWidth = Mathf.Max(width, 0.0001f);
        float centerX = anchorRight
            ? spriteBounds.max.x - maskWidth * 0.5f
            : spriteBounds.min.x + maskWidth * 0.5f;

        maskTransform.localPosition = new Vector3(
            centerX,
            spriteBounds.center.y,
            0f);

        maskTransform.localScale = new Vector3(
            maskWidth / maskBounds.size.x,
            spriteBounds.size.y / maskBounds.size.y,
            1f);
    }

    private Sprite GetRandomManifestSprite()
    {
        for (int i = 0; i < manifestSprites.Length; i++)
        {
            Sprite sprite = manifestSprites[Random.Range(0, manifestSprites.Length)];
            if (sprite != null)
                return sprite;
        }

        return null;
    }

    private bool HasValidConfiguration()
    {
        if (manifestSprites == null || manifestSprites.Length == 0)
        {
            Debug.LogWarning("DevilManifestSequenceEffect requires at least one manifest sprite.", this);
            return false;
        }

        if (maskSprite == null)
        {
            Debug.LogWarning("DevilManifestSequenceEffect requires a rectangular mask sprite.", this);
            return false;
        }

        return true;
    }

    private void OnValidate()
    {
        if (maxSpawnInterval < minSpawnInterval)
            maxSpawnInterval = minSpawnInterval;
    }
}
