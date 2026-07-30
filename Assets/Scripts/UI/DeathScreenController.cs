using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DeathScreenController : MonoBehaviour
{
    public static DeathScreenController Instance;

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Button restartButton;
    [SerializeField] private float fadeDuration = 1.5f;

    private Coroutine fadeRoutine;

    private void Awake()
    {
        Instance = this;

        if (restartButton != null)
            restartButton.onClick.AddListener(Restart);

        if (canvasGroup != null)
            canvasGroup.alpha = 0f;

        if (restartButton != null)
            restartButton.gameObject.SetActive(false);

        gameObject.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);

        if (restartButton != null)
            restartButton.gameObject.SetActive(false);

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            if (canvasGroup != null)
                canvasGroup.alpha = Mathf.Clamp01(t / fadeDuration);
            yield return null;
        }

        if (canvasGroup != null)
            canvasGroup.alpha = 1f;

        if (restartButton != null)
            restartButton.gameObject.SetActive(true);

        fadeRoutine = null;
    }

    public void Restart()
    {
        SaveManager.BeginSceneTransition();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
