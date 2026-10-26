using System.Collections;
using UnityEngine;

public class FadeInOut : MonoBehaviour
{
    [SerializeField] private float showDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.5f;

    private SpriteRenderer sr;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }
    void Start()
    {
        StartCoroutine(ShowTime());
    }

    IEnumerator ShowTime()
    {
        yield return new WaitForSeconds(showDuration);

        StartCoroutine(FadeSpriteRenderer(sr, 1f, 0f, fadeOutDuration));
    }

    private IEnumerator FadeSpriteRenderer(SpriteRenderer sr, float from, float to, float duration)
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
    }
}
