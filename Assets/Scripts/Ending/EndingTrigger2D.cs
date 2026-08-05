using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public sealed class EndingTrigger2D : MonoBehaviour
{
    [Header("Ending")]
    [SerializeField] private GameEvent endingEvent;
    [SerializeField] private PlayerScriptUI scriptUI;

    [Header("Fade")]
    [SerializeField] private CameraFader cameraFader;
    [SerializeField, Min(0f)] private float fadeInDuration = 1f;

    private bool hasTriggered;

    private void Awake()
    {
        if (cameraFader == null && Camera.main != null)
            cameraFader = Camera.main.GetComponent<CameraFader>();

        if (scriptUI == null)
            scriptUI = FindFirstObjectByType<PlayerScriptUI>();
    }

    private void Reset()
    {
        BoxCollider2D trigger = GetComponent<BoxCollider2D>();
        trigger.isTrigger = true;

        if (Camera.main != null)
            cameraFader = Camera.main.GetComponent<CameraFader>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered)
            return;

        PlayerController playerController = other.GetComponentInParent<PlayerController>();
        if (playerController == null)
            return;

        if (endingEvent == null)
        {
            Debug.LogError("The ending trigger has no ending event.", this);
            return;
        }

        hasTriggered = true;
        StartCoroutine(BeginEndingWhenReady(playerController));
    }

    private IEnumerator BeginEndingWhenReady(PlayerController playerController)
    {
        while (scriptUI != null && scriptUI.IsShowing)
            yield return null;

        playerController.EnterScriptMode();

        if (cameraFader == null)
        {
            Debug.LogWarning("The ending trigger has no CameraFader. Starting the ending immediately.", this);
            StartEnding();
            yield break;
        }

        cameraFader.cameraFadeInComplete += StartEnding;
        cameraFader.StartFade(fadeInDuration);
    }

    private void OnDisable()
    {
        if (cameraFader != null)
            cameraFader.cameraFadeInComplete -= StartEnding;
    }

    private void StartEnding()
    {
        if (cameraFader != null)
            cameraFader.cameraFadeInComplete -= StartEnding;

        GameEventManager.Raise(endingEvent);
    }
}
