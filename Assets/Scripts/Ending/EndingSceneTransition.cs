using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class EndingSceneTransition : MonoBehaviour
{
    [SerializeField, Min(0f)] private float delaySeconds = 1.5f;
    [SerializeField] private string endingSceneName = "EndingCredits";

    private Coroutine transitionCoroutine;

    public void Begin()
    {
        if (transitionCoroutine != null)
            return;

        transitionCoroutine = StartCoroutine(TransitionAfterDelay());
    }

    private IEnumerator TransitionAfterDelay()
    {
        yield return new WaitForSecondsRealtime(delaySeconds);

        if (string.IsNullOrWhiteSpace(endingSceneName))
        {
            Debug.LogError("The ending scene name is empty.", this);
            transitionCoroutine = null;
            yield break;
        }

        if (!Application.CanStreamedLevelBeLoaded(endingSceneName))
        {
            Debug.LogError(
                $"Ending scene '{endingSceneName}' is not available. Add it to the Build Profiles scene list.",
                this);
            transitionCoroutine = null;
            yield break;
        }

        SaveManager.BeginSceneTransition();
        SceneManager.LoadScene(endingSceneName);
    }
}
