using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CameraFader : MonoBehaviour
{
    public delegate void CameraFadeInComplete();
    public CameraFadeInComplete cameraFadeInComplete;

    public delegate void CameraFadeOutComplete();
    public CameraFadeOutComplete cameraFadeOutComplete;

    public Image overlay;
    public float defaultFadeInDuration = 1f;
    public float defaultFadeOutDuration = 1f;
    public float holdDuration = 0.5f;
    [SerializeField, Range(0f, 1f)] private float targetAlpha = 1f;

    private Coroutine currentFade;

    private void Awake()
    {
        if (overlay == null)
            CreateOverlay();

        SetAlpha(0f);
    }

    public void StartFade(float fadeInDuration = -1f, float fadeOutDuration = -1f)
    {
        if (fadeInDuration < 0f) fadeInDuration = defaultFadeInDuration;
        if (fadeOutDuration < 0f) fadeOutDuration = defaultFadeOutDuration;

        if (currentFade != null)
            StopCoroutine(currentFade);

        currentFade = StartCoroutine(FadeSequence(fadeInDuration, fadeOutDuration));
    }

    IEnumerator FadeSequence(float fadeInDuration, float fadeOutDuration)
    {
        yield return FadeTo(targetAlpha, fadeInDuration);

        cameraFadeInComplete?.Invoke();
        cameraFadeInComplete = null;

        yield return new WaitForSeconds(holdDuration);
        yield return FadeTo(0f, fadeOutDuration);

        cameraFadeOutComplete?.Invoke();
        cameraFadeOutComplete = null;
        currentFade = null;
    }

    IEnumerator FadeTo(float alpha, float duration)
    {
        float start = overlay.color.a;
        if (Mathf.Approximately(start, alpha))
            yield break;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(start, alpha, duration <= 0f ? 1f : t / duration);
            SetAlpha(a);
            yield return null;
        }
        SetAlpha(alpha);
    }

    void SetAlpha(float alpha)
    {
        Color c = overlay.color;
        c.a = Mathf.Clamp01(alpha);
        overlay.color = c;
    }

    private void CreateOverlay()
    {
        var go = new GameObject("CameraFadeOverlay");
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32767; // 최상위로 설정

        go.AddComponent<CanvasScaler>();
        go.AddComponent<GraphicRaycaster>();

        var imgGO = new GameObject("OverlayImage");
        imgGO.transform.SetParent(go.transform, false);

        var img = imgGO.AddComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0f);
        img.raycastTarget = false;

        var rt = imgGO.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        overlay = img;
        DontDestroyOnLoad(go);
    }
}
