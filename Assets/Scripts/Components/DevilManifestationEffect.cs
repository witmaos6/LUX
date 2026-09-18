using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class DevilManifestationEffect : MonoBehaviour
{
    [SerializeField] private Sprite[] sprites;
    [SerializeField] private int minCount = 1;
    [SerializeField] private int maxCount = 3;
    [SerializeField] private float boundary = 1.0f;
    [SerializeField] private float showDuration = 0.5f;
    [SerializeField] private float hideDuration = 0.5f;

    private SpriteRenderer[] pool;
    private Coroutine loopCoroutine;

    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float fadeOutDuration = 0.5f;

    private void Awake()
    {
        pool = new SpriteRenderer[maxCount];
        for(int i = 0; i < maxCount; i++)
        {
            GameObject go = new GameObject($"ManifestSlot_{i}");
            go.transform.SetParent(transform, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 6;
            go.SetActive(false);
            pool[i] = sr;
        }
    }

    private void OnEnable()
    {
        loopCoroutine = StartCoroutine(ManifestLoop());
    }

    private void OnDisable()
    {
        if(loopCoroutine != null)
            StopCoroutine(loopCoroutine);
    }

    IEnumerator ManifestLoop()
    {
        while(true)
        {
            yield return new WaitForSeconds(hideDuration);
            ShowRandom();
            yield return new WaitForSeconds(showDuration);
            HideAll();
        }
    }

    void ShowRandom()
    {
        int count = Mathf.Clamp(Random.Range(minCount, maxCount + 1), 0, sprites.Length);

        int[] indices = new int[sprites.Length];
        for (int i = 0; i < indices.Length; i++)
            indices[i] = i;

        for(int i = 0; i < count; i++)
        {
            int r = Random.Range(i, indices.Length);
            (indices[i], indices[r]) = (indices[r], indices[i]);
        }

        for(int i = 0; i < count; i++)
        {
            var sr = pool[i];
            sr.sprite = sprites[indices[i]];
            sr.transform.localPosition = (Vector3)(Random.insideUnitCircle * boundary);
            sr.gameObject.SetActive(true);
            StartCoroutine(FadeSpriteRenderer(sr, 0f, 1f, fadeInDuration));
        }
    }

    private IEnumerator FadeSpriteRenderer(SpriteRenderer sr, float from, float to, float duration, bool deactivateOnEnd = false)
    {
        Color color = sr.color;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            color.a = Mathf.Lerp(from, to, t);
            sr.color = color;
            yield return null;
        }

        color.a = to;
        sr.color = color;

        if (deactivateOnEnd)
            sr.gameObject.SetActive(false);
    }

    void HideAll()
    {
        foreach(var sr in pool)
        {
            if(sr.gameObject.activeSelf)
            {
                StartCoroutine(FadeSpriteRenderer(sr, 1f, 0f, fadeOutDuration, deactivateOnEnd: true));
            }
        }    
    }
}
